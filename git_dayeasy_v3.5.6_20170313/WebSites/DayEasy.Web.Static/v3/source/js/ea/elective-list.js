/**
 * 选课列表
 * Created by shay on 2016/9/28.
 */
(function ($, S) {
    /**
     * 更新状态
     * @param batch
     * @param status
     */
    var updateStatus = function (batch, status) {
        var url = '/elective/' + status;
        $.post(url, {batch: batch}, function (json) {
            if (json && json.status) {
                S.msg('操作成功！', 2000, function () {
                    location.reload(true);
                });
                return false;
            }
            S.alert(json.message || '操作失败，请稍后重试！');
        });
    };
    $('.btn-start').bind('click', function () {
        var $tr = $(this).parents('tr'),
            batch = $tr.data('batch');
        S.confirm('确认开启本次选课吗？<br/>开启后，学生们就可以进行选课了', function () {
            updateStatus(batch, 'start');
        });
        return false;
    });
    $('.btn-close').bind('click', function () {
        var $tr = $(this).parents('tr'),
            batch = $tr.data('batch');
        S.confirm('确认关闭本次选课吗？<br/>关闭后，学生们将不能再进行选课', function () {
            updateStatus(batch, 'close');
        });
        return false;
    });
    $('.btn-delete').bind('click', function () {
        var $tr = $(this).parents('tr'),
            batch = $tr.data('batch');
        S.confirm('确认删除本次选课吗？<br/>删除后本次选课的所有内容都将清空', function () {
            updateStatus(batch, 'delete');
        });
        return false;
    });
})(jQuery, SINGER);