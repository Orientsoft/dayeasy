/**
 * 批阅中心
 * Created by bai on 2017/01/10
 */
(function ($, S){
    var pageData = {},
        helpAssistant,
        initPage,
        loading=false,
        $window = $(window),
        $document = $(document);
    pageData = {
        DataHelp: [
            {
                href: 'http://www.dayeasy.net/topic/detail/c2047acc7b6d46299ddd7ba089188adc',
                text: '大型考试如何进行流水阅卷？'
            },
            {
                href: 'http://www.dayeasy.net/topic/detail/6adf36f3b6c74b588a2b890955470da2',
                text: '普通考试如何按班级批阅？'
            }
        ],
        DataInitPage: []
    };
    /**
     * 批阅
     */
    initPage = function (){
        var
            getData,
            bindData,
            page = 0,
            size = 10,
            $main = $('.main-marking'),
        $draftcenter = $main.find('.draft-center-list');
        // 获取数据
        getData = function (page, size, callback){
            $.ajax({
                    url: '/work/teacher/marking-list',
                    type: 'GET',
                    dataType: 'json',
                    data: {page: page, size: size},
                })
                .done(function (json){
					if(!json.status){
						S.msg(json.message);
						return false;
					}
					if(json.count == 0) {
						$window.unbind("scroll.getData");
                        $main.html(template('markingTplNot', pageData.DataInitPage));
						return false
                    }
                    if(json.data && json.data.length){
                        pageData.DataInitPage = json.data;
                        callback && callback.call(null, json.data);
                        // 数据总条数
                        if(json.count <= (page+1)*size){
                            $window.unbind("scroll.getData");
                            S.loading.done($main);
                            $draftcenter.append('<div class="box-lg-12 draft-list f-tac"><p class="not-data"><i class="iconfont dy-icon-warning"></i>没有更多数据了</p></div>');
                            return false;
                        }
                    }
                })
                .fail(function (){
                    $main.html(S.showNothing({
                        word: '数据连接请求失败'
                    }));
                });
        };
        // 绑定类表数据
        bindData = function (data){
            S.loading.done($draftcenter);
            pageData.DataInitPage=[];
            S.each(data, function (item){
                pageData.DataInitPage.push({
                    isJoint: item.isJoint,
                    paperTitle: item.paperTitle,
                    time: item.time,
                    alloted: item.alloted,
                    aCount: item.aCount,
                    bCount: item.bCount,
                    paperType: item.paperType,
                    status: item.status,
                    groupName: item.group.name,
                    hrefTest:'/paper/detail/'+ item.paperId,
                    // 圈子地址
                    hrefGroup: S.sites.main + '/group/' + item.group.id,
                    // 普通阅卷
                    hrefIsJointFalse: S.sites.main + '/marking?batch=' + item.batch,
                    // 协同阅卷
                    hrefIsJointTrue: S.sites.main + '/marking/mission_v2/' + item.batch,
                    // 协同阅卷分配题目
                    hrefAllot: S.sites.main + '/marking/allot/' + item.batch
                });
            });
            $draftcenter.append(template('markingTpl', pageData.DataInitPage));
        };
        //数据初始化
        getData(page, size, function (data){
            bindData(data);
            loading=false;
        });
        // 绑定滚屏加载
        $window.bind('scroll.getData', function (){
            var winHeight = window.innerHeight ? window.innerHeight : $window.height(),
                closeToBottom = ($window.scrollTop() + winHeight > $document.height()-270);
            if(!loading && closeToBottom){
                loading=true;
                $draftcenter.append('<div class="dy-loading"><i></i></div>');
                page++;
                getData(page, size, function (data){
                    bindData(data);
                    S.loading.done($main);
                    loading=false;
                });
            }
        });
    };
    /**
     * 小助手
     */
    helpAssistant = function (){
        var html = '',
            $boxHelp = $('.sidebar-marking').find('.box-text-list'),
            data = pageData.DataHelp;
        for (var i = 0; i < data.length; i++) {
            html += '<li><a target="_blank" href="' + data[i].href + '" title="' + data[i].text + '？">' + data[i].text + '</a></li>'
        }
        $boxHelp.html(html);
    };

    initPage();
    helpAssistant();

})(jQuery, SINGER);