FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Set environment variables for NuGet
ENV NUGET_PACKAGES=/root/.nuget/packages
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true
ENV DOTNET_CLI_TELEMETRY_OPTOUT=true
ENV DOTNET_NUGET_FALLBACK_PACKAGES=""

# Create a custom NuGet.config directly in the container
RUN echo '<?xml version="1.0" encoding="utf-8"?>\n\
<configuration>\n\
  <packageSources>\n\
    <clear />\n\
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />\n\
  </packageSources>\n\
  <disabledPackageSources>\n\
    <clear />\n\
  </disabledPackageSources>\n\
  <fallbackPackageFolders>\n\
    <clear />\n\
  </fallbackPackageFolders>\n\
  <config>\n\
    <add key="globalPackagesFolder" value="/root/.nuget/packages" />\n\
  </config>\n\
</configuration>' > /root/NuGet.Config

COPY ["Company.Auth/Company.Auth.csproj", "Company.Auth/"]
COPY ["Company.Domain/Company.Domain.csproj", "Company.Domain/"]
COPY ["Company.Application/Company.Application.csproj", "Company.Application/"]
COPY ["Company.Infrastructure/Company.Infrastructure.csproj", "Company.Infrastructure/"]

# Restore packages using the custom NuGet.Config
RUN dotnet restore "Company.Auth/Company.Auth.csproj" --configfile /root/NuGet.Config

COPY . .
WORKDIR "/src/Company.Auth"
RUN dotnet build "Company.Auth.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Company.Auth.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Company.Auth.dll"] 