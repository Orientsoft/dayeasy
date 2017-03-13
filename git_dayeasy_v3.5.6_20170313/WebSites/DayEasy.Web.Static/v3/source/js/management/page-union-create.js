/**
 * Created by shay on 2017/1/20.
 */
/**
 * 创建大型考试
 * Created by shay on 2016/4/26.
 */
(function ($, S) {
    var selectCookieName = '__dayeasy_exams', i, clearSelect,
        selectList = S.json(S.cookie.get(selectCookieName)) || [],
        $createBtn = $('#createBtn'),
        logger = S.getLogger('union-create'),
        createUnion;
    if (selectList.length > 0) {
        //绑定
        for (i = 0; i < selectList.length; i++) {
            var $tr = $('tr[data-eid="' + selectList[i] + '"');
            if ($tr.length > 0) {
                $tr.addClass('selected').find('input[name="ckbJoint"]').attr('checked', 'checked');
            }
        }
        $('.dy-actions em').html(selectList.length);
        if (selectList.length > 1) {
            $createBtn.removeClass('disabled').removeAttr('disabled');
        }
    }
    /**
     * 清空选择
     */
    clearSelect = function () {
        selectList = [];
        S.cookie.set(selectCookieName, S.json(selectList));
        $createBtn.addClass('disabled').attr('disabled', 'disabled');
        $('.dy-actions em').html(selectList.length);
        $('.dy-table-wrap tr').removeClass('selected').find('input[name="ckbJoint"]').removeAttr('checked');
    };
    $('.dy-table-wrap tr').bind('click', function (e) {
        var $tr = $(this), checked, examId;
        checked = !$tr.hasClass('selected');
        $tr.toggleClass('selected');
        examId = $tr.data('eid');
        $tr.find('input[name="ckbJoint"]').get(0).checked = checked;
        if (checked) {
            selectList.push(examId);
        } else {
            for (i = 0; i < selectList.length; i++) {
                if (selectList[i] === examId) {
                    selectList.splice(i, 1);
                    break;
                }
            }
        }
        S.cookie.set(selectCookieName, S.json(selectList));
        $('.dy-actions em').html(selectList.length);
        if (selectList.length < 2) {
            $createBtn.addClass('disabled').attr('disabled', 'disabled');
        } else {
            $createBtn.removeClass('disabled').removeAttr('disabled');
        }
    });
    $('.b-clear').bind('click', function () {
        clearSelect();
        $(this).blur();
    });
    /**
     * 创建关联
     * @param postData
     * @param error
     */
    createUnion = function (postData, error) {
        $.post('/exam/add-union', postData, function (json) {
            if (json.status) {
                S.alert('创建关联成功！', function () {
                    clearSelect();
                    location.href = '/exam/unions';
                });
            } else {
                S.alert(json.message, function () {
                    error && error.call(this);
                });
            }
        });
    };
    $createBtn.bind('click', function () {
        if ($createBtn.hasClass('disabled'))
            return false;
        if (selectList.length < 2) {
            S.alert('请选择至少两次大型考试！');
            return false;
        }
        var
            postData = {
                examIds: selectList
            };
        $createBtn.disabled('稍后..');
        createUnion(postData, function () {
            $createBtn.unDisabled();
        });
    });
})(jQuery, SINGER);