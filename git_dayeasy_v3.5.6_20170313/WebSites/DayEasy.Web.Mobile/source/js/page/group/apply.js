/**
 * 申请加入圈子
 * Created by Boz on 2016/6/13.
 */

(function ($, S){
    var
        loadGroups,
        uri             = S.uri(),
        $GroupConter    = $('.d-group-list'),
        $warpGroup      = $('.d-group'),
        $btn            = $('#applybutton'),
        HtmlGroupConter = '';
    var $realname    = $('#realname'),
        $information = $('#information');

    var  username;

    S.progress.start();
    //用户姓名存在不需要显示
    S.setUser(function (user) {
        if(user.name){
            $realname.val(user.name);
        }else{
            $realname.removeClass('hide');
            $btn.addClass('disabled');
        }
    });
    // 圈子信息列表
    loadGroups = function (){
        HtmlGroupConter += '<ul>';
        HtmlGroupConter += '<li>';
        HtmlGroupConter += '<img src="' + uri.logo + '" width="80" height="80" alt="' + uri.name + '">';
        HtmlGroupConter += '<span class="group-name">' + uri.name + '</span>';
        HtmlGroupConter += '<div class="group-count am-text-truncate">' + uri.code + '</div>';
        HtmlGroupConter += '<div class="group-count group-member am-text-truncate ">' + (uri.count || '') + '</div>';
        HtmlGroupConter += '<div class="group-count group-member am-text-truncate ">' + (uri.type || '') + '</div>';
        HtmlGroupConter += '</li>';
        HtmlGroupConter += '</ul>';
        $GroupConter.append(HtmlGroupConter);
    };
    loadGroups();
    S.progress.done();
    $warpGroup
        // 文本提交规则
        .on('keyup', '#realname', function (event){
            event.preventDefault();
            var inputVal    = $realname.val();
            !(inputVal == '')? $btn.removeClass('disabled') : $btn.addClass('disabled');
        });
    // 提交申请加入圈子
    $btn.on('click', function (event){
        event.preventDefault();
        var $this = $(this);
        if($this.hasClass('disabled')) return false;

        S.setUser(function (user) {
            if(user.name){
                username=user.name;
            }else{
                username= $realname.val();
                var myReg = /^[\u4E00-\u9FA5]+$/;
                if (!myReg.test( $.trim(username))) {
                    S.msg('请输入中文姓名');
                    return false
                }
            }
        $.dAjax({
            method : 'group_apply',
            groupId: uri.groupId,
            message: $information.val(),
            name   : username
        }, function (json){
            if(!json.status){
                S.msg(json.message);
                return false;
            }
            location.href = "/page/group/apply-success.html?name="+uri.name+"&code="+uri.code+"&logo="+uri.logo;
        },true);


        });


    });
})(jQuery, SINGER);

