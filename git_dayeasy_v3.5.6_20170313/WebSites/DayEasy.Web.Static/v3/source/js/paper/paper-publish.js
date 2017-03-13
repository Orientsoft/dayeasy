/**
 * Created by shay on 2016/11/10.
 */
(function ($, S) {
    var post = function (url, data, callback) {
        $.ajax({
            url: url,
            type: 'Post',
            data: data,
            xhrFields: {
                withCredentials: true
            },
            //dataType: 'application/json; charset=utf-8',
            success: function (html) {
                callback.call(this, html);
            }
        });
    };
    var getPapers = function (pageIndex) {
        var paperId = $("#paperId").val();
        post(singer.sites.apps + '/paper/publishpapers', {pageIndex: pageIndex, paperId: paperId}, function (res) {
            $("#paperList").empty();
            if (res.Status && res.Data) {
                $.each(res.Data, function (index, item) {
                    var radioItem = '<label class="checkbox-group group-radio"><input type="radio" name="options" class="paperItem" value="' + item.PaperId + '"><span>' + item.PaperName + '<em>' + item.Time + '</em></span><i class="iconfont dy-icon-radio"></i></label>';

                    $("#paperList").append(radioItem);
                });

                if (res.TotalCount <= 4) {
                    if (res.TotalCount === 1) {
                        $("#paperList .group-radio input").click();
                    }
                    $("#pageButton").addClass('hide');
                } else {
                    $("#pageButton").removeClass('hide');
                }
                $("#pageButton").attr("data-totalcount", res.TotalCount);
            } else {
                $("#paperList").append('<label class="checkbox-group group-radio">没有找到要发布的试卷！</label>');
            }
        });
    };

    getPapers(1);//获取发布试卷

    $("#pageButton a").click(function () {
        var currentPage = parseInt($("#pageIndex").val());
        if (isNaN(currentPage)) {
            currentPage = 1;
        }

        var newPage = currentPage;
        var index = $(this).index();
        if (index === 0) {//上一页
            newPage = newPage - 1;
        } else {//下一页
            newPage = newPage + 1;
        }

        if (newPage < 1) {
            newPage = 1;
        }

        var totalCount = $("#pageButton").data("totalcount");
        var totalPage = Math.ceil(totalCount / 4);//总页数
        if (newPage > totalPage) {
            newPage = totalPage;
        }

        if (newPage == currentPage) {
            return false;
        }

        $("#pageIndex").val(newPage);

        getPapers(newPage);
    });

    $("#btn_sure").click(function () {
        var $t = $(this),
            pid = $(".paperItem:checked").val(),
            sendMsg = $("#otherMsg").val(),
            groupIds = [];
        $t.blur();

        var groupList = $(".groupItem:checked");
        $.each(groupList, function (index, item) {
            groupIds.push($(item).val());
        });

        if (!$.trim(pid)) {
            $("#errorMsg").text("请先选择要推送的试卷！");
            window.setTimeout(function () {
                $("#errorMsg").text("");
            }, 2000);

            return false;
        }

        if (groupIds.length < 1) {
            $("#errorMsg").text("请先选择要推送的圈子！");
            window.setTimeout(function () {
                $("#errorMsg").text("");
            }, 2000);

            return false;
        }

        var sourceGId = $("#groupId").val();
        $t.disableField('提交中...');
        post(singer.sites.apps + '/paper/pubpaper', {
            pId: pid,
            sendMsg: sendMsg,
            groupIds: JSON.stringify(groupIds),
            sourceGId: sourceGId
        }, function (res) {
            if (res.Status) {
                var d = singer.dialog.getCurrent();
                d.close().remove();
                singer.msg(res.Data);
            } else {
                $("#errorMsg").text(res.Message);
                window.setTimeout(function () {
                    $("#errorMsg").text("");
                }, 2000);
            }
            $t.undisableFieldset();
        });

        return false;
    });
})(jQuery, SINGER);