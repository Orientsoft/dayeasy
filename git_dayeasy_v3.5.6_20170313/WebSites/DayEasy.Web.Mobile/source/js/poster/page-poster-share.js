/**
 * 生成成功
 * Created by shay on 2016/9/3.
 */
(function ($, S) {
    var id = S.uri().id,
        restUrl = 'http://open' + (S.sites.domain || '.v3.deyi.com') + '/activity/';
    if (!id) {
        location.href = 'make.html';
        return false;
    }
    if (!document.referrer) {
        location.href = 'item.html?id=' + id;
        return false;
    }
    $.get(restUrl + 'god-poster/' + id, {}, function (json) {
        if (json) {
            $('#posterImg').attr('src', json.posterUrl);
            $('#posterImg').parents('.image-popup-vertical-fit').attr('href',json.posterUrl);
            document.title = S.format('我为{school}{name}制作了教师节海报，是第{index}个制作者，快来接力吧！', json);
        }
    });
    $('#btnMake').bind('click', function () {
        location.href = 'make.html';
    });

    $('#btnShare').bind('click', function () {
        //分享
        var htmlSharePop = $('<div class="share-pop"></div>');
        $('body').append(htmlSharePop);
        htmlSharePop.bind('click', function () {
            $(this).remove();
        });
    });
    $('#btnSubmit').bind('click', function () {
        var $btn = $(this),
            $mobile = $('#txtMobile'),
            mobile = $mobile.val();
        if (!S.isMobile(mobile)) {
            S.alert('请输入正确的手机号码！');
            return false;
        }
        $btn.attr('disabled', 'disabled');
        $.post(restUrl + 'teacher-mobile', {
            id: id,
            mobile: mobile
        }, function (json) {
            if (json.status) {
                S.alert('提交成功！', function () {
                    $btn.remove();
                    $mobile.attr('readonly', 'readonly');
                });
            } else {
                S.alert(json.message, function () {
                    $btn.removeAttr('disabled');
                });
            }
        });
    });
    // 分享图片弹框
    $('.image-popup-vertical-fit').magnificPopup({
        type: 'image',
        closeOnContentClick: true,
        mainClass: 'mfp-img-mobile',
        image: {
            verticalFit: true
        }

    });
})(jQuery, SINGER);