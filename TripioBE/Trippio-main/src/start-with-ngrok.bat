@echo off
REM ===================================================
REM Trippio Development Startup Script
REM ===================================================

echo.
echo ============================================
echo   Starting Trippio Backend Services
echo ============================================
echo.

REM Start Docker Compose services
echo [1/2] Starting Docker containers...
docker compose up -d

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to start Docker services!
    pause
    exit /b 1
)

echo.
echo ✓ Docker services started successfully
echo.

REM Wait for API to be ready
echo [2/2] Waiting for API to be ready...
timeout /t 15 /nobreak > nul

echo.
echo ============================================
echo   Starting Ngrok Tunnel
echo ============================================
echo.
echo Static Domain: uninvigorated-unmoribundly-nayeli.ngrok-free.dev
echo Local API: http://localhost:7142
echo Ngrok Dashboard: http://localhost:4040
echo.
echo Press Ctrl+C to stop ngrok and services
echo.

REM Start ngrok with static domain
"C:\Users\Windows\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe" http 7142 --domain=uninvigorated-unmoribundly-nayeli.ngrok-free.dev

REM When ngrok stops (Ctrl+C), stop Docker services
echo.
echo Stopping Docker services...
docker compose down

echo.
echo ============================================
echo   All services stopped
echo ============================================
pause
