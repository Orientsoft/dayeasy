(function ($, S) {
    var chooseQuestion,
        checkStatus,
        addMission,
        deleteMission,
        submitMission,
        addTeacher,
        jointBatch,
        logger,
        allotDialog;
    jointBatch = $('.d-box').data('joint');
    logger = S.getLogger('marking-allot');
    /**
     * 选择题目
     */
    chooseQuestion = function () {
        var $choose = $('.d-choose-questions'),
            $sections = $('.d-task-section'),
            $questions,
            empty = true;
        $choose.empty();
        for (var i = 0; i < $sections.length; i++) {
            var $section = $sections.eq(i),
                $header = $section.find('h4');
            $questions = $section.find('li.checked');
            if ($questions.length > 0) {
                $choose.append($header.clone());
                var $ul = $('<ul class="d-questions">');
                $ul.append($questions.clone());
                $choose.append($ul);
                empty = false;
            }
        }
        empty && $choose.html('<div class="dy-nothing">暂无选中的题目</div>');
        checkStatus();
    };
    /**
     * 选择教师
     */
    checkStatus = function () {
        var $questions = $('.d-choose-questions li.checked'),
            $teachers = $('.d-teachers-area li.checked');
        var $btn = $('#addMission');
        if ($questions.length > 0 && $teachers.length > 0) {
            $btn.removeClass('disabled').removeAttr('disabled');
        } else {
            $btn.addClass('disabled').attr('disabled', 'disabled');
        }
    };
    /**
     * 添加任务
     */
    addMission = function () {
        var $list = $('.d-task-list'),
            $missions = $('.d-task-item'),
            $questions = $('.d-task-choose .d-questions'),
            $teacherArea = $('.d-teachers-area'),
            $teachers = $teacherArea.find('li.checked'),
            i, $section;
        for (i = 0; i < $questions.length; i++) {
            var $item = $questions.eq(i);
            $section = $item.prev();
            //添加新任务： 添加相同教师时，自动合并
            var data = {
                title: '任务' + (S.padLeft($missions.length + i + 1, 2, '0')),
                section: $section.length > 0 ? $section.html() : ''
            };
            var html = template('missionTemplate', data);
            var $task = $(html);
            $task.find('.d-questions').html($item.find('li').clone().removeClass('checked'));
            var $teacherList = $teachers.clone().removeClass('checked');
            $teacherList.find('i').remove();
            $task.find('.d-teachers').html($teacherList);
            $list.append($task);
        }
        $('.d-choose-questions').html('<div class="dy-nothing">暂无选中的题目</div>');
        $teachers.removeClass('checked');
        $('.d-task-left .checked').removeClass('checked').addClass('hide');
        var $sections = $('.d-task-section');
        for (i = 0; i < $sections.length; i++) {
            $section = $sections.eq(i);
            if ($section.find('li').length == $section.find('li.hide').length) {
                $section.addClass('hide');
            }
        }
        if ($('.d-task-left li').length == $('.d-task-left li.hide').length) {
            $('.d-task-area').addClass('hide');
            $('#submitBtn').removeClass('disabled').removeAttr('disabled');
        }
        $('#addMission').addClass('disabled').attr('disabled', 'disabled');
        S.msg('添加成功！');
    };
    /**
     * 删除任务
     */
    deleteMission = function ($mission) {
        var $questions = $mission.find('.d-questions li'),
            $area = $('.d-task-area'), i;
        for (i = 0; i < $questions.length; i++) {
            var $item = $questions.eq(i),
                id = $item.data('qid'),
                $question = $area.find('li[data-qid="' + id + '"]');
            $question.parents('.d-task-questions').prev().removeClass('hide');
            $question.parents('.d-task-section').removeClass('hide');
            $question.removeClass('hide');
        }
        if ($area.hasClass('hide'))
            $area.removeClass('hide');
        $('#submitBtn').addClass('disabled').attr('disabled', 'disabled');
        $mission.remove();
        var $missions = $('.d-task-item');
        for (i = 0; i < $missions.length; i++) {
            var title = '任务' + (S.padLeft(i + 1, 2, '0'));
            $missions.eq(i).find('.d-task-header').html(title);
        }
    };
    /**
     * 提交分配
     */
    submitMission = function () {
        var $list = $('.d-task-item'),
            $item, mission, missions = [], i, j;
        for (i = 0; i < $list.length; i++) {
            $item = $list.eq(i);
            mission = {sectionType: 1, questions: [], teacherIds: []};
            var $section = $item.find('.d-section');
            if ($section.length && $section.html() == 'B卷')
                mission.sectionType = 2;
            var $questions = $item.find('.d-questions li');
            for (j = 0; j < $questions.length; j++) {
                mission.questions.push($questions.eq(j).data('qid'));
            }
            var $teachers = $item.find('.d-teachers li');
            for (j = 0; j < $teachers.length; j++) {
                mission.teacherIds.push($teachers.eq(j).data('uid'));
            }
            missions.push(mission);
        }
        logger.info(missions);
        $.post('/marking/allot-submit', {
            jointBatch: jointBatch,
            missions: encodeURIComponent(S.json(missions))
        }, function (json) {
            if (json.status) {
                S.msg('分配成功！', 2000, function () {
                    $(window).unbind('beforeunload.allot');
                    location.href = '/marking/mission_v2/' + jointBatch;
                });
            } else {
                S.alert(json.message);
                $('#submitBtn').undisableFieldset();
            }
        });
    };
    /**
     * 添加教师
     */
    addTeacher = function (gid, teachers, $teachers) {
        $.post('/marking/allot-add', {
            id: gid,
            teachers: teachers.join(',')
        }, function (json) {
            if (json.status) {
                allotDialog.close().remove();
                var $btn = $teachers.prev().find('.d-add-box');
                for (var i = 0; i < teachers.length; i++) {
                    var $item = $teachers.find('li[data-uid="' + teachers[i] + '"]');
                    $item.find('i').remove();
                    $btn.before($item);
                }
                if ($teachers.find('li').length == 0) {
                    $btn.remove();
                    $teachers.remove();
                }
                S.msg('添加成功！');
            } else {
                S.alert(json.message);
            }
        });
    };
    $('.d-task-type').bind('click', function () {
        var $t = $(this),
            $list = $t.next().find('li:visible'),
            checked = $t.hasClass('checked');
        $t.add($list).toggleClass('checked', !checked);
        chooseQuestion();
    });
    $('.d-task-questions').find('li').bind('click', function () {
        var $t = $(this),
            checked = $t.hasClass('checked'),
            $type = $t.parents('.d-task-questions').prev();
        if (checked) {
            $type.removeClass('checked');
        } else {
            var $list = $t.siblings(':visible'),
                $checked = $t.siblings('.checked');
            if ($list.length == $checked.length)
                $type.addClass('checked');
        }
        $t.toggleClass('checked');
        chooseQuestion();
    });
    $('.d-teachers-area').find('li').bind('click', function () {
        $(this).toggleClass('checked');
        checkStatus();
    });
    $('#addMission').bind('click', function () {
        var $t = $(this);
        if ($t.hasClass('disabled'))
            return false;
        addMission();
        return false;
    });
    $('#submitBtn').bind('click', function () {
        var $t = $(this);
        if ($t.hasClass('disabled'))
            return false;
        $t.disableField('分配中...');
        submitMission();
    });
    $('#cancelBtn').bind('click', function () {
        S.confirm('确认取消分配任务?', function () {
            $(window).unbind('beforeunload.allot');
            location.reload(true);
        })
    });
    $('#completeBtn').bind('click', function () {
        S.confirm('确认完成协同任务分配?', function () {
            $(window).unbind('beforeunload.allot');
            if (document.referrer && document.referrer.indexOf('marking/mission_v2') == -1)
                location.href = document.referrer;
            else
                window.close();
        })
    });
    $('.d-add-box').bind('click', function () {
        var $t = $(this),
            $teachers = $t.parents('.d-teachers-wrap').find('.d-add-teachers'),
            gid = $t.data('gid');
        var $html = $teachers.clone();
        allotDialog = S.dialog({
            title: '添加教师',
            content: $html.html(),
            okValue: '确认添加',
            align: 'bottom left',
            quickClose: true,
            backdropOpacity: 0.5,
            width: 400,
            ok: function () {
                var $checked = $(this.node).find('li.checked'),
                    teachers = [];
                if ($checked.length == 0) {
                    S.msg('请选择要添加的教师！');
                    return false;
                }
                for (var i = 0; i < $checked.length; i++) {
                    teachers.push($checked.eq(i).data('uid'));
                }
                addTeacher(gid, teachers, $teachers);
                return false;
            },
            onshow: function () {
                $(this.node).find('li').bind('click', function () {
                    $(this).toggleClass('checked');
                });
            }
        });
        allotDialog.showModal($t.get(0));
    });
    $(window).bind("beforeunload.allot", function () {
        return '离开当前页面，数据将不会被保存，是否继续？';
    });
    $('.d-task-list')
        .delegate('.d-task-remove', 'click', function () {
            var $mission = $(this).parents('.d-task-item');
            S.confirm('确认要删除该任务？', function () {
                deleteMission($mission);
            });
        })
    ;
})(jQuery, SINGER);