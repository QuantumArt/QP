// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

module.exports = {
  root: true,
  env: {
    es6: false
  },
  extends: [
    './.eslint/default.eslintrc'
  ].map(require.resolve),
  parser: 'espree',
  parserOptions: {
    ecmaVersion: 5,
    sourceType: 'script',
    ecmaFeatures: {
      impliedStrict: false
    }
  },
  globals: {
    $q: false,
    $c: false,
    $l: false,
    $ctx: false,
    Quantumart: false
  },
  rules: {
    /* Best Practices: http://eslint.org/docs/rules/#best-practices */
    'no-invalid-this': 'off',


    /* Node.js and CommonJS: http://eslint.org/docs/rules/#nodejs-and-commonjs */
    'callback-return': 'off',
    'global-require': 'off',
    'handle-callback-err': 'off',
    'no-mixed-requires': 'off',
    'no-new-require': 'off',
    'no-path-concat': 'off',
    'no-process-env': 'off',
    'no-process-exit': 'off',
    'no-restricted-modules': 'off',
    'no-sync': 'off',


    /* Stylistic Issues: http://eslint.org/docs/rules/#stylistic-issues */
    'func-name-matching': 'off',
    'no-underscore-dangle': 'off',


    /* ECMAScript 6: http://eslint.org/docs/rules/#ecmascript-6 */
    'arrow-body-style': 'off',
    'arrow-parens': 'off',
    'arrow-spacing': 'off',
    'constructor-super': 'off',
    'generator-star-spacing': 'off',
    'no-class-assign': 'off',
    'no-confusing-arrow': 'off',
    'no-const-assign': 'off',
    'no-dupe-class-members': 'off',
    'no-duplicate-imports': 'off',
    'no-new-symbol': 'off',
    'no-restricted-imports': 'off',
    'no-this-before-super': 'off',
    'no-useless-computed-key': 'off',
    'no-useless-constructor': 'off',
    'no-useless-rename': 'off',
    'no-var': 'off',
    'object-shorthand': 'off',
    'prefer-arrow-callback': 'off',
    'prefer-const': 'off',
    'prefer-destructuring': 'off',
    'prefer-numeric-literals': 'off',
    'prefer-rest-params': 'off',
    'prefer-spread': 'off',
    'prefer-template': 'off',
    'require-yield': 'off',
    'rest-spread-spacing': 'off',
    'sort-imports': 'off',
    'symbol-description': 'off',
    'template-curly-spacing': 'off',
    'yield-star-spacing': 'off'
  }
};
