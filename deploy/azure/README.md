# Deploy the API to Azure Container Apps

`provision.sh` creates everything the API needs on Azure with the `az` CLI and prints the public URL.
The image comes from GitHub Container Registry, built by `.github/workflows/image.yml`.
Out of scope here: custom domain, scaling rules, the Angular frontend, Bicep/Terraform, CD.

```
GitHub (push to main) ──image.yml──> ghcr.io/learning-projects-v1/chat-api
                                              │ pull
Azure resource group rg-chatapp               ▼
  Container Apps environment ──> container app chatapp-api (:8080, HTTPS ingress, 0–1 replica)
                                   │  API + NotificationWorker in one process
                                   ├──> PostgreSQL Flexible Server (B1ms)   EF migrations run at startup
                                   └──> Service Bus (Basic) queue message-sent   publish + consume
  Log Analytics workspace <── container logs
```

## What it creates

| Resource | Name (default) | Settings |
|---|---|---|
| Resource group | `rg-chatapp` | region `centralindia` (`LOCATION=` to change) |
| Log Analytics workspace | `log-chatapp` | 30-day retention, **0.1 GB/day ingestion cap** |
| Container Apps environment | `cae-chatapp` | consumption plan |
| Service Bus namespace + queue | `sb-chatapp-<suffix>` / `message-sent` | Basic tier; queue matches the emulator config (max delivery 3, lock 30 s, TTL 1 h) |
| Queue SAS rule | `chatapp-api` | **Send + Listen on that one queue only** (no Manage, no other queues) |
| PostgreSQL Flexible Server + database | `psql-chatapp-<suffix>` / `chatapp` | Burstable B1ms, 32 GB, v16, no HA, firewall = "allow Azure services" |
| Container app | `chatapp-api` | 0.25 vCPU / 0.5 GiB, min 0 / max 1 replica, external ingress on 8080 |

`<suffix>` is 6 digits derived from your subscription id, so the global names are unique but stable across re-runs.

**Secrets** (Postgres connection string, Service Bus connection string, JWT key) are stored as
Container Apps secrets and reach the app as env vars through `secretref:`. The script reads them from
env vars or a hidden prompt (Enter = generate a random one), never prints them and never writes them to disk.
They do appear briefly as `az` arguments on your own machine (`ps`), which is fine for a single-user laptop.

**App settings it sets:** `ASPNETCORE_ENVIRONMENT=Production` (so `appsettings.Production.json` runs EF
migrations + demo seed data on startup), `ServiceBus__Enabled=true`, `ConnectionStrings__DefaultConnection`,
`ConnectionStrings__ServiceBus`, `JwtSettings__*`, and `AllowedFrontendOrigin` if you pass `FRONTEND_ORIGIN`.

**Redis** is not deployed: the API references the StackExchange.Redis package but never uses it, so nothing
needs a Redis server at startup.

## Cost (rough, per month, USD list prices)

| Resource | Cost |
|---|---|
| Container app, scale to zero | $0 inside the monthly free grant (180k vCPU-s, 360k GiB-s, 2M requests). `MIN_REPLICAS=1` ≈ $3–5 |
| Log Analytics | ~$0 (first 5 GB/month free; the daily cap keeps it there) |
| Service Bus Basic | ~$0 ($0.05 per million operations) |
| PostgreSQL B1ms + 32 GB | ~$15–17, **or $0 for 12 months** on an Azure free account / Azure for Students (750 h B1ms + 32 GB storage free). Check *Free services* in the portal. |
| GHCR (public package) | $0 |

So: ~$0 on free-tier credit, ~$15–20 without it, almost all of it Postgres. Between demos you can
`az postgres flexible-server stop -g rg-chatapp -n psql-chatapp-<suffix>` (Azure starts it again after 7 days;
the API fails to start while it is stopped).

**Set a $5 budget alert yourself before the first run:** portal → Cost Management → Budgets → Add →
scope = your subscription, amount $5, alerts at 50% and 100% to your email. The script does not create it.

## Run it

1. **Image.** Merge to `main` (or run the *Image* workflow by hand: Actions → Image → Run workflow).
   It pushes `ghcr.io/learning-projects-v1/chat-api:latest` and `:sha-<short>`.
   Then make the package pullable, one of:
   - GitHub → org `learning-projects-v1` → Packages → `chat-api` → Package settings → Change visibility → **Public** (simplest), or
   - keep it private and pass `GHCR_USERNAME=<you> GHCR_TOKEN=<classic PAT with read:packages>` to the script.
2. **Azure CLI.** Install `az`, then `az login` and, if you have several subscriptions,
   `az account set --subscription "<name>"`.
3. **Budget alert** (above).
4. **Provision** (Postgres creation takes 5–10 minutes; the whole run ~15):
   ```bash
   ./deploy/azure/provision.sh
   ```
   It shows the subscription and names and asks before creating anything. Options are env vars at the top of the
   script, e.g. `LOCATION=southeastasia`, `IMAGE_TAG=sha-abc1234`, `MIN_REPLICAS=1`, `FRONTEND_ORIGIN=https://...`.
   If Azure says the region is not available for your subscription (common for Postgres on free/student
   subscriptions), re-run with another `LOCATION`; delete the half-made group first with teardown.
5. **Re-run any time** to deploy a new image or rotate secrets: existing resources are kept, the container app
   gets a new revision. Note: a re-run without `PG_ADMIN_PASSWORD`/`JWT_SECRET` generates new ones (the Postgres
   password is reset to match; a new JWT key logs everyone out).

## Check it end to end

```bash
URL=https://<fqdn printed by the script>
curl $URL/health                                   # Healthy  (first call can take ~20 s: cold start + migrations)

# Demo users u1..u15@gmail.com come from the seed data and log in with any password.
TOKEN=$(curl -s -X POST $URL/api/auth/login -H 'Content-Type: application/json' \
  -d '{"email":"u1@gmail.com","password":"demo"}' | jq -r .accessToken)
THREAD=$(curl -s $URL/api/ChatOverview -H "Authorization: Bearer $TOKEN" | jq -r '.[0].chatThreadId')
curl -s -X POST $URL/api/threads/$THREAD/messages -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d "{\"chatThreadId\":\"$THREAD\",\"content\":\"hello from Azure\",\"sentAt\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}"

# The notification went through Service Bus if the worker logged it:
az containerapp logs show -g rg-chatapp -n chatapp-api --tail 50 | grep "Processed MessageSent"
```
The portal shows the same in the Service Bus namespace → *Metrics* (Incoming / Completed messages).
Dead-lettered messages (3 failed attempts) are under the queue → *Service Bus Explorer* → Dead-letter.

## Tear down

```bash
./deploy/azure/provision.sh teardown     # asks you to type the group name, then: az group delete -n rg-chatapp --yes --no-wait
```
Everything lives in the one resource group, so this removes all of it (and stops all cost).
The GHCR image stays on GitHub (free).

## Known limits (on purpose, for now)
- **One replica max.** SignalR connections and the in-process NotificationWorker have no backplane; more
  replicas would need Azure SignalR Service (or a Redis backplane) and sticky sessions.
- **Connection strings, not managed identity.** The app takes `ConnectionStrings:ServiceBus`; switching to a
  managed identity + `DefaultAzureCredential` would remove the Service Bus and Postgres secrets entirely.
- **Postgres firewall = "allow Azure services"** (any Azure-hosted client can reach the port; it still needs the
  password and TLS). A VNet-integrated environment + private access is the stricter, pricier option.
- **The app uses the Postgres admin user.** A separate least-privilege DB user is a later hardening step.
- **Demo logins:** the seeded demo users skip the password check (existing app behaviour), so anyone with the URL
  can log in as them. There is no real data in this deployment.
