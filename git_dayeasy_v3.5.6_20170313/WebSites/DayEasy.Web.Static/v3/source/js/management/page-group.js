/**
 * 圈子管理
 * Created by shay on 2015/12/9.
 */
(function ($, S) {
    var $form = $('#searchForm'),
        submit,
        deleteMember,
        publishJoint,
        addMember,
        convertUser;
    submit = function () {
        $form.submit();
    };

    /**
     * 转换用户信息
     * @param users
     * @param managerId
     * @returns {Array}
     */
    convertUser = function (users, managerId) {
        var list = [], actionTpl = '<i class="fa fa-times b-delete" title="删除成员"></i>';
        S.each(users, function (item) {
            var action = actionTpl;
            if (managerId < 0 || item.id == managerId)
                action = '';
            list.push({
                uid: item.id,
                code: item.code,
                avatar: S.makeThumb(item.avatar, 30, 30),
                name: (item.name ? item.name : (item.nick ? item.nick : '未填写')),
                role: item.role == 2 ? '学生' : (item.role == 1 ? '家长' : item.subjectName),
                action: action
            });
        });
        return list;
    };
    /**
     * 添加成员
     * @param groupId
     * @param userIds
     * @param callback
     * @param error
     * @returns {boolean}
     */
    addMember = function (groupId, userIds, callback, error) {
        if (!groupId || !userIds || !userIds.length)
            return false;
        //callback && callback.call(this);
        $.post('/user/group/add-members', {
            groupId: groupId,
            users: userIds
        }, function (json) {
            if (json.status) {
                callback && callback.call(this, json.data);
            } else {
                S.alert(json.message);
                error && error.call(this);
            }
        });
    };
    /**
     * 发起协同
     * @param data
     * @param success
     * @param error
     * @returns {boolean}
     */
    publishJoint = function (data, success, error) {
        if (!data || !data.groupId) {
            toastr.error('圈子Id不能为空');
            error && error.call(this);
            return false;
        }
        if (!/^[0-9]{11}$/gi.test(data.paperCode)) {
            toastr.error('试卷编号格式不正确,应为11位数字');
            error && error.call(this);
            return false;
        }
        if (!data.userId || data.userId <= 0) {
            toastr.error('请选择负责人');
            error && error.call(this);
            return false;
        }
        $.post('/user/group/publish-joint', data, function (json) {
            if (json.status) {
                toastr.success('发布成功,可到协同列表中查看');
                success && success.call(this);
            } else {
                toastr.error(json.message);
                error && error.call(this);
            }
        });
    };
    /**
     * 移出圈子成员
     * @param groupId
     * @param userId
     * @param success
     */
    deleteMember = function (groupId, userId, success) {
        $.post('/user/group/delete-member', {
            groupId: groupId,
            userId: userId
        }, function (json) {
            if (json && json.status) {
                S.msg('移出成功！', 2000, function () {
                    success && success.call(this);
                });
            } else {
                S.alert(json.message);
            }
        });
    };

    $('#type,#auth,#ct').bind('change', function () {
        submit();
    });
    //删除圈子
    $(".b-delete").click(function () {
        var $t = $(this),
            status = $t.data('status'),
            id = $t.parents("tr").data("gid"),
            msg = (status == 4 ? "删除" : "还原");
        S.confirm(S.format("您确定要{0}该圈子?", msg), function () {
            var d = this;
            $.post('/user/group/delete', {groupId: id}, function (res) {
                if (res.status) {
                    d.close().remove();
                    S.msg(msg + '成功！', 2000, function () {
                        window.location.reload(true);
                    });
                } else {
                    S.alert(res.message);
                }
            });
        });
        return false;
    });
    //圈子认证
    $('.b-certificate').bind('click', function () {
        var id = $(this).parents("tr").data("gid");
        $.Dayez.confirm("您确定要认证该圈子?", function () {
            var d = this;
            $.post('/user/group/certificate', {groupId: id}, function (res) {
                if (res.status) {
                    d.close().remove();
                    S.msg('认证成功！', 2000, function () {
                        window.location.reload(true);
                    });
                } else {
                    S.alert(res.message);
                }
            });
        }, function () {
        });
        return false;
    });
    //编辑圈子信息
    $('.d-edit').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('gid'),
            html, d;
        $.get('/user/group/load', {
            groupId: id
        }, function (json) {
            if (json.status) {
                var groupInfo = json.data;
                if (groupInfo.type == 0 || groupInfo.type == 1) {
                    var stages = ['小学', '初中', '高中'];
                    groupInfo.stageCn = stages[groupInfo.stage - 1];
                    if (groupInfo.type == 0) {
                        var years = [], current = new Date().getFullYear();
                        for (var i = current - 6; i <= current; i++) {
                            years.push(i);
                        }
                        groupInfo.years = years;
                    }
                }
                html = template('editTpl', groupInfo);
                d = S.dialog({
                    title: '编辑圈子信息',
                    width: 500,
                    content: html,
                    onshow: function () {
                        var $node = $(this.node),
                            $input = $node.find('.agency-input'),
                            $item = $node.find('.dy-agency-wrap'),
                            $list = $node.find('.dy-agencies');
                        $input.length && $input.agency({
                            item: $item,
                            list: $list,
                            add: function (agency) {
                                $item.removeClass('hide');
                                $item.find('#agencyId').val(agency.id);
                                $item.find('.dy-agency-item').html(S.format('<em class="stage{stageCode}">{stage}</em>{name}<small>{area}</small><i title="删除" class="fa fa-times"></i>', agency));
                            }
                        });
                        $node.find('.btn-save').bind('click', function () {
                            var $btn = $(this),
                                postData = {
                                    id: id,
                                    name: $node.find('#groupName').val(),
                                    summary: $node.find('#groupSummary').val()
                                };
                            var $agency = $node.find('#agencyId'),
                                $gradeYear = $node.find('.d-grade');
                            if ($agency.length) {
                                postData.agencyId = $agency.val();
                                if (!postData.agencyId || postData.agencyId.length != 32) {
                                    S.alert('请输入所属机构');
                                    return false;
                                }
                            }
                            if ($gradeYear.length) {
                                postData.gradeYear = ~~$gradeYear.val();
                                if (!postData.gradeYear || !S.inArray(postData.gradeYear, groupInfo.years)) {
                                    S.alert('请选择入学年份');
                                    return false;
                                }
                            }
                            console.log(postData);
                            $btn.disabled('稍后..');
                            $.post('/user/group/update', postData, function (json) {
                                if (json.status) {
                                    S.alert('更新成功', function () {
                                        location.reload(true)
                                    });
                                    return false;
                                }
                                S.alert(json.message);
                                $btn.unDisabled();
                            });
                        });
                    }
                });
                d.showModal();
            } else {
                d.close().remove();
                S.alert(json.message);
            }
        });
        return false;
    });
    //成员管理
    $('.d-members').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('gid'),
            name = $tr.find('td:eq(0)').text(),
            managerId = $tr.data('mid'),
            memberDialog, searchUser;
        /**
         * 搜索用户
         * @param $node
         */
        searchUser = function ($node) {
            var keyword = $node.find('.g-keyword').val(),
                $form = $node.find('.search-form'),
                $users = $form.find('.form-group');
            if (!keyword) {
                return false;
            }
            $users.html('<div style="padding: 40px 0 25px;"><span class="ui-dialog-loading">Loading..</span></div>');
            $form.find('.btn-add').disabled();
            $form.find('.d-total em').html(0);
            $.get('/user/group/search-user', {
                groupId: id,
                keyword: S.trim(keyword)
            }, function (json) {
                $users.empty();
                if (json.status) {
                    $form.removeClass('hide');
                    if (json.data && json.data.length) {
                        var userList = convertUser(json.data, -1);
                        var html = template('usersTpl', userList);
                        $users.html(html);
                    } else {
                        $users.html('<div class="dy-nothing">没有找到相关用户</div>');
                    }
                } else {
                    $form.addClass('hide');
                }
            });
        };
        memberDialog = S.dialog({
            width: 800,
            content: '<div style="padding: 40px 0 25px;"><span class="ui-dialog-loading">Loading..</span></div>',
            padding: 0,
            quickClose: true,
            backdropOpacity: 0.3,
            //fixed: true,
            onshow: function () {
                var $node = $(this.node);
                $node
                    .delegate('.ui-dialog-close', 'click', function () {
                        memberDialog.close().remove();
                    })
                    .delegate('.b-delete', 'click', function () {
                        var $li = $(this).parents('.user-item'),
                            name = $li.attr('title'),
                            uid = $li.data('uid');
                        S.confirm(S.format('确认要将成员【<b>{0}</b>】移出圈子吗？', name), function () {
                            deleteMember(id, uid, function () {
                                $li.remove();
                                //更新成员数
                                var $count = $tr.find('td:eq(3)');
                                $count.html(~~$count.html() - 1);
                            });
                        });
                    })
                    .delegate('.search-form .user-item', 'click', function () {
                        //选择用户
                        var $t = $(this),
                            $searchForm = $('.search-form'),
                            $btn = $searchForm.find('.btn-add');
                        $t.toggleClass('hover');
                        var len = $searchForm.find('.user-item.hover').length;
                        if (len > 0) {
                            $btn.unDisabled();
                        } else {
                            $btn.disabled();
                        }
                        $node.find('.d-total em').html(len);
                    })
                    .delegate('.g-keyword', 'keyup', function (e) {
                        if (e.keyCode === 13) {
                            searchUser($node);
                        }
                    })
                    .delegate('.btn-search', 'click', function () {
                        //搜索用户
                        searchUser($node);
                    })
                    .delegate('.search-form .btn-add', 'click', function () {
                        //添加成员
                        var $t = $(this),
                            $list = $node.find('.search-form .user-item.hover'), users = [];
                        if (!$list.length) {
                            S.alert('请选择要添加的用户');
                            return false;
                        }
                        S.confirm('确认将选中的用户添加到圈子中？', function () {
                            $t.disabled('稍后..');
                            $list.each(function (index, item) {
                                users.push($(item).data('uid'));
                            });
                            addMember(id, users, function (uids) {
                                S.msg('添加成功', 2000, function () {
                                    var $wrap = $node.find('.g-members'),
                                        $userList = $wrap.find('.user-list');
                                    if (!$userList.length) {
                                        $userList = $('<ul class="user-list"></ul>');
                                        $wrap.append($userList).find('.dy-nothing').remove();
                                    }
                                    $list.each(function (index, item) {
                                        var $item = $(item);
                                        if (!S.inArray($item.data('uid'), uids))
                                            return;
                                        $item.removeClass('hover').append('<i class="fa fa-times b-delete" title="删除成员"></i>');
                                        $userList.append($item);
                                    });
                                    //更新成员数
                                    var $count = $tr.find('td:eq(3)');
                                    $count.html(~~$count.html() + uids.length);
                                    //$node.find('.search-form').addClass('hide').find('.form-group').empty();
                                    $node.find('.g-keyword').focus();
                                    $t.unDisabled();
                                    var len = users.length - uids.length;
                                    if (len <= 0) {
                                        $t.disabled();
                                        $node.find('.d-total em').html('0');
                                    } else {
                                        $node.find('.d-total em').html(len);
                                    }
                                });
                            }, function () {
                                $t.unDisabled();
                            });
                        });
                    })
                ;
            }
        });
        memberDialog.showModal();
        $.post('/user/group/members', {
            groupId: id
        }, function (json) {
            if (!json.status)
                return false;
            var data = {members: [], name: name};
            data.members = convertUser(json.data, managerId);
            var html = template('membersTpl', data);
            memberDialog.content(html);
        });
        return false;
    });
    //发起协同
    $('.d-joint').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('gid'), jointDialog;
        jointDialog = S.dialog({
            title: '发起协同',
            width: 600,
            onshow: function () {
                var $node = $(this.node);
                $node
                    .delegate('.user-item', 'click', function () {
                        var $t = $(this),
                            $btn = $node.find('.btn-publish');
                        if ($t.hasClass('hover')) {
                            $t.removeClass('hover');
                            $btn.disabled();
                        } else {
                            $t.addClass('hover').siblings().removeClass('hover');
                            $btn.unDisabled();
                        }
                    })
                    .delegate('.btn-publish', 'click', function () {
                        var $t = $(this),
                            $text = $node.find('.d-keyword'),
                            $user = $node.find('.user-item.hover'),
                            paperNo = S.trim($text.val() || ''),
                            userId = $user.data('uid');
                        $t.disabled();
                        publishJoint({
                            groupId: id,
                            paperCode: paperNo,
                            userId: userId
                        }, function () {
                            jointDialog.close().remove();
                        }, function () {
                            $t.unDisabled();
                        })
                    })
                ;
            }
        });
        jointDialog.showModal();
        $.post('/user/group/members', {
            groupId: id
        }, function (json) {
            if (!json.status || !json.data || !json.data.length) {
                jointDialog.close().remove();
                S.alert('同事圈还没有任何成员，不能发起协同');
                return false;
            }
            var data = convertUser(json.data, -1);
            var html = template('jointTpl', data);
            jointDialog.content(html);
        });
        return false;
    });
    $('#keyword').agency({
        add: submit,
        remove: submit
    });

})(jQuery, SINGER);