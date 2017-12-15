/* eslint-env node */

module.exports = {
  root: true,
  extends: [
    './.eslint/.eslintrc'
  ].map(require.resolve),
  globals: {
    // _: false,
    ko: false,
    $a: false,
    $c: false,
    $e: false,
    $l: false,
    $o: false,
    $q: false,
    Sys: false,
    Url: false,
    // $ctx: false,
    pmrpc: false,
    Quantumart: false,
  },
  rules: {
    'no-invalid-this': 'off',
    'no-underscore-dangle': 'off',
    'func-names': 'off',
    'sort-imports': 'off'
  }
};
