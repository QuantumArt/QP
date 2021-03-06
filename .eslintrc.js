/* eslint-env node */

module.exports = {
  root: true,
  extends: [
    './.eslint/.eslintrc'
  ].map(require.resolve),
  plugins: [
    'import'
  ],
  globals: {
    ko: false,
    // $a: false,
    // $c: false,
    $e: false,
    $l: false,
    // $o: false,
    // $q: false,
    // $ctx: false,
    Sys: false,
    // Url: false,
    pmrpc: false,
    Quantumart: false,
  },
  rules: {
    'no-invalid-this': 'off',
    'no-underscore-dangle': 'off',
    'func-names': 'off',
    'sort-imports': 'off',
    'class-methods-use-this': 'off'
  }
};
