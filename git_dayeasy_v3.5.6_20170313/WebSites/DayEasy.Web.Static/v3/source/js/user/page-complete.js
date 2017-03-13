/**
 * 完善用户信息
 * Created by shay on 2016/8/26.
 */
(function ($, S) {
    var lastWord;
    /**
     * 机构搜索
     * @param stage
     * @param keyword
     */
    var searchAgency = function (stage, keyword) {
        $.ajax({
            type: "GET",
            data: {keyword: keyword, stage: stage},
            url: S.sites.main + "/agency-search",
            dataType: "jsonp",
            success: function (json) {
                lastWord = keyword;
                showAgency(json);
            }
        });
    };
    var showAgency = function (list) {
        var $list = $('.agency-list');
        if (!list) {
            $list.addClass('hide');
            return false;
        }
        $list.removeClass('hide');
        $list = $list.find('ul');
        $list.empty();
        if (!list || !list.length) {
            $list.append('<li>没有找到相关学校</li>');
            return false;
        } else {
            S.each(list, function (item) {
                $list.append(S.format('<li class="item" data-id="{id}" title="{name}">{name}</li>', item));
            });
        }
    };
    /**
     * 随机用户
     * @param role
     */
    var randomUsers = function (role) {

    };
    $('#keyword').bind('keyup', function () {
        var word = $(this).val(),
            stage = $('#stageList').val();
        if (lastWord == word) return false;
        if (!word) {
            showAgency();
            return false;
        }
        searchAgency(stage, S.trim(word));
    });
    var $checked = $('.agency-checked'),
        $agencyList = $('.agency-list'),
        resetAgency = function () {
            $checked.addClass('hide').find('.agency-item').data('id', '').empty();
            $agencyList.prev().removeClass('hide').val('').focus();
            lastWord = '';
        };
    $agencyList.on('click', 'li.item', function () {
        var $t = $(this),
            $item = $checked.find('.agency-item');
        $checked.removeClass('hide');
        $agencyList.addClass('hide').prev().addClass('hide');
        var name = $t.html();
        $item.data('id', $t.data('id')).html(name).attr('title', name);
    });
    $checked.find('i').bind('click', function () {
        resetAgency();
    });
    $('#stageList').bind('change', function () {
        resetAgency();
    });
    /**
     * 跳转
     */
    var redirect = function () {
        var returnUrl = S.uri().return_url,
            url = '/user/introduce';
        if (returnUrl) {
            url += '?return_url=' + returnUrl;
        }
        location.href = url;
    };
    //提交数据
    $('#submitBtn').bind('click', function () {
        var $btn = $(this),
            name = $('#name').val(),
            agencyId = $('.agency-item').data('id'),
            year = $('#yearDdl').val(),
            month = $('#monthDdl').val() || '1',
            start = S.format('{0}-{1}', year, month),
            gender = $('#genderDdl').val();
        if (!agencyId) {
            S.msg('请输入学校信息！', 2000, function () {
                $('#keyword').focus();
            });
            return false;
        }
        if (!/^[\u4e00-\u9fa5]{2,5}$/.test(name)) {
            S.msg('真实姓名需要输入2-5个汉字！', 2000, function () {
                $('#name').focus();
            });
            return false;
        }
        $btn.disableField('稍候..');
        var postData = {
            name: name,
            agencyId: agencyId,
            start: start,
            gender: gender
        };
        //console.log(postData);
        //return false;
        $.post('/user/complete', postData, function (json) {
            if (!json.status) {
                S.alert(json.message);
                $btn.undisableFieldset();
                return false;
            }
            redirect();
        });
        return false;
    });
})(jQuery, SINGER);