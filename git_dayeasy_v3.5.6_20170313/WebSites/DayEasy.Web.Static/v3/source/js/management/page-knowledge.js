/**
 * 知识点管理 - 后台
 * Created by shay on 2015/12/9.
 */
(function ($, S) {
    var _uri = S.uri(),
        stage = _uri.stage || 0,
        subject = _uri.subjectId || 0,
        $stage = $('#stage'), $subject = $('#subjectId'),
        showResult,
        addKnowledge,
        editKnowledge,
        moveKnowledge,
        updateStatus,
        questionCount,
        editDialog;
    /**
     * 显示结果
     * @param json
     * @param successMsg
     */
    showResult = function (json, successMsg) {
        if (json.status) {
            editDialog && editDialog.close().remove();
            S.msg(json.message || successMsg, 2000, function () {
                window.location.reload(true);
            });
        } else {
            S.alert(json.message);
            $(editDialog.node).find('[data-id="ok"]')
                .removeClass('disabled')
                .removeAttr('disabled')
                .html(function () {
                    return $(this).data('word') || '确认';
                });
        }
    };
    /**
     * 添加知识点
     * @param parent
     */
    addKnowledge = function (parent) {
        var html = template('editTemplate', {parent: parent});
        var add = function (name, sort) {
            $.post('/sys/kps/add', {
                parentId: parent.id,
                stage: stage,
                subjectId: subject,
                name: name,
                sort: sort
            }, function (json) {
                showResult(json, '添加成功！');
            });
        };
        editDialog = S.dialog({
            title: '添加知识点',
            content: html,
            okValue: '添加',
            ok: function () {
                var name = $('#textName').val(),
                    sort = $('#textSort').val() || 0;
                if (name == '' || S.trim(name) == '') {
                    S.msg('请输入知识点名称！');
                    return false;
                }
                add(name, sort);
                return false;
            },
            cancelValue: '取消',
            cancel: function () {
                this.close().remove();
            }
        });
        editDialog.showModal();
    };
    /**
     * 编辑知识点
     * @param knowledge
     */
    editKnowledge = function (knowledge) {
        var html = template('editTemplate', knowledge);
        var edit = function (name, sort) {
            $.post('/sys/kps/edit', {
                id: knowledge.id,
                name: name,
                sort: sort
            }, function (json) {
                showResult(json, '编辑成功！');
            });
        };
        editDialog = S.dialog({
            title: '编辑知识点',
            content: html,
            okValue: '保存',
            ok: function () {
                var name = $('#textName').val(),
                    sort = $('#textSort').val() || 0;
                if (name == '' || S.trim(name) == '') {
                    S.msg('请输入知识点名称！');
                    return false;
                }
                edit(name, sort);
                return false;
            },
            cancelValue: '取消',
            cancel: function () {
                this.close().remove();
            }
        });
        editDialog.showModal();
    };
    /**
     * 更新知识点状态
     * @param id
     * @param status
     */
    updateStatus = function (id, status) {
        var msg = '';
        switch (status) {
            case 4:
                msg = '您确定要删除该知识点？';
                break;
            case 1:
                msg = '您确定要还原该知识点？';
                break;
            case -1:
                msg = '您确定要彻底删除该知识点吗?(<strong class="text-danger">删除之后不可还原</strong>)';
                break;
        }
        S.confirm(msg, function () {
            $.post('/sys/kps/update-status', {
                    knowledgeId: id,
                    status: status
                }, function (json) {
                    showResult(json, '更新成功！');
                }
            )
        });
    };

    /**
     * 查看试题数量
     * @param code
     * @param callback
     */
    questionCount = function (code, callback) {
        $.post("/sys/kps/question-count", {code: code}, function (json) {
            callback && callback.call(this, json);
        });
    };

    /**
     * 转移知识点
     * @param code
     * @param name
     */
    moveKnowledge = function (code, name) {
        var move, getKnowledge, html, target = {};
        move = function () {
            if (!target || !target.code) {
                showResult({status: false, message: '请选择目标知识点！'});
                return false;
            }
            var msg = S.format('确认将【{0}<small class="text-gray ml5">[{1}]</small>】转移到【{2}】？', name, code, target.name);
            S.confirm(msg, function () {
                $.post('/sys/kps/move', {
                    source: code,
                    target: target.code
                }, function (json) {
                    showResult(json, '转移任务已提交！');
                });
            }, function () {
                showResult({status: false, message: '已取消'});
            });
        };
        getKnowledge = function (keyword, callback) {
            $.get('/sys/kps/list', {
                stage: stage,
                subjectId: subject,
                keyword: keyword
            }, function (json) {
                json.length && callback && callback.call(this, json);
            });
        };
        html = template('moveTemplate', {name: name, code: code});
        editDialog = S.dialog({
            title: '转移知识点',
            content: html,
            okValue: '确认转移',
            ok: function () {
                var $btn = $(this.node).find('[data-id="ok"]');
                $btn
                    .addClass('disabled')
                    .attr('disabled', 'disabled')
                    .data('word', $btn.html())
                    .html('转移中..');
                move();
                return false;
            },
            cancelValue: '取消',
            cancel: function () {
                this.close().remove();
            },
            onshow: function () {
                $(this.node).find('.d-target').bind('keyup', function () {
                    var $input = $(this),
                        keyword = $input.val(),
                        $list = $('.d-knowledges');
                    getKnowledge(keyword, function (list) {
                        $list.removeClass('hide').empty();
                        for (var i = 0; i < list.length; i++) {
                            var item = list[i];
                            if (item.code.indexOf(code) == 0)
                                continue;
                            if (S.keys(item.path).length > 0) {
                                S.each(item.path, function (t) {
                                    item.name = t + '/' + item.name;
                                })
                            }
                            $list.append(S.format('<div class="d-knowledge-item" data-code="{code}">{name}<small class="text-gray ml5">[{code}]</small></div>', item));
                            $list.find('.d-knowledge-item').bind('click', function () {
                                var $t = $(this);
                                target.code = $t.data('code');
                                target.name = $t.html();
                                $list.addClass('hide');
                                $input.val($t.text());
                            });
                        }
                    });
                });
            }
        });
        editDialog.showModal();
    };

    $stage.add($subject).change(function () {
        $("#searchForm").submit();
    });

    //添加子级
    $(".b-add").click(function () {
        var $tr = $(this).parents('tr');
        var id = $tr.data('id');
        var name = $tr.data('name');
        addKnowledge({name: name, id: id});
        return false;
    });

    //添加顶级
    $("#btn-add").click(function () {
        if (stage <= 0 || subject <= 0) {
            S.msg("请先选择学段和科目！");
            return false;
        }
        var stageName = $stage.find(':selected').text(),
            subjectName = $subject.find(':selected').text();
        addKnowledge({name: stageName + ' ' + subjectName, id: 0});
        return false;
    });

    //编辑
    $(".b-edit").click(function () {
        var $tr = $(this).parents("tr"),
            kpId = $tr.data("id"),
            kpText = S.stripTags($tr.data("name")),
            sort = $tr.data("sort");
        editKnowledge({
            id: kpId,
            name: kpText,
            sort: sort
        });
        return false;
    });
    $(".b-count").bind("click", function (e) {
        e.stopPropagation();
        var $t = $(this),
            $tr = $t.parents('tr'),
            code = $tr.data("code"),
            status = $tr.data('status'),
            $node = $t.parent();
        $node.html("...");
        questionCount(code, function (count) {
            $node.html(count);
            if (count == 0) {
                $node.parents('tr').find('td:last').append('<a href="#" data-status="-1" class="b-update text-danger ml20">彻底删除</a>');
            }
            if (count > 0) {
                $node.append('<a href="#" class="b-move ml20">转移</a>');
            }
        });
        return false;
    });

    $(document)
        .delegate('.b-move', 'click', function () {
            var $tr = $(this).parents("tr");
            moveKnowledge($tr.data('code'), $tr.data('name'));
            return false;
        })
        .delegate('.b-update', 'click', function () {
            var $t = $(this),
                kpId = $t.parents("tr").data("id"),
                status = $t.data('status');
            updateStatus(kpId, status);
            return false;
        })
    ;
})(jQuery, SINGER);