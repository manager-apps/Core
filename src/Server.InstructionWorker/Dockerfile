FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Server.InstructionWorker/Server.InstructionWorker.csproj", "src/Server.InstructionWorker/"]
COPY ["src/Common/Common.csproj", "src/Common/"]
COPY ["src/Server.Domain/Server.Domain.csproj", "src/Server.Domain/"]
RUN dotnet restore "src/Server.InstructionWorker/Server.InstructionWorker.csproj"
COPY . .
WORKDIR "/src/src/Server.InstructionWorker"
RUN dotnet build "./Server.InstructionWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Server.InstructionWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Server.InstructionWorker.dll"]
