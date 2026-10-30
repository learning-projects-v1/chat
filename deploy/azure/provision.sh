#!/usr/bin/env bash
# Provision the ChatApp API on Azure Container Apps (block B04).
#
#   az login                                  # you do this yourself, once
#   ./deploy/azure/provision.sh               # create, or update if it already exists
#   ./deploy/azure/provision.sh teardown      # delete everything (the whole resource group)
#
# Safe to re-run: every resource is "create if missing", and the container app gets its
# secrets, env vars and image refreshed on each run (a new revision).
#
# Secrets come from env vars, or you are prompted (input hidden). Press Enter at a prompt to
# let the script generate a random value. Secrets are never printed and never written to disk.
#   PG_ADMIN_PASSWORD   Postgres admin password (no ';'). On a re-run it is set again on the server,
#                       so whatever you pass (or the newly generated value) becomes the password.
#   JWT_SECRET          JWT signing key, >= 32 characters. A new one logs every user out.
#   GHCR_USERNAME + GHCR_TOKEN   only if the GHCR package is private (PAT with read:packages).
#
# ---------------------------------------------------------------------------------------------
# Rough monthly cost (USD, list prices, light demo traffic; check the Azure pricing calculator):
#   Container Apps, consumption, 0.25 vCPU / 0.5 GiB, min 0 / max 1 replica
#       $0 inside the monthly free grant (180k vCPU-s, 360k GiB-s, 2M requests).
#       MIN_REPLICAS=1 (always warm, no cold start) is roughly $3-5.
#   Log Analytics workspace, 30-day retention, daily cap 0.1 GB      ~$0 (first 5 GB/month free)
#   Service Bus Basic namespace + 1 queue ($0.05 per million ops)      ~$0
#   PostgreSQL Flexible Server, Burstable B1ms + 32 GB storage         ~$15-17
#       FREE for 12 months on an Azure free account (750 h of B1ms + 32 GB storage a month);
#       check "Free services" in the portal, Azure for Students subscriptions include it too.
#       Otherwise it is the only real cost: stop it between demos with
#       `az postgres flexible-server stop -g <rg> -n <server>` (Azure restarts it after 7 days).
#   GHCR image (public package)                                        $0
# Total: ~$0 on free-tier credit, ~$15-20 without it.
#
# REMINDER: set a $5 budget alert yourself before running this (portal: Cost Management >
# Budgets > Add, scope = your subscription, alert at 50% / 100%). The script does not create one.
#
# Teardown: ./deploy/azure/provision.sh teardown   (= az group delete -n "$RG" --yes --no-wait)
# ---------------------------------------------------------------------------------------------
set -euo pipefail

# ---- Settings: override any of these with an env var -----------------------------------------
PREFIX="${PREFIX:-chatapp}"
LOCATION="${LOCATION:-centralindia}"      # close to Dhaka; try southeastasia / eastus2 if your
                                          # subscription says the region is restricted for Postgres
RG="${RG:-rg-${PREFIX}}"
LOG_WORKSPACE="${LOG_WORKSPACE:-log-${PREFIX}}"
ACA_ENV="${ACA_ENV:-cae-${PREFIX}}"
APP_NAME="${APP_NAME:-${PREFIX}-api}"
SB_QUEUE="message-sent"                   # must match QueueNames.MessageSent in the app
SB_RULE="${APP_NAME}"                     # queue-level SAS rule: Send + Listen only
PG_ADMIN="${PG_ADMIN:-chatadmin}"
PG_DB="${PG_DB:-chatapp}"
IMAGE="${IMAGE:-ghcr.io/learning-projects-v1/chat-api:${IMAGE_TAG:-latest}}"
TARGET_PORT=8080                          # the Dockerfile's ASPNETCORE_URLS port
CPU="${CPU:-0.25}"
MEMORY="${MEMORY:-0.5Gi}"
MIN_REPLICAS="${MIN_REPLICAS:-0}"         # 0 = scale to zero when idle (cold start on first request)
MAX_REPLICAS="${MAX_REPLICAS:-1}"         # keep 1: SignalR + NotificationWorker have no backplane
FRONTEND_ORIGIN="${FRONTEND_ORIGIN:-}"    # optional, e.g. https://my-frontend.example (CORS)
# Service Bus namespace and Postgres server names are global DNS names, so they get a short
# suffix derived from the subscription id (stable across re-runs). Set SUFFIX to override.
SUFFIX="${SUFFIX:-}"

# ---- Helpers ---------------------------------------------------------------------------------
log() { printf '\n==> %s\n' "$*"; }
die() { printf 'Error: %s\n' "$*" >&2; exit 1; }
# exists <az show command...>: true when the resource is there.
exists() { "$@" --only-show-errors -o none >/dev/null 2>&1; }
is_tty() { [[ -t 0 ]]; }

generate_secret() {  # generate_secret <length>: random [A-Za-z0-9]
  openssl rand -base64 64 | tr -dc 'A-Za-z0-9' | cut -c1-"$1"
}

# read_secret <VAR_NAME> <prompt>: keep the env var if set, else prompt (hidden), else empty.
read_secret() {
  local name="$1" prompt="$2" value="${!1:-}"
  if [[ -z "$value" ]] && is_tty; then
    read -r -s -p "$prompt" value
    printf '\n' >&2
  fi
  printf -v "$name" '%s' "$value"
}

confirm() {  # confirm <question>: YES=1 skips the question (non-interactive runs)
  [[ "${YES:-}" == "1" ]] && return 0
  is_tty || die "Not a terminal: set YES=1 to run without confirmation."
  local answer
  read -r -p "$1 [y/N] " answer
  [[ "$answer" == "y" || "$answer" == "Y" ]]
}

# ---- Preflight -------------------------------------------------------------------------------
command -v az >/dev/null || die "Azure CLI (az) not found: https://learn.microsoft.com/cli/azure/install-azure-cli"
command -v openssl >/dev/null || die "openssl not found."
az account show -o none 2>/dev/null || die "Not logged in. Run 'az login' yourself, then re-run."

SUB_NAME="$(az account show --query name -o tsv)"
SUB_ID="$(az account show --query id -o tsv)"
if [[ -z "$SUFFIX" ]]; then
  SUFFIX="$(printf '%s' "$SUB_ID" | cksum | cut -d' ' -f1 | cut -c1-6)"
fi
SB_NAMESPACE="${SB_NAMESPACE:-sb-${PREFIX}-${SUFFIX}}"
PG_SERVER="${PG_SERVER:-psql-${PREFIX}-${SUFFIX}}"

# ---- Teardown --------------------------------------------------------------------------------
if [[ "${1:-}" == "teardown" ]]; then
  log "Teardown: delete resource group '$RG' in subscription '$SUB_NAME' (everything in it)"
  exists az group show -n "$RG" || { echo "Resource group '$RG' does not exist. Nothing to do."; exit 0; }
  is_tty || die "Teardown needs a terminal (it asks you to type the group name)."
  read -r -p "Type the resource group name to confirm: " typed
  [[ "$typed" == "$RG" ]] || die "Names do not match; nothing deleted."
  az group delete -n "$RG" --yes --no-wait
  echo "Deletion started (takes a few minutes). Check with: az group show -n $RG"
  exit 0
fi
[[ -z "${1:-}" ]] || die "Unknown argument '$1'. Use no argument to provision, or 'teardown'."

cat <<EOF

Subscription : $SUB_NAME ($SUB_ID)
Region       : $LOCATION
Resource grp : $RG
Creates      : $LOG_WORKSPACE, $ACA_ENV, $SB_NAMESPACE (queue $SB_QUEUE), $PG_SERVER (db $PG_DB), $APP_NAME
Image        : $IMAGE
EOF
confirm "Continue?" || die "Cancelled."

# ---- Secrets ---------------------------------------------------------------------------------
read_secret PG_ADMIN_PASSWORD "Postgres admin password (Enter = generate): "
if [[ -z "${PG_ADMIN_PASSWORD:-}" ]]; then
  # Upper + lower + digit, as Azure's password policy requires.
  PG_ADMIN_PASSWORD="Pg$(generate_secret 24)7"
  echo "Generated a Postgres admin password (not shown). Reset it any time by re-running this script."
fi
[[ "$PG_ADMIN_PASSWORD" != *";"* ]] || die "PG_ADMIN_PASSWORD must not contain ';' (it breaks the connection string)."
[[ ${#PG_ADMIN_PASSWORD} -ge 8 ]] || die "PG_ADMIN_PASSWORD must be at least 8 characters."

read_secret JWT_SECRET "JWT signing secret, >= 32 chars (Enter = generate): "
if [[ -z "${JWT_SECRET:-}" ]]; then
  JWT_SECRET="$(generate_secret 48)"
  echo "Generated a JWT secret (not shown)."
fi
[[ ${#JWT_SECRET} -ge 32 ]] || die "JWT_SECRET must be at least 32 characters (HS256 needs a 256-bit key)."

GHCR_USERNAME="${GHCR_USERNAME:-}"
GHCR_TOKEN="${GHCR_TOKEN:-}"

# ---- Providers + CLI extension ---------------------------------------------------------------
log "Registering resource providers (first run only takes a minute)"
for ns in Microsoft.App Microsoft.OperationalInsights Microsoft.ServiceBus Microsoft.DBforPostgreSQL; do
  az provider register --namespace "$ns" --wait --only-show-errors
done
az extension add --name containerapp --upgrade --only-show-errors

# ---- Resource group --------------------------------------------------------------------------
log "Resource group $RG"
az group create -n "$RG" -l "$LOCATION" --tags project=chatapp block=B04 -o none

# ---- Log Analytics + Container Apps environment ----------------------------------------------
log "Log Analytics workspace $LOG_WORKSPACE"
if ! exists az monitor log-analytics workspace show -g "$RG" -n "$LOG_WORKSPACE"; then
  # --quota = daily ingestion cap in GB: a runaway log loop cannot run up a bill.
  az monitor log-analytics workspace create -g "$RG" -n "$LOG_WORKSPACE" -l "$LOCATION" \
    --retention-time 30 --quota 0.1 -o none
fi

log "Container Apps environment $ACA_ENV"
if ! exists az containerapp env show -g "$RG" -n "$ACA_ENV"; then
  LOG_ID="$(az monitor log-analytics workspace show -g "$RG" -n "$LOG_WORKSPACE" --query customerId -o tsv)"
  LOG_KEY="$(az monitor log-analytics workspace get-shared-keys -g "$RG" -n "$LOG_WORKSPACE" --query primarySharedKey -o tsv)"
  az containerapp env create -g "$RG" -n "$ACA_ENV" -l "$LOCATION" \
    --logs-workspace-id "$LOG_ID" --logs-workspace-key "$LOG_KEY" -o none
  unset LOG_KEY
fi

# ---- Service Bus -----------------------------------------------------------------------------
log "Service Bus namespace $SB_NAMESPACE (Basic) + queue $SB_QUEUE"
if ! exists az servicebus namespace show -g "$RG" -n "$SB_NAMESPACE"; then
  az servicebus namespace create -g "$RG" -n "$SB_NAMESPACE" -l "$LOCATION" --sku Basic -o none
fi
if ! exists az servicebus queue show -g "$RG" --namespace-name "$SB_NAMESPACE" -n "$SB_QUEUE"; then
  # Same settings as servicebus-emulator/Config.json, so local and Azure behave alike.
  az servicebus queue create -g "$RG" --namespace-name "$SB_NAMESPACE" -n "$SB_QUEUE" \
    --max-delivery-count 5 --lock-duration PT30S --default-message-time-to-live PT1H -o none
fi
if ! exists az servicebus queue authorization-rule show -g "$RG" --namespace-name "$SB_NAMESPACE" \
     --queue-name "$SB_QUEUE" -n "$SB_RULE"; then
  # Scoped to this one queue with Send + Listen: the app cannot manage or delete anything.
  az servicebus queue authorization-rule create -g "$RG" --namespace-name "$SB_NAMESPACE" \
    --queue-name "$SB_QUEUE" -n "$SB_RULE" --rights Send Listen -o none
fi
SB_CONNECTION="$(az servicebus queue authorization-rule keys list -g "$RG" --namespace-name "$SB_NAMESPACE" \
  --queue-name "$SB_QUEUE" -n "$SB_RULE" --query primaryConnectionString -o tsv)"

# ---- PostgreSQL ------------------------------------------------------------------------------
log "PostgreSQL Flexible Server $PG_SERVER (Burstable B1ms, 32 GB)"
if ! exists az postgres flexible-server show -g "$RG" -n "$PG_SERVER"; then
  echo "Creating the server takes 5-10 minutes..."
  # --public-access 0.0.0.0 = firewall rule "allow Azure services": the container app's outbound
  # IPs are not fixed on the consumption plan. The server still needs the password + TLS.
  az postgres flexible-server create -g "$RG" -n "$PG_SERVER" -l "$LOCATION" \
    --tier Burstable --sku-name Standard_B1ms --storage-size 32 --version 16 \
    --admin-user "$PG_ADMIN" --admin-password "$PG_ADMIN_PASSWORD" \
    --public-access 0.0.0.0 --high-availability Disabled --backup-retention 7 \
    --yes -o none
else
  # Re-run: make the server password match the one we are about to store as a secret.
  az postgres flexible-server update -g "$RG" -n "$PG_SERVER" --admin-password "$PG_ADMIN_PASSWORD" -o none
fi
if ! exists az postgres flexible-server db show -g "$RG" -s "$PG_SERVER" -d "$PG_DB"; then
  az postgres flexible-server db create -g "$RG" -s "$PG_SERVER" -d "$PG_DB" -o none
fi
# ADO.NET format, as DbContextMiddleware expects. Azure requires TLS.
DB_CONNECTION="Host=${PG_SERVER}.postgres.database.azure.com;Port=5432;Database=${PG_DB};Username=${PG_ADMIN};Password=${PG_ADMIN_PASSWORD};SSL Mode=Require"

# ---- Container app ---------------------------------------------------------------------------
# Secrets live in the container app (encrypted at rest). Env vars only reference them by name
# (secretref:), so `az containerapp show` and the portal's env var view never contain the values.
SECRETS=(
  "db-connection=${DB_CONNECTION}"
  "servicebus-connection=${SB_CONNECTION}"
  "jwt-secret=${JWT_SECRET}"
)
ENV_VARS=(
  "ASPNETCORE_ENVIRONMENT=Production"          # appsettings.Production.json: RunMigrations=true
  "ConnectionStrings__DefaultConnection=secretref:db-connection"
  "ConnectionStrings__ServiceBus=secretref:servicebus-connection"
  "ServiceBus__Enabled=true"
  "JwtSettings__Secret=secretref:jwt-secret"
  "JwtSettings__Issuer=ChatApp"
  "JwtSettings__Audience=ChatAppUsers"
  "JwtSettings__AccessTokenExpirationMinutes=600"
  "JwtSettings__RefreshTokenExpirationHours=24"
)
[[ -z "$FRONTEND_ORIGIN" ]] || ENV_VARS+=("AllowedFrontendOrigin=${FRONTEND_ORIGIN}")

REGISTRY_ARGS=()
if [[ -n "$GHCR_TOKEN" ]]; then
  [[ -n "$GHCR_USERNAME" ]] || die "GHCR_TOKEN is set but GHCR_USERNAME is not."
  REGISTRY_ARGS=(--registry-server ghcr.io --registry-username "$GHCR_USERNAME" --registry-password "$GHCR_TOKEN")
fi

log "Container app $APP_NAME ($IMAGE)"
if ! exists az containerapp show -g "$RG" -n "$APP_NAME"; then
  az containerapp create -g "$RG" -n "$APP_NAME" --environment "$ACA_ENV" \
    --image "$IMAGE" ${REGISTRY_ARGS[@]+"${REGISTRY_ARGS[@]}"} \
    --ingress external --target-port "$TARGET_PORT" --transport auto \
    --cpu "$CPU" --memory "$MEMORY" --min-replicas "$MIN_REPLICAS" --max-replicas "$MAX_REPLICAS" \
    --secrets "${SECRETS[@]}" \
    --env-vars "${ENV_VARS[@]}" \
    -o none
else
  if [[ -n "$GHCR_TOKEN" ]]; then
    az containerapp registry set -g "$RG" -n "$APP_NAME" --server ghcr.io \
      --username "$GHCR_USERNAME" --password "$GHCR_TOKEN" -o none
  fi
  az containerapp secret set -g "$RG" -n "$APP_NAME" --secrets "${SECRETS[@]}" -o none
  # A new revision suffix forces a fresh revision, so new secrets and a re-pushed :latest are picked up.
  az containerapp update -g "$RG" -n "$APP_NAME" --image "$IMAGE" \
    --cpu "$CPU" --memory "$MEMORY" --min-replicas "$MIN_REPLICAS" --max-replicas "$MAX_REPLICAS" \
    --set-env-vars "${ENV_VARS[@]}" \
    --revision-suffix "r$(date +%m%d%H%M%S)" \
    -o none
fi
unset PG_ADMIN_PASSWORD JWT_SECRET SB_CONNECTION DB_CONNECTION SECRETS GHCR_TOKEN REGISTRY_ARGS

FQDN="$(az containerapp show -g "$RG" -n "$APP_NAME" --query properties.configuration.ingress.fqdn -o tsv)"
cat <<EOF

Done. Public URL: https://${FQDN}

  curl https://${FQDN}/health          # "Healthy" (first call may take ~20 s: cold start + migrations)
  az containerapp logs show -g $RG -n $APP_NAME --follow     # app logs
  ./deploy/azure/provision.sh teardown                       # delete everything
EOF
