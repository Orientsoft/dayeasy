/**
 * Created by shay on 2016/5/17.
 */
(function ($, S) {
    $('#role').bind('change', function () {
        $('#searchForm').submit();
    });
    $('.b-user-active').bind('click', function () {
        var id = $(this).parents('tr').data('uid');
        $.get('/user/active/' + id, {}, function (html) {
            S.dialog({
                title: '用户活跃信息',
                content: html
            }).showModal();
        });
        return false;
    });
    $('.b-certificate').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('uid');
        var certificate = function () {
            $.post('/user/certificate', {id: id}, function (json) {
                if (!json.status) {
                    S.alert(json.message);
                    return false;
                }
                S.msg('认证成功！', 2000, function () {
                    location.reload(true);
                });
            });
            return false;
        };
        S.confirm('确定认证该用户？', function () {
            certificate();
        });
    });
    //删除/还原
    $(".a-delete").click(function () {
        var $t = $(this),
            status = $t.data('status'),
            id = $t.parents("tr").data("uid"),
            msg = (status == 4 ? "删除" : "还原");
        $.Dayez.confirm(S.format('确定要{0}该用户？', msg), function () {
            $.post('/user/delete', {userId: id}, function (res) {
                if (res.status) {
                    S.msg(msg + '成功！', 2000, function () {
                        window.location.reload(true);
                    });
                } else {
                    S.alert(res.message);
                }
            });
        }, function () {
        });
    });
    //重置密码
    $(".a-reset").click(function () {
        var id = $(this).parents("tr").data("uid");
        $.Dayez.confirm("您确定要重置该用户密码?", function () {
            $.post('/user/reset', {userId: id}, function (res) {
                if (res.status) {
                    S.msg('重置成功！', 2000);
                } else {
                    S.alert(res.message);
                }
            });
        }, function () {
        });
    });
    //编辑资料
    $(".a-edit").click(function () {
        var $tr = $(this).parents("tr");
        var id = $tr.data("uid"), name = $tr.data("rname");
        var content = template('edit-template', {name: name});

        var $dialog = S.dialog({
            title: "编辑用户资料",
            content: content,
            okValue: "确定",
            cancelValue: "取消",
            ok: function () {
                name = $("#txtRealName").val();
                if (!name || !name.length) {
                    S.msg("真实姓名不能为空");
                    return false;
                }
                $.post("/user/edit", {userId: id, name: name}, function (json) {
                    if (!json.status) {
                        S.msg(json.message);
                        return;
                    }
                    $tr.data("rname", name).find("td").eq(1).html(name);
                    $dialog.close().remove();
                });
                return false;
            },
            cancel: function () {
            }
        });
        $dialog.showModal();
    });

    var importDialog;
    window.importCallback = function (json) {
        if (json.status) {
            S.msg('导入成功！');
            importDialog && importDialog.close().remove();
        } else {
            S.alert(json.message);
        }
    };
    $('.b-import-num').bind('click', function () {
        var importNumbers, $content;
        importNumbers = function ($form) {
            $form.submit();
        };
        $content = $('<div class="d-import-area">').css({width: 400});
        $content.append('<iframe name="importTarget" class="hide" frameborder="0" height="0" width="0">');
        var $form = $('<form action="/user/import-num?callback=parent.importCallback" method="POST" target="importTarget" enctype="multipart/form-data">');
        $form.append('<div class="text-gray" style="margin-bottom: 15px">请输入学生学号信息，格式：<em>得一号,学号</em>，一行一个。</div>');
        $form.append('<textarea class="form-control" name="numbers" style="height:200px;margin-bottom:15px"></textarea>');
        $form.append('<input name="numberFile" type="file"/>');
        $content.append($form);
        importDialog = S.dialog({
            title: '导入学生学号',
            content: $content,
            okValue: '确认导入',
            ok: function () {
                var $form = $(this.node).find('.d-import-area form');
                importNumbers($form);
                return false;
            }
        });
        importDialog.showModal();
    });
    $(document)
        .delegate('.j-release', 'click', function () {
            var $t = $(this), uid = $t.data('uid');
            $t.disableField('稍后..');
            S.confirm('确认要释放该用户？<br /><b class="text-danger">释放后，将重置该用户的邮箱、手机以及第三方信息，且用户将不能登录</b>', function () {
                $.post('/user/release-user', {
                    userId: uid
                }, function (json) {
                    if (json.status) {
                        S.msg('释放成功');
                        $t.remove();
                        return false;
                    }
                    S.alert(json.message);
                    $t.undisableFieldset();
                })
            }, function () {
                $t.undisableFieldset();
            });

        })
    ;
})(jQuery, SINGER);