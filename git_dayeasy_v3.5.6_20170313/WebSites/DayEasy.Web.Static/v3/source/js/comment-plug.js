/**
 * 评论
 * Created by epc on 2015/12/02.
 */
 var comment = (function () {
    var C = {
        info: {
            hide: false
        },
        box: 0,
        addCall: false, //评论回调
        deleteCall: false, //删除回调
        templateLoaded: false,
        /**
         * 解析数据并渲染模版
         * @param data
         */
        load: function(data){
            C.info.uid = data.uid || 0;
            C.info.name = data.name || '';
            C.info.icon = data.icon || '';
            C.info.placeholder = data.placeholder || '我有话说';
            C.info.hasDelete = data.hasDelete || false;
            C.addCall = data.addCall || false;
            C.box = data.box || $(".reason-comment");
            for(var i=0;i<data.comments.length;i++){
                var comment = data.comments[i];
                comment.autor = comment.uid;
                comment.uid = C.info.uid;
                comment.hasDelete = C.info.hasDelete;
                if(comment.comments && comment.comments.length){
                    for(var j=0;j<comment.comments.length;j++){
                        var reply = comment.comments[j];
                        reply.autor = reply.uid;
                        reply.uid = C.info.uid;
                    }
                }
            }
            if (!C.templateLoaded) {
                singer.loadTemplate('comment-plug', function () {
                    C.templateLoaded = true;
                    C.box.html(singer.render('comment-list', data));
                });
            } else {
                C.box.html(singer.render('comment-list', data));
            }

            C.init(); //初始化页面事件
        },
        /**
         * 初始化界面事件
         */
        init: function(){
            //点评
            C.box.delegate(".o-edit","click",function(){
                $(this).parent().siblings(".cm-replys").find(".cm-add-box").eq(0).toggleClass("hide");
            });
            //删除
            C.box.delegate(".o-delete","click",function(){
                var sid = $(this).data("sid");
                var $p = $(this).parents(".cm-item")[0];
                if (C.deleteCall && singer.isFunction(C.deleteCall)) {
                    C.deleteCall.call(this,sid,function(){
                        $p.remove();
                    });
                }else{
                    singer.msg("未能加载提交函数");
                    return;
                }
            });
            //评论
            C.box.delegate(".cm-btn","click",function(){
                var $txt = $(this).parent().find(".cm-txt");
                var $box = $txt.parents(".comment-box");
                C.trim($txt);
                var sid = $txt.data("sid"),
                    uid = $txt.data("uid"),
                    uname = $txt.data("uname"),
                    content = $txt.val();
                if(!content.length){
                    singer.msg("写点什么，再评论吧~");
                    return;
                }
                var $rps = $(this).parents(".cm-replys");
                var $cms = $rps.length > 1
                    ? $rps.eq(1).find(".cm-list")
                    : $(this).parents(".cm-item").siblings(".cm-list");

                if (C.addCall && singer.isFunction(C.addCall)) {
                    C.addCall.call(this,$box,sid,uid,uname,content, function(){
                        var $item = $txt.parents(".cm-item").eq(0);
                        $txt.val("");
                        $item.find("em").text("140");
                        if($item.data("hide") == "1"){
                            $item.addClass("hide");
                        }
                        var html = singer.render("comment-sigle",{
                            icon: C.info.icon,
                            name: C.info.name,
                            sname: (sid && sid.length && uname && uname.length ? ' <span style="color:#999;font-size:12px">回复</span> ' + uname : ''),
                            sid: '99999999',
                            hasDelete: C.info.hasDelete,
                            time: '刚刚',
                            content: content
                        });
                        $cms.html(html+$cms.html());
                    });
                }else{
                    singer.msg("未能加载提交函数");
                    return;
                }
            });
            //文本域监视
            C.box.delegate(".cm-txt","keyup",function(){
                $(this).siblings(".cm-txt-alt").find("em").text(140 - $(this).val().length);
            }).delegate(".cm-txt","change",function(){
                C.trim($(this));
            });
        },
        /**
         * 去除文本域前后空格
         * @param $txt
         */
        trim: function($txt){
            $txt.val($txt.val().replace(/^\s+/,'').replace(/\s+$/,''));
            $txt.siblings(".cm-txt-alt").find("em").text(140 - $txt.val().length);
        }
    };
    return C;
})();

$(function(){
    /**
     * 加载评论输入
     */
    template.helper('commentAdd', function (show,sid,uid,uname) {
        var tmp = comment.info;
        tmp.hide = show == 0;
        tmp.uid = uid || 0;
        tmp.sid = sid || '';
        tmp.uname = uname || '';
        if(tmp.uname.length) tmp.placeholder = '回复 '+uname;
        return singer.render('comment-add',tmp);
    });
});