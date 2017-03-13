(function (S) {
    S._mix(S, {
        orgMenus: [
            { 'name': '基本信息', 'url': '/' },
            { 'name': '年级结构', 'url': '/grade' },
            { 'name': '教师职工', 'url': '/employee' },
            { 'name': '认证审核', 'url': '/check' }
        ]
    });
})(SINGER);

/**
 * 弹出模版
 * 弹出模版样式 css/site-base.less -> deyi-dialog
 **/

var deyiDialogIsInit = false,
    DeyiDialogInit = function(){
        if(deyiDialogIsInit)
            return;
        var obj = $(".deyi-dialog");
        var x = $(window).width(),
            y = $(window).height(),
            w = (typeof($(obj).attr("width") == "undefined") ? 500 : $(obj).attr("width")),
            h = (typeof($(obj).attr("width") == "undefined") ? 300 : $(obj).attr("height"));
        var l = (x - w) / 2,
            t = (y - h) / 2;
        if(l<0) l=0;
        if(t<0) t=0;
        var _style = "left:" + l + "px;top:" + t + "px;"
        $(obj).attr("style",_style);
        deyiDialogIsInit = true;
    };

$(function($){
    $(document)
        .delegate(".deyi-dialog-close", "click", function () {
            ddClose();
        })
    ;
});

function ddShow() {
    $("body").append("<div id='disable-body' onclick='ddClose()' class='d-mask'></div>");
    DeyiDialogInit();
    $(".deyi-dialog").slideDown(400);
}

function ddClose() {
    $("#disable-body").remove();
    $(".deyi-dialog").fadeOut(200);
}
/** 弹出模版结束*/

/**
 分页条
 计算后生成对象
 pages_html={
        l:分页条标签起始位置,
        r:结束位置,
        data[{
            index:页码数值,
            text:用于显示,
            bind:是否重新绑定
        }]
    }
 分页条标签点击事件：
 若 pages_html.data[i].bind=true、
 当前页码小于分页条起始位置、
 或           大于分页条结束位置、
 需 重新调用 PageHtml()

 数据源（必须）,若max有值则不需要size和total，size和total用于计算总页码max
 {index:当前页码,max:总页码,page:分页条标签数量,size:每页显示数量,total:总数量}
 $scope.pages = { index: 1, max: 18, page: 5, size: 10, total: 177 };

 调用示例：

 Controller 提供 $scope.pages 并调用 PageHtml($scope);

 html ：
 <li ng-repeat="p in pages_html.data" ng-class="{'active':pages.index==p.index}">
 <span ng-click="cpage(p.index,p.bind)">
 {{p.text}}
 </span>
 </li>

 Controller 提供翻页 $scope.cpage = function (页码,是否重新绑定)

 **/
function PageHtml(obj) {
    if (typeof (obj.pages.max) == "undefined" || obj.pages.max < 1)
        obj.pages.max = Math.ceil(obj.pages.total / obj.pages.size);
    if (obj.pages.index < 1)
        obj.pages.index = 1;
    if (obj.pages.index > obj.pages.max && obj.pages.max > 0)
        obj.pages.index = obj.pages.max;
    var idx = obj.pages.index;
    if (idx % obj.pages.page == 0) {
        idx -= (obj.pages.page - 1);
    } else if (idx % obj.pages.page != 1) {
        idx = idx - (idx % obj.pages.page) + 1;
    }

    obj.pages_html = { l: idx, r: idx + obj.pages.page - 1, data: [] };
    //起始页码
    if (idx > obj.pages.page)
        obj.pages_html.data.push({ index: idx - obj.pages.page, text: (idx - obj.pages.page) + "...", bind: true });
    for (var i = idx; i <= obj.pages.max; i++) {
        obj.pages_html.data.push({ index: i, text: i, bind: false }); //标签
        //结束页码
        if (i >= idx + obj.pages.page - 1) {
            obj.pages_html.data.push({ index: i + 1, text: (i + 1) + "...", bind: true });
            break;
        }
    }
}

/**Json 日期时间格式化
 * @return {string}
 **/
function DateFormat(time, format) {
    if (!time) return "";
    format = format || "yyyy-MM-dd hh:mm";
    var date = new Date(parseInt(time.replace("/Date(", "").replace(")/", ""), 10));
    return singer.formatDate(date, format);
}