// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

/* ECMAScript 6: http://eslint.org/docs/rules/#ecmascript-6 */
module.exports = {
  env: {
    es6: true
  },
  parserOptions: {
    ecmaVersion: 6,
    sourceType: 'module',
    ecmaFeatures: {
      experimentalObjectRestSpread: true
    }
  },
  rules: {
    // require braces in arrow function body
    'arrow-body-style': ['error', 'as-needed', { requireReturnForObjectLiteral: false }],

    // require parens in arrow function arguments
    'arrow-parens': ['error', 'as-needed', { requireForBlockBody: false }],

    // require space before/after arrow function’s arrow
    'arrow-spacing': 'error',

    // verify calls of super() in constructors
    'constructor-super': 'error',

    // enforce spacing around the * in generator functions
    'generator-star-spacing': ['error', { before: false, after: true }],

    // disallow modifying variables of class declarations
    'no-class-assign': 'error', // TODO: maybe 0 for React HOCs

    // disallow arrow functions where they could be confused with comparisons
    'no-confusing-arrow': ['error', { allowParens: true }],

    // disallow modifying variables that are declared using const
    'no-const-assign': 'error',

    // disallow duplicate name in class members
    'no-dupe-class-members': 'error',

    // disallow duplicate imports
    'no-duplicate-imports': 'error',

    // disallow symbol constructor
    'no-new-symbol': 'error',

    // disallow specific imports
    'no-restricted-imports': 'off',

    // disallow use of this/super before calling super() in constructors
    'no-this-before-super': 'error',

    // disallow unnecessary computed property keys on objects
    'no-useless-computed-key': 'error',

    // disallow unnecessary constructor
    'no-useless-constructor': 'error',

    // disallow renaming import, export, and destructured assignments to the same name
    'no-useless-rename': 'error',

    // require let or const instead of var
    'no-var': 'error',

    // require object literal shorthand syntax
    'object-shorthand': ['error', 'always', { avoidQuotes: true }],

    // suggest using arrow functions as callbacks
    'prefer-arrow-callback': 'error',

    // suggest using const
    'prefer-const': 'error',

    // prefer destructuring from arrays and objects
    // TODO: inspect later
    'prefer-destructuring': ['warn', {
      array: true,
      object: true
    }, {
      enforceForRenamedProperties: false
    }],

    // disallow parseint() in favor of binary, octal, and hexadecimal literals
    'prefer-numeric-literals': 'error',

    // suggest using the rest parameters instead of arguments
    'prefer-rest-params': 'error',

    // suggest using the spread operator instead of .apply()
    'prefer-spread': 'error',

    // suggest using template literals instead of string concatenation
    'prefer-template': 'error',

    // disallow generator functions that do not have yield
    'require-yield': 'error',

    // enforce spacing between rest and spread operators and their expressions
    'rest-spread-spacing': 'error',

    // import sorting
    'sort-imports': 'error',

    // require symbol description
    'symbol-description': 'error',

    // enforce usage of spacing in template strings
    'template-curly-spacing': 'error',

    // enforce spacing around the * in yield* expressions
    'yield-star-spacing': 'error'
  }
};
