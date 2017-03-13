/**
 * 推荐目标
 * Created by shay on 2016/8/26.
 */
(function ($, S) {
    $(".layB").slide({
        mainCell: ".slide",
        autoPlay: false,
        effect: "left"
    });
    $('.b-target').bind('click', function () {
        var $t = $(this),
            $li = $t.parents('li'),
            id = $li.data('aid');
        $.post('/user/add-relation', {
            agencyId: id,
            status: 2
        }, function (json) {
            if (!json.status) {
                S.alert(json.message);
                return false;
            }
            $t.replaceWith('<span class="text-gray">已设定</span>');
            var d = S.dialog({
                content: '<div class="success-pop f-tac">' +
                '<i class="iconfont dy-icon-13"></i>' +
                '<b>设定成功！</b><p>欢迎加入得一！' +
                '</p></div>'
            });
            d.showModal();
            S.later(function () {
                location.href = S.sites.main + '/user';
            }, 2000);
        });
    });
})(jQuery, SINGER);