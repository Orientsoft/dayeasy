/**
 * create by epc 2016/03/11
 */
$(function ($) {
    var getData, pager, checkState, setAction,
        batches = [],
        param = {
            index: 1,
            size: 10
        },
        lock = false,
        S = SINGER,
        $action = $('.dy-actions'),
        $box = $('.data-box'),
        $batchs = $("#batches"),
        $btnRank = $('#btnRank'),
        $total = $('.total-num'),
        $pager = $(".d-pager");
    param.agency_id = $('.dy-content').data('agency');
    S.pageLoading();
    /**
     * 加载数据
     */
    getData = function (index) {
        if (lock) return;
        lock = true;
        if (index)
            param.index = index;
        $batchs.val('');
        $btnRank.addClass('disabled').attr('disabled', 'disabled');
        $pager.hide();
        $action.removeClass('d-fixed');
        $.get('/ea/data', param, function (json) {
            lock = false;
            if (!json.status) {
                S.msg(json.message);
                return;
            }
            $total.html(json.count);
            var templateHtml = template('list-template', json);
            $box.html(templateHtml);
            pager(S.page({
                current: param.index - 1,
                size: param.size,
                total: json.count
            }));
            S.pageLoaded();
            $('body,html').animate({scrollTop: 0}, 500);
        });
    };
    /**
     * 初始化分页
     */
    pager = function (pages) {
        if (pages.data && pages.data.length) {
            $pager.show();
            var $ul = $(".pagination");
            $ul.html("");
            if (pages.prev) {
                $ul.append('<li><a href="javascript:void(0);" data-num="' + (pages.current - 1) + '">«</a></li>');
            }
            for (var i = 0; i < pages.data.length; i++) {
                var g = pages.data[i];
                var li = '<li' + (g.isActive ? ' class="active"' : '') + '><a href="javascript:void(0);" data-num="' + (g.isActive ? '0' : g.page) + '">' + (g.page > 0 ? g.page : '...') + '</a></li>';
                $ul.append(li);
            }
            if (pages.next) {
                $ul.append('<li><a href="javascript:void(0);" data-num="' + (pages.current + 1) + '">»</a></li>');
            }
        } else {
            $pager.hide();
        }
    };
    getData();
    //分页
    $(".pagination").delegate("a", "click", function () {
        var num = parseInt($(this).data("num"));
        if (S.isUndefined(num) || num < 1 || num == param.index) return;
        param.index = num;
        getData();
    });
});