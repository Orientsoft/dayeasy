var tags, selector, solveContent, stage = 1;

$(function () {
    $("#description").keyup(function () {
        var value = $(this).val();
        if (value.length > 80) {
            $(this).val(value.substr(0, 80));
        }
        var length = $(this).val().length;
        $(this).next('p.f-tar').text('还可输入' + (80 - length) + '个字');
    });


    //图片上传处理
    $("#uploadImg").click(function () {
        $(".webuploader-element-invisible").click();
    });
    singer.uploader.on("uploadSuccess", function (file, response) {
        if (response.state) {
            $("#tutorPhotoUrl").val(response.urls[0]);
            $("#tutorPhoto").attr("src", singer.makeThumb(response.urls[0], 300, 300));
        }
        singer.uploader.reset();
    });

    //标签
    tags = singer.tags({
        container: $(".d-tags"),
        data: $(".d-tags").data("tags"),
        canEdit: true,
        max: 5,
        change: function (data) {
            //console.log(data);
        }
    });

    //知识点
    selector = $("#kPoints").tokenInput('/sys/knowledges?stage=' + stage, {
        method: "POST",
        queryParam: "keyword",
        hintText: "",
        noResultsText: "没有找到相关知识点",
        searchingText: "Searching...",
        placeholder: "输入知识点关键字",
        tokenLimit: 5,
        excludeCurrent: false,
        preventDuplicates: true,
        prePopulate: $("#kPoints").data('kps')
    });

    $('#grade').bind('change', function () {
        var g = $(this).val();
        var t = g < 7 ? 1 : (g > 9 ? 3 : 2);
        if (t == stage) return;
        stage = t;
        $("#kPoints").data('settings').url = '/sys/knowledges?stage=' + stage;
        selector.tokenInput("clear");
    });

    //知识点特征与常见解法
    solveContent = UE.getEditor("solveContent", editorOptions);

    //删除委托操作
    $('#tutorContentDiv').delegate('.f-close', "click", function () {
        var $this = this;
        $.Dayez.confirm('您确定要删除该项吗？', function () {
            $($this).parent('div').parent('div').fadeOut(500, function () {
                this.remove();
                updateSortNo();
            });
        }, function () {
        });
    });

    //渲染编辑器
    var textAreas = $("#tutorContentDiv").find('textarea');
    $.each(textAreas, function (index, item) {
        var id = $(this).attr('id');
        UE.getEditor(id, editorOptions);
    });

    //添加文本操作
    $("#addText").click(function () {
        var contentTemp = $("#contentTemp").html();

        var sortNo = contentCount() + 1;//当前有的内容的数量

        contentTemp = contentTemp.replace('{{contentType}}', contentType.Text).replace('{{sortNo}}', sortNo);

        var editorRandom = new Date().getTime();
        var addTextTemp = $("#addTextTemp").html().replace('{{No}}', editorRandom);
        contentTemp = contentTemp.replace('{{contentDetail}}', addTextTemp);

        $("#tutorContentDiv").append(contentTemp);

        UE.getEditor("text_" + editorRandom, editorOptions);
    });

    //选择题目
    $("#chooseQuestion,#addNewQuestion,#uploadVideo").click(function () {
        var requestionUrl = $(this).data('url');

        var paperBase = {};
        //paperBase.ChooseQus = [];
        paperBase.CompleteUrl = $("#chooseQuCompleteUrl").val();
        paperBase.AutoData = JSON.stringify(getCurrentData());//自动出卷的数据

        var form = $('<form></form>');
        form.attr('action', requestionUrl);
        form.attr('method', 'post');
        form.attr('target', '_self');
        //paperBaseHidden
        var paperBaseHidden = $('<input type="hidden" name="paperBase" />');
        paperBaseHidden.attr('value', JSON.stringify(paperBase));
        form.append(paperBaseHidden);
        form.appendTo("body");
        form.submit();
    });

    //保持草稿操作  and  保存操作
    $("#btnSaveDraft,#btnSave").click(function () {
        var tutorData = getCurrentData();
        if (!$.trim(tutorData.Title)) {
            $.Dayez.msg("请输入标题！");
            return false;
        }
        if (!$.trim(tutorData.Author)) {
            $.Dayez.msg("请输入作者！");
            return false;
        }
        if (!$.trim(tutorData.Kps)) {
            $.Dayez.msg("请选择知识点！");
            return false;
        }
        if (!$.trim(tutorData.SolveContent)) {
            $.Dayez.msg("请输入知识点特征与常见解法！");
            return false;
        }
        if (tutorData.Contents.length < 1) {
            $.Dayez.msg("请添加辅导内容！");
            return false;
        }

        var id = $(this).attr('id');
        var draft = $(this).data('draft');
        var confirmMsg = id == "btnSave" ? "您确定要保存该辅导吗？" : "您确定要保存草稿吗？";

        $.Dayez.confirm(confirmMsg, function () {
            $.post($("#saveTutor").val(), {tutorData: JSON.stringify(tutorData), draft: draft}, function (res) {
                if (res.Status) {
                    window.location = $("#tutorListUrl").val();
                } else {
                    $.Dayez.msg(res.Message);
                }
            });
        }, function () {
        });
    });
});


//获取当前的所有数据
var getCurrentData = function () {
    var tutorData = {};

    tutorData.EditId = $("#editId").val();
    tutorData.Profile = $("#tutorPhotoUrl").val();
    tutorData.Title = $("#tutorTitle").val();
    tutorData.Diff = $("#diffLevel").children("option:selected").val();
    tutorData.Author = $("#author").val();
    tutorData.Grade = $("#grade").children("option:selected").val();
    tutorData.Kps = selector.tokenInput("get");
    tutorData.Tags = tags.get();
    tutorData.Subject = $("#subject").children("option:selected").val();
    tutorData.Description = $("#description").val();
    tutorData.SolveContent = $('<div/>').text(solveContent.getContent()).html();
    tutorData.Contents = [];

    var contentObjs = $("#tutorContentDiv").children('div');
    if (contentObjs) {
        $.each(contentObjs, function (index, item) {
            var content = {};
            content.Sort = index;
            content.Type = $(item).data('type');
            content.Remarks = $(item).find('input.add-notes').val();

            if (content.Type == contentType.Text) {//文本
                var textareaId = $(item).find('textarea').prev('div').attr('id');
                content.Detail = $('<div/>').text(UE.getEditor(textareaId).getContent()).html();
            } else {//视频或者问题
                content.Detail = $(item).data('sourceid');
            }
            tutorData.Contents.push(content);
        });
    }

    return tutorData;
}

//内容类型
var contentType = {
    Text: 0,
    Video: 1,
    Question: 2
}

//编辑器配置
var editorOptions = {
    toolbars: [
        ['source', '|', 'Undo', 'Redo', '|', 'bold', 'italic', 'underline', '|', 'insertimage', 'inserttable', 'deletetable', 'superscript', 'subscript', '|', 'kityformula', 'spechars',
            'link']
    ],
    serverUrl: $("#fileSite").val() + '/uploader',
    initialContent: "",
    autoClearinitialContent: false, //focus时自动清空初始化时的内容
    wordCount: false, //关闭字数统计
    elementPathEnabled: false, //关闭elementPath
    enableAutoSave: false, //关闭自动保存
    enableContextMenu: true, //关闭右键菜单功能
    saveInterval: 5 * 1000 * 1000,
    retainOnlyLabelPasted: true,
    pasteplain: false,
    //fileNumLimit: 3,
    initialFrameHeight: 100 //默认的编辑区域高度
};

var contentCount = function () {
    return $("#tutorContentDiv").children('div').length;
}

var updateSortNo = function () {
    var contentObjs = $("#tutorContentDiv").children('div');
    if (contentObjs) {
        $.each(contentObjs, function (index, item) {
            $(item).find('span.titile-list').text(index + 1);
        });
    }
}