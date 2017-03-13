/**
 * Created by epc on 2015/8/27.
 */
var reason = (function () {
    var R = {
        id: "",
        box: $(".reason-box"),
        templateLoaded: false,
        /**
         * 加载错因分析编辑界面
         * @param id
         * @param box
         * @param callback
         * @param existsCallBack
         * @param params {show_count(true):是否显示已有多少人分析}
         */
        load: function (id, box, callback, existsCallBack, params) {
            R.id = id;
            R.box = box || R.box;
            if (!R.templateLoaded) {
                singer.loadTemplate('reason-edit', function () {
                    R.templateLoaded = true;
                    R.getData(callback, existsCallBack, params);
                });
            } else {
                R.getData(callback, existsCallBack, params);
            }
        },
        getData: function (callback, existsCallBack, params) {
            $.post("/reason/load", {id: R.id}, function (data) {
                R.box.html(singer.render('reason-item', data));
                //个性配置
                if (params) {
                    if (!params.show_count) {
                        R.box.find(".s-count").remove();
                    }
                }
                if (data.tags && data.tags.length && data.content)
                    existsCallBack && singer.isFunction(existsCallBack) && existsCallBack.call(this);
                else R.init(callback);
            });
        },
        init: function (callback) {
            var $box = R.box;
            var tags = singer.tags({
                canEdit: true,
                max: 5,
                container: $box.find('.d-tags'),
                change: function (data) {
                    $box.find(".txt-reason-tag").val(data.join(' '));
                }
            });
            //推荐标签点击
            $box.find(".sys-tags span").bind("click", function () {
                tags.set($(this).text());
            });
            //分析内容
            $box.find(".txt-reason-content").bind("keyup", function () {
                var num = 140 - $(this).val().length;
                if (num < 0) num = 0;
                $box.find(".hight-num").text(num);
            });
            //提交
            $box.find(".btn-reason-submit").bind("click", function () {
                var tag = "",
                    _tags = [],
                    id = $box.find(".txt-reason-id").val().trim(),
                    content = $box.find(".txt-reason-content").val().trim();
                var $tag = $box.find(".txt-reason-tag");
                if (!singer.isUndefined($tag) && $tag.data("untag") == "1") {
                    tag = $tag.val().trim();
                    if (tag == "") {
                        singer.msg("请添加错因标签");
                        return;
                    }
                    var tags = tag.split(" ");
                    if (!tags || !tags.length) {
                        singer.msg("请添加错因标签");
                        return;
                    }
                    for (var i = 0; i < tags.length; i++) {
                        if (tags[i].trim() == "")
                            continue;
                        if (tags[i].length > 10) {
                            singer.msg("单个错因标签最多10个字符");
                            return;
                        }
                        var find = false;
                        for (var j = 0; j < _tags.length; j++) {
                            if (tags[i] == _tags[j]) {
                                find = true;
                                break;
                            }
                        }
                        if (!find) _tags.push(tags[i].replace(/'/g, ""));
                    }
                    if (!_tags.length) {
                        singer.msg("请添加错因标签");
                        return;
                    }
                    if (_tags.length > 5) {
                        singer.msg("最多添加5个错因标签");
                        return;
                    }
                    tag = singer.json(_tags);
                }
                if (tag == "" && content == "") {
                    singer.msg("请填写简短分析");
                    return;
                }
                if (content.length > 140) {
                    singer.msg("简短分析最多140个字符");
                    return;
                }
                $.post("/reason/add", {
                    id: id,
                    content: encodeURIComponent(content),
                    tag: encodeURIComponent(tag),
                    isOpen: true
                }, function (json) {
                    if (json.status) {
                        singer.msg("保存成功", 2000, function () {
                            if (callback && singer.isFunction(callback)) {
                                callback.call(this, _tags, content);
                            } else {
                                window.location.reload();
                            }
                        });
                    } else {
                        singer.msg(json.message);
                    }
                });

            });
        }
    };
    return R;
})();