/**
 * Created by bai on 2016/11/21.
 */
(function ($, S){

    var
        $document = $(document),
        checkboxOFF = true;
    var Esta = {
        stage: null,
        stageText: '',
        learningStage:[6,3,3], //学习阶段 [小学,初中,高中]
        dialogGroup: {
            BatchGroup: null,
            estaGroupPop: null,
            estaImportPop: null
        }
    };
    S._mix(S, {
        textArea: function (str){
            if(str == ''){
                return false;
            }
            str = str.replace(/\s+/g, ' ').split(" ");
            if(!S.isArray(str)){
                S.msg('粘贴格式错误');
            }
            //去除数组中空值
            for (var i = 0; i < str.length; i++) {
                if(str[i] == "" || typeof(str[i]) == "undefined"){
                    str.splice(i, 1);
                    i = i - 1;
                }
            }
            return str;
        },
        getStr: function (string, str){
            var str_before = string.split(str)[0];
            var str_after = string.split(str)[1];
            return [str_before, str_after]
        },
        inputCheck: function (obj, num){
            S.inputCheck($(this)[0], 20);
            var number = obj.value;
            var number2 = parseInt(number);
            if((/^[.0-9]+$/).test(number) && number2 >= 1 && number2 <= num)
                return;
            else
                obj.value = number.substring(0, number.length - 1);
        },
        /**
         * 检查姓名
         * @param name
         * @returns {boolean}
         */
        checkName: function (name){
            return /^[\u4e00-\u9fa5]{2,5}$/.test(name);
        },
        /**
         * 数组去重
         * @returns {Array}
         */
        pageArrRepeat: function (arr, callback){
            if(!S.isArray(arr) && !arr.length){
                return;
            }
            var nary = arr.sort();
            var repeatData = [];
            for (var i = 0; i < nary.length - 1; i++) {
                var item = nary[i];
                if(item == nary[i + 1] && !S.inArray(item, repeatData)){
                    repeatData.push(item);
                }
            }
            callback && callback.call(this, repeatData);
        }
    });
    /**
     * 创建圈子数据
     * @type {{GetFullYear: DataEstaGroupPop.GetFullYear, subjects: boolean, getSubjects: DataEstaGroupPop.getSubjects, hrefGet: string}}
     */
    var DataEstaGroupPop = {
        /*入学年份默认值*/
        GetFullYear: function (callback){
            var stageNum = ~~$('#stage').val();
            var stageTextArr = ['小', '初', '高'];
            var funYear = function (num){
                var
                    newTime = new Date(),
                    GetFullYear = newTime.getFullYear(),
                    Time = [],
                    i = 0;
                while (i < num+1) {
                    i++;
                    Time.push(GetFullYear--);
                }
                Esta.stage = num;
                Esta.stageText = stageTextArr[stageNum - 1];
                callback.call(this, Time, num);
            };
            switch (stageNum) {
                case 1:
                    funYear(Esta.learningStage[0]);
                    break;
                case 2:
                    funYear(Esta.learningStage[1]);
                    break;
                case 3:
                    funYear(Esta.learningStage[2]);
                    break;
                default :
                    S.msg('没有此学段，数据错误');
            }
        },
        subjects: false,
        getSubjects: function (callback){
            if(DataEstaGroupPop.subjects){
                callback.call(this, DataEstaGroupPop.subjects);
                return;
            }
            $.ajax({
                url: singer.sites.apps + '/ea/LoadSubjects',
                type: 'Post',
                success: function (rec){
                    DataEstaGroupPop.subjects = rec;
                    callback.call(this, rec);
                }
            });
        },
        hrefGet: singer.sites.apps + '/ea/CreateGroupForManage',
        agencyUsers: false,
        getAgencyUsers: function (callback, subId, groupId){
//            if(DataEstaGroupPop.agencyUsers){
//                callback.call(this, DataEstaGroupPop.agencyUsers);
//                return;
//            }
            var groupType = $('.dy-list').data('type');
            $.post(singer.sites.apps + '/ea/LoadUsersByAgencyId', {
                groupId: groupId, subjectId: subId, groupType: groupType
            }, function (data){
                DataEstaGroupPop.agencyUsers = data;
                callback.call(this, data);
            });
        }
    };
    /**
     * 批量上传数据处理
     */
    var bindEstaGroup = function (){
        /**
         * 批量上传后台数据格式
         */
        var DataBatchGroup = {
            ClassGroups: [],
            ColleagueGroups: []
        };
        /*班级圈数据组装*/
        var $groupNum = $('#groupNum'),
            $startTime = $('#group-start'),
            classCount = parseInt($groupNum.val()),
            $startTimeval = $startTime.val(),
            $GradeYearCreateval = parseInt($('#GradeYearCreate').val()),
            classGroupNum = ~~$('.classnum').val();
        if(classCount > 0){
            for (var i = 0; i < classCount; i++) {
                DataBatchGroup.ClassGroups.push({
                    name: classGroupNum > 1 ? $startTimeval + (classGroupNum + i) + '班' : $startTimeval + (i + 1) + '班',
                    GradeYear: $GradeYearCreateval
                });
            }
        }
        /*同事圈数据组装*/
        var $obj = $('.itme-subjects .item-form').find('input').siblings('span');
        $obj.each(function (){
            var $t = $(this);
            if($t.prev('input').is(':checked')){
                DataBatchGroup.ColleagueGroups.push({
                    name: $t.text(),
                    SubjectId: $t.data('subjectid')
                });
            }
        });
        if($.isEmptyObject(DataBatchGroup)){
            S.msg('请创建圈子');
            return false;
        }
        /*创建圈子*/
        var getGroup = function (){
            $.ajax({
                    url: '/ea/BatchCreateGroups',
                    type: 'POST',
                    dataType: 'json',
                    data: DataBatchGroup
                })
                .done(function (){
                    S.msg('创建成功');
                    location.reload();
                })
        };
        /*圈子去重*/
        $.ajax({
                url: '/ea/LoadRepeatMsg',
                type: 'POST',
                dataType: 'json',
                data: DataBatchGroup
            })
            .done(function (json){
                if(json.status){
                    Esta.dialogGroup.BatchGroup.close().remove();
                    /*重复圈子处理*/
                    var html = template('pop-create-confirm', json.data.groupsSuccess);
                    S.dialog({
                        title: '确认提示',
                        content: html,
                        okValue: '跳过重复圈子，继续创建',
                        cancelValue: '取消',
                        ok: function (){
                            getGroup();
                            this.close().remove();
                        },
                        cancel: function (){
                            this.close().remove();
                        }
                    }).showModal();
                } else {
                    Esta.dialogGroup.BatchGroup.close().remove();
                    getGroup();
                }
            })
    };
    /**
     * type 1 班级  2同事圈
     * @param astage
     */
    var bindItemForm = function (astage, type){
        /*同事圈赋值*/
        var arr = [],
            html = '',
            str = '';
        $('.item-form .checkbox-group').find('span').each(function (){
            var StrSpanText = S.getStr($(this).text(), '级')[1];
            arr.push(StrSpanText);
        });
        if(type == 1){
            for (var j = 0; j < arr.length; j++) {
                html += '<li>';
                html += '<label class="checkbox-group group-checkbox">';
                html += '<input type="checkbox" name="options"><span data-subjectId="">' + astage + arr[j] + str + '</span>';
                html += '<i class="iconfont dy-icon-checkbox"></i>';
                html += '</label>';
                html += '</li>';
            }
        }
        if(type == 2){
            arr = [];
            str = '组';
            S.each(DataEstaGroupPop.subjects, function (value, key){
                arr.push({
                    text: value,
                    id: key
                });
            });
//            for (var attr in DataEstaGroupPop.subjects) {
//                if(!DataEstaGroupPop.subjects.hasOwnProperty(attr))
//                    continue;
//                arr.push({
//                    text: DataEstaGroupPop.subjects[attr],
//                    id: attr
//                });
//            }
            for (var i = 0; i < arr.length; i++) {
                html += '<li>';
                html += '<label class="checkbox-group group-checkbox">';
                html += '<input type="checkbox" name="options"><span data-subjectId="' + arr[i].id + '">' + astage + arr[i].text + str + '</span>';
                html += '<i class="iconfont dy-icon-checkbox"></i>';
                html += '</label>';
                html += '</li>';
            }
        }
        $('.item-form').html(html);
        $('.checkall').prop('cheched', false);
        var $ocheckbox = $('.itme-subjects .group-checkbox').find('.dy-icon-checkbox');
        $ocheckbox.removeClass('dy-icon-checkboxhv');
    };
    /**
     *Excel 批量导入学生
     */
    var addExcelFun = function (groupId, fn){
        /**
         * Excel导入班级成员圈
         */
        var htmlFile = '<form name="form" id="form-file"  method="post">'
            + '<div class="wrap-file f-cb">'
            + '<input type="file" name="fileExcel" accept="application/vnd.ms-excel"  class="input-file" style="display:none">'
            + '<div class="box-file">'
            + '<div class="input-file-text f-toe" title="请上传office2003格式的excel文件">请上传office2003格式的excel文件</div><span class="uploadBtn dy-btn dy-btn-info">Excel上传</span>'
            + '</div>'
            + '</div>'
            + '</form>';
        //上传效
        $document
            .off('.inputFile')
            .on('click.inputFile', '.uploadBtn', function (){
                $('.input-file').click();
            })
            .on('change.inputFile', '.input-file', function (){
                var val = $(this).val();
                $('.input-file-text').text(val).attr('title', val);
            });
        $('.excel-in').on('click', function (){
            Esta.dialogGroup.estaImportPop.close();
            S.dialog({
                title: "Excel学生名单导入",
                content: htmlFile,
                okValue: '确定',
                ok: function (){
                    var _this = this,
                        valPath = $('.input-file').val();
                    $('#form-file').ajaxSubmit({
                        type: 'post',
                        url: '/ea/ExcelBatchImportStudents',
                        data: {'groupId': groupId, 'role': 2},
                        beforeSubmit: function (){
                            if(valPath != ''){
                                var reg = /^.*\.(?:xls|xlsx)$/i;
                                if(!reg.test(valPath)){
                                    S.msg("文件格式必须是:xls");
                                    return false;
                                }
                            } else {
                                S.msg('未选择Excel文件');
                                return false;
                            }
                        },
                        success: function (json){
                            _this.close().remove();
                            fn(json);
                        }
                    });
                    return false;
                },
                cancelValue: '取消',
                cancel: function (){
                    Esta.dialogGroup.estaImportPop.showModal();
                    this.close().remove();
                }
            }).showModal();
        });
    };
    /**
     * 创建圈子
     */
    $document
    /**
     * 创建圈子弹框
     */
        .on('click', '.btn-esta-group', function (){
            DataEstaGroupPop.getSubjects(function (subjects){
                DataEstaGroupPop.GetFullYear(function (year){
                    var HtmlEstaGroupPop = template('esta-group-pop', {
                        GetFullYear: year,
                        subjects: subjects
                    });
                    Esta.dialogGroup.estaGroupPop = S.dialog({
                        fixed: true,
                        title: "创建圈子",
                        content: HtmlEstaGroupPop,
                        okValue: "确认创建",
                        cancelValue: '取消',
                        ok: function (){
                            var _this = this,
                                group = {
                                    name: "",
                                    type: 0
                                };
                            /**
                             * type  0 班级圈  1同事圈
                             */
                            group.type = $("input[name='typeGroup']:checked").val();
                            group.name = $("#GroupName").val();
                            if(group.name == ''){
                                S.msg('请输入圈子名称');
                                return false;
                            }
                            switch (group.type) {
                                case "0":
                                    group.gradeYear = $("#GradeYear").val();
                                    break;
                                case "1":
                                    group.subjectId = $("#SubjectId").val();
                                    break;
                            }
                            $.post(DataEstaGroupPop.hrefGet, group, function (json){
                                if(json.status){
                                    S.msg("创建成功", 2000, function (){
                                        location.reload(true);
                                    });
                                    _this.close().remove();
                                } else {
                                    S.msg(json.message);
                                }
                            });
                            return false;
                        },
                        cancel: function (){
                            this.close().remove();
                        }
                    }).showModal();
                });
            });
        })
        /**
         * 批量创建圈子
         */
        .on('click', '#batch-esta-group', function (event){
            event.preventDefault();
            /*初始化加载数据*/
            DataEstaGroupPop.GetFullYear(function (year, EntranceSection){
                var DataBatchCreate = {
                    GetFullYear: year,
                    classNameInit: {
                        BeginTime: year[0] + EntranceSection
                    },
                    stageText: Esta.stageText,
                    SubjectId: DataEstaGroupPop.subjects
                };
                var BatchGroup = template('pop-batch-create', DataBatchCreate);
                Esta.dialogGroup.estaGroupPop.close().remove();
                Esta.dialogGroup.BatchGroup = S.dialog({
                    title: "批量创建圈子",
                    content: BatchGroup,
                    okValue: '确认创建',
                    ok: function (){
                        bindEstaGroup();
                        return false;
                    },
                    cancelValue: '取消',
                    displayCancel: false,
                    cancel: function (){
                        this.close().remove();
                    }
                }).showModal();
            });
        })
        /**
         * 批量添加人员
         * @constructor
         */
        .on('click', '.grou-personnel', function (event){
            event.preventDefault();
            var groupId = $(this).attr("groupid");
            var subid = $(this).data('subid');
            var bindData = function (users){
                /*选择班级圈 同事圈类型*/
                var titleName = '';
                var database = {
                    agencyUsers: users
                };
                var BatchGroup = template('batch-group-wrap-pop', database);
                //添加成员弹框
                !function (callback){
                    /**
                     * 学生批量导入回调数据展示
                     * @param json
                     */
                    var addStudentsPopHtml = function (json){
                        if(json.status){
                            var $htmlSuccess = '',
                                repeatCount = json.data.repeatCount,
                                repeatUsers = json.data.repeatUsers,
                                messageCount = json.data.messageCount;
                            if(repeatCount == 0 && repeatUsers == null){
                                $htmlSuccess += S.format('<div class="esta-group-success-pop"><div class="esta-group-success "><p class="mb10"><span class="font-color">{0}</span>位新同学已导入成功！</p><p>学生可使用得一号登录，默认密码为“<span class="font-color">123456</span>”</p> </div></div>', messageCount);
                            } else {
                                $htmlSuccess += '<div class="pop-create-repeat">';
                                $htmlSuccess += '<p class="mb10"><span class="font-color">' + messageCount + '</span>位新同学已导入成功！</p>';
                                $htmlSuccess += '<p class="mb20">学生可使用得一号登录，默认密码为“<span class="font-color">123456</span>”</p>';
                                if(!(repeatCount == 0)){
                                    $htmlSuccess += '<div class="mb10">有' + repeatCount + '位同学已在该班级圈中，不能重复导入：</div>';
                                    $htmlSuccess += '<ul class="ul-list-name">';
                                    for (var i = 0; i < repeatUsers.length; i++) {
                                        $htmlSuccess += '<li>' + repeatUsers[i] + '</li>';
                                    }
                                    $htmlSuccess += '</ul>';
                                    $htmlSuccess += '</div>';
                                }
                            }
                            S.alert($htmlSuccess, function (){
                                location.reload();
                            });
                        } else {
                            S.msg(json.message);
                        }
                    };
                    Esta.dialogGroup.estaImportPop = S.dialog({
                        title: titleName,
                        content: BatchGroup,
                        okValue: '确定',
                        ok: function (){
                            var role,
                                addStudents,
                                addTeachers;
                            /*批量添加学生*/
                            addStudents = function (){
                                var dto = {
                                    groupId: groupId,
                                    users: S.textArea($('.text-area').val()),
                                    role: 2
                                };
                                if(!dto.users.length){
                                    S.msg('你还没有添加学生');
                                    return false;
                                }
                                var nameFlase = [];
                                for (var i = 0; i < dto.users.length; i++) {
                                    if(!(S.checkName(dto.users[i]))){
                                        nameFlase.push(dto.users[i]);
                                    }
                                }
                                if(nameFlase.length){
                                    S.msg('姓名错误:' + nameFlase.splice(','));
                                    return false;
                                }
                                var postAction = function (){
                                    $.post(singer.sites.apps + '/ea/BatchImportStudents', {data: dto}, function (json){
                                        Esta.dialogGroup.estaImportPop.close().remove();
                                        addStudentsPopHtml(json);
                                    });
                                };
                                S.pageArrRepeat(dto.users, function (repeatName){
                                    if(repeatName && repeatName.length){
                                        S.msg('姓名输入重复：' + repeatName);
                                        return false;
                                    }
                                    postAction();
                                });
                            };
                            /*批量添加教师*/
                            addTeachers = function (){
                                var postData = {
                                    ids: [],
                                    groupId: groupId
                                };
                                var teachers = [];
                                var url = '/ea/ColleagueBatchTeacher';
                                $('.table-teacher').find('input[type=checkbox]:checked').each(function (){
                                    var $td = $(this).parents('td');
                                    var $valuename = $td.siblings('.value-name');
                                    var teacherId = $valuename.data('teacherid');
                                    var subjectText = $td.siblings('.subject-name').text();
                                    var teacherText = $valuename.text();
                                    postData.ids.push(teacherId);
                                    teachers.push({
                                        subject: subjectText,
                                        name: teacherText
                                    });
                                });
                                /*班级圈(添加教师)*/
                                if($('.dy-list').data('type') == 0){
                                    var nary = teachers.sort(function (a, b){
                                        return a.subject > b.subject ? -1 : 1;
                                    });
                                    var repeatData = [], repeatIndex = [];
                                    for (var i = 0; i < nary.length - 1; i++) {
                                        if(nary[i].subject == nary[i + 1].subject){
                                            if(!S.inArray(i, repeatIndex)){
                                                repeatData.push(nary[i]);
                                                repeatIndex.push(i);
                                            }
                                            repeatData.push(nary[i + 1]);
                                            repeatIndex.push(i + 1);
                                        }
                                    }
                                    if(repeatData.length){
                                        var $html = '';
                                        $html += '<div class="pop-create-repeat">';
                                        $html += '<p class="mb10"><span class="font-color"></span>导入失败！</p>';
                                        $html += '<div class="mb10">一个班一个学科只能有一位任课老师以下为重复导入老师。</div>';
                                        $html += ' <table class="dy-table">';
                                        $html += '<tbody>';
                                        for (var i = 0; i < repeatData.length; i++) {
                                            $html += '<tr><td>' + repeatData[i].name + '</td><td>' + repeatData[i].subject + '</td></tr>';
                                        }
                                        $html += '</tbody>';
                                        $html += '</table>';
                                        $html += '</div>';
                                        Esta.dialogGroup.estaImportPop.close();
                                        S.alert($html, function (){
                                            Esta.dialogGroup.estaImportPop.showModal();
                                        });
                                        return false;
                                    }
                                    url = '/ea/BatchTeachers';
                                }
                                $.post(singer.sites.apps + url, postData, function (json){
                                    Esta.dialogGroup.estaImportPop.close().remove();
                                    if(json.status){
                                        var $htmlSuccess = S.format('<div class="esta-group-success-pop"><div class="esta-group-success "><p class="mb10"><span class="font-color">{0}</span>位老师已导入成功！</p></div></div>', json.data.messageCount);
                                        S.alert($htmlSuccess, function (){
                                            location.reload();
                                        });
                                    } else {
                                        S.msg(json.message);
                                    }
                                });
                            };
                            /*获取提交类型*/
                            role = $('.tab-nav-item').find('li.on').data('role');
                            /*选项卡提交： 角色 1  学生提交  2 教师提交*/
                            switch (role) {
                                case 1:
                                    addStudents();
                                    break;
                                case 2:
                                    addTeachers();
                                    break;
                            }
                            return false;
                        },
                        cancelValue: '取消',
                        cancel: function (){
                            this.close().remove();
                        }
                    }).showModal();
                    callback && callback.call(this, groupId, addStudentsPopHtml);
                }(addExcelFun);
            };
            DataEstaGroupPop.getAgencyUsers(bindData, subid, groupId);
            return false;
        })
        .on({
            keyup: function (){
                var len = S.textArea($('.text-area').val()).length;
                $('.js-text-personnel').text(len);
            }
        }, '#nameText');
    <!--==============================SHOW=================================-->
    $('.tab-nav-manage').on('click', '.j-li-tab', function (event){
        event.stopPropagation();
        $(this).addClass('on').siblings().removeClass('on');
    });
    $document
    /**
     * 弹框选项卡
     */
        .on('click', '.tab-nav-item li', function (event){
            event.preventDefault();
            var $t = $(this);
            var index = $t.index();
            $t.addClass('on').siblings().removeClass('on');
            $t.parents('.batch-group-pop').find('.tab-con-item .con-item')
                .css('display', 'none')
                .eq(index).css('display', 'block');
        })
        /**
         * 班级圈同事圈切换
         */
        .on('click', '.esta-group .list-item .checkbox-group', function (){
            var $t = $(this);
            $('.d-toggle').find('.d-toggle-box').css('display', 'none').eq($t.index()).css('display', 'block')
        })
        /**
         * 数量改变效果
         */
        .on({
            keyup: function (){
                S.inputCheck($(this)[0], 20);
                var $t = $(this),
                    $amendon = $('.amend-on'),
                    $groupBegin = $('#group-begin'),
                    groupNum = $.trim($t.val()),
                    classGroupNum = ~~$('.classnum').val();
                $groupBegin.data("classNumber", groupNum);
                if($.isNumeric(groupNum) && groupNum !== ''){
                    $amendon.removeClass('hide');
                    groupNum == 1 ? $amendon.addClass('on') : $amendon.removeClass('on');
                    $groupBegin.val((~~groupNum + classGroupNum - 1) + '班');
                } else {
                    $amendon.addClass('hide');
                }
            }
        }, '#groupNum')
        /**
         * 批量修改名字
         */
        .on('keyup', '#group-start', function (){
            bindItemForm($('#group-start').val(), 2);
        })
        /**
         *入学年份改变
         */
        .on('change', '#GradeYearCreate', function (){
            var $t = $(this),
                val = S.getStr($t.val(), '级')[0],
                astage = parseInt(val) + Esta.stage;
            // 班级圈赋值
            $('#group-start').val(Esta.stageText + astage + '级');
            $('#group-begin').val($('#groupNum').val() + '班');
            //同事圈
            bindItemForm(Esta.stageText + astage + '级', 2);
        })
        /**
         * 输入班级
         */
        .on('keyup', '.classnum', function (event){
            event.preventDefault();
            var $t = $(this),
                $groupBegin = $('#group-begin');
            S.inputCheck($t[0], 50);
            if($t.val() !== ''){
                $groupBegin.val((~~$t.val() + (~~$groupBegin.data('classNumber')) - 1) + '班');
            }
        })
        /**
         * 全选效果
         */
        .on('click', '.checkall', function (){
            var $ocheckbox = $('.itme-subjects .group-checkbox').find('.dy-icon-checkbox');
            var checkedall;
            checkedall = function (off){
                $('.itme-subjects').find("input[name='options']").each(function (){
                    $(this).prop("checked", off);
                });
            };
            if(checkboxOFF){
                checkedall(true);
                $ocheckbox.addClass('dy-icon-checkboxhv');
                checkboxOFF = false;
            } else {
                checkedall(false);
                $ocheckbox.removeClass('dy-icon-checkboxhv');
                checkboxOFF = true;
            }
        })
        .on({
            click: function (event){
                event.preventDefault();
                var
                    $wrapTeache = $("#search-member-con"),
                    $tbody = $wrapTeache.find('tbody'),
                    $tr = $tbody.find('tr'),
                    $teacherList = $('.teacher-list'),
                    valSearch = $('#search-member').val(),
                    $nothing = $teacherList.find('.dy-nothing');
                $tr.find('.dy-icon-checkbox').removeClass('dy-icon-checkboxhv');
                $('.table-teacher :checkbox').attr("checked", false);
                $('.teacher-selected').text(0);
                $nothing.remove();
                if(valSearch == '快速搜索'){
                    $wrapTeache.find('tbody').find('tr').removeClass('hide');
                    return false;
                }
                $tr.each(function(index,item){
                   var $t =$(item),
                       name = $t.find('td.value-name').html();
                    $t.toggleClass('hide',name.indexOf(valSearch)<0);
                });
                if($tbody.find('tr:visible').length >0){
                    $nothing.remove();
                } else {
                    $tr.addClass('hide');
                    var htmlNoThing = S.showNothing({
                        word: '没有此老师'
                    });
                    $teacherList.append(htmlNoThing);
                }
            }
        }, '#search-member-btn')
        /*添加教师选中人数*/
        .on('click', '.teacher-list input[type=checkbox]', function (){
            var len = $('.teacher-list').find('tr').find('input[type=checkbox]:checked').length;
            $('.teacher-selected').text(len);
        });
    /**
     * 圈子认证
     */
    $('.j-auth').bind('click', function (){
        var $t = $(this),
            status = $t.data('status'),
            groupId = $t.parents('tr').data('gid');
        $t.disableField('稍后');
        var msg = (status
                ? '确认要<b class="text-primary" style="font-weight: 600;">认证</b>该圈子?'
                : '确定要<b class="text-danger" style="font-weight: 600;">拒绝</b>认证该圈子?'
        );
        S.confirm(msg, function (){
            $.ajax({
                url: '/ea/auth-group',
                type: 'post',
                data: {Id: groupId, isAuth: status},
                success: function (rec){
                    if(rec.status){
                        S.msg(status ? "认证成功" : "已拒绝", 2000, function (){
                            window.location.reload(true);
                        });
                    } else {
                        S.alert(rec.message);
                        $t.undisableFieldset();
                    }
                }
            });
        }, function (){
            $t.undisableFieldset();
        });
    });
    /**
     * 首尾增加选择器
     */
    $(".js-first-last").each(function (){
        var $thisChidren = $(this).children();
        $thisChidren.first().addClass("first-item");
        $thisChidren.last().addClass("last-item");
    });
    /**
     * 弹框-增加教师列表滚动条
     */
    $(".y-teacher-list").mCustomScrollbar({
        axis: "y",
        theme: "dark-thin",
        autoExpandScrollbar: true,
        advanced: {autoExpandHorizontalScroll: true}
    });
    $('.item-form').mCustomScrollbar({
        axis: "y",
        theme: "dark-thin",
        autoExpandScrollbar: true,
        advanced: {autoExpandHorizontalScroll: true}
    });

})(jQuery, SINGER);





