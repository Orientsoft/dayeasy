/**
 * Created by bozhuang on 2015/11/17.
 */
$(function(){
   //个人设置
    (function(){

        $('.mian-cont').find('.iconfont').each(function(){
            $(this).click(function() {
                $(this).parent('p').addClass('z-sel').parent('li').siblings().children('p').removeClass('z-sel');
                $(this).siblings('input,textarea').focus();
            });

        });

    })();

});