@echo off
REM ===================================================
REM Trippio Setup Script for First-Time Users
REM ===================================================

echo.
echo ============================================
echo   Trippio Backend - First Time Setup
echo ============================================
echo.

REM Check if ngrok exists
if not exist "C:\Users\Windows\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe" (
    echo WARNING: Ngrok not found at default location!
    echo.
    echo Please follow these steps:
    echo 1. Download ngrok from: https://ngrok.com/download
    echo 2. Extract to: C:\Users\Windows\Downloads\ngrok-v3-stable-windows-amd64\
    echo 3. Or update the path in start-with-ngrok.bat
    echo.
    echo After setup, you can configure ngrok with:
    echo   ngrok config add-authtoken 34Htf0hDwTVnzBII6IjzIFRxa55_38erVz7YFPKvqkZpbc6M7
    echo.
    pause
    exit /b 1
)

REM Configure ngrok auth token
echo [1/3] Configuring ngrok authentication...
"C:\Users\Windows\Downloads\ngrok-v3-stable-windows-amd64\ngrok.exe" config add-authtoken 34Htf0hDwTVnzBII6IjzIFRxa55_38erVz7YFPKvqkZpbc6M7

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to configure ngrok!
    pause
    exit /b 1
)

echo ✓ Ngrok configured successfully
echo.

REM Pull Docker images
echo [2/3] Pulling Docker images...
docker compose pull

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to pull Docker images!
    pause
    exit /b 1
)

echo ✓ Docker images pulled successfully
echo.

REM Build API image
echo [3/3] Building API Docker image...
docker compose build

if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to build Docker images!
    pause
    exit /b 1
)

echo ✓ Build completed successfully
echo.

echo.
echo ============================================
echo   Setup Complete! 
echo ============================================
echo.
echo Next steps:
echo   1. Run: start-with-ngrok.bat (to start all services with ngrok)
echo   2. Or run: docker compose up -d (without ngrok)
echo.
echo Your API will be available at:
echo   - Local: http://localhost:7142
echo   - Public (ngrok): https://uninvigorated-unmoribundly-nayeli.ngrok-free.dev
echo.
echo Swagger UI: http://localhost:7142/swagger
echo.
pause
