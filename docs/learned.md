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
