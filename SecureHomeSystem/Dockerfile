# Multi-stage Dockerfile for the SecureHomeSystem worker. Comments describe the
# release-day story so we remember why each instruction exists when narrating the
# stack. See https://aka.ms/customizecontainer for Visual Studio optimisations.

# Runtime image used both locally and in Compose. Bundles the Docker CLI so the
# worker can talk to the host daemon through /var/run/docker.sock.
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

# Install Docker CLI so the worker can query the host daemon when containerised.
RUN apt-get update \
    && apt-get install -y --no-install-recommends ca-certificates curl gnupg \
    && install -d /etc/apt/keyrings \
    && curl -fsSL https://download.docker.com/linux/debian/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg \
    && echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/debian $(. /etc/os-release && echo $VERSION_CODENAME) stable" > /etc/apt/sources.list.d/docker.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends docker-ce-cli \
    && rm -rf /var/lib/apt/lists/*


# Build stage – restores dependencies and compiles the worker for repeatable CI builds.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SecureHomeSystem/SecureHomeSystem.csproj", "SecureHomeSystem/"]
RUN dotnet restore "./SecureHomeSystem/SecureHomeSystem.csproj"
COPY . .
WORKDIR "/src/SecureHomeSystem"
RUN dotnet build "./SecureHomeSystem.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage – trims the worker to ready-to-run binaries for the final image.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SecureHomeSystem.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image copied into the Compose stack and production-like deployments.
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SecureHomeSystem.dll"]
