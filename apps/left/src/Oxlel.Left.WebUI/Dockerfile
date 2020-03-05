FROM mcr.microsoft.com/dotnet/core/sdk:3.1-bionic AS build

ENV ASPNETCORE_ENVIRONMENT=Development

RUN mkdir -p /cache
RUN mkdir -p /persistence

# Setup DinkToPDF dependencies
RUN apt update
RUN apt-get install -y libssl1.0-dev
RUN apt install -y libgdiplus
RUN ln -s /usr/lib/libgdiplus.so /lib/x86_64-linux-gnu/libgdiplus.so
RUN apt-get install -y --no-install-recommends zlib1g fontconfig libfreetype6 libx11-6 libxext6 libxrender1 wget gdebi
RUN wget https://github.com/wkhtmltopdf/wkhtmltopdf/releases/download/0.12.5/wkhtmltox_0.12.5-1.bionic_amd64.deb
RUN gdebi --n wkhtmltox_0.12.5-1.bionic_amd64.deb
RUN ln -s /usr/local/lib/libwkhtmltox.so /usr/lib/libwkhtmltox.so

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

WORKDIR /build/apps/left-marine/src/Oxlel.LeftMarine.WebUI/bin/Release/netcoreapp3.1/publish
ENTRYPOINT [ "dotnet", "Oxlel.LeftMarine.WebUI.dll" ]