/**
 * Created by shay on 2016/4/25.
 */
(function ($, S) {
    $.fn.extend({
        agency: function (conf) {
            var $input = $(this);
            conf = S.mix({
                item: $('.dy-agency-wrap'),
                list: $('.dy-agencies'),
                init: undefined,
                add: undefined,
                remove: undefined,
                singe: false
            }, conf || {});
            if ($input.length == 0 || conf.item.length == 0 || conf.list.length == 0)
                return false;
            conf.singe = $input.data('singe');
            var $agency = conf.item.find('#agencyId'),
                agencyId = $agency.val(),
                keyword = '';
            if (agencyId) {
                $.post('/system/agency', {
                    id: agencyId
                }, function (json) {
                    if (json.status) {
                        conf.item.removeClass('hide');
                        var item = S.format('<em class="stage{stageCode}">{stage}</em>{name}<small>{area}</small>', json.data);
                        conf.item.find('.dy-agency-item').html(item)
                            .append('<i title="删除" class="fa fa-times"></i>');
                        if (conf.singe) {
                            $input.hide();
                        }
                        conf.init && S.isFunction(conf.init) && conf.init.call(this, json.data);
                    }
                });
            }
            $input.bind('keyup.agency', function () {
                var word = S.trim($(this).val());
                if (agencyId || keyword == word) {
                    return false;
                }
                keyword = word;
                if (!keyword || !S.trim(keyword)) {
                    conf.list.empty();
                    conf.list.addClass('hide');
                    return false;
                }
                $.post('/system/agencies', {
                    keyword: encodeURIComponent(keyword)
                }, function (json) {
                    conf.list.empty();
                    if (json.status) {
                        for (var i = 0; i < json.data.length; i++) {
                            var item = json.data[i];
                            var $item = $(S.format('<p class="dy-agency-item"><em class="stage{stageCode}">{stage}</em>{name}<small>{area}</small></p>', item));
                            $item.data('agency', item);
                            conf.list.append($item);
                        }
                        conf.list.removeClass('hide');
                    } else {
                        conf.list.addClass('hide');
                    }
                });
            });
            conf.list
                .delegate('p', 'click', function () {
                    var $t = $(this),
                        agency = $t.data('agency');
                    $agency.val(agency.id);
                    $input.val('');
                    conf.list.empty().addClass('hide');
                    //conf.item.removeClass('hide');
                    //conf.item.find('.dy-agency-item').html($t.html()).append('<i title="删除" class="fa fa-times"></i>');
                    if (conf.singe) {
                        $input.hide();
                    }
                    conf.add && S.isFunction(conf.add) && conf.add.call(this, agency);
                })
                .delegate('p', 'mouseenter', function () {
                    $(this).addClass('hover');
                })
                .delegate('p', 'mouseleave', function () {
                    $(this).removeClass('hover');
                });
            conf.item.delegate('i', 'click', function () {
                $agency.val('');
                conf.item.addClass('hide');
                conf.item.find('.dy-agency-item').html('');
                if (conf.singe) {
                    $input.show().focus();
                }
                keyword = '';
                agencyId = '';
                conf.remove && S.isFunction(conf.remove) && conf.remove.call(this);
            });
        }
    });
    $('.agency-input').agency();
})(jQuery, SINGER);