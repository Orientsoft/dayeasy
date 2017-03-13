/**
 * Created by epc on 2016/3/4.
 */
(function ($, S) {
    var preCodes = [];
    var agency = {
        id: '',
        name: ''
    };
    //机构
    $('.b-agency-key').bind('keyup', function (e) {
        var keyword = $(this).val(),
            $agencies = $('.dy-agencies');
        if (!keyword || !S.trim(keyword)) {
            $agencies.empty();
            $agencies.addClass('hide');
            return false;
        }
        $.post('/system/agencies', {
            keyword: encodeURIComponent(keyword)
        }, function (json) {
            $agencies.empty();
            if (json.status) {
                for (var i = 0; i < json.data.length; i++) {
                    var item = json.data[i];
                    var $item = $(S.format('<p class="dy-agency-item"><em class="stage{stageCode}">{stage}</em>{name}<small>{area}</small></p>', item));
                    $item.data('id', item.id);
                    $item.data('name', item.name);
                    $agencies.append($item);
                }
                $agencies.removeClass('hide');
            } else {
                $agencies.addClass('hide');
            }
        });
        return false;
    });
    $('.dy-agencies')
        .delegate('p', 'click', function () {
            var $t = $(this);
            agency.id = $t.data("id");
            agency.name = $t.data("name");
            $(".b-agency-key").val(agency.name);
            $('.dy-agencies').empty().addClass('hide');
        })
        .delegate('p', 'mouseenter', function () {
            $(this).addClass('hover');
        })
        .delegate('p', 'mouseleave', function () {
            $(this).removeClass('hover');
        });
    //用户
    var userIds,existInUserId,removeInUserId,existInPre,removeInPre,getRoleName,searchUser;
    existInUserId = function(id){
        if(!userIds || !userIds.length){
            userIds = [];
            $(".b-user-list tr").each(function(i,tr){
                userIds.push($(tr).data("id"));
            });
        }
        if(!userIds.length) return false;
        for(var i=0;i<userIds.length;i++){
            if(userIds[i] == id) return true;
        }
        return false;
    };
    removeInUserId = function(id){
        if(!userIds || !userIds.length) return;
        for(var i=0;i<userIds.length;i++){
            if(userIds[i] == id){
                userIds.splice(i,1);
                return;
            }
        }
    };
    existInPre = function(code){
        if(!preCodes.length) return false;
        for(var i=0;i<preCodes.length;i++){
            if(preCodes[i] == code) return true;
        }
        return false;
    };
    removeInPre = function(code){
        if(!preCodes.length) return;
        for(var i=0;i<preCodes.length;i++){
            if(preCodes[i] == code){
                preCodes.splice(i,1);
                return;
            }
        }
    };
    getRoleName = function(role){
        if(!role) return '-';
        var result = '';
        if((role & 4) > 0) result += '<span class="mr5">教师</span>';
        if((role & 128) > 0) result += '<span class="mr5">系统管理员</span>';
        return result;
    };
    searchUser = function () {
        var code = S.trim($('.b-user-code').val());
        if (!code || code.length !== 5) {
            return false;
        }
        if(existInPre(code)){
            S.msg("请勿重复添加");
            return false;
        }
        $.post('/sys/apps/user', {code: code}, function (json) {
            if (json.status) {
                $('.b-user-code').val('').focus();
                if(existInUserId(json.data.id)){
                    S.msg("请勿重复添加");
                    return;
                }
                var info = json.data.email ? json.data.email : json.data.nick;
                var $tr = $('<tr data-id="'+json.data.id+'" data-code="'+code+'" data-agency="'+agency.id+'">');
                $tr.append('<td>'+info+'</td>');
                $tr.append('<td>'+json.data.name+'</td>');
                $tr.append('<td>'+getRoleName(json.data.role)+'</td>');
                $tr.append('<td>'+(agency.name && agency.name.length ? agency.name : "-")+'</td>');
                var $operation = $('<td class="dy-operations">');
                $operation.append('<span data-t="push" class="dy-operation dy-success" title="确认添加"><i class="fa fa-check"></i></span>');
                $operation.append('&nbsp;|&nbsp;');
                $operation.append('<span data-t="remove" class="dy-operation dy-danger" title="删除"><i class="fa fa-close"></i></span>');
                $tr.append($operation);
                $('.b-user-list').append($tr);
                preCodes.push(code);
            } else {
                S.msg(json.message);
            }
        });
    };
    $(".b-user-search").bind("click",searchUser);
    $(".b-user-code").bind("keyup",function(e){
        e.stopPropagation();
        if (e.keyCode === 13)
            searchUser();
        return false;
    });

    $('.b-user-list').delegate(".dy-operation","click",function(){
        var $this = $(this);
        var t = $this.data("t");
        var $tr = $this.parents("tr");
        if(t == "remove"){
            removeInPre($tr.data("code"));
            $tr.remove();
            return;
        }
        var appId = $(".d-app-configure").data("appid");
        if(t == "delete"){
            var userId = $tr.data("id");
            S.confirm('确认删除用户的应用？', function () {
                $.post("/sys/apps/configure-remove", {
                    appId: appId,
                    userId: userId
                }, function (json) {
                    if (json.status) {
                        removeInUserId(userId);
                        S.msg("取消分配成功",1000,function(){
                            $tr.remove();
                        });
                    } else {
                        S.msg(json.message);
                    }
                });
            });
            return;
        }
        if(t == "push"){
            $.post("/sys/apps/configure-submit",{
                appId: appId,
                userId: $tr.data("id"),
                agencyId: $tr.data("agency")
            },function(json){
                if(json.status){
                    S.msg("已成功添加用户的应用",1000,function(){
                        $this.parents("td").html('<span data-t="delete" class="dy-operation dy-danger"><i class="fa fa-trash"></i> 删除</span>');
                    });
                }else{
                    S.msg(json.message);
                }
            });
        }

    });
})(jQuery, SINGER);