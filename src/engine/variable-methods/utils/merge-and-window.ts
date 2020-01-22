import fs from 'fs';
import winston from 'winston';
import { runCommand } from './run-command';
import { SlicedTime, SimpleDate } from "./../../api/types";

type TwoWayMap<A,B> = {
    Forward: (arg:A) => (B | undefined),
    Backward: (arg:B) => (A | undefined)
}

// Create a lookup between tile names and spatial location
const tileLookupTable : () => TwoWayMap<string,{}> = () => {
    const csv = fs.readFileSync(__dirname + "/tiles.csv");
    const lines = csv.toString().split("\n");
    const boundsFromName = new Map<string, {}>()
    const nameFromBounds = new Map<{}, string>()
    lines.forEach(line => {
        const data = line.split(',');
        const name = data[0];
        const coord = [ parseInt(data[1].trim()), 
                        parseInt(data[2].trim()), 
                        parseInt(data[3].trim()), 
                        parseInt(data[4].trim()) ]
        boundsFromName.set(name, coord);
        nameFromBounds.set(coord, name);
    });
    return {
        Forward: boundsFromName.get,
        Backward: nameFromBounds.get
    };
}

const listSubDirectories = (path:string) => {
    return fs.readdirSync(path).filter(file => {
        return fs.statSync(path + "/" + file).isDirectory()
    });
}

const isValidDate = (year:number, month:number, day:number) => {
    const tempDate = new Date(year, --month, day);
    return month === tempDate.getMonth();   
}

// Parses subdirectories as time slices based on format YYYY-MM-DD
const timeSlices = (tileDir:string) => {
    const timeSlices = new Map<SimpleDate,string>();
    const tileDirectories = listSubDirectories(tileDir);
    tileDirectories.forEach(subdir => {
        console.log("Subdir is " + subdir);
        let yr:number, m:number, d:number;
        const parts = subdir.split('-');
        if (parts.length > 3) return;
        if (!isNaN(Number(parts[0]))) {
            yr = Number(parts[0]);
        } else return;
        if (parts.length == 1) {
            timeSlices.set({ Year: yr, Month: null, Day: null }, tileDir+'/'+subdir);
        }
        if (!isNaN(Number(parts[1]))) {
            if (Number(parts[1]) > 0 && Number(parts[1]) < 13) { m = Number(parts[1]); }
            else return;
        } else return;
        if (parts.length == 2) {
            timeSlices.set({ Year: yr, Month: m, Day: null }, tileDir+'/'+subdir);
        }
        if (!isNaN(Number(parts[2]))) {
            d = Number(parts[2]);
            if (isValidDate(yr,m,d)) {
                timeSlices.set({ Year: yr, Month: m, Day: d }, tileDir+'/'+subdir);
            } 
        }
    });
    winston.debug("Time slices: " + JSON.stringify(timeSlices));
    return timeSlices;
}

// Exports
// ______________

export function getTimeSlices(tileDir:string) {
    return Array.from(timeSlices(tileDir).keys());
}