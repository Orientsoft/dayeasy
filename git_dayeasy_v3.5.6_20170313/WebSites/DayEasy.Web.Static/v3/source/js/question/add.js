/**
 * Created by shoy on 2014/9/19.
 */
var logger = singer.getLogger("addCtrl"),
    /**
     * 添加题目控制器
     * @type {*[]}
     */
    addCtrl = ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
        $scope.qt = {stage: '', type: '', range: 0, subject_id: '', points: [], tags: [], difficulty: 3};
        $scope.qType = -1;//题型
        $scope.qTypes = [
            {id: -1, name: '请选择题型'}
        ];//科目支持的题型
        $scope.editorText = "";//快速出题内容
        $scope.step = 1;//步骤
        $scope.tagState = false;//添加标签的状态
        $scope.typeTags = [];//可用标签
        $scope.points = [];//知识点
        $scope.saving = false;//保存状态
        $scope.ranges = singer.ranges.concat();

        var currentUploadItem,
            currentType,//当前题型
            loader = (function () {
                $http.post('/question/init')
                    .success(function (json) {
                        $scope.qt.subjectId = json.subjectId;
                        $scope.stages = json.stages;
                        $scope.qTypes = $scope.qTypes.concat(json.types);
                        $scope.qType = -1;
                        //学段
                        if ($scope.stages.length == 1) {
                            $scope.qt.stage = $scope.stages[0].id;
                        }
                    });
                //标签
                singer.tags({
                    canEdit: true,
                    max: 5,
                    change: function (tags) {
                        $scope.qt.tags = tags;
                        $scope.$digest();
                    }
                });
            })();

        /**
         * 标签回车事件
         */
        $(".b-tag").keypress(function (e) {
            if (e.keyCode == 13)
                $scope.addTag();
        });
        $scope.clearEditor = function () {
            $scope.editorText = "";
        };
        var starUrl = singer.sites.static + '/plugs/raty/images/';
        /**
         * 难度系数
         */
        $(".q-star").raty({
            path: starUrl,
            score: $scope.qt.difficulty,
            hints: ['容易', '普通', '一般', '困难', '极难'],
            click: function (score, event) {
                $scope.qt.difficulty = score;
            }
        });

        $(window).bind("beforeunload.question", function () {
            return '离开当前页面，数据将不会被保存，是否继续？';
        });
        /**
         * 设置步骤
         * @param step
         * @returns {boolean}
         */
        $scope.setStep = function (step) {
            var set = function () {
                if (step == 2) {
                    if ($scope.questionForm.stage.$invalid
                        || $scope.questionForm.qType.$invalid
                        || $scope.qType < 0
                        || !$scope.qt.knowledges
                        || $scope.qt.knowledges.length < 1) {
                        return false;
                    }
                    $scope.qt.type = $scope.qType;
                    logger.info($scope.qt);
                    for (var i = 0; i < $scope.qTypes.length; i++) {
                        if ($scope.qTypes[i].id == $scope.qType) {
                            currentType = $scope.qTypes[i];
                            $scope.typeTags = singer.getTags(currentType.style || 0);
                            break;
                        }
                    }
                }
                if (step == 3) {
                    if (!$scope.editorText)
                        return false;
                    $scope.question = singer.recognition($("#q-editor").val(), $scope.typeTags);
                    logger.info($scope.question);
                    setTimeout(function () {
                        $("body,html").animate({scrollTop: $(".step-03").offset().top}, "fast");
                        var $editors = $(".qth-content").find("textarea");
                        $editors.each(function (i, item) {
                            var th = item.scrollHeight + 12;
                            th = (th > 220 ? 220 : th);
                            $(item).css("height", th);
                        });
                    }, 200);
                }
                $scope.step = step;
            };
            if ($scope.step >= step) {
                if (step == 2) {
                    singer.confirm("你刚才切换了题型，是否初始化出题界面？<br/>如果不初始化出题界面可能导致界面不符合你出题型的要求...",
                        function () {
                            $scope.editorText = "";
                            $scope.question = {};
                            set();
                            $scope.$digest();
                        }, function () {
                            set();
                            $scope.$digest();
                        });
                } else if (step == 3) {
                    singer.confirm("你刚才重新点击了快速出题，是否确认重置题目详情界面？", function () {
                        set();
                        $scope.$digest();
                    }, function () {
                        return false;
                    });
                }
            } else {
                set();
            }
        };

        /**
         * 设置区域是否显示
         * @param tag
         * @returns {*}
         */
        $scope.showSection = function (tag) {
            return singer.inArray(singer.getTag(tag) || "", $scope.typeTags);
        };

        /**
         * 选项字母集
         * @type {optionWords|*}
         */
        $scope.optionWords = singer.optionWords;

        //singer.points({
        //    init: function () {
        //        return {
        //            subject_id: $scope.qt.subject_id,
        //            stage: $scope.qt.stage
        //        }
        //    },
        //    max: 5,
        //    change: function (json) {
        //        $scope.qt.knowledges = json;
        //        $scope.$digest();
        //    }
        //});
        var selector = $("#kPoints").tokenInput('/sys/knowledges?stage=', {
            method: "POST",
            queryParam: "keyword",
            hintText: "",
            noResultsText: "没有找到相关知识点",
            searchingText: "Searching...",
            placeholder: "输入知识点关键字",
            tokenLimit: 10,
            propertyToSearch: 'path',
            tokenFormatter: function (item) {
                var string = item['name'];
                return "<li><p>" + string + "</p></li>";
            },
            excludeCurrent: false,
            preventDuplicates: true,
            onAdd: function () {
                $scope.qt.knowledges = selector.tokenInput("get");
                $scope.$digest();
            },
            onDelete: function () {
                $scope.qt.knowledges = selector.tokenInput("get");
                $scope.$digest();
            }
        });

        /**
         * 添加小问或选项
         * @param data
         */
        $scope.add = function (data) {
            data = data || [];
            data.push({body: '', images: [], sort: data.length});
        };

        /**
         * 删除小问或选项
         * @param index
         * @param list
         * @returns {boolean}
         */
        $scope.del = function (index, list) {
            if (!singer.isArray(list)) return false;
            list.splice(index, 1);
            for (var i = 0; i < list.length; i++)
                list[i].sort = i;
        };

        /**
         * 设置正确答案
         * @param opt
         * @param opts
         */
        $scope.setCorrect = function (opt, opts) {
            //单选，多选判段
            if (!currentType.multi && !opt.isCorrect) {
                for (var i = 0; i < opts.length; i++) {
                    opts[i].isCorrect = false;
                }
            }
            opt.isCorrect = !opt.isCorrect;
        };

        /**
         * 上传图片
         * @param item
         * @returns {boolean}
         */
        $scope.uploadImg = function (item) {
            item = item || {};
            item.images = item.images || [];
            currentUploadItem = item;
            $(".webuploader-element-invisible").click();
        };
        /**
         * 获取缩略图
         * @param url
         * @param width
         * @param height
         * @returns {*}
         */
        $scope.getImageUrl = function (url, width, height) {
            return singer.makeThumb(url, width, height);
        };

        /**
         * 公式编辑
         * @param item
         */
        $scope.editText = function (item) {
            singer.textEditor(item, function () {
                $scope.$digest();
                setTimeout(singer.loadFormula, 120);
            });
        };

        $scope.trustHtml = function (html) {
            if (!html) return "";
            html = singer.formatText(html);
            return $sce.trustAsHtml(html);
        };

        /**
         * 保存
         */
        $scope.save = function () {
            if ($scope.saving) return false;
            var data = $.extend({}, $scope.question, $scope.qt);
            logger.info(data);
            if (!singer.checkQuestion(data, currentType)) return false;
            $scope.saving = true;
            //logger.info(data);
            $http.post('/question/save', data).success(function (json) {
                if (json.status) {
                    $(window).unbind("beforeunload.question");
                    singer.confirm("老师辛苦了，题已加入<span class='tip-color'>题库</span>啦！", function () {
                        location.href = '/question';
                    }, function () {
                        $scope.step = 1;
                        $scope.saving = false;
                        $scope.editorText = '';
                        $scope.$digest();
                    }, ["回到题库", "继续添加"]);
                } else {
                    singer.msg(json.message);
                    $scope.saving = false;
                }
            }).error(function (json) {
                singer.msg("网络请求异常，请稍后重试！");
                $scope.saving = false;
                console.log(json);
            });
        };
        $('select[name="stage"]').bind('change', function () {
            var stage = $(this).find('option:selected').val();
            $("#kPoints").data('settings').url = '/sys/knowledges?stage=' + stage;
            $scope.qt.knowledges = [];
            selector.tokenInput("clear");
        });

        /**
         * 图片上传处理
         */
        singer.uploader.on("uploadSuccess", function (file, response) {
            if (response.state) {
                currentUploadItem.images = response.urls;
                $scope.$digest();
            }
            singer.uploader.reset();
        });
        singer.uploader.on("uploadError", function (file) {
        });
    }];
(function ($, S) {

    $(document)
        .delegate('.q-tags li', 'click', function () {
            var str = '【' + $(this).html() + '】',
                obj = $('#q-editor').get(0);
            S.insertText(obj, str);
        })
        .delegate(".q-tag", "click", function () {
            return false;
        })
        .delegate(".step-text", "click", function () {
            var $parent = $(this).parents(".q-step");
            if (!$parent.hasClass("step-active")) return false;
            $("body,html").animate({scrollTop: $parent.offset().top}, "fast");
        })
//        .delegate(".b-clear", "click", function () {
//            $("#q-editor").val("");
//        })
        .delegate(".qth-editor", "mouseenter", function () {
            $(this).addClass("qth-editor-hover");
        })
        .delegate(".qth-editor", "mouseleave", function () {
            $(this).removeClass("qth-editor-hover");
        })
        .delegate(".image-view", "mouseenter", function () {
            $(this).addClass("image-hover");
        })
        .delegate(".image-view", "mouseleave", function () {
            $(this).removeClass("image-hover");
        })
        .delegate(".btn-return", "click", function () {
            location.href = "/question";
        })
        .delegate(".q-help li", "click", function () {
            $.get(singer.sites.static + "/data/question-helper.html", {
                    t: Math.random()
                },
                function (html) {
                    singer.dialog({
                        title: '快速出题 - 帮助文档',
                        content: html,
                        height: 420
                    }).showModal();
                }
            )
            ;
        });
    var $steps = $(".step-text"),
        setSteps = function (index) {
            var set = function (i) {
                var $step = $steps.eq(i);
                $step.data("opt", {
                    top: $step.offset().top,
                    left: $step.offset().left,
                    h: $step.height()
                });
            };
            if (singer.isNumber(index))
                set(index);
            else {
                for (var i = 0; i < $steps.length; i++) {
                    set(i);
                }
            }
        };
    $(function () {
        setSteps();
    });
    var paddingBottom = 5;
    $(window).bind("scroll.step", function () {
        var $step,
            top = $(this).scrollTop(),
            opt;
        for (var i = 0; i < $steps.length; i++) {
            $step = $steps.eq(i);
            opt = $step.data("opt");
            if (top >= opt.top - i * (opt.h + paddingBottom)) {
                if (!$step.data("fixed")) {
                    $step.css({"position": "fixed", top: (opt.h + paddingBottom) * i, left: opt.left});
                    $step.data("fixed", true);
                }
            } else {
                if ($step.data("fixed")) {
                    $step.css({"position": "relative", top: 0, left: 0});
                    $step.data("fixed", false);
                } else {
                    setSteps(i);
                }
            }
        }
    });
})(jQuery, singer);