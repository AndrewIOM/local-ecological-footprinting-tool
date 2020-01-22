import { IVariableMethod, TemporalDimension, SlicedTime } from "./../api/types"
import { getTimeSlices } from "./utils/merge-and-window";

@IVariableMethod.register
class IntersectTiffsVariableMethod {

    private config : any;

    constructor(conf:any) {
        this.config = conf;
    }

    computeToFile(options:any) {

    }

    spatialDimension() {
        let boundingBox = [
            { Latitude: 90, Longitude: -180 },
            { Latitude: -90, Longitude: -180 },
            { Latitude: -90, Longitude: 180 },
            { Latitude: 90, Longitude: 180 },
            { Latitude: 90, Longitude: -180 }
        ]
        return boundingBox;
    }

    temporalDimension() : TemporalDimension {
        let slices : SlicedTime = {
            kind: "timeSlice",
            slices: getTimeSlices(this.config.tiledir)
        }
        return (slices);
    }

    availableForDate() { return true; }

    availableForSpace() { return true; }
}

