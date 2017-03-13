(function ($, S){
    var uri = S.uri();
    var paperType;
    S.progress.start();



    /**
     * 获取试卷详情-展示
     * @param Questions
     */
    var bindQuestions,
        $QuestionsBody = $('.dy-questions-cont'),
        QuestionsHtml  = '';

    var sysTagsfun,
        bindsysTagsfun;

    /**
     * 错因分析未提交列表
     * @id 错题ID
     * @ErrorAnalysis  DOM 绑定数据 对象
     */
    bindsysTagsfun = function (id, ErrorAnalysis){

        var ErrorAnalysisHtml = '';
        /*添加错因*/
        ErrorAnalysisHtml += '<div class="text-tags f-cb">';
        ErrorAnalysisHtml += '<div class="am-fl q-tiile">错因标签：</div>';
        ErrorAnalysisHtml += '<div class="q-tags am-fl">';
        ErrorAnalysisHtml += '<div class="am-fl q-tags-add">';
        ErrorAnalysisHtml += '</div>';
        ErrorAnalysisHtml += '<div class="tag-edit am-fl">';
        ErrorAnalysisHtml += '<div class="q-button">';
        ErrorAnalysisHtml += '<button class="btn btn-tag"><i class="fa fa-plus"></i></button><span class="help-inline">(最多5个,每个10字符以内)</span>';
        ErrorAnalysisHtml += '</div>';
        ErrorAnalysisHtml += '</div>';
        ErrorAnalysisHtml += '</div>';
        ErrorAnalysisHtml += '</div>';
        ErrorAnalysis.append(ErrorAnalysisHtml);
        /*推荐错因分析*/
        $.dAjax({
            method : 'work_recommendTags',
            errorId: id // 错题ID
        }, function (json){
            var sysTagshtml = '';
            sysTagshtml += '<div class="f-cb sys-tags">';
            sysTagshtml += '常见错因：</em>';
            for (var i = 0; i < json.data.length; i++) {
                sysTagshtml += '<span>' + json.data[i].name + '</span>';
            }
            sysTagshtml += '</div>';
            ErrorAnalysis.append(sysTagshtml);
            /*添加评语*/
            var ErrorAnalysisButtomHtml = '';
            ErrorAnalysisButtomHtml += '<div class="d-textarea" data-errorId="' + id + '">';
            ErrorAnalysisButtomHtml += '<textarea placeholder="添加评语" name="message" class="textarea mb5 mt10" cols="30" rows="4" maxlength="140"></textarea>';
            ErrorAnalysisButtomHtml += '<p class="f-tar f-db d-result">你还可以输入 <em>140</em>个字</p>';
            ErrorAnalysisButtomHtml += '</div>';
            ErrorAnalysisButtomHtml += '<div class="g-btn am-text-right">';
            ErrorAnalysisButtomHtml += '<button class="button-tages">提 交</button>';
            ErrorAnalysisButtomHtml += '</div>';
            ErrorAnalysis.append(ErrorAnalysisButtomHtml);
            /*添加评语End*/
        });


    };

    /**
     * 错因分析
     * @id 问题ID
     * @ErrorAnalysis  DOM 绑定数据 对象
     */
    sysTagsfun    = function (id, ErrorAnalysis){
        /*获取错因分析*/
        $.dAjax({
            method    : 'work_errorAnalysis',
            batch     : uri.batch,
            questionId: id
        }, function (json){
            var errorId = json.data.errorId;
            ErrorAnalysis.data('errorIds', errorId);
            var dataTag = json.data;
            // 错因分析列表存在判断
            if(dataTag.tagList.length == 0){
                bindsysTagsfun(errorId, ErrorAnalysis);
            } else {
                var sysTagshtml = '';
                sysTagshtml += '<div class="text-tags f-cb">';
                sysTagshtml += '<div class="am-fl q-tiile">错因标签：</div>';
                sysTagshtml += '<div class="q-tags am-fl">';
                sysTagshtml += '<div class="am-fl q-tags-add">';
                for (var i = 0; i < dataTag.tagList.length; i++) {
                    sysTagshtml += '<div class="d-tag">' + dataTag.tagList[i] + '</div>';
                }
                sysTagshtml += '</div>';
                sysTagshtml += '</div>';
                sysTagshtml += '</div>';
                sysTagshtml += '<div class="f-cb text-tags">';
                sysTagshtml += '<div class="f-cb q-tiile am-fl">错因分析：</div>';
                //错因评价未提交为空
                if(dataTag.content == null){
                    /*添加评语*/
                    sysTagshtml += '<div class="d-textarea textarea-content-verification" data-errorId="' + errorId + '">';
                    sysTagshtml += '<textarea placeholder="添加评语" name="message" class="textarea mb5 mt10" cols="30" rows="4" maxlength="140"></textarea>';
                    sysTagshtml += '<p class="f-tar f-db d-result">你还可以输入 <em>140</em>个字</p>';
                    sysTagshtml += '</div>';
                    sysTagshtml += '<div class="g-btn am-text-right">';
                    sysTagshtml += '<button class="button-tages">提 交</button>';
                    sysTagshtml += '</div>';
                    /*添加评语End*/
                } else {
                    sysTagshtml += '<div class="tag-content am-fl" >' + dataTag.content + '</div>';
                }
                sysTagshtml += '<div>';
                ErrorAnalysis.append(sysTagshtml);
            }
        });
    };
    /**
     * 获取试卷详情-题干及其答案绑定
     * @param Questions
     * data  试卷详情               data
     * studentDetails  学生答题详细  data
     */
    bindQuestions = function (data, studentDetails, WorkErrorStatisticsData){

        var initData = function (uid){
            /*
             *问题ID   studentDetails[i].questionId
             *问题ID   data.id
             */
            var findDetail = function (qid){
                for (var i = 0; i < studentDetails.length; i++) {
                    if(studentDetails[i].questionId == qid)
                        return studentDetails[i];
                }
                return null;
            };
            var TheNumber  = function (qid){
                if(WorkErrorStatisticsData.errors.hasOwnProperty(qid))
                    return WorkErrorStatisticsData.errors[qid].length;
                return 0;
            }    // 错题人数

            // 判断答题卡图片显示跳转
//            var BooleanImg = "<img";
//            function isImg(str, BooleanImg){
//                return str.indexOf(BooleanImg) >= 0;
//            }

            var titleName = 1;
            QuestionsHtml += '<div class="box-questions-list">';
            for (var i = 0; i < data.sections.length; i++) {
                var questions = data.sections[i].questions;
                QuestionsHtml += '<div class="box-questions-list">';
                QuestionsHtml += '<h3 class="section-title">';
                QuestionsHtml += convertToChinese(titleName++) + '、' + data.sections[i].description;
                QuestionsHtml += '</h3>';
                for (var j = 0; j < questions.length; j++) {
                    var question         = questions[j];
                    question.show_option = false;
                    /*题干*/
                    QuestionsHtml += S.showQuestion(question);
                    /*题干End*/
                    QuestionsHtml += '<div class="dy-questions-con-list" data-id=' + question.id + '>';
                    /*QuestionsHtml+=QuestionFunc(question);*/
                    /*查看答案 错因分析*/
                    QuestionsHtml += '<div class="dy-questions-btn">';
                    if(paperType == 2){
                        QuestionsHtml += '<div class="f-cb">';
                        if(WorkErrorStatisticsData.isPrint){
                            QuestionsHtml += '<div class="am-fl add-wrongpeople">' + TheNumber(question.id) + '人错</div>';
                        }
                        if(WorkErrorStatisticsData.isPrint){
                            QuestionsHtml += '<ul class="f-cb tab-menu tab-menu-box4 am-fl">';
                        } else {
                            QuestionsHtml += '<ul class="f-cb tab-menu am-fl" style="width: 100%;">';
                        }
                        QuestionsHtml += '<li class="see-answer-li"><a data-isPrint="' + WorkErrorStatisticsData.isPrint + '" href="javascript:void(0);">查看答案<i class="angle"></i></a></li>';
                        var detail = findDetail(question.id);
                        //扫描试卷错题人数
                        if(detail && !detail.isCorrect){
                            //错因分析
                            QuestionsHtml += '<li class="error-analysis-li">';
                            QuestionsHtml += '<a class="iserror-is"  href="javascript:void(0);">错因分析<i class="angle"></i></a>';
                            QuestionsHtml += '</li>';
                        } else {
                            if(!WorkErrorStatisticsData.isPrint){//推送试卷
                                var errorItem = WorkErrorStatisticsData.errors[question.id] || 0;
                                var _Number   = 0, isError = false;
                                if(errorItem && errorItem.length){
                                    _Number = errorItem.length;
                                    for (var kkk = 0; kkk < errorItem.length; kkk++) {
                                        if(errorItem[kkk] == uid){
                                            isError = true;
                                            break;
                                        }
                                    }
                                }
                                QuestionsHtml += '<li class="error-analysis-li">';
                                QuestionsHtml += isError
                                    ? '<a class="iserror-is" href="javascript:void(0);">错因分析<i class="angle"></i></a>'
                                    : '<a class="mark-error" href="javascript:void(0);"><span class="mark-errors">标记错题 <span class="the-number"><em class="am-text-truncate">&#40;' + _Number + '人&#41;</em></span></span></a>';
                                QuestionsHtml += '</li>';
                            }
                        }
                        QuestionsHtml += '</ul>';
                        if(WorkErrorStatisticsData.isPrint){
                            /*未提交试卷*/
                            var DetailScore;
                            detail ? (   DetailScore = detail.score) : (  DetailScore = 0);
                            if(DetailScore < question.score){
                                if(DetailScore == 0){
                                    QuestionsHtml += '<div class="am-fl add-fraction"><span>' + DetailScore + '</span>&nbsp;/&nbsp;' + question.score + '分</div>';
                                } else {
                                    QuestionsHtml += '<div class="am-fl add-fraction"><span style="color: red;">' + DetailScore + '</span>&nbsp;/&nbsp;' + question.score + '分</div>';
                                }
                            } else {
                                QuestionsHtml += '<div class="am-fl add-fraction">' + DetailScore + '&nbsp;/&nbsp;' + question.score + '分</div>';
                            }
                        }
                        QuestionsHtml += '</div>';
                    } else {
                        QuestionsHtml += '<ul class="f-cb tab-menu">';
                        QuestionsHtml += '<li class="see-answer-li"><a data-isPrint="' + WorkErrorStatisticsData.isPrint + '" href="javascript:void(0);">查看答案<i class="angle"></i></a></li>';
                        var detail = findDetail(question.id);
                        //扫描试卷错题人数
                        // paperType  试卷类型
                        if(detail && !detail.isCorrect){
                            //错因分析
                            QuestionsHtml += '<li class="error-analysis-li">';
                            QuestionsHtml += '<a class="iserror-is"  href="javascript:void(0);">错因分析<i class="angle"></i></a>';
                            QuestionsHtml += '</li>';
                        } else {
                            if(!WorkErrorStatisticsData.isPrint){//推送试卷
                                var errorItem = WorkErrorStatisticsData.errors[question.id] || 0;
                                var _Number   = 0, isError = false;
                                if(errorItem && errorItem.length){
                                    _Number = errorItem.length;
                                    for (var kkk = 0; kkk < errorItem.length; kkk++) {
                                        if(errorItem[kkk] == uid){
                                            isError = true;
                                            break;
                                        }
                                    }
                                }
                                QuestionsHtml += '<li class="error-analysis-li">';
                                QuestionsHtml += isError
                                    ? '<a class="iserror-is" href="javascript:void(0);">错因分析<i class="angle"></i></a>'
                                    : '<a class="mark-error" href="javascript:void(0);"><span class="mark-errors">标记错题 <span class="the-number"><em class="am-text-truncate">&#40;' + _Number + '人&#41;</em></span></span></a>';
                                QuestionsHtml += '</li>';
                            }
                        }
                        QuestionsHtml += '</ul>';
                    }
                    QuestionsHtml += '<div class="tab-conten">';

                    QuestionsHtml += '<div class="tab-base see-answer">';
                    /*我的答案*/ // 推送试卷没有我的答案
//                    if(WorkErrorStatisticsData.isPrint){
                    QuestionsHtml += '<div class="answer-box-1 f-cb">';
                    QuestionsHtml += '<div class="am-fl g-answer-title">我的答案：</div>';
                    QuestionsHtml += '<div class="am-fl g-answer-body my-answer"></div>';
                    QuestionsHtml += '</div>';
//                    }
                    /*我的答案*/
                    /*参考答案*/
                    QuestionsHtml += '<div class="answer-box-2 f-cb">';
                    QuestionsHtml += '<div class="am-fl g-answer-title">参考答案：</div>';
                    if(question.answers.length || question.details.length){
                        var Answersdata = question.answers;
                        var Detailsdata = question.details;  //小问题 答案

                        // 没小问答案
                        if(Answersdata.length){
                            QuestionsHtml += '<div class="am-fl g-answer-body">';
                            if(Answersdata.length == 1){
                                for (var l = 0; l < Answersdata.length; l++) {
                                    if(Answersdata[l].isCorrect == true){
//                                        if(isImg(Answersdata[l].body, BooleanImg)){
//                                            QuestionsHtml += '<div class="js-answers-wrap">';
//                                            QuestionsHtml += '<div class="hide js-answersimg">' + Answersdata[l].body + '</div>';
//                                            QuestionsHtml += '<div><a class="a1 js-see-answer" href="javascript:void(0);">查看答案</a></div>';
//                                            QuestionsHtml += '</div>';
//                                        } else {
                                            QuestionsHtml += Answersdata[l].body;
//                                        }
                                    }
                                }
                            } else {
                                for (var l = 0; l < Answersdata.length; l++) {
                                    if(Answersdata[l].isCorrect == true){
                                        QuestionsHtml += Answersdata[l].tag;
                                    }
                                }
                            }
                            QuestionsHtml += '</div>';
                        }
                        // 有小问答案
                        if(Detailsdata.length){
                            var serial = 1;
                            QuestionsHtml += '<div class="am-fl g-answer-body">';
                            for (var m = 0; m < Detailsdata.length; m++) {
                                for (var n = 0; n < Detailsdata[m].answers.length; n++) {
                                    if(Detailsdata[m].answers[n].isCorrect){
                                        var serials = serial++;
                                        QuestionsHtml += '（' + serials + '）' + '&nbsp' + Detailsdata[m].answers[n].tag + '</br>';
                                    }
                                }
                            }
                            QuestionsHtml += '</div>';
                        }

                    } else {
                        QuestionsHtml += '<div class="am-fl g-answer-body">暂时没有答案</div>';
                    }
                    QuestionsHtml += '</div>';
                    /*参考答案-End*/
                    QuestionsHtml += '</div>';
                    QuestionsHtml += '<div class="tab-base error-analysis">';
                    /*---- 错因分析-----*/
                    QuestionsHtml += '</div>';
                    QuestionsHtml += '</div>';
                    QuestionsHtml += '</div>';
                    QuestionsHtml += '</div>';
                }
                QuestionsHtml += '</div>';
            }

            QuestionsHtml += '</div>';
            $QuestionsBody.append(QuestionsHtml);
            S.loadFormula();
            S.progress.done();
            $QuestionsBody.find('img').load(function(){
                $(this).attr('data-action','zoom');
            });

        };
        S.setUser(function (user){
            if(user.role == 1){
                $.dAjax({
                    method: 'user_children'  // 用户接口  家长绑定的学生列表
                }, function (json){
                    if(!json.status || json.data.length == 0)
                        return false;
                    initData(json.data[0].id);
                }, true);
            } else {
                initData(user.id);
            }
        });


    };
    var StudentDetailsData, LoadPaperInfo, LoadErrorAnalysis;
    //绑定页面数据
    LoadPaperInfo     = function (studentDetails){
        /*试卷错题统计
         *data.isPrint 是否为扫描批阅试卷
         */
        $.dAjax({
            method : 'paper_info',
            keyword: uri.paperId
        }, function (json){
            paperType = json.data.paperType;
            LoadErrorAnalysis(json.data, studentDetails);
        });
    };
    LoadErrorAnalysis = function (QuestionsData, studentDetails){
        $.dAjax({
            method: 'work_errorStatistics',
            batch : uri.batch
        }, function (json){
            var WorkErrorStatisticsData = json.data;
            bindQuestions(QuestionsData, studentDetails, WorkErrorStatisticsData);
        });
    }
    /**
     * 考试作业接口-学生答题详细
     * 判断题目是否正确
     * @param  uri.batch
     * @param  questionId
     */
    $.dAjax({
        method : 'work_studentDetails', /*work_paperErrors*  试卷错题列表/  /*work_studentDetails*/
        batch  : uri.batch,
        paperId: uri.paperId
    }, function (json){
        LoadPaperInfo(json.data);
    }, false);
    // 标记错题
    $('body').on('click', '.mark-error', function (event){
        event.preventDefault();
        var $this          = $(this);
        var $questions     = $this.parents('.dy-questions-con-list');
        var $ErrorBox      = $this.parents('.error-analysis-li');
        var $ErrorAnalysis = $questions.find('.error-analysis');
        var id             = $questions.data('id');
        var Markerror      = function (){
            $.dAjax({
                method    : 'work_setError',
                batch     : uri.batch,
                paperId   : uri.paperId,
                questionId: id
            }, function (json){
                var errorId = json.data;  // 返回错题ID
                var setErrorHtml = '<a href="javascript:void(0);" data-errorId-ling="' + errorId + '">错因分析<i class="angle"></i></a>';
                $ErrorBox.empty().append(setErrorHtml);
                $ErrorAnalysis.data('errorIds', errorId);
                bindsysTagsfun(errorId, $ErrorAnalysis);
            }, true);
        }
        S.confirm('标记错题', function (){
            Markerror();
        })

    });
    /**
     * 错因分析列表
     * @param  uri.batch   批次号
     * @param  questionId  问题ID
     */
    $('body').on('click', '.error-analysis-li .iserror-is', function (){

        var $this          = $(this),
            $questions     = $this.parents('.dy-questions-con-list'),
            $ErrorAnalysis = $questions.find('.error-analysis');
        var id             = $questions.data('id');
        // 阻止多次请求
        if($this.is('.tabsload')){
            return false;
        }
        $(this).addClass('tabsload');
        sysTagsfun(id, $ErrorAnalysis);

    });
    /**
     * 提交错因分析   work_setAnalysis
     * @errorId  错题ID
     * @content    分析内容
     * @tags    错因标签,多个以,隔开
     */
    $('body').on('click', '.button-tages', function (event){
        event.preventDefault();
        var $this      = $(this);
        var $parents   = $this.parents('.error-analysis');
        var $dtag      = $parents.find('.q-tags-add .d-tag');
        var $dTextarea = $parents.find('.d-textarea');
        var _errorId   = $dTextarea.data('errorid');
        var _content   = $parents.find('.textarea').val();
        var _dtag      = '';

        if($dtag.length == 0){
            S.msg('请选择错因标签');
            return false;
        }
        if($dTextarea.hasClass('textarea-content-verification')){
            if(_content == ''){
                S.msg('请添加错因评语');
                return false;
            }
        }

        $dtag.each(function (){
            _dtag += $(this).text() + ',';
        });
        _dtag          = _dtag.substring(0, _dtag.length - 1);
        var $questions = $this.parents('.dy-questions-con-list');
        var id         = $questions.data('id');
        $.dAjax({
            method : 'work_setAnalysis',
            errorId: _errorId,
            content: _content,//encodeURIComponent(_content),
            tags   : _dtag //encodeURIComponent(_dtag)
        }, function (json){
            if(json.status){
                $parents.empty();
                S.msg('评价成功');
                sysTagsfun(id, $parents);  //id  错题ID
            } else {
                S.msg('评价失败');
            }
        }, true);
    });
    // 我的答案获取
    $('body').on('click', '.see-answer-li a', function (event){
        event.preventDefault();
        var $this   = $(this);
        var isprint = $this.data('isprint'); // 是否为扫描卷
        var $warplist  = $this.parents('.dy-questions-con-list');
        var questionId = $warplist.data('id');
        var $answerCon = $warplist.find('.my-answer');
        if($this.hasClass('one')){
            return false
        }
        $this.addClass('one');
        $.dAjax({
            method    : 'work_studentAnswer',   // 考试作业接口 学生答题信息
            batch     : uri.batch,
            paperId   : uri.paperId,
            questionId: questionId
        }, function (json){
            var Answer = json.data.answer;
            if(isprint){
                /*扫描试卷*/
                if(Answer !== null){ // 判断试卷是否提交
                    var substr = "data:image";

                    function isContains(str, substr){
                        return str.indexOf(substr) >= 0;
                    }

                    /*答案中是否有data-64编码图片*/
                    if(isContains(Answer, substr)){
                        $answerCon.append('<div class=""><a class="a1 pop-answer" href="/page/work/answer-sheet.html?batch=' + uri.batch + '&paperId=' + uri.paperId + '&questionId=' + questionId + '&type=' + 100 + '">查看答案</a></div>');
                        return false;
                    } else {
                        $answerCon.append(Answer);
                    }
                } else {
                    $answerCon.append('未提交');
                }
            } else {
                /*推送试卷*/
                $answerCon.append('未使用网阅，暂无详情');
            }
        });

    });
    /*参考答案跳转*/
//    $('body').on('click', '.js-see-answer', function (event){
//        event.preventDefault();
//        var $this = $(this),
//            $wrap = $this.parents('.js-answers-wrap'),
//            $ImgSrc  = $wrap.find('.js-answersimg').find('img').attr("src");
//            location.href ='/page/work/answer-sheet.html?img=' +$ImgSrc + '&type=' + '110';
//    });
    //图片弹框查看
//
//    $('body').on('click', 'img', function(event) {
//    	event.preventDefault();
//        var src = $(this).find('img').attr('src') || $(this).prop("src");
//        src = src.replace(/_s(\d+)x[^\.]*/gi, '');
//        S.showImage(src);
//        return false;
//    });



})(jQuery, SINGER);
$(function (){
    /**
     * 错因标签-添加-用户操作JS
     * @param
     * @param
     */
    (function ($){
        //  The  plugin .
        $.fn.TextTags          = function (options){
            if(!this.length){
                return this;
            }
            var opts = $.extend(true, {}, $.fn.TextTags.defaults, options);

            $('body').on('click', '.error-analysis .tag-edit .btn-tag', function (event){
                event.preventDefault();
                var _this     = $(this),
                    $TagEdit  = _this.parents('.tag-edit');
                var InputHtml = '<input class="form-control" maxlength="10" type="text"/>';
                $TagEdit.html(InputHtml);
                var $Input    = $TagEdit.find('.form-control');
                $Input.focus();
            });
            $('body').on('change', '.error-analysis .tag-edit .form-control', function (event){
                event.preventDefault();
                var $this      = $(this),
                    InputVal   = $this.val(),
                    $Tags      = $this.parents('.text-tags'),
                    $qtagsadd  = $Tags.find('.q-tags-add'),
                    $TagsList  = $Tags.find('.d-tag'),
                    $TagEdit   = $this.parents('.tag-edit');
                var TagHtml    = '<div class="d-tag fa fa-times tag-remove">' + InputVal + '</div>';
                var ButtonHtml = '<div class="q-button"> <button class="btn btn-tag"><i class="fa fa-plus"></i></button><span class="help-inline">(最多5个,每个10字符以内)</span></div>'
                $qtagsadd.append(TagHtml);
                $TagEdit.html(ButtonHtml);
                $TagsList.length >= 4 && $Tags.addClass('on');
            });
            $('body').on('click', '.error-analysis .q-tags-add .tag-remove', function (event){
                event.preventDefault();
                var $this = $(this),
                    $Tags = $this.parents('.text-tags');
                $this.remove();
                $Tags.removeClass('on');
            });
            $('body').on('click', '.error-analysis .sys-tags span', function (event){
                event.preventDefault();
                var $this      = $(this);
                var tagval     = $this.text();
                var $Tags      = $this.parents('.error-analysis');
                var $qtagsadd  = $(this).parents('.error-analysis').find('.q-tags-add');
                var $TagRemove = $qtagsadd.find('.tag-remove');
                var off        = true;
                var $TagsList  = $Tags.find('.d-tag');
                $TagsList.length >= 4 && $Tags.find('.text-tags').addClass('on');
                if($TagRemove.length < 5){
                    if($TagRemove.length > 0){
                        var TagRemoveArrText = [];
                        for (var i = 0; i < $TagRemove.length; i++) {
                            TagRemoveArrText.push($TagRemove.eq(i).text());
                        }
                        $.inArray(tagval, TagRemoveArrText) >= 0 ? off = false : off = true;
                    } else {
                        off = true;
                    }
                } else {
                    off = false;
                }
                if(off){
                    var TagHtml = '<div class="d-tag fa fa-times tag-remove">' + tagval + '</div>';
                    $qtagsadd.append(TagHtml);
                }
            });
            this.each(function (){
                var $this = $(this);
            });
            return this;
        };
        $.fn.TextTags.defaults = {
            defaultOne  : true,
            defaultTwo  : false,
            defaultThree: 'yay!'
        };
    })(jQuery);
    $('.error-analysis').TextTags();


});
