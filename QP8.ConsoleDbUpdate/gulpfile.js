/// <binding AfterBuild='default' Clean='clean' />

var fs = require('fs');
var del = require('del');
var gulp = require('gulp');
var chalk = require('chalk');
var es6Promise = require('es6-promise');
es6Promise.polyfill();

var $ = require('gulp-load-plugins')();
var argv = require('yargs').argv;
var project = JSON.parse(fs.readFileSync('../package.json'));

var config = {
  name: project.name,
  version: project.version,
  environment: 'development',
  commit: process.env.BUILD_SOURCEVERSION || '0',
  branchName: process.env.BUILD_SOURCEBRANCHNAME || '',
  buildNumber: process.env.BUILD_BUILDNUMBER || ''
};

// Nodejs arguments
if (process.env.NODE_ENV && (process.env.NODE_ENV.toLowerCase() === 'production' || process.env.NODE_ENV.toLowerCase() === 'release')) {
  config.environment = 'production';
}

// Gulp arguments
if (argv.env && (argv.env.toLowerCase() === 'production' || argv.env.toLowerCase() === 'release')) {
  config.environment = 'production';
}

var reportError = function(error) {
  $.notify({
    title: 'Task Failed [' + error.plugin  + ']',
    message: error.lineNumber ? 'Line: ' + error.lineNumber + ' -- ' : 'See console.',
    sound: 'Sosumi'
  }).write(error);

  var report = '';
  report += chalk.underline.bgRed('Task:') + ' [' + chalk.underline.bgCyan(error.plugin) + ']\n';
  if (error.fileName) {
    report += chalk.underline.bgRed('FileName:') + ' ' + chalk.underline.bgCyan(error.fileName) + '\n';
  }

  if (error.lineNumber) {
    report += chalk.underline.bgRed('LineNumber:') + ' ' + chalk.underline.bgCyan(error.lineNumber) + '\n';
  }

  report += chalk.underline.bgRed('Message:') + ' ' + error.message.replace('Error:', chalk.underline.bgRed('Error:')) + '\n';
  console.error(report);
  this.emit('end');
};

gulp.task('assets:revisions', function() {
  var assemblyFileVersion = config.version + '.' + 0;
  var assemblyDescription = 'Local builded at ' + new Date().toLocaleDateString();
  if (config.commit && config.buildNumber) {
    var splitted = config.buildNumber.split('.');
    assemblyFileVersion = config.version + '.' + splitted[splitted.length - 1];
    assemblyDescription = 'Server builded at ' +  config.buildNumber + ', Sha: ' + config.commit;
  }

  // TODO: Add local build tag from gulp-git
  var assemblyInfo = assemblyFileVersion + '+' + config.branchName + '.Sha.' + config.commit;

  var assemblies = gulp.src('Properties/AssemblyInfo.cs')
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.dotnetAssemblyInfo({
      version: config.version,
      fileVersion: assemblyFileVersion,
      informationalVersion: assemblyInfo,
      description: assemblyDescription
    }))
    .pipe(gulp.dest('Properties/'));

  return assemblies;
});

gulp.task('default', function() {
  var welcomeMsg = '\nGulp tasks were started in ' + chalk.blue.underline.yellow(config.environment) + ' mode. Version: ' + config.version + '.';
  console.log(welcomeMsg);
  $.notify({ title: welcomeMsg, message: 'gulp is running' }); // TODO: ne pashet
  gulp.start('assets:revisions');
});

// Install Steps:
// 1. Install external node js and npm from official site
// 2. Install global packages for npm runner: gulp, cross-env
// 3. Set priority for VS: https://blogs.msdn.microsoft.com/webdev/2015/03/19/customize-external-web-tools-in-visual-studio-2015/
