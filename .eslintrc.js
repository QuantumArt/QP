/* eslint-env node */

module.exports = {
  root: true,
  extends: [
    './.eslint/.eslintrc'
  ].map(require.resolve),
  globals: {
    $q: false,
    $c: false,
    $l: false,
    $ctx: false,
    Quantumart: false
  },
  rules: {
    // 'no-invalid-this': 'off',
    // 'func-name-matching': 'off',
    'no-underscore-dangle': 'off',
    'func-names': 'off'
  }
};
