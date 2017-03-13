/**
 * Created by shay on 2017/1/19.
 */
(function ($, S) {
    var batch = $('.dy-main').data('batch'),
        subjectId = 0,
        currentMenu = 1,
        api = S.chartApi,
        $contain = $('.html-main'),
        $side = $('.html-side'),
        focusOption = {},
        ranksView,
        averageView,
        focusRateView,
        segmentView,
        showView;
    /**
     * 成绩排名
     */
    ranksView = function () {
        var ranks, html = '', sideHtml = '';
        if (!subjectId) {
            //综合排名
            ranks = api.ranks();
            html = template('ranksTpl', ranks);
            sideHtml = template('sideCountTpl', ranks.count);
        } else {
            //单科排名
            ranks = api.subjectRanks(subjectId);
            html = template('subjectRanksTpl', ranks);
        }
        $contain.html(html);
        $side.html(sideHtml);
        $contain.tableScroll();
    };
    /**
     *  平均分对比
     */
    averageView = function () {
        var viewData = api.averageCompare(subjectId),
            html = template('averageTpl', {isAb: viewData.isAb}),
            initChart,
            $chart, $menus;
        $contain.html(html);
        $chart = $contain.find('.container02');
        $menus = $contain.find('.js-choice-li');
        initChart = function (chartData) {
            $chart.empty().chart({
                type: 'score',
                data: chartData.list,
                showAverage: true,
                max: chartData.max,
                direct: 'v',
                average: chartData.average,
                width: 956,
                height: 300,
                xTitle: '学校',
                chartWidth: 100,
                yTitle: '分数',
                xFormat: function (data) {
                    return data.name;
                }
            });
        };
        $menus.find('li').bind('click', function () {
            var $t = $(this),
                source = $t.data('source');
            if ($t.hasClass('on')) return false;
            $t.addClass('on').siblings().removeClass('on');
            if (viewData.hasOwnProperty(source)) {
                initChart(viewData[source]);
            } else {
                $chart.html('<div class="dy-nothing">报表数据异常</div>');
            }
        }).eq(0).click();
    };
    /**
     * 重点率
     */
    focusRateView = function () {
        if (!focusOption.hasOwnProperty(subjectId))
            focusOption[subjectId] = {};
        var viewData = api.focusRateAnalysis(subjectId, focusOption[subjectId]),
            html = template('focusRateTpl', focusOption[subjectId]),
            initChart,
            $chart, $menus;
        $contain.html(html);
        $chart = $contain.find('.container03');
        $menus = $contain.find('.js-choice-li');
        initChart = function (chartData) {
            console.log(chartData);
            $chart.empty().chart({
                type: 'percent',
                data: chartData.list,
                showAverage: true,
                max: 100,
                direct: 'v',
                average: chartData.average,
                width: 956,
                height: 280,
                xTitle: '学校',
                yTitle: '百分比',
                xFormat: function (data) {
                    return data.name;
                }
            });
        };
        $menus.find('li').bind('click', function () {
            var $t = $(this),
                source = $t.data('source');
            if ($t.hasClass('on')) return false;
            $t.addClass('on').siblings().removeClass('on');
            if (viewData.hasOwnProperty(source)) {
                initChart(viewData[source]);
            } else {
                $chart.html('<div class="dy-nothing">报表数据异常</div>');
            }
        }).eq(0).click();
        //计算
        var $btn = $('#KeyCalculation'),
            $focus = $('#FractionAll'),
            $qualified = $('#FractionA'),
            $failure = $('#FractionAb');
        $contain.find('input').bind('change', function () {
            $btn.removeAttr('disabled').removeClass('disabled');
        });
        $btn.bind('click', function () {
            $btn.attr('disabled', 'disabled').addClass('disabled').blur();
            var option = focusOption[subjectId];
            option.keyScore = parseFloat($focus.val());
            option.scoreA = parseFloat($qualified.val());
            option.unScoreA = parseFloat($failure.val());
            viewData = api.focusRateAnalysis(subjectId, option);
            console.log(viewData);
            $menus.find('li').removeClass('on').eq(0).click();
            $btn.removeAttr('disabled').removeClass('disabled');
        });
    };
    /**
     * 分数段
     */
    segmentView = function () {
        if (!subjectId) {
            $contain.html('');
            return false;
        }
        var viewData = api.segmentDist(subjectId),
            html = template('segmentTpl', viewData);
        $contain.html(html);

    };
    api.init(batch, function () {
        ranksView();
    });
    showView = function () {
            $contain.html('<div class="dy-loading"><i></i></div>');
            $side.html('');
            switch (currentMenu) {
                case 1:
                    ranksView();
                    break;
                case 2:
                    averageView();
                    break;
                case 3:
                    focusRateView();
                    break;
                case 4:
                    segmentView();
                    break;
            }
    };

    var $subjects = $('.js-subject'),
        $comprehensive = $('.comprehensive'),
        $subjectMenus = $('.m-subject'),
        $nav = $('.js-nav-main');
    //科目
    $subjects.on('click', 'li', function () {
        var $t = $(this),
            id = ~~$t.data('subject');
        if (subjectId == id) return false;
        $t.addClass('on').siblings().removeClass('on');
        $subjectMenus.removeClass('hide');
        $comprehensive.removeClass('on');
        subjectId = id;
        showView();
    });
    //综合
    $comprehensive.bind('click', function () {
        if (!subjectId) return false;
        $comprehensive.addClass('on');
        $subjectMenus.addClass('hide');
        $subjects.find('li').removeClass('on');
        subjectId = 0;
        if (currentMenu == 4) {
            currentMenu = 1;
            $nav.find('li').eq(0).addClass('on current');
        }
        showView();
    });
    //二级菜单
    $nav.on('click', 'li', function () {
        var $t = $(this),
            menu = ~~$t.data('menu');
        if (currentMenu == menu) return false;
        currentMenu = menu;
        $t.addClass('on current').siblings().removeClass('on current');
        showView();
    });
    //单个学生成绩
    $contain.on('click', '.js-students-pop', function () {
        var $t = $(this),
            id = $t.data('uid');
        var data = api.studentScores(id); //S.resultRankingUi(rankData, index);
        var htmlNames = template('studentScoresTpl', data);
        $('#SubjectsAll').html(htmlNames);
        /*调用弹框*/
        $('.pop-result-ranking').popWindow(data.name);
    });
    $('#excel-export').bind('click', function () {
        api.download();
    });
})(jQuery, SINGER);