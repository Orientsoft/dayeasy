/**
 * 完善历程
 * Created by shay on 2016/8/25.
 */
(function ($, S) {
    var stages = ["小学", "初中", "高中"],
        $node = $('.my-history-pop'),
        year = new Date().getFullYear(),
        years = [],
        eventBind,
        searchAgency,
        deleteRelation,
        updateRelation,
        addRelation;
    for (var i = 0; i < 50; i++) {
        years.push(year - i);
    }
    /**
     * 搜索机构
     * @returns {boolean}
     */
    searchAgency = function (keyword, callback) {
        if (!keyword)
            return false;
        $.ajax({
            type: "GET",
            data: {keyword: keyword},
            url: S.sites.main + "/agency-search",
            dataType: "jsonp",
            success: function (json) {
                callback && callback.call(this, json);
            }
        });
    };
    /**
     * 添加关系
     * @param data
     * @param callback
     */
    addRelation = function (data, callback) {
        $.post('/user/add-relation', data, function (json) {
            if (!json.status) {
                json.message && S.alert(json.message);
                callback && callback.call(this);
                return false;
            }
            S.msg('添加成功~!', 2000, function () {
                $node.html('<div class="dy-loading"><i></i></div>');
                $.get('/user/history', {}, function (html) {
                    $node.replaceWith(html);
                });
                S.loadAgency && S.loadAgency.call(this);
            });
        });
    };

    /**
     * 删除关系
     * @param id
     * @param callback
     */
    deleteRelation = function (id, callback) {
        S.confirm('确认删除该历程？', function () {
            $.post('/user/remove-relation', {
                id: id
            }, function (json) {
                if (json.status) {
                    S.msg('删除成功！', 2000, function () {
                        callback && callback.call(this);
                        $('.process-row[data-aid="' + id + '"]').remove();
                        S.loadAgency && S.loadAgency.call(this);
                    });
                    return true;
                }
                json.message && S.alert(json.message);
            });
        });
    };
    /**
     * 更新关系
     * @param data
     * @param callback
     */
    updateRelation = function (data, callback) {
        if (!data || !data.id) {
            S.msg('学校关系无效！');
            callback && callback.call(this);
            return false;
        }
        $.post('/user/update-relation', data, function (json) {
            if (!json || !json.status) {
                json.message && S.msg(json.message);
                callback && callback.call(this, false);
                return false;
            }
            S.msg('更新成功！');
            S.loadAgency && S.loadAgency.call(this);
            callback && callback.call(this, true);
        });
    };

    /**
     * 事件绑定
     */
    eventBind = function () {
        /**
         * 编辑/添加操作
         */
        $node.find('.b-edit,.add-school,.btn-cancel').bind('click', function () {
            var $list = $(this).parents('.con-list'),
                $time = $list.find('.time-class'),
                $edit = $list.find('.edit-box');
            $edit.add($time).toggleClass('hide');
        });
        /**
         * 选择器插件
         */
        $node.find('.d-time').monthPicker({
            years: years,
            topOffset: 20
        });
        /**
         * 删除
         */
        $node.find('.b-delete').bind('click', function () {
            var $t = $(this),
                id = $t.parents('.con-list').data('aid');
            deleteRelation(id, function () {
                $t.parents('.con-list').remove();
            });
        });
        //编辑
        $node.find('.btn-submit').bind('click', function () {
            var $btn = $(this),
                $con = $btn.parents('.con-list'),
                id = $con.data('aid'), start, end;
            if (!id) return false;
            start = $con.find('.t-start').val();
            end = $con.find('.t-end').val();
            $btn.disableField('稍候..');
            updateRelation({
                id: id, start: start, end: end
            }, function (status) {
                $btn.undisableFieldset();
                if (status) {
                    var $time = $con.find('.time-class'),
                        $edit = $con.find('.edit-box');
                    start = start.replace('-', '.');
                    end = end ? end.replace('-', '.') : '至今';
                    $time.html(S.format('<span>{0}</span> - <span>{1}</span>', start, end));
                    $time.add($edit).toggleClass('hide');
                }
            });
        });
        //搜索 - 回车
        $inputPart.find('input').bind('keypress', function (e) {
            if (e.keyCode == 13) {
                var keyword = $inputPart.find('input').val();
                searchAgency(keyword, function (list) {
                    showAgency(list);
                });
            }
        });
        //搜索按钮
        $inputPart.find('button').bind('click', function () {
            var keyword = $inputPart.find('input').val();
            searchAgency(keyword, function (list) {
                showAgency(list);
            });
        });
    };
    var $inputPart = $node.find('.input-part'),
        $editBox = $inputPart.parents('.edit-box'),
        showAgency = function (list) {
            //console.log(list);
            if (!list || !list.length) {
                S.msg('没有查询到相关机构');
                return false;
            }
            var $list = $inputPart.next();
            $list.empty();
            S.each(list, function (item) {
                item.stageCn = stages[item.stage - 1];
                $list.append(S.format('<li title="{name}" data-aid="{id}"><span class="stage-{stage}">{stageCn}</span><span class="name">{name}</span></li>', item));
            });
            $inputPart.next().removeClass('hide').slideDown();
            //搜索列表点击事件
            $list.off('click', 'li').on('click', 'li', function () {
                var $t = $(this), id = $t.data('aid');
                $list.addClass('hide');
                $list.parent().addClass('hide');
                $editBox.find('.agency').html($t.html()).data('aid', id).attr('title', $t.attr('title'));
                $editBox.find('.selected-wrap,.edit-date').removeClass('hide');
                $editBox.find('.btn-submit').undisableFieldset();
            });
            //编辑学校事件
            $editBox.find('.agency-wrap .dy-icon-index-edit').unbind('click').bind('click', function () {
                $list.parent().removeClass('hide');
                $editBox.find('.selected-wrap,.edit-date').addClass('hide');
            });
            //选择当前学校事件
            $editBox.find('.agency-status input').unbind('change').bind('change', function () {
                var $time = $editBox.find('.edit-date'),
                    checked = this.checked;
                $time.find('.current').toggleClass('hide', !checked);
                $time.find('.history').toggleClass('hide', checked);
            });
            //确认添加
            $editBox.find('.btn-submit').unbind('click').bind('click', function () {
                var $btn = $(this),
                    agencyId = $editBox.find('.agency').data('aid'),
                    isCurrent = $editBox.find('.agency-status input').get(0).checked,
                    start, end;
                if (!agencyId || $editBox.find('.selected-wrap').hasClass('hide')) {
                    S.msg('请选择学校~!');
                    return false;
                }
                if (isCurrent) {
                    start = $editBox.find('.current .t-start').val();
                    if (!start) {
                        S.msg('请输入起始时间！');
                        return false;
                    }
                } else {
                    var $timeBox = $editBox.find('.history');
                    start = $timeBox.find('.t-start').val();
                    end = $timeBox.find('.t-end').val();
                    if (!start || !end) {
                        S.msg('请输入起始时间和截至时间！');
                        return false;
                    }
                }
                $btn.disableField('稍候..');
                var postData = {
                    agencyId: agencyId,
                    status: isCurrent ? 0 : 1,
                    start: start,
                    end: end
                };
                addRelation(postData, function () {
                    $btn.undisableFieldset();
                });
            });
        };
    eventBind();
})(jQuery, SINGER);