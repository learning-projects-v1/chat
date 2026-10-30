using System.Net;
using ChatApp.IntegrationTests.Infrastructure;

namespace ChatApp.IntegrationTests.Health;

[Collection(ApiCollection.Name)]
public class HealthEndpointTests
{
    private readonly ApiFixture _fixture;

    public HealthEndpointTests(ApiFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Health_WithoutToken_Returns200Healthy()
    {
        // /health is what you curl after an Azure deploy, so it must not need a JWT.
        var client = _fixture.Factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
