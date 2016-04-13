/// <binding AfterBuild='lint, images, styles' Clean='clean' />
var del = require('del');
var gulp = require('gulp');
var gutil = require('gulp-util');
var gulpLoadPlugins = require('gulp-load-plugins');
var plugins = gulpLoadPlugins();

var env = gutil.env.type || 'development';

var paths = {
  scripts: [
    'Scripts/Quantumart/**/*.js',
    '!Scripts/Quantumart/**/*.min.js'
  ],
  styles: [
    'Content/**/*.css',
    '!Content/**/*.min.css'
  ],
  images: [
    'Content/**/*.png',
    'Content/**/*.jpg',
    'Content/**/*.gif'
  ],
  configs: {
    jscs: '.jscsrc',
    jshint: '.jshintrc',
    editor: '.editorconfig',
    gulp: 'gulpfile.js',
    npm: 'packages.json'
  },
  clean: [
    'Scripts/Quantumart/**/*.min.js',
    'Scripts/Quantumart/**/*.map'
  ]
};

var errorHandler = function(e) {
  gutil.log('Some errors were occured!', e);
  console.error(e);
};

gulp.task('lint-jshint', function() {
  return gulp.src(paths.scripts)
    .pipe(plugins.jshint())
    .pipe(plugins.jshint.reporter('jshint-stylish', { verbose: true }))
    .pipe(plugins.jshint.reporter('fail'))
    .on('error', errorHandler);
});

gulp.task('lint-jscs', function() {
  return gulp.src(paths.scripts)
    .pipe(plugins.jscs())
    .pipe(plugins.jscs.reporter())
    .pipe(plugins.jscs.reporter('fail'))
    .on('error', errorHandler);
});

gulp.task('lint-jscs-fix', function() {
  return gulp.src(paths.scripts, { base: './' })
    .pipe(plugins.jscs({ fix: true }))
    .pipe(plugins.jscs.reporter())
    .pipe(gulp.dest('.'))
    .on('error', errorHandler);
});

gulp.task('lint', ['lint-jshint', 'lint-jscs']);
gulp.task('styles', gutil.noop);
gulp.task('images', gutil.noop);
gulp.task('scripts', function() {
  return gulp.src(paths.scripts, { base: './' })
    .pipe(plugins.sourcemaps.init({ loadMaps: true }))
    .pipe(plugins.rename({ suffix: '.min' }))
    .pipe(plugins.uglify())
    .on('error', errorHandler)
    .pipe(plugins.sourcemaps.write('.'))
    .pipe(gulp.dest('.'));
});

gulp.task('clean', function() {
  return del(paths.clean);
});

gulp.task('watch', function() {
  gulp.watch(paths.scripts, ['lint-jshint', 'lint-jscs']);
});

gulp.task('default', ['clean'], function() {
  gutil.log('Gulp is running!');
  gulp.start('styles', 'scripts', 'images');
});
