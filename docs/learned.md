# What I learned

## B01 · unit tests + CI (30 Oct 2026)
- `ci.yml` is the GitHub Actions workflow. On a push or PR to `main`, it checks out the repo, installs .NET, restores the NuGet packages, builds, and runs the tests. If any step fails, the build goes red.
- Unit tests only test behaviour: whether the methods do what they're supposed to. The dependencies are faked.
- Tests can be written before the code, so the code is developed to pass them.
- Fakes are assumptions, not the real thing. They can be wrong, because an assumption may depend on other behaviour that gets missed.
- The sender came from the request body, so a logged-in user could impersonate someone else. Now it comes from the JWT.
- Even then, anyone with a thread ID could post into it. There was no membership check. Now non-members get 403.

## B02 · integration tests with Testcontainers (3 Nov 2026)
- EF Core InMemory means there's no real Postgres server, just EF Core holding objects in memory. SQL is never generated, so invalid queries, constraints (like a unique email) and Postgres-specific behaviour are missed. Testcontainers runs a real Postgres in Docker.
- The factory passes the connection string (and the JWT settings) with `UseSetting`. `Program.cs` reads them while the app is still being built, which is too early for `ConfigureAppConfiguration`. `UseSetting` values are already there at that point and override `appsettings.json`.
- One Postgres container is shared by every test in the collection during a run (`ApiFixture`). A new one starts each run. Data persists between tests unless it's cleared, so each test makes unique users (a GUID in the email).
- The fake `IsMember()` didn't behave like the real one. The real one always returned true (`ToListAsync()` never returns null, only an empty list), so the B01 403 check never worked against a real DB. The integration test caught it → fixed with `AnyAsync`.
- (Claude explained, 3 Nov) The MSB3277 warning: `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11 pulls EF Core Relational 8.0.11, while `Microsoft.EntityFrameworkCore` 8.0.16 wants Relational 8.0.16. The build picks 8.0.11, which can fail at runtime with a missing method. Fix: keep the EF packages on one version (e.g. bump the Npgsql provider to the newest 8.0.x).

## B03 · notifications over Azure Service Bus (4 Nov 2026)
- A queue handles three things: load (a lot of requests at once go to the queue), service failures (messages stay saved in the queue, so failures can be recovered), and taking work off the API (a worker service handles the notifications).
- The lock stops the same message being taken by another consumer: it's hidden from all consumers for the lock duration (30 s here). If the consumer doesn't complete it in that time, it comes back.
- `DeliveryCount` is how many times a message has been delivered. At `MaxDeliveryCount` (3 in the emulator now) it moves to the dead-letter queue, where it can be inspected and resubmitted.
- If the worker saves the notifications and dies before completing the message, it's redelivered after 30 s with `DeliveryCount` + 1. The guard (`ExistsAsync`, `MessageSentHandler.cs:36`) sees the processed row and skips it, then the worker completes it. The live SignalR push is lost, but the row is there: rows exactly once, push at most once.
- (Claude explained, 3 Nov) Known gap, no outbox: `SendMessage` commits the message before publishing. If the publish fails, the message is saved but nobody is notified. A transactional outbox would fix that.

## B04 · ChatApp on Azure Container Apps (5 Nov 2026)
- Container Apps lets us run our image and services. It also provides the necessary components: HTTPS, a URL, secret management, and scaling to zero when idle.
- Secrets live in the container app and reach the process as env vars through `secretref:`. The app can read them, and so can anyone with enough Azure rights on the subscription, like me as Owner.
- The script had never touched real Azure, so the first runs found what the emulator couldn't: my subscription allows only 5 regions (`koreacentral` works, `centralindia` is blocked), two `az` flags had changed, and the image was private, so Azure couldn't have pulled it. Org settings had to allow public packages first.
- The network can look fine and still break `az`: IPv6 to `management.azure.com` kept resetting, so the script died at random until I turned IPv6 off for the run.
- Cost so far: $0 on my student credit, but Azure's cost data lags, so this isn't proven yet. List price is about $15–20 a month, almost all Postgres. The credit expires around 26 Dec.
