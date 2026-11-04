using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Networks;
using Testcontainers.MsSql;
using Testcontainers.ServiceBus;

namespace ChatApp.IntegrationTests.Infrastructure;

/// <summary>
/// Same as <see cref="ApiFixture"/>, plus the Azure Service Bus emulator (and the SQL Server
/// it needs), so the API runs with ServiceBus:Enabled = true and NotificationWorker consumes
/// the real "message-sent" queue.
///
/// Slow to start (SQL Server + emulator, ~1 min), so it can be skipped with
/// SKIP_SERVICEBUS_TESTS=true (see <see cref="ServiceBusFactAttribute"/>).
/// </summary>
public sealed class ServiceBusApiFixture : ApiFixture
{
    // Same images as docker-compose.yml. SQL Server is x64-only: on Apple silicon Docker
    // Desktop runs it under Rosetta. The emulator image is multi-arch (native arm64).
    private const string EmulatorImage = "mcr.microsoft.com/azure-messaging/servicebus-emulator:latest";
    private const string SqlImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string SqlAlias = "servicebus-sql";
    private const string SqlPassword = "Integration-Tests-0nly";

    private INetwork? _network;
    private MsSqlContainer? _sql;
    private ServiceBusContainer? _serviceBus;

    protected override async Task<string?> StartServiceBusAsync()
    {
        if (ServiceBusFactAttribute.IsSkipped)
        {
            return null; // tests are skipped; don't start anything
        }

        _network = new NetworkBuilder().Build();
        _sql = new MsSqlBuilder(SqlImage)
            .WithPassword(SqlPassword)
            .WithNetwork(_network)
            .WithNetworkAliases(SqlAlias)
            .Build();
        _serviceBus = new ServiceBusBuilder(EmulatorImage)
            // Accepts the emulator's EULA (dev/test use only); the compose file asks for ACCEPT_EULA=Y instead.
            .WithAcceptLicenseAgreement(true)
            .WithMsSqlContainer(_network, _sql, SqlAlias, SqlPassword)
            // The same Config.json docker-compose mounts: declares the "message-sent" queue.
            .WithConfig(Path.Combine(AppContext.BaseDirectory, "servicebus-emulator", "Config.json"))
            .Build();

        // Starting the emulator starts the SQL container it depends on first.
        await _serviceBus.StartAsync();
        return _serviceBus.GetConnectionString();
    }

    protected override async Task StopServiceBusAsync()
    {
        if (_serviceBus is not null) await _serviceBus.DisposeAsync();
        if (_sql is not null) await _sql.DisposeAsync();
        if (_network is not null) await _network.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ServiceBusCollection : ICollectionFixture<ServiceBusApiFixture>
{
    public const string Name = "API with PostgreSQL and the Service Bus emulator";
}

/// <summary>
/// A [Fact] that is skipped when SKIP_SERVICEBUS_TESTS=true, e.g. on a machine that can't run
/// the SQL Server image. (xUnit 2 has no runtime skip, so the attribute decides up front.)
/// </summary>
public sealed class ServiceBusFactAttribute : FactAttribute
{
    public static bool IsSkipped =>
        string.Equals(Environment.GetEnvironmentVariable("SKIP_SERVICEBUS_TESTS"), "true", StringComparison.OrdinalIgnoreCase);

    public ServiceBusFactAttribute()
    {
        if (IsSkipped)
        {
            Skip = "SKIP_SERVICEBUS_TESTS=true";
        }
    }
}
