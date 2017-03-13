$(function() {
    //限制字符
    $.LimitFont();
    $('.ed-details-tab').EdDetailsTab();
    // task-center.html //Tab 选项卡
    $.TaskCenter();

    //老师可见
    $('.f-click').bind('click', function(event) {
        $(this).next().toggleClass('show');
    });

    //去除最后子元素边框
    $(".ed-comment .other-comments").children('.comments-list:last-child').addClass('last-child');
    $(".ed-comment").children('.ed-comment-list').addClass('last-child');

    // 列表中心
    $('.click-show').click(function() {
        $(this).find('span').text($(".u-open").is(":hidden") ? "收起" : "更多信息");
        $('.u-open').slideToggle();
    });


});




;(function($) {
    $.fn.extend({
        //Tab 选项卡
        "EdDetailsTab": function() {
            var This = $(this);
            This.find('.tab-menu li').bind('click', function() {
                $(this).addClass('z-crt').siblings().removeClass('z-crt');
                This.find('.tab-contenr').find('.tab-con').eq($(this).index()).addClass('show').siblings().removeClass('show');
            });
        }
      
        
    });

    $.extend({
        //限制字符
        "LimitFont": function() {
            var maxLength = 140;
            $('#limit').keyup(function(event) {
                var l = $(this).val().length;
                $('#f-subs').text(maxLength - l);
                if (parseInt($('#f-subs').text()) < 0) {
                    $('#f-subs').text('0');
                    var val = $(this).val().substring(0, 140);
                    $(this).val(val);
                };
            });
        },
        // task-center.html //Tab 选项卡
        "TaskCenter":function(){
            $('.task-cent-menu span').bind('click', function() {
                $(this).addClass('z-crt').siblings().removeClass('z-crt');
                $('.task-cent-teb').show()
                .find('.tab-con').eq($(this).index()).addClass('show').siblings().removeClass('show');
            });
            $('.f-posa-close').bind('click', function() {
                $(this).parents('.task-cent-teb').hide();
            });

        }
    });


})(jQuery);