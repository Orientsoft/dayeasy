"use strict";
var gulp = require('gulp'),
    gulpLoadPlugins = require('gulp-load-plugins'),
    plugins = gulpLoadPlugins(),
    SRC_BASE = 'source/',
    SRC_CSS = [SRC_BASE + 'css/**/*.css', '!' + SRC_BASE + 'css/base.css'],
    SRC_JS = SRC_BASE + 'js/**/*.js',
//SRC_PLUGS = SRC_BASE + '{name}/**.js',
    DEST_CSS = 'css',
    DEST_JS = 'js',
    SRC_CSS_MENTION = SRC_BASE + 'mention/style/css/**/*.css',
    HTML_SRC = ['html/v2.0/**/*.html', '!html/v2.0/template/*.html'],
    HTML_DEST = 'html/';
//scripts
gulp.task("scripts", function () {
    return gulp.src(SRC_JS)
        .pipe(plugins.uglify())
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(gulp.dest(DEST_JS));
});

gulp.task('plugs',function(){
    return gulp.src('source/plugs/*.js')
        .pipe(plugins.uglify())
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(gulp.dest('plugs'));
});

//styles
gulp.task('styles', function () {
    return gulp.src(SRC_CSS)
        .pipe(plugins.autoprefixer('last 2 version', 'safari 5', 'ie 8', 'ie 9', 'opera 12.1', 'ios 6', 'android 4'))
        //.pipe(gulp.dest(CSS_DEST))
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.minifyCss())
        .pipe(gulp.dest(DEST_CSS));
});

//mention styles
gulp.task('mention-styles', function () {
    return gulp.src(SRC_CSS_MENTION)
        .pipe(plugins.autoprefixer('last 2 version', 'safari 5', 'ie 8', 'ie 9', 'opera 12.1', 'ios 6', 'android 4'))
        //.pipe(gulp.dest(CSS_DEST))
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.minifyCss())
        .pipe(gulp.dest(DEST_CSS));
});

// clean
gulp.task('clean', function () {
    return gulp.src([DEST_CSS, DEST_JS], {read: false})
        .pipe(plugins.clean());
});

gulp.task('default', function () {
    return gulp.start('scripts', 'styles');
});


//html @@
gulp.task('html', function () {
    return gulp.src(HTML_SRC)
        .pipe(plugins.fileInclude({
            prefix: '@@',
            basepath: 'html/v2.0/template'
        }))
        //.pipe(plugins.htmlmin({collapseWhitespace: true}))
        .pipe(gulp.dest(HTML_DEST))
        //.pipe(plugins.notify({message: 'html task complete'}));
});


//watch
gulp.task('watch', function() {
    gulp.watch(SRC_CSS, ['styles']);
    gulp.watch(SRC_JS, ['scripts']);
    gulp.watch(HTML_SRC, ['html']);
});





//browser-sync start --server  --files "**"