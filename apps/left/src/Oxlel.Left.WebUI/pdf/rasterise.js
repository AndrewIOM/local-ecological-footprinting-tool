"use strict";
var page = require('webpage').create(),
    system = require('system'),
    address, output, size, pageWidth, pageHeight;

page.onConsoleMessage = function(msg, lineNum, sourceId) {
  console.log('CONSOLE: ' + msg + ' (from line #' + lineNum + ' in "' + sourceId + '")');
};

page.onError = function(msg, trace) {
  console.log('ERROR: ' + msg + ' trace: ' + trace);
};

function replaceCssWithComputedStyle(html) {
  return page.evaluate(function(html) {
    var host = document.createElement('div');
    host.setAttribute('style', 'display:none;'); // Silly hack, or PhantomJS will 'blank' the main document for some reason
    host.innerHTML = html;

    // Append to get styling of parent page
    document.body.appendChild(host);

    var elements = host.getElementsByTagName('*');
    // Iterate in reverse order (depth first) so that styles do not impact eachother
    for (var i = elements.length - 1; i >= 0; i--) {
      elements[i].setAttribute('style', window.getComputedStyle(elements[i], null).cssText);
    }

    // Remove from parent page again, so we're clean
    document.body.removeChild(host);
    return host.innerHTML;
  }, html);
}

if (system.args.length < 3 || system.args.length > 5) {
    console.log('Usage: rasterize.js URL filename [paperwidth*paperheight|paperformat] [zoom]');
    console.log('  paper (pdf output) examples: "5in*7.5in", "10cm*20cm", "A4", "Letter"');
    console.log('  image (png/jpg output) examples: "1920px" entire page, window width 1920px');
    console.log('                                   "800px*600px" window, clipped to 800x600');
    phantom.exit(1);
} else {
    address = system.args[1];
    output = system.args[2];
    page.viewportSize = { width: 1170, height: 967 };
    page.zoomFactor = 1;
    if (system.args.length > 3 && system.args[2].substr(-4) === ".pdf") {
        page.paperSize = {
            format: 'A4',
            orientation: 'landscape',
            margin: {
                top: "1cm",
                bottom: '1cm',
                left: '1cm',
                right: '1cm'
            },
            header: {
                height: "1cm",
                contents: phantom.callback(function(pageNum, numPages) {

                    var today = new Date();
                    var dd = today.getDate();
                    var mm = today.getMonth()+1; //January is 0!
                    var yyyy = today.getFullYear();
                    if(dd<10) {
                        dd='0'+dd
                    } 
                    if(mm<10) {
                        mm='0'+mm
                    }
                    today = dd+'/'+mm+'/'+yyyy;
                    return replaceCssWithComputedStyle("<div><span style='float:right; font-size: 0.5em;'><img src='/images/logo-black.png' style='height: 1em' /> Generated on " + today + "</span></div>");
                })
            },
            footer: {
                height: "1cm",
                contents: phantom.callback(function(pageNum, numPages) {
                    return replaceCssWithComputedStyle("<div><span style='float:center; font-size: 0.5em;'>Page " + pageNum + " of " + numPages + "</span></div>");
                })
            }
        }
    } else if (system.args.length > 3 && system.args[3].substr(-2) === "px") {
        size = system.args[3].split('*');
        if (size.length === 2) {
            var pageWidth = parseInt(size[0], 10),
                pageHeight = parseInt(size[1], 10);
            page.viewportSize = { width: pageWidth, height: pageHeight };
            page.clipRect = { top: 0, left: 0, width: pageWidth, height: pageHeight };
        } else {
            console.log("size:", system.args[3]);
            var pageWidth = parseInt(system.args[3], 10),
                pageHeight = parseInt(pageWidth * 3/4, 10); // it's as good an assumption as any
            console.log ("pageHeight:",pageHeight);
            page.viewportSize = { width: pageWidth, height: pageHeight };
        }
    }
    if (system.args.length > 4) {
        page.zoomFactor = system.args[4];
    }
    page.open(address, function (status) {
        if (status !== 'success') {
            console.log('Unable to load the address!');
            phantom.exit(1);
        } else {
            window.setTimeout(function () {
                page.render(output);
                phantom.exit();
            }, 60000);
        }
    });
}
