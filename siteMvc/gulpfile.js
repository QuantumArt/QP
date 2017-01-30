var fs = require('fs');
var del = require('del');
var gulp = require('gulp');
var chalk = require('chalk');
var bs = require('browser-sync');
var es6Promise = require('es6-promise');
es6Promise.polyfill();

var $ = require('gulp-load-plugins')();
var argv = require('yargs').argv;
var project = JSON.parse(fs.readFileSync('../package.json'));


var assemblyInfoFileContent = fs.readFileSync('./properties/AssemblyInfo.cs');
var assembly = $.dotnetAssemblyInfo.getAssemblyMetadata(assemblyInfoFileContent);

var config = {
  name: project.name,
  version: project.version,
  assemblyVersion: assembly.AssemblyVersion,
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
  scripts1: [
    //'Scripts/Quantumart/**/*.js',
    'Scripts/es5-shim.js',
    'Scripts/jquery/jquery-1.7.1.js',
    'Scripts/telerik/telerik.common.js',
    'Scripts/polyfills/object.assign.polyfill.js',
    'Scripts/polyfills/array.includes.polyfill.js',
    'Scripts/polyfills/array.find.polyfill.js',
    'Scripts/polyfills/console.fns.polyfill.js',
    'Scripts/Quantumart/jQueryExtensions.js',
    'Scripts/microsoft/MicrosoftAjax.js',
    'Scripts/json2.js',
    'Scripts/Silverlight.js',
    'Scripts/pmrpc.js',
    'Scripts/underscore.js',
    'Scripts/knockout.js',
    'Scripts/immutablejs/immutable.min.js',

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
    'Scripts/jquery/jquery.signalR-2.2.0.js',

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
    'Scripts/telerik/telerik.splitter.js'
  ],
  scripts2: [
    'Scripts/Quantumart/Helpers/vanilla.helpers.js',

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

    'Scripts/PlUpload/moxie.js',
    'Scripts/PlUpload/plupload.dev.js',
    'Scripts/Quantumart/Uploader/plupload/plupload.filters.js',

    'Content/ckeditor/ckeditor.js',
    'Scripts/Quantumart/ckeditor/aspell/plugin.js',
    'Scripts/Quantumart/ckeditor/typographer/plugin.js',
    'Scripts/Quantumart/ckeditor/globalSettings.js',

    'Scripts/Quantumart/Utils.js',
    'Scripts/Quantumart/Cache.js',
    'Scripts/Quantumart/Common.js',
    'Scripts/Quantumart/ControlHelpers.js',
    'Scripts/Quantumart/Info/BackendEntityType.js',
    'Scripts/Quantumart/Info/BackendEntityObject.js',
    'Scripts/Quantumart/Info/BackendActionType.js',
    'Scripts/Quantumart/BackendActionExecutor.js',
    'Scripts/Quantumart/BackendSplitter.js',
    'Scripts/Quantumart/BackendContextMenu.js',
    'Scripts/Quantumart/BackendBreadCrumbs.js',
    'Scripts/Quantumart/Tree/BackendTreeBase.js',
    'Scripts/Quantumart/Tree/BackendTreeMenu.js',
    'Scripts/Quantumart/Tree/BackendEntityTree.js',
    'Scripts/Quantumart/Tree/BackendActionPermissionTree.js',
    'Scripts/Quantumart/BackendTabStrip.js',
    'Scripts/Quantumart/Toolbar/BackendToolbar.js',
    'Scripts/Quantumart/Toolbar/BackendActionToolbar.js',
    'Scripts/Quantumart/Toolbar/BackendViewToolbar.js',
    'Scripts/Quantumart/Search/BackendSearchBlockBase.js',
    'Scripts/Quantumart/Search/BackendSearchInArticle.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/FieldSearchBase.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/IdentifierFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/BooleanFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/DateOrTimeRangeFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/NumericRangeFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/RelationFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/TextFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/StringEnumFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/ClassifierFieldSearch.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/FullTextBlock.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/FieldSearchBlock.js',
    'Scripts/Quantumart/Search/BackendArticleSearchBlock/FieldSearchContainer.js',
    'Scripts/Quantumart/Search/BackendContentSearchBlock.js',
    'Scripts/Quantumart/Search/BackendUserSearchBlock.js',
    'Scripts/Quantumart/Search/BackendUserAndGroupSearchBlock.js',
    'Scripts/Quantumart/Search/BackendContextBlock.js',
    'Scripts/Quantumart/Document/BackendDocumentHostStateStorage.js',
    'Scripts/Quantumart/Document/BackendDocumentHost.js',
    'Scripts/Quantumart/Document/BackendDocumentContext.js',
    'Scripts/Quantumart/Document/BackendEditingDocument.js',
    'Scripts/Quantumart/Document/BackendEditingArea.js',
    'Scripts/Quantumart/Document/BackendPopupWindow.js',
    'Scripts/Quantumart/BackendEntityGrid.js',
    'Scripts/Quantumart/List/BackendEntityDataListBase.js',
    'Scripts/Quantumart/List/BackendEntityDropDownList.js',
    'Scripts/Quantumart/List/BackendEntityCheckBoxList.js',
    'Scripts/Quantumart/List/BackendEntitySingleItemPicker.js',
    'Scripts/Quantumart/List/BackendEntityMultipleItemPicker.js',
    'Scripts/Quantumart/List/BackendSelectPopupWindow.js',
    'Scripts/Quantumart/List/BackendSettingsPopupWindow.js',
    'Scripts/Quantumart/BackendActionLink.js',
    'Scripts/Quantumart/BackendLogOnWindow.js',
    'Scripts/Quantumart/Library/BackendLibrary.js',
    'Scripts/Quantumart/Library/BackendPager.js',
    'Scripts/Quantumart/Library/BackendFileList.js',
    'Scripts/Quantumart/Library/BackendFileNameListView.js',
    'Scripts/Quantumart/Library/BackendFilePreviewListView.js',
    'Scripts/Quantumart/Mediators/BackendFieldProperties.js',
    'Scripts/Quantumart/Editor/BackendFileField.js',
    'Scripts/Quantumart/Mediators/BackendVirtualContentProperties.js',
    'Scripts/Quantumart/Mediators/ContentDefaultFilters.js',
    'Scripts/Quantumart/Editor/BackendVirtualFieldTree.js',
    'Scripts/Quantumart/Editor/BackendEntityEditor.js',
    'Scripts/Quantumart/Mediators/BackendNotificationProperties.js',
    'Scripts/Quantumart/Mediators/BackendTemplateObjectProperties.js',
    'Scripts/Quantumart/Mediators/BackendPagePropertiesMediator.js',
    'Scripts/Quantumart/Editor/LibraryPopupWindow.js',
    'Scripts/Quantumart/Editor/BackendVisualEditor.js',
    'Scripts/Quantumart/Editor/BackendAggregationList.js',
    'Scripts/Quantumart/Editor/BackendWorkflowEditor.js',
    'Scripts/Quantumart/Editor/BackendHightedTextAreaEditor.js',
    'Scripts/Quantumart/Editor/BackendEditorsAutoSaver.js',
    'Scripts/Quantumart/Editor/BackendClassifierField.js',
    'Scripts/Quantumart/Uploader/IBackendUploader.js',
    'Scripts/Quantumart/Uploader/BackendHtmlUploader.js',
    'Scripts/Quantumart/Uploader/BackendPlUploader.js',
    'Scripts/Quantumart/Uploader/BackendSilverlightUploader.js',
    'Scripts/Quantumart/Audit/ActionLogComponent.js',
    'Scripts/Quantumart/Audit/ActionLogFilterTile.js',
    'Scripts/Quantumart/Audit/ActionLogFilterBase.js',
    'Scripts/Quantumart/Audit/ActionLogTextFilter.js',
    'Scripts/Quantumart/Audit/ActionLogDatetimeFilter.js',
    'Scripts/Quantumart/Audit/ActionLogItemListFilter.js',
    'Scripts/Quantumart/Audit/ActionLogUserFilter.js',
    'Scripts/Quantumart/SearchInCodeComponent.js',
    'Scripts/Quantumart/CustomAction/CustomActionProperties.js',
    'Scripts/Quantumart/CustomAction/QP8BackendApi.Interaction.js',
    'Scripts/Quantumart/CustomAction/BackendCustomActionHost.js',
    'Scripts/Quantumart/ActionPermissions/BackendActionPermissionView.js',
    'Scripts/Quantumart/Managers/BackendBreadCrumbsManager.js',
    'Scripts/Quantumart/Managers/BackendEntityGridManager.js',
    'Scripts/Quantumart/Managers/BackendEntityTreeManager.js',
    'Scripts/Quantumart/Managers/BackendEntityEditorManager.js',
    'Scripts/Quantumart/Managers/BackendTreeMenuContextMenuManager.js',
    'Scripts/Quantumart/Managers/BackendBreadMenuContextMenuManager.js',
    'Scripts/Quantumart/Managers/BackendContextMenuManager.js',
    'Scripts/Quantumart/Managers/BackendEntityDataListManager.js',
    'Scripts/Quantumart/Managers/BackendPopupWindowManager.js',
    'Scripts/Quantumart/Managers/BackendLibraryManager.js',
    'Scripts/Quantumart/Managers/BackendSearchBlockManager.js',
    'Scripts/Quantumart/Managers/BackendActionPermissionViewManager.js',
    'Scripts/Quantumart/Managers/BackendCustomActionHostManager.js',
    'Scripts/Quantumart/MultistepAction/BackendProgressBar.jquery.js',
    'Scripts/Quantumart/MultistepAction/BackendMultistepActionWindow.js',
    'Scripts/Quantumart/MultistepAction/Settings/BackendMultistepBaseSettings.js',
    'Scripts/Quantumart/MultistepAction/Settings/BackendMultistepImportSettings.js',
    'Scripts/Quantumart/MultistepAction/Settings/BackendMultistepExportSettings.js',
    'Scripts/Quantumart/MultistepAction/Settings/BackendMultistepCopySiteSettings.js',
    'Scripts/Quantumart/EntityPermissions/BackendChildEntityPermissionList.js',
    'Scripts/Quantumart/BackendDirectLinkExecutor.js',
    'Scripts/Quantumart/BackendExpandedContainer.js',
    'Scripts/Quantumart/BackendLogin.js',
    'Scripts/Quantumart/Backend.js',
    'Scripts/Quantumart/BackendImageCropResizeClient.js',
    'Scripts/Quantumart/Home.js',

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

gulp.task('lint-jshint', function() {
  return gulp.src(paths.scripts)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.jshint())
    .pipe($.jshint.reporter('jshint-stylish', { verbose: true }))
    .pipe($.jshint.reporter('fail'));
});

gulp.task('lint-jscs', function() {
  return gulp.src(paths.scripts)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.jscs())
    .pipe($.jscs.reporter())
    .pipe($.jscs.reporter('fail'));
});

gulp.task('lint-jscs-fix', function() {
  return gulp.src(paths.scripts, { base: './' })
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.jscs({ fix: true }))
    .pipe($.jscs.reporter())
    .pipe(gulp.dest('.'))
    .pipe($.notify({ message: 'assets:js task complete' }));
});


gulp.task('assets:revisions', function() {
  return gulp.src('Views/Home/Index.Template.cshtml')
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.replaceTask({
      patterns: [{
        match: 'version',
        replacement: config.assemblyVersion
      }]
    }))
    .pipe($.rename('Index.cshtml'))
    .pipe(gulp.dest('Views/Home/'));
});

gulp.task('assets:js', ['assets:js1', 'assets:js2'], function() {
  return gulp.src(destPaths.scripts)
    .pipe($.notify({ title: 'Task was completed', message: 'assets:js task complete', onLast: true }));
});

gulp.task('assets:js1', ['assets:revisions'], function() {
  return gulp.src(paths.scripts1, { base: './' })
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.sourcemaps.init({ loadMaps: true, debug: true }))
    .pipe($.rename({ suffix: '.min' }))
    .pipe($.uglify())
    .pipe($.concat('app1.min.js'))
    .pipe($.sourcemaps.write('maps'))
    .pipe(gulp.dest(destPaths.scripts))
    .pipe($.size({ title: 'assets:js', showFiles: true }))
    .pipe($.notify({ title: 'Part task was completed', message: 'assets:js1 task complete', onLast: true }));
});

gulp.task('assets:js2', ['assets:revisions'], function() {
  return gulp.src(paths.scripts2, { base: './' })
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.sourcemaps.init({ loadMaps: true, debug: true }))
    .pipe($.uglify())
    .pipe($.concat('app2.min.js'))
    .pipe($.sourcemaps.write('maps'))
    .pipe(gulp.dest(destPaths.scripts))
    .pipe($.size({ title: 'assets:js', showFiles: true }))
    .pipe($.notify({ title: 'Part task was completed', message: 'assets:js2 task complete', onLast: true }));
});

gulp.task('assets:img', function() {
  return gulp.src(paths.images)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.newer(destPaths.images))
    .pipe($.imagemin({ optimizationLevel: 3, progessive: true, interlaced: true }))
    .pipe(gulp.dest(destPaths.images))
    .pipe($.notify({ title: 'Task was completed', message: 'assets:img task complete', onLast: true }));
});

gulp.task('assets:css', ['assets:revisions'], function() {
  return gulp.src(paths.styles)
    .pipe($.plumber({ errorHandler: reportError }))
    .pipe($.sourcemaps.init({ loadMaps: true, debug: true }))
    .pipe($.sass().on('error', bs.notify))
    .pipe($.replace(/url\(\'/g, 'url(\'images/'))
    .pipe($.autoprefixer({ browsers: AUTOPREFIXER_BROWSERS }))
    .pipe($.cssnano({ zindex: false }))
    .pipe($.concat('app.min.css'))
    .pipe($.sourcemaps.write('maps'))
    .pipe(gulp.dest(destPaths.styles))
    .pipe(bs.stream({ match: '**/*.css' }))
    .pipe($.size({ title: 'assets:css', showFiles: true }))
    .pipe($.notify({ title: 'Task was completed', message: 'assets:css task complete', onLast: true }));
});




gulp.task('clean', function() {
  return del(paths.clean);
});

gulp.task('browserSync', function() {
  bs.init([paths.styles], {
    proxy: 'http://localhost:90/Backend'
  });
});

gulp.task('watch', function() {
  var reportOnChage = function(event) {
    console.log('File ' + event.path + ' was ' + event.type + ', ' + chalk.underline.bgCyan('running tasks...'));
  };

  gulp.watch(paths.styles, ['assets:css']).on('change', reportOnChage);
});

gulp.task('serve', ['watch', 'browserSync']);


gulp.task('default', ['clean'], function() {
  var welcomeMsg = '\nGulp tasks were started in ' + chalk.blue.underline.yellow(config.environment) + ' mode. Version: ' + config.assemblyVersion + '.';
  console.log(welcomeMsg);
  $.notify({ title: welcomeMsg, message: 'gulp is running' }); // TODO: ne pashet

  gulp.start('assets:js', 'assets:css', 'assets:img');
});

module.exports = gulp;

// Install Steps:
// 1. Install external node js and npm from official site
// 2. Install global packages for npm runner: gulp, cross-env
// 3. Set priority for VS: https://blogs.msdn.microsoft.com/webdev/2015/03/19/customize-external-web-tools-in-visual-studio-2015/
