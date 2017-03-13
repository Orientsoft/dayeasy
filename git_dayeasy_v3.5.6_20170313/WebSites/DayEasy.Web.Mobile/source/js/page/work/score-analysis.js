/**
 * Boz  成绩通知
 * @date    2016-05-27 12:10:41
 */
(function ($, S){
    var uri             = S.uri();
    S.progress.start();
    var bindChengji,
        ChengjiHtml     = '',
        KnowledgeHtml   = '',
        $Chengji        = $('.d-chengji'),
        $knowledge      = $('#knowledge'),
        $daticard       = $('#daticard');  //答题卡
    $.dAjax({
        method : 'paper_info',
        keyword: uri.paperId
    }, function (json){
        /*试卷类型*/
        var paperType = json.data.paperType;
        /*设计稿内容展示*/
        /**
         *  成绩统计
         * @param notice-analysis
         */
        $.dAjax({
            method   : 'work_scoreStatistics',  // 考试作业接口   成绩统计
            batch    : uri.batch,
            paperId  : uri.paperId,
            isEncrypt: false
        }, function (json){
            if(json.status){
                bindChengji(json.data);
            } else {
                /*成绩展示*/
                ChengjiHtml = '<dl class="d-before">'
                    + '<dt>成绩</dt>'
                    + '<dd>-</dd>'
                    + '</dl>'
                    + '<dl class="d-before">'
                    + '<dt>排名</dt>'
                    + '<dd>-</dd>'
                    + '</dl>'
                    + '<dl>'
                    + '<dt>平均分</dt>'
                    + '<dd>-</dd>'
                    + '</dl>'
                $Chengji.append(ChengjiHtml);
            }
        });
        /**
         * 绑定-成绩统计
         * @param item
         */
        bindChengji   = function (data){
            if(paperType == 2){
                /*成绩展示*/
                ChengjiHtml = '<dl class="d-before  before-box-5">'
                    + '<dt>成绩</dt>'
                    + '<dd>' + (data.score==-1?'-':data.score) + '</dd>'
                    + '</dl>'
                    + '<dl class="d-before before-box-5">'
                    + '<dt>A卷</dt>'
                    + '<dd>' + (data.aScore == -1 ? '-' : data.aScore) + '</dd>'
                    + '</dl>'
                    + '<dl class="d-before before-box-5">'
                    + '<dt>B卷</dt>'
                    + '<dd>' + (data.bScore == -1 ? '-' : data.bScore) + '</dd>'
                    + '</dl>'
                    + '<dl class="d-before before-box-5">'
                    + '<dt>排名</dt>'
                    + '<dd>' + (data.rank==-1?'-':data.rank) + '</dd>'
                    + '</dl>'
                    + '<dl class="before-box-5">'
                    + '<dt>平均分</dt>'
                    + '<dd>' + (data.averageScore==-1?'-':data.averageScore) + '</dd>'
                    + '</dl>';
            } else {
                /*成绩展示*/
                ChengjiHtml = '<dl class="d-before">'
                    + '<dt>成绩</dt>'
                    + '<dd>' + (data.score==-1?'-':data.score) + '</dd>'
                    + '</dl>'
                    + '<dl class="d-before">'
                    + '<dt>排名</dt>'
                    + '<dd>' + (data.rank==-1?'-':data.rank) + '</dd>'
                    + '</dl>'
                    + '<dl>'
                    + '<dt>平均分</dt>'
                    + '<dd>' + (data.averageScore==-1?'-':data.averageScore) + '</dd>'
                    + '</dl>';
            }
            $Chengji.append(ChengjiHtml);
            /*薄弱知识点*/
            if(data.kpAnalysis.length !== 0){
                $('.d-boruo-knowledge').removeClass('hide');
            }
            KnowledgeHtml += '<tr>'
                + '<th width="76%">知识点</th>'
                + '<th width="24%">错题数</th>'
                + '</tr>';
            for (var i = 0; i < data.kpAnalysis.length; i++) {
                KnowledgeHtml += '<tr>'
                    + '<th width="76%">' + data.kpAnalysis[i].kpName + '</th>'
                    + '<th width="24%">' + data.kpAnalysis[i].errorCount + '</th>'
                    + '</tr>';
            }
            $knowledge.append(KnowledgeHtml);
        };
        /*绑定分数段*/
        var fenshuduan, fenshuduanHtml = '', $fenduanappend = $('#fenshuduan');
        /**
         *  获取成绩分数段
         * @param
         */
        $.dAjax({
            method : 'work_scoreSections',
            batch  : uri.batch,
            paperId: uri.paperId
        }, function (json){
            if(json.status){
                fenshuduan(json.data);
            } else {
                $fenduanappend.append('<tr><td colspan="2"><div class="dyui-nothing">老师未录入成绩</div></td></tr>');
            }
        });
        fenshuduan = function (data){
            fenshuduanHtml = '<tr>'
                + '<th width="50%">分数段</th>'
                + '<th width="50%">人数</th>'
                + '</tr>';
            for (var i = 0; i < data.length; i++) {
                for (var j = data[i].scoreGroupes.length - 1; j >= 0; j--) {
                    fenshuduanHtml += '<tr>'
                        + '<td>' + data[i].scoreGroupes[j].scoreInfo + '</td>'
                        + '<td>' + data[i].scoreGroupes[j].count + '</td>'
                        + '</tr>';
                }
            }
            $fenduanappend.append(fenshuduanHtml);
        };
        /*答题卡   A  B*/
        var _paperType = json.data.paperType;
        $.dAjax({
            method : 'work_answerSheet',
            batch  : uri.batch,
            paperId: uri.paperId
        }, function (json){

            console.log(json.data);

            if(json.data == 0){return false;}
            $('.d-dati-ka').removeClass('hide');
            var daticardHtml = '';
            var AddHtml      = function (type, abclass, ab){
                daticardHtml += '<a class="a01" href="/page/work/answer-sheet.html?' + 'batch=' + uri.batch + '&paperId=' + uri.paperId + '&type=' + type + '"><i class="iconfont' + ' ' + abclass + '"></i>' + ab + '答题卡</a>'
            };
            //试卷类型1： 普通卷   2：AB 推送卷
            if(_paperType == 1){
                AddHtml(0, 'dy-icon-c', '');
            }
            if(_paperType == 2){
                for (var i = 0; i < json.data.length; i++) {
                    var abType = json.data[i].type;
                    if(abType == 1){
                        daticardHtml += '<a class="a01" href="/page/work/answer-sheet.html?batch=' + uri.batch + '&paperId=' + uri.paperId + '&type=' + 1 + '"><i class="iconfont dy-icon-a"></i>A卷答题卡</a>';
                    }
                }
                for (var i = 0; i < json.data.length; i++) {
                    var abType = json.data[i].type;
                    if(abType == 2){
                        daticardHtml += '<a class="a01" href="/page/work/answer-sheet.html?batch=' + uri.batch + '&paperId=' + uri.paperId + '&type=' + 2 + '"><i class="iconfont dy-icon-b"></i>B卷答题卡</a>';
                    }
                }
            }

            $daticard.append(daticardHtml);
        });


        S.progress.done();
    });


})(jQuery, SINGER);