var studentController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = { type: 1, key: "", keyword: "", isnull: false, subject_id: -1, page: 1, size: 10 };
        $scope.pages = {};
        $scope.c_groups = []; //课堂列表
        $scope.subjects = []; //科目列表
        $scope.makeThumb = singer.makeThumb;

        var urlPost = function (action, data, success) {
            $http.post(action, data).success(function (json) {
                success && singer.isFunction(success) && success.call(this, json);
            }).error(function () {
                singer.msg("网络请求异常！");
            });
        };

        //课堂列表
        $scope.load = function (t) {
            $scope.pages = {};
            $scope.c_groups = [];
            //课堂类型：1进行中、2已结束
            if (t > 0 && t < 3 && $scope.info.type != t) {
                $scope.info.type = t;
                $scope.info.page = 1;
                $scope.info.subject_id = -1;
                $scope.info.keyword = "";
            }

            $http.post("/Student/VideoClass",
                {
                    type: $scope.info.type,
                    index: $scope.info.page,
                    size: $scope.info.size,
                    key: $scope.info.keyword,
                    subject: ($scope.info.subject_id > 0 ? $scope.info.subject_id : -1)
                }).success(function (json) {
                    if (json.status) {
                        $scope.info.isnull = false;
                        $scope.c_groups = json.data.groups;
                        $scope.pages = singer.page({
                            current: $scope.info.page - 1,
                            size: $scope.info.size,
                            total: json.data.total
                        });
                    } else {
                        $scope.info.isnull = true;
                    }
                }).error(function () {
                    alert("查询失败");
                });
        }

        $(function ($) {
            $.post("/student/subjects", { "func": "getNameAndTime" }, function (json) {
                if (json.status)
                    $scope.subjects = json.data;
            }, "json");
            $scope.load(); //初始化
        });

        //加载课堂详细,进行中、已结束 需要使用批次号来定位
        $scope.detail = function (batch, id) {
            for (var i = 0; i < $scope.c_groups.length; i++) {
                for (var j = 0; j < $scope.c_groups[i].v_classes.length; j++) {
                    if ((batch.length > 0 && $scope.c_groups[i].v_classes[j].batch == batch) || (batch.length < 1 && $scope.c_groups[i].v_classes[j].id == id)) {

                        var obj = $scope.c_groups[i].v_classes[j];

                        if (!obj.tags) //标签转换为数组
                            obj.tags = [];
                        if (singer.isString(obj.tags))
                            obj.tags = singer.json(obj.tags);

                        if (singer.isUndefined(obj.ss_time)) //开始时间 format
                            obj.ss_time = DateFormat(obj.star_time);
                        if (singer.isUndefined(obj.se_time)) //结束时间
                            obj.se_time = DateFormat(obj.expire);
                        if (singer.isUndefined(obj.s_time)) //创建时间
                            obj.s_time = DateFormat(obj.time);
                        if (singer.isUndefined(obj.s_detail)) //展开缩进详细开关
                            obj.s_detail = true;
                        else
                            obj.s_detail = !obj.s_detail;
                        if (singer.isUndefined(obj.isnull_detail)) //没有子元素
                            obj.isnull_detail = false;

                        if (singer.isUndefined(obj.detail)) { //加载详细
                            obj.detail = [];
                            $http.post("/student/detail", {crid:id,batch:batch}).success(function (json) {
                                if (json.status)
                                    obj.detail = json.data;
                                else
                                    obj.isnull_detail = true;
                            }).error(function () {
                                alert("课堂详情加载失败");
                            });
                        }
                        return;
                    }
                }
            }
        };

        //打开子元素：播放视频、跳转到作业作答
        $scope.open = function (batch, d) {
            if (d.type == 0) {
                //只有进行中的课堂，观看视频时才记录
                if ($scope.info.type == 1)
                    $http.post("/student/record", {batch: batch, id: d.id}).success(function () {
                    });
                singer.play({
                    url: d.url,
                    height: 450,
                    showQuestion: $scope.info.type == 1,
                    questionCallback: function (word) {
                        urlPost("/student/ask", {
                            batch: batch,
                            sourceId: d.source_id,
                            word: word
                        }, function (json) {
                            if (json.status) {
                                $(".v-question .input-group").append('<div class="v-question-success"><i class="fa fa-check"></i>提交成功</div>');
                                setTimeout(function () {
                                    $(".v-question-success").remove();
                                }, 2000);
                            }
                        });
                    }
                });
            } else if (d.type == 1) {
                window.open(singer.sites.main + '/work/student/answer/' + batch + '/' + d.source_id + "?app=classroom");
            }
        };

        //搜索
        $scope.search = function () {
            $scope.pages = {};
            $scope.info.keyword = $scope.info.key;
            $scope.info.page = 1;
            $scope.load();
        };
        $scope.searchPress = function (e) {
            if (e && e.keyCode == 13)
                $scope.search();
        }
        //分页
        $scope.page = function (num) {
            if (num < 1 || num == $scope.info.page)
                return;
            $scope.info.page = num;
            $scope.load();
        };
    }
];
