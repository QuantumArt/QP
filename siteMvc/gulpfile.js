var gulp = require('gulp');
var jscs = require('gulp-jscs');
var jshint = require('gulp-jshint');
var jshintStyle = require('jshint-stylish');
var jscsStyle = require('gulp-jscs-stylish');

var quantumArtScripts = [
  './Scripts/PlUpload/plupload.quantum.ext.js'
];

var quantumArtConfigs = [
  '.jscsrc',
  '.jshintrc',
  'gulpfile.js'
];

gulp.task('simple-jshint', function () {
  return gulp.src(quantumArtScripts)
    .pipe(jshint())
    .pipe(jshint.reporter(jshintStyle, { verbose: true }))
    .pipe(jshint.reporter('fail'));
});

gulp.task('simple-jscs', function () {
  return gulp.src(quantumArtScripts)
    .pipe(jscs())
    .pipe(jscs.reporter())
    .pipe(jscs.reporter('fail'));
});
