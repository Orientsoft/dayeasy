(function ($) {
    $.fn.AutoPaperMaker = function (options) {

        var pMaker = new paperMaker(this, options);
        pMaker.create();

        return $(this);
    };

    //知识点
    var points = singer.points({
        container: $("#points").find(".kpPoint"),
        data: '',
        max: 15,
        isAppendData: false,
        init: function () {
            return {
                stage: $("#stage").val(),
                subject_id: $("#subjectId").val()
            }
        }, change: function (json) {
            if (json) {
                var text = $("#highSet").text();
                if ($.trim(text) == "高级配置") {
                    $("#normalKps").removeClass('hide');
                    $("#setKps").addClass('hide');
                } else {
                    $("#normalKps").addClass('hide');
                    $("#setKps").removeClass('hide');
                }

                if (json.length < 1) {
                    $("#setKps").addClass('hide');
                } else {
                    $("#leader").removeClass('hide');
                }

                $("#selKps").empty();

                var liObj = '<li data-value="{kpValue}"><span class="f-knleg"><em>{kpName}</em><i class="f-close">x</i></span></li>';

                $.each(json, function (index, item) {
                    var li = liObj.replaceAll("{kpName}", item.name).replaceAll("{kpValue}", item.id);
                    $("#selKps").append(li);
                });

                if ($("#f-pips-count").length > 0) {
                    appendSetOptions(json);//添加设置项
                }
            }
        }
    });

    //试卷对象封装
    var $buildPaper = $("#buildPaper"),
        $rebuildPaper = $("#rebuildPaper"),
        $paperNav = $("#paperNav"),
        $paperDetailDiv = $("#paperDetailDiv"),
        $pipsCount = $("#f-pips-count"),
        $paperMaker;

    var paperMaker = function (obj, options) {
        this.$element = $(obj);
        this.defaults = {
            autoGetQuestionUrl: ''//自动获取题目的url
        };

        this.options = $.extend({}, this.defaults, options);
    };

    paperMaker.prototype = {
        create: function () {
            $paperMaker = this;

            //知识点删除委托
            $("#selKps,#f-pips-count").delegate("i.f-close", "click", function () {
                var data = points.get().data;

                var $li = $(this).parents('li');
                var indexNum = $li.index();

                $("#selKps").children('li').eq(indexNum).remove();//移除知识点

                //移除高级配置
                var setKps = $("#f-pips-count");
                if (setKps.length > 0) {
                    setKps.children('li').eq(indexNum).remove();
                }

                //重置知识点树
                var updateKp = [];
                $.each(data, function (index, item) {
                    if (index != indexNum) {
                        updateKp.push(item);
                    }
                });

                if (updateKp.length < 1) {
                    $("#setKps").addClass('hide');
                }

                points.set(updateKp);
            });
            //自动出卷生成试卷
            $buildPaper.bind("click", this.buildPaperClick);
            //重新生成试卷
            $rebuildPaper.bind("click", this.buildPaperClick);
            //点击基本信息导航事件
            $paperNav.find("ul li").bind("click", this.navStepClick);
            //包含题型选择事件
            $("#qTypes,#qTypesA,#qTypesB").find("ul li span").bind("click", this.questionTypeChoose);
        },
        navStepClick: function () {
            var index = $(this).index();
            if (index == 0) {
                if (!$(this).children('a').hasClass("crt")) {
                    $.Dayez.confirm("您确定要重置出卷条件吗？重置后当前数据将丢失。", function () {
                        window.location = window.location.href;
                    }, function () { });
                }
            } else {
                return false;
            }
            return false;
        },
        buildPaperClick: function () {
            var autoPaperData = $("#autoData").val();

            if (!autoPaperData) {
                if (!validateForm()) {
                    return false;
                }
            }

            $paperDetailDiv.empty();//先清空试卷内容

            //获取自动出卷数据
            if (!autoPaperData) {
                autoPaperData = JSON.stringify($paperMaker.getAutoPaperData());
            }

            //自动找题
            var form = $('<form></form>');
            form.attr('action', $paperMaker.options.autoGetQuestionUrl);
            form.attr('method', 'post');
            form.attr('target', '_self');
            //autoDataHidden
            var autoDataHidden = $('<input type="hidden" name="autoData" />');
            autoDataHidden.attr('value', autoPaperData);
            form.append(autoDataHidden);
            form.appendTo("body");
            form.submit();

            return false;
        },
        getAutoPaperData: function () {
            //获取知识点
            var kpData = [];
            var lis = $pipsCount.find("li");
            $.each(lis, function (index, item) {
                var kp = {};
                kp.Name = $(item).data('value');// $(item).find('span em').text();
                kp.Count = parseInt($(item).children('div').attr('data-value'));

                kpData.push(kp);
            });

            //获取题型与数量
            var qtypeData = [];
            if ($("#qTypes").length > 0) {//常规卷
                var qTypes = $("#qTypes").find('ul li.sel');
                $.each(qTypes, function (index, item) {
                    var qtype = {};
                    qtype.Type = $(item).data('value');
                    qtype.Name = $(item).children('span').text();
                    var count = parseInt($(item).children('.qtype-num').val());
                    qtype.Count = isNaN(count) ? 0 : count;
                    qtype.PaperSectionType = "A";

                    qtypeData.push(qtype);
                });
            } else {//AB卷
                var qTypesA = $("#qTypesA").find('ul li.sel');
                $.each(qTypesA, function (index, item) {
                    var qtype = {};
                    qtype.Type = $(item).data('value');
                    qtype.Name = $(item).children('span').text();
                    var count = parseInt($(item).find('.qtype-num').val());
                    qtype.Count = isNaN(count) ? 0 : count;
                    qtype.PaperSectionType = "A";

                    qtypeData.push(qtype);
                });

                var qTypesB = $("#qTypesB").find('ul li.sel');
                $.each(qTypesB, function (index, item) {
                    var qtype = {};
                    qtype.Type = $(item).data('value');
                    qtype.Name = $(item).children('span').text();
                    var count = parseInt($(item).find('.qtype-num').val());
                    qtype.Count = isNaN(count) ? 0 : count;
                    qtype.PaperSectionType = "B";

                    qtypeData.push(qtype);
                });
            }

            //试卷难度
            var paperDiffic = $("#paperLevel").children("li.sel").data('value');

            return {
                Kps: kpData,
                Qtypes: qtypeData,
                Diffic: paperDiffic
            };
        },
        questionTypeChoose: function () {
            var $this = $(this).parent('li');
            $this.toggleClass("sel");

            if ($this.hasClass("sel")) { //选中
                $this.find(".qtype-num").focus();
            } else { //未选中
                $this.find(".qtype-num").val('');
            }

            var ulobj = $this.parents("ul");
            var nums = ulobj.find('.qtype-num');
            var totalCount = 0;

            $.each(nums, function (index, item) {
                var num = parseInt($(item).val());
                if (isNaN(num)) {
                    num = 0;
                }
                totalCount += num;
            });
            ulobj.parent("div").prev("h3").find("em span").text(totalCount);
        }
    };

    //验证试卷基本信息
    var validateForm = function () {
        var kps = points.get().data;
        if (!kps || kps.length < 1) {
            $.Dayez.msg("请选择知识点！");
            return false;
        }
        if ($("#qTypes").length > 0) {
            var qtypes = $("#qTypes").find("ul li.sel");
            if (qtypes.length < 1) {
                $.Dayez.msg("请选择所含题型！");
                return false;
            }
        } else {//AB 卷验证
            var qTypesA = $("#qTypesA").find("ul li.sel");
            var qTypesB = $("#qTypesB").find("ul li.sel");
            if (qTypesA.length < 1) {
                $.Dayez.msg("请选择 A卷 所含题型！");
                return false;
            }
            if (qTypesB.length < 1) {
                $.Dayez.msg("请选择 B卷 所含题型！");
                return false;
            }
        }

        var totalNum = 0;
        var totalObj = $("#paperInfoDiv").find("span.q-totalnum");
        $.each(totalObj, function (index, item) {
            var num = parseInt($(item).text());
            if (isNaN(num)) {
                num = 0;
            }
            totalNum += num;
        });

        if (totalNum > 100 || totalNum < 1) {
            $.Dayez.msg("试卷题目总数介于1到100之间！");
            return false;
        }

        return true;
    };

    var appendSetOptions = function (json) {
        $("#f-pips-count").empty();//先清空

        if (json) {
            $.each(json, function (index, item) {
                appendOneKp(item.name, item.id);
            });
        }
    }

    var appendOneKp = function (kpName, kpId) {
        var startNum = 25;

        var liObj = '<li data-value="{kpValue}"><span class="f-knleg"><em>{kpName}</em><i class="f-close">x</i></span><div class="f-fr pips-coun" data-value="' + startNum + '"></div></li>';

        liObj = liObj.replaceAll("{kpName}", kpName).replaceAll("{kpValue}", kpId);

        var $li = $(liObj);

        //滑动控件
        $li.find(".pips-coun").noUiSlider({
            range: {
                min: 1,
                max: 50
            },
            start: startNum,
            step: 1
        });
        $li.find(".pips-coun").on({
            slide: function () {
                $(this).attr("data-value", $(this).val());
            }
        });

        $("#f-pips-count").append($li);
    }
})(jQuery);


$(function () {
    var updateQNum = function (obj) {
        updateQTotalCount(obj);

        var totalNum = 0;
        var totalObj = $("#paperInfoDiv").find("span.q-totalnum");
        $.each(totalObj, function (index, item) {
            var num = parseInt($(item).text());
            if (isNaN(num)) {
                num = 0;
            }
            totalNum += num;
        });

        if (totalNum > 100 || totalNum < 0) {
            $.Dayez.msg("试卷题目总数介于1到100之间！");
            $(obj).val('').focus();
            updateQTotalCount(obj);
        }
    }

    var updateQTotalCount = function (obj) {
        var ulobj = $(obj).parents("ul");

        var nums = ulobj.find('.qtype-num');
        var totalCount = 0;

        $.each(nums, function (index, item) {
            var num = parseInt($(item).val());
            if (isNaN(num)) {
                num = 0;
            }

            totalCount += num;
        });
        ulobj.parent("div").prev("h3").find("em span").text(totalCount);
    }


    //高级配置点击
    $("#highSet").click(function () {
        var text = $(this).text();
        if ($.trim(text) == "高级配置") {
            text = "收起配置";
            $("#normalKps").addClass('hide');
            $("#setKps").removeClass('hide');
        } else {
            text = "高级配置";
            $("#normalKps").removeClass('hide');
            $("#setKps").addClass('hide');
        }
        $(this).text(text);

        if ($("#f-pips-count").children('li').length < 1) {
            $("#setKps").addClass('hide');
            $("#leader").addClass('hide');
        } else {
            $("#leader").removeClass('hide');
        }
    });

    //试卷难度点击
    $("#paperLevel li").click(function () {
        $(this).addClass("sel").siblings().removeClass("sel");
    });

    //数量输入委托
    $(document).delegate(".qtype-num", "keyup", function () {
        var $t = $(this);
        if (/(\d{1,2})/.test($t.val())) {
            $t.val(RegExp.$1);
        } else {
            $t.val("");
        }
        updateQNum(this);
    }).delegate(".qtype-num", "change", function () {
        var $t = $(this);
        if (/(\d{1,2})/.test($t.val())) {
            $t.val(RegExp.$1);
        } else {
            $t.val("");
        }
        updateQNum(this);
    });
});


