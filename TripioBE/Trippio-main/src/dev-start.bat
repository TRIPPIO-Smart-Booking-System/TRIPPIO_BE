@echo off
REM Development startup script for Windows
echo 🛠️  Starting development environment...

REM Stop existing containers
echo 🛑 Stopping existing containers...
docker-compose down

REM Build and start development services
echo 🔨 Building and starting development services...
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d --build

REM Wait for services
echo ⏳ Waiting for services to start...
timeout /t 30 /nobreak

REM Check service status
echo 📊 Service status:
docker-compose ps

REM Show development endpoints
echo ✅ Development environment ready!
echo 🌐 API: http://localhost:7142
echo 📝 Swagger: http://localhost:7142/swagger
echo 📊 Portainer: http://localhost:9000
echo 📧 MailHog: http://localhost:8025
echo 💾 SQL Server: localhost:1433

echo Press any key to view logs...
pause
docker-compose logs -f