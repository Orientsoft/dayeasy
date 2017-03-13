/**
 * Created by bozhuang on 2016/9/7.
 */
(function ($, S){
    var t = S.now(), start = new Date('2016/12/26 08:00:00'), end = new Date('2017/01/09 0:00:00');
    if(t < start || t > end || location.href.indexOf('/act/poster') >= 0)
        return false;
    var $top       = $('#topBlock'),
        cookieName = '__activity_top_block',
        topBlock, bottomBlock;
    /**
     * 机构首页  个人首页弹出广告
     */
    topBlock       = function (){
        var htmlTopBlock = $('<div class="topblock">' +
            '<a target="blank" href="/act/poster"> ' +
            '<img class="imgtop" src="' + singer.sites.static + '/v3/image/activity/banner.jpg"/> ' +
            '</a> ' +
            '<div class="slide-close"> ' +
            '<div class="close-text">关闭后不再显示</div> ' +
            '</div>' +
            '</div>');
        $top.append(htmlTopBlock);
        $('body').on('click', '.slide-close', function (){
            $('.topblock').slideUp();
            S.cookie.set(cookieName, true, 30);
            bottomBlock && bottomBlock.call();
        });
        $(document).ready(function (){
            S.later(function (){
                htmlTopBlock.slideDown(1500);
            }, 2000);
        });
    };
    /**
     * 底部滑动广告
     */
    bottomBlock    = function (){
        var bottomblockHtml = ' <div class="bottomblock2016">' +
            '<div class="bottomblock2016Show">' +
            '<div class="bottomblock2016Bg"></div>' +
            '<div class="bottomblock2016Content">' +
            '<a href="/act/poster" target="_blank" class="bottomblock2016Banner"> </a>' +
            '<span class="bottomblock2016CloseBtn"></span>' +
            '</div>' +
            '</div>' +
            '</div>' +
            '<div class="bottomblock2016OpenBtn cloudy "></div>';

        $('body')
            .append(bottomblockHtml)
            .on('click', '.bottomblock2016OpenBtn', function (event){
                event.preventDefault();
                var $this            = $(this),
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
            })
            .on('click', '.bottomblock2016CloseBtn', function (){
                var $bottomblock2016        = $('.bottomblock2016'),
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
    var TeacherDay = {
        start: function (){
            if(!$top.length || S.cookie.get(cookieName)){
                bottomBlock();
                return false;
            }
            topBlock();

        }
    };
    if(!S.config('teachersDay') || !S.config('teachersDay').later){
        TeacherDay.start();
    } else {
        S._mix(S, {
            teachersDay: TeacherDay
        });

    }
})(jQuery, SINGER);



