import * as d3 from 'd3';

// Format decimal places

const decimalPlaces = function (n) {
    function hasFraction(n) {
      return Math.abs(Math.round(n) - n) > 1e-10;
    }
    var decimalCount = 0;
    while (hasFraction(n * (Math.pow(10,decimalCount))) && isFinite(Math.pow(10,decimalCount))) {
        decimalCount++;
    }
    return decimalCount;
  }
