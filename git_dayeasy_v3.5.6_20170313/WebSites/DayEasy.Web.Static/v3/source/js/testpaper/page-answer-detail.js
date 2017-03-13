/**
 * Created by shay on 2016/5/9.
 */
(function ($, S) {
    var batch = $("#batch").val(),
        loadErrors,
        setErrorCount,
        shareAnswers,
        shareDetail,
        worship,
        loadReason,
        shareAnswer;
    /**
     * 加载错题人数
     */
    loadErrors = function () {
        $.get('/work/error-count', {batch: batch}, function (json) {
            if (json && json.status && json.data) {
                for (var attr in json.data) {
                    if (!json.data.hasOwnProperty(attr))
                        continue;
                    setErrorCount(attr, json.data[attr]);
                }
            }
        });
    };
    /**
     * 设置错误人数
     * @param qid
     * @param studentIds
     */
    setErrorCount = function (qid, studentIds) {
        var $error = $('#error_' + qid);
        if ($error.length > 0) {
            var errorCount = ($error.data('count') || 0) + (studentIds.length || 0);
            $error.data('count', errorCount);
            $error.find('em').html(errorCount);
        }
    };
    /**
     * 分享答案详情
     * @param id
     * @param callback
     */
    shareDetail = function (id, callback) {
        $.get('/work/share-detail', {id: id}, function (json) {
            if (!json.status) {
                S.alert(json.message);
                return false;
            }
            if (!json.data) {
                S.alert('没有找到该答案，请稍后重试！');
                return false;
            }
            var data = json.data;
            var $div = $('<div class="d-worship-wrap" style="max-width:650px"/>');
            $div.append('<img src=' + data.img + ' alt="同学答案" style="border:10px solid #f0f0f0;max-width:650px;" />');
            var worshiped = data.hadWorship, $worship;
            if (worshiped) {
                $worship = S.format('<div class="mt10">' +
                    '<a href="javascript:void(0)" class="worshiped">' +
                    '<i class="fa fa-thumbs-o-up"></i> 已膜拜 (<em>{1}</em>)' +
                    '</a></div>', id, data.worshipCount);
            } else {
                $worship = S.format('<div class="mt10">' +
                    '<a href="javascript:void(0)" class="b-worship" data-sid="{0}" data-count="{1}" title="点击膜拜">' +
                    '<i class="fa fa-thumbs-o-up"></i> 膜拜 (<em>{1}</em>)' +
                    '</a></div>', id, data.worshipCount);
            }
            $div.append($worship);
            if (data.worshipName) {
                $div.append('<div class="mt10"><span class="text-info f-names">' + data.worshipName + '</span>&emsp;进行了膜拜</div>');
            }
            var d = singer.dialog({
                title: data.studentName + " 的答案",
                content: $div,
                backdropBackground: '#000',
                backdropOpacity: 0.3
            });
            $div.find("img").bind("load", function () {
                d.showModal();
            });
        });
    };
    /**
     * 膜拜
     * @param id
     * @param callback
     * @returns {boolean}
     */
    worship = function (id, callback) {
        $.post('/work/worship', {id: id}, function (json) {
            if (json.status) {
            callback.call(this, json.message);
        } else {
            singer.msg(json.message);
        }
    });
    };

    /**
     * 分享答案
     * @param qid
     * @param callback
     */
    shareAnswer = function (qid, callback) {
        var paperId = $("#paperId").val();
        var groupId = $("#groupId").val();
        S.confirm("您确定要公开该题的答案？", function () {
            $.post('/work/share-answer', {
                batch: batch,
                paperId: paperId,
                qid: qid,
                groupId: groupId
            }, function (json) {
                if (json.status) {
                    singer.msg("分享成功！");
                    callback.call(this);
                } else {
                    singer.alert(res.message);
                }
            });
        });
    };

    //查看分享的答案
    $(".shareAnswers").delegate(".f-showShareAnswer", "click", function () {
        var $t = $(this);
        shareDetail($t.data('id'));
    });

    //查看同学答案
    $(".f-showAnswer").click(function () {
        var $t = $(this),
            qid = $t.data('qid');
        var shareAnswerDiv = $("#shareAnswers_" + qid);
        if (shareAnswerDiv.length == 0) {
            return false;
        }
        var hadAjax = $t.data("hadAjax");
        if (hadAjax) {
            return false;
        }
        $t.data("hadAjax", true);
        var groupId = $("#groupId").val();
        $.get('/work/share-answers', {qId: qid, groupId: groupId}, function (json) {
            if (json && json.status && json.data && json.data.length > 0) {
                var data = json.data;
                $.each(data, function (indexNo, item) {
                    var span = '<a class="z-crt f-showShareAnswer" href="javascript:void(0);" data-id="' + item.id + '"><span class="hover-color-bule">' + item.name + '</span> ( <i class="iconfont dy-icon-zan"></i><span class="goodcount">' + item.count + '</span> )</a>';
                    shareAnswerDiv.prepend(span);
                });
            } else {
                shareAnswerDiv.prepend('<span>还没有同学进行分享额~</span>');
            }
        });
        return true;
    });

    //分享答案
    $(".f-shareAnswer").click(function () {
        var $t = $(this),
            qid = $t.data('qid');
        shareAnswer(qid, function () {
            $t.remove();
        });
    });
    var flag=true;
    //点击查看我的答案
    $(".selectPicture").click(function () {
        var $t= $(this);
        if($t.data('picture')){
            S.showImage($(this).data('picture'));
            return false;
        }
        var paperid = $(this).attr('paperid');
        var paperType = $(this).attr('paperType');
        var questionid = $(this).attr('questionid');
        var hrefStr = '/StudentWork/InterceptPicture';
        if(flag){
            flag=false;
            $.ajax({
                type: 'GET',
                url: hrefStr,
                data: { 'batch': batch, 'paperid': paperid, 'type': paperType, 'questionid': questionid },
                dataType: 'json',
                success: function (rec) {
                    if (!rec.status) {
                        S.msg(rec.message);
                    } else {
                        if(rec.data!=""){
                            $t.data('picture',rec.data);
                            S.showImage(rec.data);
                        }
                    }
                    flag=true;
                }
            });
        }
    });
    //加入错题库
    $(".f-adderrorquestion").click(function () {
        var $t = $(this),
            qid = $t.data('qid');
        S.confirm("您确定要将该题加入错题库？", function () {
            var paperId = $("#paperId").val();
            $.post('/work/add-error', {
                batch: batch,
                paperId: paperId,
                qid: qid
            }, function (json) {
                if (json.status) {
                    $t.after('<span class="mark-error"><i class="iconfont dy-icon-flag"></i>&nbsp;已标记为错题</span>');
                    setErrorCount(qid, 1);
                    $t.remove();
                    $("#errorAnalysis_" + qid).removeClass('hide');
                    $("#errorAnalysisDiv_" + qid).removeClass('hide');
                } else {
                    S.msg(json.message);
                }
            });
        });
    });

    //加载错因分析
    $(".btn-show-reason").bind("click", function () {
        if ($(this).data("loaded"))
            return;
        var $box = $(this).parents(".cont-list").find(".my-reason");
        var batch = $("#batch").val(),
            qid = $(this).data("qid");

        reason.load({eid: '', batch: batch, qid: qid}, $box,
            function (tags, content) {
                var $tags = $('<div class="q-tags"></div>');
                for (var i = 0; i < tags.length; i++) {
                    $tags.append('<div class="d-tag">' + tags[i] + '</div>');
                }
                $box.find(".text-tags").html($tags);
                $box.find(".text-content").html(content);
            }, false, {show_count: true});
        $(this).data("loaded", 1);
    });
    $('#showBody').bind('change', function () {
        $('.questions-con,.questions-btn,.questions-bottom-cont').toggleClass('hide', !this.checked);
    });
    $(document)
        .delegate('.b-worship', 'click', function () {
            var $t = $(this), id, count, $div;
            if ($t.hasClass('worshiped')) {
                return false;
            }
            id = $t.data('sid');
            count = ~~$t.data('count');
            $div = $t.parents('.d-worship-wrap');
            count = isNaN(count) ? 0 : count;
            worship(id, function (userName) {
                count++;
                var $names = $div.find(".f-names");
                if ($names.length) {
                    $names.html(userName + "," + $names.html());
                } else {
                    $div.append('<div class="mt10"><span class="text-info f-names">' + userName + '</span>&emsp;进行了膜拜</div>');
                }
                $t.addClass("worshiped")
                    .unbind("click")
                    .removeClass("f-worship")
                    .removeAttr('title')
                    .html('<i class="fa fa-thumbs-o-up"></i> 已膜拜 (<em>' + count + '</em>)');
                $('.f-showShareAnswer[data-id="' + id + '"] .goodcount').html(count);
            });
            return false;
        })
    ;
    loadErrors();
})(jQuery, SINGER);