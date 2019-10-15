#FROM node:8.0
FROM ubuntu:xenial

ENV ROOTDIR /usr/local/
ENV GDAL_VERSION 2.2.0

# Setup Directories
RUN mkdir -p /data
RUN mkdir -p /output

# Install GDAL 1.*
# RUN apt-get install --yes gdal-bin
# RUN apt-get install --yes python-gdal

# Install Node.js
RUN apt-get update
RUN apt-get install --yes curl
RUN apt-get install --yes sudo
RUN curl --silent --location https://deb.nodesource.com/setup_10.x | sudo bash -
RUN apt-get install --yes nodejs
RUN apt-get install --yes build-essential

# Install GDAL
WORKDIR $ROOTDIR/
# ADD http://download.osgeo.org/gdal/${GDAL_VERSION}/gdal-${GDAL_VERSION}.tar.gz $ROOTDIR/src/
# ADD ./lib/gdal-2.2.0.tar.gz $ROOTDIR/src/
# RUN apt-get update -y && apt-get install -y \
#     software-properties-common \
#     python-software-properties \
#     build-essential \
#     python-dev \
#     python-numpy \
#     libspatialite-dev \
#     sqlite3 \
#     libpq-dev \
#     libcurl4-gnutls-dev \
#     libproj-dev \
#     libxml2-dev \
#     libgeos-dev \
#     libnetcdf-dev \
#     libpoppler-dev \
#     libspatialite-dev \
#     libhdf4-alt-dev \
#     libhdf5-serial-dev \
#     wget
# RUN cd src && cd gdal-${GDAL_VERSION} \
#     && ./configure --with-python --with-spatialite --with-pg --with-curl \
#     && make && make install && ldconfig \
#     && apt-get update -y \
#     && apt-get remove -y --purge build-essential wget \
#     && rm -Rf $ROOTDIR/src/*
# CMD gdalinfo --version && gdalinfo --formats && ogrinfo --formats
RUN apt-get install --yes software-properties-common
RUN add-apt-repository ppa:ubuntugis/ubuntugis-unstable
RUN apt-get update
RUN apt-get install --yes gdal-bin python-gdal

# Ecoset
# Create app directory
RUN mkdir -p /usr/src/app
WORKDIR /usr/src/app

# Bundle app source
ADD . /usr/src/app

# Install app dependencies
RUN npm install
WORKDIR /usr/src/app/api
RUN npm install
WORKDIR /usr/src/app/executables
RUN npm install

WORKDIR /usr/src/app

EXPOSE 5001:5002
VOLUME /data
VOLUME /output

CMD [ "npm", "start" ]