FROM ubuntu:xenial

ENV ROOTDIR /usr/local/

RUN apt-get update
RUN apt-get install --yes curl
RUN apt-get install --yes sudo
RUN apt-get install --yes build-essential
RUN apt install software-properties-common --yes
RUN apt-get install --yes locales

# Set the locale
RUN locale-gen en_US.UTF-8
ENV LANG en_US.UTF-8
ENV LANGUAGE en_US:en
ENV LC_ALL en_US.UTF-8

# Install GDAL
RUN add-apt-repository ppa:ubuntugis/ubuntugis-unstable
RUN apt-get update
RUN apt-get install --yes gdal-bin python-gdal
RUN apt-get install --yes python-pip

# Set the working directory to /app
WORKDIR /img_files

# Copy the src directory contents into the container at /app
COPY ./img_files /img_files
COPY ./vol_files /vol_files

# Install any needed packages specified in requirements.txt
RUN pip install --trusted-host pypi.python.org -r requirements.txt

# Tell Docker about the volumes
RUN mkdir /download
VOLUME [ "/download" ]
WORKDIR /vol_files

#CMD [ "python", "update.py" ]