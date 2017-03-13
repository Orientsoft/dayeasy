var gulp = require('gulp'),
    gulpLoadPlugins = require('gulp-load-plugins'),
    plugins = gulpLoadPlugins(),
    pngquant = require('imagemin-pngquant'),
    jpegtran = require('imagemin-jpegtran'),
    gifsicle = require('imagemin-gifsicle'),
    IMAGE_JPG_SRC = ["source/image/**/*.jpg"],
    IMAGE_GIF_SRC = ["source/image/**/*.gif"],
    IMAGE_PNG_SRC = ["source/image/**/*.png"],
    IMAGE_SRC = ["source/image/**/*"],
    IMAGE_DEST = "image",
    JS_DEST = "js",
    JS_SRC = 'source/js/**/*.js',
    CSS_SRC = "source/css/**/*.css",
    CSS_DEST = "css",

    HTML_SRC = ['source/html/**/*.html', '!source/html/template/*.html'],
    HTML_DEST = 'html/',
    LESS_SOURCE = 'source/css/**/*.less';
var PLUGS_DEST = "plugs",
    PLUGS_JS = 'source/plugs/**/*.js',
    PLUGS_CSS = "source/plugs/**/*.css",
    PLUGS_LESS = 'source/plugs/**/*.less';
var FONTICON_DEST = "fonticon",
    FONTICON_SRC = 'source/fonticon/**/*.css';
gulp.task("scripts", function () {
    gulp.src(JS_SRC)
        .pipe(plugins.uglify())
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(gulp.dest(JS_DEST));
    gulp.src(PLUGS_JS)
        .pipe(plugins.uglify())
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(gulp.dest(PLUGS_DEST));
});
gulp.task('less', function () {
    gulp.src(LESS_SOURCE)
        .pipe(plugins.less())
        .pipe(gulp.dest(function (f) {
            return f.base;
        }));

    gulp.src(PLUGS_LESS)
        .pipe(plugins.less())
        .pipe(gulp.dest(function (f) {
            return f.base;
        }));
});
gulp.task('less-watch', function () {
    return gulp.watch([LESS_SOURCE, PLUGS_LESS], ['less']);
});
gulp.task('styles', function () {
    gulp.src(CSS_SRC)
        .pipe(plugins.autoprefixer('last 2 version', 'safari 5', 'ie 8', 'ie 9', 'opera 12.1', 'ios 6', 'android 4'))
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.minifyCss())
        .pipe(gulp.dest(CSS_DEST));

    gulp.src(PLUGS_CSS)
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.minifyCss())
        .pipe(gulp.dest(PLUGS_DEST));

    gulp.src([FONTICON_SRC, '!source/fonticon/demo.css'])
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.minifyCss())
        .pipe(gulp.dest(FONTICON_DEST));

});
gulp.task('html', function () {
    return gulp.src(HTML_SRC)
        .pipe(plugins.fileInclude({
            prefix: '@@',
            basepath: 'source/html/template'
        }))
        .pipe(plugins.htmlmin({collapseWhitespace: true}))
        .pipe(gulp.dest(HTML_DEST))
});
gulp.task('images', ['clean-image'], function () {
    return gulp.start('jpg', 'gif', 'png');
});
gulp.task('watch', function () {
    gulp.watch([CSS_SRC, PLUGS_CSS], ['styles']);
    gulp.watch([JS_SRC, PLUGS_JS], ['scripts']);
    gulp.watch(HTML_SRC, ['html']);
    gulp.watch(IMAGE_SRC, ['images']);
    gulp.watch(['page/**']).on('change', function (file) {
        plugins.livereload.changed(file.path);
    });
});
gulp.task('default', function () {
    return gulp.start('scripts', 'styles', 'html', 'images');
});

gulp.task('help', function () {
    console.log('	gulp default			生产')
    console.log('	gulp less			less解析');
    console.log('	gulp less-watch			less监控');
    console.log('	gulp styles			css');
    console.log('	gulp script			js');
    console.log('	gulp html			html');
    console.log('	gulp watch			文件监控打包');
    console.log('	gulp help			gulp参数说明');
    console.log(' browser-sync start --server --files "**"    自动刷新');
});

