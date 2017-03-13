/**
 * Created by epc on 2016/5/31.
 */
$(function () {
    var S = SINGER,
        batch = $("#txtBatch").val(), paperId = $("#txtPaperId").val(),
        loadSuvQuestionAndKps, bindSuvQuestions, bindSuvKps, //题目作答、知识点情况
        loadSuvScoreGroup, bindSuvScoreGroup, //分数段统计
        loadSuvStudents, bindSuvStudents, //需要关注的学生
        linkToDetail, //页面跳转
        nothingHtml; //获取失败提示Html

    loadSuvQuestionAndKps = function () {
        $.post("/work/teacher/survey-graspings", {batch: batch, "paper_id": paperId}, function (json) {
            $(".suv-questions .dy-loading").remove();
            $(".suv-kps .dy-loading").remove();

            if (json.status && json.data.questions && json.data.questions.length) {
                bindSuvQuestions(json.data.questions);
            } else {
                $(".suv-questions").append(nothingHtml(json.message));
            }

            if (json.status && json.data.knowledges && json.data.knowledges.length) {
                bindSuvKps(json.data.knowledges);
            } else {
                $(".suv-kps").append(nothingHtml(json.message));
            }
        });
    };
    bindSuvQuestions = function (questions) {
        var $box = $(".suv-questions");
        var dangers = [], warns = [], safetys = [];
        for (var i = 0; i < questions.length; i++) {
            var question = questions[i];
            if (question.scoreRate < 50) {
                dangers.push(question);
            } else if (question.scoreRate < 80) {
                warns.push(question);
            } else {
                safetys.push(question);
            }
        }

        if (dangers && dangers.length) {
            var dangerHtml = template('ss-question-template', {
                "sqClass": "ss-danger",
                "title": "掌握较差",
                "content": " 建议重点评讲",
                "list": dangers
            });
            $box.append(dangerHtml);
        }
        if (warns && warns.length) {
            var warnHtml = template('ss-question-template', {
                "sqClass": "ss-warn",
                "title": "掌握一般",
                "content": " 建议稍作评讲",
                "list": warns
            });
            $box.append(warnHtml);
        }
        if (safetys && safetys.length) {
            var primaryHtml = template('ss-question-template', {
                "sqClass": "ss-safety",
                "title": "掌握较好",
                "content": " 可以不评讲",
                "list": safetys
            });
            $box.append(primaryHtml);
        }

        $(".suv-questions .ss-qitem").bind("click", function () {
            var qid = $(this).data("qid");
            if (!qid || !qid.length) return;
            linkToDetail(qid);
        });
    };
    bindSuvKps = function (data) {
        var kpsHtml = $(template('ss-kps-template', data));
        kpsHtml.find('.ratio-bar-deep').each(function (index, item) {
            var $t = $(item),
                rate = $t.data('rate'), cssName;
            if(rate>0) {
                if (rate < 50) cssName = 'bg-color-danger';
                else if (rate < 80) cssName = 'bg-color-warn';
                else cssName = 'bg-color-safety';
                $t.css({
                    width: rate + '%'
                }).addClass(cssName);
            }
        });
        $(".suv-kps").append(kpsHtml);

        $(".suv-kps .sk-qitem").bind("click", function () {
            var qid = $(this).data("qid");
            if (!qid || !qid.length) return;
            linkToDetail(qid);
        });
    };

    loadSuvScoreGroup = function () {
        $.post("/work/teacher/statistics-avges", {
            "batch": batch,
            "paper_id": paperId,
            "single": true
        }, function (json) {
            $(".suv-score-group .dy-loading").remove();
            if (!json.status || !json.data.length) {
                $(".suv-score-group").append(nothingHtml(json.message));
                return;
            }
            bindSuvScoreGroup(json.data[0]);
        });
    };
    bindSuvScoreGroup = function (data) {
        //图表分数段
        var cate = [], counts = [];
        var i = data.score_groupes.length - 1;
        for (; i >= 0; i--) {
            var item = data.score_groupes[i];
            cate.push(item.score_info);
            counts.push({
                y: item.count,
                color: i % 2 == 0 ? '#a7e0f4' : '#caecf8'
            });
        }

        var $groupBox = $(".ss-group-box");
        if (cate.length && counts.length) {
            $groupBox.removeClass("hide")
                .highcharts({
                    chart: {type: 'bar'},
                    title: {
                        text: '分数段',
                        align: "left",
                        style: {fontSize: "16px", color: "#555"}
                    },
                    legend: {enabled: false},
                    xAxis: {categories: cate},
                    yAxis: {
                        allowDecimals: false,
                        title: {text: '人数', align: 'high'}
                    },
                    series: [{
                        name: '人数',
                        data: counts,
                        dataLabels: {
                            enabled: true,
                            color: '#666',
                            align: 'right',
                            x: 40,
                            y: 2,
                            style: {fontSize: '12px'},
                            formatter: function () {
                                return this.y + '人';
                            }
                        }
                    }]
                });
        } else {
            $groupBox.removeClass("hide").html(nothingHtml());
        }

        //表格
        var tableHtml = template('ss-score-group-template', data);
        $(".suv-score-group").append(tableHtml);
    };

    loadSuvStudents = function () {
        $.post("/work/teacher/survey-analysis", {"batch": batch, "paper_id": paperId}, function (json) {
            $(".suv-students .dy-loading").remove();
            if (!json.status) {
                $(".suv-students").append(nothingHtml(json.message));
                return;
            }
            bindSuvStudents(json.data);
        });
    };
    bindSuvStudents = function (data) {
        var studentHtml = template('ss-student-template', {"batch": batch, "paperId": paperId, "data": data});
        $(".suv-students").append(studentHtml);

        //查看更多
        $(".suv-students .td2").each(function (i, td) {
            var $td = $(td);
            if ($td.height() > 30) {
                $td.append('<div class="st-more"><i class="iconfont dy-icon-anglebottom"></i></div>');
                var $box = $td.find(".stu-box");
                if ($box.data("big") == "1") {
                    if ($td.height() > 40)
                        $box.addClass("more-hide more-hide-lg");
                } else {
                    $box.addClass("more-hide");
                }
            }
        });
        $(".suv-students .st-more").bind("click", function () {
            var $this = $(this);
            var $box = $this.siblings(".stu-box");
            if ($this.data("open") == "1") {
                $this.data("open", 0).find("i").attr("class", "iconfont dy-icon-anglebottom");
                $box.addClass("more-hide");
                if ($box.data("big") == "1")
                    $box.addClass("more-hide-lg");
            } else {
                $this.data("open", 1).find("i").attr("class", "iconfont dy-icon-angletop");
                $box.removeClass("more-hide");
                if ($box.data("big") == "1")
                    $box.removeClass("more-hide-lg");
            }
        });
    };

    linkToDetail = function (questionId) {
        var url = S.sites.apps + "/work/teacher/detail/" + batch + "/" + paperId + "/" + questionId;
        S.open(url);
    };

    nothingHtml = function (msg) {
        msg = msg || "没有相关数据";
        return '<div class="dy-nothing">' + msg + '</div>';
    };

    //初始化...
    loadSuvQuestionAndKps();
    loadSuvScoreGroup();
    loadSuvStudents();

});