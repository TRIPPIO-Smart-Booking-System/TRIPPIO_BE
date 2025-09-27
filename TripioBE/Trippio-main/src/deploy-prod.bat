@echo off
REM Production deployment script for Windows
echo 🚀 Starting production deployment...

REM Stop existing containers
echo 🛑 Stopping existing containers...
docker-compose down

REM Build and start services
echo 🔨 Building and starting services...
docker-compose up -d --build

REM Wait for services to be healthy
echo ⏳ Waiting for services to be healthy...
timeout /t 60 /nobreak

REM Check service status
echo 📊 Service status:
docker-compose ps

REM Show logs
echo 📝 Recent logs:
docker-compose logs --tail=50

echo ✅ Deployment completed!
echo 🌐 API: https://localhost:7142
echo 📊 Portainer: http://localhost:9000
echo 💾 SQL Server: localhost:1433
pause