/**
 * 网站首页
 * Created by shay on 2016/8/15.
 */
(function ($, S) {
    var pageLoad, fullPage, searchAgency, coverageData,
        sites = S.sites || {}, showUser, showMsg;
    /**
     * 未读消息
     */
    showMsg = function (count) {
        var $messageCount = $('.dy-message-count');
        if (~~count > 99)
            count = '99+';
        $messageCount.html(count);
        if (count > 0) {
            $messageCount.addClass('po-detail-ba');
        }
    };
    /**
     * 用户信息
     */
    showUser = function (pageData) {
        if (!pageData || pageData.userId <= 0)
            return false;
        $('.head-user-notlogged').remove();
        $('.ul-user-box').find('li.hide').removeClass('hide');
        if (pageData.isTeacher) {
            $(".footer-tag-list").append('<a target="_blank" href="' + sites.open + 'download?type=20">扫描工具</a>');
        }
        $('.ul-nav-box').find('.hide').removeClass('hide');
        var head = pageData.avatar || sites.static + "/v3/image/default/user_s40x40.jpg";
        $('.user-img').find('img').attr('src', head);
        showMsg(pageData.messageCount);
        return true;
    };
    /**
     * 页面加载
     */
    pageLoad = function (pageData) {
        if (!pageData)
            return false;
        showUser(pageData);
        var list = ['targetCount', 'userCount', 'agencyCount'];
        S.each(list, function (key) {
            $('#' + key).html(pageData[key].toString().replace(/\B(?=(\d{3})+(?!\d))/g, ","));
        });
        if (pageData.hotAgencies) {
            var $hotAgencies = $('#hotAgencies');
            S.each(pageData.hotAgencies, function (name, id) {
                $hotAgencies.append(S.format('<dd><a target="blank" href="{0}/agency/{1}">{2}</a></dd>', S.sites.main, id, name));
            });
        }
    };
    /**
     * 全屏插件
     */
    fullPage = function () {
        // 单页分屏配置
        $('#fullpage').fullpage({
            sectionsColor: ['#fff', '#fff', '#fafafa', '#fff', '#fafafa'],
            css3: true,
            autoScrolling: false,       // 显示游览器默认滚动条
            navigation: true,       // 开启菜单导航图标
            scrollBar: true,
            fitToSection: false,         // false  滚动条可以停留到页面任何位置
            afterLoad: function (anchorLink, index) {
            }
        });
    };
    /**
     * 机构搜索
     */
    searchAgency = function () {
        var $input = $('.key-agency'),
            $inner = $(".select_inner"),
            $btn = $('.b-search-agency'),
            lastWord,
            search,
            showAgency;
        showAgency = function (data) {
            $inner.empty();
            if (!data) {
                $inner.slideUp('fast');
                return false;
            }
            if (data.length == 0) {
                $inner.append('<li class="no-result">没有找到相关学校</li>');
            }
            else {
                var stages = ['小学', '初中', '高中'];
                S.each(data, function (item) {
                    item.stageCn = stages[item.stage - 1];
                    item.site = S.sites.main;
                    $inner.append(S.format('<li><a href="{site}/agency/{id}" target="_blank"><span class="stage-{stage}">{stageCn}</span>{name}</a></li>', item));
                });
            }
            $inner.slideDown('fast');
        };
        search = function (btnEvent) {
            var keyword = S.trim($input.val());
            if (!keyword) {
                showAgency();
                return false;
            }
            if (!btnEvent && keyword == lastWord)
                return false;
            $.ajax({
                type: "GET",
                data: {keyword: keyword},
                url: sites.main + "/agency-search",
                dataType: "jsonp",
                success: function (json) {
                    lastWord = keyword;
                    showAgency(json);
                }
            });
        };
        //搜索框
        $input.bind('keyup', function (e) {
            search();
        });
        $btn.bind('click', function () {
            $inner.show();
            search();
        });
        $(document).mouseup(function (e) {
            var _con = $input.add($btn); // 设置目标区域
            if (!_con.is(e.target) && _con.has(e.target).length === 0) {
                $inner.hide();
            }
        });
    };
    /**
     * 数据覆盖
     */
    coverageData = function () {
        var $logoList = $('#logo-list');
        //logo
        try {
            $logoList.tagcanvas({
                interval: 40,
                maxSpeed: 0.005,
                fadeIn: 100,
                depth: 0.0,
                initial: [0.9, -0.1],
                stretchX: 1.9,
                imageScale: 0.5,
                outlineMethod: 'none',
                activeCursor: 'default',
                wheelZoom: false
            }, 'linkContainer');
        } catch (e) {
            // in Internet Explorer there is no canvas!
            $logoList.remove();
            $('#linkContainer').remove();
            $('#logo-static').css({
                display: 'block'
            });
        }
    };

    fullPage();
    searchAgency();
    coverageData();
    /**
     * 首页topLink向下滚动一屏
     */
    $("a.topLink").click(function () {
        $("html, body").animate({
            scrollTop: $($(this).attr("href")).offset().top + "px"
        }, {
            duration: 500,
            easing: "swing"
        });
        return false;
    });
    // 滚动条加载效果
    if (!(/msie [6|7|8|9]/i.test(navigator.userAgent))) {
        new WOW().init();
    }
    /**
     * 推荐榜样-微信二维码弹框
     */
    $('.g-index-title-5').on('click', '.m-example', function (event) {
        event.preventDefault();
        var html = '<div class="pop-m-example"></div>';
        S.dialog({
            title: '推荐榜样',
            quickClose: true,
            backdropOpacity: 0.6,
            content: html,
            cancelValue: '取消',
            cancelDisplay: false,
            padding: 0
        }).showModal();
    });
    //转盘效果
    $('.m-index-nav').find('.nav-list li').on('mouseover', function (event) {
        event.preventDefault();
        var index = $(this).index(),
            t = "on" + index;
        $('.m-index-data > div').removeClass().addClass(t)
    });
    $.ajax({
        type: "GET",
        url: sites.main + "/home-data",
        dataType: "jsonp",
        success: function (json) {
            if (json.status) {
                pageLoad(json.data);
            }
        }
    });


})(jQuery, SINGER);

