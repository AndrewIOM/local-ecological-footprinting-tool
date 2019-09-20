# Ecoset Scheduler

*Author: Philip Holland*

## Description

A standalone node application which acts as an interface between a front-end for the ecoset procedure, and the backend - a sequence of executables which together create a single output to pass back to the front-end.

## Configuration

Configuration is done in .json files within the "config" directory. "default.json" contains configuration information intended to be identical across all instances (development, production, etc.) A second file in the same directory, "local.json" can be used to overwrite these configuration settings. This file is ignored by git.

## Requirements

- NodeJS
- Python 2.7
- Redis
- GDAL
- Python GDAL Bindings
