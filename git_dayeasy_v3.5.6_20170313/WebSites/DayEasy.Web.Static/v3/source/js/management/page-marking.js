/**
 * 考试 & 推送列表
 * Created by shay on 2017/1/11.
 */
(function ($, S) {
    var $form = $('#searchForm'),
        submit;
    submit = function () {
        $form.submit();
    };
    //撤回
    $('.b-recall').bind('click', function () {
        var batch = $(this).parents("tr").data("batch");
        S.confirm("确认撤回该批次考试", function () {
            $.post("/marking/recall", {batch: batch}, function (json) {
                if (!json.status) {
                    S.msg(json.message);
                    return;
                }
                S.msg("操作成功", 1000, function () {
                    window.location.reload();
                });
            });
        });
    });

    $('#subjectId,#markingStatus,#showAll').bind('change', function () {
        submit();
    });
    $('#keyword').agency({
        add: submit,
        remove: submit
    });
})(jQuery, SINGER);