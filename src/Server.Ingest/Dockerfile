FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Server.Ingest/Server.Ingest.csproj", "src/Server.Ingest/"]
COPY ["src/Common/Common.csproj", "src/Common/"]
COPY ["src/Server.Domain/Server.Domain.csproj", "src/Server.Domain/"]
RUN dotnet restore "src/Server.Ingest/Server.Ingest.csproj"
COPY . .
WORKDIR "/src/src/Server.Ingest"
RUN dotnet build "./Server.Ingest.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Server.Ingest.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.Ingest.dll"]
