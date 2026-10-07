#!/bin/bash
set -e

IMAGE_NAME="doanson44/thaix"
TAG="latest"
CONTAINER_NAME="thaix"

# 1. Pull the latest image
echo "Pulling latest image..."
docker pull $IMAGE_NAME:$TAG

# 2. Stop and remove the existing container if it exists
if [ "$(docker ps -aq -f name=^/${CONTAINER_NAME}$)" ]; then
    echo "Stopping and removing existing container..."
    docker rm -f $CONTAINER_NAME
fi

# 3. Ensure directories exist and have proper permissions for SELinux (if on Oracle Linux)
echo "Ensuring host directories exist..."
sudo mkdir -p /srv/thaix/{storage,logs,keys}
# If you are not running as root, make sure your user owns the folders, or container can write to them
# sudo chown -R 1000:1000 /srv/thaix

# 4. Run the new container
echo "Starting new container..."
docker run -d --name $CONTAINER_NAME --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v /srv/thaix/appsettings.Production.json:/app/appsettings.Production.json:ro,z \
  -v /srv/thaix/storage:/app/storage:z \
  -v /srv/thaix/logs:/app/Logs:z \
  -v /srv/thaix/keys:/root/.aspnet/DataProtection-Keys:z \
  $IMAGE_NAME:$TAG

# 5. Clean up dangling images (images with <none> tag) to free up space
echo "Cleaning up dangling images..."
docker image prune -f

echo "Deployment complete! Checking status:"
docker ps --filter name=$CONTAINER_NAME
