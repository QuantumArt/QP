// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

module.exports = {
  extends: [
    './browser.es5.eslintrc',
    './es6'
  ].map(require.resolve),
  parser: 'espree',
  parserOptions: {
    ecmaFeatures: {
      impliedStrict: true
    }
  }
};
