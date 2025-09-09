@echo off
REM AutoBattler - Start both backend and frontend (Windows)
REM This script will start the .NET backend and Angular frontend

echo 🚀 Starting AutoBattler...
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

REM Check prerequisites
echo 🔍 Checking prerequisites...

REM Check for .NET
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: .NET is not installed. Please install .NET 8.0 or later
    echo    Download from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)

REM Check for Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: Node.js is not installed. Please install Node.js
    echo    Download from: https://nodejs.org/
    pause
    exit /b 1
)

REM Check for Angular CLI
ng version >nul 2>&1
if errorlevel 1 (
    echo ❌ Error: Angular CLI is not installed. Installing...
    npm install -g @angular/cli
    if errorlevel 1 (
        echo ❌ Failed to install Angular CLI
        pause
        exit /b 1
    )
)

echo ✅ Prerequisites check passed
echo.

REM Start backend in a new command window
echo 🔧 Starting backend (.NET API)...
start "AutoBattler Backend" cmd /k "cd /d %~dp0backend && dotnet run"

REM Wait a moment for backend to start
timeout /t 3 /nobreak >nul

REM Start frontend in a new command window
echo 🎨 Starting frontend (Angular)...
start "AutoBattler Frontend" cmd /k "cd /d %~dp0frontend && ng serve"

echo.
echo 🎉 AutoBattler is starting up!
echo    Backend: http://localhost:5000 (or https://localhost:5001)
echo    Frontend: http://localhost:4200
echo.
echo 💡 To stop the servers, close the command windows or press Ctrl+C in each
echo.
pause
