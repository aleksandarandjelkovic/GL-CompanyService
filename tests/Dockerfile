FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Set environment variables for NuGet
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_NUGET_FALLBACK_PACKAGES=""

# Copy the entire solution
COPY . .

# Create a simple NuGet.config directly in the container
RUN echo '<?xml version="1.0" encoding="utf-8"?>\n\
<configuration>\n\
  <packageSources>\n\
    <clear />\n\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />\n\
  </packageSources>\n\
  <fallbackPackageFolders>\n\
    <clear />\n\
  </fallbackPackageFolders>\n\
</configuration>' > /src/NuGet.config

# Install coverage tools
RUN dotnet tool install -g dotnet-reportgenerator-globaltool
ENV PATH="${PATH}:/root/.dotnet/tools"

# Add coverlet packages to test projects
RUN dotnet add Company.Api.UnitTests/Company.Api.UnitTests.csproj package coverlet.msbuild && \
    dotnet add Company.Infrastructure.UnitTests/Company.Infrastructure.UnitTests.csproj package coverlet.msbuild && \
    dotnet add Company.Application.UnitTests/Company.Application.UnitTests.csproj package coverlet.msbuild && \
    dotnet add Company.Api.IntegrationTests/Company.Api.IntegrationTests.csproj package coverlet.msbuild

# Restore and build each project separately
RUN dotnet restore Company.Domain/Company.Domain.csproj && \
    dotnet restore Company.Application/Company.Application.csproj && \
    dotnet restore Company.Infrastructure/Company.Infrastructure.csproj && \
    dotnet restore Company.Api/Company.Api.csproj && \
    dotnet restore Company.Auth/Company.Auth.csproj && \
    dotnet restore Company.Api.UnitTests/Company.Api.UnitTests.csproj && \
    dotnet restore Company.Infrastructure.UnitTests/Company.Infrastructure.UnitTests.csproj && \
    dotnet restore Company.Application.UnitTests/Company.Application.UnitTests.csproj && \
    dotnet restore Company.Api.IntegrationTests/Company.Api.IntegrationTests.csproj

# Build each project
RUN dotnet build Company.Domain/Company.Domain.csproj -c Release && \
    dotnet build Company.Application/Company.Application.csproj -c Release && \
    dotnet build Company.Infrastructure/Company.Infrastructure.csproj -c Release && \
    dotnet build Company.Api/Company.Api.csproj -c Release && \
    dotnet build Company.Auth/Company.Auth.csproj -c Release && \
    dotnet build Company.Api.UnitTests/Company.Api.UnitTests.csproj -c Release && \
    dotnet build Company.Infrastructure.UnitTests/Company.Infrastructure.UnitTests.csproj -c Release && \
    dotnet build Company.Application.UnitTests/Company.Application.UnitTests.csproj -c Release && \
    dotnet build Company.Api.IntegrationTests/Company.Api.IntegrationTests.csproj -c Release

# Create directory for test results
RUN mkdir -p /app/TestResults

# Create a test script that will run all test projects
RUN echo '#!/bin/bash\n\
dotnet test Company.Api.UnitTests/Company.Api.UnitTests.csproj "$@"\n\
dotnet test Company.Infrastructure.UnitTests/Company.Infrastructure.UnitTests.csproj "$@"\n\
dotnet test Company.Application.UnitTests/Company.Application.UnitTests.csproj "$@"\n\
dotnet test Company.Api.IntegrationTests/Company.Api.IntegrationTests.csproj "$@"\n\
' > /src/run-tests.sh && chmod +x /src/run-tests.sh

# Default entrypoint runs the test script
ENTRYPOINT ["/src/run-tests.sh"]
CMD ["--logger", "trx;LogFileName=/app/TestResults/TestResults.trx"] 