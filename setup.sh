#!/bin/bash

# AutoBattler - Setup Script for macOS/Linux
# This script installs all dependencies and sets up the project

echo "🚀 AutoBattler Setup Script"
echo "=========================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if a command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check if we're in the right directory
if [ ! -f "backend/backend.csproj" ] || [ ! -f "frontend/package.json" ]; then
    print_error "Please run this script from the AutoBattler root directory"
    exit 1
fi

print_status "Starting AutoBattler setup..."
echo ""

# ===========================================
# SYSTEM REQUIREMENTS CHECK
# ===========================================

print_status "Checking system requirements..."

# Check for .NET
if ! command_exists dotnet; then
    print_error ".NET 8.0 is not installed!"
    print_status "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0"
    print_status "Or on macOS: brew install dotnet"
    print_status "Or on Ubuntu: sudo apt-get install dotnet-sdk-8.0"
    exit 1
else
    DOTNET_VERSION=$(dotnet --version)
    print_success ".NET is installed: $DOTNET_VERSION"
fi

# Check for Node.js
if ! command_exists node; then
    print_error "Node.js is not installed!"
    print_status "Please install Node.js from: https://nodejs.org/"
    print_status "Or on macOS: brew install node"
    print_status "Or on Ubuntu: sudo apt-get install nodejs npm"
    exit 1
else
    NODE_VERSION=$(node --version)
    print_success "Node.js is installed: $NODE_VERSION"
fi

# Check for npm
if ! command_exists npm; then
    print_error "npm is not installed!"
    exit 1
else
    NPM_VERSION=$(npm --version)
    print_success "npm is installed: $NPM_VERSION"
fi

echo ""

# ===========================================
# BACKEND SETUP
# ===========================================

print_status "Setting up backend (.NET)..."
cd backend

# Restore .NET packages
print_status "Restoring .NET packages..."
if dotnet restore; then
    print_success "Backend packages restored successfully"
else
    print_error "Failed to restore backend packages"
    exit 1
fi

# Build the project
print_status "Building backend project..."
if dotnet build; then
    print_success "Backend built successfully"
else
    print_error "Failed to build backend"
    exit 1
fi

cd ..
echo ""

# ===========================================
# FRONTEND SETUP
# ===========================================

print_status "Setting up frontend (Angular)..."

# Check if Angular CLI is installed globally
if ! command_exists ng; then
    print_status "Installing Angular CLI globally..."
    if npm install -g @angular/cli; then
        print_success "Angular CLI installed successfully"
    else
        print_error "Failed to install Angular CLI"
        exit 1
    fi
else
    ANGULAR_VERSION=$(ng version --json | grep -o '"@angular/cli":"[^"]*"' | cut -d'"' -f4)
    print_success "Angular CLI is already installed: $ANGULAR_VERSION"
fi

cd frontend

# Install npm packages
print_status "Installing frontend packages..."
if npm install; then
    print_success "Frontend packages installed successfully"
else
    print_error "Failed to install frontend packages"
    exit 1
fi

# Build the frontend
print_status "Building frontend project..."
if ng build; then
    print_success "Frontend built successfully"
else
    print_warning "Frontend build failed, but this might be normal for development"
fi

cd ..
echo ""

# ===========================================
# DATABASE SETUP (Optional)
# ===========================================

print_status "Database setup (optional)..."
print_status "The project uses SQLite for development (no setup required)"
print_status "For production, you'll need PostgreSQL"
print_status "Install PostgreSQL from: https://www.postgresql.org/download/"
echo ""

# ===========================================
# FINAL SETUP
# ===========================================

# Make run script executable
chmod +x run.sh
print_success "Made run.sh executable"

# Create a simple .env template if it doesn't exist
if [ ! -f "backend/.env" ]; then
    print_status "Creating .env template..."
    cat > backend/.env << EOF
# AutoBattler Environment Variables
# Copy this file and fill in your values

# Database
ConnectionStrings__DefaultConnection=Data Source=autobattler.db

# JWT Settings
JWT__SecretKey=your-secret-key-here
JWT__Issuer=AutoBattler
JWT__Audience=AutoBattler
JWT__ExpiryMinutes=60

# Google OAuth (optional)
Google__ClientId=your-google-client-id
Google__ClientSecret=your-google-client-secret
EOF
    print_success "Created .env template in backend directory"
fi

echo ""
print_success "🎉 Setup completed successfully!"
echo ""
print_status "Next steps:"
echo "  1. Configure your environment variables in backend/.env"
echo "  2. Run the application with: ./run.sh"
echo "  3. Backend will be available at: http://localhost:5000"
echo "  4. Frontend will be available at: http://localhost:4200"
echo ""
print_status "For more information, see requirements.txt"
