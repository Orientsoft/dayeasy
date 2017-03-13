/**
 * 登录页
 * Created by shay on 2016/5/24.
 */

(function ($, S) {
    var $form = $('#form_login'),
        $inputs = $form.find('input'),
        $button = $form.find('button'),
        validation,
        errorMessage,
        uri = S.uri(),
        logger = S.getLogger('login');
    /**
     * 登录按钮状态
     */
    $inputs.bind('keyup', function () {
        var count = 0;
        $(this).parents('.d-input').removeClass('d-input-err');
        $inputs.each(function (i, item) {
            var text = S.trim($(item).val());
            if (text && text.length > 0) {
                count++;
            }
        });
        if (count == $inputs.length) {
            S.enableBtn($button, true);
        } else {
            S.enableBtn($button, false);
        }
    });

    /**
     * 错误消息
     */
    errorMessage = function (message) {
        if (!message || !message.length) {
            return false;
        }
        var $input;
        if (message.indexOf('帐号') >= 0) {
            $input = $('#account');
        } else if (message.indexOf('密码') >= 0) {
            $input = $('#pwd');
        } else {
            $input = $('#account');
        }
        $input.parents('.d-input').addClass('d-input-err');
        $input.next().html(message);
    };
    /**
     * 帐号验证
     */
    validation = function (data) {
       var accountError = false,
           emailReg = /^[a-zA-Z0-9_]+@[a-zA-Z0-9]+.[a-zA-Z]+$/,
           mobileReg = /^1[3|5|7|8][0-9]{9}$/,
           codeReg = /^[\d]{5,}$/g;
        if (!accountError && !data.account.length) {
            accountError = true;
            errorMessage('请输入登录帐号！');
            return false;
        }

        if (!accountError &&emailReg.test(data.account) &&!mobileReg.test(data.account) &&codeReg.test(data.account)) {
            accountError = true;
            errorMessage('登录帐号格式不正确s！');
            return false;
        }
        if (!data.password || data.password.length < 6) {
            errorMessage('登录密码至少6位！');
            return false;
        }
        return true;
    };

    /**
     * 登录
     */
    $button.bind('click', function () {
        var $t = $(this),
            data = {
                account: $inputs.eq(0).val(),
                password: $inputs.eq(1).val()
            };
        if (!validation(data))
            return false;
        S.enableBtn($t, false, '登录中...');
        data.method = 'user_login';
        data.isEncrypt = false;
        $.dAjax(data, function (json) {
            var data = json.data;
            if (json.status) {
                $.setToken(data.token);
                //教师登录跳转至主站
                if ((data.user.role & 4) > 0) {
                    location.href = S.sites.main + 'group?from=wap';
                    return false;
                }
                if (uri.back) {
                    location.href = uri.back;
                } else {
                    location.href = '/page/group/home.html';
                }
            } else {
                errorMessage(json.message);
                S.enableBtn($t, true);
            }
        }, true);
        return false;
    });
    /**
     * 显示密码
     */
    $('.d-btn-pwd').bind('click', function () {
        var $t = $(this),
            $pwd = $('#pwd');
        if ($pwd.attr('type') === "password") {
            $pwd.attr('type', 'text');
            $t.addClass('d-btn-on');
        } else {
            $pwd.attr('type', 'password');
            $t.removeClass('d-btn-on');
        }
    });
    /**
     * 第三方登录
     */
    $('.login-qq').bind('click', function () {
        var url = 'http://account.dayeasy.net/login/third-login?type=0&host={0}&back={1}';
        var host = location.host;
        if (host.indexOf('dayeasy.net') >= 0) {
            url = S.format(url, '', encodeURIComponent('http://m.dayeasy.net'));
        } else {
            host = host.substring(host.indexOf('.'));
            url = S.format(url, encodeURIComponent('http://account' + host + '/bind'), encodeURIComponent('http://m' + host));
        }
        location.href = url;
    });
})(jQuery, SINGER);