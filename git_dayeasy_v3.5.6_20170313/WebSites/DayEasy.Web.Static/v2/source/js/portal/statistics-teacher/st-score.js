/**
 * Created by epc on 2015/8/27.
 */

$(function ($) {
    //加载分数段、平均分统计数据
    var loaded = false;
    var loadAvgs = function () {
        var batch = $("#txtBatch").val(),
            paperId = $("#txtPaperId").val();
        $.post("statistics-avges", {batch: batch, paper_id: paperId}, function (json) {
            if (!json.status) {
                var _errHtml = '<div style="padding-top:20px;text-align: center; font-size: 16px;">' + json.message + '</div>';
                $(".tab-2").html(_errHtml);
                $(".tab-3").html(_errHtml);
                return;
            }
            bindAvgs(json.data);
        });
    };
    var bindAvgs = function (data) {
        var isAb = $("#txtIsAb").val() == 1;
        var currentClassId = data[0].class_id;
        var $tbScore = $('<table class="tables tables-canyi-1 tb-score">'),
            $tbAvg = $('<table class="tables tables-canyi-1 two-bg">'),
            $tbScoreA,
            $tbScoreB;

        //平均分标题行
        var trAvg = '<tr class="thead">' +
            '<th width="140">类型/分数/班级</th>' +
            '<th>平均分</th>' +
            '<th>最高分</th>' +
            '<th>最低分</th>' +
            (isAb ? '<th>A卷平均分</th><th>B卷平均分</th>' : '') +
            '</tr>';
        $tbAvg.append(trAvg);

        if (data[0].score_group && data[0].score_group.length) {
            if (data[0].score_group.length > 12) {
                //设置分数段表格宽度
                $tbScore.attr("style", "width:" + (140 * data[0].score_group.length) + "px;");
            }
            //整卷分数段标题行
            var $trg = $('<tr class="thead">');
            $trg.append('<th width="140">分数段/人数/班级</th>');
            for (var g = data[0].score_group.length - 1; g >= 0; g--) {
                var gItem = data[0].score_group[g];
                $trg.append('<th>' + gItem.ScoreInfo + '</th>');
            }
            $tbScore.append($trg);
        }
        if (isAb) {
            $tbScoreA = $('<table class="tables tables-canyi-1 tb-score hide">');
            $tbScoreB = $('<table class="tables tables-canyi-1 tb-score hide">');
            var abGroup = data[0].ab_score_group;
            if (abGroup && abGroup.length == 2) {
                if (abGroup[0] && abGroup[0].length) {
                    if (abGroup[0].length > 12) {
                        $tbScoreA.attr("style", "width:" + (140 * abGroup[0].length) + "px;");
                    }
                    //A卷分数段标题行
                    var $trga = $('<tr class="thead">');
                    $trga.append('<th width="140">分数段/人数/班级</th>');
                    for (var ga = abGroup[0].length - 1; ga >= 0; ga--) {
                        $trga.append('<th>' + abGroup[0][ga].ScoreInfo + '</th>');
                    }
                    $tbScoreA.append($trga);
                }
                if (abGroup[1] && abGroup[1].length) {
                    if (abGroup[1].length > 12) {
                        $tbScoreB.attr("style", "width:" + (140 * abGroup[1].length) + "px;");
                    }
                    //A卷分数段标题行
                    var $trgb = $('<tr class="thead">');
                    $trgb.append('<th width="140">分数段/人数/班级</th>');
                    for (var gb = abGroup[1].length - 1; gb >= 0; gb--) {
                        $trgb.append('<th>' + abGroup[1][gb].ScoreInfo + '</th>');
                    }
                    $tbScoreB.append($trgb);
                }
            }
        }

        var classids = []; //用于区分同班级多次考试的班级名称显示
        data = data.sort(function (a, b) {
            return a.class_name > b.class_name ? 1 : 0;
        });
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            var finded = false, className = item.class_name;
            for (var j = 0; j < classids.length; j++) {
                if (classids[j] == item.class_id) {
                    finded = true;
                }
            }
            if (finded) {
                className = item.class_name + "[" + item.time + "]";
            } else {
                classids.push(item.class_id);
            }
            //平均分
            var $trAvg = $('<tr>');
            if (currentClassId == item.class_id) {
                $trAvg.addClass('d-active');
            }
            $trAvg.append('<th>' + className + '</th>');
            $trAvg.append('<td>' + item.avg + '</td>');
            $trAvg.append('<td>' + item.max + '</td>');
            $trAvg.append('<td>' + item.min + '</td>');
            if (isAb && item.ab_avg) {
                $trAvg.append('<td>' + item.ab_avg.AAv + '</td>');
                $trAvg.append('<td>' + item.ab_avg.BAv + '</td>');
            }
            $tbAvg.append($trAvg);
            //整卷分数段
            var $trScore = $('<tr>');
            if (currentClassId == item.class_id) {
                $trScore.addClass('d-active');
            }
            $trScore.append('<th>' + className + '</th>');
            if (item.score_group && item.score_group.length) {
                for (var s = item.score_group.length - 1; s >= 0; s--) {
                    $trScore.append('<td>' + item.score_group[s].Count + '</td>');
                }
            }
            $tbScore.append($trScore);
            if (isAb) {
                //A卷分数段
                var $trScoreA = $('<tr>');
                $trScoreA.append('<th>' + className + '</th>');
                if (item.ab_score_group && item.ab_score_group.length == 2 && item.ab_score_group[0] && item.ab_score_group[0].length) {
                    for (var sa = item.ab_score_group[0].length - 1; sa >= 0; sa--) {
                        $trScoreA.append('<td>' + item.ab_score_group[0][sa].Count + '</td>');
                    }
                }
                $tbScoreA.append($trScoreA);
                //B卷分数段
                var $trScoreB = $('<tr>');
                $trScoreB.append('<th>' + className + '</th>');
                if (item.ab_score_group && item.ab_score_group.length == 2 && item.ab_score_group[1] && item.ab_score_group[1].length) {
                    for (var sb = item.ab_score_group[1].length - 1; sb >= 0; sb--) {
                        $trScoreB.append('<td>' + item.ab_score_group[1][sb].Count + '</td>');
                    }
                }
                $tbScoreB.append($trScoreB);
            }
        }
        //绑定到页面
        var boxScore = $(".tab-2 .hider-y");
        boxScore.html("").append($tbScore);
        if (isAb) {
            boxScore.append($tbScoreA);
            boxScore.append($tbScoreB);
        }
        $(".tab-3").html("").append($tbAvg);
    };

    //选项卡切换
    $(".tac-wrap li").bind("click", function () {
        if ($(this).hasClass("z-crt")) return;
        $(this).siblings().removeClass("z-crt");
        $(this).addClass("z-crt");
        var $tabs = $(".tab-list");
        $tabs.hide();
        $tabs.eq($(this).index()).show();
        if ($(this).index() != 0 && !loaded) {
            loaded = true;
            loadAvgs();
        }
    });
    //下拉框AB卷分数切换
    $("#ddlChangeTab").bind("change", function () {
        var idx = parseInt($(this).val());
        $(".tb-score").addClass("hide");
        $(".tb-score").eq(idx).removeClass("hide");
    });
    //排序
    $('table').tablesorter({
        widgets: ['zebra', 'columns'],
        headers: {
            0: {sorter: false},
            1: {sorter: false}
        }
    });
});