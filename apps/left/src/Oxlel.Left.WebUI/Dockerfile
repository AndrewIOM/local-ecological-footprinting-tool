FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

RUN mkdir -p /cache
RUN mkdir -p /persistence

# Setup NodeJs + phantomjs
RUN apt-get update && \
    apt-get install -y wget && \
    apt-get install -y gnupg2 && \
    wget -qO- https://deb.nodesource.com/setup_10.x | bash - && \
    apt-get install -y build-essential nodejs phantomjs

# Prepare ecoset webui library
WORKDIR /build/src/web
COPY src/web /build/src/web

# Copy source
WORKDIR /build/apps/left-marine/src/Oxlel.Left.WebUI
COPY apps/left-marine/src/Oxlel.Left.WebUI /build/apps/left-marine/src/Oxlel.Left.WebUI

RUN dotnet restore
RUN dotnet publish -c Release

ENTRYPOINT dotnet bin/Release/netcoreapp3.0/publish/Oxlel.Left.WebUI.dll