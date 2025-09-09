#!/bin/bash

# AutoBattler - Start both backend and frontend
# This script will start the .NET backend and Angular frontend in separate terminals

echo "🚀 Starting AutoBattler..."
echo ""

# Check if we're in the right directory
if [ ! -f "backend/backend.csproj" ] || [ ! -f "frontend/package.json" ]; then
    echo "❌ Error: Please run this script from the AutoBattler root directory"
    exit 1
fi

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "🔍 Checking prerequisites..."

if ! command_exists dotnet; then
    echo "❌ Error: .NET is not installed. Please install .NET 8.0 or later"
    exit 1
fi

if ! command_exists node; then
    echo "❌ Error: Node.js is not installed. Please install Node.js"
    exit 1
fi

if ! command_exists ng; then
    echo "❌ Error: Angular CLI is not installed. Installing..."
    npm install -g @angular/cli
fi

echo "✅ Prerequisites check passed"
echo ""

# Start backend in a new terminal window
echo "🔧 Starting backend (.NET API)..."
osascript -e 'tell application "Terminal" to do script "cd '"$(pwd)"'/backend && dotnet run"' 2>/dev/null || {
    echo "⚠️  Could not open new terminal window. Starting backend in current terminal..."
    echo "   Backend will start in 3 seconds. Press Ctrl+C to stop it, then run 'ng serve' in frontend directory"
    sleep 3
    cd backend && dotnet run &
    cd ..
}

# Wait a moment for backend to start
sleep 2

# Start frontend in a new terminal window
echo "🎨 Starting frontend (Angular)..."
osascript -e 'tell application "Terminal" to do script "cd '"$(pwd)"'/frontend && ng serve"' 2>/dev/null || {
    echo "⚠️  Could not open new terminal window. Please manually run:"
    echo "   cd frontend && ng serve"
}

echo ""
echo "🎉 AutoBattler is starting up!"
echo "   Backend: http://localhost:5000 (or https://localhost:5001)"
echo "   Frontend: http://localhost:4200"
echo ""
echo "💡 To stop the servers, close the terminal windows or press Ctrl+C in each"
