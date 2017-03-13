/**
 * Created by shay on 2015/10/28.
 */
var logger = singer.getLogger("addCtrl"),
    createCtrl = ['$scope', function ($scope) {
        $scope.submiting = false;
        $scope.step = 1;
        $scope.years = [];
        $scope.currentType = {name: '', img: ''};
        $scope.group = {
            type: -1,
            name: '',
            summary: '',
            stage: 1,
            agencyId: '',
            agencyName: '',
            grade: 0,
            channelId: -1,
            postAuth: 0,
            joinAuth: 0,
            subjectId: -1,
            isManager: false
        };
        $scope.subjects = [{id: -1, name: '选择科目'}];
        var returnUrl = singer.uri().return_url;
        $scope.group.isManager = returnUrl && (returnUrl.indexOf('/user/group') > 0);
        if ($scope.group.isManager) {
            $.get('/group/subjects', {}, function (json) {
                if (json.status) {
                    singer.each(json.data, function (item) {
                        $scope.subjects.push(item);
                    });
                    $scope.$digest();
                }
            });
        }
        var initYears = function (count) {
            var now = (new Date()).getFullYear();
            $scope.years = [];
            for (var i = 0; i < count; i++) {
                $scope.years.push(now - i);
            }
            $scope.group.grade = now.toString();
        };
        var submit = function () {
            $('.dy-btn-info').blur();
            if ($scope.submiting) {
                return false;
            }
            var group = $scope.group;
            if ($scope.needName) {
                if (!group.userName) {
                    singer.alert('请输入真实姓名！');
                    return false;
                }
                if (!/^[\u4e00-\u9fa5]{2,5}$/.test(group.userName)) {
                    singer.alert('真实姓名需要输入2-5个汉字！');
                    return false;
                }
            }
            if (group.type == 0 || group.type == 1) {
                if (!group.agencyId || group.agencyId.length != 32) {
                    singer.alert("请选择学校！");
                    return false;
                }
            } else if (group.type == 2) {
                if (group.channelId <= 0) {
                    singer.alert("请选择频道！");
                    return false;
                }
            }
            if (!group.name || group.name.length > 20) {
                singer.alert("圈子名字不能为空，且长度不能超过20个字！");
                return false;
            }
            if ($scope.group.isManager && $scope.group.type == 1 && $scope.group.subjectId <= 0) {
                singer.alert("请选择同事圈科目");
                return false;
            }
            $scope.submiting = true;
            $.post('/group/create', $scope.group, function (data) {
                if (data.status) {
                    if (!returnUrl) returnUrl = '/group/success/' + data.data.id;
                    location.href = returnUrl;
                } else {
                    singer.alert(data.message);
                    $scope.submiting = false;
                    $scope.$digest();
                }
            });
        };
        $scope.stageChange = function () {
            var count = ($scope.group.stage == 1 ? 6 : 3);
            initYears(count);
        };
        /**
         * 设置步骤
         * @param step
         * @param type
         * @param obj
         */
        $scope.setStep = function (step, type, obj) {
            if ($scope.step == 3
                || $scope.step == step
                || (step > 1 && $scope.group.type < 0 && type < 0))
                return false;
            if (step == 3) {
                submit();
                return false;
            }
            $scope.step = step;
            if (step == 2 && type >= 0) {
                var $li = $(obj);
                if (!$li.is('li')) {
                    $li = $li.parents('li');
                }
                $scope.currentType = {
                    name: $li.find('h3').html(),
                    img: $li.find('img').attr('src')
                };
                $scope.group.type = type;
                $scope.needName = ($('#needName').val() == 1);
                if ($scope.group.type == 2) {
                    singer.later(function () {
                        //标签
                        singer.tags({
                            canEdit: true,
                            max: 3,
                            change: function (tags) {
                                $scope.group.tags = singer.json(tags).replace(/'/gi, '"');
                                $scope.$digest();
                            }
                        });
                    }, 100);
                } else {
                    initYears(6);
                }
            }
        };
        var agencySelector, stages = {};
        $scope.agencySelector = function () {
            if (!agencySelector) {
                agencySelector = singer.dialog({
                    title: '选择学校',
                    content: '<div class="dy-loading" style="margin-bottom: 30px;width: 320px"><i></i></div>',
                    quickClose: false,
                    cancel: function () {
                        this.close();
                        return false;
                    },
                    cancelDisplay: false
                });
            }
            agencySelector.showModal();
            if (!stages.hasOwnProperty($scope.group.stage)) {
                $.get('/group/agency-selector', {
                    stage: $scope.group.stage,
                    code: 5101,
                    agencyId: $scope.group.agencyId,
                    keyword: ''
                }, function (html) {
                    agencySelector.content(html);
                    stages[$scope.group.stage] = html;
                });
            } else {
                agencySelector.content(stages[$scope.group.stage]);
            }
        };
        $(document)
            .delegate('.school-name li', 'click.selector', function () {
                var $li = $(this);
                $li.addClass('z-sel').siblings().removeClass('z-sel');
                $scope.group.agencyId = $li.data('aid');
                $scope.group.agencyName = $li.html();
                $scope.$digest();
                agencySelector.close();
                return false;
            })
        ;
        $.get('/group/current-agency', {}, function (json) {
            if (json.status) {
                $scope.group.stage = json.data.stage;
                $scope.group.agencyId = json.data.id;
                $scope.group.agencyName = json.data.name;
                $scope.$digest();
            }
        });
    }];