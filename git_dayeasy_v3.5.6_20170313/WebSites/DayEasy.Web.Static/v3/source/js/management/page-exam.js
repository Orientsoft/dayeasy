/**
 * Created by shay on 2016/4/26.
 */
(function ($, S) {
    var showDetails, sendExam, deleteExam, logger = S.getLogger('exam'), download;
    /**
     * 展示详情
     */
    showDetails = function ($tr) {
        var show, details = $tr.data('details'), i, name;
        name = $tr.find('td').eq(0).find('a').html();
        /**
         * 展示详情
         */
        show = function () {
            var $table = $('<table class="table table-bordered"><colgroup><col style="width:4em"/><col style="width:15em"/><col style="width:6em"/><col/></colgroup></table>');
            $table.append('<thead><tr><th>科目</th><th>试卷名称</th><th>发起人</th><th>班级详情</th></tr></thead>');
            var $tbody = $('<tbody>');
            for (i = 0; i < details.length; i++) {
                var item = details[i];
                var classes = '';
                for (var j = 0; j < item.jointClasses.length; j++) {
                    classes += S.format('<span class="dy-class-item">{className}<small>({studentCount})</small></span>', item.jointClasses[j]);
                }
                $tbody.append(S.format('<tr><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>',
                    item.subject, item.paperTitle, item.creator.name, classes));
            }
            $table.append($tbody);
            S.dialog({
                title: name,
                content: $table,
            }).showModal();
        };
        if (!details || !details.length) {
            var id = $tr.data('eid');
            $.get('/exam/details', {
                id: id
            }, function (json) {
                details = json.data || [];
                $tr.data('details', details);
                show();
            });
        } else {
            show();
        }
    };
    /**
     * 推送考试
     */
    sendExam = function ($tr) {
        var name = $tr.find('td').eq(0).find('a').html();
        S.confirm(S.format('确认推送"<strong>{0}</strong>"？<label class="text-danger">推送后将不能取消！</label>', name), function () {
            $.post('/exam/send', {
                id: $tr.data('eid')
            }, function (json) {
                if (json.status) {
                    S.alert('推送成功！', function () {
                        location.reload(true);
                    });
                } else {
                    S.alert(json.message);
                }
            });
            return false;
        });
    };
    /**
     * 删除考试
     */
    deleteExam = function ($tr) {
        var name = $tr.find('td').eq(0).find('a').html();
        S.confirm(S.format('确认删除"<strong>{0}</strong>"？<label class="text-danger">删除后将不能还原！</label>', name), function () {
            $.post('/exam/delete', {
                id: $tr.data('eid')
            }, function (json) {
                if (json.status) {
                    S.alert('删除成功！', function () {
                        location.reload(true);
                    });
                } else {
                    S.alert(json.message);
                }
            });
            return false;
        });
    };
    /**
     * 大型考试统计下载
     * @param eid
     */
    download = function (eid) {
        var $form = $('#exportForm');
        if (!$form || !$form.length) {
            var $body = $('body');
            $form = $('<form method="post" target="_blank" class="hide">');
            $body.append($form);
        } else {
            $form.empty();
        }
        //总排名
        $form.attr('action', S.sites.apps + '/ea/ranking-download');
        $form.append('<input name="examId" value="' + eid + '" />');
        $form.submit();
    };
    $('.b-details').bind('click', function () {
        var $tr = $(this).parents('tr');
        showDetails($tr);
        return false;
    });
    $('.b-send').bind('click', function () {
        var $tr = $(this).parents('tr');
        sendExam($tr);
        return false;
    });
    $('.b-delete').bind('click', function () {
        var $tr = $(this).parents('tr');
        deleteExam($tr);
        return false;
    });
    $('.b-export').bind('click', function () {
        var $tr = $(this).parents('tr');
        download($tr.data('eid'));
        return false;
    });
})(jQuery, SINGER);