// Licensed by MIT
// Copyright (c) 2017 Alex Kostyukov
// https://github.com/AuthorProxy/dotfiles

/* eslint-env node */

/* Stylistic Issues: http://eslint.org/docs/rules/#stylistic-issues */
module.exports = {
  rules: {
    // disallow or enforce spaces inside of brackets
    'array-bracket-spacing': 'error',

    // disallow or enforce spaces inside of single line blocks
    'block-spacing': 'error',

    // require brace style
    'brace-style': 'error',

    // require camelcase
    camelcase: 'error',

    // enforce or disallow capitalization of the first letter of a comment
    'capitalized-comments': 'off',

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
    'func-style': ['warn', 'expression', { allowArrowFunctions: true }],

    // disallow specified identifiers
    // TODO: check codebase for any ugly repeated identifiers
    'id-blacklist': 'error',

    // enforce minimum and maximum identifier lengths
    'id-length': ['warn', { exceptions: ['e', 'i'] }],

    // require identifiers to match a specified regular expression
    'id-match': 'off',

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
    'linebreak-style': ['off', 'windows'],

    // require empty lines around comments
    'lines-around-comment': ['error', {
      beforeLineComment: true,
      allowBlockStart: true,
      allowObjectStart: true,
      allowArrayStart: true
    }],

    // require or disallow newlines around directives
    // TODO: check on code for default rule settings
    'lines-around-directive': ['error', { before: 'never', after: 'always' }],

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
      ignoreUrls: true,
      ignoreStrings: false,
      ignoreTemplateLiterals: false,
      ignoreRegExpLiterals: false
    }],

    // enforce a maximum file length
    'max-lines': ['warn', { max: 500, skipComments: true }],

    // enforce a maximum depth that callbacks can be nested
    'max-nested-callbacks': 'warn',

    // enforce a maximum number of parameters in function definitions
    'max-params': ['warn', 5],

    // enforce a maximum number of statements allowed per line
    'max-statements-per-line': 'error',

    // enforce a maximum number of statements allowed in function blocks
    'max-statements': ['warn', 30],

    // enforce or disallow newlines between operands of ternary expressions
    // REMARK: should be added some options, like 'always-if-any-operand-is-expression' or 'longer-than-n-symbols'
    'multiline-ternary': 'off',

    // require constructor names to begin with a capital letter
    'new-cap': ['error', {
      newIsCapExceptions: ['$.telerik.stringBuilder', 'CKEDITOR.dialogCommand'],
      capIsNewExceptions: [
        'window.CollectGarbage',
        'Immutable.Map',
        'Immutable.Set',
        'Immutable.List'
      ]
    }],

    // require parentheses when invoking a constructor with no arguments
    'new-parens': 'error',

    // require or disallow an empty line after variable declarations
    // REMARK: should be added 'ignore' option for some cases,
    // such as single line var
    'newline-after-var': 'off',

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
      'WithStatement'
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
    'object-property-newline': ['error', { allowMultiplePropertiesPerLine: true }],

    // require or disallow newlines around variable declarations
    'one-var-declaration-per-line': 'error',

    // enforce variables to be declared either together or separately in functions
    'one-var': ['error', { initialized: 'never', uninitialized: 'always' }],

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
    quotes: ['error', 'single', { avoidEscape: true, allowTemplateLiterals: true }],

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
    'wrap-regex': 'off'
  }
};
