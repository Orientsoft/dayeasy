/**
 * Created by bozhuang on 2016/8/10.
 */

(function ($,S){

    $('.autograph-input').focus(function (){
        event.stopPropagation();
        var $t = $(this);
        $t.addClass("on");
        $t.val() == this.defaultValue && $t.val("");
        var $edit=  $(this).siblings('i');
        $edit.toggleClass('dy-icon-index-edit');
        $edit.toggleClass('dy-icon-13');
    }).blur(function (){
        event.stopPropagation();
        var $t = $(this);
        $t.removeClass('on');
        $t.val() == '' && $t.val(this.defaultValue);
        var $edit=  $(this).siblings('i');
        $edit.toggleClass('dy-icon-index-edit');
        $edit.toggleClass('dy-icon-13');
        $edit.removeClass('show');
    }).hover(function (){
        $(this).siblings('.dy-icon-index-edit').addClass('show');
    }, function (){
        $(this).siblings('.dy-icon-index-edit').removeClass('show');
    });


    $(document).on('click', function (){
        $(".select_inner").hide();
    });

    // 印象贴纸-POP-SHOW
//    $('.sticker').on('click', '.js-sticker-show', function (event){
//        event.preventDefault();
//        console.log('aa');
//        var $this   = $(this),
//            $parent = $this.parents('.sticker-list'),
////            $parents = $parent.parents('.sticker-list'),
//            $target = $parent.find('.pop-cont');
//        $(this).toggleClass('dy-icon-angleright');
//        $(this).toggleClass('dy-icon-anglebottom');
//
//        $target.show();
//    });

    //相关人气老师 Tab
    jQuery(".new-actives").slide({
        mainCell : ".tab-bd-in",
        effect   : "left",
        delayTime: 400,
        pnLoop   : false,
        easing   : "easeOutCubic"
    });



//  var  html = template('historyTpl', {});

    $('.pop-text').click(function(event) {
        S.dialog({
            title: '完善历程',
            content: '展示没有内容',
            cancelValue: '取消',
            cancelDisplay: false,
            onshow: function () {
                var $node = $(this.node);
                //$node.find('.con-list').hover(function () {
                //    $(this).find('.current-class').toggleClass('hide');
                //});
                $node.find('.b-edit,.add-school').bind('click', function () {
                    var $list = $(this).parents('.con-list'),
                        $time = $list.find('.time-class'),
                        $edit = $list.find('.edit-box');
                    $edit.add($time).toggleClass('hide');
                });
            }
        }).showModal();
        return false;
    });


})(jQuery,SINGER);




