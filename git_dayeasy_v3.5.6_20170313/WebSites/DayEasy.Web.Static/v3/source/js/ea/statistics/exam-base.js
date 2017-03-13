/**
 * 教务管理 - 数据源管理
 * Created by shay on 2016/7/18.
 */
(function ($, S) {
    var urls = {
            subjects: '/ea/subjects/{0}',
            ranks: '/ea/ranks/{0}',
            classKeys: '/ea/class-analysis-keys',
            classLayers: '/ea/class-analysis-layers',
            subjectKeyAndLayer: '/ea/subject-analysis',
            subjectScoreRate: '/ea/subject-score-rates'
        },
        examId,
        cacheData = {
            subjects: null,
            ranks: null,
            score: 0,
            scoreRates: {}
        },
        /**
         * 数据接口
         */
        examApi = {
            /**
             * 大型考试ID
             * @param id
             */
            setExamId: function (id) {
                examId = id;
            },
            /**
             * 获取大型考试科目
             * @param callback
             */
            getSubjects: function (callback) {
                //if (cacheData.subjects) {
                //    callback && callback.call(this, cacheData.subjects);
                //    return false;
                //}
                    $.get(S.format(urls.subjects, examId), {}, function (json) {
                    cacheData.subjects = {};
                    for (var i = 0; i < json.length; i++) {
                        var item = json[i];
                        cacheData.score += item.score;
                        cacheData.subjects[item.id] = {
                            id:item.subjectId,
                            name: item.subject,
                            paperType: item.paperType,
                            score: item.score,
                            scoreA: item.scoreA,
                            scoreB: item.scoreB,
                            average: item.averageScore,
                            averageA: item.averageScoreA,
                            averageB: item.averageScoreB
                        };
                    }
                    callback && callback.call(this, json);
                });
            },
            subject: function (id) {
                return cacheData.subjects ? cacheData.subjects[id] : null;
            },
            /**
             * 获取大型考试排名信息
             * @param callback
             */
            getRanks: function (callback) {
                if (cacheData.ranks) {
                    callback && callback.call(this, cacheData.ranks);
                    return false;
                }
                var url = S.format(urls.ranks, examId);
                $.get(url, {}, function (json) {
                    if (!json.status) {
                        S.msg(json.message);
                        return false;
                    }
                    cacheData.ranks = json.data;
                    callback && callback.call(this, json.data);
                });
            },
            /**
             * 班级重点率
             * @param params
             * @param params.keyType    重点率类型
             * @param params.keyScore   重点率的阀值
             * @param params.scoreA     A卷合格率
             * @param params.unScoreA   A卷不合格率
             * @param callback          回调
             */
            getClassKeys: function (params, callback) {
                params["examId"] = examId;
                $.get(urls.classKeys, params, function (json) {
                    callback && callback.call(this, json);
                })
            },
            /**
             * 班级分层
             * @param params
             * @param params.layerA    A层比例
             * @param params.layerB    B层比例
             * @param params.layerC    C层比例
             * @param params.layerD    D层比例
             * @param params.layerE    E层比例
             * @param callback  回调
             */
            getClassLayers: function (params, callback) {
                if (params.layerA + params.layerB + params.layerC + params.layerD + params.layerE != 100) {
                    S.msg('各层分配比例之和需为100%');
                    return false;
                }
                params["examId"] = examId;
                $.get(urls.classLayers, params, function (json) {
                    callback && callback.call(this, json);
                });
            },
            /**
             * 获取科目统计信息
             * @param params
             * @param params.subjectId  考试科目Id
             * @param params.keyType    重点率类型
             * @param params.keyScore   重点率阀值
             * @param params.scoreA     A卷合格分数
             * @param params.unScoreA   A卷不合格分数
             * @param callback  回调
             */
            getSubjectData: function (params, callback) {
                $.get(urls.subjectKeyAndLayer, {
                    examId: examId,
                    examSubjectId: params.subjectId,
                    keyType: params.keyType,
                    keyScore: params.keyScore,
                    scoreA: params.scoreA,
                    unScoreA: params.unScoreA
                }, function (json) {
                    callback && callback.call(this, json);
                });
            },
            /**
             * 获取科目各题得分率
             * @param id        考试科目ID
             * @param callback
             */
            getScoreRates: function (id, callback) {
                if (cacheData && cacheData.scoreRates.hasOwnProperty(id)) {
                    callback && callback.call(this, cacheData.scoreRates[id]);
                    return false;
                }
                $.get(urls.subjectScoreRate, {
                    id: id
                }, function (json) {
                    if (!json.status) {
                        S.msg(json.message);
                        return false;
                    }
                    cacheData.scoreRates[id] = json.data;
                    callback && callback.call(this, json.data);
                });
            },
            /**
             * 报表下载
             * @param options.type              类型：0,总排名;1,综合概况;2,单科统计;
             * @param options.classKeysParams   重点率参数(仅综合概况需要)
             * @param options.classLayersParams 学生分层参数(仅综合概况需要)
             * @param options.subjectParams     学科统计参数(仅单科统计需要)
             * @param options.subjectName       科目(仅单科统计需要)
             */
            download: function (options) {
                var $form = $('#exportForm');
                if (!$form || !$form.length) {
                    var $body = $('body');
                    $form = $('<form method="post" target="_blank" class="hide">');
                    $body.append($form);
                } else {
                    $form.empty();
                }
                var name = cacheData.ranks.name;
                switch (options.type) {
                    case 0:
                        //总排名
                        $form.attr('action', '/ea/ranking-download');
                        $form.append('<input name="examId" value="' + examId + '" />');
                        break;
                    case 1:
                        options.classKeysParams.examId = examId;
                        options.classLayersParams.examId = examId;
                        //班级统计下载
                        $form.attr('action', '/ea/class-analysis-download');
                        $form.append('<input name="keys" value="' + S.json(options.classKeysParams) + '" />');
                        $form.append('<input name="layer" value="' + S.json(options.classLayersParams) + '" />');
                        $form.append('<input name="name" value="' + name + '" />');
                        break;
                    case 2:
                        //单科统计下载
                        options.subjectParams.examId = examId;
                        options.subjectParams.examSubjectId = options.subjectParams.subjectId;
                        delete options.subjectParams.subjectId;
                        $form.attr('action', '/ea/subject-analysis-download');
                        $form.append('<input name="keys" value="' + S.json(options.subjectParams) + '" />');
                        $form.append('<input name="name" value="' + name + '-' + options.subjectName + '" />');
                        break;
                }
                $form.submit();
            },
            /**
             * 班级单科分化度
             * @param id        考试科目ID
             * @param averages  各班平均分
             * @param count     个数
             */
            classDiffDegree: function (id, averages, count) {
                var degrees = [];
                if (!cacheData.ranks)
                    return degrees;
                var students = cacheData.ranks.students,
                    average, //年级平均
                    classDegrees = {};
                S.each(students, function (student) {
                    var detail = student.scoreDetails[id];
                    if (detail.score < 0) return;
                    if (!averages.hasOwnProperty(student.classId))
                        return;
                    average = averages[student.classId];
                    if (!classDegrees.hasOwnProperty(student.classId)) {
                        classDegrees[student.classId] = {
                            name: student.className,
                            count: 0,
                            degree: 0
                        };
                    }
                    classDegrees[student.classId].degree += Math.abs(detail.scoreA + detail.scoreB - average);
                    classDegrees[student.classId].count++;
                });
                S.each(classDegrees, function (item) {
                    if (item.degree <= 0)
                        return;
                    degrees.push({
                        name: item.name,
                        degree: (item.degree / item.count).toFixed(2) * 1
                    });
                });
                degrees = degrees.sort(function (a, b) {
                    return a.degree < b.degree ? 1 : -1;
                });
                if (count)
                    degrees.splice(count);
                return degrees;
            }
        };
    S.mix(S, {
        examApi: examApi,
        /**
         * 格式化班级名称
         * @param name
         */
        formatClassName: function (name) {
            var m = (/([0-9]+班)/gi.exec(name));
            return m ? m[1] : name;
        },
        /**
         * 得分率
         * @param id  科目ID
         */
        scoreRate: function (id) {
            if (!id || !S.isString(id) || id.length != 32)
                return false;
            var scoreRateBind, rateData, showChart, diffDegree;
            /**
             * 图表展示
             * @param $item
             */
            showChart = function ($item) {
                var chartData = [],
                    id = $item.data('qid'),
                    rate = $item.data('rate'),
                    rates;
                $item.addClass('on').siblings().removeClass('on');
                if (rateData.classScoreRates.hasOwnProperty(id))
                    rates = rateData.classScoreRates[id];
                S.each(rateData.classList, function (name, i) {
                    chartData.push({
                        name: name,
                        value: rates ? (rates[i] || 0) : 0
                    });
                });
                $('.score-rate').empty().chart({
                    type: 'percent',
                    data: chartData,
                    average: rate,
                    max: 100,
                    showAverage: true,
                    width: 956,
                    height: 300,
                    yTitle: '得分率',
                    xTitle: '班级',
                    xFormat: function (item) {
                        return S.formatClassName(item.name);
                    }
                });
            };
            /**
             * 数据绑定
             */
            scoreRateBind = function () {
                var $main = $('.html-main'),
                    data = {sorts: []};
                S.each(rateData.questionSorts, function (sort, id) {
                    var rate = rateData.scoreRates[id];
                    data.sorts.push({
                        id: id,
                        sort: sort,
                        rate: rate
                    });
                });
                var html = template('scoreRateTemp', data);
                $main.html(html);
                var degree = diffDegree(5);
                var htmlside = template('side-6', degree);
                $('.html-side').html(htmlside);

                var $list = $main.find('li');
                showChart($list.eq(0));
                $main.find('li').bind('click', function () {
                    var $t = $(this);
                    if ($t.hasClass('on'))
                        return false;
                    showChart($t);
                });
            };
            /**
             * 题目分化度
             */
            diffDegree = function (count) {
                var degrees = [];
                S.each(rateData.questionSorts, function (sort, id) {
                    var degree = 0,
                        rate = rateData.scoreRates[id],//年级平均得分率
                        classRates = rateData.classScoreRates[id];
                    S.each(classRates, function (item) {
                        //与平均值之差的累加
                        degree += Math.abs(item - rate);
                    });
                    if (degree > 0) {
                        degrees.push({
                            sort: sort,
                            degree: degree.toFixed(2) * 1
                        });
                    }
                });
                if (degrees.length == 0) return degrees;
                degrees = degrees.sort(function (a, b) {
                    return a.degree < b.degree ? 1 : -1;
                });
                degrees.splice(count);
                return degrees;
            };
            examApi.getScoreRates(id, function (data) {
                rateData = data;
                scoreRateBind();
            });
        }
    });
})(jQuery, SINGER);