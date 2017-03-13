var ebDetail = (function () {
    var D = {};
    return D;
})();

$(function ($) {
    //参数
    ebDetail.error_id = $("#txtErrorId").val();
    ebDetail.batch = $("#txtBatch").val();
    ebDetail.paper_id = $("#txtPaperId").val();
    ebDetail.question_id = $("#txtQuestionId").val();
    ebDetail.class_id = $("#txtClassId").val();
    ebDetail.user_id = $("#txtUserId").val();
    ebDetail.is_objective = $("#txtObjectiveQuestion").val() == "1";
    ebDetail.is_ebook = $("#txtIsEBook").val() == "1";
    ebDetail.is_teacher = $("#txtIsTeacher").val() == "1";

    //解析并展示公式
    setTimeout(singer.loadFormula, 120);

    //错因标签
    if(!ebDetail.is_teacher){
        reason.load(ebDetail.error_id, $(".my-reason"), false, false,{show_count:false});
    }

    //评论
    var commentTemplate = $("#commentTemplate").html();
    function initComments(box,template,data,id){
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            var headPic = item.head;
            if (singer.isUndefined(headPic) || !headPic || !headPic.length) {
                headPic = deyi.sites.static + "/image/default/user_s50x50.jpg";
            }
            var newId = item.id;
            if(id && id.length) newId = id;
            var detailsStr = "";
            if(item.details && item.details.length){
                var $details = $('<div class="comments-detail" data-id="1" data-pid="'+newId+'">');
                initComments($details,template,item.details,item.id);
                if(item.details.length < item.detail_count){
                    $details.append('<div class="d-more">更多 &raquo;</div>');
                }
                detailsStr = $details.prop("outerHTML");
            }
            box.append(template
                .replace("{head_pic}", headPic)
                .replace("{content}", item.content)
                .replace("{time}", item.time)
                .replace("{details}",detailsStr)
                .replace("last-child",(i != data.length - 1 ? "" : "last-child"))
                .replace("{user_name}", (item.parent_name && item.parent_name.length
                    ? (item.user_name + " <span style='color:#999999;'>回复</span> " + item.parent_name)
                    : item.user_name))
                .replace("{call}",(item.user_id != ebDetail.user_id
                    ? '<a class="a-call" data-id="'+ newId +'" data-uid="'+item.user_id+'" data-uname="'+item.user_name +'" href="#" title="回复TA"><i class="fa fa-pencil-square-o"></i></a>'
                    : '')));
        }
    }
    function loadComments($comment, def) {
        var id = $comment.find(".txt-comment-id").val().trim();
        var idx = 1;
        if (!def) {
            idx = parseInt($comment.data("idx") || 1);
        }
        $.post("/reason/comments", {id: id, index: idx}, function (json) {
            $comment.find(".f-more").html('更多 &raquo;');
            var box = $comment.find(".other-comments");
            if (json.status) {
                if (json.data && json.data.length) {
                    if (def) box.html("");
                    $comment.data("idx", (idx + 1));
                    initComments(box,commentTemplate,json.data);
                    $comment.parent().find(".a-comment-power").text("评论(" + json.count + ")");
                    if (json.count < idx * 10) {
                        $comment.find(".f-more").hide();
                    } else {
                        $comment.find(".f-more").show();
                    }
                } else {
                    if (def) box.html("暂无评论");
                }
            } else {
                if (def) box.html(json.message);
            }
        });
    }
    //更多回复
    $(".comment-bottom").delegate(".d-more","click",function(){
        var $box = $(this).parents(".comments-detail");
        var id = $(this).parents(".comment-bottom").find(".txt-comment-id").val().trim();
        var pid = $box.data("pid");
        var idx = ($box.data("idx") || 1) + 1;
        if(id == "" || pid == "" || idx < 2){
            singer.msg("参数错误，请刷新重试");
            return;
        }
        $(this).html('<i class="fa fa-spin fa-spinner fa-1x"></i>&nbsp;&nbsp;正在加载，请稍后...');
        $.post("/reason/commentDetails", {id: id, pid: pid, index: idx}, function (json) {
            $box.find(".d-more").remove();
            if(json.status){
                for(var i=0;i<json.data.length;i++){
                    var item = json.data[i];
                    var headPic = item.head;
                    if (singer.isUndefined(headPic) || !headPic || !headPic.length) {
                        headPic = deyi.sites.static + "/image/default/user_s50x50.jpg";
                    }
                    $box.append(commentTemplate
                        .replace("{head_pic}", headPic)
                        .replace("{content}", item.content)
                        .replace("{time}", item.time)
                        .replace("{details}","")
                        .replace("last-child",(i != json.data.length - 1 ? "" : "last-child"))
                        .replace("{user_name}", (item.user_name + " <span style='color:#999999;'>回复</span> " + item.parent_name))
                        .replace("{call}",(item.user_id != ebDetail.user_id
                            ? '<a class="a-call" data-id="'+ pid +'" data-uid="'+item.user_id+'" data-uname="'+item.user_name +'" href="#" title="回复TA"><i class="fa fa-pencil-square-o"></i></a>'
                            : '')));
                }
                if(idx * 10 < json.count){
                    $box.data("idx",idx);
                    $box.append('<div class="d-more">更多 &raquo;</div>');
                }
            }else{
                singer.msg(json.message);
            }
        });
    });
    //展开缩起评论列表
    $(".a-comment-power").bind("click", function () {
        var $comment = $(this).parents(".ed-comment-list").find(".comment-bottom");
        $comment.toggleClass("hide");
        if ($(this).data("load") == "0") {
            $(this).data("load", 1);
            loadComments($comment, true);
        }
    });
    //加载更多
    $(".f-more").bind("click", function () {
        $(this).html('<i class="fa fa-spin fa-spinner fa-1x"></i>&nbsp;&nbsp;正在加载，请稍后...');
        loadComments($(this).parents(".comment-bottom"), false);
    });
    //发表评论
    function addComment($comment) {
        var id = $comment.find(".txt-comment-id").val().trim();
        var $txt = $comment.find(".txt-comment-content");
        var content = $txt.val().trim();
        var uid = $txt.data("uid"),
            uname = $txt.data("uname");
        if (id == "") {
            singer.msg("参数错误，请刷新重试");
            return;
        }
        if (content == "") {
            singer.msg("请填写评论内容");
            return;
        }
        $.post("/reason/AddComment", {id: id, content: content, pid: "",uid: uid, uname: uname}, function (json) {
            if (json.status) {
                $comment.find(".txt-comment-content").val("");
                $(".f-subs").text('140');
                loadComments($comment, true);
            } else {
                singer.msg(json.message);
            }
        });
    }
    $(".btn-addcomment").bind("click", function () {
        addComment($(this).parents(".comment-bottom"));
    });
    //回复他人评论
    $(".ed-comment").delegate(".a-call","click",function(){
        if($(this).data("open") == "1"){
            $(this).data("open","0");
            $(this).parent().find(".comment-cc").remove();
            return false;
        }
        $(this).data("open","1");
        var $box = $('<div class="comment-cc text-right">');
        var $txt = $('<textarea class="txt-cc-content" cols="30" rows="2" maxlength="140" placeholder="回复 '+$(this).data("uname")+'"></textarea>')
        $txt.data("id",$(this).data("id"));
        $txt.data("uname",$(this).data("uname"));
        $txt.data("uid",$(this).data("uid"));
        $box.append($txt);
        var $btn = $('<button type="button" style="width: 60px; height: 22px; padding: 0;" class="btn btn-primary btn-addcomment">回复</button>');
        $btn.bind("click",function(){
            var $comment = $(this).parents(".comment-bottom");
            var id = $comment.find(".txt-comment-id").val().trim(),
                content = $txt.val().trim(),
                pid = $txt.data("id"),
                uid = $txt.data("uid"),
                uname = $txt.data("uname");
            if (id == "" || pid == "" || uname == "") {
                singer.msg("参数错误，请刷新重试");
                return false;
            }
            if(content == ""){
                singer.msg("请输入回复内容");
                return false;
            }
            $.post("/reason/AddComment", {id: id, content: content, pid: pid, uid: uid, uname: uname}, function (json) {
                if (json.status) {
                    $box.remove();
                    $box.parents(".list-time").data("open","0");
                    loadComments($comment, true);
                } else {
                    singer.msg(json.message);
                }
            });
        });
        $box.append($btn);
        $(this).parent().append($box);
        return false;
    });
    //评论输入框监视
    $(".txt-comment-content").bind("keyup", function () {
        var num = 140 - $(this).val().length;
        if(num < 0) num = 0;
        $(this).parent().find(".f-subs").text(num);
    });
    //删除分析及评论
    $(".a-comment-delete").bind("click", function () {
        var id = $(this).data("id");
        singer.confirm("确定删除分析及评论吗？", function () {
            $.post("/reason/delete", {id: id}, function (json) {
                if (json.status) {
                    singer.msg("删除成功", 2000, function () {
                        window.location.reload();
                    });
                } else {
                    singer.msg(json.message);
                }
            });
        });
    });
    //选项卡切换
    $('.tab-menu li').bind('click', function () {
        $(this).addClass('z-crt').siblings().removeClass('z-crt');
        $('.tab-contenr').find('.tab-con').eq($(this).index()).addClass('show').siblings().removeClass('show');
        if ($(this).index() == 1) {
            if (singer.isUndefined($(".tab-con-2").data("load")) || $(".tab-con-2").data("load") != "1") {
                $(".tab-con-2").data("load", "1");
                //我的答案
                if(!ebDetail.is_teacher){
                    $.post("/errorBook/answer",
                        {
                            batch: ebDetail.batch,
                            paperId: ebDetail.paper_id,
                            questionId: ebDetail.question_id,
                            source_type: ebDetail.is_ebook ? 1 : 0
                        },function(json){
                            var $box = $(".eb-my-answer");
                            if(json.status && json.is_print){
                                //来源打印
                                var $q = $('<div class="q-item">');
                                $q.attr("style", "margin-bottom:10px;");
                                if (json.body && json.body.length) {
                                    $q.append('<div style="margin:5px 0;">' + json.body + '</div>');
                                }
                                if (json.img && josn.img.length) {
                                    $q.append('<div class="q-image"><img alt="我的答案" src="' + json.img + '" style="border:5px solid #f0f0f0;" /></div>');
                                }
                                var $a = $('<div style="margin:5px 0;">');
                                var link = '/work/marking-detail?batch='+ebDetail.batch+'&paperId='+ebDetail.paper_id+'&studentId='+ebDetail.user_id + (json.isb ? '&type=b' : '');
                                $a.append('<a target="_blank" href="'+link+'" style="color:#65cafc;">查看我的答卷</a>');
                                $q.append($a);
                                $box.append($q);
                            }else if(json.status && json.data && json.data.length){
                                //来源移动端
                                for(var i=0;i<json.data.length;i++){
                                    var item = json.data[i];
                                    var $a = $('<div class="q-item">');
                                    $a.attr("style","margin-bottom:10px;");
                                    if(item.body && item.body.length){
                                        $a.append('<div>'+item.body+'</div>');
                                    }
                                    if(item.images && item.images.length){
                                        var $imgs = $('<div>');
                                        $imgs.attr("style","margin:5px 0").addClass("q-image");
                                        for(var j=0;j<item.images.length;j++){
                                            $imgs.append('<div title="查看大图"><img src="'+item.images[j]+'" /></div>');
                                        }
                                        $a.append($imgs);
                                    }
                                    if(!item.body && (!item.images || !item.images.length)){
                                        $a.append('<div class="q-not-answer"><strong>未作答</strong></div>');
                                    }
                                    $box.append($a);
                                }
                            }else{
                                var link = '/work/marking-detail?batch='+ebDetail.batch+'&paperId='+ebDetail.paper_id+'&studentId='+ebDetail.user_id + (json.isb ? '&type=b' : '');
                                $box.append('<a target="_blank" href="'+link+'" style="color:#65cafc;">点击答卷详细</a>');
                            }
                        });
                }
                //同学分享的答案
                if (!ebDetail.is_objective && !ebDetail.is_ebook) {
                    $(".students-answer").show();
                    loadAnswerShare(false);
                }
            }
        }
    });
    //加载同学分享的答案
    function loadAnswerShare(all) {
        $.post("/reason/Shares", {questionId: ebDetail.question_id, classId: ebDetail.class_id, all: all}, function (json) {
            var $box = $(".tab-con-2").find(".students-answer");
            if (json.status) {
                if (json.data && json.data.length) {
                    var dds = $('<dl class="answer-dl after">');
                    dds.append("<dt>同学答案：</dt>")
                    for (var i = 0; i < json.data.length; i++) {
                        dds.append('<dd data-val="' + json.data[i].id + '">' + json.data[i].name + '</dd>');
                    }
                    if (json.count > json.data.length) {
                        dds.append('<dd class="f-fr" data-val="more">更多…</dd>');
                    }
                    $box.html("").append(dds);
                } else {
                    $box.html("暂无同学分享答案");
                }
            } else {
                $box.html("同学答案加载失败");
            }
        });
    }
    //查看同学答案详细
    $(".students-answer").delegate("dd", "click", function () {
        var v = $(this).data("val");
        if (v == "more") {
            loadAnswerShare(true);
        } else {
            $.post("/reason/Detail", {id: v}, function (json) {
                if (json.status) {
                    var $img = $('<img class="answer-img" src="'+json.data.img+'" alt="同学答案"/>');
                    var $thumb = $('<div class="answer-thumb mt5"><i class="fa fa-thumbs-o-up"></i>膜拜(<span>' + json.data.count + '</span>)</div>');
                    if (json.data.hadworship) {
                        $thumb.attr("style", "cursor:default;color:#ffa64c;");
                    } else {
                        $thumb.data("id", v);
                        $thumb.bind("click", function () {
                            var id = $(this).data("id");
                            $.post("/reason/Worship", {id: id}, function (res) {
                                if (res.status) {
                                    $(".answer-thumb").unbind("click");
                                    var count = parseInt($(".answer-thumb").find("span").text()) + 1;
                                    $(".answer-thumb").find("span").text(count);
                                    $(".answer-thumb").attr("style", "cursor:default;color:#ffa64c;");
                                    if (count > 1) {
                                        $(".answer-bottom").find("span").text("我," + $(".answer-bottom").find("span").text());
                                    }
                                } else {
                                    singer.msg(res.message);
                                }
                            });
                        });
                    }
                    var $box = $('<div class="answer-share-box">');
                    $box.append($img)
                        .append($thumb);
                    if (json.data.count > 0) {
                        $box.append('<div class="answer-bottom mt5"><span>'
                            + json.data.name + '</span> '
                            + (json.data.count > 5 ? ' 等' + json.data.count + '人' : '')
                            + '进行了膜拜</div>');
                    }
                    var _title = json.data.studentname + " 的答案分享";
                    var dialog = singer.dialog({
                        title: _title,
                        content: $box,
                        fixed: true,
                        backdropOpacity: .7
                    });
                    $img.bind("load",function(){
                        dialog.showModal();
                    });
                } else {
                    singer.msg(json.message);
                }
            });

        }
    });
    //显隐变式训练答案
    $(".span-variant-answer-p").bind("click", function () {
        $(".variant-answer").toggleClass("hide");
    });

    //错误率图表
    var gaugeOptions = {
        chart: {
            type: 'solidgauge'
        },
        title: null,
        pane: {
            background: {
                backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || '#ccc',
                innerRadius: '80%',
                outerRadius: '100%',
                shape: 'arc'
            }
        },
        tooltip: {
            enabled: false
        },
        yAxis: {
            stops: [[1, '#fc9797']],
            lineWidth: 0,
            minorTickInterval: null,
            tickPixelInterval: 400,
            tickWidth: 0,
            title: {y: 110},
            labels: {y: 0}
        },
        plotOptions: {
            solidgauge: {
                dataLabels: {
                    y: 5,
                    borderWidth: 0,
                    useHTML: true
                }
            }
        }
    };
    function initHigchart(rateC,countC,rateY,countY){
        //班级错误率
        $('#container-survey-class').highcharts(Highcharts.merge(gaugeOptions, {
            yAxis: {
                min: 0,
                max: 100,
                labels: {
                    enabled: false
                },
                title: {
                    text: '<span style="color:#fc9797;font-size:16px">班级 错</span><span style="color:#65cafc;font-size:16px">'+countC+'</span><span style="color:#fc9797;font-size:16px">人</span>'
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                name: '班级错误率',
                data: [rateC],
                innerRadius: '80%',
                dataLabels: {
                    format: '<div style="text-align:center;font-size:20px;color:#fc9797">{y}%</div>',
                    y: -20
                }
            }]
        }));
        //年级错误率
        $('#container-survey-year').highcharts(Highcharts.merge(gaugeOptions, {
            yAxis: {
                min: 0,
                max: 100,
                labels: {
                    enabled: false
                },
                title: {
                    text: '<span style="color:#fc9797;font-size:16px">年级 错</span><span style="color:#65cafc;font-size:16px">'+countY+'</span><span style="color:#fc9797;font-size:16px">人</span>'
                }
            },
            credits: {
                enabled: false
            },
            series: [{
                name: '年级错误率',
                data: [rateY],
                innerRadius: '80%',
                dataLabels: {
                    format: '<div style="text-align:center;font-size:20px;color:#fc9797">{y}%</div>',
                    y: -20
                }
            }]
        }));
    }
    if(ebDetail.is_ebook){
        $.post("/reason/eBookRates",{
            chapterId: ebDetail.paper_id,
            classId: ebDetail.class_id,
            questionId: ebDetail.question_id
        },function(json){
            initHigchart(parseFloat(json.rate_c),json.count_c,parseFloat(json.rate_y),json.count_y);
        });
    }else{
        $.post("/reason/rates",{
            batch: ebDetail.batch,
            paperId: ebDetail.paper_id,
            questionId: ebDetail.question_id
        },
        function(json){
            initHigchart(parseFloat(json.rate_c),json.count_c,parseFloat(json.rate_y),json.count_y);
        });
    }
});
