import { IVariableMethod, PointWGS84 } from "./types";
import config from 'config';
import winston from 'winston';
import  "./../variable-methods/index";

// Configuration file parser
// __________________________

type MethodListItem = {
    Id: string
    FriendlyName: string
    Description: string
    Implementation: string
    Options: any
}

type VariableListItem = {
    Id: string
    FriendlyName: string
    Description: string
    Methods: MethodListItem[]
}

let parseVariableConfiguration = 
    (variables:any[]) : VariableListItem[] =>
        variables.map((x:any) => {
            let keys = Object.keys(x);
            let name = keys[0];
            let v = x[name];
            let methods =
                v.methods.map((m:any) => {
                    let keys = Object.keys(m);
                    let name = keys[0]
                    let v = m[name]
                    return {
                        Id: name,
                        FriendlyName: v.name,
                        Description: v.description,
                        Implementation: v.implementation,
                        Options: v.options
                    }
                })
            return {
                Id: name,
                FriendlyName: v.name,
                Description: v.description,
                Methods: methods
            }
        });

// Helpers
// __________________________

const variablesWithDimensions = (variables:VariableListItem[]) => {
    let x =
        variables.map(v => {
            const methods = 
                v.Methods.map(m => {
                    const imp = variableMethods.find(vm => m.Implementation == vm.name.replace("VariableMethod",""));
                    if (imp == undefined) { return null; }
                    const method = new imp(m.Options);
                    return {
                        Id: m.Id,
                        Name: m.FriendlyName,
                        Time: method.temporalDimension(),
                        Space: method.spatialDimension()
                    };
                });
            return {
                Id: v.Id,
                Name: v.FriendlyName,
                Description: v.Description,
                Methods: methods
            }
        });
    return x;
}


// Load variables and config
// __________________________

const variables = parseVariableConfiguration(config.get("variables"));
const variableMethods = IVariableMethod.getImplementations();
const friendlyVariables = variablesWithDimensions(variables);

variableMethods.map(v => { winston.info("Loaded method: " + v.name) })
variables.map(v => { winston.info("Loaded variable: " + v.FriendlyName) })
friendlyVariables.map(v => winston.info("Loaded dimensions for: " + v.Name))

export function listVariables () {
    return friendlyVariables;
}
