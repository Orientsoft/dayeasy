/**
 * 创建大型考试
 * Created by shay on 2016/4/26.
 */
(function ($, S) {
    var selectCookieName = '__dayeasy_joints',
        selectSubjectCookieName = '__dayeasy_subjects',
        submit = function () {
            $('#searchForm').submit();
        }, i, clearSelect, agency,
        selectList = S.json(S.cookie.get(selectCookieName)) || [],
        subjects = S.json(S.cookie.get(selectSubjectCookieName)) || [],
        $createBtn = $('#createBtn'),
        logger = S.getLogger('exam-create'),
        createExamination;
    if (selectList.length > 0) {
        //绑定
        for (i = 0; i < selectList.length; i++) {
            var $tr = $('tr[data-joint="' + selectList[i] + '"');
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
        subjects = [];
        S.cookie.set(selectCookieName, S.json(selectList));
        S.cookie.set(selectSubjectCookieName, S.json(subjects));
        $createBtn.addClass('disabled').attr('disabled', 'disabled');
        $('.dy-actions em').html(selectList.length);
        $('.dy-table-wrap tr').removeClass('selected').find('input[name="ckbJoint"]').removeAttr('checked');
    };
    $('#keyword').agency({
        init: function (json) {
            agency = json;
        },
        add: submit,
        remove: function () {
            clearSelect();
            submit();
        }
    });
    $('.dy-table-wrap tr').bind('click', function (e) {
        var $tr = $(this), checked, joint,
            $subject = $tr.find('.d-subject');
        checked = !$tr.hasClass('selected');
        if (checked) {
            for (i = 0; i < subjects.length; i++) {
                if (subjects[i].id == $subject.data('sid')) {
                    S.msg('已选择相同科目的协同！');
                    return false;
                }
            }
        }
        $tr.toggleClass('selected');
        joint = $tr.data('joint');
        $tr.find('input[name="ckbJoint"]').get(0).checked = checked;
        if (checked) {
            selectList.push(joint);
            subjects.push({
                id: $subject.data('sid'),
                name: $subject.data('sname')
            });
            subjects.sort(function (a, b) {
                return a.id > b.id ? 1 : -1;
            })
        } else {
            for (i = 0; i < selectList.length; i++) {
                if (selectList[i] === joint) {
                    selectList.splice(i, 1);
                    break;
                }
            }
            for (i = 0; i < subjects.length; i++) {
                if (subjects[i].id == $subject.data('sid'))
                    subjects.splice(i, 1);
            }
        }
        S.cookie.set(selectCookieName, S.json(selectList));
        S.cookie.set(selectSubjectCookieName, S.json(subjects));
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
     * 创建大型考试
     * @param postData
     * @param error
     */
    createExamination = function (postData, error) {
        //logger.info(postData);
        //error && error.call(this);
        $.post('/exam/create', postData, function (json) {
            if (json.status) {
                S.alert('创建考试成功！', function () {
                    clearSelect();
                    location.href = '/exam';
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
            S.alert('请选择至少两次协同阅卷！');
            return false;
        }
        if (!agency || !agency.id) {
            S.alert('请选择机构！');
            return false;
        }
        var subjectStr = '';
        for (i = 0; i < subjects.length; i++) {
            subjectStr += subjects[i].name + '、';
        }
        subjectStr = subjectStr.substring(0, subjectStr.length - 1);
        var html = template('createTpl', {subjectStr: subjectStr});
        var createDialog = S.dialog({
            title: '创建大型考试',
            width: 600,
            content: html,
            onshow: function () {
                var $node = $(this.node),
                    $name = $node.find('.d-name'),
                    $btn = $node.find('.btn-create'),
                    $types = $node.find('input[name=examinationType]');
                var checkStatus = function () {
                    var name = $name.val(),
                        type = $types.filter(':checked').val();
                    if (name && name.length >= 5 && typeof type !== "undefined")
                        $btn.unDisabled();
                    else
                        $btn.disabled();
                };
                $name.bind('change', function () {
                    checkStatus();
                });
                $types.bind('click', function () {
                    checkStatus();
                });
                $btn.bind('click', function () {
                    if ($btn.hasClass('disabled'))
                        return false;
                    var name = $name.val(),
                        type = $types.filter(':checked').val(),
                        postData = {
                            name: name,
                            agencyId: agency.id,
                            agencyName: agency.name,
                            stage: agency.stageCode,
                            subjects: subjectStr,
                            jointBatches: S.json(selectList),
                            type: type
                        };
                    $btn.disabled('稍后..');
                    createExamination(postData, function () {
                        $btn.unDisabled();
                    });
                });
            }
        });
        createDialog.showModal();
    });
})(jQuery, SINGER);