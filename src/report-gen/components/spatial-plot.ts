import d3 from "d3";
import topojson from "topojson-client";
import _ from "underscore";
import { getColor } from "./palette";
import JSDOM from "jsdom";
import { BoundingBox, PaletteItem, RasterData, TableData } from "./types";

/// Class representing a spatial map that can hold raster or vector data layers.
/// Uses JsDOM to make an SVG element for export.
export class SpatialPlot {

    svgSize: number;
    mapSize: number;
    palette: PaletteItem[] | undefined;
    noDataValue: number | undefined;
    extent: BoundingBox;
    innerMap: d3.Selection<SVGSVGElement, unknown, null, undefined> | undefined;
    projection: d3.GeoProjection | undefined;
    
    dom: JSDOM.JSDOM;
    svgContainer: d3.Selection<HTMLDivElement, unknown, null, undefined> | undefined;
    svg: d3.Selection<SVGSVGElement, unknown, null, undefined> | undefined;
    svgLegend: undefined;

    graticuleBuffer() {
        return this.svgSize - this.mapSize;
    }

    constructor(extent: BoundingBox) {
        this.dom = new JSDOM.JSDOM(`<!DOCTYPE html><body></body>`);
        this.svgSize = 650;
        this.mapSize = 600;
        this.extent = extent;
    }

    getSvg() {
        if (this.svg) {
            const result = {
                Map: this.svg.html(),
                Legend: undefined
            };
            return result;
        }
        return undefined;
    }

    /// Create a new map SVG element
    setup() {

        let body = d3.select(this.dom.window.document.querySelector("body"));
        this.svgContainer = body.append('div');
        this.svg = this.svgContainer.append('svg');
        this.svg
            .attr('width', this.svgSize)
            .attr('height', this.svgSize)
            .attr('xmlns', 'http://www.w3.org/2000/svg');

        this.svg.append("rect")
            .attr("class", "spatial-extent")
            .attr("width", this.mapSize)
            .attr("height", this.mapSize)
            .attr("x", this.graticuleBuffer())
            .attr("y", 0);

        this.innerMap = this.svg.append("svg")
            .attr("width", this.mapSize)
            .attr("height", this.mapSize)
            .attr("x", this.graticuleBuffer())
            .attr("y", 0);

        const extentGeoJson : d3.ExtendedFeature = {
            "type": "Feature",
            "geometry": {
                "type": "Polygon",
                "coordinates": [
                    [
                        [this.extent.west, this.extent.south],
                        [this.extent.west, this.extent.north],
                        [this.extent.east, this.extent.north],
                        [this.extent.east, this.extent.south],
                        [this.extent.west, this.extent.south]
                    ]
                ]
            },
            "properties": {}
        };

        this.projection = d3.geoMercator()
            .fitSize([this.mapSize, this.mapSize], extentGeoJson);
    }

    /// Add a rectangle to the map, styled with the
    /// focus-area-box class.
    addBox (north: number, south: number, east: number, west: number) {
        if (this.projection && this.svg) {
            const focusBoxProjTl = this.projection([west, north]);
            const focusBoxProjBr = this.projection([east, south]);
            if (focusBoxProjBr != undefined && focusBoxProjTl != undefined) {
                this.svg.append("rect")
                    .attr("class", "focus-area-box")
                    .attr("width", focusBoxProjBr[0] - focusBoxProjTl[0])
                    .attr("height", focusBoxProjBr[1] - focusBoxProjTl[1])
                    .attr("x", focusBoxProjTl[0] + this.graticuleBuffer())
                    .attr("y", focusBoxProjTl[1] + 0);
            }
        }
    }

    /// 
    addWireframe() {
        const worldJsonUrl = "/geojson/world50.json";
        if (this.projection) {
            const path = d3.geoPath().projection(this.projection);
            d3.json(worldJsonUrl).then((world:any) => {
                if (this.innerMap) {
                    this.innerMap
                        .insert("path", ".land")
                        .datum(topojson.feature(world, world.objects.countries))
                        .attr("class", "wireframe")
                        .attr("d", path);
                }
            }).catch(err => {
                console.log(err);
            });
        }
    }

    /// Adds a graticule
    addGraticule() {
        if (!this.projection || !this.svg) {
            return;
        }
        const graticuleBuffer = this.graticuleBuffer;
        const extentBuffer = 0;
        const bl_x = -0,
            bl_y = this.mapSize + extentBuffer,
            tr_x = this.mapSize + extentBuffer,
            tr_y = -extentBuffer;

        if (!this.projection.invert) {
            console.log("Projection unable to invert");
            return;
        }
        const projectedGraticuleBoundsBl = this.projection.invert([bl_x, bl_y]);
        const projectedGraticuleBoundsTr = this.projection.invert([tr_x, tr_y]);
        if (projectedGraticuleBoundsBl == null || projectedGraticuleBoundsTr == null) {
            console.log("Projected graticule bounds were null");
            return;
        }

        const stepx = Math.round((this.extent.east - this.extent.west) / 4.0 * 100) / 100;
        const stepy = Math.round((this.extent.north - this.extent.south) / 4.0 * 100) / 100;
        const graticule = d3.geoGraticule()
            .step([stepx, stepy])
            .extent([projectedGraticuleBoundsBl, projectedGraticuleBoundsTr]);

        const to2dp = (num: number) => {
            return Math.round(num * 100) / 100;
        }

        this.svg.selectAll('text')
            .data(graticule.lines())
            .enter().append("text")
            .text((d) => {
                const c = d.coordinates;
                if ((c[0][0] == c[1][0])) {
                    return to2dp(c[0][0]);
                } else if (c[0][1] == c[1][1]) {
                    return to2dp(c[0][1]);
                }
                return "";
            })
            .attr("class", "axis-label")
            .attr("style", (d) => {
                    var c = d.coordinates;
                    return (c[0][1] == c[1][1]) ? "text-anchor: end" : "text-anchor: middle";
                })
            .attr("dx", (d) => {
                    var c = d.coordinates;
                    return (c[0][1] == c[1][1]) ? this.graticuleBuffer() + extentBuffer - 10 : this.graticuleBuffer() + extentBuffer;
                })
            .attr("dy", (d) => {
                    var c = d.coordinates;
                    return (c[0][1] == c[1][1]) ? extentBuffer + 4 : extentBuffer + 15;
                })
            .attr('transform', (d) => {
                if (!this.projection) {
                    console.log("No projection set for graticule translate");
                    return "";
                } else {
                    return ('translate(' + this.projection([d.coordinates[0][0], d.coordinates[0][1]]) + ')')
                }
            });

        var tickLength = 5;
        this.svg.selectAll("line")
            .data(graticule.lines())
            .enter().append("line")
            .attr("class", "graticule-tick")
            .attr("x1", (d) => {
                const c = d.coordinates;
                if (!this.projection) {
                    return null;
                }
                const projected = this.projection([c[0][0], c[0][1]]);
                if (projected == null) {
                    return null;
                }
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return this.graticuleBuffer() + extentBuffer + projected[0] - tickLength;
                } else {
                    // x-axis
                    return this.graticuleBuffer() + extentBuffer + projected[0];
                }
            })
            .attr("y1", (d) => {
                const c = d.coordinates;
                if (!this.projection) {
                    return null;
                }
                const projected = this.projection([c[0][0], c[0][1]]);
                if (projected == null) {
                    return null;
                }
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return extentBuffer + projected[1];
                } else {
                    // x-axis
                    return extentBuffer + projected[1];
                }
            })
            .attr("x2", (d) => {
                const c = d.coordinates;
                if (!this.projection) {
                    return null;
                }
                const projected = this.projection([c[0][0], c[0][1]]);
                if (projected == null) {
                    return null;
                }
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return this.graticuleBuffer() + extentBuffer + projected[0];
                } else {
                    // x-axis
                    return this.graticuleBuffer() + extentBuffer + projected[0];
                }
            })
            .attr("y2", (d) => {
                const c = d.coordinates;
                if (!this.projection) {
                    return null;
                }
                const projected = this.projection([c[0][0], c[0][1]]);
                if (projected == null) {
                    return null;
                }
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return extentBuffer + projected[1];
                } else {
                    // x-axis
                    return extentBuffer + projected[1] + tickLength;
                }
            });
    }

    /// Adds a layer of occurrence point data to a map.
    addPointLayer(occurrenceData: TableData, colourDictionary: Map<string, string>) {
        
        if (!this.projection || !this.svgContainer || !this.svg) {
            return;
        }

        const colourLookup = (name: string) => {
            var found = colourDictionary.get(name);
            if (found == null) return 'black';
            return found;
        }

        const canvas = this.svgContainer
            .append("canvas")
            .attr("width", this.mapSize)
            .attr("height", this.mapSize)
            .attr("style", "display:none");
        const canvasNode = canvas.node();
        if (canvasNode == null) return null;
        const context = canvasNode.getContext("2d");
        if (context == null) return null;

        const drawCircle = (occurrence: Map<string, string | number>) => {
            const r = 4;
            const taxon = occurrence.get("taxon");
            if (_.isString(taxon)) {
                context.strokeStyle = colourLookup(taxon ?? "black");
            }
            context.lineWidth = 1;
            context.beginPath();

            const lon = occurrence.get("lon");
            const lat = occurrence.get("lat");
            if (lon && lat && this.projection) {
                if(_.isNumber(lon) && _.isNumber(lat)) {
                    const projected = this.projection([lon, lat]);
                    if (projected != null) {
                        context.arc(projected[0], projected[1], r, 0, 2 * Math.PI, true);
                    }
                }
            }
            context.stroke();
            context.closePath();
        }

        occurrenceData.Rows.forEach(drawCircle);

        this.svg.append("svg:image")
            .attr('x', this.graticuleBuffer)
            .attr('y', 0)
            .attr('width', this.mapSize)
            .attr('height', this.mapSize)
            .attr('preserveAspectRatio', 'none');
        this.drawScalebar();
    }

    addRasterLayer(data: RasterData, noDataValue: number | undefined, discretePalette: PaletteItem[] | undefined, maskValues: boolean, isCategorical: boolean): void {
        this.palette = discretePalette;
        this.noDataValue = noDataValue;

        if (this.palette == undefined) {
            const allData_dataOnly = _.chain(data.DataCube)
                .flatten()
                .filter( (d) => {
                    return d != this.noDataValue && !isNaN(d) && d != null;                    
                }).value();
            const max = _.max(allData_dataOnly);
            const min = _.min(allData_dataOnly);
            const valueRange = max - min;
            const tickSize = 1000.00;
            const arr_points = [];
            for (let i = 1; i <= tickSize; i++) {
                if (valueRange == 0) {
                    arr_points.push((tickSize / 2.0) / tickSize);
                } else {
                    arr_points.push(i / tickSize);
                }
            };
            const arr_values = arr_points.map(getColor);
            const domain = _.map(arr_points, function (x) {
                return (x * valueRange) + min;
            });
            let scale = null;
            if (arr_points.length == 1) { 
                scale = (() => { 
                    const c = d3.color(arr_values[0]);
                    if (c == null) return "";
                    return c.toString(); }) } 
            else {
                scale = d3.scaleLinear()
                .domain(domain)
                .range(arr_values);
            }
            this.drawRasterImage(data, scale);
            this.drawContinuousKey(min, max, maskValues);
            this.drawScalebar();
        } else {
            // Discrete scale
            if (!discretePalette) {
                console.log("No palette was specified to draw a discrete scale");
                return;
            }
            const includedCats = _.union.apply(_, data.DataCube);
            const legendCats =
                _.filter(discretePalette, (pal) => {
                    const result = _.find(includedCats, (val) => {
                        return (val == pal.value);
                    });
                    return result != undefined;
                });
            const domain = _.map(legendCats, (c) => {
                return c.value;
            });
            const range = _.map(legendCats, (d) => {
                return "rgb(" + d.R + "," + d.G + "," + d.B + ")";
            });
            const scale = d3.scaleOrdinal()
                .domain(domain)
                .range(range)
                .unknown('rgb(177,177,177)');
            this.drawRasterImage(data, scale);
            if(isCategorical) {
                this.drawDiscreteKey(legendCats);
            } else {
                this.drawContinuousKey(data.Stats.Minimum, data.Stats.Maximum, maskValues);
            }
            this.drawScalebar();
        }
    }

    drawRasterImage(data: RasterData, scale) {

        if (!this.svgContainer || !this.projection || !this.innerMap) {
            return;
        }

        const canvas = this.svgContainer
            .append("canvas")
            .attr("width", this.mapSize)
            .attr("height", this.mapSize)
            .attr("id", "overlay")
            .attr("style", "display:none");
        const canvasNode = canvas.node();
        if (canvasNode == null) return null;
        const context = canvasNode.getContext("2d");
        if (context == null) return null;

        const dataWidth = data.DataCube[0].length;
        const dataHeight = data.DataCube.length;
        const xScale = (this.extent.east - this.extent.west) / dataWidth;
        const yScale = (this.extent.north - this.extent.south) / dataHeight;

        const projectedSize = this.projection([this.extent.east, this.extent.south]);
        if (projectedSize == null) {
            console.log("Could not project raster data");
            return;
        }

        const outputWidth = Math.round(projectedSize[0]);
        const outputHeight = Math.round(projectedSize[1]);

        const outId = context.createImageData(outputWidth, outputHeight);
        const outData = outId.data;
        let pos = 0;

        for (let j = 0; j < outputHeight; j++) {
            for (let i = 0; i < outputWidth; i++) {
                const coord = this.projection.invert([i, j]);
                const ix = Math.floor((coord[0] - this.extent.west) / xScale);
                const iy = dataHeight - 1 - Math.floor((coord[1] - this.extent.south) / yScale);

                let rgbString = "";
                let alpha = null;
                if (ix >= 0 && ix < dataWidth && iy >= 0 && iy < dataHeight) {
                    const value = data.DataCube[iy][ix];
                    if (value === this.noDataValue || isNaN(value)) {
                        rgbString = "rgb(177,177,177)";
                        alpha = 255;
                    } else {
                        rgbString = scale(value);
                        alpha = 255;
                    }
                } else {
                    rgbString = "rgb(100,100,200)";
                    alpha = 0;
                }

                const rgb = rgbString.substring(4, rgbString.length - 1).replace(/ /g, '').split(',');

                outData[pos] = Number(rgb[0]);
                outData[pos + 1] = Number(rgb[1]);
                outData[pos + 2] = Number(rgb[2]);
                outData[pos + 3] = alpha;
                pos = pos + 4;
            }
        }
        context.putImageData(outId, 0, 0);
        this.innerMap.append("svg:image")
            .attr('x', 0)
            .attr('y', 0)
            .attr('width', this.mapSize)
            .attr('height', this.mapSize)
            .attr('preserveAspectRatio', 'none')
            .attr("xlink:href", this.dom.window.getElementById("overlay").toDataURL());
    }

    drawScalebar() {                
        const scaleBar = geoScaleBar()
            .projection(this.projection)
            .size([this.mapSize, this.mapSize])
            .left(0.05)
            .top(0.05)
            .label("Kilometres");
        this.innerMap
            .append("g")
            .call(scaleBar);
    }

    drawContinuousKey(min, max, maskValues) {
        const border = 50;
        const tickSize = 200.00;
        const arr_points = [];
        for (const i = 1; i <= tickSize; i++) {
            if (max - min == 0) {
                arr_points.push((tickSize / 2.0) / tickSize);
            } else {
                arr_points.push(i / tickSize);
            }
        };
        const arr_values = arr_points.map(getColor);
        const cs_def = {
            positions: arr_points,
            colors: arr_values
        };
        const scaleWidth = 275;
        const canvasColorScale = d3.select("#" + id + "-scale").append("canvas")
            .attr("width", scaleWidth + border * 2)
            .attr("height", 20);

        const contextColorScale = canvasColorScale.node().getContext("2d");
        const gradient = contextColorScale.createLinearGradient(0, 0, scaleWidth, 1);

        for (let i = 0; i < cs_def.colors.length; ++i) {
            gradient.addColorStop(cs_def.positions[i], cs_def.colors[i]);
        }
        contextColorScale.fillStyle = gradient;
        contextColorScale.fillRect(border, 0, scaleWidth, 20);

        contextColorScale.fillStyle = "black";
        contextColorScale.textAlign = "center";
        contextColorScale.font = '16px Consolas';

        if (maskValues) {
            contextColorScale.fillText("Low", border / 2, 15);
            contextColorScale.fillText("High", border + scaleWidth + border / 2, 15);
        } else {
            if (decimalPlaces(min) > 0) {
                contextColorScale.fillText(min.toFixed(3), border / 2, 15);
            } else {
                contextColorScale.fillText(min.toString(), border / 2, 15);
            }
            if (decimalPlaces(max) > 0) {
                contextColorScale.fillText(max.toFixed(3), border + scaleWidth + border / 2, 15);
            } else {
                contextColorScale.fillText(max.toString(), border + scaleWidth + border / 2, 15);
            }
        }
    }

    drawDiscreteKey(legendCats) {
        d3.json(this.paletteUrl, (rawPalette) => {
            const legend = d3.select("#" + this.id + "-scale")
                .append("ul")
                .attr('class', 'category-legend');
            const keys = legend.selectAll('li.key')
                .data(legendCats);
            keys.enter().append('li')
                .attr('class', 'key')
                .style('border-left-color', (d) => "rgb(" + d.R + "," + d.G + "," + d.B + ")")
                .text((d) => { return d.class; });
        });
    }

}