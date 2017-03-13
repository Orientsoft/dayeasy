var errorBook = (function () {
    var EB = {
        total: 0,
        list: [],
        params: {
            index: 1,
            size: 10,
            subject: -1,
            type: -1,
            start: "",
            expire: "",
            key: "",
            source_type: 0,
            has_reason: -1
        },//筛选参数
        /**
         * 错题列表
         */
        load: function () {
            EB.list = [];
            $(".loading").show();
            $(".q-list").hide();
            $(".v-empty").hide();
            $.post("/errorBook/questions", EB.params).success(function (json) {
                $(".loading").hide();
                if (json.status) {
                    EB.total = json.count;
                    EB.pager(singer.page({
                        current: EB.params.index - 1,
                        size: EB.params.size,
                        total: json.count
                    }));
                    if (json.data && json.data.length) {
                        EB.list = json.data;
                        EB.bind();
                    } else {
                        $(".v-empty").show();
                    }
                }
            }).error(function () {});
        },
        /**
         * 数据绑定
         */
        bind: function () {
            //错题总数
            $(".e-total").text(EB.total);
            //错题列表
            var $list = $(".q-list");
            $list.html("").show();
            var template = $("#ebookItem").html();

            for (var i = 0; i < EB.list.length; i++) {
                var item = EB.list[i];
                var paper_title = '';
                if (EB.params.source_type == 0) {
                    paper_title = '<a href="/work/student/pub-paper/' + item.batch+'" target="_blank">' + item.paper_title + '</a>';
                } else {
                    paper_title = '<span class="text-muted">' + item.paper_title + '</span>';
                }
                var temp = template
                    .replace("{paper_title}", paper_title)
                    .replace("{subject_name}", item.subject_name)
                    .replace("{subject_name}", item.subject_name)
                    .replace("{time}", item.time)
                    .replace("{errorId}", item.id)
                    .replace("{errorId}", item.id)
                    .replace("{questionId}", item.question.question_id);
                //错因
                if (item.reason && item.reason.tags && item.reason.tags.length) {
                    var tagHtml = "";
                    for (var j = 0; j < item.reason.tags.length; j++) {
                        tagHtml += '<div class="d-tag">' + item.reason.tags[j].name + '</div>';
                    }
                    temp = temp.replace("{reasons}", tagHtml);
                } else {
                    temp = temp.replace("{reasons}", "");
                }
                var $item = $(temp);
                var $q = singer.render('q-template', item.question);
                $item.find(".q-main").html("").append($q);
                if (i == EB.list.length - 1) {
                    $item.addClass("q-item-last");
                }
                if (EB.params.source_type != 0) {
                    $item.find(".span-dw").remove();
                } else if (EB.dw.check(item.id)) {
                    $item.find(".span-dw").addClass("dw-active").html('<i class="fa fa-times"></i> 移除下载</span>');
                }
                $list.append($item);
            }
            //解析并展示公式
            setTimeout(singer.loadFormula, 120);
        },
        /**
         * 初始化分页控件
         * @param pages
         */
        pager: function (pages) {
            if (pages.data && pages.data.length) {
                $(".d-pager").show();
                var $ul = $(".pagination");
                $ul.html("");
                if (pages.prev) {
                    $ul.append('<li><a href="javascript:void(0);" data-num="' + (pages.current - 1) + '">上一页</a></li>');
                }
                for (var i = 0; i < pages.data.length; i++) {
                    var g = pages.data[i];
                    var li = '<li' + (g.isActive ? ' class="active"' : '') + '><a href="javascript:void(0);" data-num="' + (g.isActive ? '0' : g.page) + '">' + (g.page > 0 ? g.page : '...') + '</a></li>';
                    $ul.append(li);
                }
                if (pages.next) {
                    $ul.append('<li><a href="javascript:void(0);" data-num="' + (pages.current + 1) + '">下一页</a></li>');
                }
            } else {
                $(".d-pager").hide();
            }
        },
        /**
         * 错题下载
         */
        dw: {
            show: false,
            data: {subjects: [], list: []},
            /**
             * 刷新下载列表页面
             */
            refreshHtml: function () {
                $(".dc-items").html("");
                if (EB.dw.data.list.length) {
                    var total = EB.dw.data.list.length;
                    $(".dw-icon").text(total);
                    $(".dc-num").text(total);
                    for (var i = 0; i < EB.dw.data.subjects.length; i++) {
                        var subject = EB.dw.data.subjects[i];
                        $(".dc-items").append('<div class="dc-item"><span>' +
                            subject.name + '</span><span>' +
                            subject.count + '</span></div>');
                    }
                    $(".d-dowload").show();
                } else {
                    $(".d-dowload").hide();
                }
            },
            /**
             * 添加移除下载列表
             * @param id 错题ID
             * @param name 科目名称
             * @returns {boolean}
             */
            refresh: function (id, name) {
                for (var i = 0; i < EB.dw.data.list.length; i++) {
                    if (EB.dw.data.list[i] == id) {
                        EB.dw.data.list.splice(i, 1);
                        for (var j = 0; j < EB.dw.data.subjects.length; j++) {
                            if (EB.dw.data.subjects[j].name == name) {
                                EB.dw.data.subjects[j].count -= 1;
                                if (EB.dw.data.subjects[j].count < 1) {
                                    EB.dw.data.subjects.splice(j, 1);
                                }
                            }
                        }
                        return true;
                    }
                }
                if (EB.dw.data.list.length > 49) {
                    singer.msg("已经选择了很多错题啦，先下载一份吧~~");
                    return false;
                }
                EB.dw.data.list.push(id);
                for (var k = 0; k < EB.dw.data.subjects.length; k++) {
                    if (EB.dw.data.subjects[k].name == name) {
                        EB.dw.data.subjects[k].count += 1;
                        return true;
                    }
                }
                EB.dw.data.subjects.push({name: name, count: 1});
                return true;
            },
            /**
             * 清空下载列表
             */
            reset: function () {
                EB.dw.data = {subjects: [], list: []};
                $(".span-dw").removeClass("dw-active").html('<i class="fa fa-plus"></i> 加入下载</span>');
                EB.dw.refreshHtml();
            },
            /**
             * 下载
             */
            download: function () {
                if (EB.dw.data.list.length < 1) {
                    singer.msg("请选择错题");
                    return;
                }
                $("#txtDwData").val(encodeURIComponent(singer.json(EB.dw.data.list)));
                $("#dwForm").submit();
                singer.dialog({
                    title: "下载提示",
                    content: "请问您下载成功了吗？<br/>确定下载成功将<span style='color:#ffab00'>清空</span>错题下载列表",
                    fixed: true,
                    backdropOpacity: .7,
                    okValue: "是的",
                    cancelValue: "还没有",
                    ok: function () {
                        $("#txtDwData").val("");
                        EB.dw.reset();
                    },
                    cancel: function () {
                    }
                }).showModal();
            },
            /**
             * 检测是否已加入下载列表
             * @param id 错题ID
             * @returns {boolean}
             */
            check: function (id) {
                if (!EB.dw.data.list.length)
                    return false;
                for (var j = 0; j < EB.dw.data.list.length; j++) {
                    if (id == EB.dw.data.list[j]) {
                        return true;
                    }
                }
                return false;
            }
        }
    };
    return EB;
})();

$(function ($) {
    singer.loadTemplate('question-item', function () {
        errorBook.load();
    });

    /*****加载学科*****/
    $.get("/errorBook/subjects").success(function (json) {
        var $subjects = $(".sn-subjects");
        $subjects.html("");
        if (json && json.length) {
            $subjects.append('<div class="item" data-val="-1">不限</div>');
            for (var i = 0; i < json.length; i++) {
                $subjects.append('<div class="item" data-val="' + json[i].id + '">' + json[i].name + '</div>');
            }
        }
    });
    /*****初始化页面*****/
    //来源筛选
    $(".sn-source").delegate(".item", "click", function (e) {
        e.stopPropagation();
        var $this = $(this);
        var val = parseInt($this.data("val"));
        if (singer.isUndefined(val))
            return;
        if (val == errorBook.params.type) {
            $this.parents(".sing").data("open", 0).removeClass("active");
            $this.parent().hide();
            return;
        }
        $this.parents(".sing").data("open", 0).removeClass("active").find("span").text($this.text());
        $this.parent().hide();
        if (val != "0") {
            $this.data("val", "0").text("试卷错题");
        } else {
            $this.data("val", "1").text("课本错题");
        }
        errorBook.params.source_type = val;
        errorBook.params.index = 1;
        errorBook.load();
    });
    //更改学科后加载题型并及时查询
    $(".sn-subjects").delegate(".item", "click", function (e) {
        e.stopPropagation();
        var $this = $(this);
        var val = parseInt($this.data("val"));
        if (singer.isUndefined(val))
            return;
        if (val == errorBook.params.subject) {
            $this.parents(".sing").data("open", 0).removeClass("active");
            $this.parent().hide();
            return;
        }
        var _text = $this.text();
        if (val == "-1") _text = "学科";
        $this.parents(".sing").data("open", 0).removeClass("active").find("span").text(_text);
        $this.parent().hide();

        errorBook.params.subject = val;
        errorBook.params.type = -1;
        errorBook.params.index = 1;
        errorBook.load();
        //加载题型
        var $types = $(".sn-types");
        $types.parents(".sing").find("span").text("题型");
        $types.html("");
        if (val > -1) {
            $.get("/errorBook/questionType?subjectId=" + val).success(function (json) {
                if (json && json.length) {
                    $types.append('<div class="item" data-val="-1">不限</div>');
                    for (var i = 0; i < json.length; i++) {
                        $types.append('<div class="item" data-val="' + json[i].id + '">' + json[i].name + '</div>');
                    }
                }
            });
        } else {
            $types.append('<div class="item" data-val="-1">请选择学科</div>');
        }
    });
    //更改题型后及时查询
    $(".sn-types").delegate(".item", "click", function (e) {
        e.stopPropagation();
        var $this = $(this);
        var val = parseInt($this.data("val"));
        if (singer.isUndefined(val))
            return;
        if (val == errorBook.params.type) {
            $this.parents(".sing").data("open", 0).removeClass("active");
            $this.parent().hide();
            return;
        }
        var _text = $this.text();
        if (val == "-1") _text = "题型";
        $this.parents(".sing").data("open", 0).removeClass("active").find("span").text(_text);
        $this.parent().hide();
        errorBook.params.type = val;
        errorBook.params.index = 1;
        errorBook.load();
    });
    //时间筛选
    $(".sn-times").bind("click", function (e) {
        e.stopPropagation();
    });
    $(".st-cancel").bind("click", function () {
        $(this).parents(".sing").data("open", 0).removeClass("active");
        $(".sn-times").hide();
    });
    $(".st-ok").bind("click", function () {
        $(this).parents(".sing").data("open", 0).removeClass("active").find("span").text("时间范围");
        $(".sn-times").hide();
        var star = $("#txtStarDate").val();
        var end = $("#txtEndDate").val();
        if (star != "" && end != "") {
            $(this).parents(".sing").find("span").text(star + " - " + end);
        } else if (star != "") {
            $(this).parents(".sing").find("span").text(star + " 开始");
        } else if (end != "") {
            $(this).parents(".sing").find("span").text(end + " 截止");
        }
        if (star != errorBook.params.start || end != errorBook.params.expire) {
            errorBook.params.start = star;
            errorBook.params.expire = end;
            errorBook.params.index = 1;
            errorBook.load();
        }
    });
    $(".sn-time-reset").bind("click", function () {
        $(this).parents(".sing").data("open", 0).removeClass("active").find("span").text("时间范围");
        $(".sn-times").hide();
        $("#txtStarDate").val("");
        $("#txtEndDate").val("");
        if ("" != errorBook.params.start || "" != errorBook.params.expire) {
            errorBook.params.start = "";
            errorBook.params.expire = "";
            errorBook.params.index = 1;
            errorBook.load();
        }
    });
    //是否分析错因筛选
    $(".sn-reason").delegate(".item", "click", function (e) {
        e.stopPropagation();
        var $this = $(this);
        var val = parseInt($this.data("val"));
        if (singer.isUndefined(val))
            return;
        if (val == errorBook.params.has_reason) {
            $this.parents(".sing").data("open", 0).removeClass("active");
            $this.parent().hide();
            return;
        }
        var _text = $this.text();
        if (val == "-1") _text = "错因分析";
        $this.parents(".sing").data("open", 0).removeClass("active").find("span").text(_text);
        $this.parent().hide();
        errorBook.params.has_reason = val;
        errorBook.params.index = 1;
        errorBook.load();
    });
    //搜索
    function Search() {
        var val = $("#txtSearchKey").val().trim();
        if (val == errorBook.params.key)
            return;
        errorBook.params.key = val;
        errorBook.params.index = 1;
        errorBook.load();
    }

    //按钮搜索
    $("#btnSearch").bind("click", function () {
        Search();
    });
    //搜索框按键
    $("#txtSearchKey").bind("keyup", function (event) {
        var e = event || window.event || arguments.callee.caller.arguments[0];
        if (e && e.keyCode == 13) {
            $(this).data("idx", "-99");
            Search();
            $(this).blur();
        } else if (e && (e.keyCode == 38 || e.keyCode == 40)) {
            var $keys = $(".search-keys").find(".item");
            if ($keys.length < 1) return;
            var idx = 0;
            if (singer.isUndefined($(this).data("idx"))) idx = -1;
            else idx = parseInt($(this).data("idx"));
            if (idx == -99) {
                var newHtml = '<div class="item hide">' + $(this).val() + '</div>' + $(".search-keys").html();
                $(".search-keys").html(newHtml);
                $keys = $(".search-keys").find(".item");
                idx = 0;
            }
            $keys.removeClass("active");
            idx += e.keyCode == 38 ? -1 : 1;
            if (idx >= $keys.length) idx = 0;
            if (idx < 0) idx = $keys.length - 1;
            $(this).data("idx", idx);
            var $item = $($keys[idx]);
            $item.addClass("active");
            $(this).val($item.text());
        } else {
            $(this).data("idx", "-99");
            searchKeys();
        }
    });
    var skLock = false;
    var lastKey = "", lastData = 1;

    function closeSearchKeys() {
        $("#txtSearchKey").removeAttr("style");
        $(".search-keys").html("").hide();
    }

    //设置推荐搜索关键词
    function setSearchKeys(data) {
        var $box = $(".search-keys");
        if (data && data.length) {
            $box.html("");
            for (var i = 0; i < data.length; i++) {
                $box.append('<div class="item">' + data[i] + '</div>');
            }
            $("#txtSearchKey").attr("style", "border-bottom-left-radius: 0;");
            $box.show();
        } else {
            closeSearchKeys();
        }
        skLock = false;
    }

    //加载推荐搜索关键词
    function searchKeys() {
        var val = $("#txtSearchKey").val().trim();
        if (skLock) return;
        if (val == "") {
            lastKey = "";
            lastData = 1;
            closeSearchKeys();
            return;
        }
        if (val.indexOf(lastKey) > -1 && lastData == 0) {
            return;
        }
        skLock = true;
        $.post("/errorBook/searchKeys", {key: val, subjectId: errorBook.params.subject}, function (json) {
            if (json.status) {
                lastKey = val;
                lastData = json.data.length;
                setSearchKeys(json.data);
            } else {
                closeSearchKeys();
            }
        });
    }

    $(".search-keys").delegate(".item", "click", function () {
        $("#txtSearchKey").val($(this).text());
        Search();
    });
    //搜索框失去焦点
    $("#txtSearchKey").bind("blur", function () {
        setTimeout(closeSearchKeys, 200);
    });

    //下载
    $(".q-list").delegate(".span-dw", "click", function () {
        var id = $(this).parents(".q-item").data("id"),
            subject = $(this).parents(".q-item").data("subject");
        if (errorBook.dw.refresh(id, subject)) {
            errorBook.dw.refreshHtml();
            if ($(this).hasClass("dw-active")) {
                $(this).removeClass("dw-active").html('<i class="fa fa-plus"></i> 加入下载</span>');
            } else {
                $(this).addClass("dw-active").html('<i class="fa fa-times"></i> 移除下载</span>');
            }
        }
    });
    $(".dw-power-p").bind("click", function () {
        $(this).hide();
        $(".dw-body").show();
    });
    $(".dw-power-c").bind("click", function () {
        $(".dw-body").hide();
        $(".dw-power-p").show();
    });
    $(".dw-dowload").bind("click", function () {
        errorBook.dw.download();
    });
    $(".dw-reset").bind("click", function () {
        errorBook.dw.reset();
    });

    //更新页面标签
    function RefreshTags(id, tags) {
        $(".q-item").each(function (i, item) {
            if ($(item).data("id") == id) {
                var $tags = $(item).find(".d-tags");
                for (var i = 0; i < tags.length; i++) {
                    $tags.append("<div class='d-tag'>" + tags[i] + "</div>");
                }
                return false;
            }
        });
    }
    //分页控件
    $(".pagination").delegate("a", "click", function () {
        var num = parseInt($(this).data("num"));
        if (singer.isUndefined(num) || num < 1 || num == errorBook.params.index)
            return;
        errorBook.params.index = num;
        errorBook.load();
    });
    //筛选条件点击
    $(".sing").bind("click", function () {
        var open = $(this).data("open");
        $(".screen").find(".sing").data("open", 0).removeClass("active");
        $(".screen").find(".list").hide();
        if (!open) {
            $(this).data("open", 1).addClass("active").find(".list").show();
        }
    });
});