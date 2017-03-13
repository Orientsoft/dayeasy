/* 作业中心 - 教师端 */
var teacherController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = {loading: true, isnull: false, key: "", page: 1, size: 10};
        $scope.pages = {};
        $scope.papers = [];
        //加载数据
        $scope.load = function () {
            $scope.info.loading = true;
            $scope.info.isnull = false;
            $scope.papers = [];
            $scope.pages = {};
            $http.post(
                singer.sites.apps + "/work/teacher/papers",
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
    }
];
