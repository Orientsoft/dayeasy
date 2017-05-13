(function ($, S) {
    S._mix(S, {
        sites: DEYI.sites || {}
    });
    /**
     * 加载主题
     * @returns {boolean}
     */
    var loadTheme = function () {
        var mode = S.cookie.get("theme-mode");
        if (!mode) mode = 'default';
        var $style = $("#theme-style");
        if (!$style.length) {
            $style = $('<link id="theme-style" rel="stylesheet" />');
            $("head").append($style);
        }
        $style.attr("href", S.sites.static + "/v3/source/css/management/theme/" + mode + ".css");
        var $t = $(".color-mode li[data-style='" + mode + "']");
        $t.addClass("current").siblings().removeClass("current");
        return true;
    };
    /**
     * 加载边栏状态
     * @param toggle
     */
    var loadSidebar = function (toggle) {
        var status = S.cookie.get("sidebar-mode") || 0;
        if (toggle) {
            status = !(status == 1);
            S.cookie.set("sidebar-mode", (status ? 1 : 0), 60 * 24 * 90);
        }
        var $container = $(".page-container");
        $container.toggleClass("sidebar-closed", (status == 1));
    };
    loadTheme();
    loadSidebar();
    /**
     *初始化状态
     */
    var locationHref = window.location.href,
        urlPath = window.location.pathname,
        $navigation = $('#navigation'),
        showNavigation;
    /**
     * 显示导航及标题
     */
    showNavigation = function (nav, title, category) {
        var $title = $navigation.find('.page-title'),
            $li = $navigation.find('.breadcrumb li');
        $title.find('span').html(nav);
        $title.find('small').html(title);
        if (category) {
            $li.eq(1).html(S.format('<span>{0}</span>', category));
            $li.eq(2).html(S.format('<span>{0}</span>', nav));
        } else {
            $li.eq(1).html(S.format('<span>{0}</span>', nav));
            $li.eq(2).remove();
        }
    };
    $(".page-sidebar>ul>li>a").each(function () {
        var $t = $(this),
            url = $t.attr('href');
        if (url.indexOf('/') >= 0 && url == urlPath) {
            $t.parent().addClass("active open");
            $t.append('<span class="selected" />');
            showNavigation($t.text(), $t.attr("title") || "");
            return false;
        }
        var $links = $t.next().find('a');
        $links.each(function () {
            var $link = $(this),
                link = $link.attr("href");
            if (locationHref.indexOf(link) < 0)
                return;
            $link.parent().addClass("active");
            $t.parent().addClass("active open");
            $(".arrow", parent).addClass("open").before("<span class='selected'></span>");
            showNavigation($link.text(), $link.attr("title") || "", $t.text());
        });
    });
    $('.page-sidebar .has-sub > a').click(function () {
        var handleContentHeight = function () {
            var $content = $('.page-content'),
                $sidebar = $('.page-sidebar');

            if (!$content.attr("data-height")) {
                $content.attr("data-height", $content.height());
            }
            if ($sidebar.height() > $content.height()) {
                $content.css("min-height", $sidebar.height() + 20);
            } else {
                $content.css("min-height", $content.attr("data-height"));
            }
        };
        $(this).blur();
        var $t = $(this).parent(),
            slideTime = 200,
            $sub = $t.find(".sub"),
            $arrow = $t.find(".arrow"),
            openCss = "open";
        if ($t.hasClass(openCss)) {
            $arrow.removeClass(openCss);
            $sub.slideUp(slideTime, function () {
                handleContentHeight();
                $t.removeClass(openCss);
            });
        } else {
            $arrow.addClass(openCss);
            var $last = $t.siblings('.open');
            if ($last.length) {
                $last.each(function (i, item) {
                    var $item = $(item);
                    if (!$item.hasClass("active")) {
                        $item.find(".arrow").removeClass(openCss);
                        $item.find(".sub").slideUp(slideTime, function () {
                            $item.removeClass(openCss);
                        });
                    }
                });
            }
            $sub.slideDown(slideTime, function () {
                handleContentHeight();
                $t.addClass(openCss);
            });
        }
    });
    $(".sidebar-toggler").bind("click", function () {
        loadSidebar(true);
    });
    $(".color-mode-icons").bind("click", function () {
        var $t = $(this),
            $mode = $t.siblings('.color-mode'),
            status = $t.hasClass("icon-color-close");
        if (status) {
            $t.removeClass("icon-color-close");
            $mode.addClass("hide");
        } else {
            $t.addClass("icon-color-close");
            $mode.removeClass("hide");
        }
    });
    $(".color-mode li").bind("click", function () {
        var $t = $(this),
            mode = $t.data("style"),
            isCurrent = $t.hasClass("current");
        if (isCurrent) return false;
        S.cookie.set("theme-mode", mode, 60 * 24 * 90);
        loadTheme();
        $(".color-mode-icons").click();
        return true;
    });
    $(".color-fixed input[type=checkbox]").bind("change", function () {
        var fixed = this.checked;
        $("body").toggleClass("fixed-top", fixed);
        $(".navbar-inverse").toggleClass("navbar-fixed-top", fixed);
    });
    if (toastr) {
        toastr.options = {
            closeButton: false,
            debug: false,
            progressBar: false,
            positionClass: "toast-top-center",
            onclick: null,
            showDuration: "300",
            hideDuration: "1000",
            timeOut: "2000",
            extendedTimeOut: "1000",
            showEasing: "swing",
            hideEasing: "linear",
            showMethod: "fadeIn",
            hideMethod: "fadeOut"
        };
    }
})(jQuery, SINGER);