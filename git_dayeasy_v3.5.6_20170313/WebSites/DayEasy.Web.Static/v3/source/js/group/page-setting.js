/**
 * 圈子设置
 * Created by shay on 2015/11/9.
 */
(function ($, S) {
    var logger = S.getLogger('group-setting'),
        update,
        transfer,
        updateLogo,
        updateGroup,
        dissolution,
        deleteMember,
        groupId,
        isManager,
        quit,
        $circles;
    $circles = $('.home-circles');
    groupId = $circles.data('gid');
    isManager = $circles.data('manager') == 'True';
    /**
     * 更新圈子信息
     * @param obj
     */
    update = function (obj) {
        var $t = $(obj),
            param = $t.parents('li').data('param');
        if (param === "owner") {
            transfer(obj);
            return false;
        }
        //基础信息更新
        var $input = $t.siblings('input,textarea'),
            value = $input.val(),
            $p = $t.parent(),
            $plist = $('.mian-cont li p');
        if ($p.hasClass('z-sel')) {
            updateGroup(getUpdateData(obj), function () {
                $input.attr({
                    disabled: 'disabled',
                    readonly: 'readonly'
                });
                $p.removeClass('z-sel');
                if (param === 'name') {
                    $('.top-title .dg-name strong').html(value);
                }
            });
        } else {
            $p.addClass('z-sel');
            $plist.not($p).removeClass('z-sel');
            $input.removeAttr('disabled').removeAttr('readonly').focus();
        }
    };
    var getUpdateData = function (obj) {
        var $t = $(obj),
            param = $t.parents('li').data('param'),
            $input = $t.siblings('input,textarea'),
            value = $input.val(),
            id = groupId;
        value = S.trim(value || '');
        if (param === "name" && (!value || value.length > 20)) {
            S.msg('圈名称不能为空且不能超过20个字符！');
            return false;
        }
        if (param === "summary" && (!value || value.length > 140)) {
            S.msg('圈简介不能为空且不能超过140个字符！');
            return false;
        }
        var data = {
            id: id
        };
        data[param] = value;
        return data;
    };
    updateGroup = function (data, callback) {
        if (!data || !data.id || !isManager)
            return false;
        $.post('/group/update', data, function (json) {
            if (json.status) {
                S.msg('修改成功！');
                callback && S.isFunction(callback) && callback.call();
            } else {
                S.msg(json.message);
            }
        });
    };
    /**
     * 转让圈主
     * @param obj
     */
    transfer = function (obj) {
        if ($(obj).data('show')) {
            return false;
        }
        $.post('/group/teachers', {
                id: groupId
            }, function (json) {
                if (json.status && json.data && json.data.length) {
                    var teachers = json.data,
                        html =
                            '<div class="pop-members">' +
                            '<div class="pop-cont">' +
                            '<ul>{0}</ul>' +
                            '</div></div>',
                        lis = [];
                    for (var i = 0; i < teachers.length; i++) {
                        var item = teachers[i];
                        lis.push(
                            S.format(
                                '<li data-uid="{0}" title="{2}">' +
                                '<img width="30" height="30" src="{1}" alt="" />' +
                                '<strong>{2}</strong>' +
                                '<p>{3}</p><i class="iconfont dy-icon-gou"></i>' +
                                    //'<button class="dy-btn dy-btn-default">转让</button>' +
                                '</li>',
                                item.id,
                                S.makeThumb(item.avatar, 30, 30),
                                item.name,
                                item.subjectName || ""
                            )
                        );
                    }
                    S.dialog({
                        title: '转让圈主',
                        content: S.format(html, lis.join('')),
                        //align: 'left',
                        cancelValue: '取消',
                        okValue: '确认转让',
                        ok: function () {
                            var $node = $(this.node),
                                $select = $node.find('li.active');
                            if ($select.length == 0) {
                                S.msg('请选择要转让给的教师！');
                                return false;
                            }
                            var name = $select.find('strong').html();
                            S.confirm(S.format('确认将圈主转让给[{0}]吗？', name), function () {
                                var uid = $select.data('uid');
                                //转让圈主
                                $.post('/group/transfer', {
                                    groupId: groupId,
                                    userId: uid
                                }, function (json) {
                                    if (json.status) {
                                        S.alert('转让成功！', function () {
                                            location.reload(true);
                                        });
                                    } else {
                                        S.alert(json.message);
                                    }
                                });
                            });
                            return false;
                        },
                        cancel: function () {
                            this.close().remove();
                        },
                        onshow: function () {
                            var $node = $(this.node);
                            $node.find('li').bind('click', function () {
                                $(this).toggleClass('active').siblings().removeClass('active');
                            });
                        },
                        onclose: function () {
                            $(obj).data('show', false);
                        }
                    }).showModal();
                    $(obj).data('show', true);
                }
                else {
                    S.alert('圈内没有其他用户可以接收转让！');
                }
            }
        )
        ;
    };
    /**
     * 解散圈子
     */
    dissolution = function () {
        var msg = '解散圈子后，所有圈子成员将不能再看到圈子里的内容，确认要解散圈子吗？';
        S.confirm(msg, function () {
            $.post('/group/dissolution', {
                id: groupId
            }, function (json) {
                if (json.status) {
                    S.alert('圈子已经成功解散！', function () {
                        location.href = '/group';
                    });
                } else {
                    S.alert(json.message);
                }
            })
        });
    };
    /**
     * 删除成员
     * @param obj
     */
    deleteMember = function (obj) {
        var $t = $(obj),
            id = $t.data('id'),
            name = $t.data('name');
        S.confirm(S.format('确认移除圈成员【{0}】?', name), function () {
            $.post('/group/delete', {
                groupId: groupId,
                userId: id
            }, function (json) {
                if (json.status) {
                    S.alert('移除成功！', function () {
                        $t.remove();
                    });
                } else {
                    S.alert(json.message);
                }
            });
        });
    };
    /**
     * 上传圈子logo
     */
    updateLogo = function () {
        $(".webuploader-element-invisible").click();
    };
    /**
     * 退出圈子
     */
    quit = function () {
        S.confirm(
            S.format('退出圈子[{0}]之后，<br/><span class="text-danger">你将不能查看与原圈子相关的动态！</span><br/>是否确认要退出？', $('.dg-name strong').html()),
            function () {
                $.post('/group/quit', {
                    id: groupId
                }, function (json) {
                    if (json.status) {
                        S.alert("你已成功退出圈子！", function () {
                            location.href = S.sites.main;
                        });
                    } else {
                        S.alert(json.message);
                    }
                });
            });
    };

    //图片上传处理
    S.uploader.on("uploadSuccess", function (file, response) {
        if (response.state) {
            var url = response.urls[0];
            $('#avatar').val(url);
            $('.dg-avatar img').attr("src", S.makeThumb(url, 150, 150));
            updateGroup({
                id: groupId,
                avatar: response.urls[0]
            });
        }
        S.uploader.reset();
    });

    if (isManager) {
        /**
         * 事件绑定
         */
        $('.mian-cont .iconfont').bind('click', function () {
            update(this);
            return false;
        });
        $('.mian-cont input,.mian-cont textarea').bind('blur', function () {
            var $t = $(this),
                $area = $t.parents('.form-area');
            if ($area.hasClass('z-sel')) {
                $t.attr({
                    disabled: 'disabled',
                    readonly: 'readonly'
                });
                S.later(function () {
                    $area.removeClass('z-sel');
                }, 500);
            }
        });
        $('.dg-avatar').bind('click', function () {
            updateLogo();
        });
        $('#dissolution').bind('click', function () {
            dissolution();
            return false;
        });
        $('.post-auth').bind('change', function () {
            var auth = $(this).val();
            updateGroup({
                id: groupId,
                postAuth: auth
            });
        });
        /**
         * 更换背景
         */
        $('.dg-change-bg').bind('click', function () {
            $.get('/group/choose-image', {
                type: 2,
                image: encodeURIComponent($('.top-img img').attr('src'))
            }, function (html) {
                S.dialog({
                    content: html,
                    cancelValue: '取消'
                }).showModal();
            });
        });
        /**
         * 删除成员
         */
        $('.b-delete').bind('click', function () {
            $(this).blur();
            var $tr = $(this).parents('tr');
            deleteMember($tr);
            return false;
        });
        S._mix(S, {
            setImage: function (img) {
                updateGroup({
                    id: groupId,
                    banner: img
                }, function () {
                    $('.top-img img').attr('src', img);
                });
            }
        });
    } else {
        $('#quitBtn').bind('click', function () {
            quit();
            return false;
        });
    }
    S.tags({
        canEdit: isManager,
        max: 3,
        data: eval($('.d-tags').data('list')),
        change: function (tags) {
            updateGroup({
                id: groupId,
                tags: S.json(tags).replace(/'/gi, '"')
            });
        }
    });
})(jQuery, SINGER);