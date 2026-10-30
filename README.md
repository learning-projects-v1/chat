![CI](https://github.com/learning-projects-v1/chat/actions/workflows/ci.yml/badge.svg)

# Chat App

A one-to-one chat app: an ASP.NET Core 8 Web API (clean architecture, EF Core + PostgreSQL, JWT, SignalR) and an Angular 19 client.
Users register and log in, send and accept friend requests, and chat in real time with replies, reactions and seen status.
When a message is sent, the API publishes a `MessageSent` event to an Azure Service Bus queue; a background worker turns it into notification rows and a SignalR push.

<!-- record: docs/demo.gif, ~15 s. Two browser windows side by side (e.g. Chrome + a private window), logged in as u1@gmail.com and u2@gmail.com, same thread open.
     u1 sends a message, it appears live in u2's thread and chat list; u2 replies or reacts. Run the API with the servicebus profile on,
     and show the "Processed MessageSent ..." log line from NotificationWorker in a terminal next to the browsers. Then uncomment the line below. -->
<!-- ![Demo: two browsers chatting, the second one gets the message through Service Bus](docs/demo.gif) -->

## Contents
- [Architecture](#architecture)
- [Why Service Bus](#why-service-bus)
- [Run locally](#run-locally)
- [Tests](#tests)
- [CI](#ci)
- [Deploy](#deploy)
- [Known limitations](#known-limitations)
- [Screenshots](#screenshots)

## Architecture

```mermaid
flowchart LR
    subgraph clients["Browsers"]
        A["Angular client (user A)"]
        B["Angular client (user B)"]
    end

    subgraph api["ChatApp.API (one process)"]
        C["Controllers: REST + JWT"]
        H["SignalR hub /hubs/notifications"]
        W["NotificationWorker (BackgroundService)"]
    end

    PG[("PostgreSQL: Messages, Notifications, ProcessedMessages")]
    Q[["Service Bus queue: message-sent"]]
    DLQ[["Dead-letter queue"]]

    A -->|"(1) POST /api/threads/id/messages"| C
    C -->|"(2) save message"| PG
    C -->|"(3) publish MessageSent"| Q
    Q -->|"(4) receive (peek-lock)"| W
    W -->|"(5) Notifications + ProcessedMessages rows"| PG
    W -->|"(6) push"| H
    H -->|"MessageAllNotification"| A
    H -->|"MessageAllNotification"| B
    Q -.->|"after 5 failed deliveries"| DLQ
```

The same flow runs in two places:

| | Local | Azure (scripted, see [Deploy](#deploy)) |
|---|---|---|
| Service Bus | Service Bus emulator + its SQL Server in Docker (`servicebus` compose profile), queue from `servicebus-emulator/Config.json` | Service Bus namespace (Basic tier), queue created by `deploy/azure/provision.sh` |
| PostgreSQL | `postgres:15` container from `docker-compose.yml` | Azure Database for PostgreSQL Flexible Server (B1ms) |
| API + worker | `dotnet run` | Container Apps, image from `ghcr.io/learning-projects-v1/chat-api` |

`ServiceBus:Enabled` switches the queue on or off. It is `false` by default (`appsettings.json`): then `InProcessEventPublisher` runs the same `MessageSentHandler` inside the HTTP request, so the app works without Service Bus. With `true`, `ServiceBusEventPublisher` sends to the `message-sent` queue and `NotificationWorker` consumes it.

### Project layers (`backend/`)

| Project | What is in it |
|---|---|
| `ChatApp.Domain` | Entities (`User`, `Friendship`, `ChatThread`, `ChatThreadMember`, `Message`, `Reaction`, `MessageSeenStatus`, `Notification`, `ProcessedMessage`) and constants. No dependencies. |
| `ChatApp.Application` | DTOs, repository and service interfaces (`IEventPublisher`, `IRealTimeNotifier`, `IUnitOfWork`, ...), `AuthService`, the `MessageSent` event and `MessageSentHandler`. |
| `ChatApp.Infrastructure` | EF Core `ChatAppDbContext` + migrations, repositories, `UnitOfWork` (turns a Postgres unique violation into `DuplicateKeyException`), `JwtTokenGenerator`, the thread-member authorization handler, `ServiceBusEventPublisher` and `InProcessEventPublisher`. |
| `ChatApp.Api` | Controllers, the SignalR `NotificationHub` and `RealTimeNotifier`, `NotificationWorker`, DI setup (`Middleware/*Registration*.cs`), `Program.cs`, `/health`. |
| `tests/ChatApp.UnitTests`, `tests/ChatApp.IntegrationTests` | See [Tests](#tests). |

The Angular client is in `frontend/chatApp`.

## Why Service Bus

<!-- rewrite in your words -->
**Queue instead of inline.** Before, `SendMessage` pushed the new message to the thread members over SignalR inside the request. Now the request saves the message, publishes one `MessageSent` event and returns. The notification work (notification rows + the SignalR push) happens in `NotificationWorker`, so a failure there (database hiccup, a bug in the handler) is retried by Service Bus instead of failing the user's send.

<!-- rewrite in your words -->
**At-least-once delivery.** The worker receives in peek-lock mode with `AutoCompleteMessages = false` and completes a message only after the handler has succeeded. If the handler throws, or the process dies before the complete call, the lock expires and Service Bus delivers the message again. So the same message can arrive more than once, and the handler has to cope with that.

<!-- rewrite in your words -->
**The duplicate guard.** The chat message id is used as the Service Bus `MessageId`. `MessageSentHandler` checks the `ProcessedMessages` table for that id and skips the message if it is there. Otherwise it writes the notification rows and the `ProcessedMessages` row in one `SaveChanges` (one transaction). If two deliveries race past the check, the primary key on `ProcessedMessages.MessageId` lets only one commit; the other gets `DuplicateKeyException` and is treated as a duplicate. Duplicates are completed, not retried. The SignalR push runs after the commit, so the rows are written once and the live push happens at most once.

<!-- rewrite in your words -->
**When the worker is down.** The worker runs in the API process. While it is not consuming, messages wait in the queue; the queue's time-to-live is 1 hour and expired messages are dropped (dead-lettering on expiry is off). Users can still send messages, they just don't get the live push until the worker is back.

<!-- rewrite in your words -->
**Dead-letter queue.** After 5 failed deliveries (`MaxDeliveryCount`, same in the emulator config and the Azure script) Service Bus moves the message to the queue's dead-letter queue. Nothing in the app reads it; you look at it in Service Bus Explorer in the Azure portal.

<!-- rewrite in your words -->
**Known gap: no outbox.** The message is committed to Postgres before the event is published. If the publish fails, the message is saved but nobody is notified, and the request returns an error even though the message exists. The transactional outbox pattern (write the event to an outbox table in the same transaction, publish it from there) would close this gap; it is not built.

## Run locally

Prerequisites: Docker, the .NET 8 SDK, Node.js 20 (for the Angular client).

**1. PostgreSQL**
```bash
docker compose up -d          # starts only postgres (chat-postgres, port 5432, db ChatAppDb, user postgres / password)
```
The container name is fixed (`chat-postgres`), so remove an old container with that name first if compose reports a conflict.

**2. API settings.** `appsettings.Development.json` is git-ignored, so create it in `backend/ChatApp.Api/` (or pass the same keys as environment variables, e.g. `JwtSettings__Secret`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ChatAppDb;Username=postgres;Password=password"
  },
  "JwtSettings": {
    "Secret": "<any string of 32+ characters>",
    "Issuer": "ChatApp",
    "Audience": "ChatApp.Client",
    "AccessTokenExpirationMinutes": 60
  },
  "RunMigrations": true
}
```
`RunMigrations: true` applies the EF Core migrations at startup and seeds demo data (only when the `Users` table is empty). `ClearDatabase: true` would also drop the database on every start.

**3. API** (without Service Bus: `ServiceBus:Enabled` is `false` by default)
```bash
dotnet run --project backend/ChatApp.Api --launch-profile http
```
- API: http://localhost:5000 · Swagger: http://localhost:5000/swagger · health: http://localhost:5000/health
- SignalR hub: `/hubs/notifications`

**4. Angular client**
```bash
cd frontend/chatApp
npm install
npx ng serve                  # http://localhost:4200, talks to http://localhost:5000 (src/app/environments/environment.ts)
```

**Demo users:** `u1@gmail.com` ... `u15@gmail.com` from the seed data. They log in with any password (see [Known limitations](#known-limitations)). `u1` is friends with everyone, so two windows as `u1` and `u2` share a thread. You can also register a new user, but new users have no threads yet.

### With the Service Bus emulator

```bash
ACCEPT_EULA=Y docker compose --profile servicebus up -d    # postgres + servicebus-sql + servicebus
```
`ACCEPT_EULA=Y` accepts the emulator's and SQL Server Developer's licence terms (dev/test use only). The emulator reads the `message-sent` queue from `servicebus-emulator/Config.json`. Ports: 5672 (AMQP), 5300 (http://localhost:5300/health). The SQL Server image is x64 only; on Apple silicon Docker Desktop runs it under Rosetta.

Then start the API with the queue switched on:
```bash
ServiceBus__Enabled=true \
ConnectionStrings__ServiceBus="Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;" \
dotnet run --project backend/ChatApp.Api --launch-profile http
```
Each sent message should log `Processed MessageSent <id> from the message-sent queue (attempt 1).` from `NotificationWorker`.

Stop everything with `docker compose --profile servicebus down` (add `-v` to delete the Postgres volume).

## Tests

```bash
dotnet test backend/tests/ChatApp.UnitTests/ChatApp.UnitTests.csproj
dotnet test backend/tests/ChatApp.IntegrationTests/ChatApp.IntegrationTests.csproj   # needs Docker running
```

- **Unit tests** (xUnit + NSubstitute, no database): `AuthService` register/login, `FriendsController` send/accept rules, `MessagesController.SendMessage` (thread not found, publishes `MessageSent` for all thread members, sender taken from the token and not the request body), JWT and claims helpers, and `MessageSentHandler` (notifies everyone except the sender, same message id twice creates one notification, losing the race on the inbox key skips the push).
- **Integration tests** (`WebApplicationFactory<Program>` + Testcontainers): the real `Program.cs`, DI, JWT auth and EF Core against a PostgreSQL 16 container with the real migrations. They cover register → login → send → fetch, a request without a token getting 401, `/health`, and the in-process notification path. A second fixture adds the Service Bus emulator and its SQL Server, and checks that a notification arrives through the queue and that the same message delivered twice creates one notification. Those two are slower (about a minute to start); set `SKIP_SERVICEBUS_TESTS=true` to skip them.

<!-- rewrite in your words -->
**Why Testcontainers and not the EF Core InMemory provider.** InMemory is not a relational database: it doesn't run migrations, doesn't enforce unique indexes, has no real transactions and translates queries differently from Npgsql. The duplicate guard depends on a Postgres primary-key violation inside a transaction, and the schema comes from migrations, so those need a real Postgres to be tested at all. The cost is speed and needing Docker.

## CI

Two GitHub Actions workflows in `.github/workflows/`:

- **`ci.yml`** (the badge above): on every push and pull request to `main`. Restores and builds the solution in Release on .NET 8, runs the unit tests, then the integration tests. The `ubuntu-latest` runner has Docker, so Testcontainers starts Postgres and the Service Bus emulator there.
- **`image.yml`**: on pushes to `main` that touch `backend/**` (or by hand). Builds the API Docker image (`backend/Dockerfile`, linux/amd64) and pushes it to `ghcr.io/learning-projects-v1/chat-api`, tagged `sha-<short>` and, on `main`, `latest`. It does not deploy.

## Deploy

The Azure deploy is scripted, not automatic: `deploy/azure/provision.sh` uses the `az` CLI to create a resource group with a Container Apps environment, the API container app (image from GHCR, max 1 replica, `ServiceBus__Enabled=true`), a Service Bus Basic namespace with the `message-sent` queue, a PostgreSQL Flexible Server and a Log Analytics workspace, then prints the public URL. `./deploy/azure/provision.sh teardown` deletes the resource group.

Steps, cost, how to check the flow end to end and the deploy's own limits: [`deploy/azure/README.md`](deploy/azure/README.md).

Public URL: <!-- public URL after B04 runs --> not deployed yet.

<!-- Older deployments, from before Service Bus: AWS EC2 (http://32.192.194.160, not reachable on 30 Oct 2026) and Render
     (https://chatapp-frontend-5cl3.onrender.com/, frontend page loads; backend not checked). Notes in deploy/aws/ and backend/render.yaml.
     Keep, fix or delete. -->

## Known limitations

- **No outbox** (see [Why Service Bus](#why-service-bus)): a failed publish leaves a saved message with no notification.
- **Notifications are written but not read.** `Notifications` rows are created for each recipient, but no endpoint returns them yet; the client only uses the live SignalR push.
- **One replica.** The worker and the SignalR hub run in the API process with no SignalR backplane, so the API can't scale out yet.
- **`ProcessedMessages` grows forever.** Nothing cleans old rows up.
- **Demo logins skip the password check.** `AuthService.LoginAsync` lets the seeded `u1`...`u15@gmail.com` in with any password. Registered users are checked with BCrypt.
- **No refresh tokens.** `RefreshAsync` throws `NotImplementedException`; there is only the access token.
- **Threads only come from seed data.** There is no endpoint to create a thread, and accepting a friend request doesn't create one. The client has a reject-request call, but the API has no reject endpoint.
- **No presence tracking.** `User.IsOnline` exists but nothing sets it.
- **No frontend tests**, and the Angular client is not part of the Azure deploy. `docker-compose.yml` runs only Postgres and the Service Bus emulator, not the API or client.

## Screenshots

<details>
<summary>Login, connections, chat list, chat, reply, reactions, seen status</summary>

<p align="center"><img src="https://github.com/user-attachments/assets/10f43200-c687-43d4-ac0d-23e00d5b41c5" alt="Login screen" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/a3fea6bf-019e-46a2-91de-fcad47f8b4f1" alt="Connections: friends, pending requests, suggestions" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/6dd3166a-cf92-40be-8f5e-b8fa7c3f6f43" alt="Sending a friend request" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/098c2c03-ccf3-4157-9169-6ef610562a08" alt="Chat list with recent conversations" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/72d12355-e8ca-4a12-b276-4eefcc4771c9" alt="Real-time chat" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/f0f3ff03-69fc-4b31-be1a-b67d945b26b2" alt="Replying to a message" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/47a1648f-3302-49af-ba3a-5763c7e84f0c" alt="Message reactions" width="800" /></p>
<p align="center"><img src="https://github.com/user-attachments/assets/d88fe01e-500f-48e1-905a-52f32409fc61" alt="Seen status" width="800" /></p>

</details>

## Author
Md Jakir Hossen · [GitHub](https://github.com/Zakir-sust) · [LinkedIn](https://www.linkedin.com/in/jakir68)
