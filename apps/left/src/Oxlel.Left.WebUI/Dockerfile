FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build

ENV ASPNETCORE_ENVIRONMENT=Development

RUN mkdir -p /cache
RUN mkdir -p /persistence

# LibMan installs front-end dependencies e.g. jquery, bootstrap.
RUN dotnet tool install -g Microsoft.Web.LibraryManager.CLI
ENV PATH="$PATH:/root/.dotnet/tools"

# Prepare ecoset webui library
WORKDIR /build/src/web
COPY src/web /build/src/web

WORKDIR /build/src/web/Ecoset.WebUI/
RUN libman restore

# Copy LEFT source
WORKDIR /build/apps/left/src/Oxlel.Left.WebUI
COPY apps/left/src/Oxlel.Left.WebUI /build/apps/left/src/Oxlel.Left.WebUI

ENV ASPNETCORE_URLS=http://+:80

RUN dotnet restore --disable-parallel
RUN dotnet publish -c Release

VOLUME /cache
VOLUME /persistence

ENTRYPOINT dotnet bin/Release/netcoreapp3.0/publish/Oxlel.Left.WebUI.dll