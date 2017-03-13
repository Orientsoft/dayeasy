/**
 * Created by bai on 2016/11/18.
 * 考试地图
 */
// 源码代码解析 https://segmentfault.com/a/1190000000515420
!(function ($, S){
    var
        agencyId = $('.dy-list').data('aid'),
        pageData = {},
        subjectChart,
        classChart,
        initPage,
        getData;
    initPage = function (data){
        var dataLabels = ['userCount', 'classCount', 'paperCount', 'knowledgeCount', 'reportCount'];
        S.each(dataLabels, function (label){
            $('#' + label).html(data[label] || 0);
        });
        subjectChart(data.subjects);
        classChart(data.classList);
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
        $.get('/ea/examination-map', data, function (json){
            pageData[timeArea] = json;
            initPage(json);
            S.pageLoaded();
        });
    };
    /**
     * 学科分布
     * @param data
     */
    subjectChart = function (data){
        var chartData = [];
        if(data){
            S.each(data, function (value, key){
                chartData.push({
                    name: key,
                    y: value
                });
            });
            chartData[0].sliced = true;
            chartData[0].selected = true;
        } else {
            $('#container-chart-left').html('<div class="chart-show-not"><img src="' + S.sites.static + '/v3/image/ea/chart-showNot-map.png" alt=""></div>')
            return false;
            /*chartData.push({name: '暂无', y: 0, sliced: true});*/
        }
        $('#container-chart-left').highcharts({
            chart: {
                plotBackgroundColor: null,
                plotBorderWidth: null,
                plotShadow: false
            },
            title: {
                text: null
            },
            credits: {
                enabled: false
            },
            tooltip: {
                pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
            },
            plotOptions: {
                pie: {
                    size: 160,
                    innerSize: '100',
                    colors: [
                        '#e8525d',
                        '#486378',
                        '#2cc49e',
                        '#ffcb2d',
                        '#2cc49e',
                        '#ffcb2d'
                    ],
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: true,
                        format: '<b>{point.name}</b>: {point.y} 次',
                        style: {
                            color: "#666",
                            "fontWeight": "normal"
                        }
                    }
                }
            },
            series: [{
                type: 'pie',
                name: '学科分布',
                data: chartData
            }]
        });
    };
    /**
     * 班次分布
     * @param chartData
     */
    classChart = function (chartData){
        var $chart = $('#container-left'),
            $select = $("#container-tab-left"),
            $option,
            chartOption;
        $select.empty();
        if(!chartData || !chartData.length){
            $chart.html(S.showNothing());
            $select.hide();
            return false;
        }
        S.each(chartData, function (item){
            $option = $(S.format('<option value="{0}">{0}</option>', item.key));
            $option.data('chart', item.users);
            $select.append($option);
        });
        $select.show();
        chartOption = function ($ele){
            var data = {
                    type: 1,
                    colors: ['#2cc49e', '#ffcb2d', '#e8525d', '#486378'],
                    className: [],
                    data: []
                },
                chartData = $ele.data('chart');
            S.each(chartData, function (item){
                data.className.push(item.key);
                data.data.push({
                    y: item.value,
                    className: item.key
                });
            });

            S.binghighcharts({
                chartData: chartData,
                data: data,
                chart: $chart,
                select: $select,
                nothingText: '班级圈暂无考试'
            }, function (){
                var width = Math.max(data.className.length * 50, $chart.width());
                $chart.highcharts({
                    chart: {
                        type: 'category',
                        width: width
                    },
                    title: {
                        text: null
                    },
                    colors: data.colors,
                    tooltip: {
                        formatter: function (){
                            return S.format('{0}:{1}人次', this.point.className, this.y);
                        }
                    },
                    xAxis: {
                        categories: data.className,
                        tickWidth: 0, //刻度标签宽度
                        text: null,
                        labels: {
                            useHTML: true,
                            formatter: function (){
                                var regArr = /(\d+班)/gi.exec(this.value);
                                var name = regArr && regArr.length ? regArr[1] : this.value.substr(this.value.length - 2, this.value.length);
                                return S.format('<span title="{0}">{1}</span>', this.value, name);
                            }
                        }
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
                            pointPadding: 0, //数据点之间的距离值
                            groupPadding: 0, //分组之间的距离值
                            borderWidth: 0,
                            shadow: false,
                            pointWidth: 20 //柱子之间的距离值
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
                        name: '班次分布',
                        colorByPoint: true,
                        data: data.data,
                        showInLegend: false
                    }]
                });
            });

        };
        $select.change(function (){
            chartOption($select.find('option:selected'));
        }).change();
    };
    S._mix(S, {
        /**
         * 班级圈暂无考试
         * @param $ele
         * @param bingData
         * @param callback
         */
        binghighcharts: function (chartpar, callback){
            var dataNote = [], sum = 0;
            S.each(chartpar.chartData, function (item){
                dataNote.push(item.value);
            });
            $.each(dataNote, function (index){
                sum += dataNote[index]
            });
            if(sum > 0){
                callback && callback.call(this);
            } else {
                chartpar.chart.html(S.showNothing({
                    word: chartpar.select.val() + chartpar.nothingText,
                    css: {
                        height: '220px',
                        'line-height': '220px'
                    }
                }));
            }
        }
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





