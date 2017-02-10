// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

module.exports = {
  root: true,
  extends: [
    './.eslint/qp.eslintrc'
  ].map(require.resolve)
};
