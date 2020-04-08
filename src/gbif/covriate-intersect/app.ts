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

type Ecoregion = {
    Name: string,
    WKT: string
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

const loadEcoregions = () : Ecoregion[] => {
    const ecoregions = gdal.open(ecoregionShapefile, "r");
    return ecoregions.layers.get("meow_shape").features.map(f => {
        const name = f.fields.get("ecoregion");
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
        ST_Contains(ST_SwapXY(ST_GeomFromText('${ecoregion.WKT}')), c.coordinate) \
        group by taxonomicgroup;`;
    
    console.log(query);
    console.log("Running query for: " + ecoregion.Name);
    return await conn.query(query, undefined);
}


// Script

const db = new Database({
    host: mysqlHost,
    database: mysqlDb,
    user: mysqlUser,
    password: mysqlPassword
});

const regions = loadEcoregions();
console.log("Loaded " + regions.length + " ecoregion shapes.");

const outFile = config.get<string>("ecoregions.outputdir") + "/out.txt";
console.log("Running queries...");

let chain = Promise.resolve();
for (let region of regions) {
    chain = chain.then(() => countEcoregion(region, db)
        .then(r => fs.appendFileSync(outFile, JSON.stringify(r))));
}

// // Script
// const covariates = openCovriates(covariateDirectory);

// const gdalPoints = [
//     new Point(12,10)
// ] // List of WGS84 points

// covariates.map(c => {
//     const point = gdalPoints[0];
//     const transform = new gdal.CoordinateTransformation(point.srs, c.Dataset);
//     const pixels = transform.transformPoint(point.x, point.y);
//     const v = c.Dataset.bands.get(0).pixels.get(pixels.x,pixels.y);
//     return [ c.Name, v ];
// })
