/**
 * Created by shay on 2015/3/23.
 */

(function ($, S) {
    var logger, optionWords = [], currentUser, initUser, needRole, loadUser;
    logger = S.getLogger("site-base");
    needRole = $('body').data('role');
    if (needRole >= 0) {
        //身份验证
        (function () {
            $.dAjax({
                method: 'user_load'
            }, function (json) {
                if (!json.status) {
                    var url = '';
                    if (location.pathname != '/') {
                        url = encodeURIComponent(location.href);
                    }
                    location.href = '/page/account/login.html?back=' + url;
                    return false;
                }
                //没有权限
                if ((needRole & json.data.role) == 0) {
                    location.href = 'http://www.dayeasy.net/user/role';
                    return false;
                }
                // 家长 1  学生2  教师 4    家长学生3     组合7
                currentUser = json.data;
                currentUser.roleDesc = function () {
                    var role = currentUser.role;
                    if ((role & 2) > 0)
                        return '学生';
                    if ((role & 4) > 0)
                        return currentUser.subjectName + '教师';
                    if ((role & 1) > 0)
                        return '家长';
                    return '游客';
                };
                loadUser && loadUser.call(this, currentUser);
                initUser();
            });
        })();
        /**
         * 加载用户信息
         */
        initUser = function () {
            if ($('#right-panel').length == 0)
                return false;
            $('.d-user-avatar').attr('src', currentUser.avatar);
            $('.user-name').html(currentUser.name);
            $('.d-user-code').html('No.' + currentUser.code);
            $('.d-user-role').html(currentUser.roleDesc);
        };
    }
    //选择字母
    for (var i = 0; i < 26; i++) {
        optionWords.push(String.fromCharCode(i + 65));
    }
    var progress = {
        start: function () {
            var $progress = $('.d-progress');
            if (!$progress.length) {
                $progress = $('<div class="d-progress">');
                $progress.append($('<div class="dp-bar"><div class="dp-peg"></div></div>'));
                $progress.append($('<div class="dp-spinner"><i class="fa fa-spin fa-spinner"></i></div>'));
                $('body').append($progress);
            }
            $progress.data('progress', 0).find('.dp-bar').css('transform', 'translate3d(-100%, 0px, 0px)');
            S.later(function () {
                var d = $progress.data('progress');
                if (d < 100) {
                    d += Math.random() * 8;
                    if (d >= 100) {
                        d = 99.9;
                    }
                    $progress.data('progress', d).find('.dp-bar').css('transform', 'translate3d(' + (d - 100) + '%, 0px, 0px)');
                }
            }, 500, true);
            return $progress;
        },
        done: function (percent) {
            var $progress = $('.d-progress');
            if (!$progress.length)
                return false;
            var d = percent;
            $progress.data('progress', d).find('.dp-bar').css('transform', 'translate3d(' + (d - 100) + '%, 0px, 0px)');
            if (S.isUndefined(percent) || percent == 100) {
                S.later(function () {
                    $progress.remove();
                }, 200);
            }
        }
    };
    var optionModel = function (options) {
        for (var i = 0; i < options.length; i++) {
            var item = options[i],
                len = 0;
            //有公式
            if (item.body.indexOf('\\[') >= 0) return true;
            len += (item.images && item.images.length) ? 18 : 0;
            if (S.lengthCn(item.body) + len > 35) return false;
        }
        return true;
    };
    template.helper('optionWord', function (sort) {
        return optionWords[sort];
    });
    template.helper('optionModel', function (options) {
        return optionModel(options) ? 'q-options-horizontal' : '';
    });
    S.mix(S, {
        /**
         * 按钮状态
         * @param $btn
         * @param enabled
         * @param word
         * @returns {boolean}
         */
        enableBtn: function ($btn, enabled, word) {
            if (!$btn)
                return false;
            var DISABLED = 'disabled',
                DATA_NAME = 'cache_html';
            if (S.isUndefined(enabled)) {
                enabled = $btn.hasClass(DISABLED);
            } else {
                if (enabled == !$btn.hasClass(DISABLED)) {
                    return false;
                }
            }
            if (enabled) {
                $btn.removeClass(DISABLED).removeAttr(DISABLED);
                word = $btn.data(DATA_NAME);
                word && $btn.html(word);
            } else {
                $btn.addClass(DISABLED).attr(DISABLED, DISABLED);
                if (word) {
                    $btn.data(DATA_NAME, $btn.html());
                    $btn.html(word);
                }
            }
        },
        /**
         * 进度条
         */
        progress: progress,
        /**
         * 当前用户
         */
        setUser: function (callback) {
            if (currentUser && callback) {
                callback.call(this, currentUser);
                return false;
            }
            loadUser = callback;
        },
        /**
         * 问题展示
         * @param question
         */
        showQuestion: function (question) {
            if (!question) return '';
            if ($('#q-template').length == 0)
                return '缺少q-template模板';
            question.show_option = true;
            return template('q-template', question);
        }
    });
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

               var $WindowWidth= $(window).width();
                var $img = $('<img />');
                $img.attr("src", url);
                $img.css("width", $WindowWidth-40);
//                $img.css("height", "100%");
//                $img.css("max-width", "800px");
//                $img.css("max-height", "500px");
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
            }
        });
    }
//临时注销
//    template.config('escape', false);
    (function () {
        /**
         * 设置页脚
         */
        var setFooter = function () {
            var left, $footer = $('footer'), footerType;
            if (!$footer.length)
                return false;
            left = $(document).height() - $('body').height();
            footerType = $footer.data('type');
            if (footerType == 'min') {
                left -= $footer.height();
            }
            if (left > 0) {
                $footer.addClass('min-type').data('type', 'min');
            } else {
                $footer.removeClass('min-type').data('type', '');
            }
        };
        $(window).bind('resize.dayeasy', function () {
            setFooter();
        });
        setFooter();
        var $code = $('.site-qr');
        if (S.UA.mobile) {
            $code.remove();
        } else {
            /**
             * 网页二维码
             */
            $code.bind('click', function () {
                var qr = 'http://qr.liantu.com/api.php?text={0}',
                    src = S.format(qr, encodeURIComponent(window.top.location.href));
                S.dialog({
                    title: '网页二维码',
                    content: '<img width="150" height="150" src="' + src + '" alt="">',
                    quickClose: true,
                    align: 'top right',
                    backdropOpacity: 0.5
                }).show(this);
            });
        }
    }());

})(jQuery, SINGER);

(function ($, window) {
    /**************
     * 全局函数插件
     */
    jQuery.bai = {}
    //  Tool
    function addLoadEvent(func) {
        var oldonload = window.onload;
        if (typeof window.onload != 'function') {
            window.onload = func;
        } else {
            window.onload = function () {
                oldonload();
                func();
            }
        }
    }
    function addClass(element, value) {
        if (!element.className) {
            element.className = value;
        } else {
            newClassName = element.className;
            newClassName += " ";
            newClassName += value;
            element.className = newClassName;
        }
    }
    function BaiFuc() {
        this.bd = $('body');
    }
    BaiFuc.prototype = {
        // 初始化
        _init: function () {
            this._clickHref();
            this._lookup();
        },
        // 导航链接效果
        _clickHref: function () {
            var $nav = $('[data-nav="href"]');
            var linkurl = $nav.find('a');
            for (var i = 0; i < linkurl.length; i++) {
                var linkurlhref = linkurl[i].getAttribute("href");
                var currenturl = window.location.href;
                if (currenturl.indexOf(linkurlhref) != -1) {
                    linkurl[i].className = "href";
                    var linktext = linkurl[i].getAttribute("data-list-nav");
                    console.log(linktext);
                    addClass(document.body, linktext)
                }
            }
        },
        // 浮动查看搜索
        _lookup: function () {
            /**
             * 浮动查看搜索
             */
            var $lookup = $('.bubble-lookup-list');
            jsui.bd
                .on('touchstart', '.bubble-lookup', function () {
                    event.preventDefault();
                    $lookup.addClass('on');
                })
                .on('touchstart', '.bubble-lookup-close', function () {
                    event.preventDefault();
                    $lookup.removeClass('on');
                })
        }
    };
    // 初始化加载
    var jsui = new BaiFuc();
    jsui._init();


    /**
     * 最大输入字符
     * @param obj
     * @param maxLength
     */
    var textMax = function (obj, maxLength) {
        var $t = $(obj),
            text = $t.val(),
            len = text.length,
            left = maxLength - len,
            oText = $t.siblings('.d-result').find('em');
        if (left < 0) {
            text = text.substring(0, maxLength);
            $t.val(text);
            left = 0;
        }
        oText && oText.html(left);
    };

    /**
     * 返回
     */
    $('.d-back').bind('click', function () {
        window.history.back();
        return false;
    });
    /**
     * 退出登录
     */
    $('.d-logout').bind('click', function () {
        singer.cookie.set('__dayeasy_u', '', 0.1, singer.sites.domain);
        location.href = '/page/account/login.html?back=' + encodeURIComponent(location.href);
    });

    /**
     * UI初始化限制字符
     */
    $(document)
        .delegate('textarea[maxlength],input[maxlength]', 'keyup', function () {
            textMax(this, ~~$(this).attr('maxlength'));
        });


//    var configure=new Object();
//    configure.markingIcon:'http://static.dayeasy.net/v1/image/icon/marking/';

})(jQuery, window);

//百度统计
var _hmt = _hmt || [];
(function () {
    var hm = document.createElement("script");
    hm.src = "//hm.baidu.com/hm.js?2ea1ecd3c9389f5127ffb27aa84c640a";
    var s = document.getElementsByTagName("script")[0];
    s.parentNode.insertBefore(hm, s);
})();