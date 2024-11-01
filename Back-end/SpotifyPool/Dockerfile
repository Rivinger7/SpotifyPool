# Use the ASP.NET Core image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use SDK image for build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SpotifyPool/SpotifyPool.csproj", "SpotifyPool/"]
COPY ["BusinessLogicLayer.DependencyInjection/BusinessLogicLayer.DependencyInjection.csproj", "BusinessLogicLayer.DependencyInjection/"]
COPY ["BusinessLogicLayer.Implement/BusinessLogicLayer.Implement.csproj", "BusinessLogicLayer.Implement/"]
COPY ["BusinessLogicLayer.Enum/SetupLayer.Enum.csproj", "BusinessLogicLayer.Enum/"]
COPY ["BusinessLogicLayer.Interface/BusinessLogicLayer.Interface.csproj", "BusinessLogicLayer.Interface/"]
COPY ["BusinessLogicLayer.Mapper/BusinessLogicLayer.Mapper.csproj", "BusinessLogicLayer.Mapper/"]
COPY ["BusinessLogicLayer.ModelView/BusinessLogicLayer.ModelView.csproj", "BusinessLogicLayer.ModelView/"]
COPY ["DataAccessLayer.Repository/DataAccessLayer.Repository.csproj", "DataAccessLayer.Repository/"]
COPY ["BusinessLogicLayer.Setting/SetupLayer.Setting.csproj", "BusinessLogicLayer.Setting/"]
COPY ["Shared/Utility.csproj", "Shared/"]
COPY ["DataAccessLayer.Implement/DataAccessLayer.Implement.csproj", "DataAccessLayer.Implement/"]
COPY ["DataAccessLayer.Interface/DataAccessLayer.Interface.csproj", "DataAccessLayer.Interface/"]
RUN dotnet restore "./SpotifyPool/SpotifyPool.csproj"
COPY . .
WORKDIR "/src/SpotifyPool"
RUN dotnet build "./SpotifyPool.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish phase
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SpotifyPool.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final phase to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SpotifyPool.dll"]