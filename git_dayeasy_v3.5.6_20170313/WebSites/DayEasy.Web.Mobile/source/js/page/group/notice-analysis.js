/**
 * Boz
 * @date    2016-05-27 12:10:41
 */
(function ($, S){
    var uri                = S.uri();
    S.progress.start();
    /**
     *  总分排名
     * @param notice-analysis
     * 考试作业接口-成绩汇总  work_eaSummary
     */
    $.dAjax({
        method: 'work_eaSummary',
        id    : uri.examId  //
    }, function (json){
        bindNoticeAnalysis(json.data);
    });
    /**
     * 绑定总分排名
     * @param item
     */
    var bindNoticeAnalysis = function (data){
        var MissExam      = function (obj){
            return obj == -1 ? '缺考' : obj;
        };
        var MisssNo       = function (obj){
            return obj == -1 ? '-' : obj;
        };

        $('.d-notice-analysis-name').html(data.examinationTitle);
        var HeadTitleHtml = '<div class="am-fl td-left">' +
            ' <p>我的总分</p>' +
            ' <b class="am-text-truncate">' + MissExam(data.totalScore) + '</b> ' +
            '</div>' +
            ' <div class="am-fr td-right">' +
            ' <dl>' +
            ' <dt>年级总排名</dt>' +
            ' <dd>' + MissExam(data.gradeRank) + '</dd> ' +
            '</dl>' +
            ' <dl>' +
            ' <dt class="am-text-truncate">年级平均分</dt>' +
            ' <dd>' + parseFloat(data.avgGradeScore.toFixed(2)) + '</dd> ' +
            '</dl>' +
            ' <dl> ' +
            '<dt>班内总排名</dt> ' +
            '<dd>' + MissExam(data.classRank) + '</dd>' +
            ' </dl>' +
            ' <dl> ' +
            '<dt class="am-text-truncate">班内平均分</dt>' +
            ' <dd>' + parseFloat(data.avgClassScore.toFixed(2));
        +'</dd>' +
        ' </dl> ' +
        '</div>';

        $('.head-title-analysis').append(HeadTitleHtml);

        AnalysisTableHtml = '<tr>';
        AnalysisTableHtml += '<th width="11%">学科</th>' +
            '<th width="11%">总分</th>' +
            '<th width="11%">A卷</th>' +
            '<th width="11%">B卷</th>' +
            '<th width="11%">年级平均分</th>' +
            '<th width="11%">年级A卷平均</th>' +
            '<th width="11%">年级B卷平均</th>' +
            '<th width="11%">年级排名</th>' +
            '</tr>';
        for (var i = 0; i < data.ranks.length; i++) {
            var paperType = data.ranks[i].paperType;
            AnalysisTableHtml += '<tr>' +
                '<td width="11%">' + data.ranks[i].subjectName + '</td>' +
                '<td>' + MissExam(data.ranks[i].totalScore) + '</td>' +
                '<td>' + MisssNo(data.ranks[i].aScore) + '</td>' +
                '<td>' + MisssNo(data.ranks[i].bScore) + '</td>';
            AnalysisTableHtml += '<td>' + MisssNo(parseFloat(data.ranks[i].avgGradeTotalScore.toFixed(1))) + '</td>';
            if(paperType==1){
                AnalysisTableHtml += '<td>-</td>';
                AnalysisTableHtml += '<td>-</td>';
            }
            if(paperType==2){
                AnalysisTableHtml += '<td>' + MisssNo(parseFloat(data.ranks[i].avgGradeAScore.toFixed(1))) + '</td>';
                AnalysisTableHtml += '<td>' + MisssNo(parseFloat(data.ranks[i].avgGradeBScore.toFixed(1))) + '</td>';
            }

            AnalysisTableHtml += '<td>' + MisssNo(data.ranks[i].gradeRank) + '</td>' +
                '</tr>';
        }
        $('#PerformanceDetails').append(AnalysisTableHtml);
    }
    /**
     *  各学科成绩分数段
     * @param notice-analysis
     */
    $.dAjax({
        method: 'work_eaScoreSections',
        id    : uri.examId
    }, function (json){
        bindSegment(json.data);
        S.progress.done();
    });
    /**
     * 绑定各学科成绩分数段
     * @param item
     */
    var bindSegment        = function (data){
        var SegmentHtml = '';
        for (var i = 0; i < data.length; i++) {
            SegmentHtml += '<li>'

                + '<div class="d-ranking d-tab-score">'
                + '<div class="f-cb tab-title">'
                + '<div class="am-fl subject">' + data[i].subjectName + '</div>'
                + '<div class="am-fl tab-menu">'
                + '<span class="on">总分</span>';
            if(data[i].paperType == 2){
                SegmentHtml += '<span>A卷</span>'
                    + '<span>B卷</span>';
            }
            SegmentHtml += '</div>'
                + '</div>'

                + '<div class="tab-score-conten">'
                    //+''<!--总分-->''
                + '<table class="d-analysis-table on g-segment">'
                + '<tr>'
                + '<th>分数段</th>'
                + '<th>人数</th>'
                + '</tr>';
            //总分
            for (var k = data[i].section.scoreGroupes.length - 1; k >= 0; k--) {
                SegmentHtml += '<tr>'
                    + '<td>' + data[i].section.scoreGroupes[k].scoreInfo + '</td>'
                    + '<td>' + data[i].section.scoreGroupes[k].count + '</td>'
                    + '</tr>';
            }
            SegmentHtml += '</table>';
            // 没有AB卷情况
            if(data[i].paperType == 2){
                //+''<!--A卷-->''
                SegmentHtml += '<table class="d-analysis-table g-segment">'
                    + '<tr>'
                    + '<th>分数段</th>'
                    + '<th>人数</th>'
                    + '</tr>'
                    + '<tr>';
                //A卷
                for (var g = 0; g < data[i].section.abScoreGroupes.length; g++) {
                    if(data[i].section.abScoreGroupes[g] == data[i].section.abScoreGroupes[0]){
                        for (var h = data[i].section.abScoreGroupes[g].length - 1; h >= 0; h--) {
                            SegmentHtml += '<tr>'
                                + '<td>' + data[i].section.abScoreGroupes[0][h].scoreInfo + '</td>'
                                + '<td>' + data[i].section.abScoreGroupes[0][h].count + '</td>'
                                + '</tr>';
                        }
                    }
                }
                SegmentHtml += '</table>'
                        //+''<!--B卷-->''
                    + '<table class="d-analysis-table g-segment">'
                    + '<tr>'
                    + '<th>分数段</th>'
                    + '<th>人数</th>'
                    + '</tr>';
                //B卷
                for (var g = 0; g < data[i].section.abScoreGroupes.length; g++) {
                    if(data[i].section.abScoreGroupes[g] == data[i].section.abScoreGroupes[1]){
                        for (var h = data[i].section.abScoreGroupes[g].length - 1; h >= 0; h--) {
                            SegmentHtml += '<tr>'
                                + '<td>' + data[i].section.abScoreGroupes[1][h].scoreInfo + '</td>'
                                + '<td>' + data[i].section.abScoreGroupes[1][h].count + '</td>'
                                + '</tr>';
                        }
                    }
                }
                SegmentHtml += '</table>';
            }

            SegmentHtml += '</div>'
                + '</div>'
                + '</li>';
        }

        $('.fraction-list').append(SegmentHtml);



    };
    /**
     * TAB选项卡
     * @param
     */
    $('body').on('touchstart click', '.d-tab-score .tab-menu span', function (event){
        event.preventDefault();
        var _this = $(this);
        _this.addClass('on').siblings().removeClass('on');
        _this.parents('.d-tab-score').children('.tab-score-conten').find('table').eq(_this.index()).addClass('on').siblings().removeClass('on');
    });
})(jQuery, SINGER);