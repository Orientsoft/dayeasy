(function ($) {
    var config = $.extend({
        auto: true,
        multiple: true,
        fileNum: 10,
        limit: 1,
        fileSite: 'http://file.dayez.net',
        staticSite: 'http://static.dayeasy.dev',
        ext: 'image/*',
        callback: undefined
    }, window.uploaderConfig || {}),
        baseUrl = config.staticSite + "/plugs/webuploader",
        $wrap = $('.uploadContainer'),
        $queue = $('ul.filelist'), // 图片容器
        $upload = $wrap.find('.uploadBtn'), // 上传按钮
        $imgValues = $("#imgValues"), //图片url隐藏域
        $info = $("#info"), //消息提示
        fileCount = 0, // 添加的文件数量
        fileSize = 0, // 添加的文件总大小
        ratio = window.devicePixelRatio || 1, // 优化retina, 在retina下这个值是2
        thumbnailWidth = 110 * ratio,// 缩略图大小
        thumbnailHeight = 110 * ratio,
        state = 'pedding', // 可能有pedding, ready, uploading, confirm, done.
        supportTransition = (function () {
            var s = document.createElement('p').style,
                r = 'transition' in s ||
                    'WebkitTransition' in s ||
                    'MozTransition' in s ||
                    'msTransition' in s ||
                    'OTransition' in s;
            s = null;
            return r;
        })(),
        uploader; // WebUploader实例

    if (!WebUploader.Uploader.support()) {
        alert('Web Uploader 不支持您的浏览器！如果你使用的是IE浏览器，请尝试升级 flash 播放器');
        throw new Error('WebUploader does not support the browser you are using.');
    }
    if (config.auto) {
        $('.uploadContainer').find('.uploadBtn').hide();
    }

    // 实例化
    uploader = WebUploader.create({
        auto: config.auto, // 选完文件后，是否自动上传
        pick: {
            id: '#filePicker',
            multiple: config.multiple //是否可以选择多张
        },
        dnd: '.uploadContainer ul.filelist', //拖拽容器
        paste: document.body,
        accept: {
            title: config.title,
            extensions: config.ext,
            mimeTypes: config.mimeTypes
        },
        disableGlobalDnd: true, //禁用整个页面的拖拽功能
        chunked: false, //分片处理大文件上传
        chunkSize: 100 * 1024 * 1024,
        swf: baseUrl + '/Uploader.swf', // swf文件路径
        server: config.fileSite + '/uploader?type=' + config.type,
        fileNumLimit: config.fileNum,
        fileSizeLimit: 500 * 1024 * 1024, // 500 M
        fileSingleSizeLimit: config.limit * 1024 * 1024 // 2 M
    });

    //添加文件到队列
    uploader.onFileQueued = function (file) {
        fileCount++;
        fileSize += file.size;
        addFile(file);
    };

    //从队列中移除文件
    uploader.onFileDequeued = function (file) {
        fileCount--;
        fileSize -= file.size;

        if (fileCount < 1) {
            $queue.css("display", "none");
        }
        removeFile(file);
    };

    //上传进度
    uploader.onUploadProgress = function (file, percentage) {
        var $li = $('#' + file.id),
            $percent = $li.find('.progress span');
        $percent.css('width', percentage * 100 + '%');
    };

    //文件上传成功
    uploader.onUploadSuccess = config.callback || function (file, response) {
        if (response.state == 1) {
            if (response.urls) {
                for (var i = 0; i < response.urls.length; i++) {
                    var hiddenValue = '<input type="hidden" value="' + response.urls[i] + '" class="upimgURL"/>';
                    $imgValues.append(hiddenValue);
                }
            }
        }
    };

    //上传完成（无论成功还是失败都要执行）
    uploader.onUploadComplete = function (file) {
        setTimeout(function () {
            $info.text("");
        }, 3000);
    };

    //上传失败
    uploader.onUploadError = function (file) {
        $info.text("上传失败，请稍后重试！");
    };

    uploader.on('all', function (type) {
        if (type === 'startUpload') {
            state = 'uploading';
        } else if (type === 'stopUpload') {
            state = 'paused';
        } else if (type === 'uploadFinished') {
            state = 'done';
        }

        if (state === 'uploading') {
            $upload.text('暂停上传');
        } else {
            $upload.text('开始上传');
        }
    });
    $wrap.delegate('.uploadBtn', 'click', function () {

    });
    $upload.on('click', function () {
        if (state === 'uploading') {
            uploader.stop();
        } else {
            uploader.upload();
        }
    });

    // 当有文件添加进来时执行，负责view的创建
    function addFile(file) {
        var $li = $('<li id="' + file.id + '">' +
                '<p class="title">' + file.name + '</p>' +
                '<p class="imgWrap"></p>' +
                '<p class="progress"><span></span></p>' +
                '</li>'),

            $btns = $('<div class="file-panel">' +
                '<span class="cancel">删除</span>' +
                '<span class="rotateRight">向右旋转</span>' +
                '<span class="rotateLeft">向左旋转</span></div>').appendTo($li),
            $prgress = $li.find('p.progress span'),
            $wrap = $li.find('p.imgWrap'),

            showError = function (code) {
                switch (code) {
                    case 'exceed_size':
                        text = '文件大小超出';
                        break;

                    case 'interrupt':
                        text = '上传暂停';
                        break;

                    default:
                        text = '上传失败，请重试';
                        break;
                }

                $info.text(text);
            };

        if (file.getStatus() === 'invalid') {
            showError(file.statusText);
        } else {
            // todo lazyload
            $wrap.text('预览中');
            uploader.makeThumb(file, function (error, src) {
                if (error) {
                    $wrap.text('不能预览');
                    return;
                }

                var img = $('<img src="' + src + '">');
                $wrap.empty().append(img);
            }, thumbnailWidth, thumbnailHeight);

            file.rotation = 0;
        }

        file.on('statuschange', function (cur, prev) {
            if (prev === 'progress') {
                $prgress.hide().width(0);
            } else if (prev === 'queued') {
                $li.off('mouseenter mouseleave');
                $btns.remove();
            }
            // 成功
            if (cur === 'error' || cur === 'invalid') {
                console.log(file.statusText);
                showError(file.statusText);
            } else if (cur === 'interrupt') {
                showError('interrupt');
            } else if (cur === 'queued') {

            } else if (cur === 'progress') {
                $prgress.css('display', 'block');
            } else if (cur === 'complete') {
                $li.append('<span class="success"></span>');
            }

            $li.removeClass('state-' + prev).addClass('state-' + cur);
        });

        $li.on('mouseenter', function () {
            $btns.stop().animate({ height: 30 });
        });

        $li.on('mouseleave', function () {
            $btns.stop().animate({ height: 0 });
        });

        $btns.on('click', 'span', function () {
            var index = $(this).index(),
                deg;
            switch (index) {
                case 0:
                    uploader.removeFile(file);
                    return;
                case 1:
                    file.rotation += 90;
                    break;
                case 2:
                    file.rotation -= 90;
                    break;
            }

            if (supportTransition) {
                deg = 'rotate(' + file.rotation + 'deg)';
                $wrap.css({
                    '-webkit-transform': deg,
                    '-mos-transform': deg,
                    '-o-transform': deg,
                    'transform': deg
                });
            } else {
                $wrap.css('filter', 'progid:DXImageTransform.Microsoft.BasicImage(rotation=' + (~~((file.rotation / 90) % 4 + 4) % 4) + ')');
            }
        });
        $queue.css("display", "table");
        $li.appendTo($queue);
    }

    // 负责view的销毁
    function removeFile(file) {
        var $li = $('#' + file.id);

        $li.off().find('.file-panel').off().end().remove();
    }
})(jQuery);