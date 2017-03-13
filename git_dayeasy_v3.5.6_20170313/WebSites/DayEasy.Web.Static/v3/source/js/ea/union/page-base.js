/**
 * Created by shay on 2017/1/19.
 */
(function ($, S) {
    var _url = '/ea/union-data',
        chartApi = {},
        _unionBatch,
        _sourceData,
        totalScore = 0,
        students = {},
        initData,
        _logger = S.getLogger('union-charts');
    /**
     * 数字处理
     * @param num
     * @returns {number}
     */
    var
        round = function (num) {
            return Math.round(num * 100) / 100;
        },
        calcTotal = function (item) {
            if (item.scoreA < 0 && item.scoreB < 0)
                return -1;
            var score = 0;
            if (item.scoreA > 0) score += item.scoreA;
            if (item.scoreB > 0) score += item.scoreB;
            return score;
        };
    /**
     * 初始化数据
     */
    initData = function () {
        S.each(_sourceData.subjects, function (subject) {
            totalScore += subject.score;
        });
        var ranks = {};
        //学生列表
        S.each(_sourceData.students, function (student) {
            var item = {
                id: student.id,
                name: student.name,
                className: student.className,
                rank: student.rank,
                score: student.totalScore,
                subjects: student.subjects
            };
            if (_sourceData.exams.hasOwnProperty(student.examId)) {
                var exam = _sourceData.exams[student.examId];
                item.agencyId = exam.agencyId;
                item.agency = exam.agencyName;
                if (!exam.hasOwnProperty('scores')) {
                    exam.scores = [];
                }
                exam.scores.push({
                    score: item.score,
                    subjects: item.subjects
                });
            }
            S.each(item.subjects, function (subject, id) {
                if (!ranks.hasOwnProperty(id))
                    ranks[id] = [];
                var s = _sourceData.subjects[id];
                subject.name = s.subject;
                subject.isAb = s.isAb;
                ranks[id].push({id: student.id, score: subject.score});
            });
            students[student.id] = item;
        });
        //单科排名
        S.each(ranks, function (subjectRanks, id) {
            subjectRanks = subjectRanks.sort(function (a, b) {
                return a.score > b.score ? -1 : 1;
            });
            var rank = 0, score = 0, index = 0;
            S.each(subjectRanks, function (item) {
                var subject = students[item.id].subjects[id];
                if (item.score < 0) {
                    subject.rank = -1;
                    return;
                }
                index++;
                if (score == 0 || item.score < score) {
                    rank = index;
                    score = item.score;
                }
                subject.rank = rank;
            });
        });
    };
    /**
     * 初始化数据
     * @param batch
     * @param callback
     */
    chartApi.init = function (batch, callback) {
        if (_sourceData) {
            callback && callback.call(this);
            return;
        }
        $.get(_url, {
            unionBatch: batch
        }, function (json) {
            if (!json.status) {
                S.alert(json.message, function () {
                    location.href = '/ea/exam';
                });
                return false;
            }
            _unionBatch = batch;
            _sourceData = json.data;
            initData();
            callback && callback.call(this);
        });
    };
    /**
     * 综合排名
     */
    chartApi.ranks = function () {
        var ranks = {
            count: 0,
            subjects: [],
            students: []
        };
        ranks.count = _sourceData.studentCount;
        S.each(_sourceData.subjects, function (subject) {
            ranks.subjects.push({
                id: subject.id,
                name: subject.subject
            });
        });
        S.each(students, function (student) {
            ranks.students.push(student);
        });
        return ranks;
    };
    /**
     * 学生分数详情
     * @param id
     * @returns {{name: string, subjects: {}}}
     */
    chartApi.studentScores = function (id) {
        var item = {name: '', subjects: {}};
        if (!students.hasOwnProperty(id)) {
            return item;
        }
        var student = students[id];
        item = {
            name: student.name,
            subjects: student.subjects
        };
        return item;
    };
    /**
     * 平均分对比
     * @param subjectId
     */
    chartApi.averageCompare = function (subjectId) {
        var data = {
            isAb: false,
            averages: {average: 0, max: totalScore, list: []}
        };
        if (subjectId) {
            var subject = _sourceData.subjects[subjectId];
            data.isAb = subject.isAb;
            data.averages.max = subject.score;
            if (data.isAb) {
                data.averageA = {average: 0, max: subject.scoreA, list: []};
                data.averageB = {average: 0, max: subject.scoreB, list: []};
            }
            else {
                data.highest = {average: 0, max: subject.score, list: []};
                data.lowest = {average: 0, max: subject.score, list: []};
            }
        } else {
            data.highest = {average: 0, max: totalScore, list: []};
            data.lowest = {average: 0, max: totalScore, list: []};
        }
        //平均分&最高分&最低分 计算
        var tScore = 0, tScoreLen = 0, tScoreA = 0, tScoreALen = 0, tScoreB = 0, tScoreBLen = 0;
        S.each(_sourceData.exams, function (exam) {
            var score = 0, scoreLen = 0, scoreA = 0, scoreALen = 0, scoreB = 0, scoreBLen = 0, highest = 0, lowest = 0;
            S.each(exam.scores, function (item) {
                var scoreItem = item;
                if (subjectId)
                    scoreItem = item.subjects[subjectId];
                if (scoreItem.score < 0)
                    return;
                var total = (subjectId > 0 ? calcTotal(scoreItem) : scoreItem.score);
                if (data.isAb) {
                    score += total;
                    scoreLen++;
                    if (scoreItem.scoreA >= 0) {
                        scoreA += scoreItem.scoreA;
                        scoreALen++;
                    }
                    if (scoreItem.scoreB >= 0) {
                        scoreB += scoreItem.scoreB;
                        scoreBLen++;
                    }
                } else {
                    score += total;
                    scoreLen++;
                    if (total > highest)
                        highest = total;
                    if (total > 0 && (lowest == 0 || lowest > total))
                        lowest = total;
                }
            });
            tScore += score;
            tScoreLen += scoreLen;
            tScoreA += scoreA;
            tScoreALen += scoreALen;
            tScoreB += scoreB;
            tScoreBLen += scoreBLen;
            data.averages.list.push({
                name: exam.agencyName,
                value: round(score / scoreLen)
            });
            if (data.isAb) {
                data.averageA.list.push({
                    name: exam.agencyName,
                    value: round(scoreA / scoreALen)
                });
                data.averageB.list.push({
                    name: exam.agencyName,
                    value: round(scoreB / scoreBLen)
                });
            } else {
                data.highest.list.push({
                    name: exam.agencyName,
                    value: highest
                });
                data.lowest.list.push({
                    name: exam.agencyName,
                    value: lowest
                });
            }
        });
        //average
        data.averages.average = round(tScore / tScoreLen);
        if (data.isAb) {
            data.averageA.average = round(tScoreA / tScoreALen);
            data.averageB.average = round(tScoreB / tScoreBLen);
        }
        return data;
    };

    /**
     * 重点率分析
     * @param subjectId
     * @param option
     */
    chartApi.focusRateAnalysis = function (subjectId, option) {
        var initOption = function () {
            if (subjectId) {
                var subject = _sourceData.subjects[subjectId];
                option.keyScore = subject.score * 0.75;
                option.scoreA = option.unScoreA = 60;
            } else {
                option.keyScore = totalScore * 0.75;
                option.scoreA = option.unScoreA = 60;
            }
        };
        if (!option || !option.keyScore) {
            initOption();
        }
        var data = {
            focusRate: {average: 0, list: []},
            qualifiedRate: {average: 0, list: []},
            failureRate: {average: 0, list: []}
        };
        var tCount = 0, tKeyCount = 0, tQualified = 0, tFailure = 0;
        S.each(_sourceData.exams, function (exam) {
            var count = 0, keyCount = 0, qualified = 0, failure = 0;
            S.each(exam.scores, function (item) {
                var score = 0, scoreA = 0;
                if (subjectId) {
                    var scoreItem = item.subjects[subjectId];
                    score = calcTotal(scoreItem);
                    scoreA = scoreItem.scoreA;
                }
                else {
                    //所有科目A卷平均分
                    if (!item.scoreA) {
                        var total = 0, countA = 0;
                        S.each(item.subjects, function (subject) {
                            if (subject.scoreA < 0)
                                return;
                            total += subject.scoreA;
                            countA++;
                        });
                        item.scoreA = total / countA;
                    }
                    score = item.score;
                    scoreA = item.scoreA;
                }
                if (score < 0)
                    return;
                count++;
                if (score >= option.keyScore)
                    keyCount++;
                if (scoreA >= option.scoreA)
                    qualified++;
                if (scoreA < option.unScoreA)
                    failure++;
            });
            data.focusRate.list.push({
                name: exam.agencyName,
                value: round(keyCount * 100 / count)
            });
            data.qualifiedRate.list.push({
                name: exam.agencyName,
                value: round(qualified * 100 / count)
            });
            data.failureRate.list.push({
                name: exam.agencyName,
                value: round(failure * 100 / count)
            });
            tCount += count;
            tKeyCount += keyCount;
            tQualified += qualified;
            tFailure += failure;
        });
        data.focusRate.average = round(tKeyCount * 100 / tCount);
        data.qualifiedRate.average = round(tQualified * 100 / tCount);
        data.failureRate.average = round(tFailure * 100 / tCount);
        return data;
    };

    /**
     * 分数段分布
     * @param subjectId
     */
    chartApi.segmentDist = function (subjectId) {
        var data = {
            segments: [],
            agencyList: []
        };
        if (!subjectId || !_sourceData.subjects.hasOwnProperty(subjectId))
            return data;
        var subject = _sourceData.subjects[subjectId],
            score = subject.score,
            segmentScores = [];
        //分段
        if (score % 10 != 0) {
            data.segments.push(S.format('{0}-{1}', score, score - score % 10));
            segmentScores.push({
                max: score,
                min: score - score % 10
            });
            score = score - score % 10;
        }
        var i;
        for (i = score; i > (score * 0.6); i -= 10) {
            data.segments.push(S.format('{0}-{1}', i, i - 10));
            segmentScores.push({
                max: i,
                min: i - 10
            });
        }
        data.segments.push(i + '以下');
        segmentScores.push({
            max: i,
            min: 0
        });
        //填充学校数据
        S.each(_sourceData.exams, function (exam) {
            var agency = {
                name: exam.agencyName,
                counts: {}
            };
            S.each(data.segments, function (segment) {
                agency.counts[segment] = 0;
            });
            S.each(exam.scores, function (item) {
                if (!item.subjects.hasOwnProperty(subjectId))
                    return;
                var scoreItem = item.subjects[subjectId];
                S.each(data.segments, function (segment, i) {
                    var range = segmentScores[i],
                        score = calcTotal(scoreItem);
                    if (score <= range.max && score > range.min) {
                        agency.counts[segment]++;
                    }
                });
            });
            data.agencyList.push(agency);
        });
        return data;
    };

    /**
     * 导出报表
     */
    chartApi.download = function () {
        var $form = $('#exportForm');
        if (!$form || !$form.length) {
            var $body = $('body');
            $form = $('<form method="post" target="_blank" class="hide">');
            $body.append($form);
        } else {
            $form.empty();
        }
        //总排名
        $form.attr('action', '/ea/union-download');
        $form.append('<input name="unionBatch" value="' + _unionBatch + '" />');
        $form.submit();
    };

    /**
     * 单科排名
     * @param subjectId
     */
    chartApi.subjectRanks = function (subjectId) {
        var ranks = {
            isZhe: -1,
            isAb: true,
            students: []
        };
        if (!_sourceData.subjects.hasOwnProperty(subjectId))
            return ranks;
        var subject = _sourceData.subjects[subjectId];
        ranks.isAb = subject.isAb;
        S.each(students, function (student) {
            var item = {
                name: student.name,
                className: student.className,
                agency: student.agency
            };
            if (!student.subjects.hasOwnProperty(subjectId)) {
                item.score = -1;
                item.scoreA = -1;
                item.scoreB = -1;
                item.rank = -1;
            } else {
                var score = student.subjects[subjectId];
                item.score = score.score;
                item.scoreA = score.scoreA;
                item.scoreB = score.scoreB;
                item.rank = score.rank;
                if (ranks.isZhe == -1) {
                    ranks.isZhe = item.score < calcTotal(item);
                }
            }
            ranks.students.push(item);
        });
        ranks.students = ranks.students.sort(function (a, b) {
            return a.score > b.score ? -1 : 1;
        });
        return ranks;
    };

    S.mix(S, {
        chartApi: chartApi
    });
    $.fn.extend({
        /**
         * 表格滚动监听
         */
        tableScroll: function () {
            //内容滚动条计算
            var $t = $(this),
                $table = $t.find('.dy-table-body'),
                $header = $table.siblings('.dy-table-head'),
                widthHeader = $header.width(),
                widthTr = $table.find('tr:eq(1)').width() + 1,
                len = $table.find('tr').length;
            if (len > 8 && widthHeader >= widthTr) {
                $table.siblings('.dy-table-head').addClass('crt').css('width', widthTr);
            }
            if (widthTr > widthHeader) {
                //滚动事件
                $table.bind('scroll', function (e) {
                    e.preventDefault();
                    var $t = $(this),
                        left = $t.scrollLeft(),
                        $head = $t.siblings('.dy-table-head');
                    if ($head.length === 0) return false;
                    $head.find('table').css({'margin-left': -left});
                });
            }
        },
        /**
         * 弹框
         * @param  title 标题
         */
        popWindow: function (title) {
            var
                $box = $(this),
                $d = $(document),
                $w = $(window),
                $body = $('body'),
                browserWidth = $w.width(),
                browserHeight = $w.height(),
                browserScrollTop = $w.scrollTop(),
                browserScrollLeft = $w.scrollLeft(),
                popWindowWidth = $box.outerWidth(true),
                popWindowHeight = $box.outerHeight(true),
                positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft,
                positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop,
                oMask = '<div class="mask"></div>',
                maskWidth = $d.width(),
                maskHeight = $d.height();
            $box.show().animate({
                'left': positionLeft + 'px',
                'top': positionTop + 'px'
            }, 0);
            $body.append(oMask);
            $('.mask').width(maskWidth).height(maskHeight);
            // 标题内容
            $box.find('.h2-title strong').text(title);
            // 窗口改变
            $w.resize(function () {
                if ($box.is(':visible')) {
                    browserWidth = $(window).width();
                    browserHeight = $(window).height();
                    positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
                    positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;
                    $box.animate({
                        'left': positionLeft + 'px',
                        'top': positionTop + 'px'
                    }, 0);
                }
            });
            // 滚动条改变
            $w.scroll(function () {
                if ($box.is(':visible')) {
                    browserScrollTop = $(window).scrollTop();
                    browserScrollLeft = $(window).scrollLeft();
                    positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
                    positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;
                    $box.animate({
                        'left': positionLeft + 'px',
                        'top': positionTop + 'px'
                    }, 500).dequeue();
                }
            });
            // 关闭
            $body.on('click', '.mask,.close', function (event) {
                event.preventDefault();
                $box.hide();
                $('.mask').remove();
            });
            return false;
        }
    });
})(jQuery, SINGER);