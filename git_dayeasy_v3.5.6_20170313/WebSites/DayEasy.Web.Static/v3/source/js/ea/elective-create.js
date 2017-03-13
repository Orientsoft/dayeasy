/**
 * 创建选课
 * Created by shay on 2016/9/27.
 */
(function ($, S) {
    $.fn.extend({
        /**
         * 文件选择
         * @param option
         */
        fileSelector: function (option) {
            option = $.extend({}, {
                accept: 'image/*',
                start: undefined,
                finished: undefined
            }, option || {});
            var $input = $(S.format('<input type="file" id="importFile" accept="{0}" />', option.accept || 'image/*'));
            $input.css('display', 'none');
            $('body').append($input);
            $(this).bind('click', function () {
                $input.click();
            });
            $input.bind('change', function (event) {
                var files = event.target.files,
                    file;
                if (files && files.length > 0) {
                    file = files[0];
                    //console.log(file);
                    try {
                        option.start && option.start.call(this);
                        var fileReader = new FileReader();
                        fileReader.onload = function (event) {
                            var fileData = event.target.result;
                            option.finished && option.finished.call(this, file.name, fileData);
                        };
                        fileReader.readAsDataURL(file);
                    }
                    catch (e) {
                    }
                }
            });
        }
    });
    var loadingDialog, courseList = [], $list = $('.d-list'), classList;
    /**
     * 选课范围
     * @param id
     * @param name
     * @param callback
     */
    var classRange = function (id, name, callback) {
		console.log(classList);
        var course = S.array.find(courseList, function (item) {
            return item.id == id;
        });
        var bindClass = function () {
            var list = [];
            S.each($.extend(true, {}, classList), function (value, key) {
                var item = {
                    grade: key,
                    classList: [],
                    checked: true
                };
                S.each(value, function (classItem) {
                    if (!course.classList || $.inArray(classItem.id, course.classList) >= 0) {
                        classItem.checked = true;
                    }
                    else {
                        item.checked = false;
                    }
                    item.classList.push(classItem);
                });
                var left = item.classList.length % 5;
                if (left > 0) {
                    for (var i = 0; i < 5 - left; i++) {
                        item.classList.push({id: -1, name: ''});
                    }
                }
                list.push(item);
            });
            var html = template('classRangeTpl', list);
            var d = dialog({
                title: name + ' - 选课范围',
                content: html,
                quickClose: true,
                fixed: true,
                backdropOpacity: 0.3,
                okValue: '确认',
                cancelValue: '取消',
                ok: function () {
                    var $node = $(this.node),
                        $list = $node.find('dd.check-item'),
                        $classList = $node.find('dd.active');
                    if ($classList.length == 0) {
                        S.msg('请至少选择一个班级！');
                        return false;
                    }
                    if ($list.length != $classList.length) {
                        course.classList = [];
                        $classList.each(function (index, item) {
                            course.classList.push($(item).data('cid'));
                        });
                        callback && callback.call(this, false);
                    } else {
                        delete course.classList;
                        callback && callback.call(this, true);
                    }
                },
                cancel: function () {
                },
                onshow: function () {
                    var $node = $(this.node);
                    $node.find('.group-checkbox input').bind('click', function (e) {
                        e.stopPropagation();
                        var $input = $(this),
                            checked = $input.is(':checked'),
                            $dl = $input.parents('dl'),
                            $list = $dl.find('dd.check-item');
                        $input.siblings('i').toggleClass('dy-icon-checkboxhv', checked);
                        $list.toggleClass('active', checked);
                    });
                    $node.find('dd.check-item').bind('click', function (e) {
                        e.stopPropagation();
                        var $t = $(this), $dl, $all;
                        if ($t.hasClass('disabled'))
                            return false;
                        $t.toggleClass('active');
                        $dl = $t.parents('dl');
                        $all = $dl.find('.group-checkbox');
                        if ($dl.find('dd.check-item').length == $dl.find('dd.active').length) {
                            $all.find('i').addClass('dy-icon-checkboxhv');
                            $all.find('input')[0].checked = true;
                        } else {
                            $all.find('i').removeClass('dy-icon-checkboxhv');
                            $all.find('input')[0].checked = false;
                        }
                    });
                }
            });
            d.showModal();
        };
        if (!classList) {
            $.get('/elective/classList', {}, function (json) {
                classList = json;
                bindClass();
            });
        } else {
            bindClass();
        }
    };
    /**
     * 导入课程
     */
    $('.d-import').fileSelector({
        accept: '.csv, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel',
        start: function () {
            loadingDialog = S.dialog({
                content: '正在上传...'
            });
            loadingDialog.showModal();
        },
        finished: function (name, data) {
            $.post('/elective/import', {
                name: name,
                fileData: data
            }, function (json) {
                courseList = json;
                var html = template('courseListTpl', json);
                $list.html(html);
                var $importWrap = $('.d-import-wrap');
                if ($importWrap.length) {
                    $('.d-course-list').removeClass('hide').slideDown('fast');
                    $importWrap.remove();
                }
                $(window).bind("beforeunload.elective", function () {
                    return '离开当前页面，数据将不会被保存，是否继续？';
                });
                loadingDialog.close().remove();
            });
        }
    });
    /**
     * 选课范围
     */
    $list.on('click', '.btn-class-range', function () {
        var $t = $(this),
            $tr = $t.parents('tr'),
            id = $tr.data('cid'),
            name = $tr.find('td:eq(0)').html();
        classRange(id, name, function (status) {
            $t.html(status ? '全部班级' : '部分班级');
            $t.toggleClass('text-danger', !status);
        });
        return false;
    });
    /**
     * 删除选课
     */
    $list.on('click', '.btn-delete', function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data('cid');
        S.confirm('确认要删除该课程？', function () {
            $tr.remove();
            S.msg('删除成功');
            courseList = S.array.filter(courseList, function (item) {
                return item.id != id;
            });
            $('.total-num').html(courseList.length);
        });
    });
    $('.btn-cancel').bind('click', function () {
        $(window).unbind("beforeunload.elective");
        location.href = '/elective';
    });
    $('.btn-submit').bind('click', function () {
        if (!courseList || courseList <= 0) {
            S.alert('请选导入课程！');
            return false;
        }
        S.dialog({
            title: '创建选课',
            content: '<div class="elective-title">' +
            '<div class="form-label">为本次选课任务取个名字</div>' +
            '<input type="text" class="form-control" placeholder="请输入选课标题" />' +
            '</div>',
            okValue: '确认',
            cancelValue: '取消',
            ok: function () {
                var $node = $(this.node),
                    title = $node.find('.elective-title input').val();
                if (!title) {
                    S.msg('请输入选课标题');
                    return false;
                }
                var postData = {
                    title: title,
                    courses: courseList
                };
                $.post('/elective/create', {
                    data: encodeURIComponent(S.json(postData))
                }, function (json) {
                    if (json && json.status) {
                        S.msg('创建成功！', 2000, function () {
                            $(window).unbind("beforeunload.elective");
                            location.href = '/elective';
                        });
                        return false;
                    }
                    S.alert(json.message);
                });
                return false;
            },
            cancel: function () {

            }
        }).showModal();
    });

})(jQuery, SINGER);