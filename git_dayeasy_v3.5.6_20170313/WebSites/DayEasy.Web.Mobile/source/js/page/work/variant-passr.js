/**
 * Boz
 * @date    2016-06-12 14:18:03
 */

(function ($, S) {
    var uri = S.uri();
    S.progress.start();
    var data = 0, bindData, getQuestion, bindVariant, htmlVariantQuDic;
    var vidsIndex = 1;
    var $wrap = $('.dy-questions-cont');

    $.fn.noThing = function (text) {
        $(this).append('<div class="dy-nothing">' + text + '</div>');
        return this;
    };
    //* 变式列表
    htmlVariantQuDic = function (question, questionOriginal) {
        // 判断答题卡图片显示跳转
//        var BooleanImg = "<img";
//        function isImg(str, BooleanImg){
//            return str.indexOf(BooleanImg) >= 0;
//        }

        var questionHtml = '';
        questionHtml += '<div class="dy-questions-con-list">';
        question.sort = vidsIndex++;
        // 题干 conte
        questionHtml += S.showQuestion(question);
//        questionHtml += '</div>';
        /*Tab*/
        questionHtml += '<div class="dy-questions-btn">';
        questionHtml += '<ul class="f-cb tab-menu">';
        questionHtml += '<li>';
        questionHtml += '<a href="javascript:void(0);">查看答案<i class="angle"></i></a>';
        questionHtml += '</li>';
        questionHtml += '<li><a href="javascript:void(0);">对应原题<i class="angle"></i></a></li>';
        questionHtml += '</ul>';
        questionHtml += '<div class="tab-conten">';
        questionHtml += '<div class="tab-base original-title f-cb">';

        /*答案*/
        questionHtml += '<em class="q-title">答案：</em>';
        if (question.answers.length || question.details.length) {
//            console.log(question.answers);
            var Answersdata = question.answers;
            var Detailsdata = question.details;  //小问题 答案
            // 没小问答案
            if(Answersdata.length){
                questionHtml += '<div class="am-fl g-answer-body">';
                console.log(Answersdata);
                if(Answersdata.length==1){
                    for (var l = 0; l < Answersdata.length; l++) {
                        if(Answersdata[l].isCorrect == true){
//                            console.log(Answersdata[l].body);

//                            if(isImg(Answersdata[l].body, BooleanImg)){
//                                questionHtml += '<div class="js-answers-wrap">';
//                                questionHtml += '<div class="hide js-answersimg">' + Answersdata[l].body + '</div>';
//                                questionHtml += '<div><a class="a1 js-see-answer" href="javascript:void(0);">查看答案</a></div>';
//                                questionHtml += '</div>';
//                            } else {
                                questionHtml += Answersdata[l].body;
                                if(true){
                                    questionHtml += Answersdata[l].body;
                                }
//                            }
                        }
                    }

                }else{
                    for (var l = 0; l < Answersdata.length; l++) {
                        if(Answersdata[l].isCorrect == true){
                            questionHtml += Answersdata[l].tag;
                        }
                    }
                }
                questionHtml += '</div>';
            }
            // 有小问答案
            if (Detailsdata.length) {
                var serial = 1;
                questionHtml += '<div class="am-fl g-answer-body">';
                for (var m = 0; m < Detailsdata.length; m++) {
                    for (var n = 0; n < Detailsdata[m].answers.length; n++) {
                        //                                questionHtml += '（' + Detailsdata[m].answers[n].sort + 1 + '）' + '&nbsp' + Detailsdata[m].answers[n].tag;
                        if (Detailsdata[m].answers[n].isCorrect) {
                            //                                    questionHtml += '&nbsp' + Detailsdata[m].answers[n].tag;
                            var serials = serial++;
                            questionHtml += '（' + serials + '）' + '&nbsp' + Detailsdata[m].answers[n].tag + '</br>';
                        }
                    }
                }
                questionHtml += '</div>';
            }

        } else {
            questionHtml += '<div class="am-fl g-answer-body">暂时没有答案</div>';
        }

        /*答案 End*/
        questionHtml += '</div>';

        questionHtml += '<div class="tab-base see-answer f-cb">';

        /*原题题干*/
        questionHtml += S.showQuestion(questionOriginal);
        questionHtml += '</div>';
        questionHtml += '</div>';
        questionHtml += '</div>';
        questionHtml += '</div>';
        $wrap.append(questionHtml);
        $wrap.find('img').load(function(){
            $(this).attr('data-action','zoom');
        });
    };
    //* 问题列表
    getQuestion = function (id) {
        if (!data || !data.questions || !data.questions.length) return 0;
        for (var i = 0; i < data.questions.length; i++) {
            if (data.questions[i].id == id) {
                return data.questions[i];
            }
        }
        return 0;
    };
    //* 获取变式题ID
    bindVariant = function (dict) {
        for (var key in dict) {
            var questionOriginal = getQuestion(key);
            if (!questionOriginal) {
                S.msg('没有查询到问题');
            }
            var vids = dict[key];
            for (var i = 0; i < vids.length; i++) {
                var question = getQuestion(vids[i]);
                htmlVariantQuDic(question, questionOriginal);
            }
        }
    };
    //* 数据绑定
    bindData = function (data) {

        if(!data || $.isEmptyObject(data.variantQuDic) && $.isEmptyObject(data.deyiVariantQuDic)){
            $wrap.noThing('没有相关变式');
            return;
        }
        if (data.variantQuDic) {
            if (data.teacherName) {
                $wrap.append('<h2 class="variant-title-user">' + data.teacherName + '<span>老师为你推荐了变式训练</span></h2>');
            }
            bindVariant(data.variantQuDic);
        }
        if (data.deyiVariantQuDic && S.keys(data.deyiVariantQuDic).length > 0) {
            $wrap.append('<h2 class="variant-title-user">得一推荐变式</h2>');
            bindVariant(data.deyiVariantQuDic);
        }
    };
    //* 学生考试后的变式过关
    $.dAjax({
        method: 'work_variantPass',
        batch: uri.batch
    }, function (json) {
        if (json.status) {
            data = json.data;
            bindData(data);
            S.loadFormula();
            S.progress.done();

        } else {
            $wrap.noThing('没有相关变式');
        }
    }, false);

    /*参考答案跳转*/
//    $('body').on('click', '.js-see-answer', function (event){
//        event.preventDefault();
//        var $this = $(this),
//            $wrap = $this.parents('.js-answers-wrap'),
//            $ImgSrc  = $wrap.find('.js-answersimg').find('img').attr("src");
//        location.href ='/page/work/answer-sheet.html?img=' +$ImgSrc + '&type=' + '110';
//    });

})(jQuery, SINGER);

