/* eslint-env node */

const fs = require('fs');
const path = require('path');
const del = require('del');
const gulp = require('gulp');
const chalk = require('chalk');
const bs = require('browser-sync').create();
const { argv } = require('yargs');
const loadPlugins = require('gulp-load-plugins');
const notifier = require('node-notifier');
const webpack = require('webpack');
const gulpSass = require('gulp-sass');
const sass = require('sass');
const log = require('fancy-log');
const PluginError = require('plugin-error');
const through = require('through2');
const terser = require('gulp-terser');
const sharp = require('sharp');
const glob = require('glob');
const postcss = require('gulp-postcss');
const autoprefixer = require('autoprefixer');
const cssnano = require('cssnano');

const $ = loadPlugins();
const sassCompiler = gulpSass(sass);

const project = JSON.parse(fs.readFileSync('../package.json'));
const custom = {
  project,
  config: {
    name: project.name,
    version: project.version,
    environment: 'development',
    commit: process.env.BUILD_SOURCEVERSION || '0',
    branchName: process.env.BUILD_SOURCEBRANCHNAME || '',
    buildNumber: process.env.BUILD_BUILDNUMBER || ''
  }
};

if (process.env.NODE_ENV
  && (process.env.NODE_ENV.toLowerCase() === 'production' || process.env.NODE_ENV.toLowerCase() === 'release')) {
  custom.config.environment = 'production';
}

if (argv.env && (argv.env.toLowerCase() === 'production' || argv.env.toLowerCase() === 'release')) {
  custom.config.environment = 'production';
}

custom.isProduction = function isProduction() {
  return custom.config.environment === 'production';
};

custom.destPaths = {
  scripts: 'Scripts/build',
  styles: 'Static/build',
  images: 'Static/build'
};

custom.paths = {
  vendorsjs: [
    'Scripts/polyfills/array/array.find.polyfill.js',
    'Scripts/polyfills/array/array.findIndex.polyfill.js',
    'Scripts/polyfills/array/array.from.polyfill.js',
    'Scripts/polyfills/array/array.includes.polyfill.js',
    'Scripts/polyfills/object/object.assign.polyfill.js',
    'Scripts/polyfills/object/object.entries.polyfill.js',
    'Scripts/polyfills/object/object.values.polyfill.js',
    'Scripts/polyfills/string/string.endsWith.polyfill.js',
    'Scripts/polyfills/string/string.includes.polyfill.js',
    'Scripts/polyfills/string/string.repeat.polyfill.js',
    'Scripts/polyfills/string/string.startsWith.polyfill.js',
    'Scripts/polyfills/console.fns.polyfill.js',
    'Scripts/polyfills/promise.polyfill.js',

    // 'Scripts/jquery/jquery-1.8.3.js',
    'Scripts/jquery/jquery-1.7.1.js',
    'Scripts/telerik/telerik.common.js',
    'Scripts/microsoft/MicrosoftAjax.js',
    'Scripts/pmrpc.js',
    'Scripts/knockout.js',

    'Scripts/jquery/jquery.cookie.js',
    'Scripts/jquery/jquery.timers.js',
    'Scripts/jquery/jquery.scrollTo-1.4.2.js',
    'Scripts/jquery/jquery.splitter.js',
    'Scripts/jquery/jquery.form.js',
    'Scripts/jquery/jquery.validate.js',
    'Scripts/jquery/jquery.jeegoocontext.js',
    'Scripts/jquery/jquery.verticalResizer.js',
    'Scripts/jquery/jquery.qtip.js',
    'Scripts/jquery/jquery.imgareaselect.js',
    'Scripts/jquery/jquery.nouislider.link.js',
    'Scripts/jquery/jquery.nouislider.js',

    'Scripts/telerik/telerik.draganddrop.js',
    'Scripts/telerik/telerik.treeview.js',
    'Scripts/telerik/telerik.window.js',
    'Scripts/telerik/telerik.textbox.js',
    'Scripts/telerik/telerik.combobox.js',
    'Scripts/telerik/telerik.calendar.js',
    'Scripts/telerik/telerik.datepicker.js',
    'Scripts/telerik/telerik.timepicker.js',
    'Scripts/telerik/telerik.datetimepicker.js',
    'Scripts/telerik/telerik.grid.js',
    'Scripts/telerik/telerik.grid.resizing.js',
    'Scripts/telerik/telerik.grid.filtering.js',
    'Scripts/telerik/telerik.grid.grouping.js',
    'Scripts/telerik/telerik.grid.editing.js',
    'Scripts/telerik/telerik.upload.js',
    'Scripts/telerik/telerik.splitter.js',

    'Static/ckeditor/ckeditor.js',
    '../node_modules/jsoneditor/dist/jsoneditor.js',
    '../node_modules/codemirror/lib/codemirror.js',
    '../node_modules/codemirror/mode/clike/clike.js',
    '../node_modules/codemirror/mode/sql/sql.js',
    '../node_modules/codemirror/mode/xml/xml.js',
    '../node_modules/codemirror/mode/css/css.js',
    '../node_modules/codemirror/mode/vb/vb.js',
    '../node_modules/codemirror/mode/javascript/javascript.js',
    '../node_modules/codemirror/mode/htmlmixed/htmlmixed.js',
    '../node_modules/codemirror/mode/htmlembedded/htmlembedded.js',
    '../node_modules/codemirror/addon/mode/multiplex.js',

    'Scripts/PlUpload/moxie.js',
    'Scripts/PlUpload/plupload.dev.js'
  ],
  vendorsjsLogon: [
    // 'Scripts/jquery/jquery-1.8.3.js',
    'Scripts/jquery/jquery-1.7.1.js',
    'Scripts/microsoft/MicrosoftAjax.js'
  ],
  styles: [
    'Static/basic.css',
    'Static/page.css',
    'Static/telerik.common.css',
    'Static/jquery.nouislider.css',
    'Static/jquery.qtip.css',
    'Static/imgCropResizeClient.css',
    'Static/imgareaselect.css',
    'Static/jquery.jeegoocontext.qp8.css',
    'Static/telerik.qp8.css',
    '../node_modules/codemirror/lib/codemirror.css',
    'Static/codemirrorTheme.css',
    '../node_modules/jsoneditor/dist/jsoneditor.css',
    'Static/QpCodemirror.css',
    'Static/custom/**/*.{scss,css}',

    '!Static/build/**/*.css'
  ],
  stylesLogon: [
    'Static/basic.css',
    'Static/page.css'
  ],
  images: [
    'Static/**/*.{jpg,jpeg,png,gif,svg}',
    '!Static/ckeditor/**/*.{jpg,jpeg,png,gif,svg}',
    '!Static/build/**/*.{jpg,jpeg,png,gif,svg}'
  ],
  clean: [
    'Scripts/Quantumart/**/*.{min.js,map}',
    'Static/custom/**/*.{min.css,map}',
    custom.destPaths.scripts,
    'Static/build/**/*.css'
  ]
};

custom.reportError = function (error) {
  let report;
  $.notify({
    title: `Task Failed [${error.plugin}]`,
    message: error.lineNumber ? `Line: ${error.lineNumber} -- ` : 'See console.',
    sound: 'Sosumi'
  }).write(error);

  report = `${chalk.underline.bgRed('Task:')} [${chalk.underline.bgCyan(error.plugin)}]\n`;
  if (error.fileName) {
    report += `${chalk.underline.bgRed('FileName:')} ${chalk.underline.bgCyan(error.fileName)}\n`;
  }

  if (error.lineNumber) {
    report += `${chalk.underline.bgRed('LineNumber:')} ${chalk.underline.bgCyan(error.lineNumber)}\n`;
  }

  report += `${chalk.underline.bgRed('Message:')}`
    + `${error.message.replace('Error:', chalk.underline.bgRed('Error:'))}\n`;

  global.console.error(report);
  this.emit('end');
};

gulp.task('assets:vendorsjs', function() {
  return gulp.src(custom.paths.vendorsjs, { base: './', sourcemaps: true, allowEmpty: true })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.rename({ suffix: '.min' }))
  .pipe(custom.isProduction() ? terser({
    compress: {
      sequences: false
    }
  }) : through.obj())
  .pipe($.concat('vendors.js'))
  .pipe(gulp.dest(custom.destPaths.scripts, { sourcemaps: '.' }))
  .pipe($.size({ title: 'assets:vendorsjs', showFiles: true }))
  .pipe($.notify({ title: 'Part task was completed', message: 'assets:vendorsjs task complete', onLast: true }));
});

gulp.task('assets-logon:vendorsjs', function() {
  return gulp.src(custom.paths.vendorsjsLogon, { base: './', sourcemaps: true, allowEmpty: true })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.rename({ suffix: '.min' }))
  .pipe(custom.isProduction() ? terser({
    compress: {
      sequences: false
    }
  }) : through.obj())
  .pipe($.concat('vendors-logon.js'))
  .pipe(gulp.dest(custom.destPaths.scripts, { sourcemaps: '.' }))
  .pipe($.size({ title: 'assets-logon:vendorsjs', showFiles: true }))
  .pipe($.notify({
    title: 'Part task was completed',
    message: 'assets-logon:vendorsjs task complete',
    onLast: true
  }));
});

gulp.task('webpack:qpjs', function(done) {
  webpack(require('./webpack.config.js'), function(err, stats) {
    if (err) {
      throw new PluginError('webpack:qpjs', err);
    }
    log('[webpack:qpjs]', stats.toString({ colors: true }));
    notifier.notify({ title: 'Part task was completed', message: 'webpack:qpjs task complete' });
  });
  done();
});

gulp.task('assets:js', gulp.series('assets:vendorsjs', 'webpack:qpjs', function() {
  return gulp.src(custom.destPaths.scripts)
  .pipe($.notify({ title: 'Task was completed', message: 'assets:js task complete', onLast: true }));
}));

gulp.task('assets-logon:js', gulp.series('assets-logon:vendorsjs', 'webpack:qpjs', function() {
  return gulp.src(custom.destPaths.scripts)
  .pipe($.notify({ title: 'Task was completed', message: 'assets-logon:js task complete', onLast: true }));
}));

gulp.task('assets:img', async function() {
  const imageFiles = glob.sync('Static/**/*.{jpg,jpeg,png,gif}', {
    ignore: ['Static/ckeditor/**', 'Static/codemirror/**', 'Static/build/**']
  });

  for (const file of imageFiles) {
    const outputPath = path.join(custom.destPaths.images, path.relative('Static', file));
    const outputDir = path.dirname(outputPath);

    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    const ext = path.extname(file).toLowerCase();

    try {
      if (ext === '.jpg' || ext === '.jpeg') {
        await sharp(file).jpeg({ quality: 75, progressive: true }).toFile(outputPath);
      } else if (ext === '.png') {
        await sharp(file).png({ compressionLevel: 9 }).toFile(outputPath);
      } else if (ext === '.gif') {
        // Sharp doesn't optimize GIF well, copy as-is
        fs.copyFileSync(file, outputPath);
      }
    } catch (err) {
      log('[assets:img]', `Warning: Could not process ${file}: ${err.message}`);
      // Copy file as-is on error
      fs.copyFileSync(file, outputPath);
    }
  }

  // Handle SVG files - just copy them
  const svgFiles = glob.sync('Static/**/*.svg', {
    ignore: ['Static/ckeditor/**', 'Static/codemirror/**', 'Static/build/**']
  });

  for (const file of svgFiles) {
    const outputPath = path.join(custom.destPaths.images, path.relative('Static', file));
    const outputDir = path.dirname(outputPath);

    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    fs.copyFileSync(file, outputPath);
  }

  log('[assets:img]', 'Image optimization completed');
  notifier.notify({ title: 'Task was completed', message: 'assets:img task complete' });
});

gulp.task('assets:jsoneditor-img', function() {
  return gulp.src('../node_modules/jsoneditor/dist/img/**/*')
    .pipe(gulp.dest(`${custom.destPaths.images}/img`));
});

gulp.task('assets:css', function() {
  const postcssPlugins = [
    autoprefixer(),
    cssnano({ preset: ['default', { zindex: false }] })
  ];

  return gulp.src(custom.paths.styles, { sourcemaps: true, allowEmpty: true })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe(sassCompiler({ precision: 10 }).on('error', sassCompiler.logError))
  .pipe($.replace(/url\('/g, 'url(\'images/'))
  .pipe(postcss(postcssPlugins))
  .pipe($.concat('app.css'))
  .pipe(gulp.dest(custom.destPaths.styles, { sourcemaps: '.' }))
  .pipe(bs.stream({ match: '**/*.css' }))
  .pipe($.size({ title: 'assets:css', showFiles: true }))
  .pipe($.notify({ title: 'Task was completed', message: 'assets:css task complete', onLast: true }));
});

gulp.task('assets-logon:css', function() {
  const postcssPlugins = [
    autoprefixer(),
    cssnano({ preset: ['default', { zindex: false }] })
  ];

  return gulp.src(custom.paths.stylesLogon, { sourcemaps: true, allowEmpty: true })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe(sassCompiler({ precision: 10 }).on('error', sassCompiler.logError))
  .pipe($.replace(/url\('/g, 'url(\'images/'))
  .pipe(postcss(postcssPlugins))
  .pipe($.concat('app-logon.css'))
  .pipe(gulp.dest(custom.destPaths.styles, { sourcemaps: '.' }))
  .pipe(bs.stream({ match: '**/*.css' }))
  .pipe($.size({ title: 'assets-logon:css', showFiles: true }))
  .pipe($.notify({ title: 'Task was completed', message: 'assets-logon:css task complete', onLast: true }));
});

gulp.task('clean', function() {
  return del(custom.paths.clean);
});

gulp.task('browserSync', function(done) {
  bs.init({
    proxy: 'http://localhost:90/Backend',
    files: custom.paths.styles
  }, done);
});

gulp.task('watch', function() {
  const reportOnChange = function(filePath) {
    global.console.log(
      `File ${filePath} was changed,
      ${chalk.underline.bgCyan('running tasks...')}`
    );
  };

  return gulp.watch(custom.paths.styles, gulp.series('assets:css')).on('change', reportOnChange);
});

gulp.task('serve', gulp.series('watch', 'browserSync'));
gulp.task('start', gulp.series('clean', function(done) {
  const welcomeMsg = `\nGulp tasks were started in ${chalk.blue.underline.yellow(custom.config.environment)} mode.\n`;

  global.console.log(welcomeMsg);
  notifier.notify({ title: welcomeMsg, message: 'gulp is running' });
  done();
}));

module.exports.default = gulp.series('start', 'assets:js', 'assets-logon:js', 'assets:css', 'assets-logon:css', 'assets:img', 'assets:jsoneditor-img');

// Install Steps:
// 1. Install external node js and npm from official site
// 2. Install global packages for npm runner: gulp, cross-env
// 3. Set priority for VS: https://blogs.msdn.microsoft.com/webdev/2015/03/19/customize-external-web-tools-in-visual-studio-2015/
