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
        point = {"x": 0, "y": 0, "width": 120, "height": 40},
        points = [],
        paddingTop = 10,
        paddingLeft = 15,
        bounds = 15,
        tkWidth = 230,
        paperWidth = 780,
        temp = '<div class="index-item"><div class="index-num{1}">{0}</div></div>',
        index = 0,
        uri = S.uri(),
        logger = S.getLogger("marking-area"),
        start = {},
        initArea = [],
        mouseMoved = 0,
        paperType;
    if (uri.isJoint === "true") {
        $imgMap.append('<div class="image-mask">');
    }
    var initData = function () {
        //返回列表
        $(".m-back").bind("click", function () {
            if (window.history.length > 1) {
                window.history.back(-1);
                return false;
            }
            location.href = S.sites.apps + '/work';
            return false;
        });
        $.ajax({
            url: '/marking/marking-init',
            data: {batch: uri.batch, type: uri.type, isJoint: uri.isJoint},
            type: 'GET',
            success: function (json) {
                if (!json.status) {
                    S.alert(json.message);
                    return;
                }
                var markingData = json.data;
                paperType = markingData.type;
                if (markingData.questions.length == 0) {
                    $('.index-loading').html('没有主观题！');
                    $indexMap.append('<div class="index-action"><button class="deyi-btn j-finish no-question">开始批阅</button>');
                    return;
                }
                initArea = eval('(' + markingData.areas + ')') || [];
                if (initArea && initArea.length) {
                    for (var i = 0; i < initArea.length; i++) {
                        initArea[i].x = parseFloat(initArea[i].x);
                        initArea[i].y = parseFloat(initArea[i].y);
                        initArea[i].width = parseFloat(initArea[i].width);
                        initArea[i].height = parseFloat(initArea[i].height);
                        initArea[i].index = parseInt(initArea[i].index);
                    }
                }
                points = initArea;
                setPicture(markingData.imageUrl);
                bindData(markingData.questions);
            }
        });
    };
    initData();
    var setPicture = function (picture) {
        if (!picture)
            return false;
        $(".image-map img").attr("src", picture)
            .bind("load", function () {
                $(this).siblings('canvas').css({
                    height: $(this).height(),
                    width: 780
                });
            });
    };
    var bindData = function (data) {
        var $item;
        //var lastCount = (data.length > 4 ? (data.length % 4) || 4 : 0);
        $(".index-loading").remove();
        $(".js-change").bind("click", function () {
            var batch = uri.batch,
                isJoint = S.isUndefined(uri.isJoint) ? false : uri.isJoint;
            $.post("/marking/picture-change", {
                batch: batch,
                type: paperType,
                isJoint: isJoint
            }, function (picture) {
                setPicture(picture);
            });
        });
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            $item = $(S.format(temp, item.index, item.index.length > 3 ? ' num-' + item.index.length : '')).data("id", item);
            $indexMap.append($item);
        }
        $indexMap.append('<div class="index-action"><button class="deyi-btn j-finish disabled" disabled>完成标记</button><button class="deyi-btn j-reset">重新标记</button></div>');
        $indexList = $indexMap.find('.index-item');
        if (initArea && initArea.length) {
            $indexList.addClass('active');
            $('.j-finish').removeClass('disabled').removeAttr('disabled');
            $imgMap.find('canvas').css('cursor', 'default');
            index = $indexList.length - 1;
        }
    };
    var timer = setInterval(function () {
        if ($img.width() < 780)
            return false;
        $img.areaSelect({
            initAreas: initArea,
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
            $img.areaSelect('clear');
            points = [];
            $indexList.removeClass('active');
            $('.j-finish').addClass('disabled').attr('disabled', 'disabled');
            index = 0;
        })
        .delegate('.j-finish', 'click.areaSelect', function () {
            if ($(this).hasClass('disabled') || $(this).attr('disabled'))
                return;
            $(this).addClass('disabled').attr('disabled', 'disabled').html('正在提交..');
            $.ajax({
                url: '/marking/marking-area-save',
                data: {
                    batch: uri.batch,
                    type: paperType,
                    areas: S.json(points)
                },
                type: 'POST',
                success: function (data) {
                    if (data.status) {
                        S.alert('标记完成', function () {
                            if (uri.isJoint) {
                                self.location = document.referrer;
                                return false;
                            }
                            location.href = '/marking?batch=' + uri.batch + '&type=' + paperType;
                            return false;
                        });
                    } else {
                        S.alert(data.message);
                        $(this).removeClass('disabled').removeAttr('disabled').html('完成标记');
                    }
                }
            });
        });
})(jQuery, SINGER);
