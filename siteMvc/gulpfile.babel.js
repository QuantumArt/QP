/* eslint-env node */

import fs from 'fs';
import del from 'del';
import gulp from 'gulp';
import gutil from 'gulp-util';
import chalk from 'chalk';
import bs from 'browser-sync';
import { argv } from 'yargs';
import loadPlugins from 'gulp-load-plugins';
import es6Promise from 'es6-promise';
import notifier from 'node-notifier';
import webpack from 'webpack';

es6Promise.polyfill();
const $ = loadPlugins(); // eslint-disable-line id-length, no-shadow

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

if ($.util.env.production) {
  custom.config.environment = 'production';
}

custom.isProduction = function isProduction() {
  return custom.config.environment === 'production';
};

custom.destPaths = {
  scripts: 'Scripts/build',
  styles: 'Content/build',
  images: 'Content/build'
};

custom.paths = {
  vendorsjs: [
    'Scripts/es5-shim.js',
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

    'Scripts/jquery/jquery-1.7.1.js',
    'Scripts/telerik/telerik.common.js',
    'Scripts/microsoft/MicrosoftAjax.js',
    'Scripts/json2.js',
    'Scripts/pmrpc.js',
    'Scripts/underscore.js',
    'Scripts/knockout.js',
    'Scripts/immutablejs/immutable.js',

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
    'Scripts/jquery/jquery.signalR-2.2.1.js',

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

    'Content/ckeditor/ckeditor.js',
    'Content/jsoneditor/dist/jsoneditor.js',
    'Content/codemirror/lib/codemirror.js',
    'Content/codemirror/mode/clike/clike.js',
    'Content/codemirror/mode/sql/sql.js',
    'Content/codemirror/mode/xml/xml.js',
    'Content/codemirror/mode/css/css.js',
    'Content/codemirror/mode/vb/vb.js',
    'Content/codemirror/mode/javascript/javascript.js',
    'Content/codemirror/mode/htmlmixed/htmlmixed.js',
    'Content/codemirror/mode/htmlembedded/htmlembedded.js',
    'Content/codemirror/addon/mode/multiplex.js',
    'Content/jsoneditor/dist/jsoneditor.js',

    'Scripts/PlUpload/moxie.js',
    'Scripts/PlUpload/plupload.dev.js'
  ],
  vendorsjsLogon: [
    'Scripts/es5-shim.js',
    'Scripts/jquery/jquery-1.7.1.js',
    'Scripts/microsoft/MicrosoftAjax.js'
  ],
  styles: [
    'Content/basic.css',
    'Content/page.css',
    'Content/telerik.common.css',
    'Content/jquery.nouislider.css',
    'Content/jquery.qtip.css',
    'Content/imgCropResizeClient.css',
    'Content/imgareaselect.css',
    'Content/jquery.jeegoocontext.qp8.css',
    'Content/telerik.qp8.css',
    'Content/codemirror/lib/codemirror.css',
    'Content/codemirrorTheme.css',
    'Content/jsoneditor/dist/jsoneditor.css',
    'Content/QpCodemirror.css',
    'Content/custom/**/*.{scss,css}',

    '!Content/build/**/*.css'
  ],
  stylesLogon: [
    'Content/basic.css',
    'Content/page.css'
  ],
  images: [
    'Content/**/*.{jpg,jpeg,png,gif,svg}',
    '!Content/ckeditor/**/*.{jpg,jpeg,png,gif,svg}',
    '!Content/codemirror/**/*.{jpg,jpeg,png,gif,svg}',
    '!Content/build/**/*.{jpg,jpeg,png,gif,svg}',
    '!Content/build/**/*.{jpg,jpeg,png,gif,svg}'
  ],
  clean: [
    'Scripts/Quantumart/**/*.{min.js,map}',
    'Content/custom/**/*.{min.css,map}',
    custom.destPaths.scripts,
    custom.destPaths.styles,
    custom.destPaths.images
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

gulp.task('assets:js', ['assets:vendorsjs', 'webpack:qpjs'], () => gulp.src(custom.destPaths.scripts)
  .pipe($.notify({ title: 'Task was completed', message: 'assets:js task complete', onLast: true })));

gulp.task('assets-logon:js', ['assets-logon:vendorsjs', 'webpack:qpjs'], () => gulp.src(custom.destPaths.scripts)
  .pipe($.notify({ title: 'Task was completed', message: 'assets-logon:js task complete', onLast: true })));

gulp.task('assets:vendorsjs', () => gulp.src(custom.paths.vendorsjs, { base: './' })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.rename({ suffix: '.min' }))
  .pipe(custom.isProduction() ? $.uglify({
    compress: {
      sequences: false
    }
  }) : $.util.noop())
  .pipe($.concat('vendors.js'))
  .pipe($.sourcemaps.write(''))
  .pipe(gulp.dest(custom.destPaths.scripts))
  .pipe($.size({ title: 'assets:vendorsjs', showFiles: true }))
  .pipe($.notify({ title: 'Part task was completed', message: 'assets:vendorsjs task complete', onLast: true })));

gulp.task('assets-logon:vendorsjs', () => gulp.src(custom.paths.vendorsjsLogon, { base: './' })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.rename({ suffix: '.min' }))
  .pipe(custom.isProduction() ? $.uglify({
    compress: {
      sequences: false
    }
  }) : $.util.noop())
  .pipe($.concat('vendors-logon.js'))
  .pipe($.sourcemaps.write(''))
  .pipe(gulp.dest(custom.destPaths.scripts))
  .pipe($.size({ title: 'assets-logon:vendorsjs', showFiles: true }))
  .pipe($.notify({
    title: 'Part task was completed',
    message: 'assets-logon:vendorsjs task complete',
    onLast: true
  })));

gulp.task('webpack:qpjs', callback => {
  webpack(require('./webpack.config.js'), (err, stats) => {
    if (err) {
      throw new gutil.PluginError('webpack:qpjs', err);
    }
    gutil.log('[webpack:qpjs]', stats.toString({ colors: true }));
    notifier.notify({ title: 'Part task was completed', message: 'webpack:qpjs task complete' });
    callback();
  });
});

gulp.task('assets:img', () => gulp.src(custom.paths.images)
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.newer(custom.destPaths.images))
  .pipe($.imagemin({ optimizationLevel: 3, progessive: true, interlaced: true }))
  .pipe(gulp.dest(custom.destPaths.images))
  .pipe($.notify({ title: 'Task was completed', message: 'assets:img task complete', onLast: true })));

gulp.task('assets:css', () => gulp.src(custom.paths.styles)
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.sass({ precision: 10 }).on('error', /* $.sass.logError */bs.notify))
  .pipe($.replace(/url\('/g, 'url(\'images/'))
  .pipe($.autoprefixer())
  .pipe($.cssnano({ zindex: false }))
  .pipe($.concat('app.css'))
  .pipe($.sourcemaps.write(''))
  .pipe(gulp.dest(custom.destPaths.styles))
  .pipe(bs.stream({ match: '**/*.css' }))
  .pipe($.size({ title: 'assets:css', showFiles: true }))
  .pipe($.notify({ title: 'Task was completed', message: 'assets:css task complete', onLast: true })));

gulp.task('assets-logon:css', () => gulp.src(custom.paths.stylesLogon)
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.sass({ precision: 10 }).on('error', /* $.sass.logError */bs.notify))
  .pipe($.replace(/url\('/g, 'url(\'images/'))
  .pipe($.autoprefixer())
  .pipe($.cssnano({ zindex: false }))
  .pipe($.concat('app-logon.css'))
  .pipe($.sourcemaps.write(''))
  .pipe(gulp.dest(custom.destPaths.styles))
  .pipe(bs.stream({ match: '**/*.css' }))
  .pipe($.size({ title: 'assets-logon:css', showFiles: true }))
  .pipe($.notify({ title: 'Task was completed', message: 'assets-logon:css task complete', onLast: true })));

gulp.task('clean', () => del(custom.paths.clean));

gulp.task('browserSync', () => {
  bs.init([custom.paths.styles], {
    proxy: 'http://localhost:90/Backend'
  });
});

gulp.task('watch', () => {
  const reportOnChange = ev => global.console.log(
    `File ${ev.path} was ${ev.type},
    ${chalk.underline.bgCyan('running tasks...')}`
  );

  gulp.watch(custom.paths.styles, ['assets:css']).on('change', reportOnChange);
});

gulp.task('serve', ['watch', 'browserSync']);
gulp.task('default', ['clean'], () => {
  const welcomeMsg = `\nGulp tasks were started in ${chalk.blue.underline.yellow(custom.config.environment)} mode.\n`;

  global.console.log(welcomeMsg);
  notifier.notify({ title: welcomeMsg, message: 'gulp is running' });
  gulp.start('assets:js', 'assets-logon:js', 'assets:css', 'assets-logon:css', 'assets:img');
});

module.exports = gulp;

// Install Steps:
// 1. Install external node js and npm from official site
// 2. Install global packages for npm runner: gulp, cross-env
// 3. Set priority for VS: https://blogs.msdn.microsoft.com/webdev/2015/03/19/customize-external-web-tools-in-visual-studio-2015/
