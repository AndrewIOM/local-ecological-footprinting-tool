import { Request, Response } from 'express';
import { MapRequest } from './components/types';
import { SpatialPlot } from './components/spatial-plot';

// Route actions

export const makeMap = async (req: Request, res: Response) => {
    try {
        const mapRequest: MapRequest = req.body;
        let plot = new SpatialPlot(mapRequest.extent);
        plot.setup();
        plot.addGraticule();
        plot.addRasterLayer(mapRequest.data, null, mapRequest.palette, mapRequest.maskValues, mapRequest.categorical);
        plot.addBox(mapRequest.extent.north,mapRequest.extent.south,mapRequest.extent.east,mapRequest.extent.west);
        const svg = plot.getSvg();
        res.status(200).send(svg);
    } catch (error) {
        res.status(500).send('An error occurred creating a map output');
    }
}