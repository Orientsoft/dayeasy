(function ($, S) {
    var showLogin = false,
        logger = S.getLogger('js/base/site-base');
    /**
     * jQuery Ajax过滤器
     */
    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        //登录验证过滤
    });

    $(document).ajaxComplete(function (e, jqXHR) {
        var json = jqXHR.responseJSON;
        //json && logger.info(json);
        if (!showLogin && json && json.login) {
            showLogin = true;
            if (S.alert) {
                S.alert('登录已失效，请重新登录！', function () {
                    window.top.location.href = json.url;
                });
            } else {
                window.top.location.href = json.url;
            }
            return false;
        }
    });
    S._mix(S, {
        deyi: (function () {
            return window.DEYI || {};
        })(),
        sites: (function () {
            return (window.DEYI || {}).sites || {};
        })()
    });

    $(".top-menu").mouseenter(function () {
        $(this).addClass("menu-hover");
    }).mouseleave(function () {
        $(this).removeClass("menu-hover");
    });
    //退出帐号
    $(".b_logout").bind("click", function () {
        $(window).unbind("beforeunload.question");
        location.href = S.sites.login + '/logout';//'?return_url=' + encodeURIComponent(location.href);
        return false;
    });
    //菜单栏
    var reg = /([a-z]+)\.[a-z]+\.[a-z]+(?:\/([a-z0-9]+)\/?)?/gi,
        app;
    if (S.uri().app) {
        app = S.uri().app;
    } else {
        location.href.match(reg);
        app = ((RegExp.$1 && RegExp.$1 != "www") ? RegExp.$1 : RegExp.$2) || "home";
    }
    $('nav li a').each(function (i) {
        if (this.getAttribute("href").indexOf(app.toLowerCase()) >= 0) {
            $("nav li:eq(" + i + ")").addClass("active");
        }
    });
    //返回顶部
    if (S.deyi.goTop) {
        var $goTop = $('<div class="go-top"><i class="go-Top-icon"></i><i class=" fa-angle-topb">返回顶部</i></div>'),
            right,
            $window = $(window);
        //        if ($window.width() <= 1000) return false;
        $("body").append($goTop);
        if ($window.width() < 1000)
            right = 10;
        else
            right = Math.abs(($window.width() - 1000) / 2 - 60);
        $goTop.css("right", right);
        $goTop
            .bind("click.goTop", function () {
                $('body,html').animate({ scrollTop: 0 }, "fast");
            })
            .hover(function () {
                $(this).toggleClass("go-top-hover");
            });
        $window.bind("scroll.goTop", function () {
            var top = $(this).scrollTop();
            if (top > 100) {
                $goTop.fadeIn();
            }
            else $goTop.fadeOut();
        });
    }
    S._mix(S, {
        /**
         * 缩略图
         * @param url 原图链接
         * @param width 宽度
         * @param height 高度
         * @returns {string} 缩略图链接
         */
        makeThumb: function (url, width, height) {
            if (!url) return url;
            width = width || 50;
            height = height || 'auto';
            return url.replace(/(\.[a-z]{3,4})$/gi, '_s' + width + 'x' + height + '$1');
        },
        /**
         * 格式化数字
         * @param num
         * @returns {*}
         */
        formatNum: function (num) {
            if (!/^(\+|-)?\d+(\.\d+)?$/.test(num))
                return num;
            var re = new RegExp().compile("(\\d)(\\d{3})(,|\\.|$)");
            num += "";
            while (re.test(num))
                num = num.replace(re, "$1,$2$3");
            return num;
        },
        /**
         * 分页计算
         * @param option 配置
         */
        page: function (option) {
            var opt = $.extend({}, {
                current: 0,     //当前页
                size: 15,       //每页显示数
                total: 0        //总数
            }, option || {}),
                totalPage,
                pages = {
                    prev: false,
                    next: false,
                    current: 1,
                    data: []
                },
                i;
            if (opt.total <= 0 || opt.size <= 0) return pages;
            totalPage = Math.ceil(opt.total / opt.size);
            if (totalPage < 2) return pages;
            if (opt.current < 0) opt.current = 0;
            if (opt.current >= totalPage) opt.current = totalPage - 1;
            pages.prev = opt.current > 0;
            pages.next = opt.current < totalPage - 1;
            pages.current = opt.current + 1;
            if (totalPage < 9) {
                for (i = 0; i < totalPage; i++) {
                    pages.data.push({
                        page: i + 1,
                        isActive: opt.current == i
                    });
                }
                return pages;
            }
            pages.data.push({
                page: 1,
                isActive: opt.current == 0
            });
            if (opt.current - 3 > 1) {
                pages.data.push({ page: -1 });
            }
            for (i = opt.current - 3; i < opt.current + 4; i++) {
                if (i < 1 || i > totalPage - 2) continue;
                pages.data.push({
                    page: i + 1,
                    isActive: opt.current == i
                });
            }
            if (opt.current + 5 < totalPage) {
                pages.data.push({ page: -1 });
            }
            pages.data.push({
                page: totalPage,
                isActive: opt.current == totalPage - 1
            });
            return pages;
        },
        /**
         * 学段
         */
        stages: [
            { id: 1, name: '小学' },
            { id: 2, name: '初中' },
            { id: 3, name: '高中' }
        ],
        open: function (url) {
            var $a = $('<a>');
            $a.attr("href", url);
            $a.attr("target", "_blank");
            $("body").append($a);
            $a[0].click();
            $a.remove();
        },
        templates: {},
        /**
         * 加载模版
         * @param name
         * @param callback
         */
        loadTemplate: function (name, callback) {
            var url = S.format('{0}/data/templates/{1}.html', S.sites.static, name);
            $.get(url, function (data) {
                var $list = $(data);
                for (var i = 0; i < $list.length; i++) {
                    var item = $list.eq(i),
                        id = item.attr("id"),
                        html = (item.html() || "").replace(/^([\s\n]+)|([\s\n]+)$/i, '');
                    if (id) {
                        S.templates[id] = html;
                        if (typeof (template) !== 'undefined') {
                            template.cache[id] = template.compile(html, {
                                filename: id
                            });
                        }
                    }
                }
                callback && S.isFunction(callback) && callback.call(this);
            });
        },
        /**
         * 渲染模版
         * @param name
         * @param data
         * @returns {*}
         */
        render: function (name, data) {
            if (!S.templates.hasOwnProperty(name))
                return '';
            if (typeof (template) !== "undefined") {
                template.config('compress', true);
                data.sites = S.sites;
                return template(name, data);
            }
            return '';
        }
    });

    /**
     * placeholder
     */
    $.fn.extend({
        "placeholder": function (options) {
            options = $.extend({
                placeholderColor: '#ACA899',
                isUseSpan: false, //是否使用插入span标签模拟placeholder的方式,默认false,默认使用value模拟
                onInput: true  //使用标签模拟(isUseSpan为true)时，是否绑定onInput事件取代focus/blur事件
            }, options);

            $(this).each(function () {
                var _this = this;
                var supportPlaceholder = 'placeholder' in document.createElement('input');
                if (!supportPlaceholder) {
                    var defaultValue = $(_this).attr('placeholder');
                    var defaultColor = $(_this).css('color');
                    if (options.isUseSpan == false) {
                        $(_this).focus(function () {
                            var pattern = new RegExp("^" + defaultValue + "$|^$");
                            pattern.test($(_this).val()) && $(_this).val('').css('color', defaultColor);
                        }).blur(function () {
                            if ($(_this).val() == defaultValue) {
                                $(_this).css('color', defaultColor);
                            } else if ($(_this).val().length == 0) {
                                $(_this).val(defaultValue).css('color', options.placeholderColor)
                            }
                        }).trigger('blur');
                    } else {
                        var $imitate = $('<span class="wrap-placeholder" style="position:absolute; display:inline-block; overflow:hidden; color:' + options.placeholderColor + '; width:' + $(_this).outerWidth() + 'px; height:' + $(_this).outerHeight() + 'px;">' + defaultValue + '</span>');
                        $imitate.css({
                            'margin-left': $(_this).css('margin-left'),
                            'margin-top': $(_this).css('margin-top'),
                            'font-size': $(_this).css('font-size'),
                            'font-family': $(_this).css('font-family'),
                            'font-weight': $(_this).css('font-weight'),
                            'padding-left': parseInt($(_this).css('padding-left')) + 2 + 'px',
                            'line-height': _this.nodeName.toLowerCase() == 'textarea' ? $(_this).css('line-weight') : $(_this).outerHeight() + 'px',
                            'padding-top': _this.nodeName.toLowerCase() == 'textarea' ? parseInt($(_this).css('padding-top')) + 2 : 0
                        });
                        $(_this).before($imitate.click(function () {
                            $(_this).trigger('focus');
                        }));

                        $(_this).val().length != 0 && $imitate.hide();

                        if (options.onInput) {
                            //绑定oninput/onpropertychange事件
                            var inputChangeEvent = typeof (_this.oninput) == 'object' ? 'input' : 'propertychange';
                            $(_this).bind(inputChangeEvent, function () {
                                $imitate[0].style.display = $(_this).val().length != 0 ? 'none' : 'inline-block';
                            });
                        } else {
                            $(_this).focus(function () {
                                $imitate.hide();
                            }).blur(function () {
                                /^$/.test($(_this).val()) && $imitate.show();
                            });
                        }
                    }
                }
            });
            return this;
        }
    });
    $('[placeholder]').placeholder();
})(jQuery, SINGER);
