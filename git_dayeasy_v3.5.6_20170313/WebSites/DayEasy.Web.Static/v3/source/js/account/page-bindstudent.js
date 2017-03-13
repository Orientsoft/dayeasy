/**
 * Created by shay on 2015/11/11.
 */
(function ($, S) {
    var logger,
        childLogin,
        bindDialog,
        $btn = $('#btnLoad'),
        studentId;
    logger = S.getLogger('bind-student');
    studentId = $('.cont-min').data('id');
    bindDialog = function (id) {
        $.get('/bind/child-complete', {
            studentId: id,
            return_url: S.uri().return_url
        }, function (html) {
            S.dialog({
                title: '关联学生帐号',
                content: html
            }).showModal();
        });
    };
    if (studentId && S.isNumber(studentId)) {
        bindDialog(studentId);
    }
    childLogin = function () {
        var account = $('#account').val(),
            pwd = $('#pwd').val();
        if (!account || !pwd) {
            S.msg("请输入学生帐号和密码！");
            return false;
        }
        if (!/^(1\d{10})|([\w\W]+@[0-9a-z]+(\.[0-9a-z]+)+)$/gi.test(account)) {
            S.msg("学生帐号不是有效的邮箱地址或手机号！");
            return false;
        }
        if (pwd.length < 6) {
            S.msg("学生密码长度不能小于6位！");
            return false;
        }
        $btn.disableField('正在关联...');
        $.post('/bind/load-student', {
            account: account,
            pwd: pwd
        }, function (json) {
            if (json.status) {
                bindDialog(json.data.id);
            } else {
                S.alert(json.message);
            }
            $btn.undisableFieldset();
        });
    };
    $btn.bind('click', function () {
        $btn.blur();
        childLogin();
        return false;
    });
    $('.dy-plat-list a').bind('click', function () {
        var type = $(this).data('type');
        if (!S.isUndefined(type)) {
            location.href = S.format('/login/third-login?type={0}&host={1}', type, encodeURIComponent(S.sites.account + '/bind/child'));
        }
        return false;
    });
    $('#pwd').bind('keyup', function (e) {
        e.stopPropagation();
        if (e.keyCode === 13) {
            childLogin();
        }
    });
})(jQuery, SINGER);