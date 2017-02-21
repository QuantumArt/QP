// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

/* Node.js and CommonJS: http://eslint.org/docs/rules/#nodejs-and-commonjs */
module.exports = {
  env: {
    node: true
  },
  rules: {
    // enforce return after a callback
    'callback-return': 'warn',

    // enforce require() on the top-level module scope
    'global-require': 'error',

    // enforce callback error handling
    'handle-callback-err': 'warn',

    // disallow require calls to be mixed with regular variable declarations
    'no-mixed-requires': 'error',

    // disallow new require
    'no-new-require': 'error',

    // disallow string concatenation when using __dirname and __filename
    'no-path-concat': 'error',

    // disallow process.env
    'no-process-env': 'warn',

    // disallow process.exit()
    'no-process-exit': 'error',

    // disallow Node.js modules
    'no-restricted-modules': 'off',

    // disallow synchronous methods
    'no-sync': 'warn'
  }
};
