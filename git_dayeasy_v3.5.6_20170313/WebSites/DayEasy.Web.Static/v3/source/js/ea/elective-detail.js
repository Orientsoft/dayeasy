/**
 * Created by shay on 2016/9/29.
 */
(function ($, S) {
    var classData;
    var showClass = function (list, name) {
        var show = function () {
            var tplData = [];
            S.each($.extend(true, {}, classData), function (value, key) {
                var item = {
                    grade: key,
                    classList: []
                };
                S.each(value, function (classItem) {
                    if (!list || list.length == 0 || $.inArray(classItem.id, list) >= 0) {
                        item.classList.push(classItem);
                    }
                });
                if (item.classList.length > 0) {
                    var left = item.classList.length % 5;
                    if (left > 0) {
                        for (var i = 0; i < 5 - left; i++) {
                            item.classList.push({id: -1, name: ''});
                        }
                    }
                    tplData.push(item);
                }
            });
            var html = template('classRangeTpl', tplData);
            dialog({
                title: name + ' - 选课范围',
                content: html,
                quickClose: true,
                fixed: true,
                backdropOpacity: 0.3
            }).showModal();
        };
        if (!classData) {
            $.get('/elective/classList', {}, function (json) {
                classData = json;
                show();
            });
        } else {
            show();
        }
    };
    $('.btn-class-range').bind('click', function () {
        var $t = $(this),
            cls = $t.data('class'),
            list = [];
        if (cls) {
            list = S.json(decodeURIComponent(cls));
        }
        showClass(list, $t.parents('tr').find('td:eq(0)').html());
        return false;
    });
})(jQuery, SINGER);