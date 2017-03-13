/**
 * Created by bai on 2016/11/18.
 */

(function ($, S){
    var getData,
        initPage,
        agencyId = $('.dy-list').data('aid'),
        classChart,
        colleagueChart,
        pageData;

    S._mix(S, {
        chartFun: function (id, data, options){
            var
                opts,
                Default,
                lenChart = data.className.length,
                $id = $('#' + id);
            //类型  1 树状  2横状
            switch (data.type) {
                case 1:
                    $id.width(Math.max(lenChart, 10) * 44);
                    break;
                case 2:
                    $id.height(Math.max(lenChart, 5) * 44);
                    break;
                default :
                    console.log('没有你调用的树状类型类型');
            }
            //表格默认属性值
            Default = {
                title: {
                    text: null
                },
                colors: data.colors,
                xAxis: {
                    categories: data.className,
                    text: null,
                    labels: {
                        useHTML: true,
                        formatter: data.formatter || function (){
                            return this.value;
                        }
                    },
                    tickWidth:0 //刻度标签宽度
                },
                tooltip: {
                    formatter: function (){
                        return S.format('{0}:{1}人', this.x, this.y);
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
                    },
                    tickWidth:0 //刻度标签宽度
                },
                series: [{
                    type: 'column',
                    name: '人数',
                    colorByPoint: true,
                    data: data.data,
                    showInLegend: false
                }]
            };
            opts = $.extend({}, Default, options || {});
            return $id.highcharts(opts);
        },
        /**
         * 该年级班级圈暂无学生 -该年级同事圈暂无老师
         * @param $ele
         * @param bingData
         * @param callback
         */
        binghighcharts: function (bingData, callback){
            var fnData = new bingData(),
                par = fnData.par,
                data = fnData.par.data,
                sum = 0;
            $.each(data, function (index){
                sum += data[index]
            });
            if(sum > 0){
                par.box.css('display', 'block');
                par.people.find('.dy-nothing').remove();
                callback && callback.call(this);
            } else {
                par.chart.empty();
                par.box.css('display', 'none');
                par.people.find('.dy-nothing').remove();
                par.people.append(S.showNothing({
                    word: par.select.val() + par.nothingText,
                    css: {
                        margin: '0 20px',
                        width: '440px',
                        height: '220px',
                        lineHeight: '220px'
                    }
                }));
            }
        }
    });
    /**
     * 初始化页面
     */
    initPage = function (){
        if(!pageData)
            return;
        var dataLabels = ['studentCount', 'teacherCount', 'classCount', 'colleagueCount', 'visitCount', 'targetCount'];
        S.each(dataLabels, function (label){
            $('#' + label).html(pageData[label] || 0);
        });
        classChart();
        colleagueChart();
        $('.last-refresh').find('span').html(pageData.lastRefresh);
//        $(".container-box-x").mCustomScrollbar({
//            axis: "x",
//            theme: "dark-thin",
//            autoExpandScrollbar: true,
//            advanced: {
//                autoExpandHorizontalScroll: true,
//                updateOnContentResize: true
//            }
//        });
//
//        $(".container-box-y").mCustomScrollbar({
//            axis: "y",
//            theme: "dark-thin",
//            autoExpandScrollbar: true,
//            advanced: {autoExpandHorizontalScroll: true}
//        });
    };
    /**
     * 班级圈
     */
    classChart = function (){
        if(!pageData.classList)
            return false;
        var $select = $("#container-tab-left"),
            $option,
            chartOption;
        $select.empty();
        S.each(pageData.classList, function (item){
            $option = $(S.format('<option value="{0}">{0}</option>', item.key));
            $option.data('chart', item.users);
            $select.append($option);
        });

        chartOption = function ($ele){
            var data = {
                type: 1,
                id: 'container-left',
                colors: ['#e8525d', '#486378'],
                className: [],
                data: [],
                dataY: [],
                formatter: function (){
                    var regArr = /(\d+班)/gi.exec(this.value);
                    var name = regArr && regArr.length ? regArr[1] : this.value.substr(this.value.length - 2, this.value.length);
                    return S.format('<span title="{0}">{1}</span>', this.value, name);
                }
            };
            var chartData = $ele.data('chart');
            S.each(chartData, function (item){
                data.className.push(item.key);
                data.data.push({
                    y: item.value,
                    name: item.key
                });
                data.dataY.push(item.value);
            });
            S.binghighcharts(function (){
                var _this = this;
                _this.par = {
                    data: data.dataY,
                    chart: $('#' + data.id),
                    select: $select,
                    people: $('.class-people'),
                    box: $('.container-box-x'),
                    nothingText: '班级圈暂无学生'
                };
            }, function (){
                S.chartFun(data.id, data);
            })
        };
        $select.change(function (){
            chartOption($select.find('option:selected'));
        }).change();
    };
    /**
     * 同事圈
     */
    colleagueChart = function (){
        if(!pageData.colleagueList)
            return false;
        var $select = $("#container-tab-right"), $option, chartOption;
        $select.empty();
        S.each(pageData.colleagueList, function (item){
            $option = $(S.format('<option value="{0}">{0}</option>', item.key));
            $option.data('chart', item.users);
            $select.append($option);
        });
        chartOption = function ($ele){
            var data = {
                type: 2,
                id: 'container-right',
                colors: ['#2cc49e', '#ffcb2d'],
                className: [],
                data: []
            }, options = {
                chart: {
                    inverted: true,
                    polar: false
                },
                subtitle: {
                    text: null
                }
            };
            var chartData = $ele.data('chart');
            S.each($ele.data('chart'), function (item){
                data.className.push(item.key);
                data.data.push(item.value);
            });
            S.binghighcharts(function (){
                var _this = this;
                _this.par = {
                    chartData: chartData,
                    data: data.data,
                    chart: $('#' + data.id),
                    select: $select,
                    people: $('.teacher-people'),
                    box: $('.container-box-y'),
                    nothingText: '同事圈暂无老师'
                }
            }, function (){
                S.chartFun(data.id, data, options);
            })
        };
        $select.change(function (){
            chartOption($select.find('option:selected'));
        }).change();
    };
    /**
     * 加载页面数据
     */
    getData = function (refresh){
        S.pageLoading();
        var data = {id: agencyId, t: Math.random()};
        if(refresh) data.refresh = true;
        $.get('/ea/agency-survey', data, function (data){
            pageData = data;
            initPage();
            S.pageLoaded();
        });
    };
    getData();
    $('.last-refresh').find('i').bind('click', function (){
        getData(true);
    });
})(jQuery, SINGER);
