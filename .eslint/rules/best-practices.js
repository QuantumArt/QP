// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

/* Best Practices: http://eslint.org/docs/rules/#best-practices */
module.exports = {
  rules: {
    // enforces getter/setter pairs in objects
    'accessor-pairs': 'error',

    // enforces return statements in callbacks of array’s methods
    'array-callback-return': 'error',

    // treat var as block scoped
    'block-scoped-var': 'error',

    // enforce that class methods utilize this
    'class-methods-use-this': 'error', // TODO: maybe 0 for react

    // limit cyclomatic complexity
    complexity: ['warn', 20],

    // require return statements to either always or never specify values
    'consistent-return': 'error',

    // require following curly brace conventions
    curly: 'error',

    // require default case in switch statements
    'default-case': 'error',

    // enforce newline before and after dot
    'dot-location': ['error', 'property'],

    // require dot notation
    'dot-notation': 'error',

    // require === and !==
    eqeqeq: 'error',

    // require guarding for-in
    'guard-for-in': 'error',

    // disallow use of alert
    'no-alert': 'warn',

    // disallow use of caller/callee
    'no-caller': 'error',

    // disallow lexical declarations in case/default clauses
    'no-case-declarations': 'error',

    // disallow regexs that look like division
    'no-div-regex': 'off',

    // disallow return before else
    'no-else-return': 'error',

    // disallow empty functions
    'no-empty-function': 'error',

    // disallow empty destructuring patterns
    'no-empty-pattern': 'error',

    // disallow null comparisons
    'no-eq-null': 'error',

    // disallow eval()
    'no-eval': 'error',

    // disallow extending of native objects
    'no-extend-native': 'error',

    // disallow unnecessary function binding
    'no-extra-bind': 'error',

    // disallow unnecessary labels
    'no-extra-label': 'error',

    // disallow case statement fallthrough
    'no-fallthrough': 'error',

    // disallow floating decimals
    'no-floating-decimal': 'error',

    // disallow assignment to native objects or read-only global variables
    'no-global-assign': 'error',

    // disallow the type conversion with shorter notations
    'no-implicit-coercion': 'off',

    // disallow variable and function declarations in the global scope
    'no-implicit-globals': 'error',

    // disallow implied eval()
    'no-implied-eval': 'error',

    // disallow this keywords outside of classes or class-like objects
    'no-invalid-this': 'error',

    // disallow iterator
    'no-iterator': 'error',

    // disallow labeled statements
    'no-labels': 'error',

    // disallow unnecessary nested blocks
    'no-lone-blocks': 'error',

    // disallow functions in loops
    'no-loop-func': 'error',

    // disallow magic numbers
    'no-magic-numbers': ['off', { ignoreArrayIndexes: true, enforceConst: true }],

    // disallow multiple spaces
    'no-multi-spaces': 'error',

    // disallow multiline strings
    'no-multi-str': 'error',

    // disallow function constructor
    'no-new-func': 'error',

    // disallow primitive wrapper instances
    'no-new-wrappers': 'error',

    // disallow new for side effects
    'no-new': 'error',

    // disallow octal escape sequences in string literals
    'no-octal-escape': 'error',

    // disallow octal literals
    'no-octal': 'error',

    // disallow reassignment of function parameters
    'no-param-reassign': ['error', {
      props: true,
      ignorePropertyModificationsFor: ['fn']
    }],

    // disallow use of __proto__
    'no-proto': 'error',

    // disallow variable redeclaration
    'no-redeclare': ['error', { builtinGlobals: true }],

    // disallow certain object properties
    'no-restricted-properties': ['error', {
      object: 'arguments',
      message: 'Any arguments props are deprecated, use ES6 syntax instead'
    }, {
      property: 'extend',
      message: 'Use Object.assign instead.'
    }, {
      property: '__defineGetter__',
      message: 'Use Object.defineProperty instead.'
    }, {
      property: '__defineSetter__',
      message: 'Use Object.defineProperty instead.'
    }, {
      object: 'Math',
      property: 'pow',
      message: 'Use the exponentiation operator (**) instead.'
    }],

    // disallow assignment in return statement
    'no-return-assign': ['error', 'always'], // TODO: maybe 0 for React refs assign

    // disallows unnecessary return await
    'no-return-await': 'error',

    // disallow script urls
    'no-script-url': 'error',

    // disallow self assignment
    'no-self-assign': ['error', { props: true }],

    // disallow self compare
    'no-self-compare': 'error',

    // disallow use of the comma operator
    'no-sequences': 'error',

    // restrict what can be thrown as an exception
    'no-throw-literal': 'error',

    // disallow unmodified conditions of loops
    'no-unmodified-loop-condition': 'error',

    // disallow unused expressions
    'no-unused-expressions': 'error',

    // disallow unused labels
    'no-unused-labels': 'error',

    // disallow unnecessary .call() and .apply()
    'no-useless-call': 'error',

    // disallow unnecessary concatenation of strings
    'no-useless-concat': 'error',

    // disallow unnecessary escape usage
    'no-useless-escape': 'error',

    // disallow redundant return statements
    'no-useless-return': 'error',

    // disallow use of the void operator
    'no-void': 'error',

    // disallow warning comments
    'no-warning-comments': 'warn',

    // disallow with statements
    'no-with': 'error',

    // require using error objects as promise rejection reasons
    'prefer-promise-reject-errors': 'error',

    // require radix parameter
    radix: 'error',

    // disallow async functions which have no await expression
    'require-await': 'error',

    // require variable declarations to be at the top of their scope
    'vars-on-top': 'error',

    // require iifes to be wrapped
    'wrap-iife': 'error',

    // require or disallow yoda conditions
    yoda: 'error'
  }
};
