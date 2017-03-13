/**
 * Created by epc on 2016/4/25.
 * 阅卷核心JS
 */
var marking = (function () {
    var M = {
        data: {
            batch: "",
            paperId: "",
            paperTitle: "",
            isAb: false,
            allObjective: false,
            sectionType: 1, //试卷类型：0|1-普通卷|A卷、2-B卷
            isBag: false,
            bagId: "",
            questionGroupId: "", //协同阅卷题目组ID
            deyiGroupId: "", //考试所属圈子ID
            deyiGroupName: "",
            unMarkingCount: 0,
            isRefresh: false, //刷新当前答卷
            //isJoint: true, //协同阅卷
            loaded: false, //首次进入批阅
            lock: false, //阻止切换答卷资料速度过快
            keydown: true, //是否启用键盘事件
            editScore: false, //分数框聚焦时，键盘左右事件失效
            operation: 0, //产生变化的类型：2-批阅图标变动、4-批注图标变动
            url: DEYI.sites.static + "/v1/image/icon/marking/", //图标路径
            minScore: 1, //最小分值
            semiAuto: false, //半对自动扣分
            semiScore: "50%", //半对自动扣分伐值
            errorAuto: false, //错误自动扣分
            errorScore: "100%" //错误自动扣分伐值
        }, //基本数据
        areas: [], //各题对应区域
        region: {x: 0, y: 0, width: 0, height: 0}, //按题批阅可视区域
        shares: [], //已分享的答案列表
        questions: [],
        pictures: [],
        picture: {},
        details: [], //产生变化的答题
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
            box: $("#mkBox"), //试卷图片、批阅图标容器
            n_icons: [], //新增图标
            r_icons: [], //移除图标
            images: ["full.png", "semi.png", "error.png"], //系统图标
            x: 0, //图片所在页面坐标X
            y: 60, //图片所在页面坐标Y
            w: 780, //图片宽度
            winW: 0, //网页宽度
            winH: 0, //网页高度
            scale: 1.2, //批阅图片放大比例
            isZoom: false, //缩放状态
            /**
             * 初始化已批阅图标
             */
            init: function () {
                $(".icon").remove();
                var i = 0,
                    _icons = (singer.json(M.picture.icons) || []),
                    _marks = (singer.json(M.picture.marks) || []);
                if (_marks && _marks.length) {
                    for (; i < _marks.length; i++) {
                        _icons.push({
                            x: _marks[i].x,
                            y: _marks[i].y,
                            t: _marks[i].t,
                            w: _marks[i].w
                        });
                    }
                }
                if (_icons && _icons.length) {
                    var hasQuestions = M.questions && M.questions.length; //是否有批阅任务
                    for (i = 0; i < _icons.length; i++) {
                        var $html, icon = _icons[i];
                        //系统图标 需检测该图标是否属于可批阅任务范围内
                        if (icon.id && icon.id.length && hasQuestions) {
                            var exist = false;
                            for (var j = 0; j < M.questions.length; j++) {
                                if (M.questions[j].id == icon.id) {
                                    exist = true;
                                    break;
                                }
                            }
                            if (!exist) continue;
                        }
                        //显示图标
                        icon.x -= M.region.x;
                        icon.y -= M.region.y;
                        var _x = icon.x, _y = icon.y;
                        if (M.dw.isZoom) {
                            _x *= M.dw.scale;
                            _y *= M.dw.scale;
                        }

                        if (icon.t == 4) {
                            $html = $("<div class='icon icon-lg' style='font-size:24px;line-height:1em;font-family: 宋体;font-weight:bold;color:#fa8989;left: " + _x + "px;top: " + _y + "px;'>" + icon.w + "</div>");
                        } else {
                            var src = '';
                            if (icon.t < 3) {
                                src = M.data.url + M.dw.images[icon.t];
                            } else if (icon.t == 5) {
                                src = M.data.url + "brow/" + icon.w;
                            } else if (icon.t == 3) {
                                src = M.data.url + icon.w;
                            }
                            $html = $('<div class="icon ' + (icon.t == 3 ? "icon-lg" : "icon-sm") + '" style="left:' + _x + 'px;top:' + _y + 'px;" data-x="' + icon.x + '" data-y="' + icon.y + '">');
                            $html.html('<img src="' + src + '"/>');
                            if (icon.t == 1 || icon.t == 2) {
                                if (icon.id && icon.id.length) {
                                    $html.append('<div class="score">-' + (icon.w || 0) + '</div>');
                                    var iconId = "T" + icon.t + "_" + ((icon.x + "_" + icon.y).replace(/\./gi, 'D'));
                                    $html.attr("id", iconId).data("qId", icon.id).data("t", icon.t).data("score", icon.w);
                                }
                            }
                        }
                        M.dw.box.append($html.data("x", icon.x).data("y", icon.y));
                    }
                }
            },
            /**
             * 印图标在页面上
             * @param ev
             * @param id
             * @param sort
             */
            print: function (ev, id, sort) {
                M.qt.semiBox.hide();
                var t = M.dw.box.data("t");
                if (t == "clear") return; //橡皮擦
                if (t < 0 || t > 5) t = 0;
                var w = (t < 3) ? M.dw.images[t] : (M.dw.box.data("v") || "");
                if (!w) return;
                ev = ev || window.event;
                //图标相对答卷图片定位
                var x = parseInt(ev.clientX + M.dw.doc.scrollLeft() - M.dw.x);
                var y = parseInt(ev.clientY + M.dw.doc.scrollTop() - M.dw.y);
                //图片格式坐标差异
                if (t == 0 || t == 1) {
                    y -= 23;
                } else if (t == 2) {
                    x -= 3;
                    y -= 18;
                } else if (t == 5) {
                    switch (w) {
                        case "praise.png":
                            x += 2; //赞
                            break;
                        case "line.png":
                            y += 4; //横线
                            break;
                        case "wavy.png":
                            y += 6; //波浪线
                            break;
                        case "oval.png":
                            x -= 75;
                            y -= 6; //圈
                            break;
                    }
                }
                var _x = x, _y = y;
                if (M.dw.isZoom) {
                    x /= M.dw.scale;
                    y /= M.dw.scale;
                }
                //阻止同一坐标多次点击
                if (M.dw.n_icons.length) {
                    for (var i = M.dw.n_icons.length; i > 0; i--) {
                        var item = M.dw.n_icons[i - 1];
                        if (item.x == x && item.y == y) return;
                    }
                }
                var $html, iconId, icon = {
                    x: x,
                    y: y,
                    t: t,
                    w: t > 2 ? w : 0
                };
                if (t == 4) {
                    $html = $("<div class='icon icon-lg' style='font-size:24px;line-height:1em;font-family: 宋体;font-weight:bold;color:#fa8989;left: " + _x + "px;top: " + _y + "px;'>").text(w);
                } else {
                    $html = $('<div class="icon ' + (t == 3 ? "icon-lg" : "icon-sm") + '" style="left:' + _x + 'px;top:' + _y + 'px;">')
                        .html('<img src="' + M.data.url + (t == 5 ? "brow/" + w : w) + '"/>');
                    if (t < 3) icon.id = id;
                    if (t == 1 || t == 2) {
                        $html.append('<div class="score">-0</div>');
                        if (!singer.isUndefined(id)) {
                            iconId = "T" + t + "_" + ((x + "_" + y).replace(/\./gi, 'D'));
                            $html.attr("id", iconId).data("qId", id).data("t", t).data("score", 0);
                        }
                    }
                }
                M.dw.box.append($html.data("x", x).data("y", y));
                M.dw.n_icons.push(icon);
                if (!singer.isUndefined(id)) {
                    if (!singer.isUndefined(sort)) {
                        M.mk.scrollToActive(sort, 2);
                    }
                    if (t == 1 || t == 2) {
                        M.dw.openScoreBox(id, t, {x: _x, y: _y}, iconId);
                    }
                }
            },
            /**
             * 清除图标
             * @param $icon
             */
            clear: function ($icon) {
                var _score = parseFloat(($icon.data("score") || 0));
                if (_score > 0) M.qt.setScore($icon.data("qId"), _score, false, 0);
                var x = $icon.data("x"), y = $icon.data("y");
                $icon.remove();
                //加入待擦出列表
                var isNewIcon = false;
                for (var j = 0; j < M.dw.n_icons.length; j++) {
                    var n_icon = M.dw.n_icons[j];
                    if (n_icon.x == x && n_icon.y == y) {
                        isNewIcon = true;
                        M.dw.n_icons.splice(j, 1);
                        break;
                    }
                }
                if (!isNewIcon) M.dw.r_icons.push({x: x + M.region.x, y: y + M.region.y});
            },
            /**
             * 显示打分板弹出框
             * @param id
             * @param t
             * @param XY
             * @param iconId
             */
            openScoreBox: function (id, t, XY, iconId) {
                var total = 0; //题目总分
                for (var i = 0; i < M.questions.length; i++) {
                    if (M.questions[i].id == id) {
                        total = M.questions[i].score;
                        break;
                    }
                }
                //总分为0
                if (total == 0) {
                    M.qt.setScore(id, 0, true, 0);
                    return;
                }
                //自动扣分
                if ((t == 1 && M.data.semiAuto) || (t == 2 && M.data.errorAuto)) {
                    var tmp = t == 1 ? M.data.semiScore : M.data.errorScore;
                    var autoScore = 0;
                    if (tmp == "50%") autoScore = 0 - total / 2;
                    else if (tmp == "100%") autoScore = 0 - total;
                    else autoScore = parseFloat(tmp);
                    M.qt.setScore(id, autoScore, false, iconId);
                    return;
                }
                //扣分面板
                M.qt.semiBox.show(id, t, total, XY, iconId);
            },
            /**
             * 放大、缩小后刷新图表坐标
             */
            ref: function () {
                $(".icon").each(function (i, icon) {
                    var $icon = $(icon);
                    var x = parseFloat($icon.data("x")),
                        y = parseFloat($icon.data("y"));
                    if (M.dw.isZoom) {
                        x *= M.dw.scale;
                        y *= M.dw.scale;
                    }
                    $icon.css({"left": x, "top": y});
                });
                $(".q-area").each(function (i, area) {
                    var $area = $(area);
                    var x = parseFloat($area.data("x")),
                        y = parseFloat($area.data("y")),
                        w = parseFloat($area.data("w")),
                        h = parseFloat($area.data("h"));
                    if (M.dw.isZoom) {
                        x *= M.dw.scale;
                        y *= M.dw.scale;
                        w *= M.dw.scale;
                        h *= M.dw.scale;
                    }
                    $area.attr("style", "position: absolute;top:" + y + "px;left:" + x + "px;width:" + w + "px;height:" + h + "px;");
                });
            }
        },
        /**
         * 得分板
         */
        qt: {
            /**
             * 得分板初始化
             */
            init: function () {
                $(".m-goal .gi-total").bind("click", function () {
                    var $this = $(this),
                        $box = $(".mg-score-box"),
                        $scores = $('.mg-scores');
                    var $arrow = $box.find('.arrow'),
                        qid = $this.parent().data("id");

                    if (qid == $box.data("id")) {
                        $box.data("id", "").hide();
                        return;
                    }

                    $arrow.attr("class", "arrow arrow-left");
                    $scores.html("");

                    var y = $this.offset().top - M.dw.doc.scrollTop() - 6 - 60,
                        t = parseFloat($this.parent().data("total")) || 0;

                    if (t > 100) {
                        singer.msg("分数值过大，使用手动录入方式");
                        return;
                    }

                    for (var i = 0; i <= t; i++) {
                        $scores.append('<span data-val="' + i + '">' + i + '</span>');
                    }
                    $scores.append('<span data-val="r"><i class="fa fa-reply"></i></span>');

                    var _style = "";
                    if (t > 5) {
                        if (t > 50) _style += "width:432px;";
                        // 弹框高度 + 2 : 首尾增加 分数0 与 撤销
                        var h = Math.ceil((t + 2) / (t > 50 ? 14 : 7)) * 30 + 2;
                        console.log(h + "--" + y + "--" + M.dw.winH);
                        if ((h + y + 70) > M.dw.winH) {
                            y = y - h + 32;
                            $arrow.addClass("arrow-left-bottom");
                        } else {
                            $arrow.addClass("arrow-left-top");
                        }
                    }
                    _style += "top:" + y + "px;";

                    $box.data("id", qid).attr("style", _style).show();
                });
                var $ipts = $(".m-goal input");
                $ipts
                    .bind("focus", function () {
                        $(this).select();
                    })
                    .bind("change", function () {
                        M.qt.ckScore($(this));
                    })
                    .bind("keydown", function (e) {
                        if (e.which != 13) return;
                        var $this = $(this);
                        if (!M.qt.ckScore($this)) return;
                        var idx = $this.data("index");
                        if (idx != $ipts.length) {
                            $ipts.each(function (i, n) {
                                if ($(n).data("index") == (idx + 1)) {
                                    $(n).focus();
                                }
                            });
                            return;
                        }
                        M.mk.changePicture(1);
                    });
                $(".mg-score-box").delegate("span", "click", function () {
                    var $this = $(this),
                        $box = $(".mg-score-box");
                    var val = $this.data("val");
                    if (val == "r") {
                        $box.data("id", "").hide();
                        return;
                    }
                    M.qt.setScore($box.data("id"), val, true, 0);
                    $box.data("id", "").hide();
                });
                $(".gt-btn").bind("click", function () {
                    $(".mg-list .mg-item").each(function (i, item) {
                        M.qt.setScore($(item).data("id"), 0, true, 0);
                    });
                });
            },
            /**
             * 校验分数输入框的值
             * @param $ipt
             */
            ckScore: function ($ipt) {
                var id = $ipt.parent().data("id"), val = $ipt.val(),
                    total = parseFloat(parseFloat($ipt.parent().data("total")).toFixed(2)),
                    score = parseFloat(parseFloat($ipt.data("score")).toFixed(2));

                var isFail = ((!val || !val.length) && val != "0") || !/^[0-9]{0,3}\.??[0-9]{1,2}$/.test(val);
                if (isFail) {
                    $ipt.val(score);
                    M.qt.setScore(id, score, true, 0);
                    return false;
                }

                val = parseFloat(parseFloat(val).toFixed(2));
                if (val > total) {
                    val = total;
                    $ipt.val(total);
                }
                M.qt.setScore(id, val, true, 0);
                return true;
            },
            /**
             * 设置题目分数
             * @param id 问题ID
             * @param score 变动分数 正负
             * @param isEqual 是否赋值：true 该题分数 = 参数score值；false 该题得分 += 参数score值
             * @param iconId 批阅图标ID
             */
            setScore: function (id, score, isEqual, iconId) {
                isEqual = isEqual || false;
                score = parseFloat(score);
                var i = 0, question = 0;
                if (!M.questions || !M.questions.length) {
                    singer.msg("没有批阅任务");
                    return;
                }
                if (!M.picture.details || !M.picture.details.length)
                    return false;
                //原题
                for (; i < M.questions.length; i++) {
                    if (M.questions[i].id == id) {
                        question = M.questions[i];
                        break;
                    }
                }
                if (!question) {
                    singer.msg("没有检测到题目分数，请刷新重试");
                    return;
                }
                //变动记录
                var existChange = false, detail = 0, detailIndex = -1;
                if (M.details && M.details.length) {
                    for (i = 0; i < M.details.length; i++) {
                        if (M.details[i].question_id == id) {
                            detailIndex = i;
                            existChange = true;
                            detail = M.details[i];
                            break;
                        }
                    }
                }
                var currentScore = 0; //当前得分
                if (detail) {
                    currentScore = detail.score;
                } else {
                    //无变化
                    detail = {question_id: id};
                    for (i = 0; i < M.picture.details.length; i++) {
                        if (M.picture.details[i].questionId == id) {
                            currentScore = M.picture.details[i].score;
                            break;
                        }
                    }
                }
                //批阅图标扣分
                if (!singer.isUndefined(iconId) && iconId != 0) {
                    var textScore = 0 - score;
                    if (isEqual) textScore = currentScore + score;
                    if ((currentScore + score) < 0) textScore = currentScore;
                    //显示
                    var $icon = $("#" + iconId);
                    $icon.data("score", textScore);
                    $icon.find("div").text("-" + textScore);
                    //数据
                    var t = $icon.data("t"), x = $icon.data("x"), y = $icon.data("y");
                    for (i = (M.dw.n_icons.length - 1); i >= 0; i--) {
                        var icon = M.dw.n_icons[i];
                        if (icon.t == t && icon.x == x && icon.y == y) {
                            icon.w = textScore;
                            break;
                        }
                    }
                }
                //设置分数
                var lastScore = currentScore; //备份当前得分
                currentScore = isEqual ? score : currentScore + score;
                if (currentScore < 0) currentScore = 0;
                if (currentScore > question.score) currentScore = question.score;
                //更新变动记录
                detail.score = currentScore;
                detail.is_correct = currentScore == question.score;
                if (!existChange) {
                    detailIndex = M.details.length;
                    M.details.push(detail);
                }
                //上次批阅得分
                var needRemove = false;
                for (i = 0; i < M.picture.details.length; i++) {
                    var _d = M.picture.details[i];
                    if (_d.questionId == id) {
                        needRemove = _d.score == currentScore;
                        break;
                    }
                }
                if (needRemove) {
                    M.details.splice(detailIndex, 1);
                }

                //更新试卷总得分 - 非协同
                M.picture.totalScore += (currentScore - lastScore);
                $(".stu-score").html(M.picture.totalScore);
                //更新得分板
                $(".mg-list .mg-item").each(function (i, item) {
                    var $item = $(item);
                    if ($item.data("id") == id) {
                        $item.find("input").val(currentScore);
                        return;
                    }
                });
                M.mk.scrollToActive(question.sort);
            },
            /**
             * 批阅图标触发扣分面板
             */
            semiBox: {
                timerId: 0,
                iconId: 0,
                /**
                 * 显示扣分面板
                 * @param id 问题ID
                 * @param t 半对、错误
                 * @param total 问题总分
                 * @param XY 批阅图标坐标 XY.x XY.y
                 * @param iconId 批阅图标ID
                 */
                show: function (id, t, total, XY, iconId) {
                    if (M.qt.semiBox.timerId != 0 || M.qt.semiBox.iconId != 0) M.qt.semiBox.hide();
                    total = parseFloat((total || 0));
                    var semiBox = $("#semiBox"); //扣分面板容器
                    semiBox.html("");
                    var semiH = 122; //用于计算扣分版坐标
                    var $ul = $('<ul>');
                    $ul.data("id", id).data("iconId", iconId);
                    if (id && total > 0) {
                        var tmpScore = total > 20 ? 20 : total; //最多展示20分
                        if (tmpScore > 9) semiH = Math.ceil(tmpScore / 3) * 30 + 32; //扣分版高度
                        if (total > 4) {
                            var half = 0 - (t == 1 ? total / 2 : total);
                            $ul.append('<li class="one" data-v="' + half + '">' + (t == 1 ? "扣一半" : "全扣") + '</li>');
                        } //大于4分才增加扣一半、全扣
                        for (var i = 1; i <= tmpScore;) {
                            if (M.data.minScore != 1) {
                                $ul.append('<li class="li-float-score" style="display: none;" data-v="' + (0 - i + 0.5) + '">' + (0 - i + 0.5) + '</li>');
                            } //允许小数扣分
                            $ul.append('<li class="li-int-score" data-v="' + (0 - i) + '">' + (0 - i) + '</li>');
                            if (++i > tmpScore) {
                                if (i != (tmpScore + 1)) {
                                    $ul.append('<li class="li-int-score" data-v="' + (0 - tmpScore) + '">' + (0 - tmpScore) + '</li>');
                                }
                            }
                        }
                        $ul.append('<li data-v="cancel" title="取消"><i class="fa fa-reply"></i></li>'); //撤销
                        $ul.find("li").bind("click", function () {
                            var val = $(this).data("v");
                            var _id = $(this).parent().data("id");
                            var _iconId = $(this).parent().data("iconId");
                            if (val == "cancel") {
                                M.qt.semiBox.hide();
                                return;
                            }
                            M.qt.semiBox.iconId = 0;
                            M.qt.semiBox.hide();
                            M.qt.setScore(_id, parseFloat(val), false, _iconId);
                        }); //事件
                    } //扣分面板
                    if (M.data.minScore != 1) {
                        semiH += 30;  //扣分版坐标计算Y +30
                        var $tab = $('<div class="box semi-box-tab"></div>');
                        $tab.append('<div class="box-lg-6 active" data-v="1">整分</div>');
                        $tab.append('<div class="box-lg-6" data-v="0">小数</div>');
                        $tab.find("div").bind("mouseover", function () {
                            var $this = $(this);
                            if ($this.hasClass("active")) return;
                            $tab.find("div").removeClass("active");
                            $this.addClass("active");
                            var $box = $("#semiBox");
                            if ($this.data("v") == "1") {
                                $box.find(".li-int-score").show();
                                $box.find(".li-float-score").hide();
                            } else {
                                $box.find(".li-int-score").hide();
                                $box.find(".li-float-score").show();
                            }
                        });
                        semiBox.append($tab);
                    } //允许小数扣分 - 增加整数、小数Tab切换面板
                    semiBox.append($ul);
                    if (!singer.isUndefined(XY)) {
                        if ((M.dw.w - XY.x) < 180) XY.x -= 172;
                        if (XY.y > semiH) XY.y -= (semiH - 50);
                        semiBox.data("xy", XY.x + "-" + XY.y + "-" + semiH).attr("style", "top:" + XY.y + "px;left:" + (XY.x + 50) + "px;");
                    }
                    M.qt.semiBox.iconId = iconId;
                    M.qt.semiBox.timerId = window.setTimeout(function () {
                        M.qt.semiBox.hide(200);
                    }, 5000);
                    semiBox.show();
                },
                /**
                 * 关闭扣分面皮
                 * @param t 关闭动画用时(毫秒)
                 */
                hide: function (t) {
                    if (M.qt.semiBox.timerId != 0) window.clearTimeout(M.qt.semiBox.timerId);
                    if (M.qt.semiBox.iconId != 0) M.dw.clear($("#" + M.qt.semiBox.iconId));
                    M.qt.semiBox.timerId = 0;
                    M.qt.semiBox.iconId = 0;
                    $("#semiBox").html("").fadeOut(t || 0);
                }
            }
        },
        /**
         * 页面事件
         */
        mk: {
            /**
             * 初始化页面事件
             */
            init: function () {
                //返回按钮
                $(".m-back").bind("click", function () {
                    window.location.href = singer.sites.apps + "/work/teacher";
                    return false;
                });
                if (M.data.allObjective) {
                    //全客观题禁用部分操作
                    $(".tool-full").addClass("tool-full-disabled").removeClass("active");
                    $(".tool-semi").addClass("tool-semi-disabled");
                    $(".tool-error").addClass("tool-error-disabled");
                    $(".tool-setting").addClass("tool-setting-disabled");
                    $("#mkBox").removeClass("cur-0");
                    var $power = $(".power");
                    $power.data("show", "0").addClass("power-disabled");
                    $power.find("i").attr("class", "fa fa-chevron-right");
                    $power.find(".qc-line").hide();
                }
                $(".m-students-scroll").mCustomScrollbar({
                    axis: "y",
                    theme: "minimal-dark"
                });

                //学生列表滚动条
                $(".btn-mk-save").bind("mouseover", function () {
                    $(".mk-save-tip").stop().fadeIn();
                }).bind("mouseout", function () {
                    $(".mk-save-tip").stop().hide();
                }).bind("click", function () {
                    M.mt.submit(M.picture.pictureId, function () {
                        var pictureId = M.picture.pictureId;
                        M.mt.loadPicture(pictureId);
                        singer.msg("批阅进度已保存");
                    });
                }); //保存进度
                $(".btn-students").bind("click", function () {
                    var $studentsBox = $(".m-students");
                    if ($studentsBox.data("open")) {
                        $studentsBox.data("open", 0).hide();
                    } else {
                        $studentsBox.data("open", 1).fadeIn(500);
                    }
                }); //显隐学生列表
                $(".ms-submits").delegate("li", "click", function () {
                    var $this = $(this);
                    var id = $this.data("id");
                    if (id == M.picture.pictureId) return;
                    $(".m-students").data("open", 0).hide();
                    $(".ms-submits li").removeClass("active");
                    $(this).addClass("active");
                    $(".em-schedule-currect").html($this.data("idx"));
                    M.mt.submit(id);
                }); //点击学生名单切换答卷
                /**
                 * 图标打分板鼠标离开控制显隐
                 */
                $("#semiBox").mouseover(function () {
                    window.clearTimeout(M.qt.semiBox.timerId);
                }).mouseout(function () {
                    M.qt.semiBox.timerId = window.setTimeout(function () {
                        M.qt.semiBox.hide(200);
                    }, 5000);
                });
                /**
                 * 上、下一张试卷图片（切换答卷资料）
                 */
                $(".m-change").click(function () {
                    var direction = parseInt($(this).data("v") || "1");
                    M.mk.changePicture(direction);
                });
                /**
                 * 工具栏（批阅图标符号）切换
                 */
                $(".m-tools").find(".tool").bind("click", function () {
                    M.qt.semiBox.hide();
                    var t = $(this).data("t");
                    //全客观题 - 禁用
                    if (M.data.allObjective && (t == "0" || t == "1" || t == "2" || t == "setting")) return;
                    var $remarks = $(".remarks"), $sett = $(".m-setting");
                    //批注
                    if (t == "remark") {
                        $(".m-setting").data("open", 0).hide();
                        if ($remarks.data("open")) {
                            $remarks.data("open", 0).hide();
                            M.data.keydown = true;
                        } else {
                            $remarks.data("open", 1).fadeIn(500);
                            M.data.keydown = false;
                        }
                        return;
                    }
                    $remarks.data("open", 0).hide();
                    //设置
                    if (t == "setting") {
                        if ($sett.data("open")) {
                            $sett.data("open", 0).hide();
                        } else {
                            $sett.data("open", 1).fadeIn(500);
                        }
                        return;
                    }
                    $sett.data("open", 0).hide();
                    //缩放图片
                    if (t == "zoom") {
                        M.dw.isZoom = !M.dw.isZoom;
                        M.mk.zoom();
                        return;
                    }
                    //批阅
                    M.data.keydown = true;
                    if (M.dw.box.data("t") == t) return;
                    $(".m-tools").find(".tool").removeClass("active");
                    $(this).addClass("active");
                    M.dw.box.data("o", 2);
                    M.mk.setBox(t);
                });
                /**
                 * 鼠标右键切换工具栏（批阅图标符号）
                 */
                M.dw.box.bind("contextmenu", function () {
                    if (M.data.allObjective)
                        return;
                    $(".m-tools").find(".tool").removeClass("active");
                    var t = M.dw.box.data("t");
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
                    M.data.keydown = true;
                    $(".remarks").data("open", 0).hide();
                    M.qt.semiBox.hide();
                    M.dw.box.data("o", 2);
                    M.mk.setBox(t);
                    return false;
                });
                /**
                 * 批注图标符号切换
                 */
                $(".remarks li").bind("click", function () {
                    var t = $(this).parent().data("t");
                    var v = $(this).data("v");
                    $(".remarks").data("open", 0).hide();
                    M.data.keydown = true;
                    if (v == M.dw.box.data("v")) return;
                    $(".tool").removeClass("active");
                    $(".tool-remark").addClass("active");
                    M.dw.box.data("o", 4);
                    M.mk.setBox(t, v);
                });
                /**
                 * 自定义文字过滤
                 */
                $("#txtCustom").bind("change", function () {
                    var w = $("#txtCustom").val();
                    if (w && w.length) {
                        $("#txtCustom").val(w
                            .replace(/['"<>]/gi, '')
                            .replace('_', '')
                            .replace(/(\r)|(\n)/g, " "));
                    }
                });
                /**
                 * 自定义文字
                 */
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
                    $(".remarks").data("open", 0).hide();
                    M.data.keydown = true;
                    $(".custom-history").removeClass("hide");
                    $(".custom-edit").addClass("hide");
                    if (v == M.dw.box.data("v")) return;
                    $(".tool").removeClass("active");
                    $(".tool-remark").addClass("active");
                    M.dw.box.data("o", 4);
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
                });
                $(".custom-history").delegate(".item", "click", function () {
                    var v = $(this).text();
                    if (!(v && v.length)) {
                        singer.msg("说点什么吧...");
                        return;
                    }
                    $(".remarks").data("open", 0).hide();
                    M.data.keydown = true;
                    if (v == M.dw.box.data("v")) return;
                    $(".tool").removeClass("active");
                    $(".tool-remark").addClass("active");
                    M.dw.box.data("o", 4);
                    M.mk.setBox(4, v);
                }); //自定义文字历史点击使用
                $("#ddlMinScore").bind("change", function () {
                    var val = parseFloat($(this).val());
                    var $ddlSemi = $("#ddlSemiScore");
                    var $ddlError = $("#ddlErrorScore");
                    $ddlSemi.html('<option value="50%">扣一半</option>');
                    $ddlError.html('<option value="100%">全扣</option>');
                    for (var i = 1; i <= 5; i++) {
                        if (val != 1) {
                            $ddlSemi.append('<option value="-' + (i - 0.5) + '">-' + (i - 0.5) + '分</option>');
                            $ddlError.append('<option value="-' + (i - 0.5) + '">-' + (i - 0.5) + '分</option>');
                        }
                        $ddlSemi.append('<option value="-' + i + '">-' + i + '分</option>');
                        $ddlError.append('<option value="-' + i + '">-' + i + '分</option>');
                    }
                    $ddlSemi.append('<option value="100%">全扣</option>');
                    $ddlError.append('<option value="500%">扣一半</option>');
                }); //最小分值
                $(".m-setting").find("input[name='rdoSemi']").bind("click", function () {
                    var val = $(this).val();
                    if (val == "1") {
                        $("#ddlSemiScore").removeAttr("disabled");
                    } else {
                        $("#ddlSemiScore").attr("disabled", "disabled");
                    }
                }); //半对单选按钮点击
                $(".m-setting").find("input[name='rdoError']").bind("click", function () {
                    var val = $(this).val();
                    if (val == "1") {
                        $("#ddlErrorScore").removeAttr("disabled");
                    } else {
                        $("#ddlErrorScore").attr("disabled", "disabled");
                    }
                }); //错误单选按钮点击
                $("#btnUpdateMkArea").bind("click", function () {
                    M.mt.submit(M.picture.pictureId, function () {
                        window.location.href = "/marking/marking-area?batch=" + M.data.batch + "&type=" + M.data.sectionType;
                    });
                }); //重新标记题目区域
                $("#btnCancelSetting").bind("click", function () {
                    $(".m-setting").data("open", 0).hide();
                }); //取消批阅设置
                $("#btnSaveSetting").bind("click", function () {
                    var $setting = $(".m-setting");
                    M.data.minScore = parseFloat($("#ddlMinScore").val());
                    M.data.semiAuto = $setting.find("input[name='rdoSemi']:checked").val() == "1";
                    M.data.errorAuto = $setting.find("input[name='rdoError']:checked").val() == "1";
                    if (M.data.semiAuto) {
                        M.data.semiScore = $("#ddlSemiScore").val();
                    }
                    if (M.data.errorAuto) {
                        M.data.errorScore = $("#ddlErrorScore").val();
                    }
                    //得分版小数分
                    if (M.data.minScore != 1) {
                        $("#questionList").find(".li-float-score").show();
                    } else {
                        $("#questionList").find(".li-float-score").hide();
                    }
                    $(".m-setting").data("open", 0).hide();
                }); //保存批阅设置
                M.dw.box.click(function (ev) {
                    var t = M.dw.box.data("t");
                    if (t != 3 && t != 4 && t != 5) return;
                    M.dw.print(ev);
                    M.data.operation = M.data.operation | (M.dw.box.data("o") || 2);
                }); //只能印批注图标
                M.dw.box.delegate(".q-area", "click", function (ev) {
                    M.dw.print(ev, $(this).data("id"), $(this).data("sort"));
                    M.data.operation = M.data.operation | (M.dw.box.data("o") || 2);
                }); //印图标
                M.dw.box.delegate(".icon", "click", function () {
                    var t = M.dw.box.data("t");
                    if (t != "clear") return;
                    M.dw.clear($(this));
                    M.data.operation = M.data.operation | 6; //PS：不知道清除的是批阅图标还是批注图标 so 都提交
                }); //清除图标
                M.dw.box.delegate(".qa-sort", "click", function (ev) {
                    ev.stopPropagation();
                }); //题目区域中的序号不能点击
                M.dw.box.delegate(".qa-share-active", "click", function (ev) {
                    ev.stopPropagation();
                }); //已分享不能点击
                M.dw.box.delegate(".qa-share", "click", function (ev) {
                    ev.stopPropagation();
                    var $share = $(this);
                    var qid = $share.parent().data("id");
                    //已分享
                    if (M.shares && M.shares.length) {
                        for (var i = 0; i < M.shares.length; i++) {
                            var share = M.shares[i];
                            if (share.questionId == qid && share.studentId == M.picture.studentId)
                                return;
                        }
                    }
                    singer.confirm("请确认您分享的是本题的正确答案", function () {
                        $.post("marking/add-share", {
                            batch: M.data.batch,
                            paper_id: M.data.paperId,
                            group_id: M.data.deyiGroupId,
                            question_id: qid,
                            student_id: M.picture.studentId,
                            student_name: M.picture.studentName
                        }, function (json) {
                            if (json.status) {
                                M.shares.push({id: json.data, questionId: qid, studentId: M.picture.studentId});
                                $share.removeClass("qa-share").addClass("qa-share-active").html('<i class="fa fa-star"></i><span>已晒</span>');
                                singer.msg("分享成功");
                            } else {
                                singer.msg(json.message);
                            }
                        });
                    });
                }); //分享
                M.dw.box.delegate(".q-area", "mouseover", function () {
                    $(this).addClass("q-area-active");
                }); //批阅区域鼠标移入
                M.dw.box.delegate(".q-area", "mouseout", function () {
                    $(this).removeClass("q-area-active");
                }); //批阅区域鼠标移出

                if (M.mk.initMix && singer.isFunction(M.mk.initMix))
                    M.mk.initMix.call(this);
            },
            /**
             * 异常申报
             */
            exceptionFunc: function () {
                var $this = $(this), $li = $(".ms-submits li").eq(M.picture.index - 1);
                if ($this.data("type") == 0) {
                    var exType = -1;
                    var $box = $('<div class="exception-box">');
                    $box.append('<div class="excp-item" data-type="4"><i class="fa fa-circle-o"></i> 试卷重叠、题目内容被遮盖</div>');
                    $box.append('<div class="excp-item" data-type="2"><i class="fa fa-circle-o"></i> 试卷严重偏移/倾斜</div>');
                    $box.append('<div class="excp-item" data-type="1"><i class="fa fa-circle-o"></i> 图片不是对应题目</div>');
                    $box.append('<div class="excp-item" data-type="3"><i class="fa fa-circle-o"></i> AB卷混乱</div>');
                    $box.append('<div class="excp-item" data-type="0"><i class="fa fa-circle-o"></i> 其他</div>');
                    var $excpOther = $('<div class="excp-other hide">');
                    var $inputOther = $('<input type="text" maxlength="50" />');
                    $excpOther.html($inputOther);
                    $box.append($excpOther);
                    $box.find(".excp-item").bind("click", function () {
                        var $excpItem = $(this);
                        $box.find("i").removeClass("fa-dot-circle-o");
                        $excpItem.find("i").addClass("fa-dot-circle-o");
                        exType = parseInt($excpItem.data("type"));
                        if (exType == 0) {
                            $excpOther.removeClass("hide");
                            $inputOther.focus();
                        } else {
                            $excpOther.addClass("hide");
                        }
                    });
                    $inputOther.focus(function () {
                        M.data.editScore = true;
                    }).blur(function () {
                        M.data.editScore = false;
                    });
                    singer.dialog({
                        title: "请选择异常类型",
                        content: $box,
                        backdropOpacity: .7,
                        okValue: '确定',
                        cancelValue: '取消',
                        ok: function () {
                            if (exType < 0) {
                                singer.msg("请选择异常类型");
                                return false;
                            }
                            var qSortList = [];
                            for (var i = 0; i < M.questions.length; i++) {
                                qSortList.push(M.questions[i].sort);
                            }
                            var desc = '学生' + M.picture.index + ' 题号：' + qSortList.join('、');
                            if (M.data.isAb) {
                                desc = (M.data.sectionType == 1 ? '[A卷] ' : '[B卷] ') + desc;
                            }
                            if (exType == 0) {
                                var text = $inputOther.val();
                                if (text && text.length) desc += " [" + text + "]";
                            }
                            $.post('/marking/add-exception', {
                                id: M.picture.jointPictureId,
                                desc: desc,
                                type: exType
                            }, function (json) {
                                if (!json.status) {
                                    singer.msg(json.message);
                                    return;
                                }
                                $this.data("type", 1).html('取消异常申报');
                                var $i = $li.find("i");
                                if ($i && $i.length) $i.attr("class", "fa fa-exclamation i-declare");
                                else $li.append('<i class="fa fa-exclamation i-declare"></i>');
                            });
                        },
                        cancel: function () {
                        }
                    }).showModal();
                } else {
                    $.post('/marking/cancel-exception', {id: M.picture.jointPictureId}, function (json) {
                        if (!json.status) {
                            singer.msg(json.message);
                            return;
                        }
                        $this.data("type", 0).html('异常申报');
                        var $i = $li.find("i");
                        if (!M.data.isBag && !M.picture.isMarking) $i.attr("class", "i-undo");
                        else $i.remove();
                    });
                }
            },
            /**
             * 切换试卷
             * @param step
             * @returns {boolean}
             */
            changePicture: function (step) {
                if (M.data.stopping)
                    return false;
                if (!M.pictures || !M.pictures.length)
                    return false;
                if (M.data.lock) {
                    M.data.stopping = true;
                    singer.msg('老师别急，正在努力加载中...', 1000, function () {
                        M.data.stopping = false;
                    });
                    return false;
                }
                M.data.lock = true;
                var index = parseInt($(".em-schedule-currect").text() || "1") + step;
                var maxLen = (M.pictures && M.pictures.length) ? M.pictures.length : 0;
                if (index < 1)
                    index = 1;
                if (index > maxLen)
                    index = maxLen;
                var pictureId = index <= maxLen
                    ? (M.pictures[index - 1].pictureId)
                    : "";
                if (index <= maxLen) M.mk.studentActive(index);
                M.mt.submit(pictureId);
            },
            /**
             * 批阅图标切换绑定
             * @param t 图标类型：0正确、1半对、2错误、3批注、4自定义、5表情
             * @param v 批注图标地址
             */
            setBox: function (t, v) {
                M.dw.box.data("v", 0);
                switch (t) {
                    case 0:
                        M.dw.box.data("t", t).attr("class", "cur-0");
                        return;
                    case 1:
                        M.dw.box.data("t", t).attr("class", "cur-1");
                        return;
                    case 2:
                        M.dw.box.data("t", t).attr("class", "cur-2");
                        return;
                    case "clear":
                        M.dw.box.data("t", t).attr("class", "cur-clear");
                        return;
                    case 3:
                    case 4:
                        M.dw.box.data("t", t).data("v", v).attr("class", "cur-remark");
                        return;
                    case 5:
                        M.dw.box.data("t", t).data("v", v);
                        switch (v) {
                            case "smile.png":
                                M.dw.box.attr("class", "cur-bw-smile");
                                return;
                            case "cry.png":
                                M.dw.box.attr("class", "cur-bw-cry");
                                return;
                            case "praise.png":
                                M.dw.box.attr("class", "cur-bw-praise");
                                return;
                            case "doubt.png":
                                M.dw.box.attr("class", "cur-bw-doubt");
                                return;
                            case "line.png":
                                M.dw.box.attr("class", "cur-bw-line");
                                return;
                            case "wavy.png":
                                M.dw.box.attr("class", "cur-bw-wavy");
                                return;
                            case "oval.png":
                                M.dw.box.attr("class", "cur-bw-oval");
                                return;
                        }
                        return;
                }
            },
            /**
             * 上下切换答卷后更改学生名单高亮属性
             * @param index
             */
            studentActive: function (index) {
                var $lis = $(".ms-submits li");
                var lisLen = $lis.length;
                index -= 1;
                if (index < 0)
                    index = 0;
                if (index < lisLen) {
                    $lis.removeClass("active");
                    $lis.eq(index).addClass("active");
                }
            },
            /**
             * 缩放批阅图片
             */
            zoom: function () {
                var w = 1010; //计算出的阅卷区域宽度，不包含左右切换按钮
                if (M.dw.isZoom) {
                    M.dw.winW = $(window).width();
                    if (M.dw.winW < 1316) {
                        singer.msg("您的网页可操作区域过低，不能放大。<br/>请检查浏览器是否未开起全屏功能。");
                        M.dw.isZoom = false;
                        return;
                    }
                    w = 1166;
                }

                //答卷图片容器有10像素边框

                var x = Math.ceil((M.dw.winW - w) / 2) - 8; //最左侧 X  考虑网页滚动条 - 所以整体向左移动8像素
                M.dw.x = x + 175; //答卷图片所在网页坐标

                $(".m-header").attr("style", "width:" + w + "px;left:" + x + "px;");
                $(".m-goal").attr("style", "top:" + M.dw.y + "px;left:" + x + "px;"); //得分版坐标
                $(".m-body").attr("style", "margin-left:" + M.dw.x + "px;width:" + (w - 230) + "px;"); //批阅图片坐标
                $("#markingImage").attr("style", "width:" + (w - 230) + "px;height:auto;");
                $(".m-tools").attr("style", "top:" + M.dw.y + "px;left:" + (x + (w - 45)) + "px;"); //工具栏坐标

                $(".mg-scroll").attr("style", "max-height:" + (M.dw.winH - M.dw.y - 50) + "px;"); //得分版最大高度
                $(".m-change-left").attr("style", "top:" + (M.dw.winH / 2 - 32) + "px;left: " + (x - 75) + "px;"); //左右切换按钮坐标
                $(".m-change-right").attr("style", "top:" + (M.dw.winH / 2 - 32) + "px;left: " + (x + w + 10) + "px;");

                var $mosaic = $(".mosaic"),
                    $toolZoom = $(".tool-zoom");
                if (M.dw.isZoom) {
                    $mosaic.addClass("mosaic-lg");
                    $toolZoom.addClass("tool-zoom-small");
                } else {
                    $mosaic.removeClass("mosaic-lg");
                    $toolZoom.removeClass("tool-zoom-small");
                }
                M.dw.ref();
            },
            /**
             * 滚动到指定题目位置
             * @param sort 问题序号
             * @param t 2得分板滚动、4批阅区域滚动
             */
            scrollToActive: function (sort, t) {
                if (!sort || !sort.length) return;
                $(".mg-list .mg-item").removeClass("mg-active");
                $(".num-" + sort).addClass("mg-active");

                if ((t & 2) > 0)
                    $(".mg-scroll").mCustomScrollbar("scrollTo", ".num-" + sort);
                if ((t & 4) > 0) {
                    var $area = $(".qno-" + sort);
                    if (!$area) return;
                    var x = $area.offset().left,
                        y = $area.offset().top - 65;
                    window.scrollTo(x, y);
                }
            }
        },
        /**
         * 交互
         */
        mt: {
            initDialog: 0,
            /**
             * 加载基本数据(题号、分数、可操作区域)
             */
            loadData: function () {
                var $content = $('<div class="m-init-box f-tac">');
                $content.append('<div><i class="fa fa-spin fa-spinner fa-4x"></i></div>');
                $content.append('<div class="m-init-message">正在准备阅卷数据，请稍候...</div>');
                M.mt.initDialog = singer.dialog({
                    content: $content
                });
                M.mt.initDialog.showModal();
                //$(M.mt.initDialog.node).find(".ui-dialog-body").css("padding","0");

                $.post("/marking/load", {
                    batch: M.data.batch,
                    type: M.data.sectionType
                }, function (json) {
                    if (!json.status) {
                        $(".dy-container").css("padding", "0");
                        $(".m-message").text(json.message).removeClass("hide");
                        M.mt.initDialog.close().remove();
                        M.mt.initDialog = 0;
                    } else {
                        M.mt.bindData(json.data);
                    }
                });
            },
            /**
             * 绑定基本数据
             * @param data
             */
            bindData: function (data) {
                M.data.batch = data.batch;
                M.data.paperId = data.paperId;
                M.paperTitle = data.paperTitle;
                M.data.isAb = data.isAb;
                M.data.allObjective = data.allObjective || false;
                M.data.sectionType = data.sectionType;
                M.data.isBag = data.isBag || false;
                M.data.bagId = data.bagId || '';
                M.data.questionGroupId = data.questionGroupId || '';
                M.data.deyiGroupId = data.deyiGroupId;
                M.data.deyiGroupName = data.deyiGroupName || '';
                M.data.unMarkingCount = data.unMarkingCount || 0;
                singer.config('paperId', data.paperId);

                M.areas = singer.json(data.areas) || [];
                M.shares = data.shares || [];
                M.questions = data.questions || [];
                M.pictures = data.pictures || [];
                if (data.region && data.region.length)
                    M.region = singer.json(data.region);

                //初始化顶部数据
                var headerTemplateData = {
                    paperTitle: data.paperTitle,
                    pictures: M.pictures,
                    unSubmits: data.unSubmits || [],
                    unMarkingCount: data.unMarkingCount,
                    isAb: data.isAb || false,
                    markingStatus: data.markingStatus,
                    sectionType: data.sectionType
                };
                var headerTemplateHtml = template('m-header-box', headerTemplateData);
                $(".m-header-box").html(headerTemplateHtml).removeClass("hide");
                $(".m-header").attr("style", "left:" + (Math.ceil((M.dw.winW - 1010) / 2) - 8) + "px;");

                //初始化得分板
                var $s = template('question-template', {questions: M.questions});
                $(".mg-list").html($s);

                //初始化可批阅区域
                var $box = $("#mkBox");
                for (var i = 0; i < M.areas.length; i++) {
                    var q = M.areas[i];
                    var x = q.x - M.region.x,
                        y = q.y - M.region.y;
                    var $area = $("<div class='q-area qno-" + q.index + "' style='position: absolute;width:" + q.width + "px;height:" + q.height + "px;left: " + x + "px;top: " + y + "px;'>");
                    $area.data("x", x).data("y", y).data("t", q.t)
                        .data("w", q.width).data("h", q.height)
                        .data("id", q.id).data("sort", q.index);
                    $area.append('<div class="qa-sort">' + q.index + '</div>');
                    $area.append('<div class="qa-share"><i class="fa fa-star"></i><span class="qa-share-text">晒答案</span></div>')
                    $box.append($area);
                }

                if (M.mt.bindDataMix && singer.isFunction(M.mt.bindDataMix))
                    M.mt.bindDataMix.call(this);

                var pictureId = "";
                if (M.pictures && M.pictures.length) {
                    pictureId = data.lastPicture;
                    M.mk.studentActive(M.pictures.length);
                }
                $(".m-body").removeClass("hide");
                if (M.mt.initDialog != 0) {
                    M.mt.initDialog.close().remove();
                    M.mt.initDialog = 0;
                }

                if (pictureId) {
                    M.mt.loadPicture(pictureId); //绑定当前答卷资料
                } else {
                    $('#mkBox').replaceWith('<div class="dy-nothing" style="height: 320px;line-height: 320px"><i class="iconfont dy-icon-emoji01"></i>暂时还没有提交的答卷</div>');
                    $('.m-change,.btn-mk-save').remove();
                }
                M.qt.init(); //初始化得分板事件
                M.mk.init(); //初始化页面事件
            },

            /**
             * 加载答卷详细
             * @param pictureId 答卷图片ID、或协同图片ID
             * 含主观题的协同阅卷，id = 协同图片ID；全客观题的协同、或非协同阅卷，id = 答卷图片ID
             */
            loadPicture: function (pictureId) {
                if (!M.data.allObjective && !(M.questions && M.questions.length)) {
                    $(".m-body").addClass("hide");
                    $(".em-schedule-currect").text("0");
                    $(".dy-container").css("padding", "0");
                    $(".m-message").text("没有批阅任务").removeClass("hide");
                    $(".j-autoHeight").attr("style", "height:" + $(window).height() + "px;");
                    M.data.lock = false;
                    return;
                }
                if (pictureId && pictureId.length) {
                    //重复加载
                    var tmpId = '';
                    if (M.picture) {
                        tmpId = M.picture.pictureId;
                    }
                    if (!M.data.isRefresh && pictureId == tmpId) {
                        if (M.picture.index == 1) {
                            singer.msg("已批阅到第一份试卷啦~");
                        }
                        if (M.picture.index == M.pictures.length) {
                            singer.msg("没有待批阅的试卷啦~");
                        }
                        M.data.lock = false;
                        return;
                    }
                } else {
                    singer.msg("参数错误，请刷新重试");
                    M.data.lock = false;
                }
                M.data.isRefresh = false;
                $.post("/marking/picture", {picture_id: pictureId, type: M.data.sectionType}, function (json) {
                    M.mt.jsonPicture(json, pictureId);
                });
                if (!M.data.loaded)
                    M.data.loaded = true;
            },

            /**
             * 处理答卷详细加载完成后的数据
             * @param json
             */
            jsonPicture: function (json, pictureId) {
                if (!json.status || !json.data) {
                    //首次进入协同阅卷是没有已批阅列表的，自动获取一份答卷, 失败则显示以下内容
                    if (!M.pictures || !M.pictures.length) {
                        $(".m-body").addClass("hide");
                        $(".dy-container").css("padding", "0");
                        $(".m-message").text("没有待批阅的答卷").removeClass("hide");
                        $(".j-autoHeight").attr("style", "height:" + $(window).height() + "px;");
                        return;
                    }
                    var msg = json.message || "答卷加载失败";
                    singer.msg(msg);
                    return;
                }
                if (!json.data.pictureUrl || !json.data.pictureUrl.length) {
                    singer.msg("由于网络原因加载失败，请重新加载！");
                }
                M.picture = json.data;
                M.mt.bindPicture();
            },

            /**
             * 绑定当前答卷资料
             */
            bindPicture: function () {
                var i = 0;
                //当前批阅图片所在列表序号
                for (; i < M.pictures.length; i++) {
                    if (M.pictures[i].pictureId == M.picture.pictureId) {
                        var picture = M.pictures[i];
                        M.picture.index = i + 1;
                        M.picture.studentId = picture.studentId;
                        M.picture.studentName = picture.studentName;
                        break;
                    }
                }
                //切换图片
                $("#markingImage")
                    .unbind('load.marking')
                    .bind('load.marking', function () {
                        M.data.lock = false;
                    })
                    .attr("src", M.picture.pictureUrl);
                //绑定得分板数据
                if (M.picture.details && M.picture.details.length) {
                    var $scores = $(".mg-list .mg-item");
                    for (i = 0; i < M.picture.details.length; i++) {
                        var detail = M.picture.details[i];
                        $scores.each(function (j, item) {
                            if ($(item).data("id") == detail.questionId) {
                                $(item).find(".gi-input").data("score", detail.score).val(detail.score);
                            }
                        });
                    }
                }
                $(".em-schedule-currect").html(M.picture.index);
                $(".em-schedule-readed").text(M.pictures.length);
                $(".em-schedule-surplus").data("count", M.picture.unMarkingCount).text(M.picture.unMarkingCount);
                $(".m-goal .mg-item input").eq(0).focus();
                //绑定学生数据
                $(".stu-name").html(M.picture.studentName);
                $(".stu-score").html(M.picture.totalScore);
                $(".ms-submits li").removeClass("active").each(function (i, li) {
                    var $li = $(li);
                    if ($li.data("id") == M.picture.pictureId) {
                        $li.addClass("active");
                    }
                });
                //绑定已分享的答案
                if (M.shares && M.shares.length) {
                    $(".qa-share-active").removeClass("qa-share-active").addClass("qa-share").find("span").html("晒答案");
                    $(".q-area").each(function (n, area) {
                        var $area = $(area);
                        var qid = $area.data("id");
                        for (var i = 0; i < M.shares.length; i++) {
                            var share = M.shares[i];
                            if (share.questionId == qid && share.studentId == M.picture.studentId) {
                                $area.find(".qa-share").removeClass("qa-share").addClass("qa-share-active").find("span").html("已晒");
                                break;
                            }
                        }
                    });
                }
                //初始化已批阅图标
                M.dw.init();
            },
            /**
             * 提交数据
             * @param pictureId 待加载的图片ID - 非提交图片ID
             * @param callback 回调
             */
            submit: function (pictureId, callback) {
                M.qt.semiBox.hide();
                //检测是否需要提交
                if (M.data.operation != 0 || (M.details && M.details.length)) {
                    M.data.lock = true;
                    var _icons = [], _marks = [];
                    if (M.dw.n_icons && M.dw.n_icons.length && ((M.data.operation & 4) > 0 || (M.data.operation & 2) > 0)) {
                        for (var j = 0; j < M.dw.n_icons.length; j++) {
                            var icon = {
                                x: M.dw.n_icons[j].x + M.region.x,
                                y: M.dw.n_icons[j].y + M.region.y,
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
                        if ((M.data.operation & 2) == 0) _icons = [];
                        if ((M.data.operation & 4) == 0) _marks = [];
                    }
                    $.post("/marking/submit",
                        {
                            pictureId: M.picture.pictureId,
                            operation: M.data.operation,
                            paperType: M.data.sectionType,
                            icons: singer.json(_icons).replace(/\'/gi, '"'),
                            marks: singer.json(_marks).replace(/\'/gi, '"'),
                            removeIcons: singer.json(M.dw.r_icons).replace(/\'/gi, '"'),
                            detailData: singer.json(M.details).replace(/\'/gi, '"')
                        },
                        function (json) {
                            if (json.status) {
                                M.details = [];
                                M.data.operation = 0;
                                M.dw.n_icons = [];
                                M.dw.r_icons = [];
                                M.data.isRefresh = true;
                                $('.ms-submits li[data-id="' + M.picture.pictureId + '"]').find('i').remove();
                                if (callback && singer.isFunction(callback)) {
                                    callback.call(this);
                                    return;
                                }
                                M.mt.loadPicture(pictureId);
                            } else {
                                M.data.lock = false;
                                singer.msg(json.message);
                            }
                        });
                } else {
                    if (callback && singer.isFunction(callback)) {
                        callback.call(this);
                        return;
                    }
                    M.mt.loadPicture(pictureId);
                }
            }
        }
    });
})(marking);

$(function ($) {
    var M = marking;
    M.dw.winW = $(window).width();
    M.dw.winH = $(window).height();
    if (M.dw.winW < 1220) M.dw.winW = 1220;

    //批注
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
    $(document).bind("mouseup", function (e) {
        var $mgList = $(".m-goal");
        if (!$mgList.is(e.target) && $mgList.has(e.target).length === 0) {
            $(".mg-score-box").data("id", "").hide();
        }

        var $header = $(".m-header");
        if (!$header.is(e.target) && $header.has(e.target).length === 0) {
            $(".m-students").data("open", 0).hide();
        }

        var $tool = $(".m-tools");
        if (!$tool.is(e.target) && $tool.has(e.target).length === 0) {
            $(".remarks").data("open", 0).hide();
            $(".m-setting").data("open", 0).hide();
        }
    });
    //得分版滚动条
    $(".mg-scroll").mCustomScrollbar({
        axis: "y",
        theme: "minimal-dark"
    });
});
