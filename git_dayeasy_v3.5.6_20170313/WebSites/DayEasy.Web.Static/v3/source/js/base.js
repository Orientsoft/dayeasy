/**
 *整站脚本基类，基于jQuery
 */
(function ($, S){
    var showLogin = false,
        $document = $(document);
    /**
     * jQuery Ajax过滤器
     */
    $.ajaxPrefilter(function (options, originalOptions, jqXHR){
        //登录验证过滤
    });
    $document.ajaxComplete(function (e, jqXHR){
        var json = jqXHR.responseJSON;
        //json && logger.info(json);
        if(!showLogin && json && json.login){
            showLogin = true;
            if(S.alert){
                S.popupLogin();
                //S.alert('登录已失效，请重新登录！', function () {
                //    window.top.location.href = json.url;
                //});
            } else {
                window.top.location.href = json.url;
            }
            return false;
        }
        if(json && json.redirect){
            window.top.location.href = json.url;
            return false;
        }
    });
    // SINGER 扩展
    S._mix(S, {
        open: function (url){
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
         * @param url
         */
        loadTemplate: function (name, callback, url){
            url = url || S.format('{0}/data/templates/{1}.html', S.sites.static, name);
            //var url = S.format('{0}/data/templates/{1}.html', S.sites.static, name);
            $.get(url, function (data){
                var $list = $(data);
                for (var i = 0; i < $list.length; i++) {
                    var item = $list.eq(i),
                        id = item.attr("id"),
                        html = (item.html() || "").replace(/^([\s\n]+)|([\s\n]+)$/i, '');
                    if(id){
                        S.templates[id] = html;
                        if(typeof (template) !== 'undefined'){
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
        render: function (name, data){
            if(!S.templates.hasOwnProperty(name))
                return '';
            if(typeof (template) !== "undefined"){
                template.config('compress', true);
                data.sites = S.sites;
                return template(name, data);
            }
            return '';
        },
        /*弹窗登录*/
        popupLogin: function (){
            document.domain = S.sites.main.substr(S.sites.main.indexOf('.') + 1);
            var url = S.sites.account + "/login/popup";
            var $iframe = $('<iframe id="lgPupFrame" name="lgPupFrame" frameborder="0" scrolling="no" width="335" height="303"></iframe>');
            $iframe.attr("src", url);
            var dialog =
                S.dialog({
                    title: '登录',
                    content: $iframe,
                    onclose: function (){
                        showLogin = false;
                    },
                    fixed: true
                });
            $(dialog.node).find(".ui-dialog-body").attr("style", "padding:0;");
            dialog.showModal();
        }
    });
    // 对象扩展
    $.fn.extend({
        //按钮禁止
        disableField: function (word){
            var $t = $(this);
            $t.addClass('disabled').attr('disabled', 'disabled');
            if(word){
                $t.data('word', $t.html());
                $t.html(word);
            }
        },
        //解除按钮禁止
        undisableFieldset: function (){
            var $t = $(this);
            $t.removeClass('disabled').removeAttr('disabled');
            var word = $t.data('word');
            if(word){
                $t.html(word);
            }
        },
        /**
         * 最大输入字符
         * @param obj
         * @param maxLength
         */
        textMax: function (){
            var $t = $(this),
                maxLength = ~~$(this).attr('maxlength'),
                text = $t.val(),
                len = text.length,
                left = maxLength - len,
                oText = $t.siblings('.dy-result').find('em');
            if(left < 0){
                text = text.substring(0, maxLength);
                $t.val(text);
                left = 0;
            }
            oText && oText.html(left);
        }
    });
    // jQuert 对象扩展
    /*    $.extend({
     /!**
     *打印
     * @param message 传入类型
     *!/
     log: function (message){
     if(window.console && window.console.log){
     window.console.log(message);
     }
     }
     });*/

    /**
     * 需要首次加载的方法
     */
    var bindLoadDyFun = {
        /**
         * 消息数量
         */
        messageCount: function (){
            if(!!S.config('isHome')){
                return false;
            }
            var $messageCount = $('.dy-message-count');
            if(!$messageCount || !$messageCount.length)
                return false;
            $.ajax({
                type: "GET",
                url: S.sites.main + "/message/count",
                dataType: "jsonp",
                success: function (data){
                    if(~~data > 99){
                        data = '99+';
                        $messageCount.html(data);
                    }
                    if(data > 0){
                        $messageCount.addClass('po-detail-ba');
                    }
                }
            });
        },
        /**
         * footer 二维码滑动效果
         */
        qrCode: function (){
            //子集浮动撑开
            var $havechild = $("[data-menu]");
            $gul2 = $havechild.siblings('.g-ul-2');
            $gul2.width($havechild.outerWidth());
            //二维码
            $('.copy-right li').each(function (){
                var $t = $(this),
                    app = $t.find('.app-qrcode');
                $t.hover(function (){
                    app.stop().css({'opacity': '0', 'top': '-179px'}).show().animate({
                        'opacity': '1',
                        'top': '-145px'
                    }, 600)
                }, function (){
                    app.stop().css({'opacity': '1', 'top': '-145px'}).animate({
                        'opacity': '0',
                        'top': '-179px'
                    }, 600).hide();
                });
            });
        },
        /**
         * 退出登录
         */
        logout: function (){
            $('.dy-header').delegate('.d-logout', 'click', function (){
                $(window).unbind("beforeunload.question");
                location.href = S.sites.login + '/logout?return_url=' + encodeURIComponent(location.href);
                return false;
            });
        },
        /**
         * 返回顶部
         * singer.config({global:{goTop:true}})
         */
        goTop: function (){
            var globalConfig = S.config('global');
            if(!globalConfig || !globalConfig.goTop){
                return false;
            }
            var $goTop = $('.go-top'),
                right,
                $window = $(window);
            if(!$goTop.length){
                $goTop = $('<div class="go-top"><i class="iconfont dy-icon-gotop animated"></i><span>返回顶部</span></div>');
                $("body").append($goTop);
            }
            if($window.width() < 1280)
                right = 10;
            else
                right = Math.abs(($window.width() - 1200) / 2 - 60);
            $goTop.css("right", right);
            $goTop.bind("click.goTop", function (){
                $('body,html').animate({scrollTop: 0}, "500");
            }).hover(function (){
                $(this).toggleClass("go-top-hover");
            });
            $window.bind("scroll.goTop", function (){
                var top = $(this).scrollTop();
                if(top > 100){
                    $goTop.fadeIn();
                }

                else $goTop.fadeOut();
            });
        },
        /**
         *
         * @param obj   绑定 document对象上
         *  配合 /v3/html/ui/index.html DOM结构使用
         */
        bindDocument: function (){
            $document
            // input 模拟选中效果
                .delegate('.group-checkbox input', 'click', function (){
                    $(this).siblings('.iconfont').toggleClass('dy-icon-checkboxhv');
                })
                // input 模拟不选中效果
                .delegate('.group-radio input', 'click', function (){
                    $(this).siblings('.iconfont').addClass('dy-icon-radiohv');
                    $(this).addClass('dy-icon-radiohv').parents('.group-radio').siblings('.group-radio').children('.dy-icon-radio').removeClass('dy-icon-radiohv');
                })
                // textarea 限制最大字符
                .delegate('textarea[maxlength]', 'keyup', function (){
                    $(this).textMax();
                })
                /**
                 * 文本框得到失去焦点
                 */
                .on({
                    focus: function (){
                        var $t = $(this);
                        $t.addClass("focus").val() == this.defaultValue && $t.val("");
                    },
                    blur: function (){
                        var $t = $(this);
                        $t.removeClass("focus").val() == '' && $t.val(this.defaultValue);
                    },
                }, '[data-delval]');
        }
    };
    /**
     * 执行 bindLoadDyFun下所有函数
     */
    var initDyFun = function (){
        for (var attr in bindLoadDyFun) {
            bindLoadDyFun[attr]();
        }
    };
    //初始化
    initDyFun();


})(jQuery, SINGER);

