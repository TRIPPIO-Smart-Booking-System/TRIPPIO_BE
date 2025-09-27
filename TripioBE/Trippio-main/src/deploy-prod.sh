#!/bin/bash

# Production deployment script
echo "🚀 Starting production deployment..."

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker-compose down

# Remove old images (optional, uncomment if needed)
# docker image prune -af

# Build and start services
echo "🔨 Building and starting services..."
docker-compose up -d --build

# Wait for services to be healthy
echo "⏳ Waiting for services to be healthy..."
timeout 300 bash -c 'until docker-compose ps | grep -q "healthy"; do sleep 5; done'

# Check service status
echo "📊 Service status:"
docker-compose ps

# Show logs
echo "📝 Recent logs:"
docker-compose logs --tail=50

echo "✅ Deployment completed!"
echo "🌐 API: https://localhost:7142"
echo "📊 Portainer: http://localhost:9000"
echo "💾 SQL Server: localhost:1433"