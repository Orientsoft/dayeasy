var dialogObj;
var voteData = {}; //投票数据

//添加投票
$(function () {
    var editorContent = UE.getEditor("qContent", {
        toolbars: [
            ['source', '|', 'Undo', 'Redo', '|', 'bold', 'italic', 'underline', '|',
                'forecolor', 'backcolor', 'insertorderedlist', 'insertunorderedlist', 'selectall', 'cleardoc', '|',
                'fontfamily', 'fontsize', '|',
                'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', '|',
                'imagenone', 'imageleft', 'imageright', 'imagecenter', '|',
                'link', 'unlink', 'anchor', '|',
                'emotion', 'insertimage', 'inserttable', 'deletetable', 'superscript', 'subscript',
                '|', 'kityformula', 'spechars', 'fullscreen']
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
        initialFrameHeight: 200 //默认的编辑区域高度
    });

    var tags = singer.tags({
        container: $(".d-tags"),
        data: $(".d-tags").data("tags"),
        canEdit: true,
        type: 2,
        max: 3,
        change: function (data) {
            //console.log(data);
        }
    });

    //添加投票
    $("#btn_addVote").click(function () {
        var $addVote = getVoteHtml();

        dialogObj = dialog({
            title: "添加投票",
            content: $addVote,
            backdropBackground: '#000',
            backdropOpacity: 0.3
        }).showModal();
    });

    //发布
    $("#btn_pub").click(function () {
        var title = $("#topicTitle").val();
        if (!$.trim(title)) {
            singer.msg("请输入帖子标题！");
            $("#topicTitle").focus();
            return false;
        }

		if(title.length > 250){
            singer.msg("帖子标题最多250个字！");
			$("#topicTitle").focus();
            return false;
		}
		
        var content = $('<div/>').text(editorContent.getContent()).html();
        if (!editorContent.hasContents()) {
            singer.msg("请输入帖子内容！");
            return false;
        }

        var groupId = $("#groupId").val();
        var tag = tags.get();

        var topic = {};
        topic.Title = title;
        topic.Content = content;
        topic.Tags = tag;
        topic.GroupId = groupId;
        topic.PubVote = voteData;

        $.post($("#saveUrl").val(), {topic: JSON.stringify(topic)}, function (res) {
            if (res.Status) {
                window.location = $("#redirectUrl").val();
            } else {
                singer.msg(res.Message);
            }
        });

        return false;
    });
});

//投票模板
var getVoteHtml = function () {
    var $addVote = $($("#addVoteTemp").html());
    $addVote.find("#addOption").bind("click", function () {
        var count = $("#optionLis").children("li").length + 1;
        $("#optionLis").append('<li><input type="text" placeholder="选项' + count + '" /><i class="iconfont dy-icon-close f-remove"></i></li>');
    });
    $addVote.delegate(".f-remove", "click", function () {
        var $this = $(this);
        singer.confirm("您确定要移除该选项？", function () {
            $this.parent('li').remove();

            $.each($("#optionLis").children('li'), function (index, item) {
                $(item).find('input').prop("placeholder", "选项" + (index + 1));
            });
        }, function () {
        });
    });

    $addVote.find("#btn_cancel").bind("click", function () {
        dialogObj.close().remove();
    });
    $addVote.find("#btn_sure").bind("click", function () {
        var title = $("#voteTitle").val();
        if (!$.trim(title)) {
            singer.msg("请输入标题！");
            $("#voteTitle").focus();
            return false;
        }

        var optionLis = $("#optionLis").children('li');
        if (optionLis.length < 1) {
            singer.msg("请先添加选项！");
            return false;
        }

        var options = [];
        $.each(optionLis, function (index, item) {
            var value = $(item).find('input').val();
            if (!$.trim(value)) {
                singer.msg("选项内容不能为空！");
                $(item).find('input').focus();
                options = [];
                return false;
            }

            var option = {};
            option.Sort = index;
            option.OpContent = value;

            options.push(option);
        });

        if (options.length < 1) {
            return false;
        }

        var isSingle = $("input[name='chooseType']:checked").val();
        var finishTime = $("#finishTime").val();
        var isPublic = $("input[name='secret']:checked").val();

        voteData = {};
        voteData.Title = title;
        voteData.IsSingle = isSingle;
        voteData.IsPublic = isPublic;
        voteData.FinishedAt = finishTime;
        voteData.VoteOptions = options;

        addVoteSuccess(); //添加成功，显示详情
        dialogObj.close().remove(); //关闭对话框

        return false;
    });

    return $addVote;
}

//添加投票成功
var addVoteSuccess = function () {
    $("#btn_addVote").addClass('hide'); //隐藏添加按钮

    var $voteDetail = $($("#voteDetailTemp").html());
    $voteDetail.find("#deleteVote").bind("click", function () {
        singer.confirm("您确定要删除该投票吗？", function () {
            $("#voteDetail").empty(); //情况投票
            $("#btn_addVote").removeClass('hide'); //放开添加按钮
            voteData = {}; //清空投票数据
        }, function () {
        });
    });
    $voteDetail.find("#editVote").bind("click", editVote);

    $voteDetail.find("#title").text("【投票】" + voteData.Title);

    var voteOptionsObj = $voteDetail.find("#voteOptions");
    $.each(voteData.VoteOptions, function (index, item) {
        var color = index == 0 ? "#fc6e51" : (index == 1 ? "#ffce54" : "#ccd1d9");

        voteOptionsObj.append('<li><label>' + item.OpContent + '</label><em style="background: ' + color + ';"></em> 0</li>');
    });

    $("#voteDetail").html($voteDetail);
}

//编辑投票
var editVote = function () {
    var $editVote = getVoteHtml();

    $editVote.find("#voteTitle").val(voteData.Title);
    $editVote.find("#finishTime").val(voteData.FinishedAt);
    $editVote.find("#optionLis").empty();
    $.each(voteData.VoteOptions, function (index, item) {
        $editVote.find("#optionLis").append('<li><input type="text" value="' + item.OpContent + '" /><i class="iconfont dy-icon-close f-remove"></i></li>');
    });

    dialogObj = dialog({
        title: "编辑投票",
        content: $editVote,
        backdropBackground: '#000',
        backdropOpacity: 0.3,
        onshow: function () {
            if (voteData.IsSingle == "false") {
                $editVote.find("input[name='chooseType']:eq(1)").click();
            }
            if (voteData.IsPublic == "false") {
                $editVote.find("input[name='secret']:eq(1)").click();
            }
        }
    }).showModal();
}