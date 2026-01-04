#!/bin/bash

# Sqordia BDD Test Runner
# This script runs the BDD tests using SpecFlow

set -e

echo "🚀 Starting Sqordia BDD Tests..."
echo "=================================="

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install .NET 8.0 SDK."
    exit 1
fi

# Check .NET version
DOTNET_VERSION=$(dotnet --version)
echo "📦 .NET Version: $DOTNET_VERSION"

# Navigate to the BDD tests directory
cd "$(dirname "$0")/Sqordia.BDDTests"

echo "📁 Working directory: $(pwd)"

# Restore packages
echo "📥 Restoring NuGet packages..."
dotnet restore

# Build the project
echo "🔨 Building BDD test project..."
dotnet build --configuration Release --no-restore

# Run BDD tests with different options
echo ""
echo "🧪 Running BDD Tests..."
echo "======================"

# Function to run tests with specific options
run_tests() {
    local description="$1"
    local filter="$2"
    local additional_args="$3"
    
    echo ""
    echo "📋 $description"
    echo "----------------------------------------"
    
    if [ -n "$filter" ]; then
        dotnet test --configuration Release --no-build --verbosity normal --filter "$filter" $additional_args
    else
        dotnet test --configuration Release --no-build --verbosity normal $additional_args
    fi
}

# Run all BDD tests
run_tests "Running all BDD tests" "" ""

# Run tests by feature
echo ""
echo "📊 Running tests by feature..."
run_tests "Business Plan Generation tests" "Feature=BusinessPlanGeneration" ""
run_tests "User Authentication tests" "Feature=UserAuthentication" ""
run_tests "Organization Management tests" "Feature=OrganizationManagement" ""
run_tests "AI Integration tests" "Feature=AIIntegration" ""

# Run tests by tag
echo ""
echo "🏷️  Running tests by tag..."
run_tests "Business plan tests" "Tag=business-plan" ""
run_tests "Authentication tests" "Tag=authentication" ""
run_tests "Organization tests" "Tag=organization" ""
run_tests "AI tests" "Tag=ai" ""
run_tests "Happy path tests" "Tag=happy-path" ""

# Run tests with detailed output
echo ""
echo "📈 Running tests with detailed output..."
dotnet test --configuration Release --no-build --verbosity detailed --logger "console;verbosity=detailed"

# Generate test reports
echo ""
echo "📊 Generating test reports..."
dotnet test --configuration Release --no-build --logger "trx;LogFileName=BDDTestResults.trx" --collect:"XPlat Code Coverage"

echo ""
echo "✅ BDD Tests completed!"
echo "======================"
echo "📁 Test results: ./TestResults/"
echo "📄 Test report: ./TestResults/BDDTestResults.trx"
echo "📊 Coverage report: ./TestResults/coverage.cobertura.xml"

# Check if tests passed
if [ $? -eq 0 ]; then
    echo "🎉 All BDD tests passed successfully!"
else
    echo "❌ Some BDD tests failed. Check the output above for details."
    exit 1
fi
