/**
 * 个人设置页
 * Created by shay on 2015/11/17.
 */
(function ($, S) {
    var updateAvatar, updateEmail, updatePwd, update, logger, getUpdateData, updateMobile;
    logger = S.getLogger('account/page-setting');
    S._mix(S, {
        setField: function ($li, status, info) {
            S.resetField($li);
            var cls = status ? 'dy-success' : 'dy-error',
                icon = status ? 'dy-icon-gou' : 'dy-icon-cha';

            $li.addClass(cls)
                .append('<i class="iconfont ' + icon + '"></i>');
            if (!status && info) {
                $li.append('<span class="dy-error-info">' + info + '</span>');
            }
        },
        resetField: function ($li) {
            $li.removeClass('dy-error').removeClass('dy-success');
            $li.find('.iconfont,.dy-error-info').remove();
        }
    });
    /**
     * 上传个人头像
     */
    updateAvatar = function () {
        $(".webuploader-element-invisible").click();
    };
    /**
     * 绑定邮箱
     */
    updateEmail = function () {
        $.get('/edit-email', {}, function (html) {
            S.dialog({
                title: '绑定邮箱',
                content: html
            }).showModal();
        });
    };

    /**
     * 修改密码
     */
    updatePwd = function () {
        $.get('/change-pwd', {}, function (html) {
            S.dialog({
                title: '修改密码',
                content: html
            }).showModal();
        });
    };
    
    /**
     * 绑定手机号码
     */
    updateMobile = function(){
        $.get('/bind-mobile', {}, function (html) {
            S.dialog({
                title: '绑定手机',
                content: html
            }).showModal();
        });
    };

    /**
     * 获取更新数据
     * @param obj
     */
    getUpdateData = function (obj) {
        var $t = $(obj),
            param = $t.parents('li').data('param'),
            $input = $t.siblings('input,textarea'),
            value = $input.val();
        value = S.trim(value || '');
        if (param === "nick" && (!value || value.length > 20)) {
            S.msg('昵称不能为空且不能超过20个字符！');
            return false;
        }
        if (param === "mobile" && (!value || value.length !== 11)) {
            S.msg('手机号码格式不正确！');
            return false;
        }
        if (param === "studentNo" && (!value || value.length > 15)) {
            S.msg('学号不能为空且不能超过15个字符！');
            return false;
        }
        if (param === "name" && (!value || !/^[\u4e00-\u9fa5]{2,5}$/.test(value))) {
            S.msg('真实姓名需要输入2-5个汉字！');
            return false;
        }
        var data = {};
        data[param] = value;
        return data;
    };
    /**
     * 更新个人资料
     * @param data
     * @param callback
     */
    update = function (data, callback) {
        $.post('/update', data, function (json) {
            if (json.status) {
                S.msg('修改成功！');
            } else {
                S.msg(json.message);
            }
            callback && S.isFunction(callback) && callback.call();
        });
    };

    /**
     * 图片上传处理
     */
    S.uploader.on("uploadSuccess", function (file, response) {
        if (response.state) {
            var url = response.urls[0];
            update({avatar: url}, function () {
                $('#bu-update-avatar img').attr('src', S.makeThumb(url, 160, 160));
            });
        }
        S.uploader.reset();
    });


    /**
     * 事件绑定
     */
    $('#bu-update-avatar').bind('click', function () {
        updateAvatar();
    });

    $('.mian-cont .iconfont').bind('click', function () {
        var $t = $(this),
            param = $t.parents('li').data('param');
        if (param === "email") {
            updateEmail();
            return false;
        }
        if (param === "password") {
            updatePwd();
            return false;
        }
        if(param === "mobile"){
            updateMobile();
            return false;
        }
        var $input = $t.siblings('input,textarea'),
            value = $input.val(),
            $p = $t.parent(),
            $plist = $('.mian-cont li p');
        if ($p.hasClass('z-sel')) {
            var data = getUpdateData($t);
            if (!data)
                return false;
            update(data, function () {
                $input.attr({
                    disabled: 'disabled',
                    readonly: 'readonly'
                });
                $p.removeClass('z-sel');
                if (param === 'name') {
                    $('.home-class-name span').html(value);
                }
            });
        } else {
            $p.addClass('z-sel');
            $plist.not($p).removeClass('z-sel');
            $input.removeAttr('disabled').removeAttr('readonly').focus();
        }
    });

    $('.b-plat-bind').bind('click', function () {
        window.location.href = "/plat/bind/0";
        return false;
    });
    $('.b-plat-unbind').bind('click', function () {
        S.confirm("确定要解除第三方登录绑定吗？", function () {
            $.post("/plat/unbind", {type: 0}, function (json) {
                if (json.status) {
                    S.msg("操作成功", 1000, function () {
                        window.location.reload();
                    });
                } else {
                    S.msg(json.message);
                }
            });
        });
        return false;
    });
    $('.b-unbind-relation').bind('click', function () {
        var id = $(this).parent().data('id');
        S.confirm('确认解除对孩子帐号的关联？', function () {
            $.post('/unbind-child', {
                id: id
            }, function (json) {
                if (json.status) {
                    S.msg("操作成功", 1000, function () {
                        window.location.reload();
                    });
                } else {
                    S.msg(json.message);
                }
            })
        });
        return false;
    });
})(jQuery, SINGER);