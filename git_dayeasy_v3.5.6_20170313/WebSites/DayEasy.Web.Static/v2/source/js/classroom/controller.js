var classroomController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = { index: 1, type: 0, share: 0, key: "",keyword: "",page: 1,size: 10,grade: -1, isnull: false, err_msg: "" }; //模拟数据 index:选项卡,type:我的课堂中二级导航,share:视频库中二级导航,key:搜索关键字,显示使用
        $scope.pages = {};
        $scope.sendC = { id: "", name: "", c_class: [], expire: "",start_time: "", all_class: [] }; //等待发布的课堂
        $scope.stages = [];
        $scope.grades = [];
        $scope.c_groups = []; //课堂列表
        $scope.v_groups = []; //视频列表
        $scope.makeThumb = singer.makeThumb;

        //课堂列表
        $scope.load = function (t) {
            $scope.pages = {};
            $scope.c_groups = [];
            //课堂类型：0课堂夹、1进行中、2已结束
            if (t > -1 && t < 3 && $scope.info.type != t) {
                $scope.info.type = t;
                $scope.info.page = 1;
                $scope.info.keywork = "";
                $scope.pages = {};
            }

            $http.post("/Home/VideoClass",
                {
                    type: $scope.info.type,
                    index: $scope.info.page,
                    size: $scope.info.size,
                    key: $scope.info.keyword,
                    grade: $scope.info.grade
                }).success(function (json) {
                if (json.status) {
                    $scope.info.isnull = false;
                    $scope.c_groups = json.data.groups;
                    $scope.pages = singer.page({
                        current: $scope.info.page -1,
                        size: $scope.info.size,
                        total: json.data.total
                    });
                } else {
                    $scope.info.isnull = true;
                }
            }).error(function () {});
        };

        /**
         * 视频列表
         * @param s
         */
        $scope.videos = function (s) {
            $scope.v_groups = [];
            //分享范围：0自己、1校内
            if (s > -1 && s < 2 && $scope.info.share != s) {
                $scope.info.share = s;
                $scope.info.page = 1;
                $scope.info.keywork = "";
            }
            $scope.pages={};
            $http.post("/Home/Videos",
                {
                    share:$scope.info.share,
                    index:$scope.info.page,
                    size:$scope.info.size,
                    key:$scope.info.keyword,
                    grade:$scope.info.grade
                }).success(function (json) {
                if (json.status) {
                    $scope.info.isnull = false;
                    $scope.v_groups = json.data.groups;
                    for (var j = 0; j < $scope.v_groups.length; j++) {
                        for (var k = 0; k < $scope.v_groups[j].videos.length; k++) {
                            var v = $scope.v_groups[j].videos[k];
                            if (singer.isUndefined(v.tags))
                                v.tags = !v.tagids ? [] : singer.json(v.tagids);
                            v.points = (!v.points || !v.points.length) ? [] : singer.json(v.points);
                            v.e_data = $.extend(true, {}, v);
                            v.e_data.add_status = false;
                        }
                    }
                    if (json.data.total > $scope.info.size) {
                        $scope.pages = singer.page({
                            current: $scope.info.page - 1,
                            size: $scope.info.size,
                            total: json.data.total
                        });
                    }
                } else {
                    $scope.info.isnull = true;
                    $scope.pages = {};
                }
            }).error(function () {
                $scope.pages = {};
            });
        };

        $(function ($) {
            //默认选中
            if (!singer.isUndefined(singer.uri().video))
                $scope.info.index = 2; //选择视频
            if (!singer.isUndefined(singer.uri().done))
                $scope.info.type = 2; //已结束课堂

            //加载学段
            $http.get("/Home/Stage").success(function (json) {
                if (json.status)
                    $scope.stages = json.data;
            });
            //加载学段中的年级
            $http.get("/Home/Grade").success(function (json) {
//                if (json.status)
//                    $scope.grades = json.data;
                if(json.status){
                    $scope.grades = [];
                    for(var i=0;i<json.data.length;i++){
                        var _name = json.data[i].name,
                            item = {
                            id:json.data[i].id,
                            name:_name,
                            stage:(
                                _name.indexOf("小学")==0 ? "小学" : (
                                        _name.indexOf("初中")==0 ? "初中" : (
                                                _name.indexOf("高中")==0 ? "高中" : ""
                                            )
                                    )
                                )
                        };
                        $scope.grades.push(item);
                    }
                }
            });

            if ($scope.info.index == 1)
                $scope.load(); //初始化数据
            else
                $scope.videos(); //视频列表

            $(document)
                .delegate(".q-share", "mouseenter", function () {
                    $(this).addClass("share-hover");
                }).delegate(".q-share", "mouseleave", function () {
                    $(this).removeClass("share-hover");
                });
        });

        /**
         * 适用年级中文名称
         * @param g
         * @returns {string}
         */
        $scope.gradeName = function (g) {
            if (singer.isString(g))
                g = parseInt(g);
            switch (g) {
                case 0:
                    return "全部";
                case 1:
                    return "小学一年级";
                case 2:
                    return "小学二年级";
                case 3:
                    return "小学三年级";
                case 4:
                    return "小学四年级";
                case 5:
                    return "小学五年级";
                case 6:
                    return "小学六年级";
                case 7:
                    return "初中一年级";
                case 8:
                    return "初中二年级";
                case 9:
                    return "初中三年级";
                case 10:
                    return "高中一年级";
                case 11:
                    return "高中二年级";
                case 12:
                    return "高中三年级";
                default:
                    return "";
            }
        };

        $scope.showTime = function (jsonDate) {
            var reg = /Date\((\d+)\+/i;
            if (reg.test(jsonDate)) {
                var date = new Date(parseInt(RegExp.$1));
                return singer.formatDate(date, "yyyy-MM-dd hh:mm");
            }
            return jsonDate;
        };

        /**
         * 加载课堂详细,进行中、已结束 需要使用批次号来定位
         * @param v
         */
        $scope.detail = function (v) {
            var batch = v.batch,
                id = v.id;
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
                        if (singer.isUndefined(obj.is_edit_expire)) //修改截至时间开关
                            obj.is_edit_expire = false;
                        if (singer.isUndefined(obj.s_detail)) //展开缩进详细开关
                            obj.s_detail = v.state = true;
                        else
                            obj.s_detail = v.state = !obj.s_detail;
                        if (singer.isUndefined(obj.isnull_detail)) //没有子元素
                            obj.isnull_detail = false;

                        if (singer.isUndefined(obj.detail)) { //加载详细
                            obj.detail = [];
                            $http.post("/Home/VideoDetail", { crid: id, batch: batch }).success(function(json) {
                                if (json.status)
                                    obj.detail = json.data;
                                else
                                    obj.isnull_detail = true;
                            }).error(function() {});
                        }
                        return;
                    }
                }
            }
        };

        /**
         * 编辑视频
         * @param video
         */
        $scope.editV = function (video) {
            if (!$scope.stages.length || !$scope.grades.length) {
                singer.msg("学段年级未能加载，请刷新重试");
                return;
            }
            video.edit = !(video.edit || false);
            if (video.edit) {
                var $edit = $('.edit[data-id="' + video.id + '"]'),
                    $tags = $edit.find(".d-tags"),
                    $points = $edit.find(".d-points");
                /**
                 * 标签
                 */
                singer.tags({
                    container: $tags,
                    data: video.e_data.tags,
                    canEdit: true,
                    change: function (tags) {
                        video.e_data.tags = tags;
                    }
                });
                singer.points({
                    container: $points,
                    treeId: video.id,
                    url: '/kpoints',
                    data: video.e_data.points,
                    init: function () {
                        var _stage = $scope.stages[0].id;
                        if ($scope.stages.length > 1) {
                            var _grade = video.e_data.grade;
                            if (_grade < 7)
                                _stage = 1;
                            else if (_grade < 10)
                                _stage = 2;
                            else if (_grade < 13)
                                _stage = 3;
                        }
                        return {
                            subject_id: 5,
                            stage: _stage
                        }
                    },
                    change: function (points) {
                        video.e_data.points = points;
                    }
                });
            }
        };

        /**
        * 删除视频
         * @param video
        */
        $scope.deleteV = function (video) {
            singer.confirm("确认删除视频："+video.name, function() {
                $http.post("/Home/DeleteV", { id: video.id }).success(function (json) {
                    if (json.status) {
                        singer.alert("删除成功!", function() {
                            var url = window.location.href;
                            if (url.indexOf("video=") == -1) {
                                if (url.indexOf("?") > -1)
                                    url += "&video=1";
                                else
                                    url += "?video=1";
                            }
                            window.location.href = url;
                        });
                    } else {
                        singer.msg(json.message);
                    }
                });
            }, function() {});
        }

        /**
         * 编辑视频 - 保存
         * @param v
         */
        $scope.save = function (v) {
            if ($scope.info.share == 1)
                return; //校内视频不能编辑
            v.e_data.tagids = singer.json(v.e_data.tags.concat());
            v.e_data.points = singer.json(v.e_data.points);
            $http.post("/Home/Update", v.e_data).success(function (json) {
                v.e_data.points = singer.json(v.e_data.points);
                if (json.status) {
                    v.edit = false;
                    v.name = v.e_data.name;
                    v.speaker = v.e_data.speaker;
                    v.share = v.e_data.share;
                    v.points = v.e_data.points;
                    v.tags = v.e_data.tags.concat();
                    v.grade = v.e_data.grade;
                    v.grade_name = $scope.gradeName(v.grade);
                    v.edit = false;
                } else
                    singer.msg("修改失败");
            }).error(function () {});
        };

        /**
         * 打开子元素：播放视频、跳转到作业作答
         * @param d
         */
        $scope.open = function (d) {
            if (d.type == 0) {
                singer.play({
                    url: d.url,
                    height: 450
                });
            } else if (d.type == 1) {
                singer.open(singer.sites.main + '/paper/detail/' + d.source_id + '?app=classroom');
                //                window.open(singer.sites.main + '/paper/detail/' + d.source_id);
            }
        };

        /**
         * 视频库也要播放视频哦
         * @param url
         */
        $scope.videoPlay = function (url) {
            singer.play({
                url: url,
                height: 450
            });
        };

        /**
         * 已结束跳转到课堂完成情况
         * @param b
         * @param d
         */
        $scope.undo = function (b, d) {
            //视频
            var _url = "/Home/Undo?batch=" + b + "&content=" + d.id;
            //微检测
            if (d.type != 0) {
                //已结束
                if ($scope.info.type == 2) {
                    _url = singer.format("{0}/work/teacher/statistics-question?batch={1}&paper_id={2}&app=classroom",
                        singer.sites.main, b, d.source_id);
                }
                else
                    _url = singer.sites.main + "/Paper/Detail/" + d.source_id + "?app=classroom";
            }
            singer.open(_url);
        };

        /**
         * 删除课堂
         * @param c
         */
        $scope.deleteC = function (c) {
            if (confirm("确定删除课堂：" + c.name)) {
                $http.post("Home/Delete?vcId=" + c.id).success(function (json) {
                    if (json.status) {
                        singer.msg("删除成功");
                        window.location.reload(true);
                    } else {
                        singer.msg(json.message);
                    }
                }).error(function () {});
            }
        };

        //编辑课堂
        $scope.editC = function (id) {
            window.location.href = "/Home/Create?crid=" + id;
            return false;
        };

        //分页
        $scope.page = function (num) {
            if (num < 1 || num == $scope.info.page)
                return;
            $scope.info.page = num;
            $scope.info.index == 1 ? $scope.load() : $scope.videos();
        };

        //切换选项卡
        $scope.tab_c = function (num) {
            if (num < 1 || num > 2 || $scope.info.index == num)
                return;
            $scope.pages = {};
            $scope.info.index = num;
            $scope.info.page = 1;
            $scope.info.grade = -1;
            $("#grade").val("-1");
            num == 1 ? $scope.load(0) : $scope.videos(0);
        };

        //搜索
        $scope.search = function () {
            //if ($scope.info.key.length == 0) {
            //    singer.msg("请输入搜索关键字");
            //    return;
            //}
            $scope.pages = {};
            $scope.info.keyword = $scope.info.key;
            $scope.info.page = 1;
            $scope.info.grade = $("#grade").children("option:selected").val();
            $scope.info.index == 1 ? $scope.load() : $scope.videos();
        };
        $scope.searchPress = function (e) {
            if (e && e.keyCode == 13)
                $scope.search();
        };

        //发布课堂弹出框
        $scope.sendOpen = function (v) {
            $scope.sendC.id = v.id;
            $scope.sendC.name = v.name;
            $scope.info.err_msg = "";
            ddShow();
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

        //发布课堂
        $scope.send = function () {
            $scope.sendC.expire = $("#txtExpire").val();
            $scope.sendC.start_time =$("#txtStartTime").val();
            if ($scope.sendC.id == "") {
                $scope.info.err_msg = "系统参数错误，请刷新重试！ ";
                return;
            }
            if ($scope.sendC.c_class.length == 0) {
                $scope.info.err_msg = "请选择发布班级";
                return;
            }
            if ($scope.sendC.start_time == "") {
                $scope.info.err_msg = "请选择开始时间";
                return;
            }
            if ($scope.sendC.expire == "") {
                $scope.info.err_msg = "请选择截止时间";
                return;
            }
            var usedToWeb = $("#usedToWeb:checked").val();
            //var url = "/Home/Send/?source=" + $scope.sendC.id + "&classes=" + $scope.sendC.c_class.join(',') + "&expire=" + $scope.sendC.expire + "&used2Web=" + usedToWeb;

            $http.post("/Home/Send", {
                source: $scope.sendC.id,
                classes: $scope.sendC.c_class.join(','),
                startTime: $scope.sendC.start_time,
                expire: $scope.sendC.expire,
                used2Web: usedToWeb
            }).success(function (json) {
                if (json.status) {
//                    $scope.info.err_msg = "发布成功";
//                    ddClose();
                    $("#disable-body").remove();
                    $(".deyi-dialog").fadeOut();
                    singer.msg("发布成功！老师辛苦了，<span class='tip-color'>进行中</span>可查看哦~");
                } else
                    $scope.info.err_msg = json.message;
            }).error(function () {});

        };

        //修改课堂截至时间弹出框
        $scope.editExpire = function (v, is_close) {
            if (is_close) {
                singer.confirm("确定关闭当前课堂？", function () {
                    $scope.saveExpire(v, true);
                }, function () {});
            } else {
                if (singer.isUndefined(v.is_edit_expire))
                    v.is_edit_expire = true;
                else
                    v.is_edit_expire = !v.is_edit_expire;
            }
        };

        //修改课堂截至时间
        $scope.saveExpire = function (v, is_close) {
            var dt = "end";
            if (!is_close) {
                dt = $("#txtEditExpire").val();
            }
            $http.post("/Home/UpdateExpire/?batch=" + v.batch + "&expire=" + dt).success(function (json) {
                if (json.status) {
                    if (is_close)
                        dt = singer.formatDate(new Date(), "yyyy-MM-dd hh:mm");
                    v.se_time = dt;
                    v.is_edit_expire = false;
                    var _msg = "关闭成功！课堂已移至<span class='tip-color'>已结束</span>咯,请刷新查看！";
                    if(!is_close)
                        _msg = "修改成功！课堂已移至<span class='tip-color'>进行中</span>咯,请刷新查看！";
                    singer.msg(_msg);
                } else
                    singer.msg(json.message);
            }).error(function () {});
        };

        //放弃发布
        $scope.cancel = function(v){
            singer.confirm("确定放弃发布当前课堂？",function(){
                $http.post("/Home/CancelPublish",{batch: v.batch}).success(function(json){
                   if(json.status){
                       singer.msg("操作成功，请刷新查看！")
                   }else{
                       singer.msg(json.message);
                   }
                });
            },function(){});
        };

        //修改分享范围
        $scope.shareChg = function (v) {
            var s = v.share == 0 ? 1 : 0;
            $http.post("/Home/UpdShare?id=" + v.id + "&share=" + s).success(function (json) {
                if (json.status)
                    v.share = s;
            });
        }
    }
];
