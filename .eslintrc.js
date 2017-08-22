/* eslint-env node */

module.exports = {
  root: true,
  extends: [
    './.eslint/.eslintrc'
  ].map(require.resolve),
  globals: {
    _: false,
    ko: false,
    $a: false,
    $c: false,
    $e: false,
    $l: false,
    $o: false,
    $q: false,
    Sys: false,
    Url: false,
    $ctx: false,
    pmrpc: false,
    Quantumart: false,
    Silverlight: false
  },
  rules: {
    // 'func-name-matching': 'off',
    'no-invalid-this': 'warn',
    'no-underscore-dangle': 'off',
    'func-names': 'off',
    'sort-imports': 'off'
  }
};
