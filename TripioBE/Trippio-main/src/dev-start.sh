#!/bin/bash

# Development startup script
echo "🛠️  Starting development environment..."

# Stop existing containers
echo "🛑 Stopping existing containers..."
docker-compose down

# Build and start development services
echo "🔨 Building and starting development services..."
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build

# Wait for services
echo "⏳ Waiting for services to start..."
sleep 30

# Check service status
echo "📊 Service status:"
docker-compose ps

# Show development endpoints
echo "✅ Development environment ready!"
echo "🌐 API: http://localhost:7142"
echo "📝 Swagger: http://localhost:7142/swagger"
echo "📊 Portainer: http://localhost:9000"
echo "📧 MailHog: http://localhost:8025"
echo "💾 SQL Server: localhost:1433"

# Follow logs
echo "📝 Following logs (Ctrl+C to exit)..."
docker-compose logs -f