/**
 * Created by epc on 2015/7/14.
 */
var marking = (function () {
    var M = {
        xiet: false, //是否协同阅卷
        set_def: true, //标识 完成阅卷是否自动补全正确答题的“勾”符号
        data: {},
        item: {},
        url: DEYI.sites.static + "/v1/image/icon/marking/", //图标路径
        isLoadArea: false, //是否已经初始化问题区域
        firstBindShare: false, //首次加载分享列表
        lock: false, //阻止切换答卷资料速度过快
        keydown: true, //是否启用键盘事件
        charCode: function (arr) {
            arr = arr || [];
            var result = "";
            for (var i = 0; i < arr.length; i++) {
                if (arr[i] > -1 && arr[i] < 26)
                    result += String.fromCharCode(65 + arr[i]);
            }
            return result;
        }, //byte[] 转换为字母字符串
        equals: function (a, b) {
            a = a || [];
            b = b || [];
            if (a.length != b.length) return false;
            for (var i = 0; i < a.length; i++) {
                if (!singer.inArray(a[i], b)) return false;
            }
            return true;
        }, //比较数组全等
        _mix: function (target, resource) {
            for (var name in resource) {
                if (resource.hasOwnProperty(name))
                    target[name] = resource[name];
            }
        }
    };
    return M;
})();
(function (M) {
    M._mix(M, {
        /**
         * 绘图
         */
        dw: {
            doc: $(document),
            icons: [], //已印图标坐标集合
            n_icons: [], //新增图标
            r_icons: [], //移除图标
            images: ["full.png", "semi.png", "error.png"], //系统图标
            curs: ["cur-0", "cur-1", "cur-2", "cur-clear", "cur-remark", "cur-bw-smile", "cur-bw-cry", "cur-bw-praise", "cur-bw-doubt"], //鼠标样式
            main: $(".m-box"), //试卷图片、批阅图标容器
            x: 0, //图片所在页面坐标X
            y: 0, //图片所在页面坐标Y
            w: 780, //图片宽度
            h: 0, //图片高度
            /**
             * 绑定当前学生各题答案的分享状态
             */
            bindShares: function () {
                if (!M.isLoadArea) return;
                if (!M.firstBindShare) M.firstBindShare = true;
                $(".q-area").each(function (n, area) {
                    var qid = $(area).data("id");
                    var exist = false;
                    if (M.item.student_id > 10) {
                        for (var i = 0; i < M.data.shares.length; i++) {
                            var share = M.data.shares[i];
                            if (share.question_id == qid && share.student_id == M.item.student_id) {
                                exist = true;
                                break;
                            }
                        }
                    }
                    var $share = $('<div>');
                    if (exist) {
                        $share.attr("class", "share-active").html('<i class="fa fa-star"></i><span>已晒</span>');
                    } else {
                        $share.attr("class", "item share").html('<i class="fa fa-star"></i><span>晒答案</span>');
                    }
                    $(area).append($share);
                });
            },
            /**
             * 初始化已批阅图标
             * @param a
             * @param b
             * @param c
             * @param d
             */
            init: function (a, b, c, d) {
                M.dw.x = a || M.dw.x;
                M.dw.y = b || M.dw.y;
                M.dw.w = c || M.dw.c;
                M.dw.h = d || M.dw.h;
                if (M.dw.icons.length) {
                    for (var i = 0; i < M.dw.icons.length; i++) {
                        var icon = M.dw.icons[i];
                        var $html;
                        if (icon.t == 4) {
                            $html = $("<div class='icon icon-lg' style='font-size:24px;line-height:1em;font-family: 宋体;font-weight:bold;color:#fa8989;left: " + icon.x + "px;top: " + icon.y + "px;'>" + icon.w + "</div>");
                        } else {
                            var src = M.url + icon.w;
                            if (icon.t == 5) {
                                src = M.url + "brow/" + icon.w;
                            } else if (icon.t < 3) {
                                src = M.url + M.dw.images[icon.t];
                            }
                            $html = $('<div class="icon ' + (icon.t == 3 ? "icon-lg" : "icon-sm") + '" style="left:' + icon.x + 'px;top:' + icon.y + 'px;">')
                                .html('<img src="' + src + '"/>');
                            if (icon.t == 1 || icon.t == 2) {
                                if (icon.id && icon.id.length) {
                                    $html.append('<div class="score">-' + (icon.w || 0) + '</div>');
                                    var iconId = "T" + icon.t + "_" + ((icon.x + "_" + icon.y).replace(/\./gi, 'D'));
                                    $html.attr("id", iconId)
                                        .data("qId", icon.id)
                                        .data("t", icon.t)
                                        .data("score", icon.w);
                                }
                            }
                        }
                        M.dw.main.append($html.data("x", icon.x).data("y", icon.y));
                    }
                }
            },
            /**
             * 印图标在页面上
             * @param ev
             * @param id
             * @param isFill
             * @param sort
             */
            print: function (ev, id, isFill, sort) {
                M.qt.semiBox.hide();
                var t = M.dw.main.data("t");
                if (t == "clear") return; //橡皮擦
                if (t < 0 || t > 5) t = 0;
                var w = (t > 2)
                    ? (M.dw.main.data("v") || "") // 批注图标
                    : M.dw.images[t]; // 正确、错误、半对 图标
                if (!w) return;
                var ev = ev || window.event;
                //鼠标坐标XY + 页面滚动距离 - 图片所在位置(相对图片定位)
                var x = parseInt(ev.clientX + M.dw.doc.scrollLeft() - M.dw.x);
                var y = parseInt(ev.clientY + M.dw.doc.scrollTop() - M.dw.y);
                //图片格式坐标差异
                if (t == 0 || t == 1) {
                    y -= 23;
                } else if (t == 2) {
                    x -= 3;
                    y -= 18;
                }
                //阻止同一坐标多次点击
                if (M.dw.icons.length > 0) {
                    for (var i = M.dw.icons.length; i > 0; i--) {
                        var item = M.dw.icons[i - 1];
                        if (item.x == x && item.y == y) return;
                    }
                }
                var $html, icon = {
                    x: x,
                    y: y,
                    t: t,
                    w: t > 2 ? w : 0
                };
                if (t == 4) {
                    $html = $("<div class='icon icon-lg' style='font-size:24px;line-height:1em;font-family: 宋体;font-weight:bold;color:#fa8989;left: " + x + "px;top: " + y + "px;'>").text(w);
                } else {
                    $html = $('<div class="icon ' + (t == 3 ? "icon-lg" : "icon-sm") + '" style="left:' + x + 'px;top:' + y + 'px;">')
                        .html('<img src="' + M.url + (t == 5 ? "brow/" + w : w) + '"/>');
                    if (t < 3) icon.id = id;
                    if (t == 1 || t == 2) {
                        $html.append('<div class="score">-0</div>');
                        if (!singer.isUndefined(id)) {
                            var iconId = "T" + t + "_" + ((x + "_" + y).replace(/\./gi, 'D'));
                            $html.attr("id", iconId)
                                .data("qId", id)
                                .data("t", t)
                                .data("score", 0);
                        }
                    }
                }
                $html.data("x", x).data("y", y);
                M.dw.main.append($html);
                M.dw.icons.push(icon);
                M.dw.n_icons.push(icon);
                if (!singer.isUndefined(id)) {
                    if (!singer.isUndefined(sort)) {
                        $("#questionList .item").removeClass("active");
                        $(".num-" + sort).addClass("active");
                        $(".scroll-items").mCustomScrollbar("scrollTo", ".num-" + sort);
                    }
                    if (t == 1) {
                        M.dw.openScoreBox(id, t, {x: x, y: y}, iconId);
                    } //半对 - 弹框改分
                    if (t == 2) {
                        //填空题
                        if (isFill) {
                            M.qt.setScore(id, 0, iconId, false);
                        } else {
                            M.dw.openScoreBox(id, t, {x: x, y: y}, iconId);
                        }
                    } //错误 - 设置本题分数为0
                }
            },
            /**
             * 清除图标
             * @param $icon
             */
            clear: function ($icon) {
                var _score = parseInt(($icon.data("score") || 0));
                if (_score > 0) {
                    M.qt.setScore($icon.data("qId"), _score, 0);
                }
                var x = $icon.data("x"), y = $icon.data("y");
                $icon.remove();

                //新标签不用加入到待擦出列表
                var isNewIcon = false;
                for(var j=0;j< M.dw.n_icons.length;j++){
                    var n_icon = M.dw.n_icons[j];
                    if(n_icon.x == x && n_icon.y == y){
                        isNewIcon = true;
                        M.dw.n_icons.splice(j,1);
                        break;
                    }
                }
                for (var i = 0; i < M.dw.icons.length; i++) {
                    var icon = M.dw.icons[i];
                    if (icon.x == x && icon.y == y) {
                        M.dw.icons.splice(i, 1);
                        if(!isNewIcon)
                            M.dw.r_icons.push({x:x,y:y});
                        break;
                    }
                }
            },
            /**
             * 显示打分板弹出框
             * @param id
             * @param t
             * @param XY
             * @param iconId
             */
            openScoreBox: function (id, t, XY, iconId) {
                var total = 0;
                var details = M.data.pictures[M.item.i - 1].details;
                for (var i = 0; i < details.length; i++) {
                    if (details[i].id == id) {
                        total = details[i].total;
                        break;
                    }
                }
                if (total == 0) {
                    M.qt.setScore(id, 0, 0, false);
                }
                else {
                    M.qt.semiBox.show(id, t, total, XY, iconId);
                }
            },
            /**
             * 打分板选择扣分值后更改icon.w
             * @param t
             * @param x
             * @param y
             * @param score
             */
            updateIcon: function (t, x, y, score) {
                for (var i = 0; i < M.dw.icons.length; i++) {
                    var icon = M.dw.icons[i];
                    if (icon.t == t && icon.x == x && icon.y == y) {
                        icon.w = score;
                        break;
                    }
                }
            },
            /**
             * 加载题目所在试卷区域
             */
            loadQuestionArea: function () {
                if (!M.isLoadArea) {
                    M.isLoadArea = true;
                    if (M.data.question_area && M.data.question_area.length) {
                        M.data.question_area = singer.json(M.data.question_area);
                        for (var i = 0; i < M.data.question_area.length; i++) {
                            var q = M.data.question_area[i];
                            var $area = $("<div class='q-area' style='position: absolute;width:" + q.width + "px;height:" + q.height + "px;left: " + q.x + "px;top: " + q.y + "px;' data-id='" + q.id + "' data-t='" + q.t + "' data-sort='" + q.index + "'>");
                            var $sort = $('<div class="item sort">');
                            $sort.text(q.index);
                            $area.append($sort);
                            M.dw.main.append($area);
                        }
                    }
                    if (!M.firstBindShare) {
                        M.firstBindShare = true;
                        M.dw.bindShares();
                    }
                }
            }
        },
        /**
         * 打分板
         */
        qt: {
            bind: function () {
                var hasObjQuestion = false, //是否存在客观题
                    hasError = false, //是有存在客观题错题
                    unScore = false, //是否存在题目未设置分数
                    questionList = $("#questionList"), //题目列表
                    objListTempStr = "", //客观题
                    notObjListTempStr = ""; //主观题
                questionList.html("");
                /**初始化数据*/
                var no = M.item.i - 1, un = 0; //非客观题数量
                var picture = M.data.pictures[no];
                if (picture.student_id < 10) {
                    $("#totalScore").text("0");
                    return;
                }
                var details = picture.details;
                M.item.total = 0; //总得分
                M.item.details = [];
                M.item.obj_count = 0;

                var objBoxTempStr = $("#objQuestionBox").html(),
                    objItemTempStr = $("#objQuestionItem").html(),
                    hasScoreTempStr = $("#hasScoreQuestionItem").html(),
                    noScoreTempStr = $("#noScoreQuestionItem").html();

                for (var q = 0; q < M.data.questions.length; q++) {
                    var $q = M.data.questions[q];
                    var $d = M.mt.detail(details, $q.id);
                    if (singer.isUndefined($d.id)) {
                        $("#totalScore").text("0");
                        singer.alert("没有找到当前学生的作答信息！");
                        return;
                    }
                    $d.total = $q.score;
                    //客观题
                    if ($q.objective) {
                        hasObjQuestion = true;
                        M.item.obj_count++;
                        if (singer.isUndefined(M.item.err_question))
                            M.item.err_question = [];
                        var dError = false;
                        if (q == M.data.questions.length - 1) {
                            objBoxTempStr.replace('<div class="qc-line"></div>', '');
                        }
                        //小问
                        if ($q.smalls && $q.smalls.length) {
                            for (var sq = 0; sq < $q.smalls.length; sq++) {
                                var $sq = $q.smalls[sq];
                                var $sd = M.mt.detail(details, $q.id, $sq.id);
                                if (singer.isUndefined($sd.id)) {
                                    $("#totalScore").text("0");
                                    singer.alert("没有找到当前学生的作答信息！");
                                    return;
                                }
                                if (!$sd.correct) {
                                    dError = true;
                                    hasError = true;
                                    $sd.score = 0;
                                    objListTempStr += objItemTempStr
                                        .replace("{id}", $d.id)
                                        .replace("{qNo}", $q.sort + "." + $sq.sort)
                                        .replace("{small_id}", $sd.small_id);
                                } else {
                                    M.item.total += $sq.score;
                                }
                            }
                        } else {
                            if (!$d.correct) {
                                dError = true;
                                hasError = true;
                                $d.score = 0;
                                objListTempStr += objItemTempStr
                                    .replace("{id}", $d.id)
                                    .replace("{qNo}", $q.sort)
                                    .replace("{small_id}", "");
                            } else {
                                M.item.total += $q.score;
                            }
                        }
                        if (dError) {
                            M.item.err_question.push({
                                i: $q.id,
                                s: $q.sort,
                                o: false
                            });
                        }
                    } else {
                        //主观题
                        un++;
                        var tmp = "";
                        if ($q.score <= 0) {
                            unScore = true;
                            tmp = noScoreTempStr
                                .replace("{id}", $d.id)
                                .replace("{qNo}", $q.sort)
                                .replace("{sort}", $q.sort)
                                .replace("{fullActive}", ($d.correct ? " active" : ""))
                                .replace("{errorActive}", (!$d.correct ? " active" : ""))
                            if (q == M.data.questions.length - 1) {
                                tmp = tmp.replace('<div class="qc-line"></div>', '');
                            }
                            notObjListTempStr += tmp;
                        } else {
                            var _li = "";
                            for (var k = 0; k <= $q.score; k++) {
                                _li += "<li" + (k == $q.score ? (" class='last-num-" + $q.sort + "'") : "") + ">" + k + "</li>";
                            }
                            tmp = hasScoreTempStr
                                .replace("{id}", $d.id)
                                .replace("{qNo}", $q.sort)
                                .replace("{sort}", $q.sort)
                                .replace("{cScore}", $d.score)
                                .replace("{liScores}", _li);
                            if (q == M.data.questions.length - 1) {
                                tmp = tmp.replace('<div class="qc-line"></div>', '').replace("{isLast}", " last");
                            } else {
                                tmp = tmp.replace("{isLast}", "");
                            }
                            notObjListTempStr += tmp;
                        }
                        M.item.total += $d.score;
                    }
                }
                if (M.item.student_id < 1) picture.details = details;
                //初始化页面
                $("#totalScore").text(M.item.total);
                if (hasObjQuestion) {
                    var objQeustionStr = "";
                    if (hasError) {
                        objQeustionStr = objBoxTempStr
                            .replace("{items}", objListTempStr)
                            .replace("cursor-clear", "")
                            .replace("{clear}", 0);
                    } else {
                        objQeustionStr = objBoxTempStr
                            .replace("{items}", "")
                            .replace("客观错题", "客观题全对")
                            .replace("{clear}", 1);
                    }
                    questionList.append(objQeustionStr);
                }
                questionList.append(notObjListTempStr);
            }, //初始化打分板
            init: function () {
                //客观题更改对错
                $("#questionList").delegate(".obj-item", "click", function () {
                    var id = $(this).data("id") || 0;
                    var smid = $(this).data("smid") || 0;
                    if (!id) return;
                    var full = $(this).data("full") || 0;
                    if (full) {
                        $(this).removeClass("active").find("i").attr("class", "fa fa-times");
                    } else {
                        $(this).addClass("active").find("i").attr("class", "fa fa-check");
                    }
                    $("#questionList .item").removeClass("active");
                    $(".obj-questions").parents(".item").addClass("active");

                    $(this).data("full", !full);
                    var details = M.data.pictures[M.item.i - 1].details;
                    var _find = false;
                    for (var i = 0; i < details.length; i++) {
                        var $d = $.extend(true, {}, details[i]);
                        if ($d.id == id && (!smid || $d.small_id == smid)) {
                            if (M.item.details.length) {
                                var tmp = M.mt.detail(M.item.details, id, smid);
                                if ((tmp.id || "") == id && (!smid || $d.small_id == smid)) {
                                    $d = tmp;
                                    _find = true;
                                }
                            }
                            $d.correct = !$d.correct;
                            $d.score = $d.correct ? $d.total : 0;
                            M.item.total += $d.correct ? $d.total : (0 - $d.total);
                            $("#totalScore").text(M.item.total);
                            M.item.o = M.item.o | 8;
                            M.qt.setErrQuestion($d.id, !full);
                            if (!_find)
                                M.item.details.push($d);
                            else
                                M.mt.remove(M.item.details, id, smid);
                            return;
                        }
                    }
                });
                //主观题更改分数
                $("#questionList").delegate(".qc-num", "click", function () {
                    if ($(this).data("clear")) return;
                    $("#questionList .item").removeClass("active");
                    $("#questionList .qc-scores").hide();
                    if ($(this).data("open")) {
                        $(this).data("open", 0);
                    } else {
                        var pItem = $(this).parents(".item");
                        pItem.addClass("active");
                        $(this).data("open", 1).siblings(".qc-scores").slideDown(200, function () {
                            if (pItem.hasClass("last"))
                                $(".scroll-items").mCustomScrollbar("scrollTo", "bottom");
                        });
                    }
                    $("#questionList .qc-num").not($(this)).data("open", 0);
                }).delegate(".qc-scores li", "click", function () {
                    var qItem = $(this).parents(".item"),
                        score = $(this).text();
                    var id = qItem.data("id");
                    qItem.find(".num").text(score);
                    $("#questionList .item").removeClass("active");
                    $(this).parents(".item").addClass("active");
                    M.qt.setScore(id, score);
                }).delegate(".no-score", "click", function () {
                    var v = $(this).data("v");
                    if (v != 1 && v != 0) return;
                    $("#questionList .item").removeClass("active");
                    $(this).parents(".item").addClass("active");
                    var id = $(this).parents(".item").data("id");
                    M.qt.setScore(id, 0, 0, v > 0);
                });
            }, //打分板操作
            setScore: function (id, score, iconId, isErr) {
                score = parseInt(score);
                var details = M.data.pictures[M.item.i - 1].details;
                for (var i = 0; i < details.length; i++) {
                    var $d = $.extend(true, {}, details[i]), _find = false;
                    if ($d.id == id) {
                        if (M.item.details.length) {
                            var tmp = M.mt.detail(M.item.details, id);
                            if ((tmp.id || "") == id) {
                                $d = tmp;
                                _find = true;
                            }
                        }
                        M.item.o = M.item.o | 8;
                        if ($d.total == 0) {
                            //无分数
                            $d.correct = isErr || false;
                            if (!_find)
                                M.item.details.push($d);
                            else if (details[i].correct == $d.correct)
                                M.mt.remove(M.item.details, $d.id);
                            $("#questionList").find(".item").each(function (i, o) {
                                if ($(o).data("id") == id) {
                                    var noScore = $(o).find(".no-score");
                                    $(noScore).removeClass("active");
                                    $(noScore).each(function (j, k) {
                                        if ($d.correct && $(k).data("v")) {
                                            $(k).addClass("active");
                                        } else if (!$d.correct && !$(k).data("v")) {
                                            $(k).addClass("active");
                                        }
                                    });
                                }
                            });
                        } else {
                            if (!singer.isUndefined(isErr)) score = 0 - $d.total;
                            if (!singer.isUndefined(iconId)) {
                                if (iconId != 0) {
                                    var $icon = $("#" + iconId);
                                    var tmp = 0 - score;
                                    if (($d.score + score) < 0) tmp = $d.score;
                                    $icon.data("score", tmp);
                                    $icon.find("div").text("-" + tmp).show();
                                    M.dw.updateIcon(
                                        $icon.data("t"),
                                        $icon.data("x"),
                                        $icon.data("y"),
                                        tmp
                                    );
                                }
                                score += $d.score;
                            }
                            if (score < 0) score = 0;
                            if (score > $d.total) score = $d.total;

                            $d.correct = score == $d.total;
                            M.item.total += score - $d.score;
                            $d.score = score;
                            $("#totalScore").text(M.item.total);
                            if (!_find) M.item.details.push($d);
                            else if (details[i].correct == $d.correct && details[i].score == $d.score) {
                                M.mt.remove(M.item.details, $d.id);
                            }
                            $("#questionList").find(".item").each(function (i, o) {
                                if ($(o).data("id") == id) {
                                    $(o).find(".num").text(score);
                                }
                            });
                        }
                        break;
                    }
                }
            }, //设置主观题分数、对错
            setErrQuestion: function (id, o) {
                if (M.item.err_question && M.item.err_question.length) {
                    for (var i = 0; i < M.item.err_question.length; i++) {
                        var eq = M.item.err_question[i];
                        if (eq.i == id) eq.o = o;
                    }
                }
            }, //客观题错题题号
            semiBox: {
                timerId: 0,
                show: function (id, t, total, XY, iconId) {
                    var semiBox = $("#semiBox");
                    total = parseInt((total || 0));
                    var semiH = 122; //用于计算扣分版坐标
                    if (id && total > 0) {
                        semiBox.html("");
                        var ul = $('<ul>');
                        var i = 1;
                        var tmpScore = total;
                        if (total > 4) {
                            i = 0;
                            var halfS = 0 - Math.ceil(total / 2);
                        } //小于4分就不要扣一半了嘛
                        if (total > 20) tmpScore = 20; //最多展示20分
                        if (tmpScore > 9) {
                            semiH = Math.ceil(tmpScore / 3) * 30 + 32;
                        }
                        for (; i <= tmpScore; i++) {
                            var tmpS = (0 - i);
                            if (i == 0) {
                                if (t == 1) tmpS = halfS;
                                else if (t == 2) tmpS = (0 - total);
                            }
                            var _item = '<li data-v="' + tmpS + '"' + (i == 0 ? 'class="one"' : '') + '>'
                                + (i == 0 ? (t == 2 ? "全扣" : "扣一半") : tmpS)
                                + '</li>';
                            var $item = $(_item).click(function () {
                                M.qt.semiBox.hide();
                                M.qt.setScore(id, parseInt($(this).data("v")), iconId);
                            });
                            ul.append($item);
                        }

                        var $apItem = $('<li data-v="cancel" title="取消"><i class="fa fa-reply"></i></li>').click(function () {
                            M.qt.semiBox.hide();
                            M.dw.clear($("#" + iconId));
                        });
                        ul.append($apItem);
                        semiBox.append(ul);
                    }
                    if (!singer.isUndefined(XY)) {
                        if ((M.dw.w - XY.x) < 180) XY.x -= 172;
                        if ((M.dw.h - XY.y) < semiH) XY.y -= (semiH - 50);
                        semiBox.data("xy", XY.x + "-" + XY.y + "-" + semiH).attr("style", "top:" + XY.y + "px;left:" + (XY.x + 50) + "px;");
                    }
                    semiBox.show();
                    M.qt.semiBox.timerId = window.setTimeout(function () {
                        M.qt.semiBox.hide(200);
                    }, 5000);
                },
                hide: function (t) {
                    window.clearTimeout(M.qt.semiBox.timerId);
                    t = t || 0;
                    if (t > 0) $("#semiBox").html("").fadeOut(t);
                    else $("#semiBox").html("").hide(t);
                }
            } //半对图标触发扣分打分板
        },
        /**
         * 页面绑定
         */
        mk: {
            init: function () {
                $(".p-no .fs-total").text(M.data.pictures.length); //总提交人数
                $("#semiBox").mouseover(function () {
                    window.clearTimeout(M.qt.semiBox.timerId);
                }).mouseout(function () {
                    M.qt.semiBox.timerId = window.setTimeout(function () {
                        M.qt.semiBox.hide(200);
                    }, 5000);
                }); //图标打分板鼠标离开控制显隐
                $(".m-back").bind("click", function () {
                    if (M.item.o > 0) {
                        M.mt.submit(-1, M.mk.back);
                    } else {
                        window.location.href = "/work";
                    }
                }); //返回列表
                var students_li = ""; //初始化试卷图片对应学生名称列表
                for (var i = 0; i < M.data.pictures.length; i++) {
                    M.data.pictures[i].i = i + 1;
                    var c = "", item = M.data.pictures[i];
                    if (i == 0)
                        c = ' class="active"'; //高亮当前
                    if (item.student_id < 1) {
                        c = ' class="fail"';
                        item.student_name = '未识别';
                    }
                    students_li += "<li" + c + " data-idx='" + (i + 1) + "' data-id='" + (item.id) + "'>" + item.student_name + "</li>";
                }
                $("#studentList").html(students_li).delegate("li", "click", function () {
                    if (!M.lock || $(this).data("id") == M.item.id) return;
                    for (var j = 0; j < M.data.pictures.length; j++) {
                        if (M.data.pictures[j].id == $(this).data("id")) {
                            M.lock = false;
                            M.mt.submit(j);
                            $(".m-progress").find(".students").fadeOut(200);
                            break;
                        }
                    }
                }); //学生名称列表点击事件（切换答卷资料）
                //未提交的学生名单
                if (M.data.un_submits && M.data.un_submits.length) {
                    var unSbox = $(".un-submits");
                    for (var j = 0; j < M.data.un_submits.length; j++) {
                        unSbox.append("<span>" + M.data.un_submits[j].name + "</span>");
                    }
                    unSbox.show();
                }
                $(".m-change").click(function () {
                    if (M.data.pictures.length < 2) return;
                    if (!M.lock) return;
                    M.lock = false;
                    var idx = $(this).data("v") + M.item.i;
                    if (idx < 1) idx = M.data.pictures.length;
                    if (idx > M.data.pictures.length) idx = 1;
                    M.mt.submit(idx - 1);
                }); //上、下一张试卷图片（切换答卷资料）
                $(document).keydown(function (e) {
                    if (!M.keydown || !M.lock) return;
                    var code = e.which;
                    if (code == 37 || code == 39) {
                        if (M.data.pictures.length < 2) return;
                        M.lock = false;
                        var idx = M.item.i + (code == 37 ? -1 : 1);
                        if (idx < 1) idx = M.data.pictures.length;
                        if (idx > M.data.pictures.length) idx = 1;
                        M.mt.submit(idx - 1);
                    }
                }); //键盘控制上、下一张试卷图片（切换答卷资料）
                $(".m-tools").find(".tool").bind("click", function () {
                    $("#dt_marks").hide();
                    M.qt.semiBox.hide();
                    $(".m-tools").find(".tool").removeClass("active");
                    $(this).addClass("active");
                    var t = $(this).data("t");
                    if (t == "remark") {
                        if ($(".remarks").data("open")) {
                            $(".remarks").data("open", 0).hide();
                            M.keydown = true;
                        } else {
                            $(".remarks").data("open", 1).fadeIn(200);
                            M.keydown = false;
                        }
                        return;
                    }
                    else {
                        $(".remarks").data("open", 0).hide();
                        M.keydown = true;
                    }
                    if (M.dw.main.data("t") == t) return;
                    M.dw.main.data("o", 2);
                    M.mk.setBox(t);
                }); //工具栏（批阅图标符号）切换
                M.dw.main.bind("contextmenu", function () {
                    return false;
                }).mousedown(function (e) {
                    if (e.which == 3) {
                        $(".m-tools").find(".tool").removeClass("active");
                        var t = M.dw.main.data("t");
                        if (t == 0) {
                            t = 1;
                            $(".tool-semi").addClass("active");
                        } else if (t == 1) {
                            t = 2;
                            $(".tool-error").addClass("active");
                        } else {
                            t = 0;
                            $(".tool-full").addClass("active");
                        }
                        M.keydown = true;
                        $(".remarks").data("open", 0).hide();
                        M.qt.semiBox.hide();
                        M.dw.main.data("o", 2);
                        M.mk.setBox(t);
                    }
                }); //鼠标右键切换工具栏（批阅图标符号）
                $(".remarks li").bind("click", function () {
                    var t = $(this).parent().data("t");
                    var v = $(this).data("v");
                    $(".remarks").data("open", 0).hide();
                    M.keydown = true;
                    if (v == M.dw.main.data("v")) return;
                    M.dw.main.data("o", 4);
                    M.mk.setBox(t, v);
                }); //批注图标符号切换
                $("#txtCustom").bind("change", function () {
                    var w = $("#txtCustom").val();
                    if (w && w.length) {
                        $("#txtCustom").val(w
                            .replace(/['"<>]/gi, '')
                            .replace('_', '')
                            .replace(/(\r)|(\n)/g, " "));
                    }
                }); //自定义文字过滤
                $("#btnUsageCustom").bind("click", function () {
                    var v = $("#txtCustom").val()
                        .replace(/['"<>]/gi, '')
                        .replace('_', '')
                        .replace(/(\r)|(\n)/g, " ");
                    if (!(v && v.length)) {
                        $(".custom-history").removeClass("hide");
                        $(".custom-edit").addClass("hide");
                        return;
                    }
                    ;
                    $(".remarks").data("open", 0).hide();
                    M.keydown = true;
                    $(".custom-history").removeClass("hide");
                    $(".custom-edit").addClass("hide");
                    if (v == M.dw.main.data("v")) return;
                    M.dw.main.data("o", 4);
                    M.mk.setBox(4, v);
                    var historys = $(".custom-history .list");
                    var items = historys.find(".item");
                    historys.html('<div class="ht item" title="点击使用">' + v + '</div>');
                    if (items && items.length) {
                        $(items).each(function (i, o) {
                            if (i < 4) {
                                historys.append($(o));
                            }
                        });
                    }
                }); //自定义文字
                $(".custom-history").delegate(".item", "click", function () {
                    var v = $(this).text();
                    if (!(v && v.length)) {
                        singer.msg("说点什么吧...");
                        return;
                    }
                    $(".remarks").data("open", 0).hide();
                    M.keydown = true;
                    if (v == M.dw.main.data("v")) return;
                    M.dw.main.data("o", 4);
                    M.mk.setBox(4, v);
                }); //自定义文字历史点击使用
                M.dw.main.click(function (ev) {
                    var t = M.dw.main.data("t");
                    if (t != 3 && t != 4 && t != 5) return;
                    if (M.item.student_id < 10) return;
                    M.dw.print(ev);
                    M.item.o = M.item.o | (M.dw.main.data("o") || 2);
                }); //只能印批注图标
                M.dw.main.delegate(".q-area", "click", function (ev) {
                    if (M.item.student_id < 10) {
                        singer.msg("学生未识别，请先绑定学生后再批阅");
                        return;
                    }
                    M.dw.print(ev, $(this).data("id"), $(this).data("t"), $(this).data("sort"));
                    M.item.o = M.item.o | (M.dw.main.data("o") || 2);
                }); //印图标
                M.dw.main.delegate(".icon", "click", function () {
                    var t = M.dw.main.data("t");
                    if (t != "clear") return;
                    if (M.item.student_id < 10) {
                        singer.msg("学生未识别，请先绑定学生后再批阅");
                        return;
                    }
                    M.dw.clear($(this));
                    M.item.o = M.item.o | 6; //PS：不知道清除的是批阅图标还是批注图标 so 都提交
                }); //清除图标
                M.dw.main.delegate(".sort", "click", function (ev) {
                    ev.stopPropagation();
                    return;
                }); //题目区域中的序号不能点击
                M.dw.main.delegate(".share-active", "click", function (ev) {
                    ev.stopPropagation();
                    return;
                }); //已分享的答案不能点击
                M.dw.main.delegate(".share", "click", function (ev) {
                    ev.stopPropagation();
                    if (M.item.student_id < 10) {
                        singer.msg("学生未识别，请先绑定学生后再分享");
                        return;
                    }
                    var $share = $(this);
                    var qid = $share.parent().data("id");
                    singer.confirm("请确认您分享的是本题的正确答案", function () {
                        $.post("addShare", {
                            batch: M.data.batch,
                            paperId: M.data.paper_id,
                            classId: M.data.class_id,
                            questionId: qid,
                            studentId: M.item.student_id,
                            studentName: M.item.student_name
                        }, function (json) {
                            if (json.status) {
                                if (!M.data.shares) M.data.shares = [];
                                M.data.shares.push({id: json.data, question_id: qid, student_id: M.item.student_id});
                                $share.removeClass("item").removeClass("share").addClass("share-active")
                                    .html('<i class="fa fa-star"></i><span>已晒</span>');
                                singer.msg("分享成功");
                            } else {
                                singer.msg(json.message);
                            }
                        });
                    });
                }); //分享学生答案
                $("#markingImage").load(function () {
                    M.dw.init(
                        $(this).offset().left,
                        $(this).offset().top,
                        $(this).width(),
                        $(this).height()
                    );
                    M.dw.loadQuestionArea();
                }); //试卷图片加载完成 - 初始化已批阅图标
                $(".finished").click(function () {
                    M.set_def = true;
                    var $fcontent = $('<div style="line-height:2em;">完成<span style="color:#fa9632;font-size:16px;">所有</span>批阅后，将生成统计报表，<span style="color:#fa9632;font-size:16px;">不可</span>再更改。<br/>确认完成吗？<br/></div>');
                    var $fcbx = $('<div class="finish-box"><div class="f01"><i class="fa fa-check"></i></div><div> &nbsp;为正确题目自动标记 “ √ ”</div></div>');
                    $fcbx.bind("click", function () {
                        if ($(this).data("ns")) {
                            $(this).data("ns", false);
                            $(this).find(".f01").attr("style", "");
                            M.set_def = true;
                        } else {
                            $(this).data("ns", true);
                            $(this).find(".f01").attr("style", "color:#fff");
                            M.set_def = false;
                        }
                    });
                    $fcontent.append($fcbx);
                    singer.dialog({
                        title: "完成批阅",
                        content: $fcontent,
                        fixed: true,
                        //quickClose: true,
                        backdropOpacity: .7,
                        okValue: "确定",
                        cancelValue: "取消",
                        ok: function () {
                            if (M.item.o > 0) {
                                M.mt.submit(-1, M.mt.finished);
                            } else {
                                M.mt.finished();
                            }
                        },
                        cancel: function () {
                        }
                    }).showModal();
                }); //完成阅卷
                $(".btn-bind").click(function () {
                    if (M.item.student_id < 1) {
                        $.post("unbind", {
                            id: M.item.id,
                            batch: M.data.batch,
                            classId: M.data.class_id
                        }, function (json) {
                            if (!json.status) {
                                singer.msg("加载未绑定学生资料失败");
                                return;
                            }
                            var $div = $('<div id="unBindParentBox" data-id="' + M.data.class_id + '" style="min-width: 300px;min-height: 80px;max-width: 600px;max-height: 250px;"></div>');
                            $div.bind("click", function (e) {
                                e.stopPropagation();
                                return false;
                            });
                            $div.append(M.mk.unBindStudents(json.data));
                            var _title = "绑定" + M.data.class_name + "的学生"
                            singer.dialog({
                                title: _title,
                                content: $div,
                                fixed: true,
                                //quickClose: true,
                                backdropOpacity: .7,
                                okValue: "确定",
                                cancelValue: "取消",
                                ok: function () {
                                    if (M.item.student_id < 1) {
                                        singer.msg("请选择学生");
                                        return false;
                                    }
                                    $.post("bind",
                                        {
                                            id: M.item.id,
                                            studentId: M.item.student_id,
                                            studentName: M.item.student_name,
                                            batch: M.data.batch
                                        },
                                        function (json) {
                                            if (json.status) {
                                                singer.msg("绑定成功", 500, function () {
                                                    var url = "/work/marking-online?batch=" + M.data.batch;
                                                    if (singer.uri().type) url += ("&type=" + singer.uri().type);
                                                    window.location.href = url;
                                                });
                                                return;
                                            } else {
                                                singer.msg(json.message);
                                            }
                                        });
                                },
                                cancel: function () {
                                    M.item.student_id = 0;
                                    M.item.student_name = "未绑定";
                                }
                            }).showModal();
                        });
                    }
                }); //绑定学生
                $(".cancel-bind").click(function () {
                    singer.dialog({
                        title: "解除绑定",
                        content: '解除绑定后将<span style="color:#fa9632;font-size:16px;">失去</span>当前答卷的批阅记录。<br/>确认解除吗？',
                        fixed: true,
                        backdropOpacity: .7,
                        okValue: "确定",
                        cancelValue: "取消",
                        ok: function () {
                            $.post("cancel-bind", {id: M.item.id}, function (json) {
                                if (json.status) {
                                    singer.msg("操作成功", 500, function () {
                                        var url = "/work/marking-online?batch=" + M.data.batch;
                                        if (singer.uri().type) url += ("&type=" + singer.uri().type);
                                        window.location.href = url;
                                    });
                                } else {
                                    singer.msg(json.Message);
                                }
                            });
                        },
                        cancel: function () {
                        }
                    }).showModal();
                }); //解除绑定
            },
            bind: function () {
                if (M.item.student_id > 0) {
                    $(".questions").show();
                    $(".loco").find(".binded").show();
                    $(".loco").find(".unbind").hide();
                    $("#studentName").text(M.item.student_name);
                } else {
                    $(".questions").hide();
                    $(".loco").find(".binded").hide();
                    $(".loco").find(".unbind").show();
                }
                $(".p-no .fs-big").text(M.item.i);
                $(".m-progress li").removeClass("active").each(function (i, o) {
                    if ($(o).data("id") == M.item.id) {
                        $(o).addClass("active");
                        return;
                    }
                }); //高亮当前试卷学生名称
            }, //绑定动态变化的数据
            /**
             * 批阅图标切换绑定
             * @param t 图标类型：0正确、1半对、2错误、3批注、4自定义、5表情
             * @param v 批注图标地址
             */
            setBox: function (t, v) {
                M.dw.main.data("v", 0);
                switch (t) {
                    case 0:
                        M.dw.main.data("t", t).removeClass(M.dw.curs.join(" ")).addClass("cur-0");
                        return;
                    case 1:
                        M.dw.main.data("t", t).removeClass(M.dw.curs.join(" ")).addClass("cur-1");
                        return;
                    case 2:
                        M.dw.main.data("t", t).removeClass(M.dw.curs.join(" ")).addClass("cur-2");
                        return;
                    case "clear":
                        M.dw.main.data("t", t).removeClass(M.dw.curs.join(" ")).addClass("cur-clear");
                        return;
                    case 3:
                    case 4:
                        M.dw.main.data("t", t).data("v", v).removeClass(M.dw.curs.join(" ")).addClass("cur-remark");
                        return;
                    case 5:
                        M.dw.main.data("t", t).data("v", v).removeClass(M.dw.curs.join(" "));
                        switch (v) {
                            case "smile.png":
                                M.dw.main.addClass("cur-bw-smile");
                                return;
                            case "cry.png":
                                M.dw.main.addClass("cur-bw-cry");
                                return;
                            case "praise.png":
                                M.dw.main.addClass("cur-bw-praise");
                                return;
                            case "doubt.png":
                                M.dw.main.addClass("cur-bw-doubt");
                                return;
                        }
                        return;
                }
            },
            /**
             * 当前班级未绑定的学生列表 返回 html - ul
             * @param data
             * @returns {*|jQuery|HTMLElement}
             */
            unBindStudents: function (data) {
                var $ul = $('<ul class="ul-bind-students after"></ul>');
                for (var i = 0; i < data.length; i++) {
                    var stu = data[i];
                    var $li = $('<li data-id="' + stu.id + '" data-name="' + stu.name + '">' + stu.name + '</li>');
                    $li.bind("click", function (e) {
                        e.stopPropagation();
                        M.item.student_id = $(this).data("id");
                        M.item.student_name = $(this).data("name");
                        $(".ul-bind-students li").removeClass("active");
                        $(this).addClass("active");
                        return false;
                    });
                    $ul.append($li);
                }
                return $ul;
            },
            /**
             * 返回列表
             */
            back: function () {
                window.location.href = "/work";
            }
        },
        /**
         * 交互
         */
        mt: {
            /**
             * 在线阅卷图片列表
             * @param b
             * @param t
             */
            list: function (b, t) {
                $.post("pictures", {batch: b, type: t}, function (json) {
                    if (!json.status) {
                        $(".j-autoHeight").attr("style", "height:" + $(window).height() + "px;");
                        $(".m-message").text(json.message).show();
                        $(".m-body").hide();
                        return;
                    }
                    M.data = json.data;
                    var no = 0, id = M.data.last_student_id;
                    var loadings = []; //需要初始化
                    for (var i = 0; i < M.data.pictures.length; i++) {
                        M.data.pictures[i].i = i;

                        if (!M.data.pictures[i].is_marking && M.data.pictures[i].student_id > 0) {
                            loadings.push({
                                id: M.data.pictures[i].id,
                                name: M.data.pictures[i].name
                            });
                        }

                        if (no == 0 && id > 1 && M.data.pictures[i].student_id == id) {
                            no = i;
                        } //上次产生批阅行为的试卷ID
                    }

                    if (loadings.length) {
                        M.mt.loadingFirst(loadings, function () {
                            M.mt.item(M.data.pictures[no], no + 1);
                            M.mk.init(); // 初始化页面事件、绑定基本数据
                            M.qt.init(); // 初始化打分板事件
                        });
                    } else {
                        M.mt.item(M.data.pictures[no], no + 1);
                        M.mk.init(); // 初始化页面事件、绑定基本数据
                        M.qt.init(); // 初始化打分板事件
                    }
                });
            },
            /**
             * 当前阅卷图片详细资料
             * @param item
             * @param i
             */
            item: function (item, i) {
                item.load = item.load || false; //缓存标识
                if (!item.load) {
                    $.post("picture", {id: item.id, type: M.data.paper_type}, function (json) {
                        if (!json.status) {
                            singer.msg(json.message);
                            return;
                        }
                        item.answers = singer.json(json.data.answers) || [];
                        item.icons = singer.json(json.data.icons) || [];
                        item.marks = singer.json(json.data.marks) || [];
                        item.details = json.data.details || [];
                        if (item.marks && item.marks.length) {
                            for (var j = 0; j < item.marks.length; j++) {
                                item.icons.push({
                                    x: item.marks[j].x,
                                    y: item.marks[j].y,
                                    t: item.marks[j].t,
                                    w: item.marks[j].w
                                });
                            }
                        }
                        item.load = true;
                        M.mt.item_set(item, i);
                    });
                } else {
                    M.mt.item_set(item, i);
                }
            },
            item_set: function (item, i) {
                M.dw.icons = []; //重置
                M.dw.n_icons = [];
                M.dw.r_icons = [];
                $(".icon").remove();
                $(".share").remove();
                $(".share-active").remove();

                var id = "";
                if (!singer.isUndefined(M.item.id)) {
                    id = M.item.id;
                }
                M.item = $.extend(true, {}, item); //引用类型
                M.dw.bindShares(); //绑定答案分享状态
                M.item.o = 0; //监视是否发生改动
                if (i) M.item.i = i; //序号
                if (M.item.icons && M.item.icons.length) M.dw.icons = M.item.icons; //已有图标
                if (id == item.id) M.dw.init();
                else $("#markingImage").attr("src", M.item.pic); //当前批阅试卷图片
                if (M.data.pictures.length > 2) {
                    var L = M.item.i - 2;
                    var R = M.item.i;
                    if (L < 0) L = M.data.pictures.length - 1;
                    if (R >= M.data.pictures.length) R = 0;
                    $("#delayImageL").attr("src", M.data.pictures[L].pic);
                    $("#delayImageR").attr("src", M.data.pictures[R].pic);
                } //延迟加载图片
                M.mk.bind(); // 动态变化的数据绑定
                M.qt.bind(); // 初始化打分板
                M.lock = true;
            },
            /**
             * 根据ID返回数组中的对象
             * @param arr
             * @param id
             * @param small_id
             * @returns {*}
             */
            detail: function (arr, id, small_id) {
                if (arr.length) {
                    for (var i = 0; i < arr.length; i++) {
                        if (arr[i].id == id && (!small_id || arr[i].small_id == small_id))
                            return arr[i];
                    }
                }
                return {};
            },
            /**
             * 根据ID移除数组中的对象
             * @param arr
             * @param id
             * @param smid
             */
            remove: function (arr, id, smid) {
                for (var i = 0; i < arr.length; i++) {
                    if (arr[i].id == id && (!smid || arr[i].small_id == smid)) {
                        arr.splice(i, 1);
                        return;
                    }
                }
            },
            /**
             * 提交数据
             * @param no
             * @param func
             */
            submit: function (no, func) {
                $("#dt_marks").data("open", 0).hide();
                M.qt.semiBox.hide();
                if (singer.isUndefined(no)) no = -1;
                // 未识别不能提交
                if (M.item.student_id < 1) {
                    if (no < 0) {
                        singer.msg("请先绑定学生");
                    } else if (M.item.o != 0) {
                        singer.dialog({
                            title: "请绑定试卷",
                            content: '当前试卷未绑定学生，切换后批阅资料将<span style="color:#fa9632;font-size:16px;">丢失</span>。<br/>是否继续?',
                            fixed: true,
                            backdropOpacity: .7,
                            okValue: "确定",
                            cancelValue: "取消",
                            ok: function () {
                                M.mt.item(M.data.pictures[no]);
                            },
                            cancel: function () {
                            }
                        }).showModal();
                    } else {
                        M.mt.item(M.data.pictures[no]);
                    }
                    return;
                }
                //验证是否需要提交数据
                if (M.item.o != 0) {
                    var _icons = [],
                        _marks = [],
                        _details = [],
                        _eq = "not";
                    if (M.dw.n_icons && M.dw.n_icons.length && ((M.item.o & 4) > 0 || (M.item.o & 2) > 0)) {
                        for (var j = 0; j < M.dw.n_icons.length; j++) {
                            var icon = {
                                x: M.dw.n_icons[j].x,
                                y: M.dw.n_icons[j].y,
                                t: M.dw.n_icons[j].t,
                                w: M.dw.n_icons[j].w
                            };
                            if (M.dw.n_icons[j].t == 0 || M.dw.n_icons[j].t == 1 || M.dw.n_icons[j].t == 2) {
                                icon.id = M.dw.n_icons[j].id;
                                _icons.push(icon);
                            }
                            else {
                                _marks.push(icon);
                            }
                        }
                        if ((M.item.o & 2) == 0) _icons = [];
                        if ((M.item.o & 4) == 0) _marks = [];
                    }
                    if ((M.item.o & 8) > 0) {
                        for (var i = 0; i < M.item.details.length; i++) {
                            var detail = M.item.details[i];
                            _details.push({
                                question_id: detail.id,
                                small_id: detail.small_id,
                                current_score: detail.score,
                                correct: detail.correct
                            });
                        }
                    }
                    if (M.item.obj_count) {
                        var tmp_eq = [];
                        $(M.item.err_question).each(function (i, tmp_q) {
                            if (!tmp_q.o) tmp_eq.push(tmp_q.s);
                        });
                        _eq = !tmp_eq.length
                            ? "全对"
                            : (tmp_eq.length == M.item.obj_count ? "全错" : tmp_eq.join(','));
                    } // 客观题错题题号
                    $.post("submit",
                        {
                            id: M.item.id,
                            o: M.item.o,
                            type: M.data.paper_type,
                            eq: _eq,
                            icons: singer.json(_icons).replace(/\'/gi, '"'),
                            marks: singer.json(_marks).replace(/\'/gi, '"'),
                            remove_icons: singer.json(M.dw.r_icons).replace(/\'/gi,'"'),
                            details: singer.json(_details).replace(/\'/gi, '"')
                        },
                        function (json) {
                            if (json.status) {
                                var _no = M.item.i - 1;
                                M.data.pictures[_no].icons = $.extend([], M.dw.icons);
                                M.data.pictures[_no].details = [];
                                M.data.pictures[_no].load = false;
                                if (M.item.o != 0) singer.msg("已保存", 1000);
                                M.item.o = 0;
                                if (func) {
                                    func();
                                }
                                M.mt.item(M.data.pictures[(no > -1 ? no : _no)]);
                            } else
                                singer.msg(json.message);
                        });
                } else {
                    M.lock = true;
                    if (no > -1) M.mt.item(M.data.pictures[no]);
                }
            },
            /**
             * 完成阅卷
             */
            finished: function () {
                $.post("finished",
                    {
                        batch: M.data.batch,
                        paperId: M.data.paper_id,
                        type: M.data.paper_type,
                        def: M.set_def
                    }, function (json) {
                        if (json.Status) {
                            singer.msg("操作成功", 0, function () {
                                window.location.href = "/work";
                            });
                        } else {
                            singer.msg(json.Message);
                        }
                    });
            },
            /**
             * 首次阅卷初始化
             */
            loadingFirst: function (list, callback) {
                var $progBar = $('<div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="45" aria-valuemin="0" aria-valuemax="100" style="min-width:2em;width: 0%">0%</div>');
                var $prog = $('<div class="progress"></div>');
                $prog.append($progBar);
                var $content = $('<div style="width:500px;">正在初始化...<br/><br/></div>');
                $content.append($prog);

                var dialog = singer.dialog({
                    content: $content,
                    fixed: true,
                    backdropOpacity: .7
                });
                dialog.showModal();
                M.mt.setProgressBar(0, list, $progBar, function () {
                    dialog.close();
                    if (callback) {
                        callback();
                    }
                });

            },
            /**
             * 更新进度条
             */
            setProgressBar: function (i, list, bar, callback) {
                if (i >= list.length) {
                    if (callback) {
                        callback();
                    }
                    return;
                }
                $.post("commit", {id: list[i].id}, function () {
                    var val = Math.round((i + 1) / list.length * 100);
                    bar.attr("style", "min-width:2em;width:" + val + "%;").text(val + "%");

                    if (++i < list.length) {
                        M.mt.setProgressBar(i, list, bar, callback);
                    } else {
                        setTimeout(function () {
                            M.mt.setProgressBar(i, list, bar, callback);
                        }, 2000);
                    }
                });
            }
        }
    });
})(marking);

$(function ($) {
    var batch = singer.uri().batch, type = singer.uri().type;
    if (singer.isUndefined(batch) || batch == "") {
        $(".m-message").text("参数错误").show();
        $(".m-body").hide();
        return;
    }

    //...
    if ($("#txtXieT").val() == "1") {
        marking.xiet = true;
    }

    //浏览器宽度
    var winW = $(window).width(),
        winH = $(window).height();
    if (!winW || winW < 1280) winW = 1280;
    var boxX = (winW - 1000) / 2 + 175 - 10,
        boxY = 40;
    if (singer.cookie.get("MARKINGALT") == "1") {
        $(".m-prompt").remove();
        boxY = 10;
    } else {
        $(".m-prompt .p-close").bind("click", function () {
            singer.cookie.set("MARKINGALT", "1", (60 * 24 * 365));
            $(".m-prompt").remove();
            $(".m-goal").attr("style", "left:" + $(".m-goal").data("x") + "px;top:10px;"); //得分版坐标
            $(".m-progress").attr("style", "left:" + $(".m-progress").data("x") + "px;top:10px;"); //进度坐标
        });
        $(".m-prompt").show();
    } //小提示开关
    $(".m-body").attr("style", "min-height:" + (winH - 50) + "px;margin-left:" + boxX + "px;"); //批阅图片坐标
    $(".m-tools").attr("style", "top:" + (winH / 2 - 125) + "px;left:" + (boxX + 781) + "px;"); //工具栏坐标
    $(".m-goal").data("x", (boxX - 175)).data("y", boxY).attr("style", "left:" + (boxX - 175) + "px;top:" + boxY + "px;"); //得分版坐标
    $(".scroll-items").attr("style", "max-height:" + (winH - 206 - boxY) + "px;"); //得分版最大高度
    $(".m-progress").data("x", (boxX + 790)).attr("style", "left:" + (boxX + 790) + "px;top:" + boxY + "px;"); //进度坐标
    $(".m-change-left").attr("style", "top:" + (winH / 2 - 37) + "px;left:" + (boxX - 175 - 74 - 50) + "px;"); //左右切换按钮坐标
    $(".m-change-right").attr("style", "top:" + (winH / 2 - 37) + "px;left:" + (boxX + 780 + 45 + 50) + "px;");

    //元素显隐
    $(".power").bind("click", function () {
        if ($(this).data("show") == "1") {
            $(this).attr("title", "点击展开").data("show", "0");
            $(this).find("i").attr("class", "fa fa-chevron-right");
        } else {
            $(this).attr("title", "点击收起").data("show", "1");
            $(this).find("i").attr("class", "fa fa-chevron-down");
        }
        $(".items").slideToggle(500, function () {
            $(".power").find(".qc-line").toggleClass("hide");
        });
    });
    $(".m-progress").find(".p-no").bind("click", function () {
        if (marking.xiet) return;
        $(".m-progress").find(".students").fadeToggle(200);
    });
    $(".remarks").find(".title").bind("click", function () {
        $(".remarks").find(".title").removeClass("active");
        $(".remarks").find(".content").addClass("hide");
        $(this).addClass("active");
        switch ($(this).data("t")) {
            case 3:
                $(".remarks").find(".c-text").removeClass("hide");
                break;
            case 5:
                $(".remarks").find(".c-brow").removeClass("hide");
                break;
            default :
                $(".remarks").find(".c-custom").removeClass("hide");
                break;
        }
    });
    $(".custom-history").find(".edit").bind("click", function () {
        $(".custom-history").addClass("hide");
        $(".custom-edit").removeClass("hide");
        $(".txt-custom").focus();
    });

    //得分版滚动条
    $(".scroll-items").mCustomScrollbar({
        axis: "y",
        theme: "minimal-dark"
    });

    if (singer.isUndefined(type)) type = "";
    marking.mt.list(batch, type); //加载数据
    $(document)
        .delegate('.q-area', 'mouseenter', function () {
            $(this).addClass('hover');
        })
        .delegate('.q-area', 'mouseleave', function () {
            $(this).removeClass('hover');
        });
});