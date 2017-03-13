/**
 * Created by shay on 2015/11/3.
 */
(function ($, S) {
    var logger = S.getLogger('page-apply');
    var submitApply = function ($t) {
        $t.blur();
        var id = $t.parent().data('group'),
            $trueName = $('#trueName'),
            name = $trueName.val(),
            message = $('.textarea').val();
        if ($trueName.length) {
            if (!name) {
                S.alert('加入圈子需要输入真实姓名！');
                return false;
            }
            if (!/^[\u4e00-\u9fa5]{2,5}$/.test(name)) {
                S.alert('真实姓名需要输入2-5个汉字！');
                return false;
            }
        }
        var data = {
            groupId: id,
            userId: $('.apply-join').data('uid'),
            message: message
        };
        if (name) {
            data.trueName = name;
        }
        $t.disableField('提交中...');
        $.post('/group/apply', data, function (json) {
            if (!json.status) {
                S.alert(json.message);
            } else {
                S.msg("申请成功！", 2000, function () {
                    location.reload(true);
                });
            }
            $t.undisableFieldset();
        });
        return false;
    };
    $('input[type="submit"]').bind("click", function () {
        submitApply($(this));
    });
})(jQuery, SINGER);