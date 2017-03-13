/**
 * 学生 - 成绩报表
 * Created by shay on 2016/5/27.
 */
(function ($, S) {

    var shieldOff = false; // 屏蔽年级排名开关

    /**
     * 默认配置
     */
    var rankOption = {
            chart: {
                type: 'solidgauge',
                backgroundColor: '#fefcfc'
            },
            title: null,
            pane: {
                startAngle: 0,
                endAngle: 360,
                background: {
                    backgroundColor: '#ececec',
                    innerRadius: '85%',
                    outerRadius: '100%',
                    borderWidth: 0,
                    shape: 'arc'
                }
            },
            tooltip: {
                enabled: false
            },
            yAxis: {
                min: 0,
                max: 100,
                stops: [[1, '#4fc1e9']],
                lineWidth: 0,
                minorTickInterval: null,
                tickPixelInterval: 400,
                tickWidth: 0,
                title: {y: 110},
                labels: {y: 0, enabled: false}
            },
            plotOptions: {
                solidgauge: {
                    dataLabels: {
                        y: 5,
                        borderWidth: 0,
                        useHTML: true
                    }
                }
            }
        },
        showClassRank,
        showGradeRank,
        showSegment,
        showScoreRanks,
        showCompare,
        rankCompare,
        errorCompare,
        knowledgeCompare,
        myRank,
        logger = S.getLogger('student-reprot'),
        isComparing = false,
        batch,
        paperId,
        isPrint,
        $rank,
        $mainList = $('.g-main-list');
    batch = $mainList.data('batch');
    paperId = $mainList.data('paper');
    isPrint = $mainList.data('print');
    /**
     * 班级排名
     */
    showClassRank = function (classRank) {
        $rank = $('.d-class-rank');
        var labelFormat = '', labelY = -40, rankData = classRank.percent;
        if (classRank.rank < 0) {
            labelFormat = '<div class="d-rank-desc">暂无排名</div>';
            labelY = -18;
        } else {
            labelFormat = S.format('<div class="d-rank-desc">击败了<b>{0}%</b>同班同学</div>', rankData);
        }
        $rank.highcharts($.extend({
            credits: {
                enabled: false
            },
            series: [{
                name: '班级排名',
                data: [rankData],
                innerRadius: '85%',
                dataLabels: {
                    format: labelFormat,
                    y: labelY
                }
            }]
        }, rankOption));
        if (classRank.rank > 0) {
            if (classRank.change == null) {
                $rank.next().html(S.format('<b>{rank}</b>名', classRank));
            } else {
                if (classRank.change >= 0) {
                    classRank.cls = 'dy-icon-arrow-up';
                } else {
                    classRank.cls = 'dy-icon-arrow-down';
                    classRank.change = Math.abs(classRank.change);
                }
                $rank.next().html(S.format('<b>{rank}</b>名<i class="iconfont {cls}"></i>{change}', classRank));
            }
        } else {
            $rank.next().html('暂无名次');
        }
        $('.d-class-average').append(S.format('<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>',
                classRank.average < 0 ? '---' : classRank.average,
                classRank.averageA < 0 ? '---' : classRank.averageA,
                classRank.averageB < 0 ? '---' : classRank.averageB)
        );
    };
    /**
     * 年级排名
     */
    showGradeRank = function (gradeRank) {
        $rank = $('.d-grade-rank');
        var labelFormat = '', labelY = -40, rankData = gradeRank.percent;

        if (!shieldOff || gradeRank.rank < 0) {
            labelFormat = '<div class="d-rank-desc">暂无排名</div>';
            labelY = -18;
        } else {
            labelFormat = S.format('<div class="d-rank-desc">击败了<b>{0}%</b>同级同学</div>', rankData);
        }
        var gradeRankPercent;
        if (shieldOff) {
            gradeRankPercent = gradeRank.percent;
        } else {
            gradeRankPercent = 0;
        }
        $rank.highcharts(
            $.extend({
                credits: {
                    enabled: false
                },
                series: [{
                    name: '年级排名',
                    data: [gradeRankPercent],
                    innerRadius: '85%',
                    dataLabels: {
                        format: labelFormat,
                        y: labelY
                    }
                }]
            }, rankOption)
        );
        if (shieldOff && gradeRank.rank > 0) {
            if (gradeRank.change == null) {
                $rank.next().html(S.format('<b>{rank}</b>名', gradeRank));
            } else {
                if (gradeRank.change > 0) {
                    gradeRank.cls = 'dy-icon-arrow-up';
                } else {
                    gradeRank.cls = 'dy-icon-arrow-down';
                    gradeRank.change = Math.abs(gradeRank.change);
                }
                $rank.next().html(S.format('<b>{rank}</b>名<i class="iconfont {cls}"></i>{change}', gradeRank));
            }
        } else {
            $rank.next().html('暂无名次');
        }
        $('.d-grade-average').append(S.format('<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>',
                !shieldOff || gradeRank.average < 0 ? '---' : gradeRank.average,
                !shieldOff || gradeRank.averageA < 0 ? '---' : gradeRank.averageA,
                !shieldOff || gradeRank.averageB < 0 ? '---' : gradeRank.averageB)
        );
    };

    /**
     * 显示分数段统计
     * @param segments
     */
    showSegment = function (segments) {
        var $segments = $('.d-segments');
        if (!segments || !segments.length) {
            $segments
                .html('<div class="dy-nothing">没有分数段统计信息</div>')
                .css({height: 'auto'})
                .next().remove();
            return false;
        }
        var cate = [], counts = [];
        S.each(segments, function (item, index) {
            cate.push(item.segment);
            counts.push({
                y: item.count,
                color: item.containsMe ? '#ed5565' : (index % 2 == 1 ? '#caecf8' : '#a7e0f4')
            });
        });
        $segments.highcharts({
            chart: {
                type: 'bar',
                backgroundColor: '#fefcfc'
            },
            title: {
                text: '分数段',
                align: "left",
                style: {fontSize: "14px"}
            },
            legend: {enabled: false},
            xAxis: {categories: cate},
            yAxis: {
                allowDecimals: false,
                title: {text: '人数', align: 'high'}
            },
            series: [{
                name: '人数',
                data: counts,
                dataLabels: {
                    enabled: true,
                    color: '#666',
                    align: 'right',
                    x: 40,
                    y: 2,
                    style: {fontSize: '14px'},
                    formatter: function () {
                        return this.y + '人';
                    }
                }
            }]
        });
    };

    /**
     * 显示排名大比拼
     * @param total
     * @param ranks
     */
    showScoreRanks = function (total, ranks) {
        var $ranks = $('.d-ranks');
        if (!ranks || !ranks.length) {
            $ranks
                .html('<div class="dy-nothing">没有排名信息！</div>')
                .css({height: 'auto'})
                .next().remove();
            showCompare();
            return false;
        }
        var cate = [], ranksData = [], firstRank, showRanks = [];
        S.each(ranks, function (item, index) {
            cate.push(item.rank);
            var mine = item.isMine;
            if (mine) {
                myRank = {rank: item.rank, score: item.score};
            } else if (!firstRank) {
                firstRank = item;
                item.selected = isPrint;
            }
            ranksData.push({
                y: item.score,
                id: item.id,
                rank: item.rank,
                selected: item.selected || false,
                isMine: mine,
                color: mine ? '#ed5565' : '#caecf8',
                marker: {
                    states: {
                        hover: {
                            color: mine ? '#ed5565' : '#4fc1e9'
                        },
                        select: {
                            color: mine ? '#ed5565' : '#4fc1e9'
                        }
                    }
                }
            });
        });
        showCompare(firstRank);
        //logger.info(ranksData);
        $ranks.highcharts({
            chart: {
                type: 'column',
                backgroundColor: '#fefcfc'
            },
            title: {
                text: '分数',
                align: "left",
                style: {fontSize: "14px"}
            },
            legend: {enabled: false},
            tooltip: {
                headerFormat: '<span style="font-size: 12px">排名：{point.key}名</span><br/>',
                pointFormat: '分数：{point.y:.1f}分'
            },
            xAxis: {
                categories: cate,
                min: 0,
                title: {text: '排名', align: 'high'},
                labels: {
                    formatter: function () {
                        if (this.isFirst || this.isLast)
                            return this.value;
                        return null;
                    },
                    maxStaggerLines: 1,
                    distance: 10
                }
            },
            yAxis: {max: parseInt(total), min: 0, title: null},
            plotOptions: {
                column: {
                    cursor: 'pointer',
                    allowPointSelect: isPrint,
                    events: {
                        click: function (event) {
                            if (!isPrint)
                                return false;
                            if (event.point.isMine) {
                                event.point.selected = false;
                                S.msg('不能和自己对比！');
                                return false;
                            }
                            if (isComparing) {
                                S.msg('正在对比，请稍候...！');
                                return false;
                            }
                            if (!event.point.selected) {
                                showCompare({
                                    id: event.point.id,
                                    rank: event.point.rank,
                                    score: event.point.y
                                });
                            }
                        }
                    }
                }
            },
            series: [{
                type: 'column',
                name: '分数',
                data: ranksData
            }]
        });
    };

    /**
     * 排名对比
     * @param rank
     */
    rankCompare = function (rank) {
        var $rankCompare = $('.d-rank-compare tbody'),
            $compares = $rankCompare.find('.compare'),
            $tds = $rankCompare.find('td[rowspan] em');
        $compares.eq(0).html(myRank.rank);
        $compares.eq(1).html(myRank.score);
        if (rank) {
            $compares.eq(2).html(rank.rank);
            $compares.eq(3).html(rank.score);
            $tds.eq(0).html(Math.abs(rank.rank - myRank.rank));
            $tds.eq(1).html(Math.abs(rank.score - myRank.score));
        }
    };

    /**
     * 错题对比
     */
    errorCompare = function (errorData) {
        var $errorCompare = $('.d-error-compare'),
            tags = ['dy-icon-cha', 'dy-icon-bangou', 'dy-icon-gou'],
            $sort = $('<tr>'),
            $mine = $('<tr>'),
            $other = $('<tr>');
        $errorCompare.empty();
        if (errorData.length == 0) {
            return false;
        }
        for (var i = 0; i < errorData.length; i++) {
            var item = errorData[i];
            $sort.append(S.format('<td>{0}</td>', item.sort));
            $mine.append(S.format('<td class="d-error"><i class="iconfont {0}"></i></td>', tags[item.mine]));
            $other.append(S.format('<td class="d-error"><i class="iconfont {0}"></i></td>', tags[item.other]));
        }
        $errorCompare
            .append($sort)
            .append($mine)
            .append($other);
    };

    /**
     * 知识点得分率对比
     */
    knowledgeCompare = function (knowledgeData) {
        var $knowledgeCompare = $('.d-knowledge-compare'),
            $knowledge = $('<tr>'),
            states = ['', 'd-rate-warning', 'd-rate-danger'],
            getState,
            $mine = $('<tr>'),
            $other = $('<tr>');
        $knowledgeCompare.empty();
        if (knowledgeData.length == 0) {
            return false;
        }
        getState = function (rate) {
            var index = 0;
            if (rate < 30)
                index = 2;
            else if (rate < 50)
                index = 1;
            return S.format('<div class="d-rate {1}"><em style="height:{0}%"></em><span>{0}%</span></div>', rate, states[index]);
        };
        for (var i = 0; i < knowledgeData.length; i++) {
            var item = knowledgeData[i];
            $knowledge.append(S.format('<td><div class="d-knowledge-name" title="{0}">{0}</div></td>', item.knowledge));
            $mine.append(S.format('<td class="d-error">{0}</td>', getState(item.mine)));
            $other.append(S.format('<td class="d-error">{0}</td>', getState(item.other)));
        }
        $knowledgeCompare
            .append($knowledge)
            .append($mine)
            .append($other);
    };

    /**
     * 显示对比
     * @param rank
     */
    showCompare = function (rank) {
        isComparing = true;
        var $loading = $('.dy-loading'),
            $compareBox = $('.d-compare-box');
        if (!isPrint || !myRank || myRank.rank < 0 || !rank) {
            $loading.hide();
            $compareBox.addClass('hide');
            isComparing = false;
            return false;
        }
        rankCompare(rank);
        var compareData = $compareBox.data('compare_' + rank.id);
        if (compareData) {
            errorCompare(compareData.errors);
            knowledgeCompare(compareData.knowledges);
            isComparing = false;
            return false;
        }
        $loading.show();
        $compareBox.addClass('hide');
        $.post('/work/student-compare', {
            batch: batch,
            paperId: paperId,
            compareId: rank.id
        }, function (json) {
            if (json.status && json.data) {
                compareData = json.data;
                errorCompare(compareData.errors);
                knowledgeCompare(compareData.knowledges);
                $compareBox.data('compare_' + rank.id, compareData);
            }
            isComparing = false;
            $loading.hide();
            $compareBox.removeClass('hide');
        });
    };

    $('.dy-table-body').bind('scroll', function () {
        var $t = $(this),
            $left = $t.find('.dy-table-left');
        $left.css({
            left: $t.scrollLeft()
        });
    });

    $.get('/work/student-reports', {
        batch: batch
    }, function (json) {
        if (!json.status) {
            return false;
        }
        var reportData = json.data;
        showClassRank(reportData.classRank);
        showGradeRank(reportData.gradeRank);
        showSegment(reportData.segments);
        showScoreRanks(reportData.totalScore, reportData.ranks);
    });
})(jQuery, SINGER);