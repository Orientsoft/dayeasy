/**
 * 个人设置
 * Created by shay on 2016/5/24.
 */
(function ($, S) {
    S.progress.start();
    var $dl = $(".d-setting-list dl"),
        itemTemp = '<dt>{0}</dt><dd>{1}</dd>';
    S.setUser(function (user) {
        $dl.append(S.format(itemTemp, '登陆邮箱', user.email?user.email:'-'));
        $dl.append(S.format(itemTemp, '昵　　称', user.nick?user.nick:'-'));
        $dl.append(S.format(itemTemp, '真实姓名', user.name?user.name:'-'));
        $dl.append(S.format(itemTemp, '手机号', user.mobile?user.mobile:'-'));
        if ((user.role & 2) > 0) {
            $dl.append(S.format(itemTemp, '学　　号', user.studentNum ? user.studentNum : '-'));
        }
        $dl.append(S.format(itemTemp, '得 一 号', user.code));
        $dl.append(S.format(itemTemp, '角　　色', user.roleDesc));
        //$dl.append(S.format(itemTemp, '第三方帐号', user.email));
    });
    S.progress.done();
})(jQuery, SINGER);