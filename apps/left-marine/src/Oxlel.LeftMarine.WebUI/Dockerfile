FROM microsoft/aspnetcore-build

ENV ASPNETCORE_ENVIRONMENT=Development

RUN mkdir -p /cache
RUN mkdir -p /persistence

# Install Node.js
RUN apt-get update
RUN apt-get install --yes curl
RUN apt-get install --yes sudo
RUN curl --silent --location https://deb.nodesource.com/setup_8.x | sudo bash -
RUN apt-get install --yes nodejs
RUN apt-get install --yes build-essential

# # Install PhantomJS
RUN apt-get install --yes chrpath libssl-dev libxft-dev
RUN apt-get install --yes libfreetype6 libfreetype6-dev libfontconfig1 libfontconfig1-dev
ADD https://bitbucket.org/ariya/phantomjs/downloads/phantomjs-2.1.1-linux-x86_64.tar.bz2 /usr/local/share/
RUN ln -sf /usr/local/share/phantomjs-2.1.1-linux-x86_64/bin/phantomjs /usr/local/bin

WORKDIR /app

COPY . .

ENTRYPOINT dotnet Oxlel.Left.WebUI.dll