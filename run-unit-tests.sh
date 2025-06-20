#!/bin/sh

echo "Running unit tests..."

# Run unit tests with detailed output
dotnet test Company.Api.UnitTests/Company.Api.UnitTests.csproj -c Release --logger:console\;verbosity=normal
dotnet test Company.Infrastructure.UnitTests/Company.Infrastructure.UnitTests.csproj -c Release --logger:console\;verbosity=normal
dotnet test Company.Application.UnitTests/Company.Application.UnitTests.csproj -c Release --logger:console\;verbosity=normal

# Check if any tests failed
if [ $? -ne 0 ]; then
    echo "Unit tests failed!"
    exit 1
fi

echo "All unit tests passed!" 