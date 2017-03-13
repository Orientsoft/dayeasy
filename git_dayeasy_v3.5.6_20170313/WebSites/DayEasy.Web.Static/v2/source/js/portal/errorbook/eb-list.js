 /**错题库**/
var ebListController = ['$scope', '$http', '$sce', function ($scope, $http, $sce) {
    $scope.info = {index: 1, loading: true, isnull: false, key: ""};
    $scope.params = {index: 1, size: 10, subject: -1, type: -1, start: "", expire: "", key: "", source_type: 0};
    $scope.pages = {};
    $scope.optionWords = [];
    $scope.ebooks = []; //错题列表
    $scope.subjects = []; //学科
    $scope.q_types = []; //题型
    $scope.totalCount = 0;

    //加载错题列表
    $scope.load = function () {
        $scope.info.loading = true;
        $scope.info.isnull = false;
        $scope.ebooks = [];
        $http.post("/errorbook/questions", $scope.params).success(function (json) {
            if (!json.status || json.data.length == 0) {
                $scope.info.isnull = true;
            } else {
                $scope.ebooks = json.data;
                $scope.pages = singer.page({
                    current: $scope.params.index - 1,
                    size: $scope.params.size,
                    total: json.count
                });
                setTimeout(singer.loadFormula, 120);
                $scope.checkPushDowload();
            }
            $scope.totalCount = (json.status ? json.count : 0);
            $scope.info.loading = false;
        }).error(function () {
            $scope.info.loading = false;
        });
    };

    $(function () {
        $scope.optionWords = singer.optionWords;
        $scope.loadSubjects();
        //$scope.load();
    });

    //加载学生答案
    $scope.detail = function (ebook) {
        ebook.s_variant = false;
        if (singer.isUndefined(ebook.answers)) {
            $http.post(
                "/errorBook/answer",
                {
                    batch: ebook.batch,
                    paperId: ebook.paper_id,
                    questionId: ebook.question.question_id,
                    source_type: $scope.params.source_type
                }
            ).success(function (json) {
                    if (json.status)
                        ebook.answers = json.data;
                    ebook.isb = json.isb;
                    ebook.user_id = json.userId;
                }).error(function () {
                    singer.msg("加载失败");
                    ebook.answers = [];
                });
        }
        if (singer.isUndefined(ebook.s_detail))
            ebook.s_detail = true;
        else
            ebook.s_detail = !ebook.s_detail;
        setTimeout(singer.loadFormula, 120);
    };

    //加载变式题
    $scope.variant = function (ebook) {
        ebook.s_detail = false;
        ebook.loading = false;
        ebook.nodata = false;
        if (singer.isUndefined(ebook.variant)) {
            $http.get("/errorBook/variant?ebookId=" + ebook.id)
                .success(function (json) {
                    ebook.loading = false;
                    if (json.status) {
                        ebook.variant = json.data[0];
                        setTimeout(singer.loadFormula, 120);
                    } else {
                        //singer.msg(json.message);
                        ebook.nodata = true;
                    }
                }).error(function () {
                    singer.msg("加载失败");
                });

        }
        if (singer.isUndefined(ebook.s_variant)) {
            ebook.loading = true;
            ebook.s_variant = true;
        }
        else {
            ebook.s_variant = !ebook.s_variant;
            ebook.loading = false;
        }
    };

    //显隐变式题答案
    $scope.showVariantAnswer = function (variant) {
        variant.s_answer = !variant.s_answer;
    };

    //加载学科
    $scope.loadSubjects = function () {
        $http.get("/errorbook/subjects?source_type=" + $scope.params.source_type)
            .success(function (json) {
                $scope.subjects = json;
                $scope.params.subject = -1;
                $scope.loadQuType();
            });
    };

    //加载题型
    $scope.loadQuType = function () {
        $scope.params.type = -1;
        if (singer.isString($scope.params.subject))
            $scope.params.subject = parseInt($scope.params.subject);
        if ($scope.params.subject == -1) {
            $scope.q_types = [];
            $scope.search();
            return;
        }
        $scope.params.type = -1;
        for (var i = 0; i < $scope.subjects.length; i++) {
            if ($scope.subjects[i].id == $scope.params.subject) {
                if (singer.isUndefined($scope.subjects[i].q_types)) {
                    $http.get("/errorBook/questionType?subjectId=" + $scope.params.subject + "&source_type=" + $scope.params.source_type)
                        .success(function (json) {
                            $scope.subjects[i].q_types = json;
                            $scope.q_types = json;
                            $scope.search();
                        }).error(function () {
                            $scope.subjects[i].q_types = [];
                        });
                } else {
                    $scope.q_types = $scope.subjects[i].q_types;
                    $scope.search();
                }
                return;
            }
        }
    };

    //搜索
    $scope.search = function () {
        if (singer.isString($scope.params.subject))
            $scope.params.subject = parseInt($scope.params.subject);
        if (singer.isString($scope.params.type))
            $scope.params.type = parseInt($scope.params.type);
        $scope.params.start = $("#txtStar").val();
        $scope.params.expire = $("#txtExpire").val();
        $scope.params.index = 1;
        $scope.pages = {};
        $scope.load();
    };
    $scope.searchPress = function (e) {
        if (e && e.keyCode == 13)
            $scope.search();
    };

    //分页
    $scope.page = function (num) {
        if (num < 1 || $scope.params.index == num)
            return false;
        $scope.params.index = num;
        $scope.pages = {};
        $scope.load();
    };

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

    $scope.formatNum = function (num) {
        return singer.formatNum(num);
    };

    $scope.trustHtml = function (html) {
        if (!html)
            return "";
        html = singer.formatText(html);
        return $sce.trustAsHtml(html);
    };

    $scope.optionModel = function (options) {
        return singer.optionModel(options);
    };

    $scope.showImage = function (url, $event) {
        $event.stopPropagation();
        singer.showImage(url);
        return false;
    };

    $scope.getAnswer = function (question) {
        return $scope.trustHtml(singer.getCorrectAnswers(question));
    };
    $(".bar span").bind("click", function () {
        var $t = $(this),
            type = $t.data("type") || 0;
        if ($t.hasClass('active')) return false;
        $t.addClass('active').siblings().removeClass('active');
        $scope.params.source_type = type;
        $scope.loadSubjects();
    });

    /*错题下载*/
    $scope.dwShow = true;
    $scope.dwData = {subjects:[],list:[]};
    $scope.dwPush = function(ebook){
        if(singer.isUndefined(ebook.s_dowload)){
            ebook.s_dowload = true;
        }else{
            ebook.s_dowload = !ebook.s_dowload;
        }

        if(ebook.s_dowload){
            if($scope.dwData.list.length > 49){
                singer.msg("已经选择了很多错题啦，先下载一份吧~~");
                ebook.s_dowload = !ebook.s_dowload;
                return;
            }
            $scope.dwData.list.push(ebook.id);
            var find = false;
            for(var j=0;j<$scope.dwData.subjects.length;j++){
                if($scope.dwData.subjects[j].id == ebook.subject_id){
                    $scope.dwData.subjects[j].count += 1;
                    find = true;
                    break;
                }
            }
            if(!find)
                $scope.dwData.subjects.push({id:ebook.subject_id,name:ebook.subject_name,count:1});
        }else{
            var find = false;
            for(var i=0;i<$scope.dwData.list.length;i++){
                if($scope.dwData.list[i] == ebook.id){
                    $scope.dwData.list.splice(i,1);
                    find = true;
                    break;
                }
            }
            if(!find) return;
            for (var j = 0; j < $scope.dwData.subjects.length; j++) {
                if ($scope.dwData.subjects[j].id == ebook.subject_id) {
                    $scope.dwData.subjects[j].count -= 1;
                }
            }
        }
    };
    $scope.showDowload = function(){$scope.dwShow = !$scope.dwShow;};
    $scope.dwReset = function(){
        for(var i=0;i<$scope.ebooks.length;i++){
            $scope.ebooks[i].s_dowload = false;
        }
        $scope.dwData = {subjects:[],list:[]};
    };
    $scope.dwDowload = function(){
        if($scope.dwData.list.length < 1){
            singer.msg("请选择错题");
            return;
        }
        $("#txtDwData").val(encodeURIComponent(singer.json($scope.dwData.list)));
        $("#dwForm").submit();
        singer.dialog({
            title: "下载提示",
            content: "请问您下载成功了吗？<br/>确定下载成功将<span style='color:#ffab00'>清空</span>错题下载列表",
            fixed: true,
            backdropOpacity: .7,
            okValue: "是的",
            cancelValue: "还没有",
            ok: function () {
                $("#txtDwData").val("");
                $scope.dwReset();
                $scope.$digest();
            },
            cancel: function () { }
        }).showModal();
    };
    //加载列表时检测是否已加入下载列表
    $scope.checkPushDowload = function(){
        if(!$scope.dwData.list.length)
            return;
        for(var i=0;i<$scope.ebooks.length;i++){
            for(var j=0;j<$scope.dwData.list.length;j++){
                if($scope.ebooks[i].id == $scope.dwData.list[j]){
                    $scope.ebooks[i].s_dowload = true;
                    break;
                }
            }
        }
    }
}];
