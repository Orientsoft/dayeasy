(function ($) {
    var autoBoxs = [];
    $.fn.extend({
        /**
         * 收起/隐藏插件
         * @param options
         * @returns {*}
         */
        hiddenAway: function (options) {
            var opts = $.extend({
                    toggleClass: 'ha-show', //切换的class
                    container: 'ha-content', //切换的容器
                    state: 0, //初始状态
                    toggleHtml: null, //切换的html
                    joint: null, //协同控制
                    time: 300, //切换时间(秒)
                    resize: true //是否重置autoHeight
                }, options || {}),
                $t = $(this);
            return $.each($t, function (i) {
                var $item = $t.eq(i),
                    data = $item.data("hiddenaway") || $item.attr("data-hiddenaway"),
                    ps = $.extend({}, opts);
                if (data && "string" === typeof data) {
                    try {
                        data = eval('(' + data + ')');
                    } catch (e) {
                    }
                }
                if ("object" === typeof data)
                    ps = $.extend(ps, data);
                $(ps.container).data("state", ps.state);
                $item.data("ps", ps);
                var toggleFn = function (obj, toggle) {
                    var $h = $(obj),
                        ops = $h.data("ps"),
                        $c = $(ops.container),
                        cls = ops.toggleClass,
                        time = ops.time,
                        htm,
                        $joint;
                    cls && $h.toggleClass(cls);
                    if (toggle) {
                        var state = $c.data("state");
                        if (state) {
                            ops.resize && setTimeout(function () {
                                window.autoHeight && window.autoHeight.set();
                            }, time + 200);
                            $c.fadeOut(time).data("state", 0);
                        } else {
                            ops.resize && window.autoHeight && window.autoHeight.set();
                            $c.fadeIn(time).data("state", 1);
                        }
                    }
                    htm = ops.toggleHtml;
                    if (htm != null) {
                        ops.toggleHtml = $h.html();
                        $h.html(htm);
                    }
                    $joint = $(ops.joint);
                    if (toggle && $joint) {
                        toggleFn($joint, false);
                    }
                    $h.data("ps", ops)
                };
                $item.bind("click", function () {
                    toggleFn(this, true);
                    return false;
                });
            });
        },
        /**
         * 自适应高度
         * @param options 参数
         */
        autoHeight: function (options) {
            var opts = $.extend({
                    leftHeight: 314,
                    minHeight: 350,
                    type: 0
                }, options || {}),
                $t = $(this);
            $t.each(function () {
                autoBoxs.push(this);
            });
            var setHeight = function () {
                var sh = $(window).height();
                $t.each(function (i) {
                    var $item = $t.eq(i),
                        left = $item.data("left") || opts.leftHeight,
                        min = $item.data("min") || opts.minHeight,
                        type = $item.data("type") || opts.type,
                        h = sh - left;
                    if (type == 0) {
                        $item.css("height", (h <= min ? min : h) + "px");
                    } else {
                        if (h <= min) {
                            $item.css({"height": "auto", "min-height": min});
                        } else {
                            $item.css({"min-height": h, "overflow": "hidden"});
                        }
                    }
                });
            };
            setHeight();
            $(window).bind("resize.autoHeight", function () {
                setHeight();
            });
            return {
                set: setHeight
            };
        },
        /**
         * 支持滚轮的滚动插件,需要引入mousewheel.js
         * @param options
         * @returns {*}
         */
        wheelMove: function (options) {
            var options = $.extend({}, {
                    items: '>li',
                    prev: '.w-prev',
                    next: '.w-next',
                    direction: 'top',
                    wheel: true, //滚轮事件
                    step: 1, //每次移动几个元素
                    min: 2, //最少元素，低于将无效果
                    start: 0, //从第几个开始
                    show: 3, //每页显示几个
                    speed: 500, //速度
                    loop: true, //是否可循环滚动
                    blank: true, //是否允许留白
                    auto: false, //是否自动滚动
                    sleep: 4000, //自动滚动间歇
                    pause: true, //鼠标经过时暂停滚动
                    controls: '', //控制器
                    controlEvent: 'click.wheel', //控制器触发事件
                    activeClass: 'active', //
                    easing: '', //切换效果
                    callback: false //回调
                }, options),
                $t = $(this),
                perPixel,
                moving = false,
                target = {},
                $controls = $(options.controls),
                lastAction;
            target.items = $t.find(options.items);
            target.len = target.items.length;

            if (target.len < options.min) return false;
            var $v = target.items.eq(0);
            if ("top" === options.direction) {
                perPixel = $v.outerHeight();
            } else {
                perPixel = $v.outerWidth();
            }
            $t.data("index", 0);

            var moveStep = function (step, isTimer) {
                var index = $t.data("index");
                index += step;
                if (moving) {
                    lastAction = {index: index, isTimer: isTimer};
                    return false;
                }
                lastAction = "";
                !isTimer && clearTimer();
                moving = true;
                //判断循环设置
                if (step > 0) {
                    //向下
                    var left = target.len - index;
                    if (!options.blank && left < options.step && left > 0) {
                        index -= options.step - left;
                    }
                } else {
                    if (index < 0 && index > 0 - options.step) index = 0;
                }
                if (!options.loop && (index < 0 || index >= target.len)) {
                    moving = false;
                    return false;
                }
                var last = target.len % options.show;
                if (last == 0) last = options.show;
                if (index < 0) index = (target.len - last);
                if (index >= target.len) index = 0;
                var easyIn = {
                    duration: options.speed,
                    complete: function () {
                        moving = false;
                        !isTimer && setTimer();
                        if (lastAction) {
                            target.to(lastAction.index);
                        }
                        options.callback && "function" === typeof options.callback && options.callback.call(this, index);
                    }
                };
                if (options.easing) {
                    easyIn.easing = options.easing;
                }
                if ("top" === options.direction) {
                    $t.animate({
                        "margin-top": (0 - index * perPixel)
                    }, easyIn);
                } else {
                    $t.animate({
                        "margin-left": (0 - index * perPixel)
                    }, easyIn);
                }
                if ($controls.length) {
                    $controls.removeClass(options.activeClass).eq(index).addClass(options.activeClass);
                }
                $t.data("index", index);
            };
            var timer, stop = false;
            var setTimer = function () {
                if (!options.auto) return;
                if (!timer) {
                    timer = setInterval(function () {
                        if (!moving)
                            moveStep(options.step, true);
                    }, options.sleep);
                    stop = false;
                }
            };
            var clearTimer = function () {
                if (!options.auto) return;
                if (timer) {
                    clearTimeout(timer);
                    timer = undefined;
                    stop = true;
                }
            };
            if (options.auto) {
                setTimer();
                if (options.pause) {
                    $t
                        .bind("mouseover", function () {
                            clearTimer();
                            stop = true;
                        })
                        .bind("mouseout", function () {
                            stop = false;
                            setTimer();
                        });
                }
            }
            options.prev && $(options.prev).bind("click", function () {
                target.prev();
            });
            options.next && $(options.next).bind("click", function () {
                target.next();
            });

            if ($controls.length) {
                $controls.bind(options.controlEvent, function () {
                    var index = $controls.index($(this));
                    target.to(index);
                });
            }

            if (options.wheel) {
                $t.bind("mousewheel.wheelmove", function (e, delta) {
                    moveStep(delta < 0 ? options.step : 0 - options.step);
                    return false;
                });
            }
            //支持响应式
            $(window).bind("resize.wheelmove", function () {
                if ("top" === options.direction) {
                    perPixel = $v.outerHeight();
                } else {
                    perPixel = $v.outerWidth();
                }
                target.to($t.data("index"));
            });
            target.reset = function () {
                target.items = $t.find(options.items);
                target.len = target.items.length;
            };
            target.prev = function () {
                moveStep(0 - options.step);
            };
            target.next = function () {
                moveStep(options.step);
            };
            target.to = function (index) {
                var current = $t.data("index");
                moveStep(index - current);
            };
            target.index = function () {
                return $t.data("index");
            };
            return target;
        }
    });
    $(".j-hiddenAway").hiddenAway();
    window.autoHeight = $(".j-autoHeight").autoHeight();
})(jQuery);
(function (S) {
    /**
     * 对话框扩展
     */
    if (window.dialog) {
        S._mix(S, {
            alert: function (msg, callback) {
                return dialog({
                    title: "消息提示",
                    content: msg,
                    okValue: "确定",
                    ok: function () {
                        callback && S.isFunction(callback) && callback.call(this);
                        this.close().remove();
                    },
                    cancel: false
                }).showModal();
            },
            confirm: function (msg, ok, cancel, values) {
                if (!values || !S.isArray(values))
                    values = ["确认", "取消"];
                return dialog({
                    title: "消息确认提示",
                    content: msg,
                    okValue: values[0],
                    ok: function () {
                        ok && S.isFunction(ok) && ok.call(this);
                    },
                    cancelValue: values[1],
                    cancel: function () {
                        cancel && S.isFunction(cancel) && cancel.call(this);
                        this.close().remove();
                    }
                }).showModal();
            },
            tip: function (obj, title, msg, position) {
                return dialog({
                    align: position,
                    title: title,
                    content: msg,
                    quickClose: true,
                    ok: function () {
                        this.close().remove();
                    }
                }).show(obj);
            },
            msg: function (msg, time, callback) {
                var d = dialog({
                    content: msg
                }).show();

                setTimeout(function () {
                    d.close().remove();
                    callback && S.isFunction(callback) && callback.call(this);
                }, time || 2000);
            },
            dialog: dialog,
            showImage: function (url) {
                var $box = $('<div>');
                var $loading = $('<div>');
                $loading.append('<i class="fa fa-spin fa-spinner fa-2x"></i> 努力加载中...');

                var $img = $('<img />');
                $img.attr("src", url);
                $img.css("max-width", "800px");
                $img.css("max-height", "500px");
                $img.css("display", "none");

                $box.append($loading);
                $box.append($img);

                var d = dialog({
                    title: '图片查看',
                    content: $box,
                    quickClose: true,
                    backdropOpacity: 0.5
                });
                d.showModal();
                $img.bind("load", function () {
                    $loading.remove();
                    $(this).show();
                    d.reset();
                });
            },
            msgArray: {
                updateSuccess: '更新成功！',
                publishSuccess: '发布成功！老师辛苦了，“<span class="tip-color">进行中</span>”可查看哦~',
                closeSuccess: '关闭成功！作业已移至“<span class="tip-color">已结束</span>”咯~',
                workSuccess: '发布成功！老师辛苦了， <span class="tip-color">批阅中心</span>可查看哦~'
            }
        });
    }
})(SINGER);

