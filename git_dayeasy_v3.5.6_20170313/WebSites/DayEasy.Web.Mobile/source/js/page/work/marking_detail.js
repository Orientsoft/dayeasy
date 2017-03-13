/**
 * Created by shay on 2015/6/9.
 */
(function ($, S) {
    $(".g-doc").addClass("g-max780");
    var pictures, batch, paperId, _init, _bindIcon, _logger;
    _logger = S.getLogger('marking_detail');
    _init = function () {
        _logger.info('init...');
        $.AMUI.progress.start();
        var uri = S.uri();
        batch = uri.batch;
        paperId = uri.paper_id;
        S.api({
            method: 'paper.printdetail',
            batch: batch,
            paper_id: paperId,
            type: 'post'
        }, function (json) {
            $.AMUI.progress.done();
            if (!json || !json.length)
                return false;
            pictures = json;
            var g = pictures[0].g,
                $pills = $('.am-nav-pills'),
                titleList = ['A卷详情', 'B卷详情'];

            if (g == 0) {
                $pills.remove();
            } else {
                $pills.append(S.format('<li class="am-disabled"><a href="#">{0}</a></li>',
                    titleList[0], 1));
                $pills.append(S.format('<li class="am-disabled"><a href="#">{0}</a></li>',
                    titleList[1], 2));
                for (var i = 0; i < pictures.length; i++) {
                    var $li = $pills.find('li').eq(pictures[i].g - 1);
                    $li.removeClass('am-disabled').data('index', i);
                    if (i == 0) {
                        $li.addClass('am-active');
                    }
                }
                $pills.find('li a').bind('click', function () {
                    var $t = $(this).parent();
                    if ($t.hasClass('am-active'))
                        return false;
                    $t.addClass('am-active').siblings().removeClass('am-active');
                    if ($t.hasClass('am-disabled')) {
                        $('.d-icons').html('<div class="q-no-answer">未提交</div>');
                    } else {
                        _bindIcon(~~$t.data('index'));
                    }
                    $(this).blur();
                    return false;
                });
            }
            _bindIcon(0);
            $pills.find('li:first').addClass('am-active');
        }, true);
    };
    _init();
    _bindIcon = function (index) {
        var picture = pictures[index];
        var html = template("d-icon-template", picture);
        $('.d-icons').html(html);
    };
    template.helper('iconImage', function (icon) {
        var baseUrl = 'http://static.dayeasy.net/v1/image/icon/marking/{0}';
        var marks = ['full.png', 'semi.png', 'error.png'];
        if (icon.t <= 2)
            return S.format(baseUrl, marks[icon.t]);
        if (icon.t == 5)
            return S.format(baseUrl, 'brow/' + icon.w);
        return S.format(baseUrl, icon.w);
    });
})(jQuery, SINGER);