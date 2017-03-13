/**
 *  jQuery Tab
 * Created by Platon on 2014/11/20.
 *<link rel="stylesheet" href="../../source/plugs/jquery-tab/jquery.tab.3.0.css"/>
 *<script src="../../source/plugs/jquery-tab/jquery.tab.3.0.js"></script>
 *
 */
!function ($){
    $.fn.extend({
        tabcont: function (options){
            //全局变量
            var obj = $.extend({}, $.fn.tabcont.defaults, options),
                evt = obj.bind,
                btn = $(this).find(obj.btnClass),
                con = $(this).find(obj.conClass),
                onClass = obj.onClass,
                anim = obj.animation,
                conWidth = con.width(),
                conHeight = con.height(),
                len = con.children().length,
                sw = len * conWidth,
                sh = len * conHeight,
                i = 0,
                len, t, timer,
                $this = $(this);

            /*初始化*/
            var $ali = $this.find(btn).find('li'),
                aliwidth = $this.attr("data-dytab");
            $ali.width(aliwidth);
            /**
             * 添加nav calss 切换效果
             * @param _this
             */
            var onclassFun = function (_this){
                _this.addClass(onClass).siblings('li').removeClass(onClass);
            };
            //nav 动画
            function slide(_this){
                onclassFun(_this);
                if(_this.hasClass('slider')){
                    return;
                }
                var whatTab = _this.index();
                var howFar = _this.width() * whatTab;
                $this.find('.slider').css({
                    left: howFar + "px"
                });

            };
            /*滑动*/
            function animated(_this, e){
                slide(_this);
                $(".ripples").remove();
                var posX = _this.offset().left,
                    posY = _this.offset().top,
                    buttonWidth = _this.width(),
                    buttonHeight = _this.height();
                _this.prepend("<span class='ripples'></span>");
                if(buttonWidth >= buttonHeight){
                    buttonHeight = buttonWidth;
                } else {
                    buttonWidth = buttonHeight;
                }
                var x = e.pageX - posX - buttonWidth / 2;
                var y = e.pageY - posY - buttonHeight / 2;
                $(".ripples").css({
                    width: buttonWidth,
                    height: buttonHeight,
                    top: y + 'px',
                    left: x + 'px'
                }).addClass("rippleEffects");
            }

            return this.each(function (){
                var _this = $(this);
                _this.find('.htmleaf-content').on('click', 'li', function (e){
                    var $this = $(this);
                    animated($this, e);
                });
                //判断动画方向
                function judgeAnim(){
                    var w = i * conWidth,
                        h = i * conHeight;
                    switch (anim) {
                        case '0':
                            con.children().hide().eq(i).show();
                            break;
                        case 'left':
                            con.css({position: 'absolute', width: sw}).children().css({
                                float: 'left',
                                display: 'block'
                            }).end().stop().animate({left: -w}, obj.speed);
                            break;
                        case 'up':
                            con.css({
                                position: 'absolute',
                                height: sh
                            }).children().css({display: 'block'}).end().stop().animate({top: -h}, obj.speed);
                            break;
                        case 'fadein':
                            con.children().hide().eq(i).fadeIn();
                            break;
                    }
                }

                //判断事件类型
                if(evt == "hover"){
                    btn.children().on("mouseover mouseout", function (event){
                        if(event.type == "mouseover"){
                            //鼠标悬浮
                            var j = $(this).index();

                            function s(){
                                i = j;
                                judgeAnim();
                            }

                            timer = setTimeout(s, obj.delay);
                            slide($(this));
                        } else if(event.type == "mouseout"){
                            clearTimeout(timer);
                        }
                    })
                } else {
                    btn.children().bind(evt, function (){
                        i = $(this).index();
                        judgeAnim();
                    })
                }
                //自动运行
                function startRun(){
                    t = setInterval(function (){
                        i++;
                        //nav 动画
                        function slidea(_this, i){

                            if(_this.hasClass('slider')){
                                return;
                            }
                            if(i >= len){
                                var howFar = $ali.width() * 0;
                            } else {
                                var howFar = $ali.width() * i;
                            }
                            $(".slider").css({
                                left: howFar + "px"
                            });
                        };
                        slidea(_this, i);
                        if(i >= len){
                            switch (anim) {
                                case 'left':
                                    con.stop().css({left: conWidth});
                                    break;
                                case 'up':
                                    con.stop().css({top: conHeight});
                            }
                            i = 0;
                        }
                        judgeAnim();
                    }, obj.autoSpeed)
                }

                //如果自动运行开启，调用自动运行函数
                if(obj.auto){
                    $(this).hover(function (){
                        clearInterval(t);
                    }, function (){
                        //todo nav 添加效果onClass
                        startRun();
                    })
                    startRun();
                }
            })
            return this;

        }
    });
    // default options
    $.fn.tabcont.defaults = {
        btnClass: '.dy-tab-nav', /*按钮的父级Class*/
        conClass: '.dy-tab-con', /*内容的父级Class*/
        onClass: 'z-crt', /*当前按钮添加onClass*/
        bind: 'click', /*事件参数 click,hover*/
        animation: 'fadein', /*动画方向 left,up,fadein,0 为无动画*/
        speed: 300, /*动画运动速度*/
        delay: 200, /*Tab延迟速度*/
        auto: false, /*是否开启自动运行 true,false*/
        autoSpeed: 3000    /*自动运行速度*/
    };

    //调用
    $("[data-dytab]").tabcont();
}(window.jQuery);
/*
 <div class="htmleaf-containers" data-dytab="100">
 <div class="htmleaf-content bgcolor-3">
 <ul class="dy-tab-nav">
 <li class="z-crt"><a href="javascript:void(0);">错因及评论</a></li>
 <li><a href="javascript:void(0);">答案解析</a></li>
 <li class="slider">
 <span></span>
 </li>
 </ul>
 </div>
 <div class="tab-con">
 <div class="dy-tab-con">
 <div class="tab-con-item" style="display: block;"></div>
 <div class="tab-con-item">b</div>
 </div>
 </div>
 </div>
 */