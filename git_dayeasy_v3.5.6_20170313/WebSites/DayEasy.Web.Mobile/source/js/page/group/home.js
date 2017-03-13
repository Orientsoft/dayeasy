/**
 * 我的圈子
 * Created by shay on 2016/5/24.
 */
(function ($, S){
    window.flag=0;
    S.progress.start();
    var loadGroups,
        bindGroups,
        bindSoso,
        scroll,
        page       = 1,
        logger     = S.getLogger('group-home'),
        $groupList = $('.d-group-list'),
        $noGroup   = $('.d-group-no-class');
    var $groupSoso     = $('.group-soso'),
        $soso          = $('.d-soso-one'),
        $noGroupSoso   = $('.d-group-no-soso'),
        $groupSosoCont = $('.d-group-soso-list'),
        $sosoIinput = $('.dy-form-field');

    // 搜索框
    var off           = true,
        $selectBtn    = $('.select-main'),
        $ali          = $('.select-option li'),
        $selectOption = $('.select-option'),
        $groupName    = $('.d-group-name');
    /**
     * 加载我的圈子
     */
    loadGroups        = function (){
        $.dAjax({
            method     : 'group_groups',
            type       : 0,  // 圈子类型:-1，所有；0，班级圈；1，同事圈。
            loadMessage: true
        }, function (json){
            if(!json.status || json.count == 0){
                $groupList.addClass('hide');
                $noGroup.removeClass('hide');
                return false;
            }
            bindGroups(json.data);
        });
    };
    /**
     * 绑定圈子
     * @param groups
     */
    bindGroups        = function (groups){
        var $dlList;
        $dlList = $groupList.find('dl');
        for (var i = 0; i < groups.length; i++) {
            var group = groups[i],
                $dl   = $dlList.eq(group.type);
            $dl.removeClass('hide');
            var html  = template('groupTemp', group);
            $dl.append(html);
        }

    };

    loadGroups();
    S.progress.done();

    //搜索
    $groupSoso.on('click', function (){
        if(off){
            $(this).find('.dy-icon-search').addClass('dy-icon-left');
            $groupList.addClass('hide');
            $soso.removeClass('hide');
            $groupName.text('圈子搜索');
            $noGroup.addClass('hide');
            $sosoIinput.val('').focus();

            page = 1;
            off  = false;
        } else {
            $(this).find('.dy-icon-search').removeClass('dy-icon-left');
            $groupName.text('我的圈子');
            $groupList.removeClass('hide');
            console.log($groupList.find('dd').length);
                if($groupList.find('dd').length==0){
                    $noGroup.removeClass('hide');
                }
            $soso.addClass('hide');
            $groupSosoCont.empty().addClass('hide');
            $noGroupSoso.addClass('hide');
            $(window).unbind("scroll");
            off = true;
        }
    });


    $soso.on('click', '.btn-soso', function (){
        var $this = $(this),
            $warp = $this.parents($soso),
            _text = $warp.find('.dy-form-field').val();
        // 防止用户多次点击
        if(window.flag == 0){
            window.flag = 1;
            S.progress.start();
            bindSoso(_text, page);
        }
    }).on('change', $sosoIinput, function() {
        window.flag = 0;
        page = 1;
    });

    // text 搜索	关键词
    // page 页码
    bindSoso = function (text, page){
        var _selectBtntext = $selectBtn.find('span').data('type');
        $.dAjax({
            method     : 'group_search',
            keyword    : text,
            type       : _selectBtntext,
            page       : page
        }, function (json){
            $noGroupSoso.addClass('hide');
            if(!json.status || json.count == 0){
                $groupSosoCont.empty();
                $noGroupSoso.removeClass('hide');
                S.progress.done();
                return false;
            }
            if(json.data == 0){
                $groupSosoCont.append('<div class="dy-load-bata">没有相关圈子了</div>');
                $(window).unbind("scroll");
                return false;
            }
            var data = json.data;
            var sosodata=function(data){
                <!--搜索圈子列表-->
                var sosoHtml = '<ul>'
                for (var i = 0; i < data.length; i++) {
                    sosoHtml += '<li>'
                    sosoHtml += '<img src="' + data[i].logo + '" width="80" height="80" alt="' + data[i].groupSummary + '">'
                    sosoHtml += '<span class="group-name">' + data[i].name + '</span>'
                    sosoHtml += '<div class="group-count am-text-truncate">' + data[i].code + '</div>'
                    sosoHtml += '<div class="group-count group-member am-text-truncate ">' + (data[i].owner || '') + '</div>'
                    sosoHtml += '<div class="d-message-count"><a href="/page/group/apply.html?groupId=' + data[i].id + '&logo=' + data[i].logo + '&name=' + data[i].name + '&code=' + data[i].code + '&count=' + (data[i].owner || "") + '&type=' + (data[i].groupSummary || "") + '">申请加入</a></div>'
                    sosoHtml += '</li>'
                }
                sosoHtml += '</ul>'
                $groupSosoCont.empty().removeClass('hide').append(sosoHtml);
                S.progress.done();
            }


            sosodata(data);

        });
    }
    /**
     * 滚动条添加-更多
     * @param item
     */
    $(window).scroll(function (){
        if(!($groupSosoCont.hasClass('hide'))){
            var scrollTop    = $(this).scrollTop();
            var scrollHeight = $(document).height();
            var windowHeight = $(this).height();
            if(scrollTop + windowHeight == scrollHeight){
                var _text = $sosoIinput.val();
                S.progress.start();
                page += 1;
                bindSoso(_text, page);
                S.progress.done();
            }
        }
    });
//搜索
    $('body').on('click', '.select-wrap', function (event){
        event.preventDefault();
        if(off){
            $selectOption.stop(true, true).slideDown();
            off = false;
        } else {
            $selectOption.stop(true, true).slideUp();
            off = true;
        }

    });
    $ali.click(function (){
        var $this = $(this);
        var oText = $this.text();
        $selectBtn.find('span').data('type', $this.data('type'));
        $selectBtn.find('span').text(oText);
        $selectOption.slideUp(10);
        $sosoIinput.focus();
    });

    // 家长没有绑定学生账号-阻止加圈
    $.dAjax({
        method : 'user_children'
    }, function (json){
        console.log(json.data)
        if(json.data==''){
            $('body').on('click', '.d-message-count', function(event) {
            	event.preventDefault();
                S.msg('请用电脑登录绑定学生后，再加圈');
                return false;
            });


        }


//        if(!json.status){
//            S.msg(json.message);
//            return false;
//        }
    },true);


})(jQuery, SINGER);