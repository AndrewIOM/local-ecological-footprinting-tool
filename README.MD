EcoSet
=====

A simple framework for serving geo-temporal datasets. The framework was built to the requirements of the Oxford Long-Term Ecology Group. 

## Requirements

* Docker
* .NET Core SDK 3.0

## Project Structure

The EcoSet project comprises a 'geotemporal engine' that slices and analyses datasets, and a web interface.

### EcoSet.WebUI

A .NET Core 3 razor class library that contains pre-defined functionality required when serving geotemporal data products. 

## Applications

The Oxford Long-Term Ecology Lab uses EcoSet to serve two different data products. The Local Ecological Footprint Tool (LEFT) and MarineLEFT both use EcoSet. 

### MarineLEFT

To run MarineLEFT:
- cd apps/left-marine
- docker-compose -f docker-compose.yml -f docker-compose.development.yml build
- docker-compose -f docker-compose.yml -f docker-compose.development.yml up -d
- cd ../../src/gbif/gbifdb
- docker-compose up -d

MarineLEFT will then be running on localhost:5001
