// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

/* Variables: http://eslint.org/docs/rules/#variables */
module.exports = {
  rules: {
    // require or disallow initialization in variable declarations
    'init-declarations': 'off',

    // disallow shadowing of variables inside of catch
    'no-catch-shadow': 'warn',

    // disallow deleting variables
    'no-delete-var': 'error',

    // disallow labels that are variables names
    'no-label-var': 'error',

    // disallow specific global variables
    'no-restricted-globals': 'off',

    // disallow shadowing of restricted names
    'no-shadow-restricted-names': 'error',

    // disallow declaration of variables already declared in the outer scope
    'no-shadow': ['error', { builtinGlobals: true }],

    // disallow initializing to undefined
    'no-undef-init': 'error',

    // disallow undeclared variables
    'no-undef': 'error',

    // disallow use of undefined variable
    'no-undefined': 'off',

    // disallow unused variables
    'no-unused-vars': 'error',

    // disallow early use
    'no-use-before-define': 'error'
  }
};
