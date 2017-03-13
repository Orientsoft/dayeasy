/**
 * 选课
 * Created by bai on 2016/7/18.
 */
(function ($, S) {
    var $wrap = $('.page-course'),
        $main = $('.page-course-main'),
        batch = $wrap.data('batch'),
        selectable = false;
    /**
     * 临时数据
     * @type {string}
     */
//        var title_text = '2018年棕北中学选课';
//        var data = [
//            {
//                "type": 0,
//                "Subject": "毛笔课程1",
//                "name": "姓名",
//                "address": "多媒体教室A-B-1",
//                "quotaPeople": "4",
//                "overallPeople": "88"
//            },
//            {
//                "type": 1,
//                "Subject": "毛笔课程2",
//                "name": "姓名",
//                "address": "多媒体教室A-B-2",
//                "quotaPeople": "60",
//                "overallPeople": "88"
//            },
//            {
//                "type": 2,
//                "Subject": "毛笔课程3",
//                "name": "姓名",
//                "address": "多媒体教室A-B-3",
//                "quotaPeople": "0",
//                "overallPeople": "88"
//            },
//            {
//                "type": 0,
//                "Subject": "毛笔课程3",
//                "name": "姓名",
//                "address": "多媒体教室A-B-3",
//                "quotaPeople": "0",
//                "overallPeople": "50"
//            },
//            {
//                "type": 0,
//                "Subject": "毛笔课程3",
//                "name": "姓名",
//                "address": "多媒体教室A-B-3",
//                "quotaPeople": "400",
//                "overallPeople": "800"
//            },
//            {
//                "type": 0,
//                "Subject": "毛笔课程3",
//                "name": "姓名",
//                "address": "多媒体教室A-B-3",
//                "quotaPeople": "55",
//                "overallPeople": "180"
//            }
//        ];
    S._mix(S, {
        /**
         * 计算效果报名百分比宽度
         **/
        proportion: function ($ele, step) {
            var g = $ele.data('total'),
                s = $ele.data('current'),
                process;
            if (step) {
                s += step;
                $ele.data('current', s);
                $ele.siblings('.course-list-bottom').find('em:eq(1)').html(g - s);
            }
            process = (g <= 0 ? 100 : ((s / g) * 100)).toFixed(2);
            $ele.find('.d-process').css({
                width: process + '%'
            });
        },
        /**
         * 数据绑定
         */
        bingData: function (json) {
            S.loading.start($main);
            $wrap.find('.h2-title').append(json.title);
            S.each(json.courses, function (item) {
                item.status = (item.selected ? 'sign-up' : (item.selectedCount == item.capacity ? 'pack-full' : 'default'));
            });
            var html = template('html-course', {
                data: json.courses
            });
            $main.html(html);
            $main.find('.course-proportion').each(function (index, item) {
                S.proportion($(item));
            });
        }
    });
    $.get('/elective/courseList', {
        batch: batch
    }, function (json) {
        if (!json.status) {
            $main.append('<div class="dy-nothing"><i class="iconfont dy-icon-none"></i>' + json.message + '</div>');
            return false;
        }
        selectable = json.data.selectable;
        S.bingData(json.data);
    });
    // 弹框
    $main.on('click', '.course-box-list button', function () {
        var $courseBoxLis = $(this).parents('.course-box-list'),
            otext = $courseBoxLis.children('h3').text(),
            id = $courseBoxLis.data('id');
        if ($courseBoxLis.hasClass('sign-up')) {
            S.confirm('你确认要退出“' + otext + '”吗？', function () {
                /**
                 * 我要退出 操作
                 */
                $.post('/elective/quit', {
                    id: id
                }, function (json) {
                    if (!json.status) {
                        S.alert(json.message);
                        return false;
                    }
                    S.msg('退课成功！', 2000, function () {
                        $courseBoxLis.toggleClass('sign-up default');
                        S.proportion($courseBoxLis.find('.course-proportion'), -1);
                    });
                });
            })
        } else if ($courseBoxLis.hasClass('default')) {
            if ($('.page-course-main').find('.sign-up').length == 0) {
                S.confirm('你确认要选择“' + otext + '”吗？', function () {
                    /**
                     * 我要报名操作
                     */
                    $.post('/elective/course', {
                        id: id
                    }, function (json) {
                        if (!json.status) {
                            S.alert(json.message);
                            return false;
                        }
                        S.msg('选课成功！', 2000, function () {
                            $courseBoxLis.toggleClass('sign-up default');
                            S.proportion($courseBoxLis.find('.course-proportion'), 1);
                        });
                    });
                });
            } else {
                S.alert('<p>每人限报一门课<br>您可以先退掉之前的课程再来选择</p>');
            }
        }
    });
})(jQuery, SINGER);