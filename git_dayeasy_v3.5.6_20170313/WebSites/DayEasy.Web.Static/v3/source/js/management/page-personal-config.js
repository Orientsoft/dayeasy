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
            $.post("/operate/personal-config/data", {}, function (json) {
                IC.data = json.data;
                IC.data.teachers = IC.data.teachers || [];
                IC.data.students = IC.data.students || [];

                $(".teachers-num").html(IC.data.teachers.length);
                $(".students-num").html(IC.data.students.length);
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
                case "teachers":
                    sources = IC.data.teachers;
                    IC.sourceLen = 6;
                    $num = $(".teachers-num");
                    break;
                case "students":
                    sources = IC.data.students;
                    IC.sourceLen = 6;
                    $num = $(".students-num");
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
            if ((IC.data.teachers && IC.data.teachers.length > 6)
                || (IC.data.students && IC.data.students.length > 6)) {
                callback && callback.call(null);
                S.msg("教师和学生模块需要3-9条图文数据");
                return;
            }
            $.post("/operate/personal-config/save", {data: S.json(IC.data)}, function (json) {
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