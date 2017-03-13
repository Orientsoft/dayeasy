/**
 * 分数排名
 * Created by epc on 2015/8/27.
 */
(function ($, S) {
    //加载分数段、平均分统计数据
    var loaded = false,
        data = [],
        batch = $("#txtBatch").val(),
        $avgLoading = $('#boxAvgLoading'),
        $avgData = $('#boxAvgData');

    /**
     * 加载分数段统计
     */
    var loadAvgs = function () {
        var jointBatch = $("#txtJointBatch").val(),
            paperId = $("#txtPaperId").val();

        if (jointBatch && jointBatch.length) {
            $.post("statistics-avges-joint", {joint_batch: jointBatch, paper_id: paperId}, function (json) {
                if (!json.status) {
                    $avgLoading.html('<div class="dy-nothing">' + json.message + '</div>');
                    return;
                }
                data = json.data;
                bindAvgs(0);
            });
        } else {
            $.post("statistics-avges", {batch: batch, paper_id: paperId}, function (json) {
                if (!json.status) {
                    $avgLoading.html('<div class="dy-nothing">' + json.message + '</div>');
                    return;
                }
                data = json.data;
                bindAvgs(0);
            });
        }
    };
    var bindAvgs = function (t) {
        if (!data || !data.length) {
			
            $avgLoading.html('<div class="dy-nothing">没有没有相关数据！</div>').show();
            $avgData.hide();
        }
        if ($("#txtIsAb").val() != 1)
            t = 0;
        if (!data || !data.length)
            return false;

        var $tab = $(".avg-wrap"),
            viewData = {segments: [], groups: [], counts: [], isUnion: false},
            segments = (t == 0 ? data[0].score_groupes : data[0].ab_score_groupes[t - 1]).concat(),
            agencyList = [];
			
        //倒序
        S.each(segments.sort(function () {
            return 1;
        }), function (segment) {
            viewData.segments.push(segment.score_info);
        });
	
        if (t == 0) {
            viewData.segments.push('最低分', '最高分', '平均分');
        }
        S.each(data, function (item) {
            viewData.groups.push({
                name: item.group_name,
                agency: item.agency_name
            });
            if (!S.inArray(item.agency_id, agencyList))
                agencyList.push(item.agency_id);
            var scores = {};
            var itemSegments = (t == 0 ? item.score_groupes : item.ab_score_groupes[t - 1]);
            S.each(itemSegments, function (segment) {
                scores[segment.score_info] = segment.count;
            });
            if (t == 0) {
                scores['最低分'] = item.min;
                scores['最高分'] = item.max;
                scores['平均分'] = item.avg;
            }
            viewData.counts.push(scores);
        });
			
        viewData.isUnion = agencyList.length > 1;
		console.log(viewData);
        var viewHtml = template('segmentTpl', viewData);
        $tab.html(viewHtml);
        $avgLoading.hide();
        $avgData.show();
        $(".ove-x").mCustomScrollbar("update");
    };

    //加载平均分统计
    $("#aLoadAvge").bind("click", function () {
        if (!loaded) {
            loaded = true;
            loadAvgs();
        }
    });

    //下拉框统计共享切换
    $("#ddlShares").bind("change", function () {
        var groupId = $(this).val();
        $.post("statistics-share", {batch: batch, group_id: groupId}, function (json) {
            if (!json.status) {
                singer.msg(json.message);
                return;
            }
            S.msg("设置成功", 1000, function () {
                loadAvgs();
            });
        });
    });

    //下拉框AB卷分数切换
    $("#ddlChangeTab").bind("change", function () {
        bindAvgs(parseInt($(this).val()));
    });

    //切换班级查看统计 - 协同
    $("#ddlGroups").bind("change", function () {
        window.location.href =
            "?joint_batch=" + $("#txtJointBatch").val() +
            "&paper_id=" + $("#txtPaperId").val() +
            "&group_id=" + $(this).val();
    });

    //编辑分数
    var editScores = [];
    var formatToFloat = function (val) {
        if (!val || !val.length) return 0;
        if (!/^[0-9]{0,3}\.??[0-9]{1,2}$/.test(val)) return 0;
        return parseFloat(parseFloat(val).toFixed(2));
    };
    $(".txt-edit-score").bind("change", function () {
        var $this = $(this);
        var val = $this.val(), reg = /^[0-9]{0,3}\.??[0-9]{1,2}$/;
        if (!reg.test(val)) {
            var _def = $this.data("def");
            if (!reg.test(_def)) $this.val(_def);
            else $this.val(parseFloat(_def || "0"));
            return;
        }
        var uid = $this.parents("tr").data("uid") || "";
        var t = $this.data("type");
        var maxScore = 0;
        var item = getScoreItem(uid);
        val = formatToFloat(val);
        if ($("#txtIsAb").val() == 1) {
            var $tr = $this.parents("tr");
            var tmp = 0;
            if (t == "a") {
                maxScore = formatToFloat($("#txtScoreA").val());
                if (val > maxScore) {
                    val = maxScore;
                    $this.val(val);
                }
                item.sectionAScore = val;
                tmp = formatToFloat($tr.find(".txt-edit-score").eq(1).val());
            } else {
                maxScore = formatToFloat($("#txtScoreB").val());
                if (val > maxScore) {
                    val = maxScore;
                    $this.val(val);
                }
                item.sectionBScore = val;
                tmp = formatToFloat($tr.find(".txt-edit-score").eq(0).val());
            }
            $tr.find(".td-total-score").text(val + tmp);
        } else {
            maxScore = formatToFloat($("#txtScoreT").val());
            if (val > maxScore) {
                val = maxScore;
                $this.val(val);
            }
            item.totalScore = val;
        }
    });
    var getScoreItem = function (uid) {
        for (var i = 0; i < editScores.length; i++) {
            if (editScores[i].studentId == uid)
                return editScores[i];
        }
        var item = {};
        $("#tabScore").find("tr").each(function (i, tr) {
            if ($(tr).data("uid") == uid) {
                var id = $(tr).data("id");
                var $ipts = $(tr).find(".txt-edit-score");
                if ($("#txtIsAb").val() == 1) {
                    //Ab卷
                    var sa = formatToFloat($ipts.eq(0).val()),
                        sb = formatToFloat($ipts.eq(1).val());
                    item = {id: id, studentId: uid, sectionAScore: sa, sectionBScore: sb};
                } else {
                    item = {id: id, studentId: uid, totalScore: formatToFloat($ipts.eq(0).val())};
                }
                editScores.push(item);
            }
        });
        return item;
    };
    //编辑分数按钮
    var showHideEdit = function (t) {
        if (t) {
            $(".slide-table").addClass("current");
            $(".txt-edit-score").removeAttr("disabled");
        } else {
            $(".slide-table").removeClass("current");
            $(".txt-edit-score").attr("disabled", "disabled");
        }
    };
    $(".ck-edit").bind("click", function () {
        editScores = [];
        $(".txt-edit-score").each(function () {
            $(this).data("def", $(this).val());
        });
        showHideEdit(true);
    });
    $(".ck-cancel").bind("click", function () {
        $(".txt-edit-score").each(function () {
            var $this = $(this);
            $this.val($this.data("def"));
            if ($("#txtIsAb").val() == 1) {
                var $ts = $this.parents("tr").find(".td-total-score");
                $ts.text($ts.data("def") || "");
            }
        });
        showHideEdit(false);
    });
    $(".txt-edit-score").bind("focus", function () {
        var $this = $(this);
        if ($this.val() == "-") $this.val("0");
        $(this).addClass("focus");
    }).bind("blur", function () {
        $(this).removeClass("focus");
    });
    //编辑分数提交
    $(".ck-complete").bind("click", function () {
        var $btn = $(this);
        $btn.attr("disabled", "disabled");
        if (editScores.length) {
            $.post("statistics-edit-socre",
                {
                    batch: batch,
                    paper_id: $("#txtPaperId").val(),
                    data: singer.json(editScores)
                }, function (json) {
                    $btn.removeAttr("disabled");
                    if (!json.status) {
                        singer.msg(json.message);
                        return;
                    }
                    S.msg("操作成功", 1000, function () {
                        window.location.reload(true);
                    });
                    // showHideEdit(false);
                });
        } else {
            showHideEdit(false);
        }
    });

    //表扬、发送成绩通知短信
    $(".cbx-stu-info").removeAttr("checked");//页面加载清空cbx缓存    
    $("#btnPraise").attr("disabled", "disabled");
    $("#btnSendScore").attr("disabled", "disabled");
    var praises = [];
    $(".cbx-stu-info").bind("change", function () {
        var id = $(this).data("id");
        var checked = this.checked;
        if (id == "0") {
            praises = [];
            $(".cbx-stu-info").not($(this)).each(function (i, cbx) {
                cbx.checked = checked;
                if (checked) {
                    praises.push({id: $(cbx).data("id"), name: $(cbx).data("name")});
                }
            });
            if (checked) {
                $(".i-stu-info").addClass("dy-icon-checkboxhv");
            } else {
                $(".i-stu-info").removeClass("dy-icon-checkboxhv");
            }
        } else {
            if (checked) {
                praises.push({id: id, name: $(this).data("name")});
            } else {
                for (var i = 0; i < praises.length; i++) {
                    if (praises[i].id == id) {
                        praises.splice(i, 1);
                        break;
                    }
                }
            }
            if (praises.length < ($(".cbx-stu-info").length - 1)) {
                $("#cbxAllPower").get(0).checked = false;
                $("#iAllPower").removeClass("dy-icon-checkboxhv");
            } else {
                $("#cbxAllPower").get(0).checked = true;
                $("#iAllPower").addClass("dy-icon-checkboxhv");
            }
        }
        if (praises.length > 0) {
            $("#btnPraise").removeAttr("disabled");
            $("#btnSendScore").removeAttr("disabled");
        } else {
            $("#btnPraise").attr("disabled", "disabled");
            $("#btnSendScore").attr("disabled", "disabled");
        }
        $(".selected-number").text(praises.length);
    });
    //表扬按钮
    $("#btnPraise").bind("click", function () {
        if (!praises.length) {
            singer.msg("请勾选需要表扬的同学");
            return;
        }
        var defVal = "《" + $("#txtPaperTitle").val() + "》以下同学这次考试表现得不错，特别表扬！";
        var stuNames = "";
        if (praises.length > 0) stuNames += praises[0].name;
        if (praises.length > 1) stuNames += "、" + praises[1].name;
        if (praises.length > 2) stuNames += "、" + praises[2].name;
        if (praises.length > 3) stuNames += "...... 等 " + praises.length + "人";

        var $box = $("<div>");
        $box.append('<textarea style="width:450px;" id="txtPraisesValue" rows="4" maxlength="140">' + defVal + '</textarea>');
        $box.append('<div style="margin-bottom:8px;">' + stuNames + '</div>');
        S.dialog({
            title: "表扬",
            content: $box,
            fixed: true,
            backdropOpacity: .7,
            okValue: "确定",
            cancelValue: "取消",
            ok: function () {
                var text = $("#txtPraisesValue").val();
                if (!text || !text.length) {
                    singer.msg("说点鼓励话语吧~~");
                    return;
                }
                var names = "";
                for (var i = 0; i < praises.length; i++) {
                    if (i != 0) names += "、";
                    names += praises[i].name;
                }
                text += "<br/>" + names;
                $.post("statistics-praise", {
                    batch: batch,
                    message: $("<div/>").text(text).html()
                }, function (json) {
                    singer.msg(json.status ? "已发送表扬" : json.message);
                });
            },
            cancel: function () {
            }
        }).showModal();
    });
    //发送成绩通知短信
    $("#btnSendScore").bind("click", function () {
        if (!praises.length) {
            singer.msg("请勾选需要发送成绩通知短信的同学");
            return;
        }
        var ids = [];
        for (var i = 0; i < praises.length; i++) {
            ids.push(praises[i].id);
        }
        singer.confirm("确认为已选中的学生及家长发送短信成绩通知？", function () {
            $.post("statistics-sendsms", {
                batch: batch,
                paper_id: $("#txtPaperId").val(),
                student_ids: singer.json(ids)
            }, function (json) {
                if (json.status) {
                    singer.msg("已发送通知短信", 1000, function () {
                        window.location.reload();
                    });
                    return;
                }
                singer.msg(json.message);
            });
        })
    });
    $('.stats-list .dy-table tr')
        .find('th:not(:first)').addClass('f-tac')
        .end().find('td:not(:first)').addClass('f-tac');
//
    var sortParam = {
        widgets: ['zebra', 'columns'],
        headers: {}
    };
    if ($("#txtIsAb").val() == "1") {
        sortParam.headers = {
            0: {sorter: false},
            2: {sorter: false},
            4: {sorter: false},
            5: {sorter: false},
            6: {sorter: false},
            7: {sorter: false}
        };
    } else {
        sortParam.headers = {
            0: {sorter: false},
            2: {sorter: false},
            4: {sorter: false},
            5: {sorter: false}
        };
    }
    var $tab = $("#tabScore");
    var $ths = $tab.find("th");
    $tab.tablesorter(sortParam);

    var ShowHideTBTen = function (isHide) {
        if (isHide) {
            $tab.addClass("tb-ten-none");
            return;
        }
        var isAsc = $ths.eq(3).attr("aria-sort") != "ascending";
        if (isAsc)
            $tab.removeClass("tb-ten-none");
        else
            $tab.addClass("tb-ten-none");
    };
    $ths.eq(1).bind("click.tbt", function () {
        ShowHideTBTen(1);
    });
    $ths.eq(3).bind("click.tbt", function () {
        ShowHideTBTen(0);
    });

    $('.dy-table-body').bind('scroll', function (e) {
        var $t = $(this),
            left = $t.scrollLeft(),
            $head = $t.siblings('.dy-table-head').find('table');
        $head.css({'margin-left': -left});
        $t.find('.dy-table-left').css({
            left: left
        });
    });

    //每题得分
    var questionScores, $scoresController, $scores, bindScoreTables;
    $scoresController = $('#detailScores');
    $scores = $('.q-scores');
    /**
     * 加载每题得分
     */
    questionScores = function () {
        if ($scores.data('loaded'))
            return false;
        $scores.data('loaded', true);
        $.get('/work/teacher/question-scores', {
            batch: batch
        }, function (json) {
            if (!json.status || !json.data) {
                return false;
            }
            bindScoreTables(json.data);
            $scores.find('.dy-loading').remove();
            $scores.find('.dy-table-wrap').removeClass('hide');
        });
    };
    bindScoreTables = function (json) {
        var $caption = $scores.find('.dy-table-caption'),
            $header = $scores.find('.dy-table-head .dy-table'),
            $left = $scores.find('.dy-table-left .dy-table'),
            $body = $scores.find('.dy-table-body > .dy-table'),
            $section,
            attr, i,
            types = json.questionTypes,
            sorts = json.questionSorts,
            isAb = Object.getOwnPropertyNames(types).length > 1,
            count = Object.getOwnPropertyNames(sorts).length;
        //题型
        for (attr in types) {
            if (!types.hasOwnProperty(attr))
                continue;
            $section = '<div class="caption-section">';
            $section += (isAb ? (attr == 1 ? "A卷" : "B卷") : '') + '题目：(';
            var section = types[attr];
            for (var t in section) {
                if (!section.hasOwnProperty(t))
                    continue;
                $section += S.format('<span>{0}：{1}</span>', section[t], t);
            }
            $section += ')</div>';
            $caption.append($section);
        }
        $header.css({
            width: 120 + count * 70
        });
        $body.css({
            width: count * 70
        });
        //表格宽度
        var $headerCols = $header.find('colgroup'),
            $bodyCols = $body.find('colgroup');
        for (i = 0; i < count; i++) {
            $headerCols.append('<col style="width:70px"/>');
            $bodyCols.append('<col style="width:70px"/>');
        }
        //题目序号
        var $headerTr = $header.find('thead tr');
        for (attr in sorts) {
            if (!sorts.hasOwnProperty(attr))
                continue;
            $headerTr.append('<th>' + sorts[attr] + '</th>');
        }
        //姓名 & 分数
        var $leftBody = $left.find('tbody'),
            $bodyScores = $body.find('tbody');
        for (i = 0; i < json.students.length; i++) {
            var student = json.students[i];
            $leftBody.append('<tr><td>' + student.name + '</td></tr>');
            var $tr = $('<tr>');
            for (attr in sorts) {
                if (!sorts.hasOwnProperty(attr))
                    continue;
                var score = '-';
                if (student.scores.hasOwnProperty(attr))
                    score = student.scores[attr];
                $tr.append('<td>' + score + '</td>');
            }
            $bodyScores.append($tr);
        }
    };
    $scoresController.bind('click', function () {
        questionScores();
    });
    $('.dy-table-body').bind('scroll', function (e) {
        var $t = $(this),
            left = $t.scrollLeft(),
            $head = $t.siblings('.dy-table-head').find('table');
        $head.css({'margin-left': -left});
        $t.find('.dy-table-left').css({
            left: left
        });
    });
    $('.dy-tab-nav li').bind('click', function () {
        var $li = $(this);
        $li.addClass('z-crt').siblings('li').removeClass('z-crt');
    });
})(jQuery, SINGER);