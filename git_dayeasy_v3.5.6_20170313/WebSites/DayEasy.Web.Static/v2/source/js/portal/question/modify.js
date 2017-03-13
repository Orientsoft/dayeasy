/**
 * Created by shoy on 2014/10/1.
 */
var logger = singer.getLogger("modifyCtrl"),
    modifyCtrl = ["$scope", "$http", "$sce", function ($scope, $http, $sce) {
        var updateMarking = false;
        $scope.qType = "";//题型
        $scope.tagState = false;//添加标签的状态
        $scope.typeTags = [];//可用标签
        $scope.points = [];//知识点
        $scope.saving = false;//保存状态
        var url = (singer.sites.main || '') + '/sys',
            currentUploadItem,
            currentType,//当前题型
            starUrl = singer.sites.static + '/plugs/raty/images/';
        /**
         * 选项字母集
         * @type {optionWords|*}
         */
        $scope.optionWords = singer.optionWords;

        var getId = function () {
            var reg = new RegExp("/modify/([0-9a-z]{32})", "gi");
            location.href.match(reg);
            return RegExp.$1;
        };
        $http.get("/question/load/" + getId())
            .success(function (json) {
                if (json.status) {
                    $scope.question = json.data;
                    $scope.question.tags = $scope.question.tags || [];
                    $scope.question.points = $scope.question.points || [];
                    $scope.question.details = $scope.question.details || [];
                    logger.info($scope.question);
                    load();
                }
                else
                    logger.warn(json);
            })
            .error(function (json) {
                logger.info(json);
            });
        var load = function () {
            /**
             * 难度系数
             */
            $(".q-star").raty({
                path: starUrl,
                score: $scope.question.difficulty,
                hints: ['容易', '普通', '一般', '困难', '极难'],
                click: function (score, event) {
                    $scope.question.difficulty = score;
                }
            });
            $scope.stages = singer.stages.concat().splice($scope.question.stage - 1, 1);
            $scope.question.stage = $scope.stages[0].id;

            $http.post(url + '/qtype/' + $scope.question.type).success(function (json) {
                //$scope.qTypes = $scope.qTypes || [];
                //$scope.qTypes.push(json);
                //currentType = json;
                $scope.qTypes = json;
                currentType = json[0];
                $scope.typeTags = singer.getTags(currentType.style || 0);
            });

            $scope.QTypeChg = function () {
                for (var i = 0; i < $scope.qTypes.length; i++) {
                    if ($scope.qTypes[i].id == $scope.question.type) {
                        currentType = $scope.qTypes[i];
                    }
                }
            };

            singer.tags({
                canEdit: true,
                max: 5,
                data: $scope.question.tags,
                change: function (tags) {
                    $scope.question.tags = tags;
                    $scope.$digest();
                }
            });

            /**
             * 显示知识点
             * @returns {boolean}
             */
            //singer.points({
            //    data: $scope.question.points,
            //    init: function () {
            //        return {
            //            subject_id: $scope.question.subject_id,
            //            stage: $scope.question.stage
            //        }
            //    },
            //    change: function (json) {
            //        $scope.question.points = json;
            //        $scope.$digest();
            //    }
            //});
            var selector = $("#kPoints").tokenInput($("#getKpUrl").val(), {
                method: "POST",
                queryParam: "kp",
                hintText: "",
                noResultsText: "没有找到相关知识点",
                searchingText: "Searching...",
                placeholder: "输入知识点关键字",
                tokenLimit: 5,
                excludeCurrent: false,
                preventDuplicates: true,
                prePopulate:$scope.question.points,
                onAdd: function () {
                    $scope.question.points = selector.tokenInput("get");
                    $scope.$digest();
                },
                onDelete: function () {
                    $scope.question.points = selector.tokenInput("get");
                    $scope.$digest();
                }
            });


            setTimeout(function () {
                var $editors = $(".qth-content").find("textarea");
                $editors.each(function (i, item) {
                    var th = item.scrollHeight + 12;
                    th = (th > 220 ? 220 : th);
                    $(item).css("height", th);
                });
                singer.loadFormula();
            }, 200);
        };
//        $(window).bind("beforeunload.question", function () {
//            return '离开当前页面，数据将不会被保存，是否继续？';
//        });

        /**
         * 设置区域是否显示
         * @param tag
         * @returns {*}
         */
        $scope.showSection = function (tag) {
            return singer.inArray(singer.getTag(tag) || "", $scope.typeTags);
        };

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
            if (!currentType.multi && !opt.is_correct) {
                for (var i = 0; i < opts.length; i++) {
                    opts[i].is_correct = false;
                }
            }
            opt.is_correct = !opt.is_correct;
            updateMarking = true;
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
        $scope.save = function (type) {
            if ($scope.saving) return false;
            var data = $scope.question;
            if (!singer.checkQuestion(data, currentType)) return false;
            var saveSubmit = function () {
                $scope.saving = true;
                $scope.saveAs = (type == "copy");
                logger.info(data);
                var method = "save";
                if (type === "copy")
                    method = "save-as";
                $http.post("/question/" + method, data)
                    .success(function (json) {
                        if (json.status) {
                            $(window).unbind("beforeunload.question");
                            singer.confirm("保存成功，是否返回列表？", function () {
                                location.href = '/question';
                            }, function () {
                                $scope.step = 1;
                                $scope.saving = false;
                                $scope.editorText = '';
                                $scope.$digest();
                            }, ["返回列表", "继续编辑"]);
                        } else {
                            singer.msg(json.message);
                            $scope.saving = false;
                        }
                    })
                    .error(function (json) {
                        singer.msg("网络请求异常，请稍后重试！");
                        $scope.saving = false;
                    });
            };
            if (updateMarking) {
                singer.confirm("即将重新设置题目正确答案，保存将会重新批阅该题目未完成阅卷的试卷", function () {
                    saveSubmit();
                }, function () {
                });
            } else {
                saveSubmit();
            }
        };

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
        .delegate(".q-tag", "click", function () {
            return false;
        })
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
            return false;
        })
    ;
})(jQuery, SINGER);