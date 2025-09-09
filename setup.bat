@echo off
REM AutoBattler - Setup Script for Windows
REM This script installs all dependencies and sets up the project

echo 🚀 AutoBattler Setup Script
echo ==========================
echo.

REM Check if we're in the right directory
if not exist "backend\backend.csproj" (
    echo ❌ Error: Please run this script from the AutoBattler root directory
    pause
    exit /b 1
)

if not exist "frontend\package.json" (
    echo ❌ Error: Please run this script from the AutoBattler root directory
    pause
    exit /b 1
)

echo [INFO] Starting AutoBattler setup...
echo.

REM ===========================================
REM SYSTEM REQUIREMENTS CHECK
REM ===========================================

echo [INFO] Checking system requirements...

REM Check for .NET
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: .NET 8.0 is not installed!
    echo [INFO] Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
) else (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo ✅ .NET is installed: %DOTNET_VERSION%
)

REM Check for Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: Node.js is not installed!
    echo [INFO] Please install Node.js from: https://nodejs.org/
    pause
    exit /b 1
) else (
    for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
    echo ✅ Node.js is installed: %NODE_VERSION%
)

REM Check for npm
npm --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: npm is not installed!
    pause
    exit /b 1
) else (
    for /f "tokens=*" %%i in ('npm --version') do set NPM_VERSION=%%i
    echo ✅ npm is installed: %NPM_VERSION%
)

echo.

REM ===========================================
REM BACKEND SETUP
REM ===========================================

echo [INFO] Setting up backend (.NET)...
cd backend

REM Restore .NET packages
echo [INFO] Restoring .NET packages...
dotnet restore
if errorlevel 1 (
    echo ❌ Failed to restore backend packages
    pause
    exit /b 1
) else (
    echo ✅ Backend packages restored successfully
)

REM Build the project
echo [INFO] Building backend project...
dotnet build
if errorlevel 1 (
    echo ❌ Failed to build backend
    pause
    exit /b 1
) else (
    echo ✅ Backend built successfully
)

cd ..
echo.

REM ===========================================
REM FRONTEND SETUP
REM ===========================================

echo [INFO] Setting up frontend (Angular)...

REM Check if Angular CLI is installed globally
ng version >nul 2>&1
if errorlevel 1 (
    echo [INFO] Installing Angular CLI globally...
    npm install -g @angular/cli
    if errorlevel 1 (
        echo ❌ Failed to install Angular CLI
        pause
        exit /b 1
    ) else (
        echo ✅ Angular CLI installed successfully
    )
) else (
    echo ✅ Angular CLI is already installed
)

cd frontend

REM Install npm packages
echo [INFO] Installing frontend packages...
npm install
if errorlevel 1 (
    echo ❌ Failed to install frontend packages
    pause
    exit /b 1
) else (
    echo ✅ Frontend packages installed successfully
)

REM Build the frontend
echo [INFO] Building frontend project...
ng build
if errorlevel 1 (
    echo ⚠️  Frontend build failed, but this might be normal for development
) else (
    echo ✅ Frontend built successfully
)

cd ..
echo.

REM ===========================================
REM DATABASE SETUP (Optional)
REM ===========================================

echo [INFO] Database setup (optional)...
echo [INFO] The project uses SQLite for development (no setup required)
echo [INFO] For production, you'll need PostgreSQL
echo [INFO] Install PostgreSQL from: https://www.postgresql.org/download/
echo.

REM ===========================================
REM FINAL SETUP
REM ===========================================

REM Create a simple .env template if it doesn't exist
if not exist "backend\.env" (
    echo [INFO] Creating .env template...
    (
        echo # AutoBattler Environment Variables
        echo # Copy this file and fill in your values
        echo.
        echo # Database
        echo ConnectionStrings__DefaultConnection=Data Source=autobattler.db
        echo.
        echo # JWT Settings
        echo JWT__SecretKey=your-secret-key-here
        echo JWT__Issuer=AutoBattler
        echo JWT__Audience=AutoBattler
        echo JWT__ExpiryMinutes=60
        echo.
        echo # Google OAuth (optional)
        echo Google__ClientId=your-google-client-id
        echo Google__ClientSecret=your-google-client-secret
    ) > backend\.env
    echo ✅ Created .env template in backend directory
)

echo.
echo ✅ 🎉 Setup completed successfully!
echo.
echo [INFO] Next steps:
echo   1. Configure your environment variables in backend\.env
echo   2. Run the application with: run.bat
echo   3. Backend will be available at: http://localhost:5000
echo   4. Frontend will be available at: http://localhost:4200
echo.
echo [INFO] For more information, see requirements.txt
echo.
pause
