/**
 * Created by shay on 2016/5/3.
 */
(function ($, S) {
    var publish = function (id, role, success, error) {
        if (!id || !S.inArray(role, [2, 4])) {
            S.alert('数据异常', function () {
                error && error.call(this);
            });
            return;
        }
        var msg, postAction;
        switch (role) {
            case 2:
                msg = '发布后，<b>参考学生</b>可在相应的<b>班级圈主页动态</b>中<br/>查看自己本次考试的成绩信息。确认发布？';
                break;
            case 4:
                msg = '推送后，<b>各科目教师</b>可以在<b>【报表中心】->【教务处报表】</b>中<br/>查看本次考试的统计报表。确认推送？';
                break;
        }
        S.confirm(msg, function () {
            postAction();
        }, function () {
            error && error.call(this);
        });
        postAction = function () {
            $.post('/ea/publish-exam', {
                examId: id,
                role: role
            }, function (json) {
                if (json.status) {
                    S.msg((role == 2 ? '发布成功' : '推送成功'), 2000, function () {
                        success && success.call(this);
                    });
                } else {
                    S.alert(json.message, function () {
                        error && error.call(this);
                    });
                }
            });
        };
    };
    $('.btn-send').bind('click', function () {
        var $t = $(this),
            id = $t.parents('tr').data('eid');
        $t.blur();
        $t.disableField('稍后..');
        publish(id, 4, function () {
            $t.disableField('已推送');
        }, function () {
            $t.undisableFieldset();
        })
    });
    $('.btn-publish').bind('click', function () {
        var $t = $(this),
            id = $t.parents('tr').data('eid');
        $t.blur();
        $t.disableField('稍后..');
        publish(id, 2, function () {
            $t.disableField('已发布');
        }, function () {
            $t.undisableFieldset();
        })
    });

    var popSelectDialog = null;
    $('.dy-content').on('click', '.pop-clicks', function () {
        var $this = $(this),
            eid = $this.parents('tr').data("eid"),
            title = $this.text(),
            html = template('pop-select-type', {
                examId: eid,
                title: title
            });
        popSelectDialog = S.dialog({
            fixed: true,
            width: 500,
            height: 300,
            content: html
        }).showModal();
        return false;
    });

    $(document).on('click', '.pop-select-close', function () {
        popSelectDialog.close().remove();
    });
})(jQuery, SINGER);