#!/bin/sh

echo "Running code coverage..."

# Clean previous results
rm -rf /app/TestResults/Coverage/*.json /app/TestResults/Coverage/*.xml
mkdir -p /app/TestResults/coverage-report

# Run unit tests with coverage
echo "==== UNIT TEST COVERAGE ===="
dotnet test Company.Api.UnitTests/Company.Api.UnitTests.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="/app/TestResults/Coverage/Company.Api.UnitTests.coverage.cobertura.xml" /p:ExcludeByAttribute="GeneratedCodeAttribute"
dotnet test Company.Infrastructure.UnitTests/Company.Infrastructure.UnitTests.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="/app/TestResults/Coverage/Company.Infrastructure.UnitTests.coverage.cobertura.xml" /p:ExcludeByAttribute="GeneratedCodeAttribute"
dotnet test Company.Application.UnitTests/Company.Application.UnitTests.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="/app/TestResults/Coverage/Company.Application.UnitTests.coverage.cobertura.xml" /p:ExcludeByAttribute="GeneratedCodeAttribute"

# Run integration tests with coverage
echo "==== INTEGRATION TEST COVERAGE ===="
dotnet test Company.Api.IntegrationTests/Company.Api.IntegrationTests.csproj -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="/app/TestResults/Coverage/Company.Api.IntegrationTests.coverage.cobertura.xml" /p:ExcludeByAttribute="GeneratedCodeAttribute"

# Generate report
echo "Generating coverage report..."
REPORT_FILES=$(find /app/TestResults/Coverage -name "*.cobertura.xml" | tr "\n" ";")
REPORT_FILES=${REPORT_FILES%";"}

# Install reportgenerator if not already installed
if ! [ -x "$(command -v reportgenerator)" ]; then
  echo "Installing reportgenerator..."
  dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generate the report
reportgenerator \
  -reports:"$REPORT_FILES" \
  -targetdir:"/app/TestResults/coverage-report" \
  -reporttypes:"Html;Cobertura;JsonSummary" \
  -title:"Company Service Code Coverage"

echo "Coverage report generated at /app/TestResults/coverage-report" 