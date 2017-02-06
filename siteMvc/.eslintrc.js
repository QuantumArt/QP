// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

module.exports = {
  root: true,
  env: {
    browser: true,
    es6: true,
    jquery: true
  },
  extends: [
    // 'plugin:react/recommended'
  ],
  parser: 'espree',
  parserOptions: {
    ecmaVersion: 6,
    sourceType: 'module',
    ecmaFeatures: {
      impliedStrict: true,
      jsx: true,
      experimentalObjectRestSpread: true
    }
  },
  globals: {
    $q: false,
    $c: false,
    $ctx: false
  },
  plugins: [
    // 'react'
  ],
  rules: {
    /* Possible Errors: http://eslint.org/docs/rules/#possible-errors */
    // disallow await inside of loops
    'no-await-in-loop': 'error',

    // disallow assignment operators in conditional statements
    'no-cond-assign': ['error', 'always'],

    // disallow the use of console
    'no-console': 'warn',

    // disallow constant expressions in conditions
    'no-constant-condition': ['error', { checkLoops: false }],

    // disallow control characters in regular expressions
    'no-control-regex': 'error',

    // disallow the use of debugger
    'no-debugger': 'error',

    // disallow duplicate arguments in function definitions
    'no-dupe-args': 'error',

    // disallow duplicate keys in object literals
    'no-dupe-keys': 'error',

    // disallow a duplicate case label
    'no-duplicate-case': 'error',

    // disallow empty character classes in regular expressions
    'no-empty-character-class': 'error',

    // disallow empty statements
    'no-empty': 'error',

    // disallow reassigning exceptions in catch clauses
    'no-ex-assign': 'error',

    // disallow unnecessary boolean casts
    'no-extra-boolean-cast': 'error',

    // disallow unnecessary parentheses
    'no-extra-parens': ['error', 'all', { nestedBinaryExpressions: false, returnAssign: false }],

    // disallow unnecessary semicolons
    'no-extra-semi': 'error',

    // disallow reassigning function declarations
    'no-func-assign': 'error',

    // disallow variable or function declarations in nested blocks
    'no-inner-declarations': ['error', 'both'],

    // disallow invalid regular expression strings in RegExp constructors
    'no-invalid-regexp': 'error',

    // disallow irregular whitespace
    // TODO: check and delete default options
    'no-irregular-whitespace': ['error', {
      skipStrings: false,
      skipComments: false,
      skipRegExps: false,
      skipTemplates: false
    }],

    // disallow calling global object properties as functions
    'no-obj-calls': 'error',

    // disallow use of Object.prototypes builtins directly
    'no-prototype-builtins': 'error',

    // disallow multiple spaces in regular expression literals
    'no-regex-spaces': 'error',

    // disallow sparse arrays
    'no-sparse-arrays': 'error',

    // disallow template literal placeholder syntax in regular strings
    'no-template-curly-in-string': 'error',

    // disallow confusing multiline expressions
    'no-unexpected-multiline': 'error',

    // disallow unreachable code after return, throw, continue, and break statements
    'no-unreachable': 'error',

    // disallow control flow statements in finally blocks
    'no-unsafe-finally': 'error',

    // disallow negating the left operand of relational operators
    'no-unsafe-negation': 'error',

    // require calls to isNaN() when checking for NaN
    'use-isnan': 'error',

    // enforce valid JSDoc comments
    'valid-jsdoc': 'warn',

    // enforce comparing typeof expressions against valid strings
    'valid-typeof': ['error', { requireStringLiterals: true }],



    /* Best Practices: http://eslint.org/docs/rules/#best-practices */
    // enforces getter/setter pairs in objects
    'accessor-pairs': 'error',

    // enforces return statements in callbacks of array’s methods
    'array-callback-return': 'error',

    // treat var as block scoped
    'block-scoped-var': 'error',

    // enforce that class methods utilize this
    'class-methods-use-this': 'error',

    // limit cyclomatic complexity
    'complexity': ['error', 10],

    // require return statements to either always or never specify values
    'consistent-return': 'error',

    // require following curly brace conventions
    'curly': 'error',

    // require default case in switch statements
    'default-case': 'error',

    // enforce newline before and after dot
    'dot-location': ['error', 'property'],

    // require dot notation
    'dot-notation': 'error',

    // require === and !==
    'eqeqeq': 'error',

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
    // TODO: revisit later
    'no-magic-numbers': ['error', { ignore: [-1, 0, 1], enforceConst: true }],
    // 'no-magic-numbers': ['error', { ignoreArrayIndexes: true, enforceConst: true }],

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
    'no-param-reassign': ['error', { props: true }],

    // disallow use of __proto__
    'no-proto': 'error',

    // disallow variable redeclaration
    'no-redeclare': ['error', { builtinGlobals: true }],

    // disallow certain object properties
    'no-restricted-properties': ['error', {
      object: 'arguments',
      message: 'Any arguments props are deprecated, use ES6 syntax instead',
    }, {
      property: 'extend',
      message: 'Use Object.assign instead.',
    }, {
      property: '__defineGetter__',
      message: 'Use Object.defineProperty instead.',
    }, {
      property: '__defineSetter__',
      message: 'Use Object.defineProperty instead.',
    }, {
      object: 'Math',
      property: 'pow',
      message: 'Use the exponentiation operator (**) instead.',
    }],

    // disallow assignment in return statement
    'no-return-assign': ['error', 'always'],

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
    'radix': 'error',

    // disallow async functions which have no await expression
    'require-await': 'error',

    // require variable declarations to be at the top of their scope
    'vars-on-top': 'error',

    // require iifes to be wrapped
    'wrap-iife': 'error',

    // require or disallow yoda conditions
    'yoda': 'error',



    /* Strict Mode: http://eslint.org/docs/rules/#strict-mode */
    // require or disallow strict mode directives
    // 'strict': ['error', 'never'], // enable with babel that inserts [[use strict]]
    'strict': ['error'],



    /* Variables: http://eslint.org/docs/rules/#variables */
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
    'no-shadow': ['error', { 'builtinGlobals': true }],

    // disallow initializing to undefined
    'no-undef-init': 'error',

    // disallow undeclared variables
    'no-undef': 'error',

    // disallow use of undefined variable
    'no-undefined': 'off',

    // disallow unused variables
    'no-unused-vars': 'error',

    // disallow early use
    'no-use-before-define': 'error',



    /* Node.js and CommonJS: http://eslint.org/docs/rules/#nodejs-and-commonjs */
    // enforce return after a callback
    'callback-return': 'warn',

    // enforce require() on the top-level module scope
    'global-require': 'error',

    // enforce callback error handling
    'handle-callback-err': 'warn',

    // disallow require calls to be mixed with regular variable declarations
    'no-mixed-requires': 'error',

    // disallow new require
    'no-new-require': 'error',

    // disallow string concatenation when using __dirname and __filename
    'no-path-concat': 'error',

    // disallow process.env
    'no-process-env': 'warn',

    // disallow process.exit()
    'no-process-exit': 'error',

    // disallow Node.js modules
    'no-restricted-modules': 'off',

    // disallow synchronous methods
    'no-sync': 'warn',



    /* Stylistic Issues: http://eslint.org/docs/rules/#stylistic-issues */
    // disallow or enforce spaces inside of brackets
    'array-bracket-spacing': 'error',

    // disallow or enforce spaces inside of single line blocks
    'block-spacing': 'error',

    // require brace style
    'brace-style': 'error',

    // require camelcase
    'camelcase': 'error',

    // enforce or disallow capitalization of the first letter of a comment
    'capitalized-comments': 'error',

    // require or disallow trailing commas
    'comma-dangle': 'error',

    // enforces spacing around commas
    'comma-spacing': 'error',

    // comma style
    'comma-style': 'error',

    // disallow or enforce spaces inside of computed properties
    'computed-property-spacing': 'error',

    // require consistent this
    'consistent-this': 'error',

    // require or disallow newline at the end of files
    'eol-last': 'error',

    // require or disallow spacing between function identifiers and their invocations
    'func-call-spacing': 'error',

    // require function names to match the name of the variable or property to which they are assigned
    'func-name-matching': ['warn', { includeCommonJSModuleExports: true }],

    // require or disallow named function expressions
    'func-names': ['warn', 'as-needed'],

    // enforce the consistent use of either function declarations or expressions
    // TODO: revisit later
    'func-style': 'warn',

    // disallow specified identifiers
    // TODO: check codebase for any ugly repeated identifiers
    'id-blacklist': 'error',

    // enforce minimum and maximum identifier lengths
    // TODO: check later for loops, that can be a problem
    'id-length': 'warn',

    // require identifiers to match a specified regular expression
    // TODO: should be turned off if will be any problems with default rule
    'id-match': 'warn',

    // enforce consistent indentation
    indent: ['error', 2, {
      SwitchCase: 1,
      VariableDeclarator: 1,
      outerIIFEBody: 1,
      MemberExpression: 1,
      FunctionDeclaration: {
        parameters: 1
      },
      FunctionExpression: {
        parameters: 1
      },
      CallExpression: {
        arguments: 1
      },
      ArrayExpression: 1,
      ObjectExpression: 1
    }],

    // enforce the consistent use of either double or single quotes in jsx attributes
    'jsx-quotes': 'error',

    // enforce consistent spacing between keys and values in object literal properties
    'key-spacing': 'error',

    // enforce consistent spacing before and after keywords
    'keyword-spacing': 'error',

    // enforce position of line comments
    'line-comment-position': 'error',

    // enforce consistent linebreak style
    'linebreak-style': ['error', 'windows'],

    // require empty lines around comments
    'lines-around-comment': ['error', {
      beforeLineComment: true,
      allowBlockStart: true,
      allowObjectStart: true,
      allowArrayStart: true
    }],

    // require or disallow newlines around directives
    // TODO: check on code for default rule settings
    'lines-around-directive': ['error', { before: 'never', after: 'always', }],

    // enforce a maximum depth that blocks can be nested
    'max-depth': 'warn',

    // enforce a maximum line length
    /* REMARK:
     * May be to strict, but I don't see any valuable reason to allow such things,
     * instead of refactoring or optionally ignoring via inline eslint directives for some config files.
     * Some of props that can be opt in are listed below and turned off explicitly, and may be cutted later.
     * All others should not be used in any case.
     */
    'max-len': ['error', 120, 2, {
      ignoreUrls: false,
      ignoreStrings: false,
      ignoreTemplateLiterals: false,
      ignoreRegExpLiterals: false
    }],

    // enforce a maximum file length
    'max-lines': ['warn', { max: 300, skipComments: true }],

    // enforce a maximum depth that callbacks can be nested
    'max-nested-callbacks': 'warn',

    // enforce a maximum number of parameters in function definitions
    'max-params': 'warn',

    // enforce a maximum number of statements allowed per line
    'max-statements-per-line': 'error',

    // enforce a maximum number of statements allowed in function blocks
    'max-statements': 'warn',

    // enforce or disallow newlines between operands of ternary expressions
    // REMARK: should be added some options, like 'always-if-any-operand-is-expression' or 'longer-than-n-symbols'
    'multiline-ternary': 'off',

    // require constructor names to begin with a capital letter
    'new-cap': ['error', {
      capIsNewExceptions: [
        'Immutable.Map',
        'Immutable.Set',
        'Immutable.List'
      ],
    }],

    // require parentheses when invoking a constructor with no arguments
    'new-parens': 'error',

    // require or disallow an empty line after variable declarations
    'newline-after-var': 'error',

    // require an empty line before return statements
    // REMARK: should be added 'ignore' option for some cases,
    // such as if followed after 'var', 'let' or 'const' declarations
    'newline-before-return': 'off',

    // require a newline after each call in a method chain
    'newline-per-chained-call': ['error', { ignoreChainWithDepth: 4 }],

    // disallow Array constructors
    'no-array-constructor': 'error',

    // disallow bitwise operators
    'no-bitwise': 'error',

    // disallow continue statements
    // TODO: disable later, check codebase
    'no-continue': 'warn',

    // disallow inline comments after code
    'no-inline-comments': 'off',

    // disallow if statements as the only statement in else blocks
    'no-lonely-if': 'error',

    // disallow mixes of different operators
    // TODO: check why airbnb disable 'allowSamePrecedence' option. Specially check for '+-' and '&&||' pairs.
    // TODO: check that default options are work
    'no-mixed-operators': 'error',

    // disallow mixed spaces and tabs for indentation
    'no-mixed-spaces-and-tabs': 'error',

    // disallow use of chained assignment expressions
    'no-multi-assign': 'error',

    // disallow multiple empty lines
    // TODO: check for default options 'maxEOF' and 'maxBOF'
    'no-multiple-empty-lines': ['error', {
      max: 2,
      maxEOF: 1,
      maxBOF: 0
    }],

    // disallow negated conditions
    'no-negated-condition': 'error',

    // disallow nested ternary expressions
    'no-nested-ternary': 'error',

    // disallow object constructors
    'no-new-object': 'error',

    // disallow the unary operators ++ and --
    // TODO: check why airbnb disallow loops option
    'no-plusplus': ['error', { allowForLoopAfterthoughts: true }],

    // disallow specified syntax
    // TODO: revisit later, now is default airbnb
    'no-restricted-syntax': [
      'error',
      'ForInStatement',
      'ForOfStatement',
      'LabeledStatement',
      'WithStatement',
    ],

    // disallow all tabs
    'no-tabs': 'error',

    // disallow ternary operators
    'no-ternary': 'off',

    // disallow trailing whitespace at the end of lines
    'no-trailing-spaces': 'error',

    // disallow dangling underscores in identifiers
    'no-underscore-dangle': 'error',

    // disallow ternary operators when simpler alternatives exist
    'no-unneeded-ternary': ['error', { defaultAssignment: false }],

    // disallow whitespace before properties
    'no-whitespace-before-property': 'error',

    // enforce consistent line breaks inside braces
    // TODO: enable once https://github.com/eslint/eslint/issues/6488 is resolved
    'object-curly-newline': ['off', {
      ObjectExpression: { minProperties: 2, multiline: true },
      ObjectPattern: { minProperties: 2, multiline: true }
    }],

    // enforce consistent spacing inside braces
    'object-curly-spacing': ['error', 'always'],

    // enforce placing object properties on separate lines
    'object-property-newline': ['error', { allowMultiplePropertiesPerLine: true, }],

    // require or disallow newlines around variable declarations
    'one-var-declaration-per-line': 'error',

    // enforce variables to be declared either together or separately in functions
    'one-var': ['error', { 'initialized': 'never', 'uninitialized': 'always' }],

    // require or disallow assignment operator shorthand where possible
    'operator-assignment': 'error',

    // enforce consistent linebreak style for operators
    'operator-linebreak': ['error', 'before'],

    // require or disallow padding within blocks
    // TODO: check for 'switches' option
    'padded-blocks': ['error', 'never'],

    // require quotes around object literal property names
    'quote-props': ['error', 'as-needed'],

    // enforce the consistent use of either backticks, double, or single quotes
    // TODO: check airbnb style guide for ignoring 'allowTemplateLiterals'
    'quotes': ['error', 'single', { avoidEscape: true, allowTemplateLiterals: true }],

    // require jsdoc comments
    'require-jsdoc': 'off',

    // enforce spacing before and after semicolons
    'semi-spacing': 'error',

    // require or disallow semicolons instead of asi
    /* TODO: check this code from eslint ASI problem samples
      var globalCounter = { }

      (function () {
          var n = 0
          globalCounter.increment = function () {
              return ++n
          }
      })()
    */
    semi: 'error',

    // require object keys to be sorted
    'sort-keys': 'off',

    // variable sorting
    'sort-vars': 'off',

    // require or disallow space before blocks
    'space-before-blocks': 'error',

    // require or disallow a space before function parenthesis
    'space-before-function-paren': ['error', {
      anonymous: 'always',
      named: 'never',
      asyncArrow: 'always'
    }],

    // disallow or enforce spaces inside of parentheses
    'space-in-parens': 'error',

    // require spacing around infix operators
    'space-infix-ops': 'error',

    // require or disallow spaces before/after unary operators
    // TODO: check for default eslint options
    'space-unary-ops': ['error', { words: true, nonwords: false }],

    // requires or disallows a whitespace (space or tab) beginning a comment
    // TODO: revisit later for '+' and '-' airbnb exceptions, and check for git diff
    'spaced-comment': ['error', 'always', {
      line: {
        exceptions: ['-', '+'],
        markers: ['=', '!']
      },
      block: {
        exceptions: ['-', '+'],
        markers: ['=', '!']
      }
    }],

    // require or disallow spacing between template tags and their literals
    // TODO: github bug because not presetn at commit list of all rules 'http://eslint.org/docs/rules/#stylistic-issues'
    'template-tag-spacing': 'error',

    // require or disallow the unicode byte order mark
    'unicode-bom': 'error',

    // require regex literals to be wrapped
    'wrap-regex': 'off',



    /* ECMAScript 6: http://eslint.org/docs/rules/#ecmascript-6 */
    // require braces in arrow function body
    'arrow-body-style': ['error', 'as-needed', { requireReturnForObjectLiteral: true }],

    // require parens in arrow function arguments
    'arrow-parens': ['error', 'as-needed', { requireForBlockBody: false }],

    // require space before/after arrow function’s arrow
    'arrow-spacing': 'error',

    // verify calls of super() in constructors
    'constructor-super': 'error',

    // enforce spacing around the * in generator functions
    'generator-star-spacing': ['error', { before: false, after: true }],

    // disallow modifying variables of class declarations
    'no-class-assign': 'error',

    // disallow arrow functions where they could be confused with comparisons
    'no-confusing-arrow': 'off',

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
      enforceForRenamedProperties: true
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
