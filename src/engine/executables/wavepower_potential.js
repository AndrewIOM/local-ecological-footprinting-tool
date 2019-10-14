var config = require("config");
var mergeAndWindow = require("./shared/merge_and_window");

// run the merge and window procedure
mergeAndWindow.run(
	process.argv[2],
	-9999,
	config.get("wavepower_potential.tileDir"),
	"wavepowerpotential_",
	"wavepower_potential"
);
