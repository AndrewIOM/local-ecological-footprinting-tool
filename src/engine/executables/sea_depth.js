'use strict';

const config = require('config');
const mergeAndWindow = require('./shared/merge_and_window');

// run the merge and window procedure
mergeAndWindow.run(
  process.argv[2],
  0,
  config.get('sea_depth.tileDir'),
  'DEPTH_',
  'sea_depth',
  0,
  true,
  256,
  false
);
