/**
 * 批阅任务
 * Created by shay on 2016/6/30.
 */
(function ($, S){
    var jointBatch,
        missionDialog,
        missionElement,
        loadChart,
        options,
        initMission,
        showMission,
        startMarking,
        eventBind,
        BindObjectiveshow,
        logger = S.getLogger('marking-mission');

    S._mix(S, {
        /**
         * 根据字符串length添加挂钩
         * @param str
         */
        addLengthClass: function (str){
            if(str.length < 3) return '';
            var classArray = [
                'd-sort-3',
                'd-sort-4',
                'd-sort-5'
            ];
            var index = Math.min(str.length - 3, classArray.length - 1);
            return ' ' + classArray[index];
        }
    });

    /**
     * 默认配置
     */
    options = {
        chart: {
            type: 'solidgauge',
            backgroundColor: '#fff',
            spacingTop: 0,
            spacingLeft: 0,
            spacingRight: 0,
            spacingBottom: 0
        },
        title: null,
        pane: {
            startAngle: 0,
            endAngle: 360,
            background: {
                backgroundColor: '#eee',
                innerRadius: '80%',
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
            stops: [[1, '#ed5565']],
            lineWidth: 0,
            minorTickInterval: null,
            tickPixelInterval: 400,
            tickWidth: 0,
            title: null,
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
    };

    /**
     * 加载每道题的进度
     * @param $rank
     */
    loadChart = function ($rank){
        var rank = ~~$rank.data('rank'),
            label = $rank.html();
//        if (rank == 0 || rank == 100) {
//            $(mission).addClass('d-bordered');
//            if (rank == 100)
//                $(mission).addClass('d-finished');
//
//        }
        var chartOption = $.extend(true, {}, {
            chart: {
                events: {
                    click: function (){
                        showMission($rank);
                    }
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                name: '班级排名',
                data: [rank],
                innerRadius: '80%',
                dataLabels: {
                    format: S.format('<strong class="d-sort{0}">{1}</strong>', S.addLengthClass(label), label),
                    y: 12,
                    x: -1,
                    padding: 0
                }
            }]
        }, options);

        if(rank == 0 || rank == 100){
            if(rank == 100)
                chartOption = $.extend(true,{},chartOption, {
                    yAxis: {
                        stops: [[1, '#2E94B9']],
                    }
                });
        }
        $rank.highcharts(chartOption);
    };
    /**
     * 初始化任务
     */
    initMission = function (){
        jointBatch = $('.dm-header').data('joint');
        var $missions = $('.d-areas .d-mission-item');
        for (var i = 0; i < $missions.length; i++) {
            loadChart($missions.eq(i).find('.d-rank'), $missions[i]);
        }
    };
    /**
     * 展示题目批阅进度
     */
    showMission = function ($rank){
        missionDialog && missionDialog.close().remove();
        if(missionElement == $rank){
            missionElement = null;
            return false;
        }
        missionElement = $rank;
        var $details = $rank.next().html();
        missionDialog = S.dialog({
            content: $details,
            padding: 10,
            align: 'top',
            width: 130,
            skin: 'd-progress-dialog',
            cancelValue: '关闭',
            cancelDisplay: false
            //quickClose: true
            //backdropOpacity: 0.5
        });
        missionDialog.show($rank.get(0));
    };
    /**
     * 开始批阅
     */
    startMarking = function (qList){
        logger.info(qList);
        var postUrl, $form, $input;
        postUrl = '/marking/combine/' + jointBatch;
        $form = $('<form method="post" class="hide">');
        $form.attr('action', postUrl);
        $input = $('<input type="text" name="qList" />');
        $input.val(encodeURIComponent(S.json(qList)));
        $form.append($input);
        $('body').append($form);
        $form.submit();
    };
    /**
     * 事件绑定
     */
    eventBind = function (){
        //我的任务
        $('.d-mission')
            .bind('click', function (){
                var $t = $(this);
                if($t.hasClass('disabled'))
                    return false;
                $(this).toggleClass('active');
                var $missions = $('.d-mission.active'),
                    $btn = $('#markingBtn');
                if($missions.length > 0){
                    $btn.undisableFieldset();
                } else {
                    $btn.disableField();
                }
            })
            .bind('mouseenter', function (){
                var $t = $(this);
                if($t.hasClass('active') || $t.hasClass('disabled'))
                    return false;
                $t.find('.d-word').removeClass('hide');
                $t.find('.d-progress').addClass('hide');
            })
            .bind('mouseleave', function (){
                var $t = $(this);
                if($t.hasClass('active') || $t.hasClass('disabled'))
                    return false;
                $t.find('.d-word').addClass('hide');
                $t.find('.d-progress').removeClass('hide');
            });
        $('#markingBtn').bind('click', function (){
            var $btn = $(this), $actives, i, qList = [];
            $btn.blur();
            if($btn.hasClass('disabled'))
                return false;
            $actives = $('.d-mission.active');
            if($actives.length == 0){
                S.msg('请选择要批阅的题目！');
                return false;
            }
            $btn.disableField('跳转中...');
            for (i = 0; i < $actives.length; i++) {
                var list = $actives.eq(i).data('qids').split(',');
                qList.push(list);
            }
            startMarking(qList);
        });
    };
    /**
     * 客观UI版块效果
     * @constructor
     */
    BindObjectiveshow = function (){
        //查看全部切换效果
        var EffectObjectiveshow,
            BindObjectiveshow,
            $objectspan = $('.top-right-tab'),
            $parent = $('.m-object-con'),
            $boxcon = $('.j-contents'),
            $TabCon = $('.j-tab-con'),
            $tabnav = $('.top-right-tab');
        S.loading.start($parent);
        /**
         * 绑定效果
         */
        EffectObjectiveshow = function (){
            //初始化页面客观题低于{50%.length==0}
            $('.j-object-list').each(function (){
                var $t = $(this);
                if($t.find('dd').hasClass('nothing')){
                    return false;
                }
                if(!($t.find('dd').hasClass('no-qua'))){
                    var text = $t.find('.paper-a').text();
                    var html = '';
                    html += '<dd class="nothing">';
                    html += S.showNothing({
                        word: text + '没有低于50%',
                        icon: 'dy-icon-emoji02',
                        css: {
                            width: '940px',
                            height: '80px'
                        }
                    });
                    html += '</dd>';
                    $t.addClass('asdasd');
                    $t.append(html);
                }
            });
            var timer;
            $objectspan.on('click', 'span', function (event){
                event.stopPropagation();
                var $t = $(this),
                    dataval = $t.data('showtab');
                timer && timer.cancel();
                $t.addClass('on').siblings('span').removeClass('on');

                if(dataval == 'low'){
                    $TabCon.addClass('animation');
                    if(S.UA.ie){
                        $TabCon.removeClass('current');
                    } else {
                        timer = S.later(function (){
                            $TabCon.removeClass('current');
                            timer.cancel();
                        }, 1500, false);

                    }
                    return false;
                }
                if(dataval == 'all'){
                    $TabCon.addClass('current').removeClass('animation');
                    return false;
                }
            });
        };
        /**
         * 绑定数据
         */
        BindObjectiveshow = function (data, callback){
            var
                html,
                bindhtml,
                DataScoreRate;
            S.loading.done($parent);
            $boxcon.removeClass('hide');
            $tabnav.removeClass('hide');
            /**
             *
             * return html
             */
            bindhtml = function (data){
                var html = '';
                var bindab;
                var paperAQuestions = data.list.paperAQuestions;
                var paperACount = data.list.paperACount;
                var paperBQuestions = data.list.paperBQuestions;
                var paperBCount = data.list.paperBCount;
                /**
                 *
                 * @param questions
                 * @param num 上传份数
                 * @param ab   AB卷
                 */
                bindab = function (questions, num, ab){
                    var addclass = function (scoreRate){
                        /**
                         *  超过50 ，没有超过50， 未提交1 挂钩
                         * @type {string[]}
                         */
                        var aryStyle = ['qua', 'no-qua', 'qua submi-not'];
                        var cN;
                        cN = aryStyle[0];
                        if(-1 < scoreRate && scoreRate < 50){
                            cN = aryStyle[1];
                        }
                        if(-1 == scoreRate){
                            cN = aryStyle[2]
                        }
                        return cN;
                    };
                    html += '<dl class="m-object-list j-object-list f-cb">';
                    html += '<dt>';
                    html += '<span class="paper-a">' + ab + '卷</span>已上传<span class="paper-b">' + num + '</span>份';
                    html += '</dt>';
//                    questions = demoData = [
//                        {
//                            id: "008cdfd1734444a18b0e687595ec3283",
//                            rightKey: "B",
//                            scoreRate: 0,
//                            sort: "1"
//                        },
//                        {
//                            id: "008cdfd1734444a18b0e687595ec3283",
//                            rightKey: "B",
//                            scoreRate: -1,
//                            sort: "2"
//                        },
//                        {
//                            id: "008cdfd1734444a18b0e687595ec3283",
//                            rightKey: "c",
//                            scoreRate: 80,
//                            sort: "3"
//                        }
//                    ];
                    if(questions.length > 0){
                        for (var i = 0; i < questions.length; i++) {
                            html += '<dd class="dd' + ' ' + addclass(questions[i].scoreRate) + '">';
                            html += '<span class="obj-t"><b>' + questions[i].sort + '</b><em>' + questions[i].rightKey + '</em></span>';
                            if(questions[i].scoreRate == -1){
                                html += '<span class="obj-b"><em class="font">未提交</em></span>';
                            } else {
                                html += '<span class="obj-b">' + questions[i].scoreRate + '%</span>';
                            }
                            html += '</dd>';
                        }
                    } else {
                        html += '<dd class="nothing base-node">';
                        html += S.showNothing({
                            word: ab + '卷没有客观题',
                            css: {
                                width: '940px',
                                height: '80px',
                            }
                        });
                        html += '</dd>';
                    }
                    html += '</dl>';
                };
                bindab(paperAQuestions, paperACount, data.text);
                data.type == 2 && bindab(paperBQuestions, paperBCount, 'B');
                return html;
            };
            //是否是AB卷
            if(data.isAb){
                DataScoreRate = {
                    type: 2,
                    text: 'A',
                    list: data
                };
            } else {
                DataScoreRate = {
                    type: 1,
                    text: '试',
                    list: data
                };
            }
            html = bindhtml(DataScoreRate);
            $TabCon.append(html);
            callback && callback.call(this);
        };
        /**
         * 请求数据
         */
        $.ajax({
            type: 'GET',
            url: '/marking/object_ScoreRate',
            data: {joinBatch: jointBatch},
            dataType: 'json',
            success: function (json){
                if(json.status){
                    BindObjectiveshow(json.data, function (){
                        EffectObjectiveshow();
                    });
                } else {
                    var html = S.showNothing({
                        word: json.message,
                        css: {
                            margin: ' 0px 20px',
                            height: '80px',
                            width: '1160px'
                        }
                    });
                    $parent.html(html);
                }
            }
        });
    };
    initMission();
    eventBind();
    BindObjectiveshow();

})(jQuery, SINGER);