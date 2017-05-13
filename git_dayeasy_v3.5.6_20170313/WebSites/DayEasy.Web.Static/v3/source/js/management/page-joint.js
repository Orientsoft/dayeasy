/**
 * 协同阅卷
 * Created by shay on 2016/4/25.
 */
(function ($, S) {
    var unCache = [],
        showUnSubmit, importData, completeJoint;
    var $form = $('#searchForm'),
        submit;
    submit = function () {
        $form.submit();
    };
    /**
     * 导入数据
     */
    importData = function (joint, paperId) {
        console.log(joint);
        var html = template('importTemp', {
            paperId: paperId,
            joint: joint
        });
        S.dialog({
            title: '导入数据',
            content: html,
            onshow: function () {
                var $el = $(this.node),
                    $btn = $el.find('.btn-choose'),
                    $btnImport = $el.find('.btn-import'),
                    $file = $el.find('#importFile'),
                    $form = $el.find('form');
                $btn.bind('click', function () {
                    $file.click();
                });
                $file.bind('change', function () {
                    var path = $file.val();
                    if (path) {
                        $btnImport.removeClass('hide');
                        $('#fileName').html(path);
                        $el.find('.d-file').removeClass('hide');
                        $btn.html('重新选择');
                        $btnImport.unDisabled();
                    }
                });
                $btnImport.bind('click', function () {
                    $btnImport.disabled('正在导入');
                    $btn.disabled();
                    $form.submit();
                });
                window.importCallback = function (json) {
                    $btnImport.unDisabled();
                    $btn.unDisabled();
                    var showAlert = function (message, cls, time) {
                        var $alert = $(S.format('<div class="alert alert-{1}"><button type="button" class="close" data-dismiss="alert" aria-label="Close"><span aria-hidden="true">×</span></button>{0}</div>', message, cls));
                        $el.find('form').append($alert);
                        if (time < 0)
                            return;
                        S.later(function () {
                            $alert.remove();
                        }, time || 2000);
                    };
                    if (!json || !json.status) {
                        showAlert(json.message || '导入失败', 'danger', -1);
                        return false;
                    }
                    showAlert('导入成功,' + json.message, 'success', -1);
                    if (json.data) {
                        var list = [];
                        S.each(json.data, function (value, key) {
                            list.push({
                                key: key,
                                value: value
                            });
                        });
                        if (list.length) {
                            var html = template('importErrorTemp', list);
                            showAlert(html, 'warning', -1);
                        }
                    }

                    // $form.append(html);
                };
            }
        }).showModal();
    };
    /**
     * 结束阅卷
     */
    completeJoint = function (joint) {
        S.confirm('确认要结束该次协同阅卷？', function () {
            var d = S.dialog({
                content: '正在结束...'
            });
            d.showModal();
            $.post('/joint/complete-joint', {
                jointBatch: joint
            }, function (json) {
                d.close();
                if (json.status) {
                    S.alert('已成功结束');
                    location.reload(true);
                } else {
                    S.alert(json.message || '操作失败');
                }
            });
        });
    };
    //撤回
    $('.a-recall').bind('click', function () {
        var id = $(this).parents("tr").data("jid");
        S.confirm("确认撤回该协同考试", function () {
            $.post("/joint/recall", {
                jointBatch: id
            }, function (json) {
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
    //重置
    $('.j-reset').bind('click', function () {
        var $t = $(this),
            id = $t.parents("tr").data("jid");
        S.confirm('确认要重置该协同？<br /><b class="text-danger">重置后将删除所有相关统计数据，并可以继续批阅。</b>', function () {
            $.post("/joint/reset", {
                jointBatch: id
            }, function (json) {
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
                    showUnSubmit(unCache[i].data);
                    return;
                }
            }
        }
        $.post("/joint/un-submits", {
            jointBatch: id
        }, function (json) {
            if (!json.status) {
                S.msg(json.message);
                return;
            }
            unCache.push({
                id: id,
                data: json.data
            });
            showUnSubmit(json.data);
        });
    });
    //导入数据
    $('.b-import').bind('click', function () {
        var $t = $(this),
            $tr = $t.parents('tr'),
            jointBatch = $tr.data('jid'),
            paperId = $tr.data('paper');
        $t.blur();
        importData(jointBatch, paperId);
    });
    //结束阅卷
    $('.b-complete').bind('click', function () {
        var $t = $(this),
            $tr = $t.parents('tr'),
            jointBatch = $tr.data('jid');
        $t.blur();
        completeJoint(jointBatch);

    });

    showUnSubmit = function (data) {
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