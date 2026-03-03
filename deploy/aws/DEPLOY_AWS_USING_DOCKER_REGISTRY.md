# Deploy to AWS EC2 using prebuilt Docker images (no EC2 build)

This flow avoids heavy builds on Free Tier EC2.

## A) Build and push images from your local machine

From repo root (`/Users/purple_ghost/Documents/my-projects/chat`):

```bash
# 1) Login to Docker Hub (or your registry)
docker login

# 2) Set your Docker Hub username and image version tag
export DOCKER_USER="YOUR_DOCKERHUB_USERNAME"
export IMAGE_TAG="v1"

# 3) Build backend image
docker build -t "$DOCKER_USER/chat-backend:$IMAGE_TAG" -f backend/Dockerfile backend

# 4) Build frontend image (AWS config is already used in Dockerfile)
docker build -t "$DOCKER_USER/chat-frontend:$IMAGE_TAG" -f frontend/chatApp/Dockerfile frontend/chatApp

# 5) Push images
docker push "$DOCKER_USER/chat-backend:$IMAGE_TAG"
docker push "$DOCKER_USER/chat-frontend:$IMAGE_TAG"
```

## B) Run pulled images on EC2

SSH to EC2:

```bash
ssh -i chat.pem ubuntu@YOUR_EC2_PUBLIC_IP
```

Clone/pull repo on EC2:

```bash
cd ~
[ -d chat ] || git clone <your-repo-url> chat
cd chat
git pull
```

Create env file for image-based deploy:

```bash
cp deploy/aws/.env.images.example deploy/aws/.env.images
nano deploy/aws/.env.images
```

Set these values:
- `BACKEND_IMAGE=YOUR_DOCKERHUB_USERNAME/chat-backend:v1`
- `FRONTEND_IMAGE=YOUR_DOCKERHUB_USERNAME/chat-frontend:v1`
- `POSTGRES_PASSWORD=<strong-password>`
- `JWT_SECRET=<32+ chars>`
- `ALLOWED_FRONTEND_ORIGIN=http://YOUR_EC2_PUBLIC_IP`

Deploy:

```bash
docker compose --env-file deploy/aws/.env.images -f deploy/aws/docker-compose.ec2.images.yml pull
docker compose --env-file deploy/aws/.env.images -f deploy/aws/docker-compose.ec2.images.yml up -d
```

Verify:

```bash
docker compose --env-file deploy/aws/.env.images -f deploy/aws/docker-compose.ec2.images.yml ps
docker compose --env-file deploy/aws/.env.images -f deploy/aws/docker-compose.ec2.images.yml logs -f backend
```

Open:

```text
http://YOUR_EC2_PUBLIC_IP
```

## C) Deploy updates later

On local machine after code changes:

```bash
export DOCKER_USER="YOUR_DOCKERHUB_USERNAME"
export IMAGE_TAG="v2"

docker build -t "$DOCKER_USER/chat-backend:$IMAGE_TAG" -f backend/Dockerfile backend
docker build -t "$DOCKER_USER/chat-frontend:$IMAGE_TAG" -f frontend/chatApp/Dockerfile frontend/chatApp

docker push "$DOCKER_USER/chat-backend:$IMAGE_TAG"
docker push "$DOCKER_USER/chat-frontend:$IMAGE_TAG"
```

On EC2:

```bash
cd ~/chat
nano deploy/aws/.env.images   # change BACKEND_IMAGE and FRONTEND_IMAGE to :v2

docker compose --env-file deploy/aws/.env.images -f deploy/aws/docker-compose.ec2.images.yml pull
docker compose --env-file deploy/aws/.env.images -f deploy/aws/docker-compose.ec2.images.yml up -d
```

## Notes

- This keeps EC2 work light; only pulls and runs images.
- PostgreSQL data persists in Docker volume `pgdata`.
- Do not delete volumes unless you want to wipe DB data.
