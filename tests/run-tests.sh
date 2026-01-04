#!/bin/bash

# Test runner script for Sqordia project
# This script runs all unit and integration tests

set -e

echo "🚀 Starting Sqordia Test Suite"
echo "================================"

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

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    print_error "dotnet CLI is not installed. Please install .NET 8.0 SDK."
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
print_status "Using .NET version: $DOTNET_VERSION"

# Get the project root directory
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
print_status "Project root: $PROJECT_ROOT"

# Change to project root
cd "$PROJECT_ROOT"

# Clean and restore packages
print_status "Cleaning and restoring packages..."
dotnet clean
dotnet restore

# Build the solution
print_status "Building the solution..."
dotnet build --configuration Release --no-restore

if [ $? -ne 0 ]; then
    print_error "Build failed. Please fix build errors before running tests."
    exit 1
fi

print_success "Build completed successfully!"

# Run unit tests
print_status "Running unit tests..."
echo "================================"

UNIT_TEST_PROJECT="tests/Sqordia.Application.UnitTests/Sqordia.Application.UnitTests.csproj"

if [ -f "$UNIT_TEST_PROJECT" ]; then
    print_status "Running unit tests from: $UNIT_TEST_PROJECT"
    dotnet test "$UNIT_TEST_PROJECT" \
        --configuration Release \
        --no-build \
        --verbosity normal \
        --collect:"XPlat Code Coverage" \
        --results-directory ./TestResults/UnitTests \
        --logger "trx;LogFileName=unittests.trx"
    
    if [ $? -eq 0 ]; then
        print_success "Unit tests passed!"
    else
        print_error "Unit tests failed!"
        UNIT_TEST_FAILED=true
    fi
else
    print_warning "Unit test project not found: $UNIT_TEST_PROJECT"
fi

# Run integration tests
print_status "Running integration tests..."
echo "================================"

INTEGRATION_TEST_PROJECT="tests/Sqordia.Infrastructure.IntegrationTests/Sqordia.Infrastructure.IntegrationTests.csproj"

if [ -f "$INTEGRATION_TEST_PROJECT" ]; then
    print_status "Running integration tests from: $INTEGRATION_TEST_PROJECT"
    dotnet test "$INTEGRATION_TEST_PROJECT" \
        --configuration Release \
        --no-build \
        --verbosity normal \
        --collect:"XPlat Code Coverage" \
        --results-directory ./TestResults/IntegrationTests \
        --logger "trx;LogFileName=integrationtests.trx"
    
    if [ $? -eq 0 ]; then
        print_success "Integration tests passed!"
    else
        print_error "Integration tests failed!"
        INTEGRATION_TEST_FAILED=true
    fi
else
    print_warning "Integration test project not found: $INTEGRATION_TEST_PROJECT"
fi

# Summary
echo ""
echo "================================"
echo "🏁 Test Suite Summary"
echo "================================"

if [ "$UNIT_TEST_FAILED" = true ]; then
    print_error "❌ Unit tests failed"
else
    print_success "✅ Unit tests passed"
fi

if [ "$INTEGRATION_TEST_FAILED" = true ]; then
    print_error "❌ Integration tests failed"
else
    print_success "✅ Integration tests passed"
fi

# Check if any tests failed
if [ "$UNIT_TEST_FAILED" = true ] || [ "$INTEGRATION_TEST_FAILED" = true ]; then
    print_error "Some tests failed. Please check the output above for details."
    exit 1
else
    print_success "All tests passed! 🎉"
    exit 0
fi
