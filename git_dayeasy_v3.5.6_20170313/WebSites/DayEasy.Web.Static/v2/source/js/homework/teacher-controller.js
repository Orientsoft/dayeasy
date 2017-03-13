/* 作业中心 - 教师端 */
var teacherController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = { loading: true, isnull: false, key: "", page: 1, size: 10 };
        $scope.pages = {};
        $scope.papers = [];
        //加载数据
        $scope.load = function () {
            $scope.info.loading = true;
            $scope.info.isnull = false;
            $scope.papers = [];
            $scope.pages = {};
            $http.post(
                "/work/teacher/papers",
                {
                    index: $scope.info.page,
                    size: $scope.info.size,
                    key: $scope.info.key
                }).success(function (json) {
                    $scope.info.loading = false;
                    if (json.status) {
                        $scope.papers = json.data;
                        $scope.pages = singer.page({
                            current: $scope.info.page - 1,
                            size: $scope.info.size,
                            total: json.count
                        });
                    }
                    if (!json.status || json.data.length == 0) {
                        $scope.info.isnull = true;
                    }
                }).error(function () {
                    $scope.info.loading = false;
                });
        };
        $(function ($) {
            $scope.load();
        });
        //搜索
        $scope.search = function () {
            $scope.info.page = 1;
            $scope.pages = {};
            $scope.load();
        };
        $scope.searchPress = function (e) {
            if (e && e.keyCode == 13)
                $scope.search();
        };
        //分页
        $scope.page = function (num) {
            if (num < 1 || num == $scope.info.page)
                return;
            $scope.info.page = num;
            $scope.load();
        };
        //时间格式化
        $scope.dFormat = function (time, start, status) {
            var _result = DateFormat(time);
            if (status == 16) {
                var _start = DateFormat(start);
                _result = _start + "（未开始） 至 " + _result;
            }
            return _result;
        };
        //修改截止时间 - 弹出层
        $scope.expire = function (p) {
            if (singer.isUndefined(p.is_edit_expire))
                p.is_edit_expire = true;
            else
                p.is_edit_expire = !p.is_edit_expire;
        };
        //修改截止时间
        $scope.saveExpire = function (p) {
            var dt = $("#txtEditExpire").val();
            if (dt.length < 6) {
                singer.msg("请选择截止时间");
                return;
            }
            $http.post("/work/teacher/UpdExpire/?batch=" + p.batch + "&expire=" + dt).success(function (json) {
                if (json.status) {
                    p.show_expire = dt;
                    p.is_edit_expire = false;
                    singer.msg("修改成功！");
                } else
                    singer.msg(json.message);
            }).error(function () {});
        };
        //放弃发布
        $scope.cancel = function (p) {
            var msg = "撤回后作业将永久消失，已提交的学生作答及已批阅的结果将作废。<br/>是否坚持撤回？";
            var d = dialog({
                title: "消息确认提示",
                backdropBackground: '#000',
                backdropOpacity: 0.3,
                content: msg,
                button: [
                    {
                        value: '取消',
                        callback: function () { },
                        autofocus: true
                    },
                    {
                        value: '确定',
                        callback: function () {
                            $http.post("/work/teacher/cancel", { batch: p.batch }).success(function (json) {
                                if (json.status) {
                                    singer.msg("操作成功，页面即将刷新！");
                                    setTimeout(function () {
                                        window.location.href = window.location.href;
                                    }, 2000);
                                } else {
                                    singer.msg(json.message);
                                }
                            });
                        }
                    }
                ]
            }).width(420).showModal();
        };
    }
];

/* 旧版本-应该不会用了-过段时间再删除*/
var workListController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = { index: 1, loading: true, isnull: false, key: "", page: 1, size: 10 };
        $scope.pages = {};
        $scope.papers = [];

        $scope.load = function () {
            $scope.info.loading = true;
            $scope.info.isnull = false;
            $scope.papers = [];
            $scope.pages = {};
            $http.post(
                "/work/teacher/papers",
                {
                    index: $scope.info.page,
                    size: $scope.info.size,
                    doing: ($scope.info.index == 1),
                    key: $scope.info.key
                }).success(function (json) {
                    $scope.info.loading = false;
                    if (json.status) {
                        $scope.papers = json.data;
                        $scope.pages = singer.page({
                            current: $scope.info.page - 1,
                            size: $scope.info.size,
                            total: json.count
                        });
                    }
                    if (!json.status || json.data.length == 0) {
                        $scope.info.isnull = true;
                    }
                }).error(function () {
                    $scope.info.loading = false;
                });
        };


        $(function ($) {
            //默认选中
            if (!singer.isUndefined(singer.uri().done))
                $scope.info.index = 2;

            $scope.load();
        });

        //分页
        $scope.page = function (num) {
            if (num < 1 || num == $scope.info.page)
                return;
            $scope.info.page = num;
            $scope.load();
        };

        $scope.search = function () {
            $scope.info.page = 1;
            $scope.pages = {};
            $scope.load();
        };
        $scope.searchPress = function (e) {
            if (e && e.keyCode == 13)
                $scope.search();
        };

        $scope.bar = function (no) {
            if (no < 1 || no > 2 || no == $scope.info.index)
                return;
            $scope.info.index = no;
            $scope.info.page = 1;
            $scope.papers = [];
            $scope.pages = {};
            $scope.load();
        };

        //修改截至时间
        $scope.expire = function (v, is_close) {
            if (is_close) {
                singer.confirm("确定关闭当前作业？", function () {
                    $scope.saveExpire(v, true);
                }, function () {
                });
            } else {
                if (singer.isUndefined(v.is_edit_expire))
                    v.is_edit_expire = true;
                else
                    v.is_edit_expire = !v.is_edit_expire;
            }
        };
        $scope.saveExpire = function (v, is_close) {
            var dt = "now"; //singer.formatDate(new Date(), "yyyy-MM-dd hh:mm");
            if (!is_close) {
                dt = $("#txtEditExpire").val();
            }
            if (dt != "now" && dt.length < 6) {
                singer.msg("请选择截止时间");
                return;
            }
            $http.post("/work/teacher/UpdExpire/?batch=" + v.batch + "&expire=" + dt).success(function (json) {
                if (json.status) {
                    if (dt == "now")
                        dt = singer.formatDate(new Date(), "yyyy-MM-dd hh:mm");
                    v.show_expire = dt;
                    v.is_edit_expire = false;
                    var _msg = "关闭成功！作业已移至<span class='tip-color'>已结束</span>咯,请刷新查看！";
                    if (!is_close)
                        _msg = "修改成功！作业已移至<span class='tip-color'>进行中</span>咯,请刷新查看！";
                    singer.msg(_msg);
                } else
                    singer.msg(json.message);
            }).error(function () {
                singer.msg("修改失败");
            });
        };

        //放弃发布
        $scope.cancel = function (v) {
            singer.confirm("确定放弃发布当前作业？", function () {
                $http.post("/work/teacher/cancel", { batch: v.batch }).success(function (json) {
                    if (json.status) {
                        singer.msg("操作成功，请刷新查看！")
                    } else {
                        singer.msg(json.message);
                    }
                });
            }, function () { });
        };

        //时间格式化
        $scope.dFormat = function (time, start, un_start) {
            var _result = DateFormat(time);
            if ($scope.info.index == 1) {
                var _start = DateFormat(start);
                _result = _start + (un_start ? "（未开始） 至 " : " 至 ") + _result;
            }
            return _result;
        }
    }
];
