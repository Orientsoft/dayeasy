$(function () {
    $.ajaxSetup({
        //发送请求前触发
        beforeSend: function (xhr) {
            singer.pageLoading();
        },
        complete: function (xhr, status) {
            singer.pageLoaded();
            //处理选中的状态
            var currentSelectData = paperBar.getCurrentData();
            if (currentSelectData) {
                var qItems;
                if ($(".topic-tab-content-1").hasClass('show')) {
                    qItems = $("#questionListDiv").children("div.q-item");
                } else {
                    qItems = $("#paperDetails").find("#questionsDiv").children("div.q-item");
                }

                $.each(qItems, function (qIndex, qItem) {
                    var fItem = $(qItem).children("div.f-qitem");
                    var qId = fItem.data('qid');

                    $.each(currentSelectData, function (tIndex, tItem) {
                        if (tItem) {
                            $.each(tItem, function (index, item) {
                                if (item && item.QId == qId) {
                                    fItem.children('div.g-mark').children('span').removeClass("g-icon-15").addClass("g-icon-14");
                                }
                            });
                        }
                    });
                });
            }
        }
    });


    var paperBar = $.TestBar(options);//创建试题栏

    var qEvents = {
        areaInit: function () {
            var area = {
                '默认': ['全国'],
                '省': ['四川', '广东', '湖南', '浙江', '江苏', '山东', '福建', '甘肃', '河南', '河北', '吉林', '陕西', '山西'],
                'ABC': ['北京', '巴中', '重庆', '成都'],
                'DEF': ['达州'],
                'GHJ': ['杭州', '淮安'],
                'KLM': ['兰州', '莱芜', '龙岩', '眉山'],
                'NPQ': ['南通', '南充', '内江', '宁夏回族自治区'],
                'RST': ['遂宁', '天津'],
                'WXY': ['武汉', '威海', '湘潭', '扬州', '雅安'],
                'Z': ['自贡']
            };

            $.each(area, function (index, item) {
                var li = '<li><label>' + index + '：</label><div class="f-fl u-g-1">';

                var areaStr = '';
                $.each(item, function (aIndex, aItem) {
                    var areaValue = aItem;
                    if (aItem == "全国") {
                        areaValue = "-1";
                    }
                    areaStr += '<span data-area="' + areaValue + '">' + aItem + '</span>';
                });

                li += areaStr;
                li += '</div></li>';
                $("#areaDiv").children('ul').append(li);
            });
        },
        selectYearClick: function () {
            $("#yearDiv").toggleClass('hide');
            $("#areaDiv").addClass('hide');
            return false;
        },
        selectAreaClick: function () {
            $("#areaDiv").toggleClass('hide');
            $("#yearDiv").addClass('hide');
            return false;
        },
        yearChange: function () {
            $("#yearDiv").addClass("hide");
            var text = $(this).text();
            if ($("#selectYear").children('span').text() == text) {
                return false;
            }

            var year = $(this).data('year');
            $("#selectYear").children('span').data('year', year).text(text);

            qEvents.questionSearch();
            return false;
        },
        areaChange: function () {
            $("#areaDiv").addClass("hide");
            var text = $(this).text();
            if ($("#selectArea").children('span').text() == text) {
                return false;
            }

            var area = $(this).data('area');
            $("#selectArea").children('span').data('area', area).text(text);

            qEvents.questionSearch();
            return false;
        },
        noStarClick: function () {
            if (!$(this).hasClass("z-sel")) {
                $(this).addClass("z-sel");
            }
            $("#starValue").val(-1);
            $('.q-star').raty('setScore', $("#starValue").val());

            qEvents.questionSearch();

            return false;
        },
        //shareRangeChange: function () {
        //    $(this).addClass('z-sel').siblings().removeClass('z-sel');

        //    var shareValue = $(this).data('value');
        //    $("#shareValue").val(shareValue);

        //    qEvents.questionSearch();

        //    return false;
        //},
        quSortChange: function () {
            $(this).addClass('z-sel').siblings().removeClass('z-sel');

            var sortValue = $(this).data('sort');
            $("#sortValue").val(sortValue);

            qEvents.questionSearch();

            return false;
        },
        qTypeChange: function () {
            $(this).addClass('z-sel').siblings().removeClass('z-sel');

            var qtype = $(this).data('qtype');
            $("#qtypeValue").val(qtype);

            qEvents.questionSearch();

            return false;
        },
        showMoreType: function () {
            var objI = $(this).children('i');
            if (objI.hasClass("g-icon_13")) {
                objI.removeClass("g-icon_13").addClass("g-icon_1");
                $(this).children('span').text('收起');
            } else {
                objI.removeClass("g-icon_1").addClass("g-icon_13");
                $(this).children('span').text('更多');
            }

            $(this).prevAll("a.qtype-more").toggleClass("hide");
        },
        questionChoose: function () {
            var chooseObj = $(this).children('div.g-mark').children('span');

            var type = chooseObj.data("type");
            var qId = chooseObj.data("qid");

            if (chooseObj.hasClass("g-icon-15")) {
                var result = paperBar.add(type, qId);//添加到试卷
                if (result) {
                    chooseObj.removeClass("g-icon-15").addClass("g-icon-14");
                }
            } else {
                paperBar.del(type, qId);//移除试卷
                chooseObj.removeClass("g-icon-14").addClass("g-icon-15");
            }
        },
        qSourceTypeChoose: function () {
            $(this).addClass('z-sel').siblings().removeClass('z-sel');

            var qtype = $(this).data('sourcetype');
            $("#qSourceTypeValue").val(qtype);
            if (qtype == -1 || qtype == 1) {
                $("#selectYear").children('span').data('year', "-1").text('全部年份');
                $("#selectArea").children('span').data('area', "-1").text('全国');
                $(this).siblings('div.g-source').addClass('hide');
            } else {
                $(this).siblings('div.g-source').removeClass('hide');
            }

            qEvents.questionSearch();

            return false;
        },
        questionSearch: function () {
            var star = $("#starValue").val();
            var sort = $("#sortValue").val();
            var qtype = $("#qtypeValue").val();
            //var share = $("#shareValue").val();
            var queryKey = $("#queryKey").val();
            var qSourceType = $("#qSourceTypeValue").val();
            var year = $("#selectYear").children('span').data("year");
            var area = $("#selectArea").children('span').data("area");
            var stage = $("#stage").val();
            var kp = $("#kp").val();

            var hasBook = $("#hasSbook").val() == "1";
            if (hasBook && $(".b-cp").hasClass("active")) {
                kp = $("#txtChapterKps").val();
            }

            $("#questionListDiv").load('/paper/questionlist', {
                star: star,
                sort: sort,
                qtype: qtype,
                kp: kp,
                queryKey: queryKey,
                qSourceType: qSourceType,
                year: year,
                area: area,
                stage: stage
            }, function () {
            });
        },
        gradeChange: function () {
            $(this).addClass('z-sel').siblings().removeClass('z-sel');

            var qtype = $(this).data('value');
            $("#gradeValue").val(qtype);

            qEvents.paperSearch();

            return false;
        },
        sourceChange: function () {
            $(this).addClass('z-sel').siblings().removeClass('z-sel');

            var qtype = $(this).data('value');
            $("#sourceValue").val(qtype);

            qEvents.paperSearch();

            return false;
        },
        paperSearch: function () {
            var grade = $("#gradeValue").val();
            var source = $("#sourceValue").val();
            var key = $("#paperQueryKey").val();
            var stage = $("#stage").val();
            var kp = $("#kp").val();

            //var hasBook = $("#hasSbook").val() == "1";
            //if(hasBook && $(".b-cp").hasClass("active")){
            //    kp = $("#txtChapterKps").val();
            //}

            $("#paperDataDiv").load('/paper/paperdata', {grade: grade, source: source, key: key, kp: kp, stage: stage});

            return false;
        },
        paperDetails: function () {
            var paperId = $(this).data('pid');
            var kp = $("#kp").val();

            $("#choosePaperDiv").addClass("hide");
            $("#paperDetails").load('/paper/showpaperdetail', {paperId: paperId, kp: kp});
        },
        /**
         * 收录全卷
         */
        addAllPaperQuestion: function () {
            var qItems = $("#paperDetails").find("#questionsDiv").children("div.q-item");
            $.each(qItems, function (qIndex, qItem) {
                var chooseObj = $(qItem).children('div.f-qitem').children('div.g-mark').children('span');

                var type = chooseObj.data("type");
                var qId = chooseObj.data("qid");

                var result = paperBar.add(type, qId);//添加到试卷
                if (result) {
                    chooseObj.removeClass("g-icon-15").addClass("g-icon-14");
                }
            });
        }
    };

    //初始化加载题目
    qEvents.questionSearch();
    //初始化地区
    qEvents.areaInit();

    //不限难度分数
    $("#noStar").bind("click", qEvents.noStarClick);
    //分享范围点击事件
    //$("#shareRange").children('a').bind("click", qEvents.shareRangeChange);
    //排序点击事件
    $("#questionSort").children('a').bind("click", qEvents.quSortChange);
    //题型点击事件
    $("#qtypes").children('a').bind("click", qEvents.qTypeChange);
    //来源点击事件
    $("#qSourceType").children('a').bind("click", qEvents.qSourceTypeChoose);
    //搜索点击事件
    $("#btn_Search").bind("click", qEvents.questionSearch);
    //查看更多题型
    $("#showMoreType").bind("click", qEvents.showMoreType);
    //选题界面点击题目(题库+试卷)
    $("#questionListDiv,#paperDetails").delegate(".f-qitem", "click", qEvents.questionChoose);
    //年份选择点击
    $("#selectYear").bind("click", qEvents.selectYearClick);
    //地区选择点击
    $("#selectArea").bind("click", qEvents.selectAreaClick);
    //年份选择
    $("#yearDiv span").bind("click", qEvents.yearChange);
    //地区选择
    $("#areaDiv span").bind("click", qEvents.areaChange);
    //搜索点击
    $("#queryKey").bind("focus", function () {
        $(this).parent('div.g-input').addClass('focus');
    });
    $("#queryKey").bind("blur", function () {
        $(this).parent('div.g-input').removeClass('focus');
    });
    $("#paperQueryKey").bind("focus", function () {
        $(this).parent('div.g-input').addClass('focus');
    });
    $("#paperQueryKey").bind("blur", function () {
        $(this).parent('div.g-input').removeClass('focus');
    });
    //年级点击选择
    $("#gradeChange").children('a').bind("click", qEvents.gradeChange);
    //来源点击选择
    $("#sourceChange").children('a').bind("click", qEvents.sourceChange);
    //试卷搜索
    $("#btn_PaperSearch").bind("click", qEvents.paperSearch);
    //试卷选题列表点击进入试卷详情选题
    $("#paperDataDiv").delegate("table.title-table tr:not(:first)", "click", qEvents.paperDetails);
    //试卷详情页面选题返回列表委托事件
    $("#paperDetails").delegate("#btn_backList", "click", function () {
        $("#choosePaperDiv").removeClass("hide");
        $("#paperDetails").empty();
    });
    //试卷详情页面选题收录全卷
    $("#paperDetails")
        .delegate("#addAllQuestion", "click", qEvents.addAllPaperQuestion);

    //难易程度星级搜索
    $(".q-star").raty({
        path: singer.sites.static + '/plugs/raty/images/',
        score: 0,
        hints: ['容易', '普通', '一般', '困难', '极难'],
        click: function (score, event) {
            $("#starValue").val(score);
            $("#noStar").removeClass("z-sel");

            qEvents.questionSearch();
        }
    });

    $(document).bind('click', function () {
        $("#yearDiv").addClass('hide');
        $("#areaDiv").addClass('hide');
    });

    //回车搜索
    document.onkeydown = function (e) {
        var ev = document.all ? window.event : e;
        if (ev.keyCode == 13) {
            if ($(".topic-tab-content-1").hasClass('show')) {
                $('#btn_Search').click();//处理事件
            } else if (!$("#choosePaperDiv").hasClass("hide")) {
                $("#btn_PaperSearch").click();
            }
        }
    }
});