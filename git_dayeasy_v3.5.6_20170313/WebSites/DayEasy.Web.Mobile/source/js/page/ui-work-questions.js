/**
 * Created by Boz 2016/5/23.
 */
$(function () {
    (function ($) {
        //Tab选项卡  查看答案-错因分析 （试卷分析）
        $('body').on('click', '.tab-menu li a', function () {
            var $oli = $(this).parents('li');
            var index = $oli.index();
            var $tab_parent = $(this).parents('.dy-questions-con-list');
            var $tab_conten_body = $tab_parent.find('.tab-conten .tab-base');
            var $li_index = $tab_conten_body.eq(index);
            $oli.addClass('on').siblings().removeClass('on');
            if ($li_index.is(':hidden')) {
                $li_index.show().siblings().hide();
            }
            else {
                $li_index.hide();
                $oli.removeClass('on');
            }
        });

    })(jQuery);
});


