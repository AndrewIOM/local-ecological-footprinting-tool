var config = require("config");
var intersectShapeFile = require("./shared/intersect_shapefile");

// run the shapefile procedure
intersectShapeFile.run(
	process.argv[2],
    "marine_protected_areas_list",
	config.get("marine_protected_areas_list.shapefile")
);
