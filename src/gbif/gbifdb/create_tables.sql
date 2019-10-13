CREATE TABLE IF NOT EXISTS master
(gbif_gbifid BIGINT PRIMARY KEY,
gbif_eventdate VARCHAR(255),
gbif_kingdom VARCHAR(255),
gbif_phylum VARCHAR(255),
gbif_class VARCHAR(255),
gbif_order VARCHAR(255),
gbif_family VARCHAR(255),
gbif_genus VARCHAR(255),
gbif_infraspecificepithet VARCHAR(255),
gbif_species VARCHAR(255),
gbif_locality LONGTEXT,
gbif_publishingorgkey VARCHAR(255),
gbif_taxonkey VARCHAR(255),
gbif_lastinterpreted VARCHAR(255),
gbif_institutioncode VARCHAR(255),
gbif_coordinateuncertaintyinmeters VARCHAR(255),
gbif_decimallatitude FLOAT NOT NULL,
gbif_decimallongitude FLOAT NOT NULL,
taxonomicgroup VARCHAR(255)
);

CREATE TABLE IF NOT EXISTS gbif_coordinate
(gbif_gbifid BIGINT PRIMARY KEY,
coordinate POINT NOT NULL SRID 0,
SPATIAL INDEX idx_coord (coordinate)
);

CREATE TABLE IF NOT EXISTS gbif_org_lookup
(gbif_publishingorgkey VARCHAR(255) PRIMARY KEY,
title VARCHAR(255)
);
