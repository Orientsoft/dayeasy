/**
 * Created by bai on 2016/11/18.
 * 学情补救
 */

(function ($, S){
    var pageData = {},
        initPage,
        getData,
        agencyId = $('.dy-list').data('aid'),
        subjectErrorChart,
        errorCharts,
        subjectAnalysisCharts;

    S._mix(S, {
        /**
         * 错题下载变式过关Y轴数据居中处理
         * @param $download
         * @param totalArr
         * @param defaults
         */
        showChartStyle: function ($id, totalArr, defaults, color){
            var $downloadParents = $id.parents('li');
            $downloadParents.find('.amend-total').remove();
            $downloadParents.append('<div class="amend-total"><span class="total-left">' + totalArr[0] + '</span><span class="total-right">' + totalArr[1] + '</span></div>');
            var option = {
                yAxis: {
                    max: totalArr[1].total,
                    stops: [[1, color]],
                    tickPositions: [0, totalArr[1].total],
                    labels: {
                        enabled: false
                    }
                }
            }
            var opt = $.extend({}, defaults, option || {});
            return opt;
        }
    });

    initPage = function (data){
        var dataLabels = ['errors', 'errorKnowledge', 'variants', 'variantDownload', 'tutors', 'paperDownload'];
        S.each(dataLabels, function (label){
            $('#' + label).html(data[label] || 0);
        });
        subjectErrorChart(data.errorDetail);
        errorCharts(data.errorAnalysis, data.errorDownload, data.errorPass);
        subjectAnalysisCharts(data.subjectAnalysis);
    };
    /**
     * 获取页面数据
     * @param timeArea
     * @param refresh
     */
    getData = function (timeArea, refresh){
        var data = {
            id: agencyId,
            area: timeArea
        };
        if(refresh) data.refresh = true;
        $.get('/ea/agency-remedy', data, function (json){
            pageData[timeArea] = json;
            initPage(json);
            S.pageLoaded();
        });
    };
    /**
     * 科目错题分布
     */
    subjectErrorChart = function (chartData){
        var $chart = $('#container-left');
        $('.box-people .dy-nothing').remove();
        if(!chartData){
            S.loading.done($('.class-people'));
            $chart.parents('.box-people').find('.m-title').after(S.showNothing({
                css: {
                    width: "620px",
                    margin: '0 auto',
                    height: '240px',
                    lineHeight: '240px'
                }
            }));
            return false;
        }
        var data = {
            colors: ['#2cc49e', '#ffcb2d', '#e8525d', '#486378'],
            subjects: [],
            data: []
        };
        S.each(chartData, function (value, key){
            data.subjects.push(key);
            data.data.push(value);
        });
        var width = Math.max(data.subjects.length * 50, $chart.width());
        $chart.highcharts({
            chart: {
                width: width
            },
            title: {
                text: null
            },
            colors: data.colors,
            xAxis: {
                categories: data.subjects,
                text: null,
                tickWidth:0 //刻度标签宽度

            },
            credits: {
                enabled: false
            },
            plotOptions: {
                column: {
                    stacking: 'normal',
                    pointPadding: 0.2,
                    borderWidth: 0,
                    pointWidth: 24
                },
                series: {
                    pointPadding: 60, //数据点之间的距离值
                    groupPadding: 0, //分组之间的距离值
                    borderWidth: 0,
                    shadow: false,
                    pointWidth: 30 //柱子之间的距离值
                }
            },
            yAxis: {
                stackLabels: {
                    enabled: true,
                    style: {
                        color: '#656d78',
                        'font-weight': "normal"
                    }
                },
                gridLineWidth: 0,
                labels: {
                    enabled: false
                },
                title: {
                    text: null
                }
            },
            series: [{
                type: 'column',
                name: '错题数',
                colorByPoint: true,
                data: data.data,
                showInLegend: false
            }]
        });
    };
    /**
     * 错题图表
     * @param analysis
     * @param download
     * @param pass
     */
    errorCharts = function (analysis, download, pass){
//
        var options = {
                chart: {
                    type: 'solidgauge',
                    height: 250
                },
                title: null,
                pane: {
                    center: ['50%', '80%'],
                    size: '150%',
                    startAngle: -90,
                    endAngle: 90,
                    background: {
                        backgroundColor: '#96e2cf',
                        innerRadius: '70%',
                        outerRadius: '100%',
                        borderColor: '#96e2cf',
                        shape: 'arc'
                    }
                },
                tooltip: {
                    backgroundColor: 'rgba(255, 255, 255, 0)',
                    borderWidth: 0,
                    pointFormatter: function (){
                        //console.log(this);
                        return '<div class="d-chart-tip">' + this.series.name + ': <b>' + this.num + '/' + this.total + '</b></div>';
                    },
                    useHTML: true,
                    style: {zIndex: 88, display: 'none'}
                },
                credits: {
                    enabled: false
                },
                yAxis: {
                    min: 0,
                    max: 100,
                    stops: [[1, '#2cc49e']],
                    lineWidth: 0,
                    startOnTick: false,
                    endOnTick: false,
                    minorTickInterval: null,
                    tickPixelInterval: 400,
                    tickWidth: 0,
                    title: {y: 0},
                    labels: {
                        y: 16,
                        align: 'center',
                        useHTML: true,
                        formatter: function (){
                            return '<b>' + this.value + '</b>'
                        }
                    }
                },
                plotOptions: {
                    solidgauge: {
                        dataLabels: {
                            y: 0,
                            borderWidth: 0,
                            useHTML: true,
                            formatter: function (){
                                return '<div class="d-chart-center">' + this.point.percent.toFixed(1) + '%</div>'
                            }
                        },
                        mouseOver: function (){
                            this.tooltip.show();
                        }
                    }
                },
                series: [{
                    name: '',
                    data: [],
                    innerRadius: '70%'
                }]
            },
            showChart,
            percent,
            $analysis = $('#analysisChart'),
            $download = $('#downloadChart'),
            $pass = $('#passChart'),
            min = 0;
        showChart = function ($ele, option){
            var opt = $.extend(true, {}, options, option || {});
            $ele.highcharts(opt);
        };
        if(analysis){
            percent = (100 * analysis.count) / analysis.total;
            //错题分析
            showChart($analysis, {
                chart: {
                    height: 250
                },
                yAxis: {
                    max: analysis.total,
                    tickPositions: [0, analysis.total]
                },
                series: [{
                    name: '错题分析',
                    data: [{
                        y: Math.max(analysis.count, analysis.total * min),
                        percent: percent,
                        total: analysis.total,
                        num: analysis.count
                    }],
                    innerRadius: '70%'
                }],
                pane: {
                    center: ['50%', '80%'],
                    size: '150%',
                    startAngle: -90,
                    endAngle: 90,
                    background: {
                        backgroundColor: '#96e2cf',
                        innerRadius: '70%',
                        outerRadius: '100%',
                        borderColor: '#96e2cf',
                        shape: 'arc'
                    }
                }
            });
        } else {
            $analysis.html(S.showNothing({
                css: {
                    width: "620px",
                    margin: '0 auto',
                    height: '240px',
                    lineHeight: '240px'
                }
            }));
        }
        if(download){
            //错题下载
            var percent = (100 * download.count) / download.total;
            var defaults = {
                chart: {
                    height: 100
                },
                pane: {
                    background: {
                        backgroundColor: '#f4a9ae',
                        borderColor: '#f4a9ae'
                    }
                },
                yAxis: {
                    max: download.total,
                    stops: [[1, '#e8525d']],
                    tickPositions: [0, download.total]
                },
                plotOptions: {
                    solidgauge: {
                        dataLabels: {
                            y: 10
                        }
                    }
                },
                series: [{
                    name: '错题下载',
                    data: [{
                        y: Math.max(download.count, download.total * min),
                        percent: percent,
                        total: download.total,
                        num: download.count
                    }]
                }]

            };
            var opt = S.showChartStyle($download, [0, pass.total], defaults, '#e8525d');
            showChart($download, opt);
        } else {
            $download.html(S.showNothing());
        }
        if(pass){
            //错题过关
            var percent = (100 * pass.count) / pass.total;
            var defaults = {
                chart: {
                    height: 100
                },
                pane: {
                    background: {
                        backgroundColor: '#ffe596',
                        borderColor: '#ffe596'
                    }
                },
                yAxis: {
                    max: pass.total,
                    tickPositions: [0, pass.total],
                    stops: [[1, '#ffcb2d']]
                },
                plotOptions: {
                    solidgauge: {
                        dataLabels: {
                            y: 10
                        }
                    }
                },
                series: [{
                    name: '错题过关',
                    data: [{
                        y: Math.max(pass.count, pass.total * min),
                        percent: percent,
                        total: pass.total,
                        num: pass.count
                    }]
                }]
            };
            var opt = S.showChartStyle($pass, [0, download.total], defaults, '#ffcb2d');
            showChart($pass, opt);
        } else {
            $pass.html(S.showNothing());
        }
    };

    /**
     * 各科分析
     */
    subjectAnalysisCharts = function (analysisList){
        var $subjectAnalysis = $('#subjectAnalysis');
        $subjectAnalysis.empty();
        if(!analysisList || !analysisList.length){
            return false;
        }
        var showSubject,
            colors = [
                ['#ffcb2d', '#ffd34c', '#ffdc71', '#ffe494', '#ffedb7','#fff5d5'],
                ['#e8525d', '#eb6c75', '#ef8a91', '#f3a7ac', '#f7c4c8','#fadcdf'],
                ['#2cc49e', '#4bcdac', '#70d7bd', '#93e1ce', '#b7ebde','d5f3ec'],
                ['#486378', '#637a8c', '#8395a3', '#a2afba', '#c1cad1','dae0e4']
            ],
            icons = [
                'dy-icon-chinese', 'dy-icon-math', 'dy-icon-english',
                'dy-icon-physics', 'dy-icon-chemistry', 'dy-icon-politics',
                'dy-icon-history', 'dy-icon-geography', 'dy-icon-biology',
                'dy-icon-computer'
            ];
        showSubject = function (data){
            if(!data)
                return false;
            var chartData = [];
            S.each(data.users, function (item){
                chartData.push([item.key, item.value]);
            });
            var color = colors[(data.id - 1) % colors.length],
                option = {
                    chart: {
                        backgroundColor: 'transparent',
                        plotBackgroundColor: null,
                        plotBorderWidth: null,
                        plotShadow: false,
                        width: 480,
                        marginRight: 200,
                        height: 200
                    },
                    title: {
                        text: null
                    },
                    credits: {
                        enabled: false
                    },
                    tooltip: {
                        pointFormat: '<b>{point.y}次</b>'
                    },
                    legend: {
                        enabled: true,
                        useHTML: true,
                        width: 150,
                        maxHeight: 200,
                        align: 'right',
                        floating: true,
                        layout: 'vertical',
                        symbolRadius: 0,
                        //itemMarginBottom: 2,
                        x: -50,
                        y: -40
                    },
                    plotOptions: {
                        pie: {
                            size: 160,
                            innerSize: '100',
                            colors: color,
                            allowPointSelect: true,
                            cursor: 'pointer',
                            dataLabels: {
                                enabled: false,
                                format: '<b>{point.name}</b>: {point.percentage:.1f} %',
                                style: {
                                    color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
                                }
                            },
                            showInLegend: true
                        }
                    },
                    series: [{
                        type: 'pie',
                        name: data.name,
                        data: chartData
                    }]
                };
            var $chart = $('<div class="d-chart-item">');
            $subjectAnalysis.append($chart);
            $chart.highcharts(option);
            if(chartData.length == 0){
                $chart
                    .append('<div class="chart-empty"><img src="' + S.sites.static + '/v3/image/ea/chart-showNot-learning.png" alt=""><p class="not-text">暂无人标记错因</p></div>')
                    .addClass('not-chart');
            }
            $chart.append('<div class="chart-center"><i class="iconfont ' + icons[data.id - 1] + '"></i>' + data.key + '</div>');
        };
        S.each(analysisList, function (item){
            showSubject(item);
        });
    };
    $(document).delegate('.d-chart-center', 'mouseenter', function (){
        $(this).parents('.highcharts-data-labels').next().show();
    });
    $('.ui-tab-nav li').bind('click', function (){
        var $t = $(this), timeArea = $t.data('area');
        if($t.hasClass('on'))
            return false;
        $t.addClass('on').siblings().removeClass('on');
        if(pageData.hasOwnProperty(timeArea)){
            initPage(pageData[timeArea]);
            return false;
        }
        S.pageLoading();
        getData(timeArea);
    });
    S.pageLoading();
    getData(0);
})(jQuery, SINGER);

