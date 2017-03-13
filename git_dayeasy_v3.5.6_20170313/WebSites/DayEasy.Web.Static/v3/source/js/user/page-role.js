/**
 * 用户角色选择
 * Created by shay on 2015/11/20.
 */
(function ($, S) {
    var $btn = $("#btnSubmit"),
        $roles = $(".role-list");


    $(".iconfont").bind("click", function () {
        $roles.find("li").removeClass("z-sel");
        $(this).parent().addClass("z-sel");
        if ($btn.attr("disabled")) {
            $btn.removeAttr("disabled");
        }
    });
    $btn.bind("click", function () {
        var role, subject_id, $role, $subject;
        $role = $('.role-list').find('.z-sel');
        role = $role.data('role');
        $subject = $('#ddlSubject').find('option:selected');
        if ($role.length == 0 || !S.inArray(role, [1, 2, 4])) {
            S.msg('请选择角色');
            return false;
        }
        if (role == 4) {
            subject_id = $subject.val();
            if (subject_id <= 0) {
                S.msg("请选择任教学科");
                return false;
            }
        }
        var bindRole = function () {
            $.post("/user/bind-role", {
                role: role,
                subject_id: subject_id
            }, function (json) {
                if (json.status) {
                    window.location.href = (S.sites.main + '/user/complete?return_url=' + S.uri().return_url);
                } else {
                    S.msg(json.message);
                }
            });
        };
        var msg = S.format('你选择了角色：<span class="text-primary">{0}</span>{1}？<p class="text-danger mt10">注：确认提交后将不能修改！</p>',
            $role.data('text'),
            (role == 4 ? S.format('，任教科目：<span class="text-primary">{0}</span>', $subject.html()) : ''));
        S.confirm(msg, function () {
            bindRole()
        });
    });
})(jQuery, SINGER);


