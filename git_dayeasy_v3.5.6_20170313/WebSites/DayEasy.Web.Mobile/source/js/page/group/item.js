/**
 * Boz
 * @date    2016-05-27 12:10:41
 */
/**
 * 圈子主页
 * Created by shay on 2016/5/24.
 */
(function ($, S){
    var uri         = S.uri(), loadDynamics, checkMember, logger = S.getLogger('group-item');
    S.progress.start();
    var mctTop      = 0;
    var
        pageItem    = 0,
        pageNumber  = 15,
        $ulList     = $('.group-list').find('.group-list-ul'),
        TtitleWidth = $ulList.width() - 72,
        bindItem;


    $('.d-group-name').html(uri.name);
    document.title  = uri.name + ' - ' + document.title;
    if(!uri.id || uri.id.length != 32){
        location.href = '/page/group/home.html';
        return false;
    }
    /*过滤html*/
    var removeHTMLTag = function (str){
        str = str.replace(/<\/?[^>]*>/g, ''); //去除HTML tag
        str = str.replace(/[ | ]*\n/g, '\n'); //去除行尾空白
        //str = str.replace(/\n[\s| | ]*\r/g,'\n'); //去除多余空行
        str = str.replace(/ /ig, '');//去掉
        return str;
    }
    /*返回圈子*/
    /**
     * 检查圈子成员
     */
    checkMember  = function (callback){
        $.dAjax({method: 'group_isMember', id: uri.id}, function (json){
            if(!json.status || json.data != 0){
                S.msg('你还不是该圈子的成员！', 1200, function (){
                    location.href = '/page/group/home.html';
                });
                return false;
            }
            callback && callback.call(this);
        });
    };
    /**
     * 加载动态
     */
    loadDynamics = function (type, page, size){
        $.dAjax({
            method : 'group_dynamics',
            groupId: uri.id,
            type   : type,
            page   : page,
            size   : size
        }, function (json){
            logger.info(json);
            if(!json.status || json.data.totalCount == 0){
                /*logger.debug('莫有动态');*/
                $('.dy-load-bata').show();
                $(window).unbind("scroll");
                S.progress.done();
                return false;
            }
            /*logger.debug('有' + json.data.totalCount + '条动态');*/
            S.progress.start();
            bindItem(json.data);
        });
    };
    checkMember(function (){
        loadDynamics(-1, pageItem, pageNumber);
    });
    /**
     * 绑定圈子详细列表
     * @param
     */
    bindItem     = function (items){
        for (var i = 0; i < items.newsDetails.length; i++) {
            var item    = items.newsDetails[i],
                itemHtml;
            var user    = items.users[item.userId] || 0;
            var _header = user ? user.avatar : "/image/default/user_s160x160.png",
                _name   = user ? user.name : "";
            itemHtml    = '<li data-type="' + item.dynamicType + '" data-batch="' + item.batch + '" data-paperid="' + item.paperId + '">';
            itemHtml += '<div class="am-g">';
            itemHtml += '<div class="f-cb">';
            itemHtml += '<div class="box-sm-6 am-text-left user-img">';
            itemHtml += '<img src="' + _header + '" width="20" height="20" alt="' + _name + '"/>';
            itemHtml += '<span>' + _name + '</span>';
            itemHtml += '</div>';
            itemHtml += '<div class="box-sm-6 am-text-right d-time">' + item.time + '</div>';
            itemHtml += '</div>';
            itemHtml += '<div class="box-sm-12 f-cb">';
            /* 不同类型挂钩
             * task-display         作业
             * notice-display       通知
             * praise-display       表扬
             * achievement-display  成绩通知
             * */
            /*作业*/
            if(item.dynamicType == 0){
                itemHtml += '<strong class="task-display base-display">作业</strong>';
                itemHtml += '<a class="group-list-title" style="width:' + TtitleWidth + 'px"  data-title="' + item.title + '" href="/page/work/answer-paper.html?batch=' + item.batch + "&paperId=" + item.paperId + "&name=" + item.title + "&groupId=" + uri.id + "&group=" + uri.name + '">' + (item.title ? item.title : '') + '</a>';
            }
            /*考试*/
            if(item.dynamicType == 1){
                itemHtml += '<strong class="examination-display base-display">考试</strong>';
                itemHtml += '<a class="group-list-title" style="width:' + TtitleWidth + 'px" data-title="' + item.title + '" href="/page/work/answer-paper.html?batch=' + item.batch + "&paperId=" + item.paperId + "&name=" + item.title + "&groupId=" + uri.id + "&group=" + uri.name + '">' + (item.title ? item.title : '') + '</a>';
            }
            /*通知*/
            if(item.dynamicType == 2){
                itemHtml += '<strong class="notice-display base-display">通知</strong>';
                itemHtml += '<a class="group-list-title item-pop" style="width:' + TtitleWidth + 'px"  href="javascript:void(0);">' + removeHTMLTag((item.message ? item.message : '')) + '</a>';
                itemHtml += '<div class="hide item-pop-html">' + (item.message ? item.message : '') + '</div>'
            }
            /*表扬*/
            if(item.dynamicType == 4){
                itemHtml += '<strong class="praise-display base-display">表扬</strong>';
                itemHtml += '<a class="group-list-title item-pop" style="width:' + TtitleWidth + 'px"  href="javascript:void(0);">' + removeHTMLTag((item.message ? item.message : '')) + '</a>';
                itemHtml += '<div class="hide item-pop-html">' + (item.message ? item.message : '') + '</div>'
            }
            /*成绩通知*/
            if(item.dynamicType == 6){
                itemHtml += '<strong class="achievement-display base-display">成绩通知</strong>';
                itemHtml += '<a class="group-list-title" style="width:' + TtitleWidth + 'px" data-title="' + item.title + '" href="/page/group/notice-analysis.html?examId=' + item.examId + "&id=" + item.id + "&name=" + item.title + '">' + (item.title ? item.title : '') + '</a>';
            }
            itemHtml += '</div>';
            itemHtml += '</div>';
            itemHtml += '</li>';
            $ulList.append(itemHtml);
        }
        $('.dy-loading').hide();
        S.progress.done();
    };
    /**
     * 滚动条添加-更多
     * @param item
     */
    $(window).scroll(function (){
        var scrollTop    = $(this).scrollTop();
        var scrollHeight = $(document).height();
        var windowHeight = $(this).height();
        if(scrollTop + windowHeight == scrollHeight){
            pageItem += 1;
            loadDynamics(-1, pageItem, pageNumber);
        }
    });
    /*弹框数据-Pop*/
    $('body').on("click", '.item-pop', handler1).on("click", '.pop-back', handler2);
    function handler1(){
        mctTop         = $(document).scrollTop()
        var $this      = $(this);
        var $Oli       = $this.parents('li');
        var _textTitle = $Oli.find('.base-display').text();
        var _textCont  = $Oli.find('.item-pop-html').html();
        $('.wrap-group').hide();
        var $wrapPop   = $('.wrap-pop');
        $wrapPop.show();
        $wrapPop.find('.d-pop-name').text(_textTitle);
        $wrapPop.find('.information-contents').html(_textCont);
    };
    function handler2(){
        event.preventDefault();
        $(document).scrollTop(mctTop);
        var $this = $(this), $wrapPop = $this.parents('.wrap-pop');
        $wrapPop.hide();
        $('.wrap-group').show();
    };
    // 链接跳转
    $ulList.on('click', 'li', function (event){
        event.preventDefault();
        var href             = $(this).find('.group-list-title').attr("href");
        window.location.href = href;
    });
    /**
     * nav 浮动
     * @param item
     */
    $(window).scroll(function (){
        var $fix         = $('#f-scroll');
        var windowHeight = $(window).height() * 2;
        if($(this).scrollTop() > windowHeight){
            $fix.addClass('on').css('position', 'fixed');
        } else {
            $fix.removeClass('on').css('position', 'relative');
        }
    });

    //窗口改变浏览器  .group-list-title  标题宽度
    $(window).resize(function (){
        var $title  = $ulList.find('.group-list-title');
        TtitleWidth = $ulList.width() - 52;
        $title.css('width', TtitleWidth);
    });


})(jQuery, SINGER);
