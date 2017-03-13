/**
 * Boz
 * @date    2016-05-27 12:10:41
 */

/**
 * 成绩通知
 * Created by shay on 2016/5/30.
 */
(function ($, S){
    var uri    = S.uri(),
        typeId = uri.type;             // AB卷答题卡类型	类型：1，A卷；2，B卷。
     $AnswerSheetAppend = $('.answer-sheet'),
    $HpTitle           = $('.d-base-header').find('.hp-h1');

//答题卡
    if(typeId ==0 || typeId == 1 || typeId == 2){
        var _bindIcon,
            _binfor,
            htmlIcon           = '';
        /**
         *  成绩统计
         * @param notice-analysis
         */
        $.dAjax({
            method   : 'work_answerSheet',
            batch    : uri.batch,
            paperId  : uri.paperId,
            isEncrypt: false
        }, function (json){
            _bindIcon(json.data);
        });
        /*绑定答题卡*/
        _bindIcon = function (data){
            /*绑定答题卡*/
            $HpTitle.html('试卷答题卡');
            var AnswerImgFun = function (data, typeId){
                for (var i = 0; i < data.length; i++) {
                    if(data[i].type == typeId){
                        htmlIcon += '<img class="answer-card-img" src="' + data[i].picture + '" width="100%" alt="试卷答题卡' + '卷答题卡"/>';
                        /*绑定答题卡图标*/
                        if(data[i].objectError){
                            htmlIcon += '<div class="p-objective-error" style="">客观错题：' + data[i].objectError + '</div>'
                        }
                        if(!(data[i].score == '')){
                            htmlIcon += '<div class="total-score">得分：' + data[i].score + '</div>'
                        }
                        // 答题图标
                        var markingsdata = data[i].markings;
                        var marksdata    = data[i].marks;
                        if(markingsdata){
                            var datasets = $.parseJSON(markingsdata);
                            for (var j = 0; j < datasets.length; j++) {
                                _binfor(datasets[j]);
                            }
                        }
                        if(marksdata){
                            var dataset = $.parseJSON(marksdata);
                            for (var k = 0; k < dataset.length; k++) {
                                _binfor(dataset[k]);
                            }
                        }
                    }
                }
                $AnswerSheetAppend.append(htmlIcon)
            }
            typeId == 0 && AnswerImgFun(data, 0);
            typeId == 1 && AnswerImgFun(data, typeId);
            typeId == 2 && AnswerImgFun(data, typeId);
        }
        //  markings; marks;
        _binfor = function (icondata){
            var t = icondata.t;
            // todo 发布图标链接地址更换
            var baseUrl = 'http://static.dayeasy.net/v1/image/icon/marking/';
            var marks   = ['full.png', 'semi.png', 'error.png'];
            /*.t
             * 0 正确
             * 1 半对
             * 2 错误
             * 3 给定文字
             * 4 自定义文字
             * 5 符号
             * 6
             * */
            htmlIcon += '<div class="icon" style="left:' + icondata.x + 'px;top:' + icondata.y + 'px;">';

            if(t <= 2){
                htmlIcon += '<img src="' + baseUrl + marks[t] + '" alt=""/>'
                if(0 < t){
                    htmlIcon += '<div class="score">' + '-' + icondata.w + '</div>';
                }
            }
            if(t == 3){
                htmlIcon += '<img src="' + baseUrl + icondata.w + '" alt=""/>'
            }
            if(t == 4){
                htmlIcon += '<div class="fontsheet"  class="icon">' + icondata.w + '</div>'
            }
            if(t == 5){
                htmlIcon += '<img src="' + baseUrl + 'brow/' + icondata.w + '" alt=""/>'
            }
            htmlIcon += '</div>';
        };
    } else if(typeId == 100){
        $HpTitle.html('我的答案');
        $.dAjax({
            method    : 'work_studentAnswer',   // 考试作业接口 学生答案详情
            batch     : uri.batch,
            paperId   : uri.paperId,
            questionId: uri.questionId
        }, function (json){
            var Answer             = json.data.answer;
            var $AnswerSheetAppend = $('.answer-sheet');
            $AnswerSheetAppend.append('<div class="id-img"><img src="' + Answer + '" alt="答题"/></div>');
        });
    }else {
        $AnswerSheetAppend.append('<div class="dy-nothing">没有相关数据</div>')
    };

//    else if(typeId ==110){
//        $HpTitle.html('参考答案');
//        $AnswerSheetAppend.append('<div class="id-img"><img width="100%" src="' + uri.img + '" alt="答题"/></div>');
//    }


})(jQuery, SINGER);


