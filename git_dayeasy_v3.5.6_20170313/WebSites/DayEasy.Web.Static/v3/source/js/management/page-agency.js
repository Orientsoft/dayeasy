/**
 * 后台 - 机构管理
 * Created by shay on 2016/8/10.
 */
(function ($, S) {
    var showEdit, edit, updateLogo, $node, areaSelector;
    showEdit = function (data) {
        var html = template('edit-template', data);
        S.dialog({
            title: '编辑机构信息',
            content: html,
            cancelValue: '取消',
            okValue: '保存',
            ok: function () {
                var $node = $(this.node),
                    name = $node.find('#name').val(),
                    summary = $node.find('#summary').val(),
                    logo = $node.find('#logo').val(),
                    sort = ~~$node.find('#sort').val(),
                    $btn = $node.find('button[data-id="ok"]');
                if (!name) {
                    S.alert('请输入机构名称！');
                    return false;
                }
                $btn.disabled('稍后..');
                var submitData = {id: data.id};
                if (data.name != name) {
                    submitData.name = name;
                }
                if (data.summary != summary) {
                    submitData.summary = summary;
                }
                if (data.logo != logo) {
                    submitData.logo = logo;
                }
                if (data.sort != sort) {
                    submitData.sort = sort;
                }
                edit(submitData, function () {
                    $btn.unDisabled();
                });
                return false;
            }
        }).showModal();
    };
    edit = function (data, callback) {
        $.post('/agency/edit', data, function (json) {
            if (!json.status) {
                S.alert(json.message);
                callback && callback.call(this);
                return false;
            }
            S.msg('编辑成功！', 2000, function () {
                location.reload(true);
            });
        })
    };
    updateLogo = function () {
        $(".webuploader-element-invisible").click();
    };
    areaSelector = function ($ele) {
        if (!$ele.length) return false;
        var getAreas, bindAreas;
        getAreas = function (code) {
            $.get('/system/areas', {
                code: code || 0
            }, function (json) {
                if (!json || !json.length) {
                    return false;
                }
                bindAreas(json);
            });
        };
        bindAreas = function (json) {
            var $select = $('<select class="form-control area-item">');
            $select.append('<option value="-1">请选择</option>');
            S.each(json, function (item) {
                $select.append(S.format('<option value="{id}">{name}</option>', item));
            });
            $ele.append($select);
            $select.bind('change', function () {
                var code = $select.val();
                var $next = $select.next();
                for (var i = 0; i < 4; i++) {
                    if (!$next.length) {
                        break;
                    }
                    $next.remove();
                    $next = $select.next();
                }
                code > 0 && getAreas(code);
            });
        };
        getAreas(0);
    };
    $('.b-edit').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('agency'),
            name = $tr.data('name'),
            summary = $tr.data('summary'),
            logo = $tr.data('logo'),
            sort = $tr.data('sort');
        showEdit({
            id: id,
            name: name,
            sort: sort,
            summary: summary,
            logo: logo
        });
        return false;
    });
    $('.b-certificate').bind('click', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('agency');
        var certificate = function () {
            $.post('/agency/certificate', {id: id}, function (json) {
                if (!json.status) {
                    S.alert(json.message);
                    return false;
                }
                S.msg('认证成功！', 2000, function () {
                    location.reload(true);
                });
            });
            return false;
        };
        S.confirm('确定认证该机构？', function () {
            certificate();
        });
    });
    $('.btn-add').bind('click', function () {
        var $t = $(this),
            html = template('addTpl', {});
        $t.blur();
        S.dialog({
            title: '添加机构',
            width: 500,
            content: html,
            onshow: function () {
                $node = $(this.node);
                areaSelector($node.find('#areaSelector'));
                $('.btn-submit').bind('click', function () {
                    var $t = $(this),
                        $name = $node.find('#agencyName'),
                        name = $name.val(),
                        type = $node.find('#agencyType').val(),
                        area = $node.find('#areaSelector > select:last-child').val(),
                        $stages = $node.find('input[name=stage]:checked'),
                        stages = [];
                    $t.blur();
                    if (!name) {
                        S.msg('请输入机构名称', 2000, function () {
                            $name.focus();
                        });
                        return false;
                    }
                    if (type < 0) {
                        S.msg('请选择机构类型');
                        return false;
                    }
                    if (area == "-1") {
                        S.msg('请选择所在区域');
                        return false;
                    }
                    if ($stages.length == 0) {
                        S.msg('请选择机构学段');
                        return false;
                    }
                    $stages.each(function (i, item) {
                        stages.push(item.value);
                    });
                    $t.disabled('稍后..');
                    var postData = {name: name, type: type, code: area, stages: stages};
                    $.post('/agency/add', postData, function (json) {
                        if (json.status) {
                            S.alert('添加成功', function () {
                                location.reload(true);
                            });
                            return false;
                        } else {
                            S.alert(json.message);
                            $t.unDisabled();
                        }
                    })
                });
            }
        }).showModal();
    });
    $(document)
        .delegate('.b-update-logo', 'click', function () {
            updateLogo();
            $node = $(this);
        });
    S.uploader.on("uploadSuccess", function (file, response) {
        if (response.state) {
            var url = response.urls[0];
            $node.prev().val(url);
            $node.attr('src', S.makeThumb(url, 50, 50));
        }
        S.uploader.reset();
    });
})(jQuery, SINGER);