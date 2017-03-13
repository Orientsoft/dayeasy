/**
 * Created by shay on 2016/4/25.
 */
(function ($, S) {
    var unCache = [], showUnsubmit;
    var $form = $('#searchForm'),
        submit;
    submit = function () {
        $form.submit();
    };
    //撤回
    $('.a-recall').bind('click', function () {
        var id = $(this).parents("tr").data("jid");
        S.confirm("确认撤回该协同考试", function () {
            $.post("/joint/recall", {jointBatch: id}, function (json) {
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
    $('.j-reset').bind('click', function () {
        var $t = $(this),
            id = $t.parents("tr").data("jid");
        S.confirm('确认要重置该协同？<br /><b class="text-danger">重置后将删除所有相关统计数据，并可以继续批阅。</b>', function () {
            $.post("/joint/reset", {jointBatch: id}, function (json) {
                if (!json.status) {
                    S.msg(json.message);
                    return;
                }
                S.msg("操作成功", 1000, function () {
                    $t.remove();
                });
            });
        });
        return false;
    });
    //未提交名单
    $('.a-unsubmits').bind('click', function () {
        var id = $(this).parents("tr").data("jid");
        if (unCache && unCache.length) {
            for (var i = 0; i < unCache.length; i++) {
                if (unCache[i].id == id) {
                    showUnsubmit(unCache[i].data);
                    return;
                }
            }
        }
        $.post("/joint/un-submits", {jointBatch: id}, function (json) {
            if (!json.status) {
                S.msg(json.message);
                return;
            }
            unCache.push({id: id, data: json.data});
            showUnsubmit(json.data);
        });
    });
    showUnsubmit = function (data) {
        var content = template('unsubmit-template', data);
        S.dialog({
            title: '协同考试《' + data.paperTitle + '》未提交名单',
            content: content
        }).showModal();
    };
    //展开缩起
    $("body").delegate(".unsi-title", "click", function () {
        var $this = $(this);
        var $box = $this.siblings(".unsi-data"),
            $i = $this.find("i");
        if ($box.data("open") == "1") {
            $box.data("open", 0).hide();
            $i.removeClass("fa-chevron-down").addClass("fa-chevron-right");
        } else {
            $box.data("open", 1).show();
            $i.addClass("fa-chevron-down").removeClass("fa-chevron-right");
        }
    });
    $('#keyword').agency({
        add: submit,
        remove: submit
    });
})(jQuery, SINGER);