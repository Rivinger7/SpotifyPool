# Use the ASP.NET Core image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
#EXPOSE 8081

# Use Alpine SDK image for build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy files from context to container
COPY Back-end/*.csproj ./

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

# Final phase to run the application
FROM base AS final
WORKDIR /app

# Install Libgdiplus to support System.Drawing.common and OnnxRuntime
RUN apk add --no-cache libgdiplus libstdc++ \
    && rm -rf /var/cache/apk/*

# Copy model ONNX into TrainingModel folder
COPY ["Back-end/SpotifyPool/TrainingModel/audio_features_model.onnx", "/app"]

# Set environmental variables for the application to find onnxruntime
ENV LD_LIBRARY_PATH="/usr/local/lib/onnxruntime:$LD_LIBRARY_PATH"

# Copy the published files to the container
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SpotifyPool.dll"]
