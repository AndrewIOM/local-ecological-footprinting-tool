function initMapboxWithDraw(mapboxAccessToken, maxLat, maxLon) {

    mapboxgl.accessToken = mapboxAccessToken;
    var map = new mapboxgl.Map({
        container: 'map',
        style: 'mapbox://styles/mapbox/outdoors-v11',
        center: [20, 50.50],
        maxBounds: [
            [-180, -85],
            [180, 85]
        ],
        zoom: 2
    });
    map.interactive = false;

    map.addControl(new MapboxGeocoder({
        accessToken: mapboxAccessToken,
        mapboxgl: mapboxgl
    }), 'bottom-left');

    var nav = new mapboxgl.NavigationControl();
    map.addControl(nav, 'top-left');

    //Layer switching
    var layerList = document.getElementById('layer-menu');
    var inputs = layerList.getElementsByTagName('button');

    function switchLayer(layer) {
        $('#layer-menu').children('button').removeClass('active');
        var layerId = layer.target.value;
        map.setStyle('mapbox://styles/mapbox/' + layerId + '-v11');
        $(layer.target).addClass('active');
    }

    for (var i = 0; i < inputs.length; i++) {
        inputs[i].onclick = switchLayer;
    }

    // init variables
    var overpoint = 0; // 0 = none, 1 = tl, 2 = tr, 3 = bl, 4 = br, 5 = t, 6 = l, 7 = r, 8 = b
    var lastlat = 0;
    var lastlon = 0;
    var startlat = null;
    var startlon = null;
    var targetlat = 0;
    var targetlon = 0;
    var currentlat = 0;
    var currentlon = 0;
    var drawing = false;
    var drawingMode = false;
    var boxvisible = false;
    var lockhorizontal = false;
    var lockvertical = false;

    var minValidLat = 0.1,
        minValidLon = 0.1,
        maxValidLat = maxLat,
        maxValidLon = maxLon;
    var isValidSize = false;

    // removes a geojson source and corresponding layer from mapbox
    function removeSourceAndLayer(name) {
        if (map.getLayer(name)) {
            map.removeLayer(name);
        }
        if (map.getSource(name)) {
            map.removeSource(name);
        }
    }

    // adds a geojson source representing a circular point and corresponding layer to mapbox
    function addSourcePointAndLayer(name) {
        map.addSource(name, {
            "type": "geojson",
            "data": {
                "type": "Point",
                "coordinates": []
            }
        });
        map.addLayer({
            "id": name,
            "source": name,
            "type": "circle",
            "paint": {
                "circle-radius": 5,
                "circle-stroke-color": "#A93E71",
                "circle-stroke-width": 1,
                "circle-opacity": 0
            }
        });
    }

    // adds a geojson source representing a linestring and corresponding layer to mapbox
    function addSourceLineAndLayer(name) {
        map.addSource(name, {
            "type": "geojson",
            "data": {
                "type": "Feature",
                "properties": {},
                "geometry": {
                    "type": "Polygon",
                    "coordinates": [
                        [
                            []
                        ]
                    ]
                }
            }
        });
        map.addLayer({
            "id": name,
            "source": name,
            "type": "fill",
            "layout": {},
            "paint": {
                'fill-color': '#088',
                'fill-opacity': 0.7
            }
        });
    }

    // creates all layers required
    function initLayer() {
        addSourceLineAndLayer('line');
        addSourcePointAndLayer('tl_point');
        addSourcePointAndLayer('tr_point');
        addSourcePointAndLayer('bl_point');
        addSourcePointAndLayer('br_point');
        addSourcePointAndLayer('t_point');
        addSourcePointAndLayer('r_point');
        addSourcePointAndLayer('l_point');
        addSourcePointAndLayer('b_point');
    }

    function recreateHandles() {
        removeSourceAndLayer('tl_point');
        removeSourceAndLayer('tr_point');
        removeSourceAndLayer('bl_point');
        removeSourceAndLayer('br_point');
        removeSourceAndLayer('t_point');
        removeSourceAndLayer('r_point');
        removeSourceAndLayer('l_point');
        removeSourceAndLayer('b_point');
        addSourcePointAndLayer('tl_point');
        addSourcePointAndLayer('tr_point');
        addSourcePointAndLayer('bl_point');
        addSourcePointAndLayer('br_point');
        addSourcePointAndLayer('t_point');
        addSourcePointAndLayer('r_point');
        addSourcePointAndLayer('l_point');
        addSourcePointAndLayer('b_point');
    }

    var difference = function (a, b) {
        return Math.abs(a - b);
    }

    // draws the area box
    var lastDraw = 0;

    function drawRect(lonmin, latmin, lonmax, latmax, force) {
        if (Date.now() - lastDraw > 40 || force == true) {
            boxvisible = true;

            // Colour box red or green depending on if valid input.
            var diffLat = difference(latmin, latmax);
            var diffLon = difference(lonmin, lonmax);
            if (diffLat < minValidLat || diffLat > maxValidLat || diffLon < minValidLon || diffLon > maxValidLon) {
                isValidSize = false;
            } else {
                isValidSize = true;
            }
            var fillColour = isValidSize ? "green" : "red";

            map.setPaintProperty('line', 'fill-color', fillColour);
            map.getSource('line').setData({
                "type": "Feature",
                "properties": {},
                "geometry": {
                    "type": "Polygon",
                    "coordinates": [
                        [
                            [lonmin, latmax],
                            [lonmax, latmax],
                            [lonmax, latmin],
                            [lonmin, latmin],
                            [lonmin, latmax]
                        ]
                    ]
                }
            });
            map.getSource('t_point').setData({
                "type": "Point",
                "coordinates": [(lonmin + lonmax) / 2, latmax]
            });
            map.getSource('b_point').setData({
                "type": "Point",
                "coordinates": [(lonmin + lonmax) / 2, latmin]
            });
            var halflat = (latmin + latmax) / 2.0;
            map.getSource('l_point').setData({
                "type": "Point",
                "coordinates": [lonmin, halflat]
            });
            map.getSource('r_point').setData({
                "type": "Point",
                "coordinates": [lonmax, halflat]
            });
            map.getSource('tl_point').setData({
                "type": "Point",
                "coordinates": [lonmin, latmax]
            });
            map.getSource('bl_point').setData({
                "type": "Point",
                "coordinates": [lonmin, latmin]
            });
            map.getSource('tr_point').setData({
                "type": "Point",
                "coordinates": [lonmax, latmax]
            });
            map.getSource('br_point').setData({
                "type": "Point",
                "coordinates": [lonmax, latmin]
            });
            lastDraw = Date.now();
        }
    }

    // called after the map finishes loading
    map.on('load', function () {
        initLayer();

        function update() {
            if (drawing) {
                currentlat += (targetlat - currentlat) * 0.2;
                currentlon += (targetlon - currentlon) * 0.2;

                var minlon = Math.min(startlon, currentlon);
                var minlat = Math.min(startlat, currentlat);
                var maxlon = Math.max(startlon, currentlon);
                var maxlat = Math.max(startlat, currentlat);

                drawRect(minlon, maxlat, maxlon, minlat);
            }
        }

        $(".mapboxgl-canvas").mousedown(function (e) {
            if (drawingMode && !drawing) {
                e.stopPropagation();
                startlat = lastlat;
                startlon = lastlon;
                targetlat = lastlat;
                targetlon = lastlon;
                currentlat = lastlat;
                currentlon = lastlon;
                drawing = true;
                drawRect(startlon, startlat, startlon, startlat);
            }
            if (overpoint > 0) {
                $(".mapboxgl-canvas").css("border", "3px solid rgba(169, 62, 113, 1)");
                e.stopPropagation();
                var n = Math.max(targetlat, startlat);
                var e = Math.max(targetlon, startlon);
                var s = Math.min(targetlat, startlat);
                var w = Math.min(targetlon, startlon);

                var tllat = n;
                var tllon = w;

                var trlat = n;
                var trlon = e;

                var bllat = s;
                var bllon = w;

                var brlat = s;
                var brlon = e;

                lockhorizontal = false;
                lockvertical = false;

                switch (overpoint) {
                    case 5:
                        lockhorizontal = true;
                    case 1:
                        // tl
                        startlat = brlat;
                        startlon = brlon;
                        targetlat = currentlat = tllat;
                        targetlon = currentlon = tllon;
                        break;
                    case 7:
                        lockvertical = true;
                    case 2:
                        // tr
                        startlat = bllat;
                        startlon = bllon;
                        targetlat = currentlat = trlat;
                        targetlon = currentlon = trlon;
                        break;
                    case 6:
                        lockvertical = true;
                    case 3:
                        // bl
                        startlat = trlat;
                        startlon = trlon;
                        targetlat = currentlat = bllat;
                        targetlon = currentlon = bllon;
                        break;
                    case 8:
                        lockhorizontal = true;
                    case 4:
                        // br
                        startlat = tllat;
                        startlon = tllon;
                        targetlat = currentlat = brlat;
                        targetlon = currentlon = brlon;
                        break;
                }

                drawing = true;
                drawRect(startlon, startlat, targetlon, targetlat);
            }
        });

        function checkFeatures(point) {
            if (boxvisible && !drawingMode && !drawing) {
                var features = map.queryRenderedFeatures(point);
                for (var i = 0; i < features.length; i++) {
                    if (features[i].layer.id.slice(-5) == "point") {
                        var dir = features[i].layer.id.substring(0, 2);
                        if (dir == "tl" || dir == "br")
                            $(".mapboxgl-canvas").css("cursor", "nwse-resize");
                        if (dir == "tr" || dir == "bl")
                            $(".mapboxgl-canvas").css("cursor", "nesw-resize");
                        if (dir == "t_" || dir == "b_")
                            $(".mapboxgl-canvas").css("cursor", "ns-resize");
                        if (dir == "r_" || dir == "l_")
                            $(".mapboxgl-canvas").css("cursor", "ew-resize");

                        // 0 = none, 1 = tl, 2 = tr, 3 = bl, 4 = br, 5 = t, 6 = l, 7 = r, 8 = b
                        if (dir == "tl") overpoint = 1;
                        if (dir == "tr") overpoint = 2;
                        if (dir == "bl") overpoint = 3;
                        if (dir == "br") overpoint = 4;
                        if (dir == "t_") overpoint = 5;
                        if (dir == "l_") overpoint = 6;
                        if (dir == "r_") overpoint = 7;
                        if (dir == "b_") overpoint = 8;
                    }
                }
            }
        }

        map.on('mousemove', function (e) {
            lastlat = e.lngLat.lat;
            lastlon = e.lngLat.lng;
            if (drawing) {
                if (!lockvertical) targetlat = e.lngLat.lat;
                if (!lockhorizontal) targetlon = e.lngLat.lng;
            }
            if (!drawingMode && !drawing) {
                overpoint = 0;
                $(".mapboxgl-canvas").css("cursor", "grab");
                $(".mapboxgl-canvas").css("cursor", "-moz-grab");
                $(".mapboxgl-canvas").css("cursor", "-webkit-grab");
            }
            checkFeatures(e.point);
        });

        map.on('mouseup', function (e) {
            if (drawing) {
                drawing = false;
                drawingMode = false;
                var minlon = Math.min(startlon, targetlon);
                var minlat = Math.min(startlat, targetlat);
                var maxlon = Math.max(startlon, targetlon);
                var maxlat = Math.max(startlat, targetlat);

                recreateHandles();
                drawRect(minlon, minlat, maxlon, maxlat, true);

                var n = Math.max(targetlat, startlat);
                var e = Math.max(targetlon, startlon);
                var s = Math.min(targetlat, startlat);
                var w = Math.min(targetlon, startlon);

                // make sure longitude is in range -180 to +180
                var nw = new mapboxgl.LngLat(w, n);
                var se = new mapboxgl.LngLat(e, s);

                nw = nw.wrap();
                se = se.wrap();

                $("#LatitudeNorth").val(nw.lat.toFixed(3));
                $("#LongitudeEast").val(se.lng.toFixed(3));
                $("#LatitudeSouth").val(se.lat.toFixed(3));
                $("#LongitudeWest").val(nw.lng.toFixed(3));

                $(".mapboxgl-canvas").css("border", "3px solid rgba(169, 62, 113, 0)");
            }
        });

        setInterval(update, 1000 / 60);
    });

    function manualArea() {
        var n = parseFloat($("#LatitudeNorth").val()).toFixed(3);
        var s = parseFloat($("#LatitudeSouth").val()).toFixed(3);
        var e = parseFloat($("#LongitudeEast").val()).toFixed(3);
        var w = parseFloat($("#LongitudeWest").val()).toFixed(3);

        if (!($.isNumeric(n) && $.isNumeric(s) && $.isNumeric(e) && $.isNumeric(w))) return;
        if (parseFloat(n) <= parseFloat(s) || parseFloat(e) <= parseFloat(w)) return;

        startlat = n;
        startlon = w;
        targetlat = s;
        targetlon = e;
        currentlat = s;
        currentlon = e;

        var minlon = Math.min(startlon, targetlon);
        var minlat = Math.min(startlat, targetlat);
        var maxlon = Math.max(startlon, targetlon);
        var maxlat = Math.max(startlat, targetlat);

        var n = Math.max(targetlat, startlat);
        var e = Math.max(targetlon, startlon);
        var s = Math.min(targetlat, startlat);
        var w = Math.min(targetlon, startlon);

        // make sure longitude is in range -180 to +180
        var nw = new mapboxgl.LngLat(w, n);
        var se = new mapboxgl.LngLat(e, s);

        nw = nw.wrap();
        se = se.wrap();

        $("#LatitudeNorth").val(nw.lat.toFixed(3));
        $("#LongitudeEast").val(se.lng.toFixed(3));
        $("#LatitudeSouth").val(se.lat.toFixed(3));
        $("#LongitudeWest").val(nw.lng.toFixed(3));

        drawRect(minlon, minlat, maxlon, maxlat);
    }

    $("#LatitudeNorth").change(manualArea);
    $("#LatitudeSouth").change(manualArea);
    $("#LongitudeEast").change(manualArea);
    $("#LongitudeWest").change(manualArea);

    $("#draw-area-button").click(function () {
        drawingMode = true;
        lockhorizontal = false;
        lockvertical = false;
        $(".mapboxgl-canvas").css("border", "3px solid rgba(169, 62, 113, 1)");
        $(".mapboxgl-canvas").css("cursor", "crosshair");
    });

    $(".mapboxgl-ctrl-icon").click(function (e) {
        $(e.currentTarget).closest("form").validate().settings.ignore = "*";
        e.preventDefault();
    });
}