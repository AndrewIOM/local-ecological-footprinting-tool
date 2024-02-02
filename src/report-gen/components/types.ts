export interface BoundingBox {
    north: number;
    south: number;
    east: number;
    west: number;
}

export interface PaletteItem {
    value: number;
    class: string;
    R: number;
    G: number;
    B: number;
}

export interface Statistics {
    Minimum: number;
    Maximum: number;
    Mean: number;
    StDev: number;
}

export interface RasterData {
    DataCube: number[][]
    Columns: number;
    Rows: number;
    Stats: Statistics;
}

export interface TableData {
    Rows: Map<string,string | number>[]
}

export interface MapRequest {
    extent: BoundingBox;
    data: RasterData | TableData;
    palette: PaletteItem[] | undefined;
    categorical: boolean;
    maskValues: boolean;
    showLandWireframe: boolean;
}