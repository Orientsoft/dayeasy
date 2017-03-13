/**
 * 在线出课
 * @type {*[]}
 */
var editController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = { index: 1, share: 0, key: "", isnull: false, err_msg: ""}; //模拟数据 index:选项卡,share:视频库中二级导航,key:搜索关键字,显示使用
        $scope.pages = { index: 1, max: 0, page: 5, size: 10, key: "" }; //分页数据 key:搜索关键字，交互使用
        $scope.data = { id: "", name: "", desc: "", tags: "", face: "", grade: 1 };
        $scope.sendC = { c_class: [], expire: "",startTime:"", all_class: [] }; //等待发布的课堂
        $scope.v_groups = []; //视频列表
        $scope.c_groups = []; //微检测列表
        $scope.checks = { data: [], tmp: [] }; //已选择视频、微检测
        $scope.contents = []; //已选择的视频、微检测  交互使用
        $scope.makeThumb = singer.makeThumb;
        $scope.grades = [
            {id: -1, name: '请选择年级'}
        ]; //学段年级

        var tags = singer.tags({ canEdit: true}),
            /**
             * 移除数组元素
             * @param arr
             * @param id
             * @param type
             * @returns {boolean}
             */
            removeArray = function (arr, id, type) {
                if (!singer.isArray(arr)) return false;
                var sort = 0,
                    temp = arr.concat(),
                    i;
                for (i = 0; i < arr.length; i++) {
                    if (temp[i].id == id && temp[i].type == type) {
                        sort = temp[i].sort;
                        arr.splice(i, 1);
                        break;
                    }
                }
                if (arr.length == 0) return false;
                for (i = 0; i < arr.length; i++) {
                    if (arr[i].sort > sort)
                        arr[i].sort -= 1;
                }
                //数组排序
                arr.sort(function (a, b) {
                    return a.sort > b.sort;
                });
            };

        //页面加载
        $(function ($) {
            //检测是否需要编辑 - 暂时不用
            if (!singer.isUndefined(singer.uri().crid)) {
                $http.get("/Home/Item?crid=" + singer.uri().crid).success(function (json) {
                    if (singer.isUndefined(json.status)) {
                        $scope.data.id = json.id;
                        $scope.data.name = json.name;
                        $scope.data.desc = json.desc;
                        $scope.data.tags = json.tags;
                        $scope.data.face = json.face;
                        if (singer.isArray(singer.json(json.tags)))
                            tags = singer.tags({ canEdit: true, data: singer.json(json.tags) });
                        $scope.checks.tmp = json.videos;
                        for (var i = 0; i < json.papers.length; i++) {
                            $scope.checks.tmp.push(json.papers[i]);
                        }
                        $scope.checks.data = $.extend(true, [], $scope.checks.tmp);
                        $scope.checks.data.sort(function (a, b) {
                            return a.sort > b.sort;
                        });
                    }
                });
            }
            //加载学段年级
            $http.get("/Home/Grade").success(function (json) {
                if (json.status) {
                    $scope.grades = $scope.grades.concat(json.data);
                    $scope.data.grade = -1;
                }
            });
        });

        /**
         * 视频列表
         * @param s
         */
        $scope.videos = function (s) {
            $scope.v_groups = [];
            //分享范围：0自己、1校内、
            if (s > -1 && s < 3 && $scope.info.share != s) {
                $scope.info.share = s;
                $scope.pages.index = 1;
                $scope.pages.max = 0;
                $scope.pages.key = "";
            }
            $scope.info.isnull = false;
            $http.get("/Home/Videos?share=" + $scope.info.share + "&index=" + $scope.pages.index + "&size=" + $scope.pages.size + "&key=" + $scope.pages.key).success(function (json) {
                $scope.info.isnull = !json.status || !json.data.groups.length || json.data.groups.length==0;
                if (json.status) {
                    $scope.v_groups = json.data.groups;
                    for (var i = 0; i < $scope.v_groups.length; i++) {
                        for (var j = 0; j < $scope.v_groups[i].videos.length; j++) {
                            var _v = $scope.v_groups[i].videos[j];
                            _v.points = (_v.points && _v.points.length)
                                ? singer.json(_v.points)
                                : [];
                            for (var k = 0; k < $scope.checks.tmp.length; k++) {
                                if (_v.id == $scope.checks.tmp[k].id)
                                    _v.checked = true;
                            }
                        }
                    }
                    var _max = Math.ceil(json.data.total / $scope.pages.size);
                    if ($scope.pages.max != _max) {
                        $scope.pages.max = _max;
                        PageHtml($scope);
                    }
                } else {
                    $scope.pages.max = 0;
                }
            }).error(function () {
                $scope.pages.max = 0;
            });
        };

        /**
         * 微检测
         * @param s
         */
        $scope.papers = function (s) {
            $scope.c_groups = [];
            //分享范围：0自己、1校内、
            if (s > -1 && s < 3 && $scope.info.share != s) {
                $scope.info.share = s;
                $scope.pages.index = 1;
                $scope.pages.max = 0;
                $scope.pages.key = "";
            }
            $scope.info.isnull = false;
            $http.get("/Home/Papers/?index=" + $scope.pages.index + "&size=" + $scope.pages.size + "&share=" + $scope.info.share + "&key=" + $scope.pages.key).success(function (json) {
                $scope.info.isnull = !json.status || !json.data.length || json.data.length == 0;
                if (json.status) {
                    for (var i = 0; i < json.data.length; i++) {
                        for (var j = 0; j < $scope.checks.tmp.length; j++) {
                            if (json.data[i].id == $scope.checks.tmp[j].id)
                                json.data[i].checked = true;
                        }
                    }
                    $scope.c_groups = json.data;
                } else {
                    $scope.pages.max = 0;
                }
            }).error(function () {
                $scope.pages.max = 0;
            });

        };

        /**
         * 勾选
         * @param v
         */
        $scope.check = function (v) {
            if (singer.isUndefined(v.checked))
                v.checked = true;
            else
                v.checked = !v.checked;
            if (v.checked) {
                var obj = {
                    id: v.id,
                    type: $scope.info.index,
                    points: v.points,
                    share: v.share,
                    sort: $scope.checks.tmp.length + 1
                };
                if ($scope.info.index == 2) {
                    obj.name = v.name;
                    obj.duration = v.duration;
                    obj.speaker = v.speaker;
                    obj.face = v.face;
                } else {
                    obj.title = v.title;
                    obj.s_time = v.s_time;
                    obj.grade_name = v.grade_name;
                }
                $scope.checks.tmp.push(obj);
            } else {
                removeArray($scope.checks.tmp, v.id, $scope.info.index);
            }
        };

        /**
         * 移除已选择的视频或微检测
         * @param id
         * @param type
         */
        var remove = function (id, type) {
            removeArray($scope.checks.data, id, type);
            $scope.checks.tmp = $scope.checks.data.concat();
        };

        /**
         * 分页
         * @param num
         * @param bind
         */
        $scope.cpage = function (num, bind) {
            if (num < 1 || num > $scope.pages.max || num == $scope.pages.index)
                return;
            $scope.pages.index = num;
            if (bind || num < $scope.pages_html.l || num > $scope.pages_html.r) {
                PageHtml($scope);
            }
            $scope.info.index == 2 ? $scope.videos() : $scope.papers();
        };

        //搜索
        $scope.search = function () {
            $scope.pages.key = $scope.info.key;
            $scope.info.index == 2 ? $scope.videos() : $scope.papers();
        };
        $scope.searchPress = function (e) {
            if (e && e.keyCode == 13)
                $scope.search();
        };

        //保存已勾选的视频和微检测
        $scope.copyChk = function () {
            $scope.checks.data = $scope.checks.tmp.concat();
            for (var i = 0; i < $scope.checks.data.length; i++) {
                $scope.checks.data[i].sort = i + 1;
            }
            $scope.barChg(1);
        };
        $scope.cansolChk = function () {
            $scope.checks.tmp = $.extend(true, [], $scope.checks.data);
            $scope.barChg(1);
        };

        //选项卡切换
        $scope.barChg = function (num) {
            if (num < 1 || num > 3 || $scope.info.index == num)
                return;
            $scope.info.share = 0;
            $scope.info.key = "";
            $scope.info.isnull = false;
            $scope.info.index = num;
            $scope.pages.index = 1;
            $scope.pages.key = "";

            if (num == 2)
                $scope.videos();
            else if (num == 3)
                $scope.papers();
        };

        //锁定按钮
        $scope.isLock = false;
        var OnLock = function(ref){
                $scope.isLock = true;
//                if(ref) $scope.$digest();
            },
            UnLock = function(ref){
                $scope.isLock = false;
//                if(ref) $scope.$digest();
            };

        //添加课堂验证
        $scope.verification = function () {
            if ($scope.data.name == "") {
                return "请填写课堂标题";
            }
            if ($scope.data.grade < 1)
                return "请选择适用年级";
            var img = $("#imgValues").find("input").val();
            if (typeof (img) != "undefined") {
                $scope.data.face = img;
            }
            if ($scope.data.face == "") {
                return "请上传课堂封面";
            }
            if ($scope.checks.data.length == 0) {
                return "请添加视频或微检测";
            }
            $scope.data.tags = singer.json(tags.get());
            $scope.contents = [];
            for (var i = 0; i < $scope.checks.data.length; i++) {
                var obj = $scope.checks.data[i];
                var _sort = i + 1;
                if (!singer.isUndefined(obj.sort) && obj.sort > 0)
                    _sort = obj.sort;
                $scope.contents.push({ id: obj.id, type: obj.type == 2 ? 0 : 1, sort: _sort });
            }
            return "";
        };

        //添加课堂
        $scope.save = function () {
            OnLock();
            var result = $scope.verification();
            if (result != "") {
                singer.msg(result);
                UnLock();
                return;
            }
            $http.post("/Home/AddVideoClass", { v: $scope.data, d: $scope.contents }).success(function (json) {
                if (json.status) {
                    singer.confirm("保存成功，是否继续出课？", function () {
                        window.location.href = window.location.href;
                    }, function () {
                        window.location.href = "/";
                    });
                } else {
                    singer.msg(json.message);
                }
                UnLock(true);
            }).error(function () {
                UnLock(true);
            });
        };

        //发布课堂弹出框
        $scope.sendOpen = function () {
            OnLock();
            var result = $scope.verification();
            if (result != "") {
                singer.msg(result);
                UnLock();
                return;
            }
            $scope.info.err_msg = "";
            ddShow();
            UnLock();
            if (!$scope.sendC.all_class.length) {
                $http.get("/Home/Classes").success(function (json) {
                    if (json.status) {
                        $scope.sendC.all_class = json.data;
                    } else {
                        $scope.info.err_msg = "未查询到任课班级";
                    }
                }).error(function () {});
            }
        };

        //班级复选框点击
        $scope.addClass = function (val) {
            if (!singer.isUndefined(val)) {
                for (var i = 0; i < $scope.sendC.c_class.length; i++) {
                    if ($scope.sendC.c_class[i] == val) {
                        $scope.sendC.c_class.splice(i, 1);
                        return;
                    }
                }
                $scope.sendC.c_class.push(val);
            }
        };

        //保存课堂并发布
        $scope.send = function () {
            OnLock();
            $scope.sendC.expire = $("#txtExpire").val();
            $scope.sendC.startTime = $("#txtStartTime").val();
            if ($scope.sendC.id == "") {
                $scope.info.err_msg = "系统参数错误，请刷新重试！";
                UnLock();
                return;
            }
            if ($scope.sendC.c_class.length == 0) {
                $scope.info.err_msg = "请选择发布班级";
                UnLock();
                return;
            }
            if ($scope.sendC.startTime == "") {
                $scope.info.err_msg = "请填写开始时间";
                UnLock();
                return;
            }
            if ($scope.sendC.expire == "") {
                $scope.info.err_msg = "请填写截止时间";
                UnLock();
                return;
            }
            var usedToWeb = $("#usedToWeb:checked").val();
            $http.post("/Home/SendVideoClass",
            {
                v: $scope.data, d: $scope.contents, classes: $scope.sendC.c_class.join(','),
                expire: $scope.sendC.expire, startTime:$scope.sendC.startTime, used2Web: usedToWeb
            }).success(function (json) {
                if (json.status) {
                    ddClose();
                    singer.confirm("发布成功，是否继续出课？", function () {
                        window.location.href = window.location.href;
                    }, function () {
                        window.location.href = "/";
                    });
                } else
                    $scope.info.err_msg = josn.message;
                UnLock();
            }).error(function () {
                UnLock();
            });
        };

        //拖拽排序
        $(".groups").dragsort({
            dragSelectorExclude: "div.remove",
            dragEnd: function () {
                $(".groups").find("li").each(function (i, item) {
                    var id = $(item).data("id"),
                        type = $(item).data("type"),
                        j;
                    for (j = 0; i < $scope.checks.data.length; j++) {
                        var d = $scope.checks.data[j];
                        if (d.id == id && d.type == type) {
                            d.sort = i + 1;
                            break;
                        }
                    }
                });
                $scope.checks.data.sort(function (a, b) {
                    return a.sort > b.sort;
                });
                $scope.$digest();
            }
        });
        $(".groups")
            .delegate(".j-remove", "click", function () {
                var $t = $(this),
                    $li = $t.parents("li"),
                    id = $li.data("id"),
                    type = $li.data("type");
                remove(id, type);
                $scope.$digest();
            })
        ;
    }
];