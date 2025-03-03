# Use the ASP.NET Core image for the runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use SDK image for build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy files from context to container
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

# Final phase to run the application
FROM base AS final
WORKDIR /app

# Install Libgdiplus to support System.Drawing.common
RUN apk add --no-cache libgdiplus

# Install the library needed to run onnxruntime
RUN apk add --no-cache libstdc++ libc6-compat wget tar

# Download and extract onnxruntime from github
RUN wget -O /tmp/onnxruntime.tar.gz https://github.com/microsoft/onnxruntime/releases/download/v1.15.0/onnxruntime-linux-x64-1.15.0.tgz && \
    mkdir -p /usr/local/lib/onnxruntime && \
    tar -xzf /tmp/onnxruntime.tar.gz -C /usr/local/lib/onnxruntime --strip-components=1 && \
    rm /tmp/onnxruntime.tar.gz

# Create SYMLINK so that the application can be found libibinxruntime.so
# RUN ln -s /usr/local/lib/onnxruntime/lib/libonnxruntime.so /usr/lib/libonnxruntime.so

# Set environmental variables for the application to find onnxruntime
ENV LD_LIBRARY_PATH="/usr/local/lib/onnxruntime:$LD_LIBRARY_PATH"

# Create folders if not exist for the model
RUN mkdir -p /var/data/TrainingModel

# Copy model ONNX into TrainingModel folder
COPY ["Back-end/SpotifyPool/TrainingModel/audio_features_model.onnx", "/var/data/TrainingModel/"]

# Copy the published files to the container
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "SpotifyPool.dll"]
