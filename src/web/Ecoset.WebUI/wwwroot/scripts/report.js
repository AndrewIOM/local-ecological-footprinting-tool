var colors = function (s) {
    return s.match(/.{6}/g).map(function (x) {
        return "#" + x;
    });
};
var ramp = function (range) {
    var n = range.length;
    return function (t) {
        return range[Math.max(0, Math.min(n - 1, Math.floor(t * n)))];
    };
};
var getColor = ramp(colors("44015444025645045745055946075a46085c460a5d460b5e470d60470e6147106347116447136548146748166848176948186a481a6c481b6d481c6e481d6f481f70482071482173482374482475482576482677482878482979472a7a472c7a472d7b472e7c472f7d46307e46327e46337f463480453581453781453882443983443a83443b84433d84433e85423f854240864241864142874144874045884046883f47883f48893e49893e4a893e4c8a3d4d8a3d4e8a3c4f8a3c508b3b518b3b528b3a538b3a548c39558c39568c38588c38598c375a8c375b8d365c8d365d8d355e8d355f8d34608d34618d33628d33638d32648e32658e31668e31678e31688e30698e306a8e2f6b8e2f6c8e2e6d8e2e6e8e2e6f8e2d708e2d718e2c718e2c728e2c738e2b748e2b758e2a768e2a778e2a788e29798e297a8e297b8e287c8e287d8e277e8e277f8e27808e26818e26828e26828e25838e25848e25858e24868e24878e23888e23898e238a8d228b8d228c8d228d8d218e8d218f8d21908d21918c20928c20928c20938c1f948c1f958b1f968b1f978b1f988b1f998a1f9a8a1e9b8a1e9c891e9d891f9e891f9f881fa0881fa1881fa1871fa28720a38620a48621a58521a68522a78522a88423a98324aa8325ab8225ac8226ad8127ad8128ae8029af7f2ab07f2cb17e2db27d2eb37c2fb47c31b57b32b67a34b67935b77937b87838b9773aba763bbb753dbc743fbc7340bd7242be7144bf7046c06f48c16e4ac16d4cc26c4ec36b50c46a52c56954c56856c66758c7655ac8645cc8635ec96260ca6063cb5f65cb5e67cc5c69cd5b6ccd5a6ece5870cf5773d05675d05477d1537ad1517cd2507fd34e81d34d84d44b86d54989d5488bd6468ed64590d74393d74195d84098d83e9bd93c9dd93ba0da39a2da37a5db36a8db34aadc32addc30b0dd2fb2dd2db5de2bb8de29bade28bddf26c0df25c2df23c5e021c8e020cae11fcde11dd0e11cd2e21bd5e21ad8e219dae319dde318dfe318e2e418e5e419e7e419eae51aece51befe51cf1e51df4e61ef6e620f8e621fbe723fde725"))

var decimalPlaces = function (n) {
    function hasFraction(n) {
      return Math.abs(Math.round(n) - n) > 1e-10;
    }
    var decimalCount = 0;
    while (hasFraction(n * (Math.pow(10,decimalCount))) && isFinite(Math.Pow(10,decimalCount))) {
        decimalCount++;
    }
    return decimalCount;
  }  

// Class representing a spatial map that can hold raster or vector data layers
var SpatialPlot = function (id, extent) {
    var self = this;
    self.id = id;
    self.svgSize = 650;
    self.mapSize = 600;
    self.graticuleBuffer = self.svgSize - self.mapSize; //Only buffer on one side
    self.paletteUrl = null;
    self.north = extent.north;
    self.south = extent.south;
    self.east = extent.east;
    self.west = extent.west;
    self.svg = null;
    self.innerMap = null;
    self.projection = null;

    self.setup = function () {
        var svgElem = document.createElementNS("http://www.w3.org/2000/svg", "svg");
        svgElem.setAttribute("id", self.id);
        svgElem.setAttribute("width", self.svgSize);
        svgElem.setAttribute("height", self.svgSize);
        document.getElementById(self.id + "-map").appendChild(svgElem);
        self.svg = d3.select("#" + self.id);

        self.svg.append("rect")
            .attr("class", "spatial-extent")
            .attr("width", self.mapSize)
            .attr("height", self.mapSize)
            .attr("x", self.graticuleBuffer)
            .attr("y", 0);

        self.innerMap = self.svg.append("svg")
            .attr("width", self.mapSize)
            .attr("height", self.mapSize)
            .attr("x", self.graticuleBuffer)
            .attr("y", 0);

        var extentGeoJson = {
            "type": "Feature",
            "geometry": {
                "type": "Polygon",
                "coordinates": [
                    [
                        [self.west, self.south],
                        [self.west, self.north],
                        [self.east, self.north],
                        [self.east, self.south],
                        [self.west, self.south]
                    ]
                ]
            },
            "properties": {}
        };

        self.projection = d3.geoMercator()
            .fitSize([self.mapSize, self.mapSize], extentGeoJson);
    }

    self.addBox = function (north, south, east, west) {
        var focusBoxProjTl = self.projection([west, north]);
        var focusBoxProjBr = self.projection([east, south]);

        self.svg.append("rect")
            .attr("class", "focus-area-box")
            .attr("width", focusBoxProjBr[0] - focusBoxProjTl[0])
            .attr("height", focusBoxProjBr[1] - focusBoxProjTl[1])
            .attr("x", focusBoxProjTl[0] + self.graticuleBuffer)
            .attr("y", focusBoxProjTl[1] + 0);
    }

    self.addWireframe = function () {
        var path = d3.geoPath().projection(self.projection);
        d3.json("/geojson/world50.json", function (error, world) {
            self.innerMap
                .insert("path", ".land")
                .datum(topojson.feature(world, world.objects.countries))
                .attr("class", "wireframe")
                .attr("d", path);
        });
    }

    self.addGraticule = function () {
        var graticuleBuffer = self.graticuleBuffer;
        var extentBuffer = 0;
        var bl_x = -0,
            bl_y = self.mapSize + extentBuffer,
            tr_x = self.mapSize + extentBuffer,
            tr_y = -extentBuffer;

        var projectedGraticuleBoundsBl = self.projection.invert([bl_x, bl_y]);
        var projectedGraticuleBoundsTr = self.projection.invert([tr_x, tr_y]);

        var stepx = Math.round((self.east - self.west) / 4.0 * 100) / 100;
        var stepy = Math.round((self.north - self.south) / 4.0 * 100) / 100;
        var graticule = d3.geoGraticule()
            .step([stepx, stepy])
            .extent([projectedGraticuleBoundsBl, projectedGraticuleBoundsTr]);

        function to2dp(num) {
            return Math.round(num * 100) / 100;
        }

        self.svg.selectAll('text')
            .data(graticule.lines())
            .enter().append("text")
            .text(function (d) {
                var c = d.coordinates;

                if ((c[0][0] == c[1][0])) {
                    return to2dp(c[0][0]);
                } else if (c[0][1] == c[1][1]) {
                    return to2dp(c[0][1]);
                }
            })
            .attr("class", "axis-label")
            .attr("style", function (d) {
                var c = d.coordinates;
                return (c[0][1] == c[1][1]) ? "text-anchor: end" : "text-anchor: middle";
            })
            .attr("dx", function (d) {
                var c = d.coordinates;
                return (c[0][1] == c[1][1]) ? graticuleBuffer + extentBuffer - 10 : graticuleBuffer + extentBuffer;
            })
            .attr("dy", function (d) {
                var c = d.coordinates;
                return (c[0][1] == c[1][1]) ? extentBuffer + 4 : extentBuffer + 15;
            })
            .attr('transform', function (d) {
                return ('translate(' + self.projection(d.coordinates[0]) + ')');
            });

        var tickLength = 5;
        self.svg.selectAll("line")
            .data(graticule.lines())
            .enter().append("line")
            .attr("class", "graticule-tick")
            .attr("x1", function (d) {
                var c = d.coordinates;
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return graticuleBuffer + extentBuffer + self.projection(c[0])[0] - tickLength;
                } else {
                    // x-axis
                    return graticuleBuffer + extentBuffer + self.projection(c[0])[0];
                }
            })
            .attr("y1", function (d) {
                var c = d.coordinates;
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return extentBuffer + self.projection(c[0])[1];
                } else {
                    // x-axis
                    return extentBuffer + self.projection(c[0])[1];
                }
            })
            .attr("x2", function (d) {
                var c = d.coordinates;
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return graticuleBuffer + extentBuffer + self.projection(c[0])[0];
                } else {
                    // x-axis
                    return graticuleBuffer + extentBuffer + self.projection(c[0])[0];
                }
            })
            .attr("y2", function (d) {
                var c = d.coordinates;
                if (c[0][1] == c[1][1]) {
                    // y-axis
                    return extentBuffer + self.projection(c[0])[1];
                } else {
                    // x-axis
                    return extentBuffer + self.projection(c[0])[1] + tickLength;
                }
            });
    }

    self.addImageLayer = function (imageUrl) {
        self.svg.append("svg:image")
            .attr('x', self.graticuleBuffer)
            .attr('y', 0)
            .attr('width', self.mapSize)
            .attr('height', self.mapSize)
            .attr('preserveAspectRatio', 'none')
            .attr("xlink:href", imageUrl);
    }

    self.addRasterLayer = function (data, noDataValue, paletteUrl, maskValues, isCategorical) {
        self.paletteUrl = paletteUrl;
        self.noDataValue = noDataValue;

        if (self.paletteUrl == "") {
            var allData_dataOnly = _.chain(data.DataCube)
                .flatten()
                .filter(function (d) {
                    return d != self.noDataValue && !isNaN(d) && d != null;                    
                }).value();
            var max = _.max(allData_dataOnly);
            var min = _.min(allData_dataOnly);
            var valueRange = max - min;
            var tickSize = 1000.00;
            var arr_points = [];
            for (var i = 1; i <= tickSize; i++) {
                if (valueRange == 0) {
                    arr_points.push((tickSize / 2.0) / tickSize);
                } else {
                    arr_points.push(i / tickSize);
                }
            };
            var arr_values = arr_points.map(getColor);
            var domain = _.map(arr_points, function (x) {
                return (x * valueRange) + min;
            });
            var scale = null;
            if (arr_points.length == 1) { scale = (function(n) { return d3.color(arr_values[0]).toString(); }) } 
            else {
                scale = d3.scaleLinear()
                .domain(domain)
                .range(arr_values);
            }
            self.drawRasterImage(data, scale);
            self.drawContinuousKey(min, max, maskValues);
            self.drawScalebar();
        } else {
            // Discrete scale
            d3.json(self.paletteUrl, function (rawPalette) {
                var includedCats = _.union.apply(_, data.DataCube);
                var legendCats =
                    _.filter(rawPalette, function (pal) {
                        var result = _.find(includedCats, function (val) {
                            return (val == pal.value);
                        });
                        return result != undefined;
                    });
                var domain = _.map(legendCats, function (c) {
                    return c.value;
                });
                var range = _.map(legendCats, function (d) {
                    return "rgb(" + d.R + "," + d.G + "," + d.B + ")";
                });
                var scale = d3.scaleOrdinal()
                    .domain(domain)
                    .range(range)
                    .unknown('rgb(177,177,177)');
                self.drawRasterImage(data, scale);
                if(isCategorical) {
                    self.drawDiscreteKey(legendCats);
                } else {
                    self.drawContinuousKey(data.Stats.Min, data.Stats.Max, maskValues);
                }
                self.drawScalebar();
            });
        }
    }

    self.drawScalebar = function() {                
        var scaleBar = d3.geoScaleBar()
            .projection(this.projection)
            .size([self.mapSize, self.mapSize])
            .left(0.05)
            .top(0.05)
            .label("Kilometres");
        this.innerMap
            .append("g")
            .call(scaleBar);
    }

    self.drawRasterImage = function (data, scale) {
        var canvasRaster = d3.select("#report-sections").append("canvas")
            .attr("width", self.mapSize)
            .attr("height", self.mapSize)
            .attr("id", id + "_overlay")
            .attr("style", "display:none");

        var contextRaster = canvasRaster.node().getContext("2d");

        var dataWidth = data.DataCube[0].length;
        var dataHeight = data.DataCube.length;
        var xScale = (self.east - self.west) / dataWidth;
        var yScale = (self.north - self.south) / dataHeight;

        var projectedSize = self.projection([self.east, self.south]);

        var outputWidth = Math.round(projectedSize[0]);
        var outputHeight = Math.round(projectedSize[1]);

        var outId = contextRaster.createImageData(outputWidth, outputHeight);
        var outData = outId.data;
        var pos = 0;

        for (var j = 0; j < outputHeight; j++) {
            for (var i = 0; i < outputWidth; i++) {
                var coord = self.projection.invert([i, j]);
                var ix = Math.floor((coord[0] - self.west) / xScale);
                var iy = dataHeight - 1 - Math.floor((coord[1] - self.south) / yScale);

                var rgbString = null;
                var alpha = null;
                if (ix >= 0 && ix < dataWidth && iy >= 0 && iy < dataHeight) {
                    var value = data.DataCube[iy][ix];
                    if (value === self.noDataValue || isNaN(value)) {
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

                var rgb = rgbString.substring(4, rgbString.length - 1).replace(/ /g, '').split(',');

                outData[pos] = rgb[0];
                outData[pos + 1] = rgb[1];
                outData[pos + 2] = rgb[2];
                outData[pos + 3] = alpha;
                pos = pos + 4;
            }
        }
        contextRaster.putImageData(outId, 0, 0);
        self.innerMap.append("svg:image")
            .attr('x', 0)
            .attr('y', 0)
            .attr('width', self.mapSize)
            .attr('height', self.mapSize)
            .attr('preserveAspectRatio', 'none')
            .attr("xlink:href", document.getElementById(id + "_overlay").toDataURL());
    }

    self.drawContinuousKey = function (min, max, maskValues) {
        var border = 50;
        var tickSize = 200.00;
        var arr_points = [];
        for (var i = 1; i <= tickSize; i++) {
            if (max - min == 0) {
                arr_points.push((tickSize / 2.0) / tickSize);
            } else {
                arr_points.push(i / tickSize);
            }
        };
        var arr_values = arr_points.map(getColor);
        var cs_def = {
            positions: arr_points,
            colors: arr_values
        };
        var scaleWidth = 275;
        var canvasColorScale = d3.select("#" + id + "-scale").append("canvas")
            .attr("width", scaleWidth + border * 2)
            .attr("height", 20);

        var contextColorScale = canvasColorScale.node().getContext("2d");
        var gradient = contextColorScale.createLinearGradient(0, 0, scaleWidth, 1);

        for (var i = 0; i < cs_def.colors.length; ++i) {
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

    self.drawDiscreteKey = function (legendCats) {
        d3.json(self.paletteUrl, function (rawPalette) {
            var legend = d3.select("#" + self.id + "-scale")
                .append("ul")
                .attr('class', 'category-legend')
            var keys = legend.selectAll('li.key')
                .data(legendCats);
            keys.enter().append('li')
                .attr('class', 'key')
                .style('border-left-color', function (d) {
                    return "rgb(" + d.R + "," + d.G + "," + d.B + ")";
                })
                .text(function (d) {
                    return d.class;
                });
        });
    }

    self.addPointLayer = function (occurrenceData, colourDictionary) {
        console.log(colourDictionary);
        function colourLookup(name) {
            var found = colourDictionary[name];
            if (found == null) return 'black';
            return found;
        }

        var canvas = d3.select("#" + id + "-map")
            .append("canvas")
            .attr("width", self.mapSize)
            .attr("height", self.mapSize)
            .attr("style", "display:none")
            .attr("id", id + "_pointdata");
        var context = canvas.node().getContext("2d");

        function drawCircle(occurrence) {
            var r = 4;
            context.strokeStyle = colourLookup(occurrence.taxon);
            context.lineWidth = 1;
            context.beginPath();
            context.arc(self.projection([occurrence.lon, occurrence.lat])[0], self.projection([occurrence.lon, occurrence.lat])[1], r, 0, 2 * Math.PI, true);
            context.stroke();
            context.closePath();
        }

        occurrenceData.forEach(function (o) {
            drawCircle(o);
        })

        self.svg.append("svg:image")
            .attr('x', self.graticuleBuffer)
            .attr('y', 0)
            .attr('width', self.mapSize)
            .attr('height', self.mapSize)
            .attr('preserveAspectRatio', 'none')
            .attr("xlink:href", document.getElementById(id + "_pointdata").toDataURL());
        self.drawScalebar();
    }
};

var drawGlobalPinpointMap = function (svgId, n, s, e, w) {

    var width = 900;
    var height = 400;

    var svg = d3.select("#" + svgId);
    var g = svg.append("g");

    svg.attr("viewBox", "0 0 " + width + " " + height)
        .attr("preserveAspectRatio", "xMinYMin");

    svg.attr("width", width);
    svg.attr("height", height);

    var projection = d3.geoEquirectangular()
        .scale(100);

    var path = d3.geoPath().projection(projection);

    d3.json("/geojson/world110.json", function (error, world) {
        g.insert("path", ".land")
            .datum(topojson.feature(world, world.objects.countries))
            .attr("class", "land")
            .attr("d", path)
            .attr('fill', '#E5E3DF');

        g.append("path")
            .datum(topojson.mesh(world, world.objects.countries, function (a, b) {
                return a !== b;
            }))
            .attr("class", "mesh")
            .attr("d", path)
            .attr('fill', 'none')
            .attr('stroke', 'white')
            .attr('stroke-width', '0.5px');

        // Draw four lines that form square around analysis location
        // North
        svg.append("line")
            .attr("x1", function (d) {
                return projection([-180, n])[0];
            })
            .attr("y1", function (d) {
                return projection([-180, n])[1];
            })
            .attr("x2", function (d) {
                return projection([180, n])[0];
            })
            .attr("y2", function (d) {
                return projection([180, n])[1];
            })
            .style("stroke", "red");

        // South
        svg.append("line")
            .attr("x1", function (d) {
                return projection([-180, s])[0];
            })
            .attr("y1", function (d) {
                return projection([-180, s])[1];
            })
            .attr("x2", function (d) {
                return projection([180, s])[0];
            })
            .attr("y2", function (d) {
                return projection([180, s])[1];
            })
            .style("stroke", "red");

        // East
        svg.append("line")
            .attr("x1", function (d) {
                return projection([e, -90])[0];
            })
            .attr("y1", function (d) {
                return projection([e, -90])[1];
            })
            .attr("x2", function (d) {
                return projection([e, 90])[0];
            })
            .attr("y2", function (d) {
                return projection([e, 90])[1];
            })
            .style("stroke", "red");

        // South
        svg.append("line")
            .attr("x1", function (d) {
                return projection([w, -90])[0];
            })
            .attr("y1", function (d) {
                return projection([w, -90])[1];
            })
            .attr("x2", function (d) {
                return projection([w, 90])[0];
            })
            .attr("y2", function (d) {
                return projection([w, 90])[1];
            })
            .style("stroke", "red");

    });
}

var drawCoamGraph = function (coamSvgId, data, margins) {

    var m;
    if (margins) {
        m = margins;
    } else {
        m = {
            top: 20,
            right: 100,
            bottom: 30,
            left: 160
        };
    };
    
    var svg = d3.select("#" + coamSvgId),
        margin = m,
        width = +svg.attr("width") - margin.left - margin.right,
        height = +svg.attr("height") - margin.top - margin.bottom;

    var y = d3.scaleBand().rangeRound([margin.top, height]),
        x = d3.scaleLinear().rangeRound([0, width]);

    var clamp = function (n) {
        if (n > 4) return 4;
        if (n < -4) return -4;
        return n;
    }

    var g = svg.append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    //x.domain([0, d3.max(data, function(d) { return d.value; })]);
    x.domain([-5, 5]);
    y.domain(data.map(function (d) {
        return d.name;
    }));

    g.append("g")
        .attr("class", "axis axis--x")
        .attr("transform", "translate(0," + height + ")")
        .call(
            d3.axisBottom(x)
            .tickValues([-5, 0, 5])
            .tickFormat(function (d) {
                if (d == -5) return 'Lower than reference region';
                if (d == 0) return "Equal to reference region";
                if (d == 5) return "Higher than reference region";
                return d;
            })
        );

    g.append("g")
        .attr("class", "axis axis--y")
        .call(d3.axisLeft(y))
        .append("text")
        .attr("transform", "rotate(-90)")
        .attr("y", 6)
        .attr("dy", "0.71em")
        .attr("text-anchor", "end")
        .text("Frequency");

    g.append('line')
        .attr('class', 'dashed-line')
        .attr("stroke-dasharray", "5, 5")
        .attr("x1", x(0))
        .attr("y1", margin.top)
        .attr("x2", x(0))
        .attr("y2", margin.top + height);

    g.append('g')
        .selectAll('.center')
        .data(data)
        .enter()
        .append('line')
        .attr('class', 'center')
        .attr("x1", function (d) {
            return x(clamp(d.value - d.sd));
        })
        .attr("y1", function (d) {
            return y(d.name) + margin.top;
        })
        .attr("x2", function (d) {
            return x(clamp(d.value + d.sd));
        })
        .attr("y2", function (d) {
            return y(d.name) + margin.top;
        })

    g.append("g")
        .selectAll('.median')
        .data(data)
        .enter()
        .append('circle')
        .attr('class', 'median')
        .attr('cx', function (d) {
            return x(clamp(d.value));
        })
        .attr('cy', function (d) {
            return y(d.name) + margin.top;
        })
        .attr('r', 5);

    g.append('g')
        .selectAll('.center-max')
        .data(data)
        .enter()
        .append('line')
        .attr('class', 'center-max')
        .attr("x1", function (d) {
            return x(clamp(d.value - d.sd));
        })
        .attr("y1", function (d) {
            return y(d.name) + margin.top + 5;
        })
        .attr("x2", function (d) {
            return x(clamp(d.value - d.sd));
        })
        .attr("y2", function (d) {
            return y(d.name) + margin.top - 5;
        })

    g.append('g')
        .selectAll('.center-min')
        .data(data)
        .enter()
        .append('line')
        .attr('class', 'center-min')
        .attr("x1", function (d) {
            return x(clamp(d.value + d.sd));
        })
        .attr("y1", function (d) {
            return y(d.name) + margin.top + 5;
        })
        .attr("x2", function (d) {
            return x(clamp(d.value + d.sd));
        })
        .attr("y2", function (d) {
            return y(d.name) + margin.top - 5;
        });

};

var populateDataSources = function (occurrenceData) {
    var list = document.getElementById('data-source-list');
    var institutions = _(occurrenceData)
        .chain()
        .map(function (d) {
            return d.title;
        })
        .uniq()
        .map(function (d) {
            var li = document.createElement('li');
            li.className = "list-group-item col-xs-4";
            li.innerHTML = d;
            list.appendChild(li);
        });
};