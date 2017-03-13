/**
 * 依赖JQuery SINGER
 * Created by shay on 2015/12/9.
 */

(function ($, S){
    $.fn.extend({
        jqbar  : function (options){
            var settings = $.extend({
                animationSpeed  : 2000,
                barLength       : 250,
                orientation     : 'h',
                barWidth        : 30,
                barColor        : 'red',
                label           : '&nbsp;',
                value           : 1000,
                type            : 1,
                allscore        : 150,
                average         : 0,
                averageScoreDiff: 0
            }, options);
            return this.each(function (){
                var valueLabelHeight  = 0;
                var progressContainer = $(this);
                settings.average == 0 && (settings.average = 10);
                settings.value < parseInt(settings.average) && (settings.barColor = '#ed5565');
                if(settings.type == 1 || settings.type == 3){
                    var valueold   = settings.value;
                    settings.value = settings.value / settings.allscore * 100;
                }
                if(settings.type == 2){
                var    valueolds= settings.value;
                }

                if(settings.orientation == 'h'){
                    console.log('横向统计-暂无开发');
                    progressContainer.addClass('jqbar horizontal').append('<span class="bar-label"></span><span class="bar-level-wrapper"><span class="bar-level"></span></span><span class="bar-percent"></span>');

                    var progressLabel      = progressContainer.find('.bar-label').html(settings.label);
                    var progressBar        = progressContainer.find('.bar-level').attr('data-value', settings.value);
                    var progressBarWrapper = progressContainer.find('.bar-level-wrapper');
                    progressBar.css({height: settings.barWidth, width: 0, backgroundColor: settings.barColor});

                    var valueLabel = progressContainer.find('.bar-percent');
                    valueLabel.html('0');
                }
                else {
                    /*相差分*/

                    if(settings.type == 1){
                        progressContainer.addClass('jqbar vertical').append('<span class="bar-percent"></span><span class="bar-max-min">相差：<b>43.5</b>分</span><span class="bar-level-wrapper"><span class="bar-level"></span></span><span class="bar-label"></span>');
                    } else {
                        progressContainer.addClass('jqbar vertical').append('<span class="bar-percent"></span><span class="bar-level-wrapper"><span class="bar-level"></span></span><span class="bar-label"></span>');
                    }
                    var progressLabel      = progressContainer.find('.bar-label').html(settings.label);
                    var progressBar        = progressContainer.find('.bar-level').attr('data-value', settings.value);
                    var progressBarWrapper = progressContainer.find('.bar-level-wrapper');
                    progressContainer.css('height', settings.barLength);
                    progressBar.css({
                        height         : settings.barLength,
                        top            : settings.barLength,
                        width          : settings.barWidth,
                        backgroundColor: settings.barColor
                    });
                    progressBarWrapper.css({height: settings.barLength, width: settings.barWidth});

                    var valueLabel   = progressContainer.find('.bar-percent');
                    valueLabel.html('0');
                    valueLabelHeight = parseInt(valueLabel.outerHeight());
                    valueLabel.css({top: (settings.barLength - valueLabelHeight) + 'px'});
                    /*相差分*/
                    if(settings.type == 1){
                        var maxmin = progressContainer.find('.bar-max-min');
                        maxmin.css({top: (settings.barLength - valueLabelHeight) + 'px'});
                    }
                }

                animateProgressBar(progressBar);

                function animateProgressBar(progressBar){
                    var level = parseInt(progressBar.attr('data-value'));
                    if(level > 100){
                        level = 100;
                        S.msg('最大值' + settings.allscore);
                    }
                    var w = settings.barLength * level / 100;

                    if(settings.orientation == 'h'){
                        progressBar.animate({width: w}, {
                            duration: 2000,
                            step    : function (currentWidth){
                                var percent = parseInt(currentWidth / settings.barLength * 100);
                                if(isNaN(percent))
                                    percent = 0;
                                progressContainer.find('.bar-percent').html(percent + ' ');
                            }
                        });
                    }
                    else {

                        var h = settings.barLength - settings.barLength * level / 100;
                        progressBar.animate({top: h}, {
                            duration: 2000,
                            step    : function (currentValue){
                                var percent = parseInt((settings.barLength - parseInt(currentValue)) / settings.barLength * 100);
                                if(isNaN(percent))
                                    percent = 0;

                                /*类型判断*/
                                if(settings.type == 1){
                                    progressContainer.find('.bar-percent').html(valueold);
//                                    progressContainer.find('.bar-max-min b').html(valueold - settings.average);
                                    progressContainer.find('.bar-max-min b').html(settings.averageScoreDiff);
                                } else if(settings.type == 2){
//                                    progressContainer.find('.bar-percent').html(Math.abs(percent) + '%');
                                    progressContainer.find('.bar-percent').html(valueolds+ '%');
                                } else if(settings.type == 3){
                                    progressContainer.find('.bar-percent').html(valueold);
                                }

                            }
                        });
                        if(settings.type == 1){
                            progressContainer.find('.bar-percent').animate({top: (h - valueLabelHeight - 18)}, 2000);
                            progressContainer.find('.bar-max-min').animate({top: (h - valueLabelHeight - 25)}, 2000);
                        } else {
                            progressContainer.find('.bar-percent').animate({top: (h - valueLabelHeight - 18)}, 2000);
                        }

                        if(settings.type == 1 || settings.type == 2){
                            setTimeout(function (){
                                $('.average').css('display', 'block');
                                $('.average').animate({
                                    opacity: 1
                                }, 2000);
                            }, 2000);
                        }


                    }
                }

            });
        },
        average: function (options){
            var settings    = $.extend({
                allscore: 150,
                average : 150
            }, options);
            var bottomStyle = settings.average / settings.allscore;
            return this[0].style.bottom = bottomStyle * 250 + 79 + 'px';
        }
    });

})(jQuery, SINGER);