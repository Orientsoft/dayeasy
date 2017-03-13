/**
 * Created by epc on 2016/4/25.
 */

$(function ($) {
    if ($("#txtFail").val()) {
        $(".dy-container").css("padding", "0");
        return;
    }
    var M = marking, S = singer;
    var batch = S.uri().batch, type = S.uri().type;
    if (S.isUndefined(batch) || batch == "") {
        $(".dy-container").css("padding", "0");
        $(".m-message").text("参数错误").removeClass("hide");
        return;
    }

    //定义页面特有事件
    M.mk.initMix = function () {
        //得分板显隐
        if (!M.data.allObjective) {
            $(".power").bind("click", function () {
                var $this = $(this);
                var isHide = $this.data("show") == "1";
                if (isHide) {
                    $this.data("show", "0").attr("title", "点击展开");
                    $this.find("i").attr("class", "fa fa-chevron-right");
                } else {
                    $this.data("show", "1").attr("title", "点击收起");
                    $this.find("i").attr("class", "fa fa-chevron-down");
                    $this.find(".qc-line").show();
                }
                $(".items").slideToggle(800, function () {
                    if (isHide) $this.find(".qc-line").hide();
                });
            });
        }
    };
    M.mt.bindDataMix = function () {
        if (M.data.isJoint) {
            //首次进入阅卷
            if (!(M.pictures && M.pictures.length) && M.questions && M.questions.length) {
                var msg = "";
                for (var i = 0; i < M.questions.length; i++) {
                    msg += ((i == 0 ? "" : ",") + M.questions[i].sort);
                }
                singer.alert("你需要批阅的题目【" + msg + "】");
            }
        }
    };

    marking.mk.zoom();

    //加载数据
    if (S.isUndefined(type)) type = 1;
    M.data.batch = batch;
    M.data.sectionType = type;
    M.data.isJoint = $("#txtIsJoint").val() == "1";
    M.mt.loadData();

});
