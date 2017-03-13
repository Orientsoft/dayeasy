/**
 * 阅卷区域标记
 * @author shay
 * @create 2015/4/9.
 */
(function ($, S) {
    var
        $imgMap = $(".image-map"),
        $indexMap = $(".index-map"),
        $indexList,
        $img = $imgMap.find("img"),
        point = {"x": 0, "y": 0, "width": 120, "height": 26},
        points = [],
        paddingTop = 10,
        paddingLeft = 15,
        bounds = 15,
        tkWidth = 230,
        paperWidth = 780,
        temp = '<div class="index-item"><div class="index-num">{0}</div></div>',
        index = 0,
        uri = S.uri(),
        logger = S.getLogger("marking-area"),
        start = {},
        mouseMoved = 0;
    var initData = function () {
        $(".m-back").bind("click", function () {
            window.location.href = "/work";
        }); //返回列表
        var id = $("#paperId").val(),
            type = uri.type || 0;
        $.ajax({
            url: '/work/marking-area-questions',
            data: {paperId: id, type: type},
            type: 'GET',
            success: function (data) {
                if (data.status) {
                    return;
                }
                if (data.length == 0) {
                    $('.index-loading').html('没有主观题！');
                    $indexMap.append('<div class="index-action"><button class="deyi-btn j-finish no-question">开始批阅</button>');
                    return;
                }
                bindData(data);
            }
        });
    };
    initData();
    var bindData = function (data) {
        var $item;
        //var lastCount = (data.length > 4 ? (data.length % 4) || 4 : 0);
        $(".index-loading").remove();
        $(".js-change").bind("click", function () {
            var batch = uri.batch,
                type = uri.type || 0;
            $.post("/work/picture-change", {batch: batch, type: type}, function (json) {
                if (json && json.status) {
                    $(".image-map img").attr("src", json.message)
                        .bind("load", function () {
                            var $t = $(this);
                            $t.siblings('canvas').height($t.height());
                        });

                }
            });
        });
        for (var i = 0; i < data.length; i++) {
            $item = $(S.format(temp, data[i].index)).data("id", data[i]);
            //if (i == 0)
            //    $item.addClass('active');
            //if (i == data.length - 1 && ((i + 1) % 4 != 0))
            //    $item.addClass('last');
            //if (data.length - i <= lastCount)

            //    $item.addClass('last-line');
            $indexMap.append($item);
        }
        $indexMap.append('<div class="index-action"><button class="deyi-btn j-finish disabled" disabled>完成标记</button><button class="deyi-btn j-reset">重新标记</button></div>');
        $indexList = $indexMap.find('.index-item');
    };
    var timer = setInterval(function () {
        if ($img.width() < 780)
            return false;
        $img.areaSelect({
            initAreas: [],
            deleteMethod: '',//or click
            padding: 3,
            area: {strokeStyle: '#d9534f', lineWidth: 2},
            point: {size: 4, fillStyle: '#d9534f', type: 'rect'},
            create: false,
            drag: true
        });
        clearInterval(timer);
    }, 200);
    /**
     * 画布点击事件
     */
    $(document)
        .delegate("canvas", "click", function (e) {
            logger.info("canvas click!");
            if (points.length >= $indexList.length)
                return;
            //组织拖动点击
            if ($img.data('AreaSelect').status != 'create') {
                logger.info('鼠标移动：' + mouseMoved);
                return;
            }
            var $t = $(this),
                sp = $.extend({}, point),
                offset = $t.offset(),
                $index = $indexList.eq(index),
                prev;
            if (index > 0) {
                prev = points[index - 1];
            }
            sp.x = e.clientX - offset.left - paddingTop;
            sp.y = e.clientY - offset.top - paddingLeft + $(window).scrollTop();
            sp.width = paperWidth - sp.x - bounds;
            //只能向下标记
            if (prev && sp.y < prev.y - point.height) {
                S.msg("坐标点不能高于上一个点！");
                return;
            }
            var data = $index.data('id');
            if (data.t) {
                sp.width = tkWidth;
            } else {
                if (index > 0) {
                    !prev.t && (prev.height = sp.y - prev.y - 10);
                }
            }
            if (index == $indexList.length - 1) {
                sp.height = $t.height() - sp.y - 20;
            }
            //区域判断
            if ($.inArea(sp, points)) {
                S.msg("标记区域发生重叠，请调整！");
                return;
            }
            sp.num = $index.text();
            $img.areaSelect('add', sp);
            sp.id = data.id;
            sp.index = data.index;
            sp.t = data.t;
            points.push(sp);
            $indexList.eq(index).addClass('active');
            index++;
            //标记完成
            if (index >= $indexList.length) {
                $t.unbind('click.areaSelect');
                $('.j-finish').removeClass('disabled').removeAttr('disabled');
                $imgMap.find('canvas').css('cursor', 'default');
                //return;
            }
            //$indexList.eq(index).addClass('active');
        })
        .delegate("canvas", "mousedown.areaSelect", function (e) {
            start = {x: e.clientX, y: e.clientY};
            mouseMoved = 0;
        })
        .delegate("canvas", "mousemove.areaSelect", function (e) {
            mouseMoved = Math.max(Math.abs(e.clientX - start.x), Math.abs(e.clientY - start.y));
        });
    $indexMap
        .delegate('.j-reset', 'click.areaSelect', function () {
            location.reload(true);
        })
        .delegate('.j-finish', 'click.areaSelect', function () {
            if ($(this).hasClass('disabled') || $(this).attr('disabled'))
                return;
            $(this).addClass('disabled').attr('disabled', 'disabled').html('正在提交..');
            $.ajax({
                url: '/work/marking-area-save',
                data: {batch: uri.batch, type: uri.type, areas: S.json(points)},
                type: 'POST',
                success: function (data) {
                    if (data.status) {
                        var url = '/work/marking-online?batch=' + uri.batch;
                        if (uri.type == 2)
                            url += '&type=b';
                        location.href = url;
                        return false;
                    } else {
                        S.alert(data.msg);
                        $(this).removeClass('disabled').removeAttr('disabled').html('完成标记');
                    }
                }
            });
        });
})(jQuery, SINGER);
