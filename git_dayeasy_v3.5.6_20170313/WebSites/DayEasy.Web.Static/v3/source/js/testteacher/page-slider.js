/**
 * Created by bozhuang on 2015/11/16.
 */
$(function(){
    // 填空题 tab
    ;(function() {

        var  $button = $('.dy-questions-list .questions-btn').children('button');
        //初始化
        $('.g-main-list .dy-questions-list').each(function(){
            $(this).find('.cont-list').last().addClass("last-item");
        });
        $button.on('click', function(event) {
            event.preventDefault();
            $(this).toggleClass('dy-btn-info').siblings().removeClass('dy-btn-info');
            var $Otabcont =$(this).parents('.questions-btn').next('.questions-bottom-cont').children('.questions-list-cont'),
                off = $(this).is('.dy-btn-info');
            off?($Otabcont.stop().slideUp().eq($(this).index()).slideDown()):($Otabcont.stop().slideUp());
        });
    })();
    //扇形图展示
    ;(function() {
        $('.c-click-li').on('click', 'li', function(event) {
            event.preventDefault();
            $(this).parents('.default2').removeClass('default2').siblings().removeClass('default1')
        });
    })();
    //变式过关 input-checkbox
    ;(function() {
        $('.checkbox-questions').on('click', function(event) {
            event.preventDefault();
            $(this).toggleClass('checkboxhv');
        });
    })();
});
