/**
 * 异步任务
 * Created by shay on 2016/11/14.
 */
(function ($, S) {
    $('.j-update').bind('click', function () {
        var $t = $(this),
            $tr = $t.parents('tr'),
            priority = $t.data('priority'),
            id = $tr.data('mid');
        $.post('/missions/update', {
            id: id,
            priority: priority
        }, function (json) {
            if (json && json.status) {
                S.msg('更新成功');
                $tr.find('td:eq(3)').html(priority);
                $t.data('priority', priority + 1);
                return false;
            }
            S.alert(json.message || '更新失败');
        });
        return false;
    });
    $('.j-reset').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('mid');
        S.confirm('确认要重置该异步任务？', function () {
            $.post('/missions/reset', {id: id}, function (json) {
                if (json && json.status) {
                    S.msg('重置成功', 2000, function () {
                        location.reload(true)
                    });
                    return false;
                }
                S.alert(json.message || '更新失败');
            });
        });
        return false;
    });
    $('.d-params').bind('click', function () {
        var data = $(this).html();
        var d = S.dialog({
            title: '任务参数',
            skin: 'd-dialog',
            width: 600,
            height: 350
        }).showModal();
        d.content(new JsonFormat(data).toString());
        return false;
    });
    $('.j-logs').bind('click', function () {
        var $log = $(this).next();
        var html = $log.html().replace(/\n/g, '<br/>');
        var d = S.dialog({
            title: '任务日志',
            skin: 'd-dialog',
            width: 600,
            height: 350
        }).showModal();
        d.content(html);
        return false;
    });
})(jQuery, SINGER);