/**
 * 海报详情
 * Created by shay on 2016/9/3.
 */
(function ($, S) { 1
    var id = S.uri().id,
        restUrl = 'http://open' + (S.sites.domain || '.v3.deyi.com') + '/activity/';
    if (!id) {
        location.href = '/poster/make.html';
        return false;
    }
    var $poster = $('#posterImg');
    $.get(restUrl + 'god-poster/' + id, {}, function (json) {
        if (json) {
            $poster.attr('src', json.posterUrl);
            $('#posterImg').parents('.image-popup-vertical-fit').attr('href',json.posterUrl);
            document.title = S.format('我为{school}{name}制作了教师节海报，是第{index}个制作者，快来接力吧！', json);
            var html = S.format('<stong>{school}</stong><span>{name}</span>', json);
            if (json.rank > 0) {
                html += '排名第<b>' + json.rank + '</b>'
            }
            $('.entrance-text').html(html);
        }
    });
    $('#btnMake').bind('click', function () {
        location.href = '/poster';
        return false;
    });
//    $poster.bind('click', function () {
//        var $img = $($poster.get(0).outerHTML),
//            w = Math.min(350, $(window).width() - 20);
//        $img.css({width: w}).attr('data-id', '0');
//        var d = S.dialog({
//            content: $img,
//            quickClose: true,
//            backdropOpacity: 0.7,
//            width: w
//        });
//        $img.bind('load', function () {
//            d.showModal();
//        })
//    });
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