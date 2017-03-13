/**
 * Created by epc on 2015/8/24.
 */
var SQ = (function () {
    var T = {
        chinese: ["零", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十"],
        def: DEYI.sites.static + '/image/default/user_s50x50.jpg',
        students: [],
        details: [],
        data: 0,
        init: function () {
            if (!T.data || !T.students.length) return;
            singer.loadTemplate('statistics-question', function () {
                $("#paperTitle").html(T.data.paper_name);
                $("#className").text(T.data.class_name);

                if ($("#txtIsAb").val() == "1") {
                    T.bind(1);
                    T.bind(2);
                } else {
                    T.bind(0);
                }

                $("#divLoading").hide();
                $("#divData").show();
            });
        },
        bind: function (t) {
            var $s = singer.render('s-section', { paper_type: t, sections: T.data.sections });
            $("#divSections").append($s);
        },
        student: function (id) {
            if (T.students.length) {
                for (var i = 0; i < T.students.length; i++) {
                    if (T.students[i].user_id == id)
                        return T.students[i];
                }
            }
            return 0;
        }
    };
    return T;
})();

//中文序号
template.helper("chineseNum", function (num) {
    var result = '';
    if (num > -1 && num < 21) {
        result = SQ.chinese[num];
    }
    return result;
});

//加载错误学生姓名、头像
template.helper("bindStudent", function (id, idx) {
    var result = '';
    for (var i = 0; i < SQ.students.length; i++) {
        var student = SQ.students[i];
        if (student.user_id == id) {
            var pic = student.head_pic && student.head_pic.length ? student.head_pic : SQ.def;
            result = '<li  class="pointer" data-studentid="' + student.user_id + '" onclick="location2Detail(this);" ' + (idx > 9 ? ' style="display: none;"' : '') + '>' +
            '<img src="' + pic + '" alt="" width="32" height="32" />' +
            '<span>' + student.real_name + '</span>' +
            '</li>';
            break;
        }
    }
    return result;
});


var location2Detail = function (obj) {
    singer.open("/work/marking-detail?batch=" + $("#txtBatch").val() + "&paperId=" + $("#txtPaperId").val() + "&studentId=" + $(obj).data('studentid'));
}

$(function ($) {
    var batch = $("#txtBatch").val(),
        paperId = $("#txtPaperId").val();
    if (!batch || !batch.length || !paperId || !paperId.length) return;
    $.post("statistics-question-data", { batch: batch, paper_id: paperId }, function (json) {
        if (json.status) {
            SQ.data = json.data;
            $.post("class-students", { classId: json.data.class_id }, function (json1) {
                if (json1.status) {
                    $("#divSections").html("");
                    SQ.students = json1.data;
                    SQ.init();
                } else {
                    initErrorView(json1.message);
                }
            });
        } else {
            initErrorView(json.message);
        }
    });

    //页面事件
    //链接 - 变式推送
    $(".topic-main").delegate(".link-variable", "click", function () {
        var id = $(this).data("id");
        singer.open("/variant/push?paper_id=" + paperId + "&question_id=" + id + "&app=work");
    });
    //链接 - 问题详细
    $(".topic-main").delegate(".link-detail", "click", function () {
        var id = $(this).data("id");
        singer.open("detail/" + batch + "/" + paperId + "/" + id);
    });
    //显隐全部错误学生
    $(".topic-main").delegate(".fill-btn", "click", function () {
        var $p = $(this).parents('.fill-inthe-blanks');
        if ($p.find('.td-3 .list-1  li:gt(9)').is(':visible')) {
            $p.find('.td-3 .list-1  li:gt(9)').hide();
            $(this).text("更多").removeClass("fill-btnhv");
        } else {
            $p.find('.td-3 .list-1  li').show();
            $(this).text("收起").addClass("fill-btnhv");
        }
    });
    //显隐饼图详情
    $(".topic-main").delegate(".f-right-expansion", "click", function () {
        var $p = $(this).parents('.fill-inthe-blanks');
        if ($p.hasClass("width-hide-show")) {
            $p.removeClass("width-hide-show");
            return;
        }
        var qid = $(this).parents(".q-parent").data("id"),
            sid = $(this).parents(".q-item").data("id");
        if (!qid || !qid.length) {
            qid = sid;
            sid = '';
        }
        //检测是否已缓存本题详情
        for (var i = 0; i < SQ.details.length; i++) {
            var $d = SQ.details[i];
            if ($d.qid == qid && $d.sid == sid) {
                $p.addClass("width-hide-show");
                return;
            }
        }
        $.post("statistics-question-detail", {
            batch: batch,
            paper_id: paperId,
            question_id: qid,
            small_question_id: sid
        }, function (json) {
            if (json.status) {
                SQ.details.push({ qid: qid, sid: sid });
                bindQuestionDetail(json.data, $p);
            } else {
                singer.msg(json.message);
            }
        });
    });
    //绑定答题详细
    var bindQuestionDetail = function (data, $p) {
        if (!data) {
            $p.addClass("width-hide-show");
            return;
        }
        $p.find(".f-tal").text("答案：" + data.system_answer);
        if (!data.answers || !data.answers.length) {
            $p.addClass("width-hide-show");
            return;
        }
        //饼图默认展开第一扇
        data.answers[0].sliced = true;
        data.answers[0].selected = true;
        $p.find(".statistics-lca").highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false,
                width: 180,
                height: 180
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
                            bindAnswerDetail(event.point.students, $p);
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
        bindAnswerDetail(data.answers[0].students, $p);
        $p.addClass("width-hide-show");
    };
    //绑定各选项详细
    var bindAnswerDetail = function (students, $p) {
        var $ul = $p.find(".list-2");
        $ul.html("");
        for (var i = 0; i < students.length; i++) {
            var student = SQ.student(students[i]);
            if (student) {
                var pic = student.head_pic && student.head_pic.length ? student.head_pic : SQ.def;
                var li = '<li class="pointer" data-studentid="' + student.user_id + '" onclick="location2Detail(this);">' +
                    '<img src="' + pic + '" alt="" width="32" height="32" />' +
                    '<span>' + student.real_name + '</span>' +
                    '</li>';
                $ul.append(li);
            }
        }
    };
    //为加载成功显示错误信息
    var initErrorView = function (msg) {
        $(".p-err-message").text(msg);
        $(".topic-head").hide();
        $("#divLoading").hide();
        $("#divData").show();
    };
});
