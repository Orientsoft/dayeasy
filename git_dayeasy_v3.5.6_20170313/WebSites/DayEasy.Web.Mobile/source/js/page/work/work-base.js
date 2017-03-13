(function ($, S){
    var uri        = S.uri(), $headhref = $('#headhref');
    var backHtml   = '';
    $('.d-work-name').html(uri.name);
    /**
     * 跳转-Tab-href
     * @param
     */
    var AnswerHref = "/page/work/answer-paper.html?batch=" + uri.batch + "&paperId=" + uri.paperId + "&name=" + uri.name + "&groupId=" + uri.groupId + "&group=" + uri.group;      //试卷分析
    var AnalysisHref = "/page/work/score-analysis.html?batch=" + uri.batch + "&paperId=" + uri.paperId + "&name=" + uri.name + "&groupId=" + uri.groupId + "&group=" + uri.group;  //成绩分析
    var VariantHref = "/page/work/variant-pass.html?batch=" + uri.batch + "&paperId=" + uri.paperId + "&name=" + uri.name + "&groupId=" + uri.groupId + "&group=" + uri.group;     //相关变式

    $headhref.find('a').eq(0).attr("href", AnswerHref);
    $headhref.find('a').eq(1).attr("href", AnalysisHref);
    $headhref.find('a').eq(2).attr("href", VariantHref);


    backHtml += '<a class="hp-text" href="/page/group/item.html?id=' + uri.groupId + '&name=' + uri.group + '">';
    backHtml += '<div class="hp-left hp-back">';
    backHtml += '<i class="iconfont dy-icon-left"></i>';
    backHtml += '</div>';
    backHtml += '</a>';
    $('.work-back').prepend(backHtml);
    /**
     * nav 浮动
     * @param item
     */
    $(window).scroll(function(){
      var $fix  =$('.head-href-list');
        if($(this).scrollTop()>$('.d-base-header').height()+$(window).height()){
            $fix.addClass('on').css('position', 'fixed');
        }else{
            $fix.removeClass('on').css('position', 'relative');
        }
    });

})(jQuery, SINGER);



