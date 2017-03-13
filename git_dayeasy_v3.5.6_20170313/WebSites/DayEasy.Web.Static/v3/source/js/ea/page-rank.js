/**
 * 教务管理 - 大型考试统计
 * Created by shay on 2016/3/25.
 */
(function ($, S) {
    var $list = $('.dy-table-body tr'),
        logger = S.getLogger('exam-rank'),
        $showList,
        classId,
        studentName,
        ranksSearch,
        loadClassKeys,
        loadClassLayers,
        loadSubjects,
        bindSegments,
        panelIndex = 0,
        loadedClass = false,
        loadedSubject = false,
        $rank = $('.dy-ea-ranks'),
        examId = $rank.data('eid'),
        examName = $rank.data('name'),
        subjectName = '',
        classKeysParams = {
            examId: examId,
            keyType: 0,
            keyScore: 80,
            scoreA: 60,
            unScoreA: 40
        },
        classLayersParams = {
            examId: examId,
            layerA: 10,
            layerB: 30,
            layerC: 30,
            layerD: 20,
            layerE: 10
        },
        subjectParams = {
            examId: examId,
            examSubjectId: '',
            keyType: 0,
            keyScore: 80,
            scoreA: 60,
            unScoreA: 40
        };
    var toPercent = function (json) {
        var percent = function (scale) {
            return (scale * 100).toFixed(2) + '%';
        };
        json.averageRatio = json.averageRatio.toFixed(5);
        json.averageRatioDiff = json.averageRatioDiff.toFixed(5);
        json.keyScale = percent(json.keyScale);
        json.keyScaleDiff = percent(json.keyScaleDiff);
        json.aScale = percent(json.aScale);
        json.aScaleDiff = percent(json.aScaleDiff);
        json.unaScale = percent(json.unaScale);
        json.unaScaleDiff = percent(json.unaScaleDiff);
        return json;
    };
    var layerRanks = function () {
        var ranks = [], prefix = 1;
        for (var i = 0; i < arguments.length; i++) {
            var rank = arguments[i];
            if (rank > 0) {
                ranks.push(S.format('{0}-{1}名', prefix, prefix + rank - 1));
                prefix += rank;
            } else {
                ranks.push('-')
            }
        }
        return ranks;
    };
    /**
     * 排名搜索
     */
    ranksSearch = function () {
        var selector = '.dy-table-body tr';
        if (classId && classId != -1) {
            selector += '[data-class="' + classId + '"]';
        }
        if (studentName) {
            selector += '[data-student*=' + studentName + ']';
        }
        $showList = $(selector);
        $showList.show();
        $list.not($showList).hide();
        $('.dy-table-foot strong').html($showList.length);
    };
    /**
     * 计算班级重点率
     */
    loadClassKeys = function (callback) {
        var $classKeys = $('#classKeys'),
            $keyType = $classKeys.find('select'),
            $keyScore = $classKeys.find('input:eq(0)'),
            $scoreA = $classKeys.find('input:eq(1)'),
            $unScoreA = $classKeys.find('input:eq(2)');
        classKeysParams.keyType = parseInt($keyType.val());
        classKeysParams.keyScore = parseFloat($keyScore.val());
        classKeysParams.scoreA = parseFloat($scoreA.val());
        classKeysParams.unScoreA = parseFloat($unScoreA.val());
        if (!S.isNumber(classKeysParams.keyScore)
            || classKeysParams.keyScore <= 0
            || !S.isNumber(classKeysParams.scoreA)
            || !S.isNumber(classKeysParams.unScoreA)) {
            S.alert('参数异常，请重新输入！');
            logger.info(classKeysParams);
            callback && S.isFunction(callback) && callback.call(this);
            return false;
        }
        if (classKeysParams.keyType == 1 && classKeysParams.keyScore > 100) {
            S.alert('重点比例不能超过100%！');
            callback && S.isFunction(callback) && callback.call(this);
            return false;
        }
        $.get('/ea/class-analysis-keys', classKeysParams, function (json) {
            var $body = $('#classKeysBody'),
                $foot = $('#classKeysFoot'),
                trTemp = '<tr>' +
                    '<td>{className}</td>' +
                    '<td>{teacher}</td>' +
                    '<td>{totalCount}</td>' +
                    '<td>{studentCount}</td>' +
                    '<td>{averageScore}</td>' +
                    '<td>{averageScoreDiff}</td>' +
                    '<td>{averageRatio}</td>' +
                    '<td>{averageRatioDiff}</td>' +
                    '<td>{keyCount}</td>' +
                    '<td>{keyScale}</td>' +
                    '<td>{keyScaleDiff}</td>' +
                    '<td>{aCount}</td>' +
                    '<td>{aScale}</td>' +
                    '<td>{aScaleDiff}</td>' +
                    '<td>{unaCount}</td>' +
                    '<td>{unaScale}</td>' +
                    '<td>{unaScaleDiff}</td>' +
                    '</tr>',
                footTemp = '<tr class="th-center">' +
                    '<th>{className}</th>' +
                    '<th></th>' +
                    '<th>{totalCount}</th>' +
                    '<th>{studentCount}</th>' +
                    '<th>{averageScore}</th>' +
                    '<th></th>' +
                    '<th></th>' +
                    '<th></th>' +
                    '<th>{keyCount}</th>' +
                    '<th>{keyScale}</th>' +
                    '<th></th>' +
                    '<th>{aCount}</th>' +
                    '<th>{aScale}</th>' +
                    '<th></th>' +
                    '<th>{unaCount}</th>' +
                    '<th>{unaScale}</th>' +
                    '<th></th>' +
                    '</tr>';
            $body.empty();
            $foot.empty();
            for (var i = 0; i < json.length; i++) {
                var item = toPercent(json[i]);
                if (i == json.length - 1) {
                    $foot.append(S.format(footTemp, item));
                } else {
                    $body.append(S.format(trTemp, item));
                }
            }
            callback && S.isFunction(callback) && callback.call(this, json);
        });
    };
    /**
     * 计算班级分层分析
     */
    loadClassLayers = function (callback) {
        var $classLayers = $('#classLayers'),
            $scoreA = $classLayers.find('input:eq(0)'),
            $scoreB = $classLayers.find('input:eq(1)'),
            $scoreC = $classLayers.find('input:eq(2)'),
            $scoreD = $classLayers.find('input:eq(3)'),
            $scoreE = $classLayers.find('input:eq(4)');
        classLayersParams.layerA = parseFloat($scoreA.val());
        classLayersParams.layerB = parseFloat($scoreB.val());
        classLayersParams.layerC = parseFloat($scoreC.val());
        classLayersParams.layerD = parseFloat($scoreD.val());
        classLayersParams.layerE = parseFloat($scoreE.val());
        if (!S.isNumber(classLayersParams.layerA)
            || !S.isNumber(classLayersParams.layerB)
            || !S.isNumber(classLayersParams.layerC)
            || !S.isNumber(classLayersParams.layerD)
            || !S.isNumber(classLayersParams.layerE)) {
            S.alert('参数异常，请重新输入！');
            logger.info(classLayersParams);
            callback && S.isFunction(callback) && callback.call(this);
            return false;
        }
        if (classLayersParams.layerA +
            classLayersParams.layerB +
            classLayersParams.layerC +
            classLayersParams.layerD +
            classLayersParams.layerE != 100) {
            S.alert('各分层的总和必须为100%，请重新输入！');
            callback && S.isFunction(callback) && callback.call(this);
            return false;
        }
        $.get('/ea/class-analysis-layers', classLayersParams, function (json) {
            var $body = $('#classLayersBody'),
                $foot = $('#classLayersFoot'),
                $heads = $('.dy-table-head em'),
                trTemp = '<tr>' +
                    '<td>{className}</td>' +
                    '<td>{teacher}</td>' +
                    '<td>{totalCount}</td>' +
                    '<td>{studentCount}</td>' +
                    '<td>{layerA}</td>' +
                    '<td>{layerB}</td>' +
                    '<td>{layerC}</td>' +
                    '<td>{layerD}</td>' +
                    '<td>{layerE}</td>' +
                    '</tr>',
                footTemp = '<tr class="th-center">' +
                    '<th>{className}</th>' +
                    '<th></th>' +
                    '<th>{totalCount}</th>' +
                    '<th>{studentCount}</th>' +
                    '<th>{layerA}</th>' +
                    '<th>{layerB}</th>' +
                    '<th>{layerC}</th>' +
                    '<th>{layerD}</th>' +
                    '<th>{layerE}</th>' +
                    '</tr>';
            $body.empty();
            $foot.empty();
            for (var i = 0; i < json.length; i++) {
                var item = json[i];
                if (i == json.length - 1) {
                    $foot.append(S.format(footTemp, item));
                    var ranks = layerRanks(item.layerA, item.layerB, item.layerC, item.layerD, item.layerE);
                    for (var j = 0; j < $heads.length; j++) {
                        $heads.eq(j).html(ranks[j]);
                    }
                } else {
                    $body.append(S.format(trTemp, json[i]));
                }
            }
            callback && S.isFunction(callback) && callback.call(this, json);
        });
    };
    /**
     * 计算科目分析
     */
    loadSubjects = function (callback) {
        var $subjectKeys = $('#subjectKeys'),
            $subject = $subjectKeys.find('select:eq(0)'),
            $keyType = $subjectKeys.find('select:eq(1)'),
            $keyScore = $subjectKeys.find('input:eq(0)'),
            $scoreA = $subjectKeys.find('input:eq(1)'),
            $unScoreA = $subjectKeys.find('input:eq(2)');
        subjectParams.examSubjectId = $subject.val();
        subjectName = $subject.find('option:selected').text();
        subjectParams.keyType = parseInt($keyType.val());
        subjectParams.keyScore = parseFloat($keyScore.val());
        subjectParams.scoreA = parseFloat($scoreA.val());
        subjectParams.unScoreA = parseFloat($unScoreA.val());
        if (!S.isNumber(subjectParams.keyScore)
            || subjectParams.keyScore <= 0
            || !S.isNumber(subjectParams.scoreA)
            || !S.isNumber(subjectParams.unScoreA)) {
            S.alert('参数异常，请重新输入！');
            logger.info(subjectParams);
            callback && S.isFunction(callback) && callback.call(this);
            return false;
        }
        if (subjectParams.keyType == 1 && subjectParams.keyScore > 100) {
            S.alert('重点比例不能超过100%！');
            callback && S.isFunction(callback) && callback.call(this);
            return false;
        }
        $.get('/ea/subject-analysis', subjectParams, function (json) {
            var $body = $('#subjectKeysBody'),
                $foot = $('#subjectKeysFoot'),
                trTemp = '<tr>' +
                    '<td>{className}</td>' +
                    '<td>{teacher}</td>' +
                    '<td>{totalCount}</td>' +
                    '<td>{studentCount}</td>' +
                    '<td>{averageScore}</td>' +
                    '<td>{averageScoreDiff}</td>' +
                    '<td>{averageRatio}</td>' +
                    '<td>{averageRatioDiff}</td>' +
                    '<td>{averageScoreA}</td>' +
                    '<td>{averageScoreB}</td>' +
                    '<td>{keyCount}</td>' +
                    '<td>{keyScale}</td>' +
                    '<td>{keyScaleDiff}</td>' +
                    '<td>{aCount}</td>' +
                    '<td>{aScale}</td>' +
                    '<td>{aScaleDiff}</td>' +
                    '<td>{unaCount}</td>' +
                    '<td>{unaScale}</td>' +
                    '<td>{unaScaleDiff}</td>' +
                    '</tr>',
                footTemp = '<tr class="th-center">' +
                    '<th>{className}</th>' +
                    '<th></th>' +
                    '<th>{totalCount}</th>' +
                    '<th>{studentCount}</th>' +
                    '<th>{averageScore}</th>' +
                    '<th></th>' +
                    '<th></th>' +
                    '<th></th>' +
                    '<th>{averageScoreA}</th>' +
                    '<th>{averageScoreB}</th>' +
                    '<th>{keyCount}</th>' +
                    '<th>{keyScale}</th>' +
                    '<th></th>' +
                    '<th>{aCount}</th>' +
                    '<th>{aScale}</th>' +
                    '<th></th>' +
                    '<th>{unaCount}</th>' +
                    '<th>{unaScale}</th>' +
                    '<th></th>' +
                    '</tr>',
                showSections;
            $body.empty();
            $foot.empty();
            for (var i = 0; i < json.length; i++) {
                var item = toPercent(json[i]);
                if (i == json.length - 1) {
                    $foot.append(S.format(footTemp, item));
                } else {
                    $body.append(S.format(trTemp, item));
                }
                if (!showSections && item.segmentA)
                    showSections = true;
            }
            bindSegments(json);
            $('#segmentSections').toggleClass('hide', !showSections);
            if (showSections) {
                $('#segmentSections').val(0);
            }
            callback && S.isFunction(callback) && callback.call(this, json);
        });
    };
    /**
     * 绑定分数段统计
     * @param json
     * @param sectionType
     */
    bindSegments = function (json, sectionType) {
        var $segmentBody = $('#subjectSegmentsBody'),
            $segmentFoot = $('#subjectSegmentsFoot'),
            $segmentHeader = $('#segmentsHeader'),
            cols = 0, key, i, j;
        if (json) {
            $segmentBody.data('segments', json);
        } else {
            json = $segmentBody.data('segments');
        }
        $segmentBody.empty();
        $segmentFoot.empty();
        $segmentHeader.find('.dy-segment-th').remove();
        $('.dy-segments colgroup .dy-segment-col').remove();
        if (!json || !json.length)
            return false;
        var gradeSegments = json[json.length - 1],
            segmentName = "segment",
            segment;
        if (sectionType) {
            switch (sectionType) {
                case 1:
                    segmentName = "segmentA";
                    break;
                case 2:
                    segmentName = "segmentB";
                    break;
            }
        }
        segment = gradeSegments[segmentName];
        if (!segment)
            return false;
        var foot = S.format('<tr class="th-center"><th>{className}</th><th></th><th>{totalCount}</th><th>{studentCount}</th>', gradeSegments);
        for (key in segment) {
            if (!segment.hasOwnProperty(key))
                continue;
            cols++;
            foot += '<th>' + segment[key] + '</th>';
            $segmentHeader.find('tr').append('<th class="dy-segment-th">' + key + '</th>');
        }
        $('.dy-segments table').css({
            width: Math.max(1136, 396 + cols * 70)
        });
        for (i = 0; i < cols; i++) {
            $('.dy-segments colgroup').append('<col class="dy-segment-col" style="width:70px" />');
        }
        foot += '</tr>';
        $segmentFoot.append(foot);
        for (i = 0; i < json.length - 1; i++) {
            var item = json[i];
            segment = item[segmentName];
            var tr = S.format('<tr><td>{className}</td><td>{teacher}</td><td>{totalCount}</td><td>{studentCount}</td>', item);
            if (segment) {
                //分数段统计
                for (key in segment) {
                    if (!segment.hasOwnProperty(key))
                        continue;
                    tr += '<td>' + segment[key] + '</td>';
                }
            } else {
                for (j = 0; j < cols; j++) {
                    tr += '<td>0</td>';
                }
            }
            tr += '</tr>';
            $segmentBody.append(tr);
        }
    };
    $('.dy-table-body').bind('scroll', function (e) {
        var $t = $(this),
            left = $t.scrollLeft(),
            $head = $t.siblings('.dy-table-head').find('table');
        $head.css({'margin-left': -left});
        console.log(left);
    });
    $('.b-class-list').bind('change', function () {
        classId = $(this).val();
        ranksSearch();
    });
    $('.btn-search i').bind('click', function () {
        var $input = $(this).siblings('input');
        studentName = singer.trim($input.val());
        ranksSearch();
    });
    $('.btn-search input').bind('keyup', function (e) {
        if (e.keyCode == 13) {
            studentName = singer.trim($(this).val());
            ranksSearch();
        }
    });
    $('.dy-ranks-nav a').bind('click', function () {
        var $li = $(this).parents('li'),
            $list = $('.dy-ranks-nav li');
        if ($li.hasClass('active')) {
            return false;
        }
        panelIndex = $list.index($li);
        if (panelIndex == 1 && !loadedClass) {
            loadedClass = true;
            loadClassKeys(function () {
                loadClassLayers();
            });
        } else if (panelIndex == 2 && !loadedSubject) {
            loadedSubject = true;
            loadSubjects();
        }
        $list.removeClass('active');
        $li.addClass('active');
        var $panels = $('.dy-ranks-panel'),
            $current = $panels.eq(panelIndex);
        $panels.addClass('hide');
        $current.removeClass('hide');
        return false;
    });
    $('#classKeys').on('click', '.dy-btn', function () {
        var $t = $(this);
        $t.disableField('计算中..');
        loadClassKeys(function () {
            $t.undisableFieldset();
        });
    });
    $('#classLayers').on('click', '.dy-btn', function () {
        var $t = $(this);
        $t.disableField('计算中..');
        loadClassLayers(function () {
            $t.undisableFieldset();
        });
    });
    $('#subjectKeys').on('click', '.dy-btn', function () {
        var $t = $(this);
        $t.disableField('计算中..');
        loadSubjects(function () {
            $t.undisableFieldset();
        });
    });
    $('.dy-key-type').bind('change', function () {
        var $t = $(this);
        $t.next().toggleClass('add-right', $t.val() == 1);
    });
    $('#segmentSections').bind('change', function () {
        var type = $(this).val();
        bindSegments(undefined, ~~type);
    });
    $('.b-export').bind('click', function () {
        var $form = $('#exportForm');
        if (!$form || !$form.length) {
            var $body = $('body');
            $form = $('<form method="post" target="_blank" class="hide">');
            $body.append($form);
        } else {
            $form.empty();
        }
        switch (panelIndex) {
            case 0:
                $form.attr('action', '/ea/ranking-download');
                $form.append('<input name="examId" value="' + examId + '" />');
                break;
            case 1:
                $form.attr('action', '/ea/class-analysis-download');
                $form.append('<input name="keys" value="' + S.json(classKeysParams) + '" />');
                $form.append('<input name="layer" value="' + S.json(classLayersParams) + '" />');
                $form.append('<input name="name" value="' + examName + '" />');
                break;
            case 2:
                $form.attr('action', '/ea/subject-analysis-download');
                $form.append('<input name="keys" value="' + S.json(subjectParams) + '" />');
                $form.append('<input name="name" value="' + examName + '-' + subjectName + '" />');
                break;
        }
        $form.submit();
    });
})(jQuery, SINGER);