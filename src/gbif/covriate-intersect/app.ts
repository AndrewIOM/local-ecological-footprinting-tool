import config from 'config';
import fs from 'fs';
import gdal, { Point } from 'gdal';
import mysql from 'mysql';

class Database {
    connection: mysql.Connection;
    constructor( config: string | mysql.ConnectionConfig ) {
        this.connection = mysql.createConnection( config );
    }
    query( sql: string, args: any ) {
        return new Promise( ( resolve, reject ) => {
            this.connection.query( sql, args, ( err, rows ) => {
                if ( err )
                    return reject( err );
                resolve( rows );
            } );
        } );
    }
    close() {
        return new Promise( ( resolve, reject ) => {
            this.connection.end( err => {
                if ( err )
                    return reject( err );
                resolve();
            } );
        } );
    }
}

type Covariate = {
    Name: string
    Dataset: gdal.Dataset
}

// Configuration settings
const mysqlHost = config.get<string>('mysql.host');
const mysqlDb = config.get<string>('mysql.db');
const mysqlUser = config.get<string>('mysql.user');
const mysqlPassword = config.get<string>('mysql.password');
const covariateDirectory = config.get<string>('covariates.dir');
const ecoregionShapefile = config.get<string>('ecoregions.shapefile');

const openCovriates = (dir:string) : Covariate[] => {
    const dirCont = fs.readdirSync(dir);
    const files = dirCont.filter(e => e.match(/.*\.(tif)/ig));
    return files.map(f => { return { Name: f, Dataset: gdal.open(f, "r") } })
}



// Script
// const covariates = openCovriates(covariateDirectory);

// const gdalPoints = [
//     new Point(12,10)
// ] // List of WGS84 points

// covariates.map(c => {

//     const transformedPoint = gdalPoints[0].transformTo(c.Dataset.srs);

//     c.Dataset.bands.get(0)
// })


const db = new Database({
    host: mysqlHost,
    database: mysqlDb,
    user: mysqlUser,
    password: mysqlPassword
});

type Ecoregion = {
    Name: string,
    WKT: string
}

// Intersect ecoregions
const loadEcoregions = () : Ecoregion[] => {
    const ecoregions = gdal.open(ecoregionShapefile, "r");
    return ecoregions.layers.get("marineecoregions_forspdensity").features.map(f => {
        const name = f.fields.get("DATA_VALUE");
        const wkt : string = f.getGeometry().toWKT() as unknown as string;
        return { Name: name, WKT: wkt };
    })
}



const countEcoregion = async (ecoregion:Ecoregion, conn:Database) => {

    const query = 
        `select taxonomicgroup as taxon, count(distinct gbif_species) as species, count(gbif_species) as count \
        from ${config.get("mysql.gbif_table")} m \
        left join ${config.get("mysql.gbif_coord_table")} c \
        on m.gbif_gbifid=c.gbif_gbifid \
        where gbif_species<>'' and \
        ST_Contains(ST_GeomFromText('${ecoregion.WKT}'), coordinate) \
        group by taxonomicgroup;`;
    
    console.log("Running query for: " + ecoregion.Name);
    return await conn.query(query, undefined);
}

const regions = loadEcoregions();
console.log("Loaded " + regions.length + " ecoregion shapes.");

const outFile = config.get<string>("ecoregions.outputdir") + "/out.txt";
console.log("Running queries...");

let chain = Promise.resolve();
for (let region of regions.reverse()) {
    chain = chain.then(() => countEcoregion(region, db)
        .then(r => fs.appendFileSync(outFile, r)));
} 