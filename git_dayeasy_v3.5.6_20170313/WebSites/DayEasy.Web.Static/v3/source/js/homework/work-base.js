/**
 * 考试中心/批阅中心基础业务
 * Created by shay on 2016/3/18.
 */
(function ($, S) {
    var $header = $('.coach-base-title'),
        isPrint = $header.data('print'),
        batch = $header.data('batch'),
        paperId = $header.data('paper'),
        $sliders = $header.find('.slider-nav li'),
        $downloadPaper, $downloadErrors,
        downloadUrl = '/work/dowload?batch={0}&paperId={1}&type=6&eq={2}';

    if (isPrint) {
        //查找未提交的试卷类型
        $.get('/work/not-submit', {
            batch: batch
        }, function (json) {
            if (json.status && json.data) {
                $("#paperName").after('<div class="d-box-danger">' + json.data + '</div>');
            }
        });
    }

    /**
     * 下载全卷
     */
    $downloadPaper = $('#downPaper');
    if ($downloadPaper.length > 0) {
        $downloadPaper.bind("click", function () {
            S.confirm("确认下载该套试卷吗？", function () {
                S.open(S.format(downloadUrl, batch, paperId, 0));
            });
            return false;
        });
    }

    /**
     * 下载试卷错题
     */
    $downloadErrors = $('#downErrors');
    if ($downloadErrors.length > 0) {
        $downloadErrors.bind("click", function () {
            S.confirm("确认下载该套试卷的所有错题吗？", function () {
                S.open(S.format(downloadUrl, batch, paperId, 1));
            });
            return false;
        });
    }
    /**
     * 下载变式
     */
    $(".download-btn").click(function () {
        $(this).parent("form").submit();
        return false;
    });

    /**
     * 滑动导航
     */
    if ($sliders.length > 0) {
        $sliders.hover(function () {
            var $t = $(this), $slider, $navList, $parent = $t.parents('.slider-nav');
            if ($t.hasClass('z-crt'))
                return false;
            $slider = $t.siblings('.slider');
            $navList = $parent.find('li').not($slider);
            var index = $navList.index($t);
            $parent.toggleClass('offset-0' + (index > 0 ? index : ''));
        });
    }
    /**
     * header浮动
     */
    var headerOffset = $header.offset(),
        $w = $(window),
        scrollTop = $w.scrollTop();
    var headerFixed = function () {
        var isFixed = $header.data('fixed');
        if (scrollTop > headerOffset.top) {
            !isFixed && $header.css({
                position: 'fixed',
                top: 5,
                left: headerOffset.left,
                zIndex: 666
            }).data('fixed', true);
        } else {
            isFixed && $header.css({
                position: 'absolute',
                top: 0,
                left: 0,
                zIndex: 0
            }).data('fixed', false);
        }
    };
    headerFixed();
    $w.bind('scroll.header', function () {
        scrollTop = $w.scrollTop();
        headerFixed();
    });
    /**
     * 按钮切换 答案解析/错因分析
     */
    var $buttons = $('.questions-btn button');
    if ($buttons.length > 0) {
        $buttons.bind('click', function (event) {
            event.preventDefault();
            var $t = $(this),
                $contList = $t.parents('.questions-btn').siblings('.questions-bottom-cont');
            $t.blur().toggleClass('dy-btn-info').siblings().removeClass('dy-btn-info');
            var $cont = $contList.find('.questions-list-cont'),
                off = $t.is('.dy-btn-info');
            off ? ($cont.stop().slideUp().eq($t.index()).slideDown()) : ($cont.stop().slideUp());
        });
    }
})(jQuery, SINGER);