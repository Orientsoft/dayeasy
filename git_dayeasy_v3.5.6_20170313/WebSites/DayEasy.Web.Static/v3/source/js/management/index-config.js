/**
 * Created by epc on 2016/02/01.
 */
var indexConfig = (function ($, S) {
    var IC = {
        data: {},
        seed: 1000,
        maxSectionLen: 6,
        maxTabLen: 4,
        sources: [], //临时变量 当前图文集合
        sourceLen: 4,
        /**
         * 获取Id
         * @param tag
         */
        getSeed: function (tag) {
            tag = tag || "S";
            return tag + (++IC.seed);
        },
        /**
         * 初始化
         */
        init: function () {
            $.post("/operate/index-config/data", {}, function (json) {
                IC.data = json.data;
                IC.data.carousels = IC.data.carousels || [];
                IC.data.fixeds = IC.data.fixeds || [];
                IC.data.sections = IC.data.sections || [];

                $(".ic-carousel").find(".num").text(IC.data.carousels.length);
                $(".ic-fixed").find(".num").text(IC.data.fixeds.length);

                if (IC.data.sections && IC.data.sections.length) {
                    for (var i = 0; i < IC.data.sections.length; i++) {
                        var section = IC.data.sections[i];
                        section.id = IC.getSeed();
                        var tag = section.id + "T";
                        if (section.tabs && section.tabs.length) {
                            section.tabs[0].active = 1;
                            for (var j = 0; j < section.tabs.length; j++) {
                                section.tabs[j].id = IC.getSeed(tag);
                            }
                        }
                    }
                    var html = template("ic-section", {sections: IC.data.sections});
                    $(".ic-sections").append(html);
                }
            });
        },
        /**
         * 添加版块
         * @param $this
         */
        pushSection: function ($this) {
            var len = IC.data.sections && IC.data.sections.length ? IC.data.sections.length : 0;
            if (len >= IC.maxSectionLen) {
                S.msg("最多添加" + IC.maxSectionLen + "个版块");
                return;
            }
            if ((len + 1) >= IC.maxSectionLen) {
                $this.parent().hide();
            }
            var $box = $(".ic-sections");
            var sectionId = IC.getSeed();
            var tabId = IC.getSeed(sectionId + "T");
            var section = {
                id: sectionId,
                sources: [],
                tabs: [
                    {
                        id: tabId,
                        active: 1,
                        sources: [],
                        adverts: [],
                        groups: []
                    }
                ]
            };
            IC.data.sections.push(section);
            var param = {sections: []};
            param.sections.push(section);
            $box.append(template("ic-section", param));
        },
        /**
         * 移除版块
         * @param $this
         */
        removeSection: function ($this) {
            var $box = $this.parents(".is-item");
            var sectionId = $box.data("id");
            for (var i = 0; i < IC.data.sections.length; i++) {
                if (IC.data.sections[i].id == sectionId) {
                    IC.data.sections.splice(i, 1);
                    break;
                }
            }
            $box.parent().siblings("div").show();
            $box.remove();
        },
        /**
         * 添加选项卡
         * @param $this
         */
        pushTab: function ($this) {
            var $box = $this.parent().siblings(".is-tabs");
            var sectionId = $this.parents(".is-item").data("id");
            var section = 0;
            for (var i = 0; i < IC.data.sections.length; i++) {
                if (IC.data.sections[i].id == sectionId) {
                    section = IC.data.sections[i];
                    break;
                }
            }
            if (!section) {
                S.msg("没有查询到当前Tab，请刷新重试");
                return;
            }
            var len = section.tabs && section.tabs.length ? section.tabs.length : 0;
            if (len >= IC.maxTabLen) {
                S.msg("最多添加" + IC.maxTabLen + "个Tab");
                return;
            }
            if ((len + 1) >= IC.maxTabLen) {
                $this.parent().hide();
            }
            var tabId = IC.getSeed(sectionId + "T");
            var tab = {
                id: tabId,
                active: 0,
                sources: [],
                adverts: [],
                groups: []
            };
            section.tabs.push(tab);
            var param = {tabs: []};
            param.tabs.push(tab);
            $box.append(template("ic-tab", param));
        },
        /**
         * 移除选项卡
         * @param $this
         */
        removeTab: function ($this) {
            var $section = $this.parents(".is-item");
            var $tab = $this.parents(".is-tab");
            var sectionId = $section.data("id"),
                tabId = $tab.data("id");
            for (var i = 0; i < IC.data.sections.length; i++) {
                if (IC.data.sections[i].id == sectionId) {
                    var section = IC.data.sections[i];
                    for (var j = 0; j < section.tabs.length; j++) {
                        if (section.tabs[j].id == tabId) {
                            section.tabs.splice(j, 1);
                            break;
                        }
                    }
                    break;
                }
            }
            var $tabs = $tab.parent();
            $tabs.siblings("div").show();
            $tab.remove();
            var $list = $tabs.find(".is-tab");
            if ($list && $list.length) {
                $tabs.eq(0).addClass("ist-active").find(".is-content-box").removeClass("hide");
            }
        },
        /**
         * 添加圈子
         * @param $this
         */
        checkGroup: function ($this) {
            var $section = $this.parents(".is-item"),
                $tab = $this.parents(".is-tab");
            var title = $tab.find(".is-name").text(),
                sectionId = $section.data("id"),
                tabId = $tab.data("id");
            var tab = 0, i = 0;
            for (; i < IC.data.sections.length; i++) {
                if (IC.data.sections[i].id == sectionId) {
                    var section = IC.data.sections[i];
                    for (var j = 0; j < section.tabs.length; j++) {
                        if (section.tabs[j].id == tabId) {
                            tab = section.tabs[j];
                            break;
                        }
                    }
                    break;
                }
            }
            if (!tab) {
                S.msg("没有查询到当前Tab，请刷新重试");
                return;
            }
            var param = {groups: []};
            for (i = 0; i < tab.groups.length; i++) {
                var group = tab.groups[i];
                if (group.type == 0) {
                    param.groups.push(group.id);
                }
            }
            var html = template("ic-group", param);
            S.dialog({
                title: "添加圈子至" + title,
                content: html,
                okValue: "确定",
                cancelValue: "取消",
                ok: function () {
                    tab.groups = [];
                    $(".ic-group").find("input[name='txtGroup']").each(function (i, input) {
                        var val = $(input).val();
                        if (val && val.length) {
                            tab.groups.push({type: 0, id: val});
                        }
                    });
                    $tab.find(".group-num").text(tab.groups.length);
                },
                cancel: function () {
                }
            }).showModal();
        },
        /**
         * 选择图文广告
         * @param $this
         */
        checkAdver: function ($this) {
            var type = $this.parent().data("type");
            var sources = [], section = 0, tab = 0,
                i = 0, $num, $section, $tab;

            if (type == "section") {
                $section = $this.parents(".is-item");
                section = IC.getItemById(IC.data.sections, $section.data("id"));
                if (!section) {
                    S.msg("没有查询到当前版块，请刷新重试");
                    return;
                }
            }
            if (type == "tab" || type == "tabAdvert" || type == "tabGroup") {
                $section = $this.parents(".is-item");
                section = IC.getItemById(IC.data.sections, $section.data("id"));
                if (!section) {
                    S.msg("没有查询到当前版块，请刷新重试");
                    return;
                }
                $tab = $this.parents(".is-tab");
                tab = IC.getItemById(section.tabs, $tab.data("id"));
                if (!tab) {
                    S.msg("没有查询到当前Tab，请刷新重试");
                    return;
                }
            }

            switch (type) {
                case "carousel":
                    sources = IC.data.carousels;
                    IC.sourceLen = 8;
                    $num = $(".carousel-num");
                    break;
                case "fixed":
                    sources = IC.data.fixeds;
                    IC.sourceLen = 4;
                    $num = $(".fixed-num");
                    break;
                case "section":
                    sources = section.sources;
                    IC.sourceLen = 1;
                    $num = $section.find(".section-num");
                    break;
                case "tab":
                    sources = tab.sources;
                    IC.sourceLen = 1;
                    $num = $tab.find(".tab-num");
                    break;
                case "tabAdvert":
                    sources = tab.adverts;
                    IC.sourceLen = 4;
                    $num = $tab.find(".advert-num");
                    break;
                case "tabGroup":
                    sources = tab.groups;
                    IC.sourceLen = 4;
                    $num = $tab.find(".group-num");
                    break;
            }

            IC.sources = [];
            if (sources && sources.length) {
                for (i = 0; i < sources.length; i++) {
                    if (type != "tabGroup")
                        IC.sources.push({id: sources[i], name: ''});
                    if (type == "tabGroup" && sources[i].type == 1)
                        IC.sources.push({id: sources[i].id, name: ''});
                }
            }

            $.get("/operate/advert?partial=1", {}, function (res) {
                S.dialog({
                    title: "选择图文广告",
                    content: res,
                    okValue: "确定",
                    cancelValue: "取消",
                    ok: function () {
                        sources.splice(0, sources.length);
                        if (IC.sources && IC.sources.length) {
                            for (i = 0; i < IC.sources.length; i++) {
                                if (type == "tabGroup") {
                                    sources.push({id: IC.sources[i].id, type: 1});
                                } else {
                                    sources.push(IC.sources[i].id);
                                }
                            }
                        }
                        $num.text(sources.length);
                        if (type == "section") {
                            section.title = IC.sources[0].name;
                            $section.find(".section-name").text(IC.sources[0].name);
                        }
                        if (type == "tab") {
                            tab.title = IC.sources[0].name;
                            $tab.find(".tab-name").text(IC.sources[0].name);
                        }
                    },
                    cancel: function () {
                    },
                    width: 800,
                    height: 410
                }).showModal();
            });
        },
        /**
         * 根据id查询list单个对象
         * @param list
         * @param id
         */
        getItemById: function (list, id) {
            if (!list || !list.length || !id) return 0;
            for (var i = 0; i < list.length; i++) {
                if (id == list[i].id) return list[i];
            }
        },
        /**
         * 保存数据
         */
        save: function () {
            //基本验证
            if (!IC.data.carousels || !IC.data.carousels.length || IC.data.carousels.length > 8) {
                S.msg("轮播广告需要1-8条图文数据");
                return;
            }
            if (!IC.data.fixeds || IC.data.fixeds.length != 4) {
                S.msg("固定广告需要4条图文数据");
                return;
            }
            $.post("/operate/index-config/save", {data: S.json(IC.data)}, function (json) {
                if (!json.status) {
                    S.alert(json.message);
                    return;
                }
                S.msg("保存成功");	
            });
        }
    };
    return IC;
})(jQuery, SINGER);

(function ($, S, IC) {
    IC.init();

    $(".ic-section-box")
        .delegate(".ic-push", "click", function () {
            var $this = $(this);
            var type = $this.data("type");
            switch (type) {
                case "section":
                    IC.pushSection($this);
                    break;
                case "tab":
                    IC.pushTab($this);
                    break;
            }
        })
        .delegate(".ic-remove", "click", function () {
            var $this = $(this);
            var type = $this.parent().data("type");
            S.confirm("确认移除？", function () {
                switch (type) {
                    case "section":
                        IC.removeSection($this);
                        break;
                    case "tab":
                        IC.removeTab($this);
                        break;
                }
            });
        })
    /** 选项卡切换 **/
        .delegate(".is-name", "click", function () {
            var $this = $(this);
            var $tab = $this.parent(),
                $tabs = $this.parents(".is-tabs");

            $tabs.find(".is-tab").removeClass("ist-active");
            $tabs.find(".is-content-box").addClass("hide");
            $tab.addClass("ist-active");
            $tab.find(".is-content-box").removeClass("hide");
        });

    /** 选择图文 **/
    $(".ic-box").delegate(".ic-check", "click", function () {
        var $this = $(this);
        if ($this.data("group")) {
            IC.checkGroup($this);
            return;
        }
        IC.checkAdver($this);
    });

    $("#btnSave").bind("click", function () {
        IC.save();
    });

})(jQuery, SINGER, indexConfig);