#!/bin/sh

echo "Running integration tests..."

# Run integration tests with detailed output
dotnet test Company.Api.IntegrationTests/Company.Api.IntegrationTests.csproj -c Release --logger:console\;verbosity=detailed

# Check if any tests failed
if [ $? -ne 0 ]; then
    echo "Integration tests failed!"
    exit 1
fi

echo "All integration tests passed!" 