var gulp = require('gulp');
var jscs = require('gulp-jscs');
var jshint = require('gulp-jshint');
var uglify = require('gulp-uglify');
var notify = require('gulp-notify');
var cache = require('gulp-cache');
var sass = require('gulp-ruby-sass');
var cssnano = require('gulp-cssnano');
var autoprefixer = require('gulp-autoprefixer');

gulp.task('watch', function() {
  gulp.watch(quantumArtScripts, ['scripts']);
  //gulp.watch('src/styles/**/*.scss', ['styles']);
  //gulp.watch('src/images/**/*', ['images']);
});

gulp.task('default', ['clean'], function() {
    gulp.start('styles', 'scripts', 'images');
});

gulp.task('test-styles-1', function () {
  // Rebuild only files that change
  //  var watch = require('gulp-watch');
  //  return gulp.src('sass/*.scss')
  //    .pipe(watch('sass/*.scss'))
  //    .pipe(sass())
  //    .pipe(gulp.dest('dist'));
});

gulp.task('test-styles-2', function () {
  //return sass('src/styles/main.scss', { style: 'expanded' })
  //  .pipe(autoprefixer('last 2 version'))
  //  .pipe(gulp.dest('dist/assets/css'))
  //  .pipe(rename({suffix: '.min'}))
  //  .pipe(cssnano())
  //  .pipe(gulp.dest('dist/assets/css'))
  //  .pipe(notify({ message: 'Styles task complete' }));
});

gulp.task('test-scripts-1', function () {
  //return gulp.src('src/scripts/**/*.js')
  //  .pipe(jshint('.jshintrc'))
  //  .pipe(jshint.reporter('default'))
  //  .pipe(concat('main.js'))
  //  .pipe(gulp.dest('dist/assets/js'))
  //  .pipe(rename({suffix: '.min'}))
  //  .pipe(uglify())
  //  .pipe(gulp.dest('dist/assets/js'))
  //  .pipe(notify({ message: 'Scripts task complete' }));
});

gulp.task('test-images-1', function () {
  //return gulp.src('src/images/**/*')
  //  .pipe(imagemin({ optimizationLevel: 3, progressive: true, interlaced: true }))
  //  .pipe(gulp.dest('dist/assets/img'))
  //  .pipe(notify({ message: 'Images task complete' }));
});

// .pipe(uglify({ outSourceMap: true }))
// jscs --es3
// jshint --es3


var gulp       = require('gulp'),
    jshint     = require('gulp-jshint'),
    sass       = require('gulp-sass'),
    sourcemaps = require('gulp-sourcemaps');

gulp.task('build-css', function() {
  return gulp.src('source/scss/**/*.scss')
    .pipe(sourcemaps.init())  // Process the original sources
      .pipe(sass())
    .pipe(sourcemaps.write()) // Add the map to modified source.
    .pipe(gulp.dest('public/assets/stylesheets'));
});

var env = plugins.gutil.env.type || 'development';
gulp.task('build-js', function() {
  return gulp.src('source/javascript/**/*.js')
    .pipe(sourcemaps.init())
      .pipe(concat('bundle.js'))
      //only uglify if gulp is ran with '--type production'
      .pipe(gutil.env.type === 'production' ? uglify() : gutil.noop())
    .pipe(sourcemaps.write())
    .pipe(gulp.dest('public/assets/javascript'));
});

// Watch
gulp.task('watch', function() {

  // Watch .scss files
  gulp.watch('src/styles/**/*.scss', ['styles']);

  // Watch .js files
  gulp.watch('src/scripts/**/*.js', ['scripts']);

  // Watch image files
  gulp.watch('src/images/**/*', ['images']);

  // Create LiveReload server
  livereload.listen();

  // Watch any files in dist/, reload on change
  gulp.watch(['dist/**']).on('change', livereload.changed);

});




var gulp = require('gulp'),
      sass = require('gulp-ruby-sass');
      autoprefixer = require('gulp-autoprefixer'),
      minifycss = require('gulp-minify-css'),
      rename = require('gulp-rename'),
      concat = require('gulp-concat');


  gulp.task('process-styles', function() {
    return gulp.src('src/styles/main.scss')
      .pipe(sass({style: 'expanded'}))
      .pipe(autoprefixer('last 2 version'))
      .pipe(gulp.dest('dest/styles/'))
      .pipe(rename({suffix: '.min'} ))
      .pipe(minifycss())
      .pipe(gulp.dest('dest/styles/'))
  });








.pipe(uglify({ outSourceMap: true }))
.pipe(rename(function (path) {
    if(path.extname === '.js') {
        path.basename += '.min';
    }
}))















gulp-git
gulp-notify
gulp-strip-debug
gulp-bump



Use BrowserSync instead of LiveReload
http://stackoverflow.com/questions/29750358/browser-sync-gulp-setup-not-working-with-net-mvc
https://github.com/BrowserSync/browser-sync/issues/20
http://karloespiritu.com/cheatsheets/gulp/























































<Target Name="AfterBuild">
  <ItemGroup>
    <JS Include="**\*.js"
      Exclude="**\*.min.js;
               gulpfile.js;
               Content\ckeditor\*.js;
               Content\ckeditor\**\*.js;
               **\jquery-1.7.1.js;
               **\jquery.scrollTo-1.4.2.js;
               **\jquery.unobtrusive-ajax.js;
               **\jquery.validate.js;
               **\jquery.validate.unobtrusive.js;
               **\telerik.*.js;
               **\MicrosoftAjax.debug.js;
               **\MicrosoftAjax.js;
               **\MicrosoftMvcAjax.debug.js;
               **\MicrosoftMvcAjax.js;
               **\MicrosoftMvcValidation.js;
               **\MicrosoftMvcValidation.debug.js;
               **\CodeMirror\codemirror.min.js;
               **\jquery.imgareaselect.js;
               **\jquery.qtip.js;" />
  </ItemGroup>

  <ItemGroup>
    <CSS
      Include="**\*.css"
      Exclude="**\*.min.css;
               Content\ckeditor\*.css;
               Content\ckeditor\**\*.css"
      />
  </ItemGroup>
  <AjaxMin JsSourceFiles="@(JS)"
    JsSourceExtensionPattern="\.js$"
    JsTargetExtension=".min.js"
    JsCollapseToLiteral="true"
    JsEvalTreatment="MakeAllSafe"
    JsLocalRenaming="KeepLocalizationVars"
    JsMacSafariQuirks="true"
    JsRemoveUnneededCode="true"
    JsStripDebugStatements="true"
    JsOutputMode="SingleLine"
    CssSourceFiles="@(CSS)"
    CssSourceExtensionPattern="\.css$"
    CssTargetExtension=".min.css"
    CssColorNames="Hex"
    CssCommentMode="Hacks"
    CssTermSemicolons="true" />
</Target>

<Import Project="$(MSBuildExtensionsPath)\Microsoft\MicrosoftAjax\ajaxmin.tasks" />


xcopy "$(SolutionDir)dal\scripts\fix_dbo.sql" "$(ProjectDir)" /s /e /r /i /y

cd "$(ProjectDir)Content\ckeditor"

rd /s /q "$(ProjectDir)Content\ckeditor\skins\qp8"
xcopy "$(ProjectDir)Content\ckeditor\_source\skins\qp8" "$(ProjectDir)Content\ckeditor\skins\qp8" /s /e /r /i /y

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\linebreak"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\linebreak" "$(ProjectDir)Content\ckeditor\plugins\linebreak" /s /e /r /i /y
rd /s /q "$(ProjectDir)Content\ckeditor\plugins\linebreak\plugin.js"

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\cleaner"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\cleaner" "$(ProjectDir)Content\ckeditor\plugins\cleaner" /s /e /r /i /y
rd /s /q "$(ProjectDir)Content\ckeditor\plugins\cleaner\plugin.js"

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\image\dialogs"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\image\dialogs" "$(ProjectDir)Content\ckeditor\plugins\image\dialogs" /s /e /r /i /y

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\flash"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\flash" "$(ProjectDir)Content\ckeditor\plugins\flash" /s /e /r /i /y
rd /s /q "$(ProjectDir)Content\ckeditor\plugins\flash\plugin.js"

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\qspecchar"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\qspecchar" "$(ProjectDir)Content\ckeditor\plugins\qspecchar" /s /e /r /i /y
rd /s /q "$(ProjectDir)Content\ckeditor\plugins\qspecchar\plugin.js"

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\typographer"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\typographer" "$(ProjectDir)Content\ckeditor\plugins\typographer" /s /e /r /i /y
rd /s /q "$(ProjectDir)Content\ckeditor\plugins\typographer\plugin.js"

rd /s /q "$(ProjectDir)Content\ckeditor\plugins\removeEmptyP"
xcopy "$(ProjectDir)Content\ckeditor\_source\plugins\removeEmptyP" "$(ProjectDir)Content\ckeditor\plugins\removeEmptyP" /s /e /r /i /y
rd /s /q "$(ProjectDir)Content\ckeditor\plugins\removeEmptyP\plugin.js"

ckpackager.exe ckeditor_custom.pack
