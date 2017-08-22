// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

module.exports = {
  extends: [
    './browser.es6.eslintrc'
  ].map(require.resolve)
};


// "no-nested-ternary": 0,  // because JSX
// 'react/jsx-handler-names': ['error', { // airbnb is disabling this rule
//   eventHandlerPrefix: 'handle',
//   eventHandlerPropPrefix: 'on',
// }],
// 'react/jsx-filename-extension': ['error', {extensions: ['.js']}], // airbnb is using .jsx
// 'react/jsx-max-props-per-line': ['error', {maximum: 3}], // airbnb is disabling this rule
// 'react/no-danger': 'error', // airbnb is using warn
// 'react/no-direct-mutation-state': 'error', // airbnb is disabling this rule

// 'react/forbid-prop-types': 'off',
// 'react/no-unused-prop-types': [1, {
//   skipShapeProps: true,
// }],

// 'react/jsx-filename-extension': [1, {
//   extensions: ['.js', '.jsx'],
// }],

// "react/sort-comp": 0, // personal preference
// "react/no-multi-comp": 0 // personal preference
// 'react/no-multi-comp': 'off',

// "import/no-extraneous-dependencies": 0, // monorepo setup
// "import/no-unresolved": [1, { ignore: ['^reactabular'] }], // monorepo setup

// 'import/no-extraneous-dependencies': 'off',
// 'import/no-dynamic-require': 'off',
