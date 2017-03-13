(function ($) {
    $.fn.PaperMaker = function (options) {

        var pMaker = new paperMaker(this, options);
        pMaker.create();

        //异步请求样式
        $('body').append('<div id="ajaxModel" style="z-index: 10000;position: absolute;height: 100%;width: 100%;left: 0;top: 0;" class="hide"><div class="text-center"><span id="dyloading" style="position:fixed; z-index: 1000;top: 50%;left:50%;"><i class="fa fa-spin fa-spinner fa-3x" ></i><br /><br />正在加载，请稍后...</span></div></div>');

        $.ajaxSetup({
            //发送请求前触发
            beforeSend: function (xhr) {
                $("#ajaxModel").removeClass('hide');
            },
            complete: function (xhr, status) {
                $("#ajaxModel").addClass('hide');
            }
        });

        return $(this);
    };

    //试卷对象封装
    var $paperDetailDiv = $("#paperDetailDiv"),
        $paperQSection = $(".paper-qSection"),
        $btnSaveTemp = $("#btn_SaveTemp"),
        $btnSave = $("#btn_Save"),
        $addQuestion = $(".f-addQuestion"),
        $paperMaker;

    var paperMaker = function (obj, options) {
        this.$element = $(obj);
        this.defaults = {
            addQuestionUrl: '',//添加问题url
            chooseQuestionUrl: '',//添加题目url
            chooseQuCompleteUrl: '',//添加题目完成跳转url
            inputAnswerUrl: ''//录入答案url
        };

        this.options = $.extend({}, this.defaults, options);
    };

    paperMaker.prototype = {
        create: function () {
            $paperMaker = this;

            //更新总分
            this.UpdateQScoreAndNum();

            //更新题目序号
            this.questionContentDragEnd();

            //编辑部分的处理事件
            questionContentDrag($paperQSection);

            //分数委托
            $(document).delegate(".q-perscore,.qScore", "keyup", function () {
                var $t = $(this);
                if (/(\d{1,2}((\.\d)|\.)?)/.test($t.val())) {
                    $t.val(RegExp.$1);
                } else {
                    $t.val("");
                }
            }).delegate(".q-perscore,.qScore", "change", function () {
                var $t = $(this);
                if (/(\d{1,2}(\.\d)?)/.test($t.val())) {
                    $t.val(RegExp.$1);
                } else {
                    $t.val("");
                }
            });

            //试卷标题宽度自适应
            $('#paperTitle').on("keyup", function () {
                var size = $(this).val().length;
                $(this).attr("size", size);
            });

            //题型板块拖动排序事件
            $paperQSection.dragsort({
                dragSelectorExclude: "input,textarea,i",
                dragEnd: this.questionSectionDragEnd
            });
            //删除题目板块
            $paperDetailDiv.delegate("i.s-del", "click", this.sectionDel);
            //删除按钮的委托事件
            $paperDetailDiv.delegate("i.paper-del", "click", this.questionDel);
            //每题好多分输入委托事件
            $paperDetailDiv.delegate("input.q-perscore", "blur", this.UpdatePerScore);
            //题目分数输入委托事件
            $paperDetailDiv.delegate("input.qScore", "blur", this.UpdateQScoreAndNum);
            //存草稿按钮点击事情
            $btnSaveTemp.bind("click", this.paperSaveTemp);
            //生成试卷按钮点击事件
            $btnSave.bind("click", this.makePaper);
            //添加题目
            $addQuestion.bind("click", this.addQuestion);
            //确认答案并保存试卷
            $("#inputAnswerDiv").delegate("#btnSureAnswer", "click", this.paperSave);
            //添加题目按钮点击
            $(".f-createQu").bind("click", this.showAddQuestion);
        },
        questionSectionDragEnd: function () {
            $paperMaker.updateSectionNo($(this).parent('ul'));//更新序号
            $paperMaker.questionContentDragEnd();//更新问题序号
        },
        questionContentDragEnd: function () {
            var qContentLis = $("ul.paper-qContent").children('li');
            $.each(qContentLis, function (index, item) {
                $(item).find("div.sortNum").text((index + 1) + ".");
            });
        },
        sectionDel: function () {
            var _this = $(this);
            $.Dayez.confirm("您确定要删除该题型及其下面的题目？", function () {
                var section = _this.parent("h3").parent("li");
                var qsections = section.parent('ul.paper-qSection');
                var sectionId = section.attr("id");

                var checkId = $.trim(sectionId).split('_')[1];

                if (sectionId.indexOf("A") > 0) {
                    $("#qTypesA,#qTypes").find("ul li.sel[data-value='" + checkId + "']").removeClass("sel");
                } else {
                    $("#qTypesB").find("ul li.sel[data-value='" + checkId + "']").removeClass("sel");
                }
                section.remove();

                $paperMaker.updateSectionNo(qsections);
                $paperMaker.questionContentDragEnd();//更新问题序号
            }, function () {
            });
        },
        questionDel: function () {
            var _this = $(this);
            var d = $.Dayez.confirm("您确定要删除该题目？", function () {
                var contentLi = _this.parents('li.m-lst-1');

                var ul = contentLi.parent('ul');
                if (ul.children('li').length == 1) {
                    var qsections = ul.parent('li').parent('ul.paper-qSection');
                    ul.parent('li').remove();
                    $paperMaker.updateSectionNo(qsections);
                } else {
                    contentLi.remove();
                }

                $paperMaker.UpdateQScoreAndNum();//更新总分
                $paperMaker.questionContentDragEnd();//更新序号

                d[0].close().remove();
            }, function () {
                d[0].close().remove();
            });
        },
        updateSectionNo: function (parentObj) {
            //更新序号
            var qSectionLis = parentObj.children("li");
            $.each(qSectionLis, function (index, item) {
                $(item).find("h3 span:first").text(convertToChinese(index + 1) + ".");
            });

            $paperMaker.UpdateQScoreAndNum();//更新总分
        },
        UpdatePerScore: function () {
            var score = parseFloat($(this).val());
            if (isNaN(score)) {
                return false;
            }
            $(this).parents('li').find("input.qScore").val(score);

            $paperMaker.UpdateQScoreAndNum();//更新总分
        },
        UpdateQScoreAndNum: function () {
            var sectionUl = $paperDetailDiv.find("ul.paper-qSection");

            var totalScore = 0,
                tScoreAObj = $("#tScoreA"),
                tScoreBObj = $("#tScoreB"),
                tScoreObj = $("#tScore");
            $.each(sectionUl, function (ulIndex, ulItem) {
                var lis = $(ulItem).children("li");
                var sectionScore = 0;

                $.each(lis, function (index, item) {
                    var qContents = $(item).find("ul.paper-qContent li");
                    $(item).find("span.qCNum").text(qContents.length);//更新板块题目数量

                    var secScore = 0;//更新板块分数
                    var scoreObjs = $(item).find("input.qScore");
                    $.each(scoreObjs, function (sIndex, sItem) {
                        var score = parseFloat($(sItem).val());
                        if (!isNaN(score)) {
                            secScore = $.Operation.Add(secScore, score);
                        }
                    });
                    $(item).find("span.qCScore").text(secScore);//更新板块分数

                    sectionScore = $.Operation.Add(sectionScore, secScore);//A，B卷总分
                });

                if (sectionUl.length > 1 && ulIndex == 0) {
                    tScoreAObj.text(sectionScore);//更新A卷总分
                }
                else if (sectionUl.length > 1 && ulIndex == 1) {
                    tScoreBObj.text(sectionScore);//更新B卷总分
                }
                totalScore = $.Operation.Add(totalScore, sectionScore);
            });

            tScoreObj.text(totalScore);
        },
        getPaperData: function (onlyObjective) {
            var paperData = {};
            paperData.PaperTitle = $("#paperTitle").val();
            paperData.PaperType = "A";//默认为空白卷
            paperData.PScores = {};
            paperData.PScores.TScore = parseFloat($("#tScore").text());
            paperData.PScores.TScoreA = 0;
            paperData.PScores.TScoreB = 0;

            if ($("#tScoreA").length > 0) {//AB卷
                paperData.PaperType = "AB";

                paperData.PScores.TScoreA = parseFloat($("#tScoreA").text());
                paperData.PScores.TScoreB = parseFloat($("#tScoreB").text());
            }

            var objectiveQType = [1, 2, 3, 4, 18, 24];//客观题ID

            paperData.PSection = [];
            var paperSections = $paperDetailDiv.children('ul.paper-qSection');//AB卷Div
            $.each(paperSections, function (index, item) {
                var paperSectionType = "A";
                if (index > 0) {
                    paperSectionType = "B";
                }
                //题型板块
                var sectionLis = $(item).children("li");
                $.each(sectionLis, function (sectionIndex, sectionItem) {
                    var sectionQuType = parseInt($(sectionItem).children('ul.paper-qContent').data('qtype'));
                    if (onlyObjective && $.inArray(sectionQuType, objectiveQType) < 0) {
                        return true;
                    }
                    var pSection = {};
                    pSection.Sort = sectionIndex + 1;
                    pSection.Description = $(sectionItem).find("h3 input.u-ipt-1").val();
                    pSection.PaperSectionType = paperSectionType;
                    pSection.SectionQuType = sectionQuType;
                    pSection.SectionScore = 0;
                    pSection.Questions = [];

                    //问题列表
                    var questionLis = $(sectionItem).find("ul.paper-qContent").children("li");
                    $.each(questionLis, function (qIndex, qItem) {
                        var question = {};
                        question.Sort = qIndex + 1;
                        question.QuestionID = $(qItem).data("qid");
                        question.Score = 0;
                        question.SmallQuestionScore = [];

                        var scoreObj = $(qItem).find("input.qScore");
                        var smallquestions = $(qItem).find("div.smallquestion").children('div.q-detail');
                        if (scoreObj.length == 1 || smallquestions.length < 1) {//(不记小问分数 && → ||)
                            var score = parseFloat($(scoreObj).val());
                            if (isNaN(score)) {
                                score = 0;
                            }
                            question.Score = score;
                        } else {//记小问分数 
                            $.each(scoreObj, function (sqIndex, sqItem) {
                                var smallQuScore = {};
                                smallQuScore.QuestionID = $(sqItem).parent("span").data("qid");
                                var sqScore = parseFloat($(sqItem).val());
                                if (isNaN(sqScore)) {
                                    sqScore = 0;
                                }
                                smallQuScore.Score = sqScore;
                                question.SmallQuestionScore.push(smallQuScore);

                                question.Score = $.Operation.Add(question.Score, sqScore);//问题总分
                            });
                        }

                        pSection.Questions.push(question);
                    });

                    //更新题型板块总分
                    $.each(pSection.Questions, function (qScoreIndex, qScoreItem) {
                        pSection.SectionScore = $.Operation.Add(pSection.SectionScore, qScoreItem.Score);
                    });

                    paperData.PSection.push(pSection);
                });
            });

            return paperData;
        },
        paperSaveTemp: function () {
            if (!validateChooseQu()) {
                return false;
            }

            var addQuDiv = $(".fa-createQuDiv:visible");
            if (addQuDiv.length > 0) {
                $.Dayez.confirm("还有题目没有保存 , 您确定要放弃该题目吗？", function () {
                    $paperMaker.showSaveDiv(true);
                }, function () {
                });
            }
            else {
                $paperMaker.showSaveDiv(true);
            }
            return false;
        },
        makePaper: function () {
            if (!validateChooseQu()) {
                return false;
            }

            var addQuDiv = $(".fa-createQuDiv:visible");
            if (addQuDiv.length > 0) {
                $.Dayez.confirm("还有题目没有保存 , 您确定要放弃该题目吗？", function () {
                    if (!validateScore()) {
                        $.Dayez.confirm("还有题目没有录入分数 , 您确定要生成试卷吗？", function () {
                            $paperMaker.inputAnswer();
                        }, function () {
                        });
                    } else {
                        $paperMaker.inputAnswer();
                    }
                }, function () {
                });
            }
            else {
                if (!validateScore()) {
                    $.Dayez.confirm("还有题目没有录入分数 , 您确定要生成试卷吗？", function () {
                        $paperMaker.inputAnswer();
                    }, function () {
                    });
                } else {
                    $paperMaker.inputAnswer();
                }
            }

            return false;
        },
        paperSave: function () {
            if (!validateAnswer()) {
                return false;
            }

            $paperMaker.showSaveDiv(false);

            return false;
        },
        showSaveDiv: function (isTemp) {
            var saveUrl = "/Paper/SavePaperData";
            var paperData = $paperMaker.getPaperData();//获取试卷数据
            var savePaperStr = $("#savePaper").html().replaceAll("{name}", paperData.PaperTitle);

            var title = isTemp ? "保存草稿" : "生成试卷";
            if (isTemp) {
                var tipStr = $(savePaperStr).find("#saveTip").html();
                savePaperStr = savePaperStr.replace(tipStr, "");
            }

            var tags, $tags;
            var d = singer.dialog({
                title: title,
                content: $(savePaperStr),
                onshow: function () {
                    $tags = $(".d-tags");
                    var data = $tags.data("tags") || [];
                    if (singer.isString(data)) {
                        data = eval('(' + data + ')');
                    }
                    tags = singer.tags({
                        container: $tags,
                        data: data,
                        canEdit: true,
                        type: 2,
                        max: 5,
                        change: function (data) {
                            //console.log(data);
                        }
                    });
                },
                okValue: "确定",
                ok: function () {
                    paperData.Tags = tags.get();

                    var name = $("#paperName").val();
                    if (!name) {
                        $("#paperName").focus();
                        return false;
                    }
                    paperData.PaperTitle = name;

                    var grade = $("#grade").children("option:selected").val();
                    if (parseInt(grade) < 0) {
                        $("#grade").focus();
                        return false;
                    }
                    paperData.Grade = grade;

                    var jsonDataStr = JSON.stringify(paperData);
                    var paperId = $("#paperId").val();
                    var shareSchool = $("#shareSchool").is(':checked');
                    var answers = '';
                    if (!isTemp) {//获取试卷答案
                        answers = JSON.stringify($paperMaker.getPaperAnswer());
                    }

                    $('.ui-dialog-button').children('button').attr("disabled", "disabled");

                    $.post(saveUrl, {
                        isTemp: isTemp,
                        paperId: paperId,
                        paperData: jsonDataStr,
                        shareSchool: shareSchool,
                        answers: answers
                    }, function (res) {
                        if (res.Status) {
                            d.close().remove();
                            window.location = "/Paper/Index";
                        } else {
                            $('.ui-dialog-button').children('button').removeAttr("disabled");
                            singer.msg(res.Message);
                        }
                    });

                    return false;
                },
                cancelValue: "取消",
                cancel: function () {
                },
                backdropBackground: '#000',
                backdropOpacity: 0.3
            }).showModal();
        },
        /**
         * 录入新题
         */
        showAddQuestion: function () {
            var $t = $(this),
                $createDivs = $('.fa-createQuDiv'),
                $currentCreateDiv = $t.parent().next();
            if (!$currentCreateDiv.length || !$createDivs.length)
                return false;
            if ($t.data('show')) {
                $currentCreateDiv.hide();
                $t.data('show', false)
                    .removeClass('btn-danger')
                    .addClass('btn-warning')
                    .html('<i class="fa fa-plus"></i> 录入新题');
            } else {
                //取消其他的板块
                $createDivs.not($currentCreateDiv)
                    .data('loaded', false)
                    .hide()
                    .children('div')
                    .empty();
                $('.f-createQu').not($t)
                    .data('show', false)
                    .removeClass('btn-danger')
                    .addClass('btn-warning')
                    .html('<i class="fa fa-plus"></i> 录入新题');

                if (!$currentCreateDiv.data('loaded')) {
                    $t.attr('disabled', 'disabled').addClass('disabled');
                    $currentCreateDiv.show();
                    //加载编辑器
                    $.ajax({
                        url: $paperMaker.options.addQuestionUrl,
                        data: {},
                        type: 'POST',
                        beforeSend: function () {
                        },
                        success: function (res) {
                            $currentCreateDiv.children('div').html(res);
                            $currentCreateDiv.children('span').addClass('hide');
                            $t.removeAttr('disabled').removeClass('disabled');
                            $currentCreateDiv.data('loaded', true);
                            $t.data('show', true)
                                .removeClass('btn-warning')
                                .addClass('btn-danger')
                                .html('<i class="fa fa-times"></i> 取消录入');
                        }
                    });
                } else {
                    $currentCreateDiv.show();
                    $t.data('show', true)
                        .removeClass('btn-warning')
                        .addClass('btn-danger')
                        .html('<i class="fa fa-times"></i> 取消录入');
                }
            }
        },
        addQuestion: function () {
            var chooseQus = [];
            //当前选择的问题
            var questionContent = $("ul.paper-qContent");
            for (var i = 0; i < questionContent.length; i++) {
                var qtype = $(questionContent[i]).data('qtype');
                var section = $(questionContent[i]).data('section');

                var qlist = [];

                if (chooseQus[qtype]) {
                    qlist = chooseQus[qtype];
                }

                var lis = $(questionContent[i]).children("li");
                $.each(lis, function (index, item) {
                    var qitem = {};

                    qitem.QId = $(item).data('qid');
                    qitem.Type = qtype;
                    var score = parseFloat($(item).find("input.qScore").val());
                    if (isNaN(score)) {
                        score = 0;
                    }
                    qitem.Score = score;
                    qitem.Sort = index;
                    qitem.PaperType = section;

                    qlist.push(qitem);
                });

                chooseQus[qtype] = qlist;
            }

            //每题分数
            var perScores = [];
            var sectionlis = $(".paper-qSection").children('li');
            if (sectionlis) {
                $.each(sectionlis, function (index, item) {
                    var qContent = $(item).children('ul.paper-qContent');

                    if (qContent) {
                        var score = $(item).find('.q-perscore').val();
                        if (!score || isNaN(score)) {
                            score = 0;
                        }
                        var obj = {};
                        obj.QSectionType = $(qContent).data('qtype');
                        obj.PaperType = $(qContent).data('section');
                        obj.PerScore = score;
                        obj.Sort = index;

                        perScores.push(obj);
                    }
                });
            }

            //试卷基础信息
            var paperBase = {};
            paperBase.Type = $("#type").val();
            paperBase.AddType = $(this).data('atype');
            paperBase.Title = $("#paperTitle").val();
            paperBase.Stage = $("#stage").val();
            paperBase.ChooseQus = '';
            paperBase.PerScores = '';
            paperBase.CompleteUrl = $paperMaker.options.chooseQuCompleteUrl;
            paperBase.AutoData = $("#autoData").val();//自动出卷的数据

            if (chooseQus.length > 0) {
                paperBase.ChooseQus = chooseQus;
            }
            if (perScores.length > 0) {
                paperBase.PerScores = JSON.stringify(perScores);
            }

            var form = $('<form></form>');
            form.attr('action', $paperMaker.options.chooseQuestionUrl);
            form.attr('method', 'post');
            form.attr('target', '_self');
            //paperBaseHidden
            var paperBaseHidden = $('<input type="hidden" name="paperBase" />');
            paperBaseHidden.attr('value', JSON.stringify(paperBase));
            form.append(paperBaseHidden);
            form.appendTo("body");
            form.submit();
        },
        inputAnswer: function () {
            $(".f-createQu").children('i').removeClass("fa-times").addClass("fa-plus");
            $('div.fa-createQuDiv').children('div').empty();
            $('div.fa-createQuDiv').hide();
            $("#chooseQuDiv").addClass('hide');
            $("#paperNav").addClass('hide');

            var paperData = $paperMaker.getPaperData();//获取试卷数据

            $("#inputAnswerDiv").empty().load($paperMaker.options.inputAnswerUrl, {paperData: JSON.stringify(paperData)});
        },
        getPaperAnswer: function () {
            var answers = [];

            //处理客观题
            var inputObjs = $('.g-wrap input[type=text]:visible:not(:disabled)');
            if (inputObjs && inputObjs.length > 0) {
                $.each(inputObjs, function (index, item) {
                    var qAnswer = {};
                    qAnswer.DetailId = $(item).data('detailid');
                    qAnswer.QId = $(item).data('qid');
                    qAnswer.Answer = $(item).val();

                    answers.push(qAnswer);
                });
            }

            //处理主观题
            var textObjs = $(".g-wrap .answerEditDiv");
            if (textObjs && textObjs.length > 0) {
                $.each(textObjs, function (index, item) {
                    var qAnswer = {};
                    qAnswer.QId = $(item).data('qid');
                    qAnswer.Answer = $('<div/>').text($("#content_" + qAnswer.QId).html()).html();

                    answers.push(qAnswer);
                });
            }

            return answers;
        }
    };

    //验证选题信息
    var validateChooseQu = function () {
        var questionContent = $("ul.paper-qContent");
        if (questionContent.length < 1) {
            singer.msg("请先添加题目！");
            return false;
        }
        for (var i = 0; i < questionContent.length; i++) {
            var lis = $(questionContent[i]).children("li");
            if (lis.length < 1) {
                singer.msg("请确保所有的题型都有题目！");
                return false;
            }
        }
        return true;
    };

    //验证客观题答案
    var validateAnswer = function () {
        var hasAnswer = true;

        var inputObjs = $('.g-wrap input[type=text]:visible:not(:disabled)');
        if (inputObjs && inputObjs.length > 0) {
            $.each(inputObjs, function (index, item) {
                var value = $(item).val();
                if (!$.trim(value)) {
                    hasAnswer = false;
                    return false;
                }
            });
        }

        if (!hasAnswer) {
            singer.msg("请先录入所有客观题的答案！");
            return false;
        }

        return true;
    };

    //验证分数
    var validateScore = function () {
        var hasScore = true;
        var scores = $(document).find(".qScore");
        if (scores) {
            $.each(scores, function (index, item) {
                var score = parseFloat($(item).val());
                if (isNaN(score) || score < 1) {
                    hasScore = false;
                    return false;
                }
            });
        }

        return hasScore;
    };

    //转换成汉字数字
    var convertToChinese = function (num) {
        var sortNum = ["〇", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十"];
        var no = parseInt(num);
        if (isNaN(no) || no > 20) {
            no = 1;
        }
        return sortNum[no];
    };

    //题型板块内部题目拖动排序事件
    var questionContentDrag = function (obj) {
        obj.find("ul.paper-qContent").dragsort({
            dragSelectorExclude: "input,textarea,div.f-posa,img.qImg,.s-del,.paper-del",
            dragEnd: $paperMaker.questionContentDragEnd
        });
    };
})(jQuery);


