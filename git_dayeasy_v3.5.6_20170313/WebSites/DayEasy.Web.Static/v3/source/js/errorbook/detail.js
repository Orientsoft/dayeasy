var ebDetail = (function () {
    var D = {};
    return D;
})();

$(function ($) {
    //参数
    ebDetail.error_id = $("#txtErrorId").val();
    ebDetail.batch = $("#txtBatch").val();
    ebDetail.paper_id = $("#txtPaperId").val();
    ebDetail.question_id = $("#txtQuestionId").val();
    ebDetail.class_id = $("#txtClassId").val(); ////////////????????
    ebDetail.is_objective = $("#txtObjectiveQuestion").val() == "1";
    ebDetail.is_teacher = $("#txtIsTeacher").val() == "1";
    ebDetail.user = {
        id: $("#txtUserId").val(),
        name: $("#txtUserName").val(),
        icon: $("#txtUserIcon").val()
    };


    //解析并展示公式
    setTimeout(singer.loadFormula, 120);

    //错因标签
    if (!ebDetail.is_teacher) {
        reason.load({eid: ebDetail.error_id, batch: '', qid: ''}, $(".my-reason"), false, false, {show_count: false});
    }

    var setPassBtn = function ($btn, pass, isHover) {
        if (!isHover) {
            $btn.data('pass', pass);
            $btn.html(pass ? '<i class="iconfont dy-icon-13"></i> <span>已过关</span>' : '<i class="iconfont dy-icon-radiohv"></i> <span>未过关</span>');
        } else {
            $btn.html(pass ? '<i class="iconfont dy-icon-13"></i> <span>已过关</span>' : '<span>取消过关</span>');
        }
        $btn.toggleClass('dy-btn-info passed', pass)
            .toggleClass('dy-btn-default', !pass);
    };
    //过关
    $(".btn-pass").bind("click", function () {
        var $this = $(this);
        var isPass = !$this.data("pass");
        $.post("/errorBook/pass", {id: ebDetail.error_id, isPass: isPass}, function (json) {
            if (json.status) {
                var msg = singer.format('错题状态更新为<b style="font-weight: 600">[{0}]</b>', (isPass ? '已过关' : '未过关'));
                singer.msg(msg);
                setPassBtn($this, isPass);
                return false;
            }
            singer.alert(json.message);
        });
        return false;
    }).hover(function () {
        var $this = $(this);
        setPassBtn($this, !$this.data('pass'), true);
    }, function () {
        var $this = $(this);
        setPassBtn($this, $this.data('pass'));
    }).focus(function () {
        $(this).blur();
    });

    //展开缩起评论列表
    $(".a-comment-power").bind("click", function () {
        var $this = $(this);
        var $box = $this.parent().siblings(".reason-comment");

        if ($this.data("load") != "1") {
            $this.data("load", 1);
            var sid = $box.data("sid");
            $.post("/reason/comments", {id: sid, pid: "", index: 1}, function (json) {
                if (json.status) {
                    var data = {
                        uid: ebDetail.user.id,
                        name: ebDetail.user.name,
                        icon: ebDetail.user.icon,
                        hasDelete: false,
                        box: $box,
                        addCall: addComment
                    };
                    var comments = [];
                    if (json.data && json.data.length) {
                        for (var i = 0; i < json.data.length; i++) {
                            var $d = json.data[i];
                            var c = {
                                uid: $d.user_id,
                                name: $d.user_name,
                                icon: $d.head,
                                sid: $d.id,
                                time: $d.time,
                                content: $d.content
                            };
                            if ($d.details && $d.details.length) {
                                c.comments = [];
                                for (var j = 0; j < $d.details.length; j++) {
                                    var $c = $d.details[j];
                                    c.comments.push({
                                        uid: $c.user_id,
                                        name: $c.user_name,
                                        sname: ($c.user_name + ' <span style="color:#999;font-size:12px">回复</span> ' + $c.parent_name),
                                        icon: $c.head,
                                        sid: $d.id,
                                        time: $c.time,
                                        content: $c.content
                                    });
                                }
                            }
                            comments.push(c);
                        }
                    }
                    data.comments = comments;
                    comment.load(data);
                } else {
                    singer.msg(json.message);
                }
            });
        }

        $box.toggleClass("hide");
    });

    //评论
    var addComment = function ($cbox, sid, uid, uname, content, callback) {
        var $box = $cbox.parent();
        if (!uid || uid.length)
            uid = $box.data("uid");
        if (!uname || !uname.length)
            uname = $box.data("uname");

        $.post("/reason/AddComment", {
            id: $box.data("sid"),
            pid: sid,
            uid: uid,
            content: content,
            uname: uname
        }, function (json) {
            if (json.status) {
                singer.msg("操作成功！", 500, function () {
                    if (callback && singer.isFunction(callback)) {
                        callback.call(this);
                    }
                });
                if (!sid || !sid.length) {
                    var $span = $box.siblings(".add-to-comment").find(".r-count");
                    var count = parseInt($span.data("count") || "0") + 1;
                    $span.data("count", count).text("点评(" + count + ")");
                }
            } else {
                singer.msg(json.message);
            }
        });
    };

    //删除分析及评论
    $(".a-comment-delete").bind("click", function () {
        var id = $(this).data("id");
        singer.confirm("确定删除分析及评论吗？", function () {
            $.post("/reason/delete", {id: id}, function (json) {
                if (json.status) {
                    singer.msg("删除成功", 2000, function () {
                        window.location.reload();
                    });
                } else {
                    singer.msg(json.message);
                }
            });
        });
    });
    //选项卡切换
    $('.tab-menu li').bind('click', function () {
        $(this).addClass('z-crt').siblings().removeClass('z-crt');
        $('.tab-contenr').find('.tab-con').hide().eq($(this).index()).show();
        if ($(this).index() == 1) {
            if (singer.isUndefined($(".tab-con-2").data("load")) || $(".tab-con-2").data("load") != "1") {
                $(".tab-con-2").data("load", "1");
                //我的答案
                if (!ebDetail.is_teacher) {
                    $.post("/errorBook/answer",
                        {
                            batch: ebDetail.batch,
                            paper_id: ebDetail.paper_id,
                            question_id: ebDetail.question_id
                        }, function (json) {
                            var $box = $(".eb-my-answer");
                            if (!json.status) {
                                var link = '/work/marking-detail?batch=' + ebDetail.batch + '&paperId=' + ebDetail.paper_id + '&studentId=' + ebDetail.user.id;
                                $box.append('<a target="_blank" href="' + link + '" style="color:#65cafc;">点击答卷详细</a>');
                            } else {
                                var data = json.data;
                                if (!data.has_answer) {
                                    //查询失败
                                    var link1 = '/work/marking-detail?batch=' + ebDetail.batch + '&paperId=' + ebDetail.paper_id + '&studentId=' + ebDetail.user.id + (data.isb ? "&type=b" : "");
                                    $box.append('<a target="_blank" href="' + link1 + '" style="color:#65cafc;">点击答卷详细</a>');
                                } else {
                                    if (!data.is_print) {
                                        //来源移动端
                                        for (var i = 0; i < data.answers.length; i++) {
                                            var item = data.answers[i];
                                            var $a = $('<div class="q-item">');
                                            $a.attr("style", "margin-bottom:10px;");
                                            if (item.body && item.body.length) {
                                                $a.append('<div>' + item.body + '</div>');
                                            }
                                            if (item.images && item.images.length) {
                                                var $imgs = $('<div>');
                                                $imgs.attr("style", "margin-bottom:5px;").addClass("q-image");
                                                for (var j = 0; j < item.images.length; j++) {
                                                    $imgs.append('<div title="查看大图"><img src="' + item.images[j] + '" /></div>');
                                                }
                                                $a.append($imgs);
                                            }
                                            if (!item.body && (!item.images || !item.images.length)) {
                                                $a.append('<div class="q-not-answer"><strong>未作答</strong></div>');
                                            }
                                            $box.append($a);
                                        }
                                    } else {
                                        //来源打印
                                        var $q = $('<div>');
                                        $q.attr("style", "margin-bottom:10px;");
                                        if (data.body && data.body.length) {
                                            $q.append('<div style="margin-bottom:5px;">' + data.body + '</div>');
                                        }
                                        if (data.img && data.img.length) {
                                            $q.append('<div class="q-image"><img alt="我的答案" src="' + data.img + '" style="border:5px solid #f0f0f0;" /></div>');
                                        }
                                        var $a1 = $('<div style="margin-bottom:5px;">');
                                        var link2 = '/work/marking-detail?batch=' + ebDetail.batch + '&paperId=' + ebDetail.paper_id + '&studentId=' + ebDetail.user.id + (data.isb ? '&type=b' : '');
                                        $a1.append('<a target="_blank" href="' + link2 + '" style="color:#65cafc;">查看我的答卷</a>');
                                        $q.append($a1);
                                        $box.append($q);
                                    }
                                }
                            }
                        });
                }
                //同学分享的答案
                if (!ebDetail.is_objective) {
                    $(".students-answer").show();
                    loadAnswerShare(false);
                }
            }
        }
    });
    //加载同学分享的答案
    function loadAnswerShare(all) {
        $.post("/reason/shares", {
            question_id: ebDetail.question_id,
            group_id: ebDetail.class_id,
            all: all
        }, function (json) {
            var $box = $(".tab-con-2").find(".students-answer");
            if (json.status) {
                if (json.data && json.data.length) {
                    var dds = $('<dl class="answer-dl after">');
                    dds.append("<dt>同学答案：</dt>")
                    for (var i = 0; i < json.data.length; i++) {
                        var item = json.data[i];
                        dds.append(singer.format('<dd data-val="{id}" title="{name}">{name}</dd>', item));
                    }
                    if (json.count > json.data.length) {
                        dds.append('<dd class="f-fr" data-val="more">更多…</dd>');
                    }
                    $box.html("").append(dds);
                } else {
                    $box.html("暂无同学分享答案");
                }
            } else {
                $box.html("同学答案加载失败");
            }
        });
    }

    //查看同学答案详细
    $(".students-answer").delegate("dd", "click", function () {
        var v = $(this).data("val");
        if (v == "more") {
            loadAnswerShare(true);
        } else {
            $.post("/reason/detail", {id: v}, function (json) {
                if (json.status) {
                    var $img = $('<img class="answer-img" src="' + json.data.img + '" alt="同学答案"/>');
                    var $thumb = $('<div class="answer-thumb mt5"><i class="fa fa-thumbs-o-up"></i>膜拜(<span class="worship-count">' + json.data.worship_count + '</span>)</div>');
                    if (json.data.had_worship) {
                        $thumb.attr("style", "cursor:default;color:#ffa64c;");
                    } else {
                        $thumb.data("id", v).attr("style", "cursor:pointer;color:#65cafc;");
                        $thumb.bind("click", function () {
                            var id = $(this).data("id");
                            $.post("/reason/worship", {id: id}, function (res) {
                                if (res.status) {
                                    $(".answer-thumb").unbind("click");
                                    var count = parseInt($(".answer-thumb").find("span").text()) + 1;
                                    $(".worship-count").text(count);
                                    $(".answer-thumb").attr("style", "cursor:default;color:#ffa64c;");
                                    var $wsName = $(".answer-bottom").find(".worship-name");
                                    var text = count > 1 ? "我," + $wsName.text() : "我";
                                    $wsName.text(text);
                                    $(".answer-bottom").show();
                                } else {
                                    singer.msg(res.message);
                                }
                            });
                        });
                    }
                    var $bottom = $('<div class="answer-bottom mt5">');
                    $bottom.append('<span class="worship-name">' + (json.data.worship_count > 0 ? json.data.worship_name : "") + '</span>');
                    if (json.data.worship_count > 5) {
                        $bottom.append('等<span class="worship-count">' + json.data.worship_count + '</span>人');
                    }
                    $bottom.append('&nbsp;进行了膜拜');
                    if (json.data.worship_count < 1) $bottom.hide();

                    var $box = $('<div class="answer-share-box">');
                    $box.append($img).append($thumb).append($bottom);

                    var _title = json.data.student_name + " 的答案分享";
                    var dialog = singer.dialog({
                        title: _title,
                        content: $box,
                        //fixed: true,
                        backdropOpacity: .7
                    });
                    $img.bind("load", function () {
                        dialog.showModal();
                    });
                } else {
                    singer.msg(json.message);
                }
            });

        }
    });
    //显隐变式训练答案
    $(".span-variant-answer-p").bind("click", function () {
        $(".variant-answer").toggleClass("hide");
    });

    //错误率图表
    var gaugeOptions = {
        chart: {
            type: 'solidgauge'
        },
        title: null,
        pane: {
            background: {
                backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || '#ccc',
                innerRadius: '80%',
                outerRadius: '100%',
                shape: 'arc'
            }
        },
        tooltip: {
            enabled: false
        },
        yAxis: {
            stops: [[1, '#ed5565']],
            lineWidth: 0,
            minorTickInterval: null,
            tickPixelInterval: 400,
            tickWidth: 0,
            title: {y: 110},
            labels: {y: 0}
        },
        plotOptions: {
            solidgauge: {
                dataLabels: {
                    y: 5,
                    borderWidth: 0,
                    useHTML: true
                }
            }
        }
    };

    function initHigchart(rateG, countG, rate, count, total) {
        //圈内错误率
        $('#container-survey-class').highcharts(Highcharts.merge(gaugeOptions, {
            yAxis: {
                min: 0,
                max: 100,
                labels: {
                    enabled: false
                },
                title: {
                    text: '<span style="color:#8d8d8d;font-size:14px">圈内 错</span><span style="color:#ed5565;font-size:16px"> ' + countG + '</span><span style="color:#8d8d8d;font-size:14px">人</span>'
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                name: '圈内错误率',
                data: [rateG],
                innerRadius: '80%',
                dataLabels: {
                    format: '<div style="text-align:center;font-size:20px;color:#ed5565">{y}%</div>',
                    y: -20
                }
            }]
        }));
        //全网错误率
        var text = (total == 0
            ? '<span style="color:#8d8d8d;font-size:14px">未进行统考</span>'
            : ('<span style="color:#8d8d8d;font-size:14px">全网 错</span><span style="color:#ed5565;font-size:16px"> '
        + count + '</span><span style="color:#8d8d8d;font-size:14px">人</span>'));

        $('#container-survey-all').highcharts(Highcharts.merge(gaugeOptions, {
            yAxis: {
                min: 0,
                max: 100,
                labels: {
                    enabled: false
                },
                title: {
                    text: text
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                name: '全网错误率',
                data: [rate],
                innerRadius: '80%',
                dataLabels: {
                    format: '<div style="text-align:center;font-size:20px;color:#ed5565">{y}%</div>',
                    y: -20
                }
            }]
        }));
    }

    //rates
    $.post("/reason/rates", {
            batch: ebDetail.batch,
            paper_id: ebDetail.paper_id,
            question_id: ebDetail.question_id
        },
        function (json) {
            if (json.status) {
                initHigchart(parseFloat(json.data.rate_g), json.data.count_g, parseFloat(json.data.rate), json.data.count, json.data.total);
            }
        });
});
