/**
 * 推荐圈子
 * Created by shay on 2016/8/26.
 */
(function ($, S) {
    var getData = function (option, callback) {
        $.get('/user/group-list', option, function (json) {
            if (!json || !json.status) {
                return false;
            }
            callback && callback.call(this, json.data);
        });
        //var list = [];
        //for (var i = 0; i < option.count; i++) {
        //    list.push({
        //        id: '123456',
        //        name: '2017级4班',
        //        count: 12,
        //        owner: '张老师',
        //        level: 1,
        //        logo: 'http://placehold.it/50x50'
        //    })
        //}
        //callback && callback.call(this, list);
    };
    var loadClass = function (year) {
        var $list = $('#classList'),
            $box = $list.parents('.layB'),
            html,
            size = $list.data('size');
        $list.empty().append('<div class="dy-loading"><i></i></div>');
        getData({
            type: 0,
            gradeYear: year,
            count: size * 2
        }, function (list) {
            var len = list.length;
            if (len == 0) {
                $list.empty();
                return false;
            }
            $list.empty();
            var page = Math.ceil(len / size);
            $list.css('width', 790 * page);
            for (var i = 0; i < page; i++) {
                html = template('groupTpl', list.splice(0, size));
                $list.append(html);
            }
        });
    };

    var applyDialog;
    $(".layB").slide({
        mainCell: ".slide",
        autoPlay: false,
        effect: "left",
        delayTime: 400
    }).on('click', '.btn-apply', function () {
        //申请加入
        var $t = $(this),
            id = $t.parents('li').data('gid'),
            html = template('applyTpl', {});
        applyDialog && applyDialog.close().remove();
        applyDialog = S.dialog({
            content: html,
            align: 'bottom',
            quickClose: true,
            okValue: '申请加入',
            cancelValue: '取消',
            //displayCancel: false,
            ok: function () {
                var $node = $(this.node),
                    $btn = $node.find('button[data-id="ok"]'),
                    $name = $node.find('input[name="trueName"]'),
                    $desc = $node.find('textarea');
                if ($name.length > 0 && !/^[\u4e00-\u9fa5]{2,5}$/.test($name.val())) {
                    S.msg('请输入真实姓名！');
                    return false;
                }
                $btn.disableField('稍候..');
                var postData = {
                    groupId: id,
                    message: $desc.val()
                };
                if ($name.length > 0) {
                    postData.trueName = S.trim($name.val());
                }
                $.post('/group/apply', postData, function (json) {
                    if (!json.status) {
                        S.alert(json.message, function () {
                            if (json.message.indexOf('等待审核') >= 0) {
                                applyDialog.close().remove();
                                $t.replaceWith('<span class="text-gray">已申请</span>');
                            }
                        });
                    } else {
                        if (postData.trueName)
                            location.reload(true);
                        else {
                            S.msg('申请成功！', 2000, function () {
                                applyDialog.close().remove();
                                $t.replaceWith('<span class="text-gray">已申请</span>');
                            });
                        }
                    }
                    $btn.undisableFieldset();
                });
                return false;
            },
            cancel: function () {
                applyDialog.close().remove();
            }
        });
        applyDialog.show(this);
        return false;
    });
    $('.data-year').on('click', 'dd', function () {
        var $t = $(this);
        if ($t.hasClass('on'))
            return false;
        $t.addClass('on').siblings('dd').removeClass('on');
        loadClass($t.data('year'));
    });
})(jQuery, SINGER);