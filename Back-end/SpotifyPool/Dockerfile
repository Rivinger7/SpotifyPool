# Use Alpine-based ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# Install dependencies for System.Drawing.Common & ONNX Runtime
RUN apk add --no-cache libgdiplus libc6-compat libstdc++ icu gcompat

# Use Alpine SDK for build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Back-end/SpotifyPool/SpotifyPool.csproj", "SpotifyPool/"]
COPY ["Back-end/BusinessLogicLayer.DependencyInjection/BusinessLogicLayer.DependencyInjection.csproj", "BusinessLogicLayer.DependencyInjection/"]
COPY ["Back-end/BusinessLogicLayer.Implement/BusinessLogicLayer.Implement.csproj", "BusinessLogicLayer.Implement/"]
COPY ["Back-end/SetupLayer.Enum/SetupLayer.Enum.csproj", "SetupLayer.Enum/"]
COPY ["Back-end/BusinessLogicLayer.Interface/BusinessLogicLayer.Interface.csproj", "BusinessLogicLayer.Interface/"]
COPY ["Back-end/BusinessLogicLayer.Mapper/BusinessLogicLayer.Mapper.csproj", "BusinessLogicLayer.Mapper/"]
COPY ["Back-end/BusinessLogicLayer.ModelView/BusinessLogicLayer.ModelView.csproj", "BusinessLogicLayer.ModelView/"]
COPY ["Back-end/DataAccessLayer.Repository/DataAccessLayer.Repository.csproj", "DataAccessLayer.Repository/"]
COPY ["Back-end/SetupLayer.Setting/SetupLayer.Setting.csproj", "SetupLayer.Setting/"]
COPY ["Back-end/Commons/Utility.csproj", "Commons/"]
COPY ["Back-end/DataAccessLayer.Implement/DataAccessLayer.Implement.csproj", "DataAccessLayer.Implement/"]
COPY ["Back-end/DataAccessLayer.Interface/DataAccessLayer.Interface.csproj", "DataAccessLayer.Interface/"]

# Restore dependencies
RUN dotnet restore "SpotifyPool/SpotifyPool.csproj"

# Copy source code
COPY Back-end/ .

# Build the application
WORKDIR "/src/SpotifyPool"
RUN dotnet build "SpotifyPool.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish phase
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SpotifyPool.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime phase
FROM base AS final
WORKDIR /app

# Copy model ONNX into TrainingModel folder
COPY ["Back-end/SpotifyPool/TrainingModel/audio_features_model.onnx", "/app"]

# Set environmental variables for ONNX Runtime
ENV LD_LIBRARY_PATH="/usr/local/lib/onnxruntime:$LD_LIBRARY_PATH"

# Copy published files from build stage
COPY --from=publish /app/publish .

# Set the entrypoint
ENTRYPOINT ["dotnet", "SpotifyPool.dll"]
