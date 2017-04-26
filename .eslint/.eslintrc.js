// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

module.exports = {
  extends: [
    './browser.es5.eslintrc'
  ].map(require.resolve),
  globals: {
    $q: false,
    $c: false,
    $l: false,
    $ctx: false,
    Quantumart: false
  },
  rules: {
    'no-invalid-this': 'off',
    'func-name-matching': 'off',
    'no-underscore-dangle': 'off'
  }
};
