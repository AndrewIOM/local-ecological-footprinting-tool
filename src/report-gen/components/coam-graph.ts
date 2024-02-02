import d3 from "d3";
import _ from "underscore";

export const drawCoamGraph = (coamSvgId:string, data, margins) => {

    let m;
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
    
    const svg = d3.select("#" + coamSvgId),
        margin = m,
        width = +svg.attr("width") - margin.left - margin.right,
        height = +svg.attr("height") - margin.top - margin.bottom;

    const y = d3.scaleBand().rangeRound([margin.top, height]),
        x = d3.scaleLinear().rangeRound([0, width]);

    const clamp = function (n) {
        if (n > 4) return 4;
        if (n < -4) return -4;
        return n;
    }

    const g = svg.append("g")
        .attr("transform", "translate(" + margin.left + "," + margin.top + ")");

    //x.domain([0, d3.max(data, function(d) { return d.value; })]);
    x.domain([-5, 5]);
    y.domain(data.map((d) => d.name));

    g.append("g")
        .attr("class", "axis axis--x")
        .attr("transform", "translate(0," + height + ")")
        .call(
            d3.axisBottom(x)
            .tickValues([-5, 0, 5])
            .tickFormat((d) => {
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
        .attr("x1", (d) => x(clamp(d.value - d.sd)))
        .attr("y1", (d) => y(d.name) + margin.top)
        .attr("x2", (d) => x(clamp(d.value + d.sd)))
        .attr("y2", (d) => y(d.name) + margin.top)

    g.append("g")
        .selectAll('.median')
        .data(data)
        .enter()
        .append('circle')
        .attr('class', 'median')
        .attr('cx', (d) => x(clamp(d.value)))
        .attr('cy', (d) => y(d.name) + margin.top)
        .attr('r', 5);

    g.append('g')
        .selectAll('.center-max')
        .data(data)
        .enter()
        .append('line')
        .attr('class', 'center-max')
        .attr("x1", (d) => x(clamp(d.value - d.sd)))
        .attr("y1", (d) => y(d.name) + margin.top + 5)
        .attr("x2", (d) => x(clamp(d.value - d.sd)))
        .attr("y2", (d) => y(d.name) + margin.top - 5)

    g.append('g')
        .selectAll('.center-min')
        .data(data)
        .enter()
        .append('line')
        .attr('class', 'center-min')
        .attr("x1", (d) => x(clamp(d.value + d.sd)))
        .attr("y1", (d) => y(d.name) + margin.top + 5)
        .attr("x2", (d) => x(clamp(d.value + d.sd)))
        .attr("y2", (d) => y(d.name) + margin.top - 5);

};