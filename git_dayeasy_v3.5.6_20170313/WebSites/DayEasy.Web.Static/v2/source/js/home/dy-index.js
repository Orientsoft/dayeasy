/**
 *
 * @date    2014-12-09 14:00:13
 */
var pageIndex = 1,
    isMobai = 0;
var deyi = window.DEYI || {};
(function ($, D, S) {
    var load = function (userType, callback) {
        var urls = [],
            len = 0;
        if (userType == 4)
            urls.push('home-teacher');
        else
            urls.push('home-student');
        for (var i = 0; i < urls.length; i++) {
            S.loadTemplate(urls[i], function () {
                len++;
                if (len == urls.length)
                    callback && S.isFunction(callback) && callback.call(this);
            });
        }
    };
    S._mix(D, {
        loadTemplates: load,
        user: eval('(' + $("#userData").data("info") + ')'),
        render: function (name, data) {
            return S.render(name, data);
        }
    });
})(jQuery, deyi, SINGER);


$(function () {
    $('body').append('<div id="ajaxModel" style="z-index: 9999;position: absolute;" class="hide"><div class="text-center"><span id="dyloading" style="position:fixed z-index: 1000;"><i class="fa fa-spin fa-spinner fa-3x" ></i><br /><br />正在加载，请稍后...</span></div></div>');
    // 你想做什么呢？
    dy.app.wantTo();
    // left right scrollTop
    dy.app.Indexleft();
    //开小灶
    dy.app.Separate();
    //:Todo
    deyi.loadTemplates(deyi.user.isTeacher ? 4 : 2, function () {
        // 列表数据
        dy.adata.list(pageIndex);
    });
    // dy.app.Alone();
    //任务数据
    dy.adata.getStatistic();
    // // 开小灶模块添加  效果
    (function () {
        var obtn = $('#robot'),
            obtn1 = $('#robot .robot-bd .m-robot-1'),
            obtn2 = $('#robot .robot-bd .m-robot-2'),
            obtn3 = $('#robot .robot-bd .m-robot-3');
        obtn.hover(function () {
            var _this = $(this);
            _this.stop().animate({
                left: '-35px'
            }, 600);
            // _this.addClass('bg1');
        }, function () {
            $(this).stop().animate({
                left: '-75px'
            }, 600);
            // $(this).removeClass('bg1');
            obtn3.removeClass('bg2');
            obtn.find('.robot-bd').removeClass('bg3');
        });
        obtn1.on("click", function () {
            if (obtn3.hasClass("bg2")) {
                obtn3.removeClass('bg2');
                // obtn.removeClass('bg1');
                obtn.find('.robot-bd').removeClass('bg3');
            } else {
                // obtn.removeClass('bg1');
                obtn.find('.robot-bd').addClass('bg3');
                obtn3.addClass('bg2');
            }
        });

        function Aloness() {

            var ali = $('.m-robot-3').find('ul li'),
                odiv = $('#dy-robot'),
                odivfind = odiv.find('.alones-top .Close');
            // 机器人显示
            ali.click(function (event) {
                $('html,body').animate({
                    scrollTop: '0px'
                }, 50);
                odiv.css('display', 'block');
                odiv.children('div').css('display', 'none').eq($(this).index()).css('display', 'block');
            });
            // 关闭当前
            odivfind.click(function (event) {
                $(this).parents('#dy-robot').slideUp();
                return false;
            });
        }

        Aloness()

    })(jQuery);

    //加载更多
    $('#macktest').click(function (event) {
        pageIndex += 1;
        dy.adata.list(pageIndex);
    });

    // div居中

    jQuery.fn.center = function () {
        this.css("position", "fixed");
        this.css("top", ($(window).height() - this.height()) / 2 + $(window).scrollTop() + "px");
        this.css("left", ($(window).width() - this.width()) / 2 + $(window).scrollLeft() + "px");
        return this;
    };
    //加载图标居中
    $("#dyloading").center();
    //膜拜
    (function () {
        $('#Testcontent').delegate('.thumbs', 'click', function (event) {
            if (isMobai) {
                singer.alert("客官别急，正在处理...");
                return false;
            }
            isMobai = 1;
            var _this = $(this),
                num = 1,
                newsId = _this.data('newid'),
                hasSupport = singer.cookie.get(newsId + "_" + home_userId);
            _this.attr("disabled", "disabled");
            if (hasSupport) {
                num = -1;
            }
            $.post(singer.sites.main + "/home/support", {
                    newsId: newsId,
                    num: num
                },
                function (res) {
                    _this[0].removeAttribute("disabled");
                    isMobai = 0;
                    // console.log(res);
                    if (res.Status) {
                        var onumber = _this.find('b').text();
                        var count = parseInt(onumber) + num;
                        if (count < 0) {
                            count = 0;
                        }
                        var textStr = '<i class="icon_mb"></i> 膜拜（<b>' + count + '</b>）';
                        //操作成功，缓存cookie为一周
                        if (num > 0) {
                            _this.html('<span class="support">' + textStr.replace('膜拜', '已膜拜') + '</span>');
                            singer.cookie.set(newsId + "_" + home_userId, 1, 24 * 60 * 5);
                        } else {
                            _this.removeClass("support");
                            _this.html(textStr);
                            singer.cookie.clear(newsId + "_" + home_userId);
                        }
                    } else {
                        $.Dayez.msg(res.Message);
                    }
                });
        });
    })();
});
var dy = {};
//  ==============================adata=================================
dy.adata = {};

//获取任务
dy.adata.getStatistic = function () {
    //更新任务
    $.post(singer.sites.main + "/Home/GetTask", {}, function (res) {
        if (res.Status) {
            $("#classNum").text(res.Data.ClassNum);
            $("#paperNum").text(res.Data.PaperNum);
        }
    });
};
/**
 * 绑定消息模版
 * @param data
 */
dy.adata.tools = function (data) {
    if (!data || !data.Data) return false;
    var details = data.Data.NewsDetails,
        len = details.length,
        role = data.Data.UserRole,
        logger = singer.getLogger('index');
    for (var i = 0; i < len; i++) {
        var tempData = details[i];
        if (!tempData || !tempData.Details)
            continue;
        var name = singer.format('t{0}-{1}-{2}', role, singer.padLeft(tempData.NewsType, 3, '0'), tempData.SourceType);
        if (tempData.Details.Question == null && tempData.SourceType == 2) {
            tempData.Status = 4;
        }
        dy.adata.show(name, tempData);
    }
};

/**
 * 显示消息
 * @param tempName
 * @param data
 * @param type
 */
dy.adata.show = function (tempName, data, type) {
    var html = deyi.render(tempName, data);
    if ($.trim(type) && type == 'robot') {
        var $html = $(html).hide();
        $('#Testcontent').prepend($html);
        $html.slideDown(600, function () {
        });
    } else {
        $('#Testcontent').append(html);
    }
    $("#macktest").removeClass('hide');
    var thumbs = $('#Testcontent').find("a.thumbs");
    $.each(thumbs, function (index, item) {
        if ($(item).html() && $(item).html().indexOf("已膜拜") > 0) {
            if (!$(item).hasClass("support")) {
                $(item).addClass("support");
            }
        }
    });
};

dy.adata.list = function (pageIndex) {
    // list数据 
    $.ajax({
        url: singer.sites.main + "/Home/GetDynamicNews",
        data: {
            pageIndex: pageIndex
        },
        type: 'POST',
        beforeSend: function (res) {
            $("#ajaxModel").removeClass("hide");
        },
        success: function (res) {
            var data = res;
            var loadLead = function () {
                $("#macktest").remove();
                var noDatahtml = deyi.render('lead', {});
                $('#Testcontent').append(noDatahtml);
            };
            if (data.Data) {
                dy.adata.tools(data);
                if (data.Data.NewsDetails.length < 20 && pageIndex == 1) {
                    loadLead();
                }
            } else {
                loadLead();
                if (pageIndex > 1) {
                    $('#Testcontent').append("<div class='dy-system-3 dy-base text-center'>没有其他信息了啦~~ </div>");
                }
            }
            singer.loadFormula();
        },
        complete: function () {
            $("#ajaxModel").addClass("hide");
        }
    });
};
//  ==============================app===================================
dy.app = {};

// 机器人效果
dy.app.wantTo = function () {

    $(".service_box .tab_nav li").hover(function () {
        var i = $(this).index();
        $(this).addClass("active").siblings().removeClass("active");

        $(".service_box .tab_plan:eq(" + i + ")").show().siblings().hide();
    });
    var t;
    $(".service_box").hover(function () {
            $(".service_box").animate({
                    right: 0
                },
                300);
            clearTimeout(t);
        },
        function () {
            t = setTimeout(function (e) {
                    $(".service_box").animate({
                            right: "-138px"
                        },
                        300);
                    $(".service_box").find(".tab_nav li").removeClass("active");
                },
                1000);
        });

    var isTransition = true;
    $(".m_logo").hover(function () {
            var r = 0;
            if (isTransition) {
                animateTime = setInterval(function () {
                    if (r >= 153) {
                        clearInterval(animateTime);
                        isTransition = true;
                    } else {
                        isTransition = false;
                        r++;
                        $(".m_logo a").attr("style", "-webkit-mask:-webkit-gradient(radial, 45 25, " + r + ", 45 25, " + (r + 15) + ", from(rgb(0, 0, 0)), color-stop(0.5, rgba(0, 0, 0, 0.2)), to(rgb(0, 0, 0)));");
                    }
                }, 5);
            }
        },
        function () {
            return false;
        });
};

dy.app.Indexleft = function () {
    //左右浮动效果
    $(window).bind("scroll.indexleft", function () {
        setLeft();
    });
    var setLeft = function () {
        if ($(window).scrollTop() > $("header").height()) {
            $("header").addClass("scoll_nav");
            $('.dy-home-article').css({
                position: 'fixed',
                top: '105px'
            });
            $('.dy-home-section').css('margin-left', '155px');
        } else {
            $("header").removeClass("scoll_nav");
            $('.dy-home-article').css({
                position: 'relative',
                top: '0'
            });
            $('.dy-home-section').css('margin-left', '0');

        }
    };
    setLeft();
    (function ($) {
        // 右侧浮动
        $.fn.ofixed = function (topobj) {

            var obj = $(this);
            var offset = obj.offset(),
                topOffset = offset.top,
                leftOffset = offset.left;
            var marginTop = obj.css("marginTop"),
                marginLeft = obj.css("marginLeft");

            $(window).scroll(function () {
                var scrollTop = $(window).scrollTop();
                if (scrollTop >= topOffset) {

                    obj.css({
                        marginTop: 0,
                        top: topobj,
                        // marginLeft: leftOffset,
                        position: 'fixed'
                    });
                }
                if (scrollTop < topOffset) {

                    obj.css({
                        marginTop: marginTop,
                        marginLeft: marginLeft,
                        position: 'relative',
                        top: 0
                    });
                }
            });

        };
    })(jQuery);
    $('#container').ofixed(90);
};


//开小灶效果
dy.app.Separate = function () {
    $('#questionList').find('li:first').find('.foldContent').css('display', 'block').prev().find('b').addClass('glyphicon-chevron-down');
    $('#questionList').find('li>h3').click(function (event) {
        $(this).find('b').addClass('glyphicon-chevron-down').end()
            .next().slideDown('400')
            .parent().siblings().children('h3')
            .find('b').removeClass('glyphicon-chevron-down').end()
            .next().slideUp('400');
        return false;
    });
};

$(function () {
    $('.bojs').find('h4:eq(0)').addClass('hover');
    $('.bojs h4').click(function () {
        $(this).addClass('hover')
            .next().slideDown('400')
            .parent().siblings().children('h4').removeClass('hover')
            .next().slideUp('400');
        return false;
    });
});


// 开小灶模块添加
// dy.app.Alone = function () {
//     var op = $('#dyrobot').find('.tab_plan p');
//     op.addClass('dyrobotoff')
//     op.click(function (e) {
//         if ($(this).hasClass('dyrobotoff')) {
//             $(this).removeClass('dyrobotoff').siblings().addClass('dyrobotoff');
//             e.stopPropagation();
//             var type = $(this).data("type");
//             dy.adata.show(type, {}, 'robot');
//             return false;
//         }
//     });
// };


// ==============================Test=================================
// 编译时间
template.helper('dateFormat', function (date, format) {
    date = parseInt(date.replace("\/Date(", "").replace(")\/", ""));
    date = new Date(date);

    var map = {
        "M": date.getMonth() + 1, //月份 
        "d": date.getDate(), //日 
        "h": date.getHours(), //小时 
        "m": date.getMinutes(), //分 
        "s": date.getSeconds(), //秒 
        "q": Math.floor((date.getMonth() + 3) / 3), //季度 
        "S": date.getMilliseconds() //毫秒 
    };


    format = format.replace(/([yMdhmsqS])+/g, function (all, t) {
        var v = map[t];
        if (v !== undefined) {
            if (all.length > 1) {
                v = '0' + v;
                v = v.substr(v.length - 2);
            }
            return v;
        } else if (t === 'y') {
            return (date.getFullYear() + '').substr(4 - all.length);
        }
        return all;
    });
    return format;
});

//数字转字母
template.helper("fromCharCode", function (num, format) {
    format = String.fromCharCode(num + 65);
    return format;
});

//是否膜拜
template.helper("supportFormat", function (newsId, format) {
    format = "膜拜";

    var hasSupport = singer.cookie.get(newsId + "_" + home_userId);
    if (hasSupport) {
        format = "已膜拜";
    }
    return format;
});