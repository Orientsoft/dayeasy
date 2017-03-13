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
            $.post("/operate/home-config/data", {}, function (json) {
                IC.data = json.data;
                $(".coverages-num").html(IC.data.coverages.length);
                $(".resources-num").html(IC.data.resources.length);
                if (IC.data.examples) {
                    $(".left-num").html(IC.data.examples.left.length);
                    $(".right-num").html(IC.data.examples.right.length);
                }
            });
        },
        /**
         * 选择图文广告
         * @param $this
         */
        checkAdver: function ($this) {
            var type = $this.parent().data("type");
            var sources = [], i = 0, $num;

            switch (type) {
                case "coverages":
                    sources = IC.data.coverages;
                    IC.sourceLen = 30;
                    $num = $(".coverages-num");
                    break;
                case "resources":
                    sources = IC.data.resources;
                    IC.sourceLen = 9;
                    $num = $(".resources-num");
                    break;
                case "left":
                    sources = IC.data.examples.left;
                    IC.sourceLen = 4;
                    $num = $(".left-num");
                    break;
                case "right":
                    sources = IC.data.examples.right;
                    IC.sourceLen = 1;
                    $num = $(".right-num");
                    break;
                default:
                    S.msg('类型无效');
                    return false;
            }

            IC.sources = [];
            if (sources && sources.length) {
                for (i = 0; i < sources.length; i++) {
                    IC.sources.push({id: sources[i], name: ''});
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
                                sources.push(IC.sources[i].id);
                            }
                        }
                        $num.text(sources.length);
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
        save: function (callback) {
            //基本验证
            if (!IC.data.resources || !IC.data.resources.length
                || IC.data.resources.length < 3 || IC.data.resources.length > 9) {
                S.msg("资源干货模块需要3-9条图文数据");
                callback && callback.call(null);
                return;
            }
            if (!IC.data.examples || IC.data.examples.left.length != 4) {
                S.msg("榜样左边模块需要4条图文数据");
                callback && callback.call(null);
                return;
            }
            if (!IC.data.examples || IC.data.examples.right.length <= 0) {
                S.msg("榜样力量右边需要1条图文数据");
                callback && callback.call(null);
                return;
            }
            $.post("/operate/home-config/save", {data: S.json(IC.data)}, function (json) {
                callback && callback.call(null);
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
        var $t = $(this);
        $t.disabled('稍后..');
        IC.save(function () {
            $t.unDisabled();
        });
    });

})(jQuery, SINGER, indexConfig);