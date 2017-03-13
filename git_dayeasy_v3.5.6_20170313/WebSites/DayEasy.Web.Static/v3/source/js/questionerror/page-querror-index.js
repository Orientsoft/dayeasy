/**
 * Created by Plato on 2017/1/25.
 */
(function ($, S){
    S.pageLoading();
    S.mix(S, {
        /**
         * 问题展示
         * @param question
         */
        showQuestion: function (question){
            if(!question) return '';
            if($('#q-template').length == 0)
                return '缺少q-template模板';
            question.show_option = true;
            return template('q-template', question);
        },
        /**
         * 获取选项是否水平显示
         * @param options
         * @returns {boolean}
         */
        optionModelS: function (options){
            for (var i = 0; i < options.length; i++) {
                var item = options[i],
                    len = 0;
                //有公式
                if(item.Body.indexOf('\\[') >= 0) return true;
                len += (item.Images && item.Images.length) ? 18 : 0;
                if(S.lengthCn(item.Body) + len > 35) return false;
            }
            return true;
        },
    });
    template.helper('optionModel', function (data){
        return S.optionModelS(data) ? 'q-options-horizontal' : '';
    });
    var $window = $(window),
        $body = $('body'),
        error = {
            //GIT 请求链接
            urls: {
                //错题列表- 判断有误错题
                errorList: '/hospital/ErrorQuestions',
                // 任教班级列表
                inClass: '/hospital/groups',
                // 错题题型列表
                errorType: '/hospital/questionTypes',
                // 知识点筛选列表
                knowledgePoint: '/hospital/knowledges',
                // 学生列表
                errorUsers: '/hospital/errorUsers',
                /*string[]*/
                /*下载*/
                dowload: '/work/DowloadEqByQIds'
            },
            data: {
                errorList: {
                    navList: {
                        DateRange: -1,      // 日期范围
                        groupid: null,      // 圈子ID
                        QuestionType: -1,    // 试题类型
                        KnowledgeCode: 0,   // 知识点类型
                        OrderOfArr: -1,      // 时间排序
                        pageindex: 0,       // 请求页数
                        pagesize: 8,         // 请求数量
                        count: 0,              //总共条数
                        UserId: 0              //学生错题信息参数
                    },
                    knowledgePoint: {
                        DateRange: -1,
                        groupid: null,
                        QuestionType: -1,
                        UserId: 0              //学生错题信息参数
                    }
                },
                inClass: [],
                errorType: [],
                list: [], //储存记忆数据
                listId: []  //储存记忆数据
            },
            /**
             * 添加移除下载列表
             * @param id 错题ID
             * @returns {boolean}
             */
            refresh: function (questionid, id){
                for (var i = 0; i < error.data.list.length; i++) {
                    if(error.data.list[i] == questionid){
                        error.data.list.splice(i, 1);
                        error.data.listId.splice(i, 1);
                        return true;
                    }
                }
                if(error.data.list.length > 49 && error.data.listId > 49){
                    S.msg("已经选择了很多错题啦，先下载一份吧~~");
                    return false;
                }
                error.data.list.push(questionid);
                error.data.listId.push(id);
                return true;
            },
            /**
             * 清空下载列表
             */
            reset: function (){
                error.data.list = [];
                error.data.listId = [];
                $('#txtDwData').val('');
                $(".in-download").removeClass("dw-active").html('+ 加入下载');
            },
            /**
             * 刷新下载列表页面
             */
            refreshHtml: function (){
                var total = error.data.list.length,
                    $download = $(".box-fixed-download");
                $(".in-total").find('b').text(total);
                total ? $download.removeClass('hide') : $download.addClass('hide');
            },
            /**
             * 储存记忆数据对比
             * @param json
             * @returns {*}
             */
            jsonAddRefresh: function (json){
                for (var i = 0; i < json.data.length; i++) {
                    if(error.data.list.indexOf(json.data[i].QuestionId) == -1){
                        json.data[i].refresh = false;
                    } else {
                        json.data[i].refresh = true;
                    }
                }
                return json;
            }
        },
        errorApi = {
            /**
             * 错题列表
             * @param options error.data.errorList.navList
             * @param callback
             */
            getErrorList: function (options, callback){
                $('.main-qu-error').html('<div class="dy-loading"><i></i></div>');
                $.get(error.urls.errorList, options, function (json){
                    if(json.data.length){
                        var $html = template('main-box', error.data.errorType);
                        $('.main-qu-error').html($html);
                        S.loadFormula();
                        error.data.errorList.navList.count = json.count;
                        /*TAB调用*/
                        callback && callback.call(this);
                    } else {
                        $('.main-qu-error').html(template('class-no-wrong', {}));

                    }
                });
            },
            /**
             * 错题组装
             * @param options
             * @param callback
             */
            getErrorListCont: function (options, callback){
                var $conSubjectList = $('.con-subject-list');
                $conSubjectList.html('<div class="dy-loading"><i></i></div>');
                $.get(error.urls.errorList, options, function (json){
                    if(json.data.length){
                        $conSubjectList.html(template('con-subject-list', error.jsonAddRefresh(json)));
                        var $jsLoadMore = $('.js-load-more');
                        if(json.count > error.data.errorList.navList.pagesize){
                            $jsLoadMore.removeClass('hide');
                        } else {
                            $jsLoadMore.addClass('hide');
                        }
                        S.loadFormula();
                        error.data.errorList.navList.count = json.count;
                        callback && callback.call(this);
                    } else {
                        $('.con-subject-list').html(S.showNothing({
                            word: "没有相关错题"
                        }));
                    }
                })
            },
            /**
             * 任教班级列表
             */
            getInClasst: function (callback){
                $.get(error.urls.inClass, function (json){
                    S.each(json, function (item, key){
                        if(key == 0){
                            error.data.errorList.navList.groupid = item.Id;
                            error.data.errorList.knowledgePoint.groupid = item.Id;
                        }
                        error.data.inClass.push({
                            Name: item.Name,
                            groupid: item.Id
                        });
                    });
                    callback && callback.call(this);
                });
            },
            /**
             * NAV 错题题型列表
             */
            getErrorType: (function (){
                if(error.data.errorType.length > 0){
                    return false;
                }
                $.get(error.urls.errorType, function (json){
                    S.each(json, function (item, key){
                        error.data.errorType.push({
                            Id: item.Id,
                            Name: item.Name
                        });
                    });
                });
            })(),
            /**
             * NAV 知识点筛选列表
             * @param options error.data.errorList.knowledgePoint
             * @param callback
             */
            grtKnowledgePoint: function (options, callback){
                $.get(error.urls.knowledgePoint, options, function (json){
                    var
                        showNumber = 5,
                        more = $('.js-more-conten'),
                        html = template('Knowledge-Point-Box', json),
                        $html = $(html);
                    $('.js-more-btn').remove();
                    more.html($html);
                    more.prepend('<li class="on" data-code="0">全部</li>');
                    if(json.count > showNumber){
                        more.moreShow({number: showNumber});
                    }
                    callback && callback.call(this, json);
                });
            },
            /**
             * 学生用户信息列表
             */
            grtErrorUsers: function (options, callback){
                S.loading.start($('.student-inio'));
                $.get(error.urls.errorUsers, options, function (json){
                    var $html = template('student-inio', json);
                    $('.student-inio').html($html);
                    callback && callback.call(this);
                });
            },
            /**
             * 初次加载数据绑定界面
             */
            init: function (){
                /**
                 * Bing-错题列表
                 */
                errorApi.getInClasst(function (){
                    S.pageLoaded();
                    if(error.data.inClass.length){
                        var $html = $(template('nva-menu', error.data.inClass));
                        $html.find('li:eq(0)').addClass('on');
                        $('.sidebar-question-error .nva-menu').html($html);
                        /**
                         * 错题列表
                         */
                        errorApi.getErrorList(error.data.errorList.navList, function (){
                            errorApi.grtKnowledgePoint(error.data.errorList.knowledgePoint, function (){
                                errorApi.getErrorListCont(error.data.errorList.navList);
                                $(".htmleaf-containers").tabcont();
                                //学生列表
                                errorApi.grtErrorUsers({"groupid": error.data.errorList.navList.groupid})
                            });
                        });
                    } else {
                        var $pageQuestionerrorIndex = $('.page-questionerror-index');
                        $pageQuestionerrorIndex.addClass('add-Class-not');
                        $pageQuestionerrorIndex.html(S.showNothing({
                            icon: 'dy-icon-no add-Class-icon',
                            word: '你还未加入任何班级，无法查看班级错题',
                            css: {height: "400px", lineHeight: '400px'}
                        }));
                    }
                });
            }
        };
    S.mix(S, {
        errorApi: errorApi,
        error: error
    });
    S.errorApi.init();
    /*隐藏-展开*/
    $.fn.moreShow = function (options){
        if(!this.length){
            return this;
        }
        var defaults = {
                number: 5,
                btn: '.js-more-btn'
            },
            opts = $.extend(true, {}, defaults, options);
        this.each(function (){
            var $this = $(this),
                $category = $this.find('li:gt(' + opts.number + ')'),
                $body = $('body'),
                $btn = $(opts.btn);
            if(!$category.length){
                $btn.remove();
                return false;
            }
            $category.hide();
            if(!($('#list-knowledge').find('.js-more-btn').length)){
                $('#list-knowledge').prepend('<b class="more js-more-btn">展开 <i class="iconfont dy-icon-anglebottom"></i></b>');
            }
            $body.on('click', opts.btn, function (){
                if($category.is(':visible')){
                    $category.hide();
                    $btn.html('展开 <i class="iconfont dy-icon-anglebottom"></i>')
                } else {
                    $category.show();
                    $btn.html('收起 <i class="iconfont dy-icon-angletop"></i>')
                }
                return false;
            });
        });
        return this;
    };
    $body
    /**
     * 任教班级切换
     */
        .on('click', '.nva-menu li', function (){
            var $this = $(this),
                groupid = $this.find('a').data('groupid');
            $this.addClass('on').siblings().removeClass('on');
            $.extend(error.data.errorList, {
                navList: {
                    DateRange: -1,      // 日期范围
                    groupid: groupid,      // 圈子ID
                    QuestionType: -1,    // 试题类型
                    KnowledgeCode: 0,    // 知识点类型
                    count: 0,            // 总共条数
                    OrderOfArr: -1,      // 时间排序
                    pageindex: 0,        // 请求页数
                    pagesize: 8,         // 请求数量
                    UserId: 0            // 学生错题信息参数
                },
                knowledgePoint: {
                    DateRange: -1,
                    groupid: groupid,
                    QuestionType: -1,
                    UserId: 0              //学生错题信息参数
                }
            });
            /**
             * 错题列表
             */
            errorApi.getErrorList(error.data.errorList.navList, function (){
                errorApi.grtKnowledgePoint(error.data.errorList.knowledgePoint, function (){
                    errorApi.getErrorListCont(error.data.errorList.navList, function (){
                        $(".htmleaf-containers").tabcont();
                        //学生列表
                        errorApi.grtErrorUsers({"groupid": error.data.errorList.navList.groupid});
                    });
                });
            });
        })
        /**
         * 知识点筛选-组装
         */
        .on('click', '.conten-list-text li', function (){
            var $this = $(this);
            if($this.hasClass('on')){
                return false;
            }
            var type = ~~$this.parents('.conten-list-text').siblings('.data-type').data('type');
            $this.addClass('on').siblings().removeClass('on');
            error.data.errorList.navList.pageindex = 0;
            if(type == 1){
                error.data.errorList.navList.DateRange = ~~$this.data('value');
                error.data.errorList.knowledgePoint.DateRange = ~~$this.data('value');
            }
            if(type == 2){
                error.data.errorList.navList.QuestionType = ~~$this.data('errortypeid');
                error.data.errorList.knowledgePoint.QuestionType = ~~$this.data('errortypeid');
            }
            if(type == 3){
                error.data.errorList.navList.OrderOfArr = ~~$this.data('time');
            }
            if(type == 4){
                error.data.errorList.navList.KnowledgeCode = ~~$this.data('code');
            }
            /*知识点筛选*/
            if(type == 1 || type == 2){
                error.data.errorList.navList.KnowledgeCode = 0;
                errorApi.grtKnowledgePoint(error.data.errorList.knowledgePoint, function (json){
                    var $knowledgePoint = $('.knowledge-point');
                    if(!json.data.length){
                        $knowledgePoint.fadeOut();
                    } else {
                        $knowledgePoint.fadeIn();
                    }
                });
            }
            $('.js-load-more').addClass('hide');
            /*错题列表*/
            errorApi.getErrorListCont(error.data.errorList.navList);
        })
        /**
         * 学生列表跳转
         */
        .on('click', '.list-inio', function (){
            $window.unbind("scroll.getData");
            var $t = $(this),
                dataUser = $t.data('user-inio'),
                ConfigJSON = eval("(" + dataUser + ")");
            $('.htmleaf-content').find('.dy-tab-nav li').eq(0).trigger('click');
            /*****题目列表*****/
            var groupid = $('.sidebar-question-error .nva-menu').find('li.on').find('a').data("groupid");
            $.extend(error.data.errorList, {
                navList: {
                    DateRange: -1,      // 日期范围
                    groupid: groupid,      // 圈子ID
                    QuestionType: -1,    // 试题类型
                    KnowledgeCode: 0,    // 知识点类型
                    count: 0,            // 总共条数
                    OrderOfArr: -1,      // 时间排序
                    pageindex: 0,        // 请求页数
                    pagesize: 8,         // 请求数量
                    UserId: ConfigJSON.Id            // 学生错题信息参数
                },
                knowledgePoint: {
                    DateRange: -1,
                    groupid: groupid,
                    QuestionType: -1,
                    UserId: ConfigJSON.Id              //学生错题信息参数
                }
            });
            /**
             * 错题列表
             */
            errorApi.getErrorList(error.data.errorList.navList, function (){
                $('.htmleaf-content').addClass('hide');
                var $html = template('user-info', ConfigJSON);
                $('.main-qu-error').prepend($html);
                errorApi.grtKnowledgePoint(error.data.errorList.knowledgePoint, function (){
                    errorApi.getErrorListCont(error.data.errorList.navList, function (){
                        //学生列表
                        errorApi.grtErrorUsers({"groupid": error.data.errorList.navList.groupid});
                    });

                });
            });
        })
        /**
         * 返回学生列表
         */
        .on('click', '.user-info .return', function (){
            /**
             * 学生列表重新更新
             */
            var groupid = $('.sidebar-question-error .nva-menu').find('li.on').find('a').data("groupid");
            error.data.errorList.navList.pageindex = 0;
            $.extend(error.data.errorList, {
                navList: {
                    DateRange: -1,      // 日期范围
                    groupid: groupid,      // 圈子ID
                    QuestionType: -1,    // 试题类型
                    KnowledgeCode: 0,    // 知识点类型
                    count: 0,            // 总共条数
                    OrderOfArr: -1,      // 时间排序
                    pageindex: 0,        // 请求页数
                    pagesize: 8,         // 请求数量
                    UserId: 0            // 学生错题信息参数
                },
                knowledgePoint: {
                    DateRange: -1,
                    groupid: groupid,
                    QuestionType: -1,
                    UserId: 0
                }
            });

            /**
             * 错题列表
             */
            errorApi.getErrorList(error.data.errorList.navList, function (){
                //                        //学生列表
                $(".htmleaf-containers").tabcont();
                $('.htmleaf-content').find('.dy-tab-nav li').eq(1).trigger('click');
                errorApi.grtErrorUsers({"groupid": error.data.errorList.navList.groupid});
                errorApi.grtKnowledgePoint(error.data.errorList.knowledgePoint, function (){
                    errorApi.getErrorListCont(error.data.errorList.navList);
                });
            });
        })
        /**
         * 添加删除下载
         */
        .on('click', '.in-download', function (){
            //下载
            var $t = $(this),
                questionid = $t.data("questionid"),
                id = $t.data("id");
            if(error.refresh(questionid, id)){
                error.refreshHtml();
                $t.hasClass("dw-active") ? $t.removeClass("dw-active").html('+ 加入下载') : $t.addClass("dw-active").html('x 移除下载');
            }
        })
        /**
         * 下载
         */
        .on('click', '.btn-question-download', function (){
            if(error.data.listId.length < 1){
                S.msg("请选择错题");
                return;
            }
            $("#txtDwData").val(encodeURIComponent(S.json(error.data.listId)));
            $("#dwForm").submit();
            S.dialog({
                title: "下载提示",
                content: "请问您下载成功了吗？<br/>确定下载成功将<span style='color:#ffab00'>清空</span>错题下载列表",
                fixed: true,
                backdropOpacity: .7,
                okValue: "是的",
                cancelValue: "还没有",
                ok: function (){
                    error.reset();
                    error.refreshHtml();
                },
                cancel: function (){
                }
            }).showModal();
        })
        /**
         * 一键清除储存记忆数据
         */
        .on('click', '.btn-key-clear', function (){
            S.confirm('确定<span style="color:#ffab00">清空</span>错题下载列表', function (){
                error.reset();
                error.refreshHtml();
            })
        })
        /**
         * 加载更多错题数据
         */
        .on('click', '.js-load-more', function (){
            var notData,
                $conSubjectList = $('.con-subject-list'),
                length = $conSubjectList.find('.tab-con-subject'),
                $jsLoadMore = $('.js-load-more');
            $jsLoadMore.addClass('hide');
            if(!length){
                return false;
            }
            if($('.tab-con').find('.student-list-item').is(':hidden')){
                error.data.errorList.navList.pageindex = ++error.data.errorList.navList.pageindex;
            }
            $conSubjectList.append('<div class="dy-loading"><i></i></div>');
            //没有更多数据了
            notData = function (){
                var lengthList = $('.con-subject-list').find('.tab-con-subject').length,
                    notWarning = $conSubjectList.find('div').hasClass('box-not-warning');
                if(lengthList > 0 && !notWarning){
                    S.loading.done($conSubjectList);
                    $conSubjectList.append('<div class="f-tac box-not-warning"><i class="iconfont dy-icon-warning"></i>没有更多数据了</div>');
                }
            }
            $.get(error.urls.errorList, error.data.errorList.navList, function (json){
                if(json.data.length){
                    var
                        count = json.count,
                        pageindex = error.data.errorList.navList.pageindex,
                        pagesize = error.data.errorList.navList.pagesize,
                        $html = template('con-subject-list', error.jsonAddRefresh(json));
                    S.loading.done($conSubjectList);
                    $conSubjectList.append($html);
                    S.loadFormula();
                    if((pageindex + 1) * pagesize >= count){
                        notData();
                    } else {
                        $jsLoadMore.removeClass('hide');
                    }
                } else {
                    notData();
                    return false;
                }
            });

        });
})(jQuery, SINGER);
/*
 http://apps.dayeasy.dev/hospital
 */
/*
 576119456@qq.com
 576119456@qq.com
 */
