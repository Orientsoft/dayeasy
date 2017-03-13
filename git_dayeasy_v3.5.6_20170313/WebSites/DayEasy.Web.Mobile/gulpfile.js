/* = Gulp组件
 -------------------------------------------------------------- */

var gulp = require('gulp'),       // 基础库
// 引入我们的gulp组件
    gulpLoadPlugins = require('gulp-load-plugins'),
    plugins = gulpLoadPlugins(),
    pngquant = require('imagemin-pngquant'),
    jpegtran = require('imagemin-jpegtran'),
    gifsicle = require('imagemin-gifsicle');
/* = 全局设置
 -------------------------------------------------------------- */
var srcPath = {
    html: 'source/html',
    css: 'source/css',
    script: 'source/js',
    image: 'source/image',
    less: 'source/css'
};
var destPath = {
    html: '.',
    css: 'css',
    script: 'js',
    image: 'image'
};

/* = 开发环境( Plato Task )
 -------------------------------------------------------------- */
// HTML处理
gulp.task('html', function (){
    return gulp.src([srcPath.html + '/**/*.html', '!' + srcPath.html + '/template/*.html'])
        //        .pipe(plugins.changed(destPath.html))
        .pipe(plugins.fileInclude({
            prefix: '@@',
            basepath: 'source/html/template'
        }))
        //        .pipe(plugins.htmlmin({collapseWhitespace: true}))
        .pipe(gulp.dest(destPath.html))
});


gulp.task('htmlrelease', function (){
    return gulp.src([srcPath.html + '/**/*.html', '!' + srcPath.html + '/template/*.html'])
        //        .pipe(plugins.changed(destPath.html))
        .pipe(plugins.fileInclude({
            prefix: '@@',
            basepath: 'source/html/template'
        }))
        .pipe(plugins.htmlmin({collapseWhitespace: true}))  // html压缩
        .pipe(gulp.dest(destPath.html))
});


//Less样式处理
gulp.task('less', function (){
    gulp.src(srcPath.less)
        .pipe(plugins.less())
        .pipe(gulp.dest(function (f){
            return f.base;
        }));
});
//png
gulp.task('png', function (){
    return gulp.src(srcPath.image + '/**/*.png')
        .pipe(pngquant({quality: '65-80', speed: 4})())
        .pipe(gulp.dest(destPath.image));
});
gulp.task('jpg', function (){
    return gulp.src(srcPath.image + '/**/*.jpg')
        .pipe(jpegtran({progressive: true})())
        .pipe(gulp.dest(destPath.image));
});
gulp.task('gif', function (){
    return gulp.src(srcPath.image + '/**/*.gif')
        .pipe(gifsicle({interlaced: true})())
        .pipe(gulp.dest(destPath.image));
});
// clean-image
gulp.task('clean-image', function (){
    return gulp.src([destPath.image], {read: false})
        .pipe(plugins.clean());
});
//images
gulp.task('images', ['clean-image'], function (){
    return gulp.start('jpg', 'gif', 'png');
});
// 本地服务器
gulp.task('webserver', function (){
    gulp.src('.') // 服务器目录（.代表根目录）
        .pipe(plugins.webserver({ // 运行gulp-webserver
            livereload: true, // 启用LiveReload
            open: true, // 服务器启动时自动打开网页
            path: '/'
        }));
});
// 监听任务
gulp.task('watch', function (){
    // 监听 html
    gulp.watch(srcPath.html + '/**/*.html', ['html']);
    // 监听 less
    //gulp.watch(srcPath.css + '/**/*.less', ['less']);
    gulp.watch(srcPath.css + '/**/*.css', ['cssRelease']);
    // 监听 images
    gulp.watch(srcPath.image + '/**/*', ['images']);
    // 监听 js
    gulp.watch([srcPath.script + '/**/*.js', '!' + srcPath.script + '/**/*.min.js'], ['scriptRelease']);
});
// 默认任务
gulp.task('default', ['webserver', 'watch']);
/* = 发布环境( Release Task )
 -------------------------------------------------------------- */
// 清理文件
gulp.task('clean', function (){
    return gulp.src([destPath.css + '/maps', destPath.script + '/maps'], {read: false}) // 清理maps文件
        .pipe(plugins.clean());
});
// 样式处理
gulp.task('cssRelease', function (){
    gulp.src([srcPath.css + '/**/*.css', '!' + srcPath.css + '/**/*.min.css'])
        //        .pipe(plugins.autoprefixer('last 2 version', 'safari 5', 'ie 8', 'ie 9', 'opera 12.1', 'ios 6', 'android 4'))
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.minifyCss())
        .pipe(gulp.dest(destPath.css));
});
gulp.task('scriptBase', function (){
    var basePath = srcPath.script + '/base/',
        baseList = [
            basePath + 'singer.min.js',
            basePath + 'jquery.min.js',
            basePath + 'artTemplate.min.js',
            basePath + 'dialog.min.js',
            basePath + 'jquery-md5.js',
            basePath + 'restHelper.js',
            basePath + '*.js'
        ];
    return gulp.src(baseList)
        .pipe(plugins.concat('site-base.js'))
        // .pipe(gulp.dest(destPath.script));
        .pipe(plugins.rename({suffix: '.min'}))
        .pipe(plugins.uglify({preserveComments: 'some'}))
        .pipe(gulp.dest(destPath.script));
});
// 脚本压缩&重命名
gulp.task('scriptRelease', ['scriptBase'], function (){
    gulp.src([srcPath.script + '/poster/*.js', '!' + srcPath.script + '/poster/*.min.js']) // 指明源文件路径、并进行文件匹配，排除 .min.js 后缀的文件
        .pipe(plugins.rename({suffix: '.min'})) // 重命名
        .pipe(plugins.uglify({preserveComments: 'some'})) // 使用uglify进行压缩，并保留部分注释
        .pipe(gulp.dest(destPath.script + '/poster')); // 输出路径

    return gulp.src([srcPath.script + '/page/**/*.js', '!' + srcPath.script + '/page/**/*.min.js']) // 指明源文件路径、并进行文件匹配，排除 .min.js 后缀的文件
        .pipe(plugins.rename({suffix: '.min'})) // 重命名
        .pipe(plugins.uglify({preserveComments: 'some'})) // 使用uglify进行压缩，并保留部分注释
        .pipe(gulp.dest(destPath.script + '/page')); // 输出路径
});
// 打包发布
gulp.task('release', ['clean'], function (){ // 开始任务前会先执行[clean]任务
    return gulp.start('htmlrelease', 'cssRelease', 'scriptRelease', 'images'); // 等[clean]任务执行完毕后再执行其他任务
});
/* = 帮助提示( Help )
 -------------------------------------------------------------- */
gulp.task('help', function (){
    console.log('----------------- 开发环境 -----------------');
    console.log('gulp default		开发环境（默认任务）');
    console.log('gulp html		    HTML处理');
    console.log('gulp less		    样式处理');
    console.log('gulp script		JS文件压缩&重命名');
    console.log('gulp images		图片压缩');
    console.log('gulp concat		文件合并');
    console.log('---------------- 发布环境 -----------------');
    console.log('gulp release		打包发布');
    console.log('gulp clean		    清理文件');
    console.log('gulp cssRelease	样式处理');
    console.log('gulp scriptRelease	脚本压缩&重命名');
    console.log('---------------------------------------------');
});





