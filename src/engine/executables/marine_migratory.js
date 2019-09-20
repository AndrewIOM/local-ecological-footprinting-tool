'use strict';

const config = require('config');
const mergeAndWindow = require('./shared/merge_and_window');

// run the merge and window procedure
mergeAndWindow.run(
  process.argv[2],
  -999,
  config.get('marine_migratory.tileDir'),
  'migratory_',
  'marine_migratory',
  0,
  true,
  256);
