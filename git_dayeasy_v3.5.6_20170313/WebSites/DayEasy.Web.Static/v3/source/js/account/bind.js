$(function ($) {
    var lock = false;
    //老用户
    $("#btnBind").bind("click", function () {
        if (lock) return;
        var msgBox = $(".d-msg");
        msgBox.html("&nbsp;");
        var id = $(".bd-main").data('id'),
            account = $("#txtAccount").val().trim(),
            pwd = $("#txtPassword").val().trim();

        if (id == "") {
            msgBox.html("参数错误，请刷新重试");
            return;
        }
        if (account == "") {
            msgBox.html("请输入已有帐号");
            return;
        }
        if (pwd == "") {
            msgBox.html("请输入登录密码");
            return;
        }
        if (pwd.length < 6 || pwd.length > 20) {
            msgBox.html("登录密码不正确");
            return;
        }
        $(this).html('<i class="fa fa-spin fa-spinner fa-1x"></i>&nbsp;登录中...');
        lock = true;
        $.post("/bind/account-bind",
            {
                platId: id,
                account: account,
                pwd: pwd
            },
            function (json) {
                $("#btnBind").html('登录');
                if (!json.status) {
                    lock = false;
                    msgBox.html(json.message);
                } else {
                    msgBox.attr("style", "color:#5c885c;");
                    msgBox.html("正在跳转...");
                    window.location.href = DEYI.sites.main;
                }
            });
    });
    //新用户
    $("#btnNew").bind("click", function () {
        if (lock) return;
        lock = true;
        $(this).html('处理中...');
        $.post("/bind/account-create",
            {
                platId: $(".bd-main").data('id')
            },
            function (json) {
                if (!json.status) {
                    lock = false;
                    $("#btnNew").html('没有帐号，去注册 >');
                    alert(json.message);
                } else {
                    $("#btnNew").html('正在跳转...');
                    window.location.href = DEYI.sites.main;
                }
            });
    });
});