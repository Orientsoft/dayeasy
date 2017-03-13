/**
 * Created by bozhuang on 2016/8/9.
 */
$(document).ready(function () {
    // 单页分屏配置
    $('#fullpage').fullpage({
        sectionsColor: ['#fff', '#fff', '#fafafa', '#fff', '#fafafa'],
        css3: true,
        autoScrolling: false,       // 显示游览器默认滚动条
        navigation: true,       // 开启菜单导航图标
        scrollBar: true,
        fitToSection: false,         // false  滚动条可以停留到页面任何位置
        afterLoad: function (anchorLink, index) {
            var loadedSection = $(this);
            //using index
            if (index == 1) {

            }
            if (index == 2) {

            }
            if (index == 3) {

            }
            if (index == 4) {

            }
            //using anchorLink
            if (anchorLink == 'secondSlide') {
            }
        }
    });
    //搜索框
    var $selectbox = $('.selectbox');
    $selectbox.wrap('<div class="select_wrapper" ></div>')
    $('.select_wrapper').append('<span class="btn-soso"><i class="iconfont dy-icon-search"></i></span>');
    $selectbox.parent().prepend('<input type="text" property="热门"/>');
    $selectbox.css('display', 'none');
    $selectbox.parent().append('<ul class="select_inner"></ul>');
    $selectbox.children().each(function () {
        var opttext = $(this).text();
        var optval = $(this).val();
        $selectbox.parent().children('.select_inner').append('<li id="' + optval + '"> <span class="xiaoxue">小学</span>' + opttext + '</li>');
    });
    $selectbox.parents('.soso-index').find('.btn-soso').on('click', function (event) {
        event.stopPropagation();
        $('.select_inner').slideToggle('fast');
    });
    $(document).on('click', function () {
        $(".select_inner").hide();
    });
    // 滚动条加载效果
    if (!(/msie [6|7|8|9]/i.test(navigator.userAgent))) {
        new WOW().init();
    }
    //转盘效果
    $('.m-index-nav').find('.nav-list li').on('mouseover', function (event) {
        event.preventDefault();
        var index = $(this).index(),
            t = "on" + index;
        $('.m-index-data > div').removeClass().addClass(t)
    });
    // TagCanvas 标签展示效果
//模拟数据
    var schoolNumber = '16589';
    $('#schoolNumber').text(schoolNumber);
});
(function ($) {
    //logo
    try {
        TagCanvas.Start('logo-list', 'linkContainer', {
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
        });
    } catch (e) {
        // in Internet Explorer there is no canvas!
        document.getElementById('logo-list').style.display = 'none';
        document.getElementById('linkContainer').style.display = 'none';
        document.getElementById('logo-static').style.display = 'block';
    }
})(jQuery);
