/**
 * Created by bai on 2016/11/18.
 * 师生画像
 */

(function ($, S){
    var pageData = {},
        initPage,
        getData,
        agencyId = $('.dy-list').data('aid'),
        loginChart,
        teacherChart,
        studentChart,
        rankList;
    /**
     * 加载页面
     * @param data
     */
    initPage = function (data, timeArea){
        loginChart(data.logins || [], timeArea);
        teacherChart(data.teacher || {});
        studentChart(data.student || {});
        rankList(data.loginRank, data.markingRank, data.visitRank);
    };
    /**
     * 登录图表
     */
    loginChart = function (loginData, timeArea){
        var categories = [], data = [];
        S.each(loginData, function (item){
            categories.push(S.formatDate(new Date(item.time), 'yyyy-MM-dd'));
            data.push({
                student: item.student,
                teacher: item.teacher,
                y: item.total
            });
        });

        //Amend 2016/12/13 Bug 1817 X轴 年份显示不全问题
        var interval = [1, 5, 16, 33][timeArea] || Math.max(Math.floor(categories.length / 8.0), 1);
        $('#container-area-draw').highcharts({
            chart: {
                type: 'area',
                spacingBottom: 30,
                height: 300
            },
            title: {
                text: null
            },
            subtitle: {
                text: null
            },
            legend: {
                layout: 'vertical',
                align: 'left',
                verticalAlign: 'top',
                x: 150,
                y: 100,
                floating: true,
                borderWidth: 1,
                backgroundColor: '#FFFFFF'
            },
            xAxis: {
                categories: categories,
                tickInterval: interval,
                showLastLabel: true,
                labels: {
                    useHTML: true,
                    formatter: function (){
                        return this.value;
                    }
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
            tooltip: {
                useHTML: true,
                formatter: function (){
                    return S.format('<table class="dy-table page-area-draw"><caption>{0}</caption><tr><td>学生</td><td>教师</td><td>总计</td></tr><tr><td>{1}</td><td>{2}</td><td>{3}</td></tr></table>',
                        this.x, this.point.student, this.point.teacher, this.y);
                }
            },
            plotOptions: {
                area: {
                    fillOpacity: 0.5,
                    color: '#647b8d',
                    fillColor: '#f6f7f8'
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                name: '登录',
                showInLegend: false,
                data: data
            }]
        });
    };
    /**
     * 教师画像
     */
    teacherChart = function (data){
        var $box = $(".teacher-painting").find(".num"),
            $chart = $("#container-chart-left");

        $box.eq(0).html(data.averageLogin);
        $box.eq(1).html(data.averageMarked);
        var chartData = [];
        if(data.portraits){
            $chart.show().siblings('.pos-img').show();
            $chart.parents('.wrap-chart').find('.chart-show-not').remove();
            S.each(data.portraits, function (value, key){
                chartData.push({
                    name: key,
                    y: value
                });
            });
            chartData[0].sliced = true;
            chartData[0].selected = true;
        } else {
            /*chartData.push({name: '暂无', y: 0, sliced: true});*/
            $chart.hide().siblings('.pos-img').hide();
            $chart.parents('.wrap-chart').append('<div class="chart-show-not"><img src="' + S.sites.static + '/v3/image/ea/chart-showNot1.png" alt=""></div>');
            return false;
        }
        $chart.highcharts({
            chart: {
                backgroundColor: 'transparent',
                plotBackgroundColor: null,
                plotBorderWidth: 0,
                plotShadow: false
            },
            title: {
                text: null
            },
            credits: {
                enabled: false
            },
            tooltip: {
                pointFormat: '{series.name}: <b>{point.y}({point.percentage:.1f} %)</b>'
            },
            plotOptions: {
                pie: {
                    size: 160,
                    innerSize: '100',
                    colors: [
                        '#2cc49e',
                        '#4bcdac',
                        '#70d7bd',
                        '#93e1ce',
                        '#b7ebde',
                        '#d5f3ec'
                    ],
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: true,
                        /*format: '<b>{point.name}</b>: {point.y}',*/
                        format: '<b>{point.name}</b>',
                        style: {
                            color: '#666'
                        }
                    }
                }
            },
            series: [{
                type: 'pie',
                name: '教师印象',
                data: chartData
            }]
        });
    };
    /**
     * 学生画像
     */
    studentChart = function (data){
        var $box = $('.student-painting').find('.num'),
            $chart = $("#container-chart-right");
        $box.eq(0).html(data.averageLogin);
        $box.eq(1).html(data.averageExam);
        $box.eq(2).html(data.averageError);
        var chartData = [];
        if(data.portraits){
            $chart.show().siblings('.pos-img').show();
            $chart.parents('.wrap-chart').find('.chart-show-not').remove();
            S.each(data.portraits, function (value, key){
                chartData.push({
                    name: key,
                    y: value
                });
            });
            chartData[0].sliced = true;
            chartData[0].selected = true;
        } else {
            $chart.hide().siblings('.pos-img').hide();
            $chart.parents('.wrap-chart').append('<div class="chart-show-not"><img src="' + S.sites.static + '/v3/image/ea/chart-showNot2.png" alt=""></div>');
            return false;
            /*chartData.push({name: '暂无', y: 0, sliced: true});*/
        }
        $chart.highcharts({
            chart: {
                backgroundColor: 'transparent',
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
                pointFormat: '{series.name}: <b>{point.y}({point.percentage:.1f} %)</b>'
            },
            plotOptions: {
                pie: {
                    size: 160,
                    innerSize: '100',
                    colors: [
                        '#e8525d',
                        '#eb6c75',
                        '#ef8a91',
                        '#f3a7ac',
                        '#f7c4c8',
                        '#fadcdf'
                    ],
                    allowPointSelect: true,
                    cursor: 'pointer',
                    dataLabels: {
                        enabled: true,
                        /*format: '<b>{point.name}</b>: {point.y}',*/
                        format: '<b>{point.name}</b>',
                        style: {
                            color: '#666'
                        }
                    }
                }
            },
            series: [{
                type: 'pie',
                name: '错因分析',
                data: chartData
            }]
        });
    };
    /**
     * 光荣榜
     */
    rankList = function (login, marking, visit){
        var $box = $('.punch-master dd'), loginRank, markingRank, visitRank;
        if(login && login.length){
            loginRank = template('rankTpl', {
                ranks: login,
                rankStandard: '登录次数'
            });
        } else {
            loginRank = S.showNothing({
                css: {
                    'height': '240px',
                    'line-height': '240px'
                }
            });
        }
        $box.eq(0).html(loginRank);
        if(marking && marking.length){
            markingRank = template('rankTpl', {
                ranks: marking,
                rankStandard: '批阅班次'
            });
        } else {
            markingRank = S.showNothing({
                css: {
                    'height': '240px',
                    'line-height': '240px'
                }
            });
        }
        $box.eq(1).html(markingRank);
        if(visit && visit.length){
            visitRank = template('rankTpl', {
                ranks: visit,
                rankStandard: '主页访问'
            });
        } else {
            visitRank = S.showNothing({
                css: {
                    'height': '240px',
                    'line-height': '240px'
                }
            });
        }
        $box.eq(2).html(visitRank);
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
        $.get('/ea/agency-portrait', data, function (json){
            pageData[timeArea] = json;
            initPage(json, timeArea);
            S.pageLoaded();
        });
    };

    $('.ui-tab-nav li').bind('click', function (){
        var $t = $(this), timeArea = $t.data('area');
        if($t.hasClass('on'))
            return false;
        $t.addClass('on').siblings().removeClass('on');
        if(pageData.hasOwnProperty(timeArea)){
            initPage(pageData[timeArea], timeArea);
            return false;
        }
        S.pageLoading();
        getData(timeArea);
    });
    getData(0);

})(jQuery, SINGER);

