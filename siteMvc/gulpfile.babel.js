/* eslint-env node */

import fs from 'fs';
import del from 'del';
import gulp from 'gulp';
import chalk from 'chalk';
import bs from 'browser-sync';
import { argv } from 'yargs';
import loadPlugins from 'gulp-load-plugins';
import es6Promise from 'es6-promise';
import notifier from 'node-notifier';

es6Promise.polyfill();
const $ = loadPlugins(); // eslint-disable-line id-length, no-shadow

const project = JSON.parse(fs.readFileSync('../package.json'));
const assemblyInfo = fs.readFileSync('./properties/AssemblyInfo.cs');
const assemblyMetadata = $.dotnetAssemblyInfo.getAssemblyMetadata(assemblyInfo);

const custom = {
  project,
  assemblyInfo,
  assemblyMetadata,
  config: {
    name: project.name,
    version: project.version,
    assemblyVersion: assemblyMetadata.AssemblyVersion,
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
    'Scripts/jquery/jquery-1.7.1.js',
    'Scripts/telerik/telerik.common.js',
    'Scripts/polyfills/object.assign.polyfill.js',
    'Scripts/polyfills/array.includes.polyfill.js',
    'Scripts/polyfills/array.find.polyfill.js',
    'Scripts/polyfills/console.fns.polyfill.js',
    'Scripts/Quantumart/jQueryExtensions.js',
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
    'Scripts/microsoft/MicrosoftAjax.js',
  ],
  qpjs: [
    'Scripts/Quantumart/Helpers/vanilla.helpers.js',
    'Scripts/Quantumart/Uploader/plupload/plupload.filters.js',

    'Scripts/Quantumart/ckeditor/aspell/plugin.js',
    'Scripts/Quantumart/ckeditor/typographer/plugin.js',
    'Scripts/Quantumart/ckeditor/codemirror/plugin.js',
    'Scripts/Quantumart/ckeditor/globalSettings.js',

    'Scripts/Quantumart/App.js',
    'Scripts/Quantumart/Utils.js',
    'Scripts/Quantumart/Cache.js',
    'Scripts/Quantumart/ControlHelpers.js',
    'Scripts/Quantumart/Common/IObserver.js',
    'Scripts/Quantumart/Common/IObservable.js',
    'Scripts/Quantumart/Common/IMediator.js',
    'Scripts/Quantumart/Common/BackendPreviousAction.js',
    'Scripts/Quantumart/Common/BackendEventArgs.js',
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
    'Scripts/Quantumart/Editor/BackendTextAreaEditor.js',
    'Scripts/Quantumart/Editor/BackendEditorsAutoSaver.js',
    'Scripts/Quantumart/Editor/BackendClassifierField.js',
    'Scripts/Quantumart/Uploader/BackendBaseUploader.js',
    'Scripts/Quantumart/Uploader/BackendHtmlUploader.js',
    'Scripts/Quantumart/Uploader/BackendPlUploader.js',
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
  qpjsLogon: [
    'Scripts/Quantumart/App.js',
    'Scripts/Quantumart/ControlHelpers.js',
    'Scripts/Quantumart/Common/IObserver.js',
    'Scripts/Quantumart/Common/IObservable.js',
    'Scripts/Quantumart/Common/IMediator.js',
    'Scripts/Quantumart/Common/BackendPreviousAction.js',
    'Scripts/Quantumart/Common/BackendEventArgs.js',
    'Scripts/Quantumart/BackendLogin.js'
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

gulp.task('assets:revisions', () => gulp.src([
  'Views/Home/Index.Template.cshtml',
  'Views/LogOn/Index.Template.cshtml',
  '../winLogonMvc/Views/WinLogOn/Index.Template.cshtml'
], { base: './' })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.replaceTask({
    patterns: [{
      match: 'version',
      replacement: custom.config.assemblyVersion
    }]
  }))
  .pipe($.rename({ basename: 'Index' }))
  .pipe(gulp.dest('.'))
);

gulp.task('assets:js', ['assets:vendorsjs', 'assets:qpjs'], () => gulp.src(custom.destPaths.scripts)
  .pipe($.notify({ title: 'Task was completed', message: 'assets:js task complete', onLast: true })));

gulp.task('assets-logon:js', ['assets-logon:vendorsjs', 'assets-logon:qpjs'], () => gulp.src(custom.destPaths.scripts)
  .pipe($.notify({ title: 'Task was completed', message: 'assets-logon:js task complete', onLast: true })));

gulp.task('assets:vendorsjs', ['assets:revisions'], () => gulp.src(custom.paths.vendorsjs, { base: './' })
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
  .pipe($.sourcemaps.write('maps'))
  .pipe(gulp.dest(custom.destPaths.scripts))
  .pipe($.size({ title: 'assets:vendorsjs', showFiles: true }))
  .pipe($.notify({ title: 'Part task was completed', message: 'assets:vendorsjs task complete', onLast: true })));

gulp.task('assets-logon:vendorsjs', ['assets:revisions'], () => gulp.src(custom.paths.vendorsjsLogon, { base: './' })
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
  .pipe($.sourcemaps.write('maps'))
  .pipe(gulp.dest(custom.destPaths.scripts))
  .pipe($.size({ title: 'assets-logon:vendorsjs', showFiles: true }))
  .pipe($.notify({
    title: 'Part task was completed',
    message: 'assets-logon:vendorsjs task complete',
    onLast: true
  })));

gulp.task('assets:qpjs', ['assets:revisions'], () => gulp.src(custom.paths.qpjs, { base: './' })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.babel())
  .pipe(custom.isProduction() ? $.uglify({
    compress: {
      sequences: false
    }
  }) : $.util.noop())
  .pipe($.concat('app.js'))
  .pipe($.sourcemaps.write('maps'))
  .pipe(gulp.dest(custom.destPaths.scripts))
  .pipe($.size({ title: 'assets:qpjs', showFiles: true }))
  .pipe($.notify({ title: 'Part task was completed', message: 'assets:qpjs task complete', onLast: true })));

gulp.task('assets-logon:qpjs', ['assets:revisions'], () => gulp.src(custom.paths.qpjsLogon, { base: './' })
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.babel())
  .pipe(custom.isProduction() ? $.uglify({
    compress: {
      sequences: false
    }
  }) : $.util.noop())
  .pipe($.concat('app-logon.js'))
  .pipe($.sourcemaps.write('maps'))
  .pipe(gulp.dest(custom.destPaths.scripts))
  .pipe($.size({ title: 'assets-logon:qpjs', showFiles: true }))
  .pipe($.notify({ title: 'Part task was completed', message: 'assets-logon:qpjs task complete', onLast: true })));

gulp.task('assets:img', () => gulp.src(custom.paths.images)
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.newer(custom.destPaths.images))
  .pipe($.imagemin({ optimizationLevel: 3, progessive: true, interlaced: true }))
  .pipe(gulp.dest(custom.destPaths.images))
  .pipe($.notify({ title: 'Task was completed', message: 'assets:img task complete', onLast: true })));

gulp.task('assets:css', ['assets:revisions'], () => gulp.src(custom.paths.styles)
  .pipe($.plumber({ errorHandler: custom.reportError }))
  .pipe($.sourcemaps.init({ loadMaps: false }))
  .pipe($.sourcemaps.identityMap())
  .pipe($.sass({ precision: 10 }).on('error', /* $.sass.logError */bs.notify))
  .pipe($.replace(/url\('/g, 'url(\'images/'))
  .pipe($.autoprefixer())
  .pipe($.cssnano({ zindex: false }))
  .pipe($.concat('app.css'))
  .pipe($.sourcemaps.write('maps'))
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
  .pipe($.sourcemaps.write('maps'))
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
  const welcomeMsg = `\nGulp tasks were started in ${chalk.blue.underline.yellow(custom.config.environment)} mode.
    Version: ${custom.config.assemblyVersion}.
  `;

  global.console.log(welcomeMsg);
  notifier.notify({ title: welcomeMsg, message: 'gulp is running' });
  gulp.start('assets:js', 'assets-logon:js', 'assets:css', 'assets-logon:css', 'assets:img');
});

module.exports = gulp;

// Install Steps:
// 1. Install external node js and npm from official site
// 2. Install global packages for npm runner: gulp, cross-env
// 3. Set priority for VS: https://blogs.msdn.microsoft.com/webdev/2015/03/19/customize-external-web-tools-in-visual-studio-2015/
