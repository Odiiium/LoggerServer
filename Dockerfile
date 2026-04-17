FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["LoggerServer.csproj", "."]
RUN dotnet restore "./LoggerServer.csproj"
COPY . .
RUN dotnet build "./LoggerServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./LoggerServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN mkdir -p /app/logs
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LoggerServer.dll"]
