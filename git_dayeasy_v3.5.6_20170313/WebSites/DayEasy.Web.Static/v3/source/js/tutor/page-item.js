/**
 * Created by shay on 2016/5/17.
 */
(function ($, S) {
    //解析并展示公式
    /*"v3/js/portal/question/formula.js",*/
    S.later(S.loadFormula, 120);

    //参考答案
    var commended = $("#txtCommentEd").val() == "1";
    $('.f-click-slideToggle').next().hide();
    $('.f-click-slideToggle').children('span').click(function () {
        $(this).find('i.icon-test-center-right').toggleClass('icon-test-center-down');
        $(this).parents('.f-click-slideToggle').next().slideToggle(400);
    });
    $('.f-checkbox').on('click', function (event) {
        if (commended) return;
        event.preventDefault();
        if ($(this).hasClass('icon-checkbox')) {
            $(this).removeClass('icon-checkbox');
            $(this).find("input").removeAttr("checked");
        } else {
            $(this).addClass('icon-checkbox');
            $(this).find("input").attr("checked", "checked");
        }
    });

    //播放视频
    $(".deyi-player").each(function (i, box) {
        var id = $(box).attr("id");
        var url = $(box).data("url");
        singer.play({
            id: id,
            p: 0,
            url: url,
            height: 300
        });
    });

    //评价
    $("#btnComment").bind("click", function () {
        if (commended) return;
        var $obj = $(this);
        var id = $("#txtCommentId").val(),
            comment = $("#txtComment").val(),
            type = 0;
        if (!id || !id.length) {
            singer.msg("参数错误，请刷新重试");
            return;
        }

        $("input[name='cbxCommentType'][checked]").each(function (i, item) {
            type |= parseInt($(item).val());
        });

        if (type == 0 && !comment && !comment.length) {
            singer.msg("请选择评价内容或填写其他建议");
            return;
        }

        $obj.attr("disabled", "disabled").addClass("disabled").text("处理中...");
        $.post("/work/add-tutor-comment", {
            id: id,
            comment: comment,
            type: type
        }, function (json) {
            if (json.Status) {
                commended = true;
                var html = $("#tacklingTemplate").html();
                $(".tickling-box").html(html);
            } else {
                $obj.removeAttr("disabled").removeClass("disabled").text("完成");
                singer.msg(json.message);
            }
        });
    });
})(jQuery, SINGER);

//v2.0》 v.30兼容
$(document).find('.dy-body').removeClass();