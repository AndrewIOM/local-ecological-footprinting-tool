import d3 from "d3";
import topojson from "topojson-client";
import _ from "underscore";

export const drawGlobalPinpointMap = (svgId, n, s, e, w) => {

    const width = 900;
    const height = 400;

    const svg = d3.select("#" + svgId);
    const g = svg.append("g");

    svg.attr("viewBox", "0 0 " + width + " " + height)
        .attr("preserveAspectRatio", "xMinYMin");

    svg.attr("width", width);
    svg.attr("height", height);

    const projection = d3.geoEquirectangular()
        .scale(100);

    const path = d3.geoPath().projection(projection);

    d3.json("/geojson/world110.json", (error, world) => {
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
            .attr("x1", (d) => projection([-180, n])[0])
            .attr("y1", (d) => projection([-180, n])[1])
            .attr("x2", (d) => projection([180, n])[0])
            .attr("y2", (d) => projection([180, n])[1])
            .style("stroke", "red");

        // South
        svg.append("line")
            .attr("x1", (d) => projection([-180, s])[0])
            .attr("y1", (d) => projection([-180, s])[1])
            .attr("x2", (d) => projection([180, s])[0])
            .attr("y2", (d) => projection([180, s])[1])
            .style("stroke", "red");

        // East
        svg.append("line")
            .attr("x1", (d) => projection([e, -90])[0])
            .attr("y1", (d) => projection([e, -90])[1])
            .attr("x2", (d) => projection([e, 90])[0])
            .attr("y2", (d) => projection([e, 90])[1])
            .style("stroke", "red");

        // South
        svg.append("line")
            .attr("x1", (d) => projection([w, -90])[0])
            .attr("y1", (d) => projection([w, -90])[1])
            .attr("x2", (d) => projection([w, 90])[0])
            .attr("y2", (d) => projection([w, 90])[1])
            .style("stroke", "red");

    });
}
