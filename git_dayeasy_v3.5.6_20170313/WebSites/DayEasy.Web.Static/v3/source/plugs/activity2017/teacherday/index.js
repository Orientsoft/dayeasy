/**
 * Created by bozhuang on 2016/9/7.
 */
(function ($, S){

    var $top = $('#bottomBlock'),
        bottomBlock;
    var t = S.now(), start = new Date('2016/12/26 08:00:00'), end = new Date('2017/01/16 0:00:00');
    if(t < start || t > end)
        return false;
    /**
     * 底部滑动广告
     */
    bottomBlock = function (){
        var bottomblockHtml = ' <div class="bottomblock2016">' +
            '<div class="bottomblock2016Show">' +
            '<div class="bottomblock2016Bg"></div>' +
            '<span class="bottomblock2016CloseBtn hide"></span>' +
            '<div class="bottomblock2016Content">' +
            '<a href="javascript:void(0);"   class="bottomblock2016Banner"> </a>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '<div class="bottomblock2016OpenBtn cloudy "></div>';

        $('body')
            .append(bottomblockHtml)
            .on('click', '.bottomblock2016OpenBtn', function (event){
                event.preventDefault();
                var $this = $(this),
                    $bottomblock2016 = $('.bottomblock2016');
                $this.animate({
                    'left': '-100%'
                }, 300, function (){
                    $this.css('display', 'none');
                    $bottomblock2016.css('display', 'block');
                });
                $bottomblock2016.animate({
                    'left': '0'
                }, 200);

                if($('#bottomBlock').data('isstudentindex') !== 'StudentIndex'){
                    $('.bottomblock2016CloseBtn').addClass('show');
                } else {
                    $('.bottomblock2016CloseBtn').remove();
                    $('.page-index-teacher').find('.g-content-teacher').css('margin-bottom', '65px');
                }
            })
            .on('click', '.bottomblock2016CloseBtn', function (){
                var $bottomblock2016 = $('.bottomblock2016'),
                    $bottomblock2016OpenBtn = $('.bottomblock2016OpenBtn');
                $bottomblock2016.animate({
                    'left': '-100%'
                }, 200, function (){
                    $bottomblock2016.css('dispaly', 'none');
                    $bottomblock2016OpenBtn.css('display', 'block');
                });
                $bottomblock2016OpenBtn.animate({
                    'left': '0'
                }, 200);

            });
        setTimeout(function (){
            $('.bottomblock2016OpenBtn').removeClass('cloudy').addClass('cloudys');
        }, 3000);
    };
    var StudentDay = {
        start: function (){
            if($top.length > 0){
                bottomBlock();
            }
        }
    };
    StudentDay.start();
})(jQuery, SINGER);



