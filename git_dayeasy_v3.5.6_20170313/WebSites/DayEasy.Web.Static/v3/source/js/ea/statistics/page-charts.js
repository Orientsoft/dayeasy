/**
 * 教务管理 - 图表版
 * Created by shay on 2016/7/18.
 */
(function ($, S){
    var exam = {
            /**
             * 配置数据
             */
            config: {
                comprtype: true,
                icons: [
                    "dy-icon-chinese",      //语文
                    "dy-icon-math",         //数学
                    "dy-icon-english",      //英语
                    "dy-icon-physics",      //物理
                    "dy-icon-chemistry",    //化学
                    "dy-icon-politics",     //政治
                    "dy-icon-history",      //历史
                    "dy-icon-geography",    //地理
                    "dy-icon-biology",      //生物
                    "dy-icon-computer",     //计算机
                    "dy-icon-sports"        //体育
                ],
                keyParams: {
                    keyType: 1,
                    keyScore: 75,
                    scoreA: 60,
                    unScoreA: 60
                },
                keyParamsOperation: null,
                layerParams: {
                    layerA: 10,
                    layerB: 30,
                    layerC: 30,
                    layerD: 20,
                    layerE: 10
                },
                subjectParams: {
                    subjectId: '',
                    keyType: 1,
                    keyScore: 75,
                    scoreA: 60,
                    unScoreA: 60
                }
            },
            /**
             * 页面缓存
             */
            data: {
                subjects: [],
                ranks: {},
                main2averageScore: [],
                classKeys: {},
                ClassLayers: {}
            },
            ui: {
                subjects: null
            },
            event: {
                init: null,
                pageLoad: null,
                resultRanking: null,       //成绩排名
                classKeys: null,        //班级重点率
                classLayers: null,      //班级分层
                subjectData: null,       //科目重点率/平均分
                scoreRate: null       //科目重点率/平均分
            }
        },
        api = S.examApi;
    var HtmlClass = {
        body: $('body'),                    // 左侧导航class
        $main: $('.html-main'),
        $side: $('.html-side')
    };
    S._mix(S, {
        /**
         * 弹框
         * @param popWindow 显示内容ID
         * @param  title 标题
         */
        NoName: function (popWindow, title){
            var
                browserWidth = $(window).width(),
                browserHeight = $(window).height(),
                browserScrollTop = $(window).scrollTop(),
                browserScrollLeft = $(window).scrollLeft(),
                popWindowWidth = popWindow.outerWidth(true),
                popWindowHeight = popWindow.outerHeight(true),
                positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft,
                positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop,
                oMask = '<div class="mask"></div>',
                maskWidth = $(document).width(),
                maskHeight = $(document).height();
            popWindow.show().animate({
                'left': positionLeft + 'px',
                'top': positionTop + 'px'
            }, 0);
            $('body').append(oMask);
            $('.mask').width(maskWidth).height(maskHeight);
            // 标题内容
            popWindow.find('.h2-title strong').text(title);
            // 窗口改变
            $(window).resize(function (){
                if(popWindow.is(':visible')){
                    browserWidth = $(window).width();
                    browserHeight = $(window).height();
                    positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
                    positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;
                    popWindow.animate({
                        'left': positionLeft + 'px',
                        'top': positionTop + 'px'
                    }, 0);
                }
            });
            // 滚动条改变
            $(window).scroll(function (){
                if(popWindow.is(':visible')){
                    browserScrollTop = $(window).scrollTop();
                    browserScrollLeft = $(window).scrollLeft();
                    positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
                    positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;
                    popWindow.animate({
                        'left': positionLeft + 'px',
                        'top': positionTop + 'px'
                    }, 500).dequeue();
                }
            });
            // 关闭
            $('body').on('click', '.mask,.close', function (event){
                event.preventDefault();
                popWindow.hide();
                $('.mask').remove();
            });
            return false;
        },
        /**
         * 输入中文限制
         * @param obj 事件对象
         * @param num 随机数3位
         */
        keynumber: function ($obj, $operation){
            HtmlClass.body.on('keyup', $obj, function (event){
                event.preventDefault();
                $operation.attr("disabled", false);
                var tmptxt = $(this).val();
                $(this).val(tmptxt.replace(/\D|^0/g, ''));
            });
        },
        /*名次转换*/
        layerRanks: function (){
            var ranks = [], prefix = 1;
            for (var i = 0; i < arguments.length; i++) {
                var rank = arguments[i];
                if(rank > 0){
                    ranks.push(S.format('{0}-{1}名', prefix, prefix + rank - 1));
                    prefix += rank;
                } else {
                    ranks.push('-')
                }
            }
            return ranks;
        },
        /**
         * 需要关注的班级
         * @param initdata [] 班级分数,班级名称
         * @param average 平均值
         * @param number 需要关注班级最大个数
         */
        careClass: function (){
            if(arguments.length == ''){
                return false;
            }
            var initdata = arguments[0],
                average = arguments[1],
                number = arguments[2],
                classList = [];
            //低于平均分按照大小排列 倒数3名班级
            initdata.sort(function (a, b){
                return a.value - b.value
            });
            for (var i = 0; i < initdata.length; i++) {
                if(initdata[i].value < average){
                    classList.push(initdata[i].name)
                }
            }
            classList.splice(number);
            return classList;
        },
        /**
         * 学生所有考试成绩-弹框数据封装
         * @param rankData
         * @param index 学生在表格中索引
         */
        resultRankingUi: function (rankData, index){
            var subjectsAll = new Array();
            for (var j = 0; j < rankData.ranks.students.length; j++) {
                var arrList = rankData.ranks.students[j],
                    nameStudent = arrList.name,
                    details = arrList.scoreDetails;
                for (var i = 0; i < rankData.subjects.length; i++) {
                    var id = rankData.subjects[i].id;
                    var subjectsName = rankData.subjects[i].subject;
                    subjectsAll.push({
                        name: nameStudent,
                        subject: subjectsName,
                        volumeA: details[id].scoreA,
                        volumeB: details[id].scoreB,
                        total: details[id].score,
                        ranking: details[id].rank
                    })
                }
            }
            /*
             chunk([1,2,3], 2)
             [ [1,2], [3] ]
             */
            // 学生参考所有科目层级
            var chunk = function (array, size){
                var result = [];
                for (var x = 0; x < Math.ceil(array.length / size); x++) {
                    var start = x * size;
                    var end = start + size;
                    result.push(array.slice(start, end));
                }
                return result;
            }
            var subjectsCombination = chunk(subjectsAll, rankData.subjects.length);
            var data = subjectsCombination[index]
            return data;

        },
        /**
         * 成绩排名扇形图
         * @param  a 缺考人数
         * @param  b 参考人数
         */
        highcharts: function (a, b){
            var c = b - a; //缺考人数
            $('#container').highcharts({
                chart: {
                    plotBackgroundColor: null,
                    plotBorderWidth: null,
                    backgroundColor: 'transparent',
                    plotShadow: false

                },
                title: null,
                tooltip: {
                    pointFormat: '<b>{point.y}</b>'
                },
                plotOptions: {
                    pie: {
                        allowPointSelect: true,
                        cursor: 'pointer',
                        borderColor: 'transparent',
                        colors: ['#3bafda', '#72cff2'],
                        dataLabels: {
                            enabled: false
                        }
                    }
                },
                series: [{
                    type: 'pie',
                    name: '',
                    data: [
                        ['参考人数', a],
                        ['缺考人数', c]
                    ]
                }]
            });
        }
    })
    /**
     * 成绩排名
     */
    exam.event.resultRanking = function (){
        var
        /*单科成绩*/
            rankData = exam.data,
            studentsDtat= rankData.ranks.students,
            subjectId = exam.data.subjectId,
            classList = [], scoreCount = 0, html;
        /*界面标题名称*/
        $('.html-title-name').html(S.format('<span title="{0}">{0}</span>', rankData.ranks.name));
        if(subjectId){
            var students = studentsDtat.concat().sort(function (a, b){
                var rankA = a.scoreDetails[subjectId].rank,
                    rankB = b.scoreDetails[subjectId].rank;
                rankA = rankA < 0 ? 999 : rankA;
                rankB = rankB < 0 ? 999 : rankB;
                return rankA > rankB ? 1 : -1;
            });
            var subject = api.subject(subjectId);
            rankData = {
                students: students,
                subjectId: subjectId,
                isAb: exam.data.paperType == 2,
                isZhe: subject.id == 4 || subject.id == 5
            };
        }
        html = template('menu-main-1', rankData);
        HtmlClass.$main.html(html);
        //内容滚动条计算
        var $table = $('.dy-table-body'),
            whiteTr = $table.find('tr:eq(1)').width() + 1,
            len = $table.find('tr').length;
        if(len > 8){
            $table.siblings('.dy-table-head').addClass('crt').css('width', whiteTr);
        }
        $table.on('scroll', function (event){
            event.preventDefault();
            var $t = $(this),
                left = $t.scrollLeft(),
                $head = $t.siblings('.dy-table-head').find('table');
            $head.css({'margin-left': -left});

        });
        // 学生各科成绩调用
        $(".js-students-pop").each(function (index){
            var $t = $(this),
                _text = $.trim($t.text());
            $t.click(function (){
                /*弹框数据*/
                var data = S.resultRankingUi(rankData, index);
                var htmlNames = template('SubjectsAllTemp', data);
                $('#SubjectsAll').html(htmlNames);
                /*调用弹框*/
                S.NoName($('.pop-result-ranking'), _text);

            });
        })
        for (var i = 0; i < studentsDtat.length; i++) {
            var student = studentsDtat[i],
                className = student.className;
            if(exam.data.subjectId){
                var detail = student.scoreDetails[exam.data.subjectId];
                if(detail.score >= 0) scoreCount++;
            } else {
                if(student.score >= 0) scoreCount++;
            }
            if(classList.indexOf(className) < 0){
                classList.push(className);
            }
        }
        var classIndex = function (name){
            var m = (/([0-9]+)班/gi.exec(name));
            return m ? ~~m[1] : 99;
        };
        classList.sort(function (a, b){
            return classIndex(a) > classIndex(b) ? 1 : -1;
        });
        html = template('side-1', {
            classList: classList,
            scoreCount: scoreCount,
            count: studentsDtat.length
        });
        HtmlClass.$side.html(html);
        // 扇行图 未提交名单
        S.highcharts(scoreCount, studentsDtat.length);
    };
    /**
     * 平均分对比
     */
    exam.event.subjectData = function (){
        var html = template('menu-main-2', {
            subjectId: exam.data.subjectId,
            isAb: exam.data.paperType == 2
        });
        HtmlClass.$main.html(html);
        // 主体部分
//            var objkeys = [];
//            for (objkeys[objkeys.length] in parameter);
        /**
         * 左侧数据展示
         * @param classKeys 综合概况平均分对比数据
         */
        var side = function (classKeys, Layer, initdata, average){
            // 左侧展示
            var MaxFen = [],
                arrAverageScore = [];
            var classList = S.careClass(initdata, average, 3);
            for (var i = 0; i < classKeys.length - 1; i++) {
                MaxFen.push(classKeys[i][Layer]);
                if(classKeys[i].keyScaleDiff < 0){
                    arrAverageScore.push(classKeys[i][Layer]);
                }
            }
            exam.data.main2averageScore = MaxFen;
            var Data_MaxFen = Math.max.apply(null, MaxFen), //班级平均分最高
                Data_MinFen = Math.min.apply(null, MaxFen), //班级平均分最低
                listclassdata = {
                    title0: '班平最高最低相差',
                    Differ: (Data_MaxFen - Data_MinFen).toFixed(2),
                    title1: '需要关注的班级：',
                    list: classList
                },
                html = template('side-2', listclassdata);
            HtmlClass.$side.html(html);
        };
        /**
         * 树状图数据绑定
         * @param data
         * @param index [] 属性
         */
        var bind = function (data, index){
            var arrs = ['averageScore', 'averageScoreA', 'averageScoreB'],
                key = arrs[index];
            var initdata = [];
            for (var i = 0; i < data.length - 1; i++) {
                initdata.push({
                    name: data[i].className,
                    value: data[i][key]
                })
            }
            var average = data[data.length - 1][key], max = 0;  // 综合平均分
            if(exam.data.subjectId){
                var subject = api.subject(exam.data.subjectId);
                switch (index) {
                    case 0:
                        max = subject.score;
                        break;
                    case 1:
                        max = subject.scoreA;
                        break;
                    case 2:
                        max = subject.scoreB;
                        break;
                }
            } else {
                S.each(exam.data.subjects, function (item){
                    max += item.score;
                });
            }
            $('.container02').empty().chart({
                type: 'score',
                data: initdata,
                showAverage: true,
                max: max,
                direct: 'v',
                average: average,
                width: 956,
                height: 300,
                xTitle: '班级',
                yTitle: '分数',
                xFormat: function (data){
                    return S.formatClassName(data.name);
                }
            });
            side(data, key, initdata, average);
        };
        if(exam.data.subjectId){
            var params = exam.config.subjectParams;
            params.subjectId = exam.data.subjectId;
            api.getSubjectData(params, function (data){
                var classKeys = exam.data.classKeys = data;
                bind(classKeys, 0);
                HtmlClass.body
                    .off('click', '.tab-average-menu2 li')
                    .on('click', '.tab-average-menu2 li', function (event){
                        event.preventDefault();
                        var $this = $(this);
                        if($this.hasClass('on')){
                            return false;
                        }
                        $this.addClass('on').siblings().removeClass('on');
                        bind(classKeys, $(this).index());
                    });
            });
        } else {
            var params = exam.config.keyParams;
            api.getClassKeys(params, function (data){
                var classKeys = exam.data.classKeys = data;
                bind(classKeys, 0);
            });
        }
    };
    /**
     * 重点率分析
     */
    exam.event.classKeys = function (){

        // 主体部分
//        var params = exam.config.keyParams;
//        console.log(params);
//        params.keyType  //1
//        params.keyScore  //75
//        params.scoreA   //60
//        params.unScoreA  //60
        var params = exam.config.keyParams;
        console.log(params);
        var html = template('menu-main-3', params);
        HtmlClass.$main.html(html);

        var bindLayers = function (parameter){
            var objkeys = [];
            for (objkeys[objkeys.length] in parameter);
            /**
             * 数据绑定
             */
            var bindData = function (data, Layer){
                var initdata = [];
                for (var i = 0; i < data.length - 1; i++) {
                    initdata.push({
                        name: data[i].className,
                        value: (data[i][Layer] * 100).toFixed(2)
                    })
                }
                var average = (data[data.length - 1][Layer] * 100).toFixed(2);  // 综合平均分
                $('.container03').empty().chart({
                    type: 'percent',
                    data: initdata,
                    showAverage: true,
                    max: 100,
                    direct: 'v',
                    average: average,
                    width: 956,
                    height: 300,
                    xTitle: '班级',
                    yTitle: '百分比',
                    xFormat: function (data){
                        //var m = /([0-9]+班)/gi.exec(data.name);
                        return S.formatClassName(data.name);
                    }
                });
                // 侧边栏 bindData
                side(data, initdata, average);
            };
            /**
             * 侧边栏-数据绑定
             */
            var side = function (classKeys, initdata, average){
                // 左侧展示
                var classList = S.careClass(initdata, average, 3);
                var listclassdata = {
                    title: '需要关注的班级：',
                    list: classList
                };
                var html = template('side-3', listclassdata);
                HtmlClass.$side.html(html);
            };
            if(exam.data.subjectId){
                var params = exam.config.subjectParams;
                params.subjectId = exam.data.subjectId;
                opts = $.extend(params, parameter || {});
                HtmlClass.body.off('click', '.tab-average-menu3 li');
                api.getSubjectData(opts, function (data){
                    var classKeys = exam.data.classKeys = data;
                    // keyScale  重点率上线比例   averageRatio A卷合格人数比例   averageRatioDiff A卷不合格人数比例
                    var arrs = ['keyScale', 'aScale', 'unaScale'];
                    bindData(classKeys, arrs[0]);
                    HtmlClass.body
                        .on('click', '.tab-average-menu3 li', function (event){
                            event.preventDefault();
                            var $this = $(this);
                            if($this.hasClass('on')){
                                return false;
                            }
                            $this.addClass('on').siblings().removeClass('on');
                            bindData(classKeys, arrs[$(this).index()]);
                        });
                });
            } else {
                api.getClassKeys(parameter, function (data){
                    var classKeys = exam.data.classKeys = data;
                    var arrs = ['keyScale', 'aScale', 'unaScale'];
                    HtmlClass.body.off('click', '.tab-average-menu3 li');
                    bindData(classKeys, arrs[0]);
                    HtmlClass.body
                        .on('click', '.tab-average-menu3 li', function (event){
                            event.preventDefault();
                            var $this = $(this);
                            if($this.hasClass('on')){
                                return false;
                            }
                            $this.addClass('on').siblings().removeClass('on');
                            bindData(classKeys, arrs[$this.index()])
                        });
                });
            }
        };
        bindLayers(params);
        /**
         * 分数比例 “%” 切换效果
         */
        $('#classkey-select').change(function (){
            var $this = $(this);
            $('#KeyCalculation').attr("disabled", false);
            $this.val() == 1 && $this.parents('dd').addClass('on');
            $this.val() == 0 && $this.parents('dd').removeClass('on');
        });
        /**
         *  计算数据切换
         */
        $('#KeyCalculation').click(function (){
            $(this).attr("disabled", true);
            var
                paramss = exam.config.keyParamsOperation = {
                    keyType: parseInt($('#classkey-select').val()), //类型
                    keyScore: parseInt($('#FractionAll').val()),
                    scoreA: parseInt($('#FractionA').val()),
                    unScoreA: parseInt($('#FractionAb').val())
                };
            var params = exam.config.keyParams = $.extend({}, exam.config.keyParams, paramss);

            if(params.keyType == 1 && params.keyScore > 100){
                S.alert('重点比例不能超过100%！');
                return false;
            }
            bindLayers(params);
            $('.tab-average-menu3 li:first').addClass('on').siblings().removeClass('on');
//          var liList=  $('.tab-average-menu3').find('li');
//            for (var i = 0; i < liList.length; i++) {
//                if($(liList[i]).hasClass('on')){
//                    $('.tab-average-menu3').find('li').eq(i).trigger('click');
//                }
//            }


        });
        // 计算效果
        S.keynumber("input[name='keynumber']", $('#KeyCalculation'));
    };
    /**
     * 班级分层
     */
    exam.event.classLayers = function (){
        // 主体部分
        var params = exam.config.layerParams;
        var html = template('menu-main-4', params);
        HtmlClass.$main.html(html);
        // 重置学生层下载参数
        var bindLayers = function (parameter){
            var objkeys = S.keys(parameter);
            api.getClassLayers(parameter, function (data){
                exam.data.classLayers = data;
                var max = 0, getData;
                S.each(data, function (item){
                    if(!item.classId) return;
                    S.each(objkeys, function (key){
                        if(item[key] > max) max = item[key];
                    });
                });
                max = $.nearMax(max);
                getData = function (key){
                    var layers = [];
                    S.each(data, function (item){
                        if(!item.classId) return;
                        layers.push({
                            name: item.className,
                            value: item[key]
                        });
                    });
                    return layers;
                };
                var bindData = function (key){
                    var layers = getData(key);
                    $('.container04').empty().chart({
                        type: 'score',
                        data: layers,
                        showAverage: false,
                        max: max,
                        direct: 'v',
                        width: 956,
                        height: 300,
                        xTitle: '班级',
                        yTitle: '人数',
                        xFormat: function (data){
                            return S.formatClassName(data.name);
                        }
                    });
                };
                bindData(objkeys[0]);
                HtmlClass.body
                    .off('click', '.tab-average-menu4 li')
                    .on('click', '.tab-average-menu4 li', function (event){
                        event.preventDefault();
                        var $this = $(this);
                        if($this.hasClass('on')){
                            return false;
                        }
                        $this.addClass('on').siblings().removeClass('on');
                        bindData(objkeys[$this.index()]);
                    });


                // 左侧展示
                var dataSide = data[data.length - 1];
                var html = template('side-4', dataSide);
                HtmlClass.$side.html(html);
                var ranks = S.layerRanks(dataSide.layerA, dataSide.layerB, dataSide.layerC, dataSide.layerD, dataSide.layerE);
                var $heads = $('.arrangement-menu .dd-people');
                for (var i = 0; i < $heads.length; i++) {
                    $heads.eq(i).find('em').html(ranks[i]);
                }

            });
        };
        bindLayers(params);
        HtmlClass.body.on('click', '#LayersCalculation', function (event){
            event.preventDefault();
            var ParamsExtend = exam.config.layerParams = $.extend({}, params, {
                layerA: parseInt($('#LayersA').val()),
                layerB: parseInt($('#LayersB').val()),
                layerC: parseInt($('#LayersC').val()),
                layerD: parseInt($('#LayersD').val()),
                layerE: parseInt($('#LayersE').val())
            });
            bindLayers(ParamsExtend);
            $('.tab-average-menu4 li:first').addClass('on').siblings().removeClass('on');

//            var liList=  $('.tab-average-menu4').find('li');
//            for (var i = 0; i < liList.length; i++) {
//                if($(liList[i]).hasClass('on')){
//                    $(liList[i]).trigger('click');
//                }
//            }

        });
        // 计算效果
        S.keynumber("input[name='keyNumberLayers']", $('#LayersCalculation'));
    };
    /**
     *分数段分布
     */
    exam.event.segment = function (){
        exam.config.subjectParams.subjectId = exam.data.subjectId;
        api.getSubjectData(exam.config.subjectParams, function (data){
            var html, classSegments = {
                segments: [],
                classList: []
            }, averages = {};
            S.each(data, function (item){
                var segment = {
                        name: item.className,
                        counts: {}
                    },
                    i = 0,
                    lastScore,
                    keysCount;
                if(item.classId){
                    averages[item.classId] = item.averageScore;
                }
                if(!item.segment){
                    classSegments.classList.push(segment);
                    return;
                }
                keysCount = S.keys(item.segment).length;
                S.each(item.segment, function (count, name){
                    if(keysCount > 6 && i >= keysCount * 0.4){
                        if(!lastScore){
                            lastScore = ~~(/-([0-9]+)$/gi.exec(name)[1]) + 1;
                        }
                        name = lastScore + '以下';
                    }
                    if(!item.classId && !S.inArray(name, classSegments.segments)){
                        classSegments.segments.push(name);
                    }
                    if(!segment.counts.hasOwnProperty(name))
                        segment.counts[name] = 0;
                    segment.counts[name] += count;
                    i++;
                });
                classSegments.classList.push(segment);
            });
            html = template('menu-main-5', classSegments);
            HtmlClass.$main.html(html);
            var degree = api.classDiffDegree(exam.data.subjectId, averages, 5);
            html = template('side-5', degree);
            HtmlClass.$side.html(html);
        });
    };
    /**
     * 得分率
     * @param id  科目ID
     */
    exam.event.scoreRate = function (id){
        if(!id || !S.isString(id) || id.length != 32)
            return false;
        var scoreRateBind, rateData, showChart;
        /**
         * 图表展示
         * @param $item
         */
        showChart = function ($item){
            var chartData = [],
                id = $item.data('qid'),
                rate = $item.data('rate'),
                rates;
            $item.addClass('on').siblings().removeClass('on');
            if(rateData.classScoreRates.hasOwnProperty(id))
                rates = rateData.classScoreRates[id];
            for (var key in rateData.classList) {
                if(!rateData.classList.hasOwnProperty(key))
                    continue;
                chartData.push({
                    name: rateData.classList[key],
                    value: rates ? (rates[key] || 0) : 0
                });
            }
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
                xFormat: function (item){
                    return S.formatClassName(item.name);
                }
            });
        };
        /**
         * 数据绑定
         */
        scoreRateBind = function (){
            var $main = $('.html-main'),
                data = {sorts: []};
            for (var id in rateData.questionSorts) {
                if(!rateData.questionSorts.hasOwnProperty(id))
                    continue;
                var rate = rateData.scoreRates[id];
                data.sorts.push({
                    id: id,
                    sort: rateData.questionSorts[id],
                    rate: rate
                });
            }
            var html = template('scoreRateTemp', data);
            $main.html(html);
            var $list = $main.find('li');
            showChart($list.eq(0));
            $main.find('li').bind('click', function (){
                var $t = $(this);
                if($t.hasClass('on'))
                    return false;
                showChart($t);
            });
        };
        examApi.getScoreRates(id, function (data){
            rateData = data;
            scoreRateBind();
        });
    };
    /**
     * 页面初始化
     */
    exam.event.init = (function (){
        S.pageLoading();
        api.setExamId($('.dy-main').data('examid'));
        var count = 0,
            success = function (){
                count++;
                if(count >= 2){
                    exam.event.pageLoad();
                }
            };
        api.getSubjects(function (data){
            exam.data.subjects = data;
            success();
        });
        api.getRanks(function (data){
            exam.data.ranks = data;
            success();
        });
    })();
    /**
     * 页面加载完毕
     */
    exam.event.pageLoad = function (){
        /*科目绑定*/
        var html = template('SubjectList', exam.data.subjects);

        $('.html-subject').html(html);

        if(exam.data.subjects.length <= 4){
            $('.html-subject').css('margin-top', '22px');
        }
        /**
         * 成绩排名
         */
        exam.event.resultRanking();

        S.pageLoaded();
    };
    /* 综合管理 */
    HtmlClass.body
        // 成绩排名考试班级-显示隐藏效果
        .on('change', '#ChoiceClass', function (){
            // 显示隐藏
            var $ChoiceShow = $('#ChoiceShow'),
                $ChoiceClass = $('#ChoiceClass'),
                $totalNumber = $('#totalNumber'),
                choicecontents = '.choice-contents',
                on = 'on',
                onTr = 'on-tr',
                checkText = $ChoiceClass.find("option:selected").text(),
                checkValue = $ChoiceClass.val();
            checkValue == 0 ? ( $ChoiceShow.removeClass(on)) : ( $ChoiceShow.addClass(on));
            $ChoiceShow.find(choicecontents).removeClass(onTr)
                .end().find('.class-name').each(function (){
                checkText == $(this).text() && ( $(this).parents(choicecontents).addClass(onTr));
            });
            // 自定义滚动条
            var $table = $('.dy-table-body'),
                $tableHead = $('.dy-table-head'),
                len = $table.find('.on-tr').length,
                whiteTr = $table.find('tr:eq(1)').width() + 1;
            if($ChoiceShow.hasClass('on')){
                whiteTr = $table.find('tr.on-tr:eq(1)').width() + 1;
            }
            $tableHead.addClass('on').removeClass('crt');
            if($table.find('.choice-contents:eq(0)').hasClass('on-tr')){
                $tableHead.removeClass('on');
            }
            if(checkValue == 0){
                len = $table.find('.choice-contents').length;
                $tableHead.removeClass('on');
            }
            $tableHead[0].style = '';
            if(len > 8){
                $table.siblings('.dy-table-head').addClass('crt').css('width', whiteTr);
            }

            // 实际人数和总人数
            var notNumber = $ChoiceShow.find('.on-tr').length,                                        // 本班人数
                totalNumber = $ChoiceShow.find('.choice-contents').length,                            //全年级总人数
                notClassNumber = $ChoiceShow.find('.on-tr').find('.miss-exam').length,               // 本班缺考人数
                notClassNumberAll = $ChoiceShow.find('.choice-contents').find('.miss-exam').length,  // 全年级缺考人数
                a = totalNumber - notClassNumberAll,     //年级实际考试人数
                b = totalNumber,                       //全年级人数
                c = notNumber - notClassNumber,          //本班人数实际考试人数
                d = notNumber;                       //本班人数

            // 全年级
            if(checkValue == 0){
                $totalNumber.find('b').html(a);
                $totalNumber.find('em').html(b);
                S.highcharts(a, b);
                return false;
            }
            $totalNumber.find('b').html(c);
            $totalNumber.find('em').html(d);
            S.highcharts(c, d);

        })
        // 成绩排名-未提交名单-弹框
        //        .on('click', '.js-no-name', function (event){
        //            event.preventDefault();
        //            S.NoName($('.popWindow'));
        //        })
        // 2级菜单选项卡
        .on('click mouseover mouseout', '.js-nav-main li', function (event){
            var $this = $(this),
                Event = event.type,
                $parents = $this.parents('.nav-main'),
                index = ~~$this.index() + 1,
                $ContentShow = $('.menu-main-' + index),
                $side = $('.side-' + index);


            if(Event == 'click'){
                var subjectId = $this.data('subject') || '';
                if(subjectId == exam.data.subjectId && $this.hasClass('current'))
                    return false;
                $this.data('subject', exam.data.subjectId);
                HtmlClass.$main.html('<div class="dy-loading"><i></i></div>');
                switch (index) {
                    case 1:
                        exam.event.resultRanking();
                        break;
                    case 2:
                        exam.event.subjectData();
                        break;
                    case 3:
                        exam.event.classKeys();
                        break;
                    case 4:
                        exam.event.classLayers();
                        break;
                    case 5:
                        exam.event.segment();
                        break;
                    case 6:
                        S.scoreRate(exam.data.subjectId);
                        break;
                }
                $this.addClass('current on').siblings().removeClass('current on');
                function Tab(obj){
                    obj.removeClass('hide').siblings().removeClass('currentstyle').addClass('hide');
                    return obj.siblings();
                }

                Tab($ContentShow).removeClass('current on');
                Tab($side);
            }
            if(Event == 'mouseover'){
                $this.addClass('on').siblings().removeClass('on');
            }
            if(Event == 'mouseout'){
                $this.removeClass('on');
                $parents.find('.current').addClass('on');
            }
        })
        // 综合概况-
        .on('click', '.js-add', function (event){
            event.preventDefault();
            exam.data.subjectId = '';
            exam.data.paperType = 1;
            $(this).addClass('on');
            $('.js-subject li').removeClass('on');
            $('#menu-nav-1').trigger("click");
            $('#menu-nav-4').removeClass('hide');
            $('#menu-nav-5 ,#menu-nav-6').addClass('hide');
        })
        // 单科
        .on('click', '.js-subject li', function (event){
            event.preventDefault();
            var $this = $(this);
            $('.js-add').removeClass('on');
            $this.addClass('on').siblings().removeClass('on');
            $('#menu-nav-4').addClass('hide');
            $('#menu-nav-5 ,#menu-nav-6').removeClass('hide');
            exam.data.subjectName = jQuery.trim($this.find('a').text());
            exam.data.subjectId = $this.data('subjectid');
            exam.data.paperType = $this.data('type');
            if(exam.data.subjectId){
                var liList = $('.js-nav-main').find('li');
                for (var i = 0; i < liList.length; i++) {
                    if($(liList[i]).hasClass('on')){
                        var index = i + 1;
                        console.log(index);
                        /*$('#menu-nav-1' + index).trigger("click");*/
                        //Amend 9:37 2017/1/24 点击回到初始值
                        $('#menu-nav-1').trigger("click"); //
                    }
                }
            } else {
                $('#menu-nav-1').trigger("click");
            }
        });
    /*上一页 下一页*/
    /*    HtmlClass.body
     .on('click', '.upper', function (event){
     event.preventDefault();
     var $navLi = $('.js-nav-main li');
     $navLi.each(function (index){
     if($(this).hasClass('on')){
     if(exam.data.subjectId){
     if(index == 0){
     index = $navLi.length;
     }
     if(index == 4){
     index = 3;
     }
     } else {
     if(index == 0){
     index = $navLi.length - 2;
     }
     }
     var $menuList = $('#menu-nav-' + (index));
     $menuList.trigger("click");
     return false;
     }
     });
     })
     .on('click', '.next', function (event){
     event.preventDefault();
     var $navLi = $('.js-nav-main li');
     $navLi.each(function (index){
     if($(this).hasClass('on')){
     if(exam.data.subjectId){
     if(index == 2){
     index = 3;
     }
     if(index == 5){
     index = -1;
     }
     } else {
     if(index == 3){
     index = -1;
     }
     }
     var $menuList = $('#menu-nav-' + (index + 2));
     $menuList.trigger("click");
     return false;
     }
     });
     });*/
    // 成绩统计下载
    $('#excel-export').click(function (){
        var subjectId = exam.data.subjectId,  // 科目ID
            keyParams = exam.config.keyParams, // 重点率参数
            layerParams = exam.config.layerParams, //学生分层参数
            subjectParams = exam.config.subjectParams, //学科统计参数
            subjectName = exam.data.subjectName, //学科统计参数
            number = 2;    // 类型：0,总排名;1,综合概况;2,单科统计;
        // 总排名 综合概况
        if(!subjectId){
            var off = $('.js-nav-main li').eq(0).hasClass('on');
            number = 1;
            if(off){
                number = 0;
            }
            api.download({
                type: number,
                classKeysParams: keyParams,
                classLayersParams: layerParams
            });
            return false;
        }
        subjectParams.subjectId = subjectId;
        // 单科
        api.download({
            type: number,
            subjectParams: subjectParams,
            subjectName: subjectName
        });

    });


})(jQuery, SINGER);

