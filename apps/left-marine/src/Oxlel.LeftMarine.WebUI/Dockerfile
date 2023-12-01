FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy AS build

ENV ASPNETCORE_ENVIRONMENT=Development

RUN mkdir -p /cache
RUN mkdir -p /persistence

# Setup DinkToPDF dependencies
RUN apt update
RUN apt-get install -y libssl3
RUN apt install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so
RUN apt-get install -y --no-install-recommends zlib1g fontconfig libfreetype6 libx11-6 libxext6 libxrender1 wget gdebi
RUN wget https://github.com/wkhtmltopdf/packaging/releases/download/0.12.6.1-2/wkhtmltox_0.12.6.1-2.jammy_amd64.deb
RUN gdebi --n wkhtmltox_0.12.6.1-2.jammy_amd64.deb
RUN ln -s /usr/local/lib/libwkhtmltox.so /usr/lib/libwkhtmltox.so

# Prepare ecoset webui library
WORKDIR /build/src/web
COPY src/web /build/src/web

WORKDIR /build/src/web/Ecoset.WebUI/

# LibMan installs front-end dependencies e.g. jquery, bootstrap.
RUN dotnet new tool-manifest
RUN dotnet tool install Microsoft.Web.LibraryManager.Cli
RUN dotnet tool run libman restore

# Copy LEFT source
WORKDIR /build/apps/left-marine/src/Oxlel.LeftMarine.WebUI
COPY apps/left-marine/src/Oxlel.LeftMarine.WebUI /build/apps/left-marine/src/Oxlel.LeftMarine.WebUI

ENV ASPNETCORE_URLS=http://+:80

RUN dotnet restore --disable-parallel
RUN dotnet publish -c Release

VOLUME /cache
VOLUME /persistence

WORKDIR /build/apps/left-marine/src/Oxlel.LeftMarine.WebUI/bin/Release/net8.0/publish
ENTRYPOINT [ "dotnet", "Oxlel.LeftMarine.WebUI.dll" ]