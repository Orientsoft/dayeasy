/**
 * Created by shoy on 2014/9/28.
 */
var logger = singer.getLogger("listCtrl"),
    $window = $(window),
    lazyLoadCount = 3,
    listCtrl = ["$scope", "$http", "$sce", function ($scope, $http, $sce) {
        //:Todo 学科
        $scope.sd = {range: 0, type: -1, keyword: '', subject_id: 0, page: 0, size: 10, stage: -1};
        $scope.pages = {};
        $scope.qtype = -1;//题型
        $scope.page = 0;//页数
        $scope.list = [];//列表数据
        $scope.totalCount = 0;
        $scope.showEmpty = false;//是否空提示
        $scope.showError = false; //数据加载失败
        $scope.loading = false;//是否正在加载
        $http.post('/question/init')
            .success(function (json) {
                $scope.sd.subjectId = json.subjectId;
                $scope.qTypes = json.types;
            });
        /**
         * 数据获取
         */
        var getData = function () {
                $http.post('/question/search', $scope.sd)
                    .success(function (json) {
                        $scope.pages = [];
                        $scope.showError = false;
                        $scope.loading = false;
                        if (json.status && json.data && json.data.length) {
                            $scope.showEmpty = false;
                            $scope.list = $scope.list.concat(json.data);
                            $scope.totalCount = json.count;
                            if (json.data.length == $scope.sd.size && ($scope.sd.page + 1) % lazyLoadCount > 0) {
                                $window.bind("scroll.lazyLoad", scrollLoad);
                            }
                            if (json.data.length < $scope.sd.size) {
                                $window.unbind("scroll.lazyLoad");
                            }
                            if ($scope.sd.page >= lazyLoadCount - 1 || json.data.length < $scope.sd.size) {
                                $scope.pages = singer.page({
                                    current: Math.floor($scope.sd.page / lazyLoadCount),
                                    size: $scope.sd.size * lazyLoadCount,
                                    total: json.count
                                });
                            }
                            setTimeout(singer.loadFormula, 120);
                        } else {
                            if (!$scope.list.length)
                                $scope.showEmpty = true;
                            $window.unbind("scroll.lazyLoad");
                            return false;
                        }
                    })
                    .error(function (json) {
                        $scope.loading = false;
                        if (!$scope.list.length)
                            $scope.showError = true;
                        $window.unbind("scroll.lazyLoad");
                        //logger.warn(json);
                    });
            },
            /**
             * 滚动加载
             * @returns {boolean}
             */
            scrollLoad = function () {
                var winH = $(window).height();
                var pageH = $(document).height();
                var scrollT = $(window).scrollTop(); //滚动条top
                var aa = (pageH - winH - scrollT) / winH;
                if (aa < 0.15 && !$scope.loading) {
                    if (($scope.sd.page + 1) % lazyLoadCount == 0) {
                        $window.unbind("scroll.lazyLoad");
                        return false;
                    }
                    $scope.loading = true;
                    $scope.$digest();
                    $scope.sd.page = $scope.sd.page + 1;
                    getData();
                }
            };
        $scope.formatNum = function (num) {
            return singer.formatNum(num);
        };

        $scope.page = function (page) {
            var newPage = (page - 1) * lazyLoadCount;
            if (page < 0 || $scope.sd.page == newPage) return false;
            $scope.sd.page = newPage;
            $scope.list = [];
            $scope.pages = [];
            $scope.showEmpty = false;
            $scope.loading = true;
            getData();
            return false;
        };

        /**
         * 选项字母集
         * @type {optionWords|*}
         */
        $scope.optionWords = singer.optionWords;

        $scope.ranges = [];
        $scope.otherRanges = function (range) {
            var ranges = [];
            for (var i = 0; i < singer.ranges.length; i++) {
                var item = singer.ranges[i];
                if (item.id != range)
                    ranges.push(item);
                $scope.ranges[item.id] = item.name;
            }
            return ranges;
        };
        $scope.updateRange = function (question, range) {
            $http.post("/question/range", {
                id: question.id,
                range: range
            }).success(function (json) {
                if (json.status) {
                    singer.msg(singer.msgArray.updateSuccess);
                    question.range = range;
                }
            });
            return false;
        };

        /**
         * 搜索回车事件
         * @param $event
         */
        $scope.keyPress = function ($event) {
            if ($event.keyCode == 13)
                $scope.search();
        };
        $(".d-range-item").bind("click", function () {
            var $t = $(this);
            if ($t.hasClass("active")) return false;
            $t.addClass("active").siblings("li").removeClass("active");
            $scope.sd.range = $(".d-range-item").index($t);
            $scope.pages = {};
            $scope.search();
        });
        /**
         * 获取缩略图
         * @param url
         * @param width
         * @param height
         * @returns {*}
         */
        $scope.makeThumb = function (url, width, height) {
            return singer.makeThumb(url, width, height);
        };
        $scope.remove = function (i, question) {
            singer.confirm("删除之后将不能找回，确认删除当前题目？", function () {
                $http.post("/question/remove", {
                    id: question.id
                }).success(function (json) {
                    if (json.status) {
                        if (--$scope.totalCount < 0) $scope.totalCount = 0;
                        $scope.list.splice(i, 1);
                    } else {
                        singer.msg(json.message);
                    }
                }).error(function () {
                });
            }, function () {
            });
        };
        /**
         * 显示答案
         * @param question
         */
        $scope.showAnswer = function (question, $event) {
            $event.stopPropagation();
            question.showAnswer = question.showMore = !(question.showAnswer || false);
            if (question.showAnswer) {
                setTimeout(singer.loadFormula, 120);
            }
            return false;
        };

        $scope.showImage = function (url, $event) {
            $event.stopPropagation();
            singer.showImage(url);
            return false;
        };
        $scope.trustHtml = function (html) {
            html = singer.formatText(html);
            return $sce.trustAsHtml(html);
        };
        $scope.bindQuestionBody = function (type, html) {
            var _type = singer.questionTypes[type];
            if (_type)
                html = '<b class="q-type" title="' + _type.name + '">' + _type.title + '</b>' + html;
            return $scope.trustHtml(html);
        };
        $scope.optionModel = function (options) {
            return singer.optionModel(options);
        };
        $scope.errorRate = function (answerCount, errorCount) {
            if (answerCount <= 0) return '0.0%';
            var rate = errorCount / answerCount;
            if (rate > 1) rate = 1;
            return Math.round(rate * 100.0).toFixed(1) + '%';
        };

        $scope.getAnswer = function (question) {
            return $scope.trustHtml(singer.getCorrectAnswers(question));
        };
        /**
         * 显示详情
         * @param question
         */
        $scope.showMore = function (question) {
            question.showMore = !(question.showMore || false);
        };
        /**
         * 搜索
         * @returns {boolean}
         */
        $scope.search = function () {
            if ($scope.loading) return false;
            $scope.sd.page = 0;
            $scope.list = [];
            $scope.showEmpty = false;
            $scope.showError = false;
            $scope.loading = true;
            $scope.pages = [];
            getData();
            return false;
        };
        $scope.search();
    }];
(function ($) {
    $(document)
        .delegate(".q-item", "mouseenter", function () {
            $(this).addClass("q-hover");
        })
        .delegate(".q-item", "mouseleave", function () {
            $(this).removeClass("q-hover");
        })
        .delegate(".q-share", "mouseenter", function () {
            $(this).addClass("share-hover");
        })
        .delegate(".q-share", "mouseleave", function () {
            $(this).removeClass("share-hover");
        })
        .delegate(".d-pager li a", "click", function () {
            return false;
        })
    ;
})(jQuery);