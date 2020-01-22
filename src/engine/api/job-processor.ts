import { EcosetJobRequest } from "./types";
import { listVariables } from "./registry";

const variables = listVariables();

export function processJob(job:EcosetJobRequest) {
    return 2;
}

// for (var x = 0; x < variableMethods.length; x++) {
//     console.log("Loaded variable method: " + variableMethods[x].name);
//     // const panel = new variableMethods[x]();
//     // panel.computeToFile();
// }

// Determines available variables based on contraints.
// export function listVariables(time?: Date, space?: PointWGS84[]) {

//     let filtered = variableMethods;

//     if (time != null) {
//         filtered =
//             filtered.filter(v => {
//                 v.
//             })
//     }
// }