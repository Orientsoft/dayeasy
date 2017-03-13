var agencyTypes = [],
getTypeName = function(num) {
    if(agencyTypes && agencyTypes.length){
        for(var i=0;i<agencyTypes.length;i++){
            if(agencyTypes[i].id == num)
                return agencyTypes[i].name;
        }
    }
    return "";
};

var agencyController = ['$scope', '$http', function ($scope, $http) {
    var defAreas = [{id:-1,name:"请选择"}];
    $scope.areas = []; //省份
    $scope.citys = []; //城市
    $scope.countys = []; //区县
    $scope.subjects = []; //科目列表
    $scope.info = { index: 0, agree: false, stages: [] }; //辅助数据
    $scope.agency = {status: -1, name: '', realName: '',subject:-1, mobile:'', code: '', type: -1, address: '', photo: '', area: -1,province:-1,city:-1,county:-1, msg: '', stages: ""}; //交互数据
    $scope.stages = [{id: -1, name: "请选择"}];
    $scope.aTypes = [];
    //初始化
    $scope.load = function () {
        $http.post("/identity/agencyInfo/").success(function (json) {
            if (json.status) {
                $scope.agency = json.data;
                if ($scope.stages.length > 0) {
                    $scope.info.stages = singer.json($scope.agency.stages) || [];
                }
                $scope.info.index = (json.data.id && json.data.id.length) ? 3 : 1;
            } else
                $scope.info.index = 1;
            //地区
            $http.get("/identity/areas/0").success(function (json_area) {
                $scope.areas = defAreas.concat(json_area);
                $scope.citys = [];
                $scope.countys = [];
                if($scope.agency.city > -1)
                    $scope.areaChange(0,true);
            }).error(function () {});
            //学科
            $http.post("/Identity/Subjects/").success(function (json_subject) {
                $scope.subjects = json_subject;
            }).error(function () {});
        }).error(function () {});
    };

    //学段
    $scope.getStages = function () {
        $http.get("/identity/stages").success(function (json) {
            if (json.status)
                $scope.stages = json.data;
        });
    };

    $(function () {
        //机构类型
        $http.post("/identity/agencyTypes").success(function(json){
            agencyTypes = json;
            $scope.aTypes = json;
        });
        $scope.getStages();
        $scope.load();
    });

    //地区更改事件
    //t:0省份改变、1城市改变
    $scope.areaChange = function (t,init) {
        var code = $scope.agency.province;
        if (code < 1) return;
        for (var i = 0; i < $scope.areas.length; i++) {
            if ($scope.areas[i].id == code) {
                if (t == 0) {
                    //省份改变
                    var obj = $scope.areas[i];
                    if (typeof (obj.child) != "undefined" && obj.child.length > 0) {
                        $scope.citys = obj.child;
                    } else {
                        $http.get("/Identity/Areas/" + code).success(function (json) {
                            $scope.citys = defAreas.concat(json);
                            $scope.areas[i].child = defAreas.concat(json);
                            if(init && $scope.agency.county > 0){
                                $scope.areaChange(1,true);
                            }
                        });
                    }
                    if(!init){
                        $scope.agency.city = -1;
                        $scope.countys = [];
                        $scope.agency.county = -1;
                    }
                    return;
                } else {
                    //城市改变
                    code = $scope.agency.city;
                    if (code < 1) return;
                    for (var j = 0; j < $scope.areas[i].child.length; j++) {
                        if ($scope.areas[i].child[j].id == code) {
                            var obj = $scope.areas[i].child[j];
                            if (typeof (obj.child) != "undefined") {
                                $scope.countys = obj.child;
                            } else {
                                $http.get("/Identity/Areas/" + code).success(function (json) {
                                    $scope.countys = defAreas.concat(json);
                                    $scope.areas[i].child[j].child = defAreas.concat(json);
                                });
                            }
                            if(!init) $scope.agency.county = -1;
                            return;
                        }
                    }
                }
                return;
            }
        }
    };

    //下一步
    $scope.next = function () {
        if (!$scope.agency.name || !$scope.agency.name.length) {
            singer.msg("请填写机构名称");
            return;
        }
        if ($scope.agency.type == -1) {
            singer.msg("请选择机构类型");
            return;
        }

        $scope.info.stages = [];
        var _stages = $("input[name='cbxStage']");

        _stages.each(function (i, s) {
            if (s.checked) {
                for (var j = 0; j < $scope.stages.length; j++) {
                    if ($scope.stages[j].id == $(s).val()) {
                        $scope.info.stages.push({id: $scope.stages[j].id, name: $scope.stages[j].name});
                    }
                }
            }
        });

        if ($scope.info.stages.length == 0) {
            singer.msg("请选择学段");
            return;
        }
        $scope.agency.stages = singer.json($scope.info.stages);
        $scope.info.index = 2;
        $(".step-2").show();
        $(window).resize();
        window.autoHeight && window.autoHeight.set();
    };

    //提交
    $scope.submit = function () {
        if (!$scope.agency.address
            || $scope.agency.address.length < 1
            || $scope.agency.province == -1
            || ($scope.agency.city == -1 && $scope.agency.county == -1)) {
            singer.msg("选择并填写详细的机构地址");
            return;
        }
        if (!$scope.agency.realName || !/^[\u4E00-\u9FA5]{2,4}$/.test($scope.agency.realName)) {
            singer.msg("真实姓名为2-4个汉字");
            return;
        }
        if ($scope.agency.subject == -1) {
            singer.msg("请选择学科");
            return;
        }
        if (!$scope.info.agree) {
            singer.msg("请先阅读《得一平台使用条款》并同意使用");
            return;
        }

        $scope.agency.area = $scope.agency.county > 0 ? $scope.agency.county : $scope.agency.city;
        $http.post("/Identity/Submit/", $scope.agency).success(function (json) {
            if (json.status) {
                $scope.info.index = 3;
                $scope.agency.status = 1;
                $(".step-2").hide();
            } else {
                singer.msg(json.message);
            }
        }).error(function () {
            singer.msg("提交失败");
        });

    };

    //返回第一步
    $scope.reset = function () {
        $scope.info.index = 1;
        $(".step-2").hide();
    };

    //检测学段ID是否在数组中，用于页面学段复选框默认是否选中
    $scope.inStage = function (id) {
        if (!singer.isNull(id) && $scope.info.stages.length) {
            for (var i = 0; i < $scope.info.stages.length; i++) {
                if ($scope.info.stages[i].id == id)
                    return true;
            }
        }
        return false;
    };

    //更换证件照 - 已申请过，被拒绝
    $scope.autoUploadPhoto = function () {
        $(".webuploader-element-invisible").click();
    };

    /**
     * 图片上传处理
     */
    singer.uploader.on("uploadSuccess", function (file, response) {
        console.log($scope.agency.photo);
        if (response.state) {
            $scope.agency.photo = response.urls[0];
            $scope.$digest();
        }
        console.log($scope.agency.photo);
        singer.uploader.reset();
    });
    singer.uploader.on("uploadError", function (file) {});

}];

var teacherController = ['$scope', '$http', function ($scope, $http) {
    $scope.subjects = []; //科目列表
    $scope.agencys = { is_null: false, data: []}; //机构列表
    $scope.pages = { index: 1, max: 0, page: 5, size: 6, total:0 }; //机构列表分页
    $scope.agency = { id: '', name: '', photo: '', type: -1 }; //当前选择机构
    $scope.info = { index: 0, search_key: "", agency_name: "", err_msg: ""}; //辅助数据
    $scope.teacher = {  name: "", status: -1, subject: -1, agency: -1, msg: "" }; //交互数据

    $(function(){
        //机构类型
        $http.post("/identity/agencyTypes").success(function(json){
            agencyTypes = json;
        });

        $http.post("/identity/relation",{t:1}).success(function (json) {
            $scope.info.index = 1;
            if (json.status) {
                $scope.teacher = json.data;
                if(json.data.status != -1)
                    $scope.info.index = 3;
            }
            $http.post("/Identity/Subjects/").success(function (json_subject) {
                $scope.subjects = json_subject;
            }).error(function () {});
        }).error(function () {});
    });

    //下一步
    $scope.next = function () {
        if(!$scope.teacher.name || !/^[\u4E00-\u9FA5]{2,4}$/.test($scope.teacher.name)){
            singer.msg("真实姓名为2-4个汉字");
            return;
        }
        if ($scope.teacher.subject == -1){
            singer.msg("请选择学科")
            return;
        }
        $scope.info.index = 2;
    };

    //搜索
    $scope.search = function () {
        if ($scope.info.search_key.length < 1) {
            singer.msg("请输入机构名称或ID");
            return;
        }
        $scope.agencys.is_null = false;
        $http.post("/Identity/SearchAgency", {
            index: $scope.pages.index,
            key: encodeURIComponent($scope.info.search_key)
        }).success(function (json) {
            if(!json.status || !json.data || !json.data.length)
                $scope.agencys.is_null = true;
            if (json.status) {
                $scope.agencys.data = json.data;
                var _max = Math.ceil(json.count / $scope.pages.size);
                if ($scope.pages.max != _max) {
                    $scope.pages.max = _max;
                    PageHtml($scope);
                }
            }
        });
    };
    $scope.searchPress = function (e) {
        if (e && e.keyCode == 13)
            $scope.search();
    };

    //选择机构，弹出模版
    $scope.signInModel = function (agency) {
        $scope.agency = agency;
        $scope.teacher.agency = $scope.agency.id;
        $scope.info.err_msg = "";
        ddShow();
    };

    //加入机构
    $scope.signInAgency = function () {
        $scope.info.err_msg = "";
        if ($scope.teacher.agency.length < 1
            || $scope.teacher.name.length < 1
            || $scope.teacher.subject < 1
            || $scope.teacher.msg.length < 1)
            return;
        $http.post("/Identity/SignAgency", $scope.teacher).success(function (json) {
            if (json.status) {
                ddClose();
                $scope.teacher.status = 1;
                $scope.info.index = 3;
            } else {
                $scope.info.err_msg = json.message;
            }
        }).error(function () {});
    };

    $scope.reset = function () {
        $scope.info.index = 1;
    };

    //分页
    $scope.cpage = function (num, bind) {
        if (num < 1 || num > $scope.pages.max || num == $scope.pages.index)
            return;
        $scope.pages.index = num;
        if (bind || num < $scope.pages_html.l || num > $scope.pages_html.r) {
            PageHtml($scope);
        }
        $scope.search();
    };

    //学科名称
    $scope.subjectName = function(id){
      if($scope.subjects && $scope.subjects.length) {
          for (var i = 0; i < $scope.subjects.length; i++) {
              if ($scope.subjects[i].id == id)
                  return $scope.subjects[i].name;
          }
      }
        return "";
    };

    //机构类型
    $scope.typeName = function (num) {
        return getTypeName(num);
    }

}];

var studentController = ['$scope', '$http', function ($scope, $http) {
    $scope.info = { index : 0,class_code:'',class_name:'',student_id: -1,lock: false,bind_text:'绑定'};
    $scope.students = []; //班级内未绑定的学生名单

    $(function(){
        $http.post("/identity/checkRole").success(function(json){
            if(json.status){
                $scope.info.index = 1;
            }else{
                window.location.href = DEYI.sites.main;
                return;
            }
        });
    });

    $scope.next = function () {
        if(!$scope.info.class_code || !$scope.info.class_code.trim().length){
            singer.msg("请输入班级码");
            return;
        }
        $http.post("/identity/checkClassCode",{
            code:$scope.info.class_code.trim()
        }).success(function(json){
            if(json.status){
                $http.post("/identity/Students",{
                    code:$scope.info.class_code.trim()
                }).success(function(json1){
                    if(json1.status){
                        $scope.info.class_name = json.data.name;
                        $scope.students = json1.data;
                        $scope.info.index = 2;
                    }else{
                        singer.msg(json1.message);
                    }
                });
            }else{
                singer.msg(json.message);
            }
        });
    };

    $scope.set = function(stu){
      $scope.info.student_id = stu.user_id;
    };

    $scope.bind = function(){
        if($scope.info.student_id < 10){
            singer.msg("请选择你的姓名");
            return;
        }
        $scope.info.lock = true;
        $scope.info.bind_text = '处理中...';
        $http.post("/identity/bindStudent",{
            studentId: $scope.info.student_id
        }).success(function(json){
            if(json.status){
                $scope.info.index = 3;
            }else{
                $scope.info.lock = false;
                $scope.info.bind_text = '绑定';
                singer.msg(json.message);
            }
        });
    };
}];