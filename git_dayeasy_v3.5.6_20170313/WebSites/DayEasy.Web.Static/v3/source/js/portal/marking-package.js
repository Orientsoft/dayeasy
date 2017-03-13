/**
 * Created by epc on 2016/4/25.
 */
$(function ($) {
    if ($("#txtFail").val()) {
        $(".dy-container").css("padding", "0");
        return;
    }
    var M = marking,S = singer;
    var batch = S.uri().batch, type = S.uri().type,bagId = S.uri().id;
    if (S.isUndefined(batch) || batch == "" || S.isUndefined(bagId) || bagId == "") {
        $(".dy-container").css("padding", "0");
        $(".m-message").text("参数错误").removeClass("hide");
        return;
    }

    marking.mk.zoom();

    //加载数据
    if (S.isUndefined(type)) type = 1;
    M.data.batch = batch;
    M.data.bagId = bagId;
    M.data.sectionType = type;
    M.data.isJoint = true;
    M.mt.loadData();
});