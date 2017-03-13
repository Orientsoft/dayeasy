/**
 * 描述自己
 * Created by shay on 2016/8/26.
 */
(function ($, S) {
    $('.wrap-base').on('click', 'li', function (event) {
        event.preventDefault();
        var $t = $(this),
            isSingle = $t.parents('ul').data('single');
        $t.toggleClass('on');
        isSingle && $t.hasClass('on') && $t.siblings().removeClass('on');
    });
    var redirect = function () {
        var returnUrl = S.uri().return_url,
            url = '/user/rec-groups';
        if (returnUrl) {
            url += '?return_url=' + returnUrl;
        }
        location.href = url;
    };
    $('#nextBtn').bind('click', function () {
        var tags = [],
            $list = $('.wrap-base').find('li.on');
        if ($list.length == 0) {
            redirect();
            return false;
        }
        $list.each(function (index) {
            tags.push($list.eq(index).html());
        });
        $.post('/user/add-impression', {
            userId: 0,
            content: tags
        }, function (json) {
            if (!json.status) {
                S.alert(json.message);
                return false;
            }
            redirect();
        });
        return false;
    });
})(jQuery, SINGER);