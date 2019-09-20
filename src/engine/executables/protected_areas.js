'use strict';

const config = require('config');
const mergeAndWindow = require('./shared/merge_and_window');

// run the merge and window procedure
mergeAndWindow.run(
  process.argv[2],
  -9999,
  config.get('protected_areas.tileDir'),
  'marineprotectedareas_',
  'protected_areas');
