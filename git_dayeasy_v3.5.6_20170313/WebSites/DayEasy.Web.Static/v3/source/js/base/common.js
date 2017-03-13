/**
 * Created by shay on 2016/4/6.
 */
(function ($, S) {
    /**
     * 对话框扩展
     */
    var singerDialog = function () {
        S._mix(S, {
            /**
             * 对话框扩展
             */
            dialog: dialog,
            /**
             * 消息提示
             * @param msg
             * @param callback
             * @returns {*}
             */
            alert: function (msg, callback) {
                return S.dialog({
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
            /**
             * 消息确认
             * @param msg
             * @param ok
             * @param cancel
             * @param values
             * @returns {*}
             */
            confirm: function (msg, ok, cancel, values) {
                if (!values || !S.isArray(values))
                    values = ["确认", "取消"];
                var d = S.dialog({
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
                });
                d.showModal();
                return d;
            },
            tip: function (obj, title, msg, position) {
                return S.dialog({
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
                var d = S.dialog({
                    content: msg
                }).show();

                setTimeout(function () {
                    d.close().remove();
                    callback && S.isFunction(callback) && callback.call(this);
                }, time || 2000);
            },
            showImage: function (url, opts) {
                var $box = $('<div>');
                var $loading = $('<div>');
                $loading.append('<i class="fa fa-spin fa-spinner fa-2x"></i> 努力加载中...');
                opts = S.mix({
                    maxWidth: 800,
                    width: 0,
                    maxHeight: 500,
                    height: 0
                }, opts);

                var $img = $('<img />');
                $img.attr("src", url);
                opts.width > 0 && $img.css("width", opts.width);
                opts.height > 0 && $img.css("height", opts.height);
                opts.maxWidth > 0 && $img.css("max-width", opts.maxWidth);
                opts.maxHeight > 0 && $img.css("max-height", opts.maxHeight);
                $img.css("display", "none");

                $box.append($loading);
                $box.append($img);

                var d = S.dialog({
                    title: '图片查看',
                    content: $box,
                    quickClose: true,
                    backdropOpacity: 0.5
                });
                d.showModal();
                $img.bind("load.showImage", function () {
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
            },
            /**
             * 没有数据显示
             * @param option
             * @returns {*}
             */
            showNothing: function (option) {
                option = $.extend({
                    word: '没有数据',
                    icon: 'dy-icon-emoji01',
                    css: null
                }, option || {});
                var $html = $(S.format('<div class="dy-nothing"><div class="dy-nothing-content">{word}</div></div>', option));
                if (option.icon) {
                    $html.find('.dy-nothing-content').prepend(S.format('<i class="iconfont {0}"></i>', option.icon));
                }
                if (option && option.css) {
                    $html.css(option.css);
                }
                return $html.get(0).outerHTML;
            },
            /**
             * 模块加载动画
             * @param obj
             */
            loading: {
                start: function (obj) {
                    obj.prepend('<div class="dy-loading"><i></i></div>');
                },
                done: function (obj) {
                    obj.find('.dy-loading').remove();
                }
            },
            /**
             * 截取字符串补全
             * @param str
             * @param num
             * @param isEllipsis
             * @returns {*}
             */
            stringhidden: function (str, num, isEllipsis) {
                var r = /[^\x00-\xff]/g;
                if (str.replace(r, "mm").length <= num) return str;
                // n = n - 3;
                var txtSubstr = "";
                var m = Math.floor(num / 2);
                for (var i = m; i < str.length; i++) {
                    if (str.substr(0, i).replace(r, "mm").length >= num) {
                        txtSubstr = str.substr(0, i);
                        if (isEllipsis) {
                            txtSubstr += "......";
                        }
                        return txtSubstr;
                    }
                }
                return str;
            }
        });
        //delete window["dialog"];
    };

    if (typeof window["dialog"] !== "undefined") {
        singerDialog();
    }
})(jQuery, SINGER);

