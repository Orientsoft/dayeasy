var undoController = [
    '$scope', '$http', function ($scope, $http) {
        $scope.info = {total: 0, undo: [], does: []};
        $scope.data = {};

        $(function () {
            if (singer.isUndefined(singer.uri().batch) || singer.isUndefined(singer.uri().content)) {
                singer.msg("参数错误");
                return;
            }
            var uri = singer.uri();
            $http.get("/Home/Users?batch=" + uri.batch + "&content=" + uri.content).success(function (json) {
                if (json.status) {
                    $scope.data = json.data;
                    $scope.info.total = json.data.users.length;
                    for (var i = 0; i < json.data.users.length; i++) {
                        if (json.data.users[i].undo)
                            $scope.info.undo.push(json.data.users[i]);
                        else
                            $scope.info.does.push(json.data.users[i]);
                    }
                    singer.play({
                        id: "dayeasy-video",
                        url: json.data.url,
                        p: 0,
                        width: 560,
                        height: 420
                    });
                } else
                    singer.msg(json.message);
            }).error(function () {});
        });
    }
];