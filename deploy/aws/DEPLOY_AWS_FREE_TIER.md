# Deploy Chat App on AWS Free Tier (EC2 + Docker)

This guide deploys your current app to **one EC2 Free Tier instance**:
- Angular frontend (served by Nginx)
- ASP.NET backend (.NET 8)
- PostgreSQL database

All services run with Docker Compose.

## 1) Create AWS resources

1. In AWS console, launch an **EC2 t2.micro or t3.micro** (Ubuntu 24.04 LTS).
2. Attach/choose a key pair so you can SSH.
3. Security Group inbound rules:
   - `22` (SSH) from your IP
   - `80` (HTTP) from `0.0.0.0/0`
   - Optional later: `443` (HTTPS) from `0.0.0.0/0`
4. Copy the instance public IP (for example `3.92.10.14`).

## 2) Connect and install Docker

SSH into the instance:

```bash
ssh -i /path/to/your-key.pem ubuntu@YOUR_EC2_PUBLIC_IP
```

Install Docker + Compose plugin:

```bash
sudo apt update
sudo apt install -y docker.io docker-compose-v2 git
sudo systemctl enable docker
sudo systemctl start docker
sudo usermod -aG docker $USER
newgrp docker
```

## 3) Copy project to EC2

On EC2:

```bash
git clone <your-repo-url> chat
cd chat
```

## 4) Create production env file

```bash
cp deploy/aws/.env.example deploy/aws/.env
nano deploy/aws/.env
```

Set real values:
- `POSTGRES_PASSWORD` = strong password
- `JWT_SECRET` = long random secret (32+ chars)
- `ALLOWED_FRONTEND_ORIGIN` = `http://YOUR_EC2_PUBLIC_IP` (or your domain)

## 5) Build and run

From repo root on EC2:

```bash
docker compose --env-file deploy/aws/.env -f deploy/aws/docker-compose.ec2.yml up -d --build
```

Check status:

```bash
docker compose --env-file deploy/aws/.env -f deploy/aws/docker-compose.ec2.yml ps
```

View logs if needed:

```bash
docker compose --env-file deploy/aws/.env -f deploy/aws/docker-compose.ec2.yml logs -f backend
```

## 6) Test app

Open in browser:

```text
http://YOUR_EC2_PUBLIC_IP
```

If load works and login/register/messages work, deployment is successful.

## 7) Update deployment (after code changes)

```bash
cd chat
git pull
docker compose --env-file deploy/aws/.env -f deploy/aws/docker-compose.ec2.yml up -d --build
```

## Notes

- PostgreSQL data is persisted in Docker volume `pgdata`.
- Backend migrations run automatically at startup (`RunMigrations=true`).
- Frontend production API base URL is now dynamic (`window.location.origin`), so the same build works on EC2 IP or domain.
- Render production config remains unchanged; AWS uses a dedicated Angular `aws` build configuration.

## Optional: add domain + HTTPS

For HTTPS, point a domain to EC2 and place a TLS reverse proxy (Caddy/Nginx+Certbot) in front of this stack.
