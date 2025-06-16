FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000
# Install netcat for wait-for-it script
RUN apt-get update && apt-get install -y netcat-traditional && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Company.Api/Company.Api.csproj", "Company.Api/"]
COPY ["Company.Application/Company.Application.csproj", "Company.Application/"]
COPY ["Company.Domain/Company.Domain.csproj", "Company.Domain/"]
COPY ["Company.Infrastructure/Company.Infrastructure.csproj", "Company.Infrastructure/"]
RUN dotnet restore "Company.Api/Company.Api.csproj"
COPY . .
WORKDIR "/src/Company.Api"
RUN dotnet build "Company.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Company.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY wait-for-it.sh .
RUN chmod +x wait-for-it.sh
ENTRYPOINT ["dotnet", "Company.Api.dll"] 