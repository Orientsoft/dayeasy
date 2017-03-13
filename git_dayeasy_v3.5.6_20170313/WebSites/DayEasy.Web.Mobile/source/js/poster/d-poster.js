/**
 * 大神海报插件
 * Created by shay on 2016/9/2.
 */
(function ($, S) {
    var getPixelRatio = function (context) {
        var backingStore = context.backingStorePixelRatio ||
            context.webkitBackingStorePixelRatio ||
            context.mozBackingStorePixelRatio ||
            context.msBackingStorePixelRatio ||
            context.oBackingStorePixelRatio ||
            context.backingStorePixelRatio || 1;
        return (window.devicePixelRatio || 1) / backingStore;
    };

    /**
     * 图片矫正
     * @param img
     * @param dir
     * @param callback
     */
    var correctImage = function (img, dir, callback) {
        //S.msg(dir);
        if (!dir) {
            callback && callback.call(this, img);
            return false;
        }
        var image = new Image();
        image.onload = function () {
            var degree = 0, drawWidth, drawHeight, width, height;
            drawWidth = this.naturalWidth;
            drawHeight = this.naturalHeight;
            //以下改变一下图片大小
            var maxSide = Math.max(drawWidth, drawHeight);
            if (maxSide > 1024) {
                var minSide = Math.min(drawWidth, drawHeight);
                minSide = minSide / maxSide * 1024;
                maxSide = 1024;
                if (drawWidth > drawHeight) {
                    drawWidth = maxSide;
                    drawHeight = minSide;
                } else {
                    drawWidth = minSide;
                    drawHeight = maxSide;
                }
            }
            var canvas = document.createElement('canvas');
            canvas.width = width = drawWidth;
            canvas.height = height = drawHeight;
            var context = canvas.getContext('2d');
            //判断图片方向，重置canvas大小，确定旋转角度，iphone默认的是home键在右方的横屏拍摄方式
            switch (dir) {
                //iphone横屏拍摄，此时home键在左侧
                case 3:
                    degree = 180;
                    drawWidth = -width;
                    drawHeight = -height;
                    break;
                //iphone竖屏拍摄，此时home键在下方(正常拿手机的方向)
                case 6:
                    canvas.width = height;
                    canvas.height = width;
                    degree = 90;
                    drawWidth = width;
                    drawHeight = -height;
                    break;
                //iphone竖屏拍摄，此时home键在上方
                case 8:
                    canvas.width = height;
                    canvas.height = width;
                    degree = 270;
                    drawWidth = -width;
                    drawHeight = height;
                    break;
            }
            //使用canvas旋转校正
            context.rotate(degree * Math.PI / 180);
            context.drawImage(this, 0, 0, drawWidth, drawHeight);
            //返回校正图片
            callback && callback.call(this, canvas.toDataURL("image/png"));
        };
        image.src = img;
    };

    /**
     * 海报画布
     */
    function PosterCanvas(ele) {
        this._ele = ele;
        this._canvas = null;
        this._context = null;
        this._sceneImage = null;
        this._customImage = null;
        this._customImageData = null;
        this._user = null;
        this._scene = null;
        this._width = 350;
        this._height = 525;
        this._customX = 0;
        this._customY = 0;
        this._dragX = 0;
        this._dragY = 0;
        this._dragListen = false;
        this._customZoom = 1;
        this._customAngle = 0;//旋转度数
        this._init();
        //画文字
        this._drawWord = function (context, callback) {
            var drawText = function (opt) {
                context.save();
                context.font = opt.font;
                context.fillStyle = opt.color;
                context.globalCompositeOperation = 'source-over';
                if (opt.point.max) {
                    var max = Math.ceil(opt.point.max);
                    for (var i = 0; i < Math.ceil(opt.text.length / max); i++) {
                        var word = opt.text.substring(i * max, (i + 1) * max);
                        context.fillText(word, opt.point.x, opt.point.y + i * (opt.lineHeight || 25));
                    }
                } else {
                    context.fillText(opt.text, opt.point.x, opt.point.y);
                }
                context.restore();
            };
            drawText(this._scene.word);
            this._scene.teacher.text = S.format(this._scene.teacher.text, this._user);
            drawText(this._scene.teacher);
            this._scene.from.text = S.format(this._scene.from.text, this._user);
            drawText(this._scene.from);
            callback && callback.call(this);
        };
        //画主题背景
        this._drawScene = function (context, callback) {
            var draw = function (poster) {
                context.drawImage(poster._sceneImage, 0, 0, poster._canvas.width, poster._canvas.height);
                callback && callback.call();
            };
            if (this._sceneImage) {
                draw(this);
                return true;
            }
            if (this._scene) {
                this._sceneImage = new Image();
                this._sceneImage.src = this._scene.image;
                var poster = this;
                this._sceneImage.onload = function () {
                    draw(poster);
                    return true;
                }
            }
        };
        //画用户图片
        this._drawCustom = function (context, callback) {
            var draw = function (poster) {
                context.save();
                var width = poster._canvas.width / 2;
                var height = (width / poster._customImage.width) * poster._customImage.height;
                var x = poster._customX + (poster._dragX || 0),
                    y = poster._customY + (poster._dragY || 0);
                if (poster._customAngle > 0) {
                    var xpos = poster._width / 2,
                        ypos = poster._height / 2;
                    context.translate(xpos, ypos);
                    context.rotate(poster._customAngle * Math.PI / 180);
                    context.translate(-xpos, -ypos);
                }
                width = width * poster._customZoom;
                height = height * poster._customZoom;
                context.drawImage(poster._customImage, 0, 0, poster._customImage.width, poster._customImage.height, x, y, width, height);
                context.restore();
                poster._dragX = 0;
                poster._dragY = 0;
                callback && callback.call(poster);
            };
            if (this._customImage) {
                draw(this);
                return;
            }
            if (this._customImageData) {
                this._customImage = new Image();
                this._customImage.src = this._customImageData;
                var poster = this;
                this._customImage.onload = function () {
                    draw(poster);
                };
                if (!this._dragListen) {
                    this.dragEvent();
                    this._dragListen = true;
                }
                return;
            }
            callback && callback.call();
        }
    }

    PosterCanvas.prototype._init = function () {
        var canvas = document.createElement('canvas');
        if (!canvas || !canvas.getContext)
            return false;
        canvas.width = this._width;
        canvas.height = this._height;
        $(this._ele).append(canvas);
        this._canvas = canvas;
        this._context = this._canvas.getContext('2d');
    };
    PosterCanvas.prototype.dragEvent = function () {
        var startX, startY, poster = this;
        var touchStart = function (event) {
            event.preventDefault();
            if (!event.touches.length) return;
            var touch = event.touches[0];
            startX = touch.pageX;
            startY = touch.pageY;
        };
        var touchMove = function (event) {
            event.preventDefault();
            if (!event.touches.length) return;
            var touch = event.touches[0];
            var x = touch.pageX - startX;
            var y = touch.pageY - startY;
            poster.dragCustom(x, y);
        };
        var touchEnd = function (event) {
            event.preventDefault();
            if (!event.changedTouches.length) return;
            var touch = event.changedTouches[0];
            var ratio = getPixelRatio(poster._context);
            poster._customX += (touch.pageX - startX) * ratio;
            poster._customY += (touch.pageY - startY) * ratio;
        };
        this._canvas.addEventListener('touchstart', touchStart, false);
        this._canvas.addEventListener('touchmove', touchMove, false);
        this._canvas.addEventListener('touchend', touchEnd, false);
    };

    /**
     * 创建画布
     * @param callback
     * @returns {*}
     */
    PosterCanvas.prototype.draw = function (callback) {
        var context = this._context,
            poster = this;
        context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        poster._drawCustom(context, function () {
            poster._drawScene(context, function () {
                poster._drawWord(context, function () {
                    callback && callback.call(poster);
                });
            });
        });
    };
    /**
     * 编辑文字
     * @param word
     * @returns {boolean}
     */
    PosterCanvas.prototype.editWord = function (word) {
        if (!this._scene || !this._scene.word)
            return false;
        this._scene.word.text = word;
        this.draw();
    };
    /**
     * 设置用户信息
     * @param user
     */
    PosterCanvas.prototype.setUser = function (user) {
        this._user = user;
    };
    /**
     * 设置主题
     * @param scene
     * @returns {boolean}
     */
    PosterCanvas.prototype.setScene = function (scene) {
        if (!scene) {
            return false;
        }
        var ratio = getPixelRatio(this._context);
        this._customX = scene.position.x * ratio;
        this._customY = scene.position.y * ratio;
        this._scene = scene;
        this._sceneImage = null;
        this.draw();
    };
    /**
     * 设置自定义图片
     * @param imageData
     * @param callback
     * @returns {boolean}
     */
    PosterCanvas.prototype.setCustom = function (imageData, callback) {
        this._customImageData = imageData;
        this._customImage = null;
        var ratio = getPixelRatio(this._context);
        this._customX = this._scene.position.x * ratio;
        this._customY = this._scene.position.y * ratio;
        this.draw(callback);
    };
    /**
     * 拖动自定义图片
     * @param x
     * @param y
     */
    PosterCanvas.prototype.dragCustom = function (x, y) {
        var ratio = getPixelRatio(this._context);
        this._dragX = x * ratio;
        this._dragY = y * ratio;
        //if (this._customX + this._dragX > 0 || this._customY + this._dragY > 0)
        //    return false;
        this.draw();
    };
    /**
     * 缩放自定义图片
     * @param zoomType 放大/缩小
     */
    PosterCanvas.prototype.zoomCustom = function (zoomType) {
        if (!this._customImage)
            return false;
        var zoom = 0;
        if (zoomType) {
            zoom = (this._customZoom >= 1 ? 0.2 : 0.05);
        } else {
            zoom = (this._customZoom > 1 ? -0.2 : -0.05);
        }
        if (this._customZoom + zoom < 0.5 || this._customZoom + zoom > 3)
            return false;

        var scale = this._width / (this._customImage.width * 2),
            width = scale * this._customZoom,
            height = scale * this._customImage.height * this._customZoom;
        this._customX -= (width * zoom) / 2;
        this._customY -= (height * zoom) / 2;
        this._customZoom = parseFloat((this._customZoom + zoom).toFixed(2));
        this.draw();
        return this._customZoom;
    };
    /**
     * 旋转自定义图片
     * @param angle
     */
    PosterCanvas.prototype.rotate = function (angle) {
        this._customAngle += angle;
        this.draw();
    };

    /**
     * 生成原始图片
     * @returns {string}
     */
    PosterCanvas.prototype.getImage = function () {
        return this._canvas.toDataURL("image/jpeg", .88);
    };

    $.fn.extend({
        /**
         * 海报
         */
        poster: function () {
            return new PosterCanvas(this);
        },
        fileSelector: function (option) {
            option = $.extend({}, {start: undefined, finished: undefined}, option || {});
            var $input = $('<input type="file" id="takePicture" accept="image/*" />');
            $input.css('display', 'none');
            $('body').append($input);
            $(this).bind('click', function () {
                $input.click();
            });
            $input.bind('change', function (event) {
                var files = event.target.files,
                    file;
                if (files && files.length > 0) {
                    file = files[0];
                    try {
                        option.start && option.start.call(this);
                        var orientation;
                        EXIF.getData(file, function () {
                            orientation = EXIF.getTag(this, 'Orientation');
                            console.log('orientation:' + orientation);
                        });
                        var fileReader = new FileReader();
                        fileReader.onload = function (event) {
                            var imageData = event.target.result;
                            correctImage(imageData, orientation, option.finished);
                            //callback && callback.call(this, imageData);
                        };
                        fileReader.readAsDataURL(file);
                    }
                    catch (e) {
                    }
                }
            });
        }
    });
})
(jQuery, SINGER);