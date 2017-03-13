/**
 * Created by epc on 2015/8/24.
 */
var SQ = (function () {
    var T = {
        def: DEYI.sites.static + '/image/default/user_s50x50.jpg',
        students: [],
        details: [],
        errors: [],
        data: 0,
        student: function (id) {
            if (T.students.length) {
                for (var i = 0; i < T.students.length; i++) {
                    if (T.students[i].id == id)
                        return T.students[i];
                }
            }
            return 0;
        }
    };
    return T;
})();

//加载错误学生姓名、头像
template.helper("bindStudent", function (id, idx) {
    var result = '', isPush = SQ.data.is_push;
    for (var i = 0; i < SQ.students.length; i++) {
        var student = SQ.students[i];
        if (student.id == id) {
            var pic = student.avatar && student.avatar.length ? student.avatar : SQ.def;
            var tmp = isPush ? ' style="cursor:default;" ' : ' onclick="location2Detail(this);" ';
            result = '<li  class="pointer" data-studentid="' + student.id + '"' + tmp + '>' +
                '<img src="' + pic + '" alt="" width="32" height="32" />' +
                '<span>' + student.name + '</span>' +
                '</li>';
            break;
        }
    }
    return result;
});

//链接到学生答题详情界面
var location2Detail = function (obj) {
    if (SQ.data.is_push) return;
    singer.open("/work/marking-detail?batch=" + $("#txtBatch").val() + "&paperId=" + $("#txtPaperId").val() + "&studentId=" + $(obj).data('studentid'));
};

$(function ($) {
    var batch = $("#txtBatch").val(),
        paperId = $("#txtPaperId").val(),
        groupId = $("#txtGroupId").val();
    if (!batch || !batch.length || !paperId || !paperId.length || !groupId || !groupId.length)
        return;

    //解析并展示公式
    singer.loadFormula();

    //加载圈内学生资料
    $.post("class-students", {group_id: groupId}, function (json) {
        if (json.status) {
            if (!json.data || !json.data.length) {
                SQ.students = -1;
                return;
            }
            SQ.students = json.data;
            //查询错题信息
            $.get('/work/error-count', {batch: batch}, function (json) {
                if (!json.status || !json.data) return;
                SQ.errors = json.data;
                bindErrors();
            });
        } else {
            singer.msg("加载圈内学生资料失败，请刷新重试")
        }
    });

    //加载各问题错因分析数量
    $.post("statistics-question-reasons", {batch: batch}, function (json) {
        if (json && json.length) {
            $(".reason-count").each(function (idx, span) {
                if (!json.length) return;
                var id = $(span).parents(".q-item").data("id");
                for (var i = 0; i < json.length; i++) {
                    if (id == json[i].key) {
                        $(span).text(json[i].value);
                        break;
                    }
                }
            });
        }
    });
    //页面事件
    //显隐错误学生
    $("#divSections").delegate(".dy-btn-default", "click", function () {
        var $this = $(this);
        $this.blur();

        if (SQ.students == -1) {
            $this.removeClass("dy-btn-info");
            singer.msg("学生资料加载失败，请刷新重试");
            return;
        }
        if (!SQ.students || !SQ.students.length) {
            $this.removeClass("dy-btn-info");
            singer.msg("正在加载学生资料，请稍后");
            return;
        }

        //容器
        var $p = $this.parents(".q-item");
        var $box = $p.find(".questions-bottom-cont");
        //当前问题
        var qid = $p.data("id");
        //是否为客观题
        var isObjective = $p.data("objective") == "1";
        //是否存在含小问的客观题
        var hasSmall = isObjective && $p.data("hassmall") == "1";
        //是否已加载错题详细模版
        if ($this.data("loadHtml") != "1") {
            $this.data("loadHtml", 1);
            var es = [];
            if (SQ.errors && SQ.errors.length && SQ.errors.hasOwnProperty(qid));
            es = SQ.errors[qid];

            $box.html(template('s-error-details', {
                is_push: SQ.data.is_push,
                is_objective: isObjective,
                error_students: es
            }));
        }

        //如果有小问-则交给小问按钮加载饼图
        if ($this.data("isshow") == "1") {
            $this.data("isshow", 0).removeClass("dy-btn-info");
            if (hasSmall) {
                $p.find(".small-sort-list").stop().slideUp();
                $box.stop().slideUp();
            } else {
                $box.stop().slideUp();
            }
        } else {
            $this.data("isshow", 1).addClass("dy-btn-info");
            if (hasSmall) {
                var $smallList = $p.find(".small-sort-list");
                $smallList.stop().slideDown();
                $box.stop().slideDown();
                //默认加载第一个小问
                if ($smallList.data("loadFirst") != "1") {
                    var $sq = $smallList.find(".small-sort").eq(0);
                    loadSmallQuestionStatistics($sq);
                    $smallList.data("loadFirst", 1);
                }
                return;
            } else {
                $box.stop().slideDown();
            }
            //加载饼图
            if (isObjective) {
                //检测是否已缓存本题详情
                for (var i = 0; i < SQ.details.length; i++) {
                    var $d = SQ.details[i];
                    if ($d.qid == qid && $d.sid == "") return;
                }
                $.post("statistics-question-detail", {
                    batch: batch,
                    paper_id: paperId,
                    question_id: qid,
                    small_question_id: ""
                }, function (json) {
                    if (json.status) {
                        SQ.details.push({qid: qid, sid: "", data: 0});
                        bindQuestionDetail(json.data, $p);
                    } else {
                        singer.msg(json.message);
                    }
                });
            }
        }
    })
        .delegate(".small-sort", "click", function () {
            //有小问 - 加载小问饼图
            loadSmallQuestionStatistics($(this));
        })
        .delegate(".link-variable", "click", function () {
            //链接 - 变式推送
            var id = $(this).parents(".q-item").data("id");
            singer.open("/variant?paper_id=" + paperId + "&question_id=" + id + "&app=work");
            return false;
        })
        .delegate(".link-detail", "click", function () {
            //链接 - 问题详细
            var id = $(this).parents(".q-item").data("id");
            singer.open("detail/" + batch + "/" + paperId + "/" + id);
            return false;
        });
    //加载小问饼图
    var loadSmallQuestionStatistics = function ($sq) {
        if ($sq.hasClass("active")) return;
        $sq.siblings(".small-sort").removeClass("active");
        $sq.addClass("active");
        var $p = $sq.parents(".q-item");
        var qid = $p.data("id");
        var sid = $sq.data("id");
        //检测是否已缓存小问的全部饼图数据
        var data = 0;
        for (var i = 0; i < SQ.details.length; i++) {
            var $d = SQ.details[i];
            if ($d.qid == qid && $d.sid == sid) {
                data = $d.data;
            }
        }
        if (data != 0) {
            bindQuestionDetail(data, $p);
            return;
        }
        $.post("statistics-question-detail", {
            batch: batch,
            paper_id: paperId,
            question_id: qid,
            small_question_id: sid
        }, function (json) {
            if (json.status) {
                SQ.details.push({qid: qid, sid: sid, data: json.data});
                bindQuestionDetail(json.data, $p);
            } else {
                singer.msg(json.message);
            }
        });
    };
    //绑定答题详细
    var bindQuestionDetail = function (data, $p) {
        if (!data) return;
        $p.find(".f-tal").text("答案：" + data.system_answer);
        if (!data.answers || !data.answers.length)
            return;
        //饼图默认展开第一扇
        data.answers[0].sliced = true;
        data.answers[0].selected = true;
        $p.find(".statistics-lca").highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                width: 200,
                height: 200
            },
            title: null,
            tooltip: {
                pointFormat: '{series.name}: <b>{point.y}%</b>'
            },
            plotOptions: {
                pie: {
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: false
                    },
                    showInLegend: false,
                    events: {
                        click: function (event) {
                            bindAnswerDetail(event.point.students, $p, event.point.name);
                        }
                    }
                }
            },
            series: [{
                type: 'pie',
                name: '选项统计',
                data: data.answers
            }]
        });
        //饼图第一扇对应选项的答错学生
        bindAnswerDetail(data.answers[0].students, $p, data.answers[0].name);
    };
    //绑定各选项详细
    var bindAnswerDetail = function (students, $p, name) {
        var $ul = $p.find(".list-nonumber");
        $ul.html("");
        var title = '<em>{0}</em>的同学';
        if (name != '未答') {
            title = '选' + title;
        }
        $('.list-statistics-title').html(singer.format(title, name));

        var tmp = SQ.data.is_push ? ' style="cursor:default;" ' : ' onclick="location2Detail(this);" ';
        for (var i = 0; i < students.length; i++) {
            var student = SQ.student(students[i]);
            if (student) {
                var pic = student.avatar && student.avatar.length ? student.avatar : SQ.def;
                var li = '<li  class="pointer" data-studentid="' + student.id + '"' + tmp + '>' +
                    '<img src="' + pic + '" alt="" width="32" height="32" />' +
                    '<span>' + student.name + '</span>' +
                    '</li>';
                $ul.append(li);
            }
        }
    };
    //绑定出错人数
    var bindErrors = function () {
        $(".error-count").each(function (idx, em) {
            for (var attr in SQ.errors) {
                if (!SQ.errors.hasOwnProperty(attr))
                    continue;
                var id = $(em).parents(".q-item").data("id");
                if (attr == id) {
                    $(em).text((SQ.errors[attr].length || 0));
                }
            }
        });
    };
    //为加载成功显示错误信息
    var initErrorView = function (msg) {
        $(".p-err-message").text(msg);
        $(".coach-base-title").hide();
        $("#divLoading").hide();
        $("#divSections").show();
    };
});
