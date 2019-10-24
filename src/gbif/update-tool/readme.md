# Requirements:

The mysql database currently requires `load data local` support. See: https://dba.stackexchange.com/questions/48751/enabling-load-data-local-infile-in-mysql

The update tool runs in a docker container.

# To run:

Change docker-compose.yml vol_files location if on windows to somewhere in the Users directory.
Change docker-compose.yml to have the required date range.

docker-compose build
docker-compose up