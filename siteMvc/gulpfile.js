/// <binding AfterBuild='lint, images, styles' Clean='clean' />
var del = require('del');
var gulp = require('gulp');
var bs = require('browser-sync');
var gutil = require('gulp-util');
var es6Promise = require('es6-promise');
var gulpLoadPlugins = require('gulp-load-plugins');

es6Promise.polyfill();
var env = gutil.env.type || 'development';
var $ = gulpLoadPlugins();

var AUTOPREFIXER_BROWSERS = [
  'ie >= 8',
  '> 3%',
  'last 3 version'
];

var destPaths = {
  scripts: 'Scripts/build',
  styles: 'Content/build',
  images: 'Content/build'
  //images: 'Content/build/images'
};

var paths = {
  scripts: [
    'Scripts/Quantumart/**/*.js',
    '!Scripts/build/**/*.js'
  ],
  styles: [
    'Content/basic.css',
    'Content/page.css',
    'Content/telerik.common.css',
    'Content/jquery.nouislider.css',
    'Content/jquery.qtip.css',
    'Content/imgCropResizeClient.css',
    'Content/imgareaselect.css',
    'Content/version.6.3.css',
    'Content/jquery.jeegoocontext.qp8.css',
    'Content/telerik.qp8.css',
    'Content/codemirror/lib/codemirror.css',
    'Content/codemirrorTheme.css',
    'Content/QpCodemirror.css',
    'Content/Quantumart/**/.{sass, css}',
    '!Content/build/**/*.css'
  ],
  images: [
    'Content/**/*.{jpg,jpeg,png,gif}',
    '!Content/ckeditor/**/*.{jpg,jpeg,png,gif}',
    '!Content/codemirror/**/*.{jpg,jpeg,png,gif}',
    '!Content/build/**/*.{jpg,jpeg,png,gif}'
  ],
  clean: [
    'Scripts/Quantumart/**/*.{min.js, map}',
    'Content/Quantumart/**/*.{min.css, map}',
    destPaths.scripts,
    destPaths.styles,
    destPaths.images
  ]
};

var reportError = function(error) {
  var lineNumber = (error.lineNumber) ? 'LINE ' + error.lineNumber + ' -- ' : '';

  gutil.beep();
  $.notify({
    title: 'Task Failed [' + error.plugin + '] at [' + error.fileName + ']',
    message: lineNumber + 'See console.',
    sound: 'Sosumi'
  }).write(error);

  var report = '';
  var chalk = gutil.colors.white.bgRed;

  report += chalk('TASK:') + ' [' + error.plugin + ']\n';
  report += chalk('PROB:') + ' ' + error.message + '\n';

  if (error.lineNumber) {
    report += chalk('LINE:') + ' ' + error.lineNumber + '\n';
  }

  if (error.fileName) {
    report += chalk('FILE:') + ' ' + error.fileName + '\n';
  }

  console.error(report);
  this.emit('end');
};

gulp.task('clean', function() {
  return del(paths.clean);
});

gulp.task('browserSync', function() {
  var files = [paths.styles];

  bs.init(files, {
    proxy: 'http://localhost:90/Backend'
  });
});



gulp.task('lint-jshint', function() {
  return gulp.src(paths.scripts)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.jshint())
    .pipe($.jshint.reporter('jshint-stylish', { verbose: true }))
    .pipe($.jshint.reporter('fail'))
});

gulp.task('lint-jscs', function() {
  return gulp.src(paths.scripts)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.jscs())
    .pipe($.jscs.reporter())
    .pipe($.jscs.reporter('fail'))
});

gulp.task('lint-jscs-fix', function() {
  return gulp.src(paths.scripts, { base: './' })
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.jscs({ fix: true }))
    .pipe($.jscs.reporter())
    .pipe(gulp.dest('.'))
    .pipe($.notify({ message: 'assets:js task complete' }))
});



gulp.task('assets:js', function() {
  return gulp.src(paths.scripts, { base: './' })
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.sourcemaps.init({ loadMaps: true, debug: true }))
    .pipe($.rename({ suffix: '.min' }))
    .pipe($.uglify())
    .pipe($.sourcemaps.write('.'))
    .pipe(gulp.dest('.'))
    .pipe($.notify({ title: 'Task was completed', message: 'assets:img task complete', onLast: true }));
});

gulp.task('assets:img', function() {
  return gulp.src(paths.images)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.newer(destPaths.images))
    .pipe($.imagemin({ optimizationLevel: 3, progessive: true, interlaced: true }))
    .pipe(gulp.dest(destPaths.images))
    .pipe($.notify({ title: 'Task was completed', message: 'assets:img task complete', onLast: true }));
});

gulp.task('assets:css', function() {
  return gulp.src(paths.styles)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.sourcemaps.init({ loadMaps: true, debug: true }))
    .pipe($.sass().on('error', bs.notify))
    .pipe($.replace(/url\(\'/g, 'url(\'images/'))
    .pipe($.autoprefixer({ browsers: AUTOPREFIXER_BROWSERS }))
    //.pipe($.cssnano())
    .pipe($.concat('app.min.css'))
    .pipe($.sourcemaps.write('maps'))
    .pipe(gulp.dest(destPaths.styles))
    .pipe(bs.stream({ match: '**/*.css' }))
    .pipe($.size({ title: 'assets:css', showFiles: true }))
    .pipe($.notify({ title: 'Task was completed', message: 'assets:css task complete', onLast: true }));
});



gulp.task('watch', function() {
  var reportOnChage = function(event) {
    console.log('File ' + event.path + ' was ' + event.type + ', running tasks...');
  };

  gulp.watch(paths.styles, ['assets:css']).on('change', reportOnChage);
});

gulp.task('serve', ['watch', 'browserSync']);


gulp.task('default', ['clean'], function() {
  gutil.log('gulp is running!');
  $.notify({ title: 'default task was started', message: 'gulp is running' });
  gulp.start('assets:js', 'assets:css', 'assets:img');
});
