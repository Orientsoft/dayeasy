$(function () {
    var selector = $("#kPoints").tokenInput($("#getKpUrl").val(), {
        method: "POST",
        queryParam: "kp",
        hintText: "",
        noResultsText: "没有找到相关知识点",
        searchingText: "Searching...",
        placeholder: "输入知识点关键字",
        tokenLimit: 5,
        excludeCurrent: false,
        preventDuplicates: true
    });

    var editorContent = UE.getEditor("qContent", {
        toolbars: [
            ['source', '|', 'Undo', 'Redo', '|', 'bold', 'italic', 'underline', '|', 'insertimage', 'inserttable', 'deletetable', 'superscript', 'subscript', '|', 'kityformula', 'spechars']
        ],
        serverUrl: $("#fileSite").val() + '/uploader',
        initialContent: "",
        autoClearinitialContent: true,//focus时自动清空初始化时的内容
        wordCount: false,//关闭字数统计
        elementPathEnabled: false,//关闭elementPath
        enableAutoSave: false,//关闭自动保存
        enableContextMenu: true,//关闭右键菜单功能
        saveInterval: 5 * 1000 * 1000,
        retainOnlyLabelPasted:true,
        pasteplain: false,
        //fileNumLimit: 3,
        initialFrameHeight: 200//默认的编辑区域高度
    });

    //完成
    $(".f-success").click(function () {
        var qtype = $("#selectedQtype").val();
        var kps = selector.tokenInput("get");
        var qContent = $('<div/>').text(editorContent.getContent()).html();
        var optionNum = $("#optionNum").children('option:selected').val();
        var smallQuNum = parseInt($("#smallQuNum").val());
        if (!qtype) {
            $.Dayez.msg("请先选择题型！");
            return false;
        }
        if (!editorContent.hasContents()) {
            $.Dayez.msg("请输入题目内容！");
            return false;
        }

        var showSmall = ['4', '18'];
        if (isNaN(smallQuNum)) {
            smallQuNum = 0;
        }
        if ($.inArray(qtype, showSmall) > -1 && smallQuNum < 1) {
            $.Dayez.msg("请输入小问数！");
            return false;
        }

        if (!kps || kps.length < 1) {
            $.Dayez.msg("请输入知识点！");
            return false;
        }

        var atype = $(this).data('atype');

        $.post($("#saveQuUrl").val(), { qtype: qtype, kps: JSON.stringify(kps), qContent: qContent, optionNum: optionNum, smallQuNum: smallQuNum }, function (res) {
            if (res.Status) {
                var paperInfo = GetPaperInfo();
                if (paperInfo) {
                    var qitem = {};
                    qitem.QId = res.Data;
                    qitem.Type = qtype;
                    qitem.Score = 0;
                    qitem.Sort = 0;
                    qitem.PaperType = atype;

                    var qlist = [];
                    var chooseQus = [];

                    if (paperInfo.ChooseQus) {
                        chooseQus = paperInfo.ChooseQus;
                        var currentQu = chooseQus[qtype];
                        if (currentQu) {
                            qitem.Sort = currentQu.length + 1;
                            qlist = currentQu;
                        }
                    }

                    qlist.push(qitem);
                    chooseQus[qtype] = qlist;

                    paperInfo.ChooseQus = chooseQus;
                }

                var form = $('<form></form>');
                form.attr('action', window.location.href + "#actionDiv_" + atype);
                form.attr('method', 'post');
                form.attr('target', '_self');
                //paperBaseHidden
                var paperBaseHidden = $('<input type="hidden" name="paperBase" />');
                paperBaseHidden.attr('value', JSON.stringify(paperInfo));
                form.append(paperBaseHidden);
                form.appendTo("body");
                form.submit();
            } else {
                $.Dayez.msg(res.Message);
            }
        });
    });

    $(".f-qtype").click(function () {
        var value = $(this).data('qtype');

        $(this).addClass("z-sel").siblings().removeClass("z-sel");
        $("#f-getmore").removeClass("z-sel").attr("data-qtype", '').children('span').text('更多');
        $("#f-getmore").parents('div.col-sm-12').css("z-index", "100");

        showOptionAndSmallQu(value);
    });

    $("#f-getmore").click(function () {
        if ($(this).next('div.f-more-p').hasClass('hide')) {
            $(this).parents('div.col-sm-12').css("z-index", "2001");
            $(this).next('div.f-more-p').removeClass('hide');
        } else {
            $(this).parents('div.col-sm-12').css("z-index", "100");
            $(this).next('div.f-more-p').addClass('hide');
        }
        return false;
    });

    $("div.f-more-p ul li").click(function () {
        var text = $(this).children('a').text();
        var value = $(this).children('a').data("qtype");

        $(".f-qtype").removeClass("z-sel");
        $("#f-getmore").addClass("z-sel").attr("data-qtype", value).children('span').text(text);

        showOptionAndSmallQu(value);
    });

    $(document).bind('click', function () {
        $("#f-getmore").parents('div.col-sm-12').css("z-index", "100");
        $("#f-getmore").next('div.f-more-p').addClass('hide');
    });
});

var GetPaperInfo = function () {
    var chooseQus = [];
    //当前选择的问题
    var questionContent = $("ul.paper-qContent");
    for (var i = 0; i < questionContent.length; i++) {
        var qtype = $(questionContent[i]).data('qtype');
        var section = $(questionContent[i]).data('section');

        var qlist = [];

        if (chooseQus[qtype]) {
            qlist = chooseQus[qtype];
        }

        var lis = $(questionContent[i]).children("li");
        $.each(lis, function (index, item) {
            var qitem = {};

            qitem.QId = $(item).data('qid');
            qitem.Type = qtype;
            var score = parseFloat($(item).find("input.qScore").val());
            if (isNaN(score)) {
                score = 0;
            }
            qitem.Score = score;
            qitem.Sort = index;
            qitem.PaperType = section;

            qlist.push(qitem);
        });

        chooseQus[qtype] = qlist;
    }

    //每题分数
    var perScores = [];
    var sectionlis = $(".paper-qSection").children('li');
    if (sectionlis) {
        $.each(sectionlis, function (index, item) {
            var qContent = $(item).children('ul.paper-qContent');

            if (qContent) {
                var score = $(item).find('.q-perscore').val();
                if (!score || isNaN(score)) {
                    score = 0;
                }
                var obj = {};
                obj.QSectionType = $(qContent).data('qtype');
                obj.PaperType = $(qContent).data('section');
                obj.PerScore = score;

                perScores.push(obj);
            }
        });
    }

    //试卷基础信息
    var paperBase = {};
    paperBase.Type = $("#type").val();
    paperBase.AddType = $(this).data('atype');
    paperBase.Title = $("#paperTitle").val();
    paperBase.ChooseQus = '';
    paperBase.PerScores = '';
    //paperBase.CompleteUrl = $paperMaker.options.addQuCompleteUrl;
    paperBase.AutoData = $("#autoData").val();//自动出卷的数据

    if (chooseQus.length > 0) {
        paperBase.ChooseQus = chooseQus;
    }
    if (perScores.length > 0) {
        paperBase.PerScores = JSON.stringify(perScores);
    }

    return paperBase;
}

var showOptionAndSmallQu = function (type) {
    var showOption = [1, 2, 3, 4, 18, 24];
    var showSmall = [4, 18];

    $("#optionDiv").addClass("hide").find('select').val(4);
    $("#smallQuDiv").addClass("hide").find('input').val('');

    if ($.inArray(type, showOption) > -1) {//验证选项
        $("#optionDiv").removeClass("hide");

        if ($.inArray(type, showSmall) > -1) {//验证小问
            $("#smallQuDiv").removeClass("hide");
        }
    }

    $("#selectedQtype").val(type);//选择的题型赋值
}

var checkNum = function (obj) {
    obj.value = obj.value.replace(/\D/g, '');
    if (obj.value.length > 2) {
        obj.value = obj.value.substring(0, 2);
    }
}