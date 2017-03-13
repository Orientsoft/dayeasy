/**
 * 报表中心js效果
 * @name BZ
 * @create 2015/6/11
 */

$(function () {
    // 右侧滑动效果
    $('.dy-container .scoTop').ofixed(0);
});

; (function ($) {

    $.fn.extend({
        "ofixed": function (topobj) {
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

        }
    });
})(jQuery);
