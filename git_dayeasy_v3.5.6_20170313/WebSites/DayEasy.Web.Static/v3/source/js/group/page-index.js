/**
 * 圈子内页
 * Created by shay on 2015/11/6.
 */
(function ($, S){
    var pageLoad,
        loadDynamics,
        loadTopics,
        loading = false,
        setLikeList,
        sendNotice,
        deleteMember,
        checkApply,
        rePublish,
        publishJoint,
        groupId,
        groupType,
        user,
        users = {},
        likeMax = 5,
        page = 0,
        size = 15,
        $dynamicList = $('.dynamic-list'),
        $topicList = $('.topic-list'),
        /**
         * 动态类型配置
         */
        dynamicType = {
            "0": {
                css: 'color-bg-green',
                type: '作业'
            },
            "1": {
                css: 'color-bg-green',
                type: '作业'
            },
            "2": {
                css: 'color-bg-orange',
                type: '通知'
            },
            "3": {
                css: 'color-bg-grassgreen',
                type: '辅导'
            },
            "4": {
                css: 'color-bg-yellow',
                type: '表扬'
            },
            "5": {
                css: 'color-bg-orange',
                type: '协同阅卷'
            },
            "6": {
                css: 'color-bg-orange',
                type: '成绩通知'
            }
        },
        currentState = 1,
        logger = S.getLogger('group-index');

    /**
     * 加载动态
     */
    loadDynamics = function (){
        if(loading)
            return false;
        loading = true;
        $.post('/message/dynamics', {
            groupId: groupId,
            page: page || 0,
            size: size || 15
        }, function (json){
            $dynamicList.find('.dy-loading').remove();
            if(json.status){
                var details = json.data.newsDetails,
                    userList = json.data.users,
                    role = json.data.userRole;
                for (var id in userList) {
                    if(!userList.hasOwnProperty(id))
                        continue;
                    users[id] = userList[id];
                }
                //组装user以及type
                if(details && details.length){
                    var $item, i;
                    for (i = 0; i < details.length; i++) {
                        var detail = details[i];
                        if(detail.dynamicType == 2 && !detail.title){
                            detail.title = '重要通知';
                        }
                        detail.sender = users[detail.userId];
                        var type = dynamicType[detail.dynamicType];
                        detail.typeCss = type.css;
                        detail.type = type.type;
                        detail.groupType = groupType;
                        if(detail.batch){
                            if(role === 2){
                                detail.link = S.sites.apps + '/work/pub-paper/' + detail.batch;
                            } else if(role === 4 && detail.userId === user.id){
                                detail.link = S.format(S.sites.apps + '/work/teacher/statistics-question?batch={0}&paper_id={1}', detail.batch, detail.paperId);
                            }
                        } else if(detail.paperId){
                            detail.link = S.sites.apps + '/paper/detail/' + detail.paperId;
                        } else if(detail.examId){
                            if(role === 2){
                                detail.link = S.sites.apps + '/ea/summary/' + detail.examId;
                            } else if(role === 4){
                                detail.link = S.sites.apps + '/ea/charts/' + detail.examId;
                            }
                        }
                        $item = $(template('dynamic-item', detail));
                        $item.data('goods', detail.goods || []);
                        $item.data('title', detail.title);
                        $item.data('id', detail.id);
                        $dynamicList.append($item);
                        setLikeList($item);
                    }
                }
                if(details.length == 0 || details.length < size){
                    if(page > 0){
                        $dynamicList.append('<div class="dy-ended">没有更多的动态</div>');
                    }
                    $(window).unbind('scroll.dynamic');
                }
            }
            if(!json.status || (page == 0 && !json.data.newsDetails.length)){
                $dynamicList.append('<div class="dy-nothing">暂时没有相关动态</div>');
                $(window).unbind('scroll.dynamic');
            }
            loading = false;
        });
    };
    /**
     * 加载帖子
     */
    loadTopics = function (){
        if(loading)
            return false;
        loading = true;
        $.get('/topic/search', {
            groupId: groupId,
            isPick: (currentState == 2),
            page: page || 0,
            size: size || 15
        }, function (json){
            $topicList.find('.dy-loading').remove();
            if(json.status){
                if(json.data && json.data.length){
                    for (var i = 0; i < json.data.length; i++) {
                        var item = json.data[i];
                        item.summary = S.stripTags(item.content).substr(0, 200);
                        item.userPhoto = S.makeThumb(item.userPhoto, 35, 35);
                        item.time = item.addedAt.substr(0, 10);
                        var topic = $(template('topic-temp', json.data[i]));
                        $topicList.append(topic);
                    }
                }
                if(json.data.length < size){
                    $(window).unbind('scroll.dynamic');
                }
                singer.loadFormula();
            }
            if(!json.status || (page == 0 && json.count == 0)){
                $topicList.append('<div class="dy-nothing">暂时没有相关帖子</div>');
            }
            loading = false;
        });
    };
    /**
     * 设置点赞人
     */
    setLikeList = function ($item, toggle){
        var goods = $item.data('goods');
        if(toggle){
            if(S.inArray(user.id, goods)){
                goods.splice(goods.indexOf(user.id), 1);
            } else {
                goods.push(user.id);
            }
            $item.data('goods', goods);
        }
        if(!goods || !goods.length){
            $item.find('.d-like-list').html('');
            return false;
        }
        //点赞人
        var likeList = [];
        for (var j = goods.length - 1; j >= 0; j--) {
            var id = goods[j] + '';
            likeList.push(users[id].name);
            if(likeList.length >= likeMax)
                break;
        }
        likeList = '<span>' + likeList.join('、') + '</span>';
        if(goods.length > likeMax){
            likeList += S.format('等{0}人', goods.length);
        }
        likeList += '觉得很赞';
        $item.find('.d-like-list').html('<i class="iconfont dy-icon-zan"></i>' + likeList);
    };
    /**
     * 发送通知
     */
    sendNotice = function (){
        var $btn = $('.btn-notice');
        $btn.blur().attr('disabled', 'disabled').addClass('disabled');
        var noticeDg = S.dialog({
            title: '发通知',
            content: $('.pop-notice').html(),
            ok: function (){
                var $node = $(this.node),
                    message = S.trim($node.find('textarea').val()),
                    $checked = $node.find('input[name="options"]:checked'),
                    classes = [];
                for (var i = 0; i < $checked.length; i++) {
                    classes.push($checked.eq(i).val());
                }
                if(!message || message.length > 140){
                    S.msg("请输入通知内容！");
                    //$btn.removeAttr('disabled').removeClass('disabled');
                    return false;
                }
                $.post('/message/send_notice', {
                    groupId: groupId,
                    message: message,
                    classes: classes.join(',')
                }, function (json){
                    if(json.status){
                        S.alert('发布成功！', function (){
                            location.reload(true);
                        });
                    } else {
                        S.alert(json.message);
                        $btn.removeAttr('disabled').removeClass('disabled');
                    }
                });
                return false;
            },
            okValue: '发　布',
            onclose: function (){
                $btn.removeAttr('disabled').removeClass('disabled');
            },
            onshow: function (){
                var $node = $(this.node),
                    $textarea = $node.find('textarea');
                $textarea.focus();
            },
            //align: 'right',
            width: 400
        }).showModal();
    };

    /**
     * 删除成员
     * @param obj
     */
    deleteMember = function (obj){
        var $t = $(obj),
            id = $t.data('id'),
            name = $t.data('name');
        S.confirm(S.format('确认移除圈成员【{0}】?', name), function (){
            $.post('/group/delete', {
                groupId: groupId,
                userId: id
            }, function (json){
                if(json.status){
                    S.alert('移除成功！', function (){
                        $t.remove();
                    });
                } else {
                    S.alert(json.message);
                }
            });
        });
        return false;
    };
    /**
     * 审核申请
     * @param id
     * @param pass
     */
    checkApply = function (id, pass){
        var message = '',
            verify;
        /**
         * 提交审核
         */
        verify = function (){
            $.post('/group/verify', {
                id: id,
                pass: pass,
                message: message
            }, function (json){
                if(json.status){
                    S.msg('操作成功！', 2000, function (){
                        location.reload(true);
                    });
                } else {
                    S.alert(json.message);
                }
            });
        };
        if(!pass){
            S.dialog({
                title: '拒绝申请',
                content: '<div>' +
                '<textarea placeholder="输入附加信息" name="" class="wf100 mb20 mt10" cols="30" rows="5" maxlength="100"></textarea>' +
                '<p class="dy-result" style="width: 320px;display:block;text-align: right">你还可以输入<em>100</em>个字</p>' +
                '</div>',
                ok: function (){
                    var $text = $(this.node).find('textarea');
                    if(!$text.val() || $text.val().length < 1){
                        S.msg('请输入附加信息！');
                        return false;
                    }
                    message = $text.val();
                    verify();
                },
                okValue: '确认拒绝'
            }).showModal();
        } else {
            verify();
        }
    };

    /**
     * 转推
     * @param paperId
     * @returns {boolean}
     */
    rePublish = function (paperId){
        if(!paperId)
            return false;
        $.ajax({
            url: S.sites.apps + '/paper/publishPaper',
            type: 'Post',
            data: {paperId: paperId, groupId: groupId},
            xhrFields: {
                withCredentials: true
            },
            //dataType: 'application/json; charset=utf-8',
            success: function (html){
                S.dialog({
                    title: '转推到我的圈子',
                    content: html
                }).showModal();
            }
        });
        //$.post(S.sites.apps + '/paper/publishPaper', {paperId: paperId, groupId: groupId}, function (html) {
        //    S.dialog({
        //        title: '转推到我的圈子',
        //        content: html
        //    }).showModal();
        //});
    };

    /**
     * 发布协同
     */
    publishJoint = function (){
        var publish, publishDialog;
        publish = function ($no){
            var paperNo = $no.val(),
                $error = $no.next();
            if(!paperNo || paperNo.length != 11){
                $no.focus();
                $error.html('请输入11位试卷编号！');
                return false;
            }
            $.post('/marking/publish-joint', {
                groupId: groupId,
                paperNo: paperNo
            }, function (json){
                if(!json.status){
                    $no.focus();
                    $error.html(json.message);
                    return false;
                }
                publishDialog.close().remove();
                S.confirm('发布成功,是否现在分配题目？', function (){
                    location.href = '/marking/allot/' + json.message;
                }, function (){
                    location.reload(true);
                });
            });
        };
        publishDialog = S.dialog({
            title: "发起协同阅卷",
            content: '<input type="text" id="paperNo" placeholder="试卷编号" style="width:300px;"/>' +
            '<p id="errorMsg" class="text-danger mt10"></p>',
            okValue: "发布",
            ok: function (){
                var $no = $(this.node).find('#paperNo');
                publish($no);
                return false;
            },
            cancelValue: "取消",
            cancel: function (){
            },
            width: 300
        });
        publishDialog.showModal();
    };

    /**
     * 页面加载
     */
    pageLoad = (function (){
        groupId = $('.home-circles').data("gid");
        var $main = $('.mian-conter');
        groupType = $main.data('type');
        user = {
            id: $main.data('user-id'),
            name: $main.data('user-name')
        };
        users[user.id] = user;
        if($dynamicList.length){
            loadDynamics();
        }
        if($topicList.length){
            loadTopics();
        }
    })();

    /**
     * 事件绑定
     */
    $(document)
        .ready(function (){
            var $overflowY = $('.ove-y');
            if($overflowY.length){
                $overflowY.mCustomScrollbar({
                    axis: "y",
                    theme: "dark-3"
                });
            }
        })

        .delegate('.btn-notice', 'click', function (){
            //通知
            sendNotice();
        })
        .delegate('.d-like', 'click', function (){
            //点赞
            var $t = $(this);
            var dynamicId = $t.parents('.box-list').data('id');
            if(!dynamicId)
                return false;
            $.post('/message/support', {
                id: dynamicId
            }, function (json){
                if(json.status){
                    $t.toggleClass('z-crt').html('<i class="iconfont dy-icon-zan"></i>' + json.data);
                    setLikeList($t.parents('.box-list'), true);
                }
            });
        })
        .delegate('.d-comment', 'click', function (){
            //评论
            var $t = $(this),
                $list = $t.parents('.box-list'),
                id = $list.data('id'),
                title = $list.data('title');
            //回调
            S.updateComment = function (num){
                var count = ~~$t.html().replace(/<[^>]*>/gi, '');
                num = (S.isNumber(num) ? num : 1);
                count += num;
                $t.html('<i class="iconfont dy-icon-messageline"></i>' + count);
            };
            $.get('/message/comment-list', {
                sourceId: id
            }, function (html){
                S.dialog({
                    title: title + ' - 评论',
                    content: html
                }).showModal();
            });
        })
        .delegate('.b-repub', 'click', function (){
            var paperId = $(this).data('pid');
            rePublish(paperId);
            return false;
        })
    ;
    $('.posa-box button').bind("click", function (){
        //审核
        var $t = $(this),
            id = $t.parents('li').data('id'),
            pass = $t.data('pass');
        checkApply(id, pass);
        return false;
    });
    $('.bm-delete').bind('click', function (e){
        e.stopPropagation();
        var $li = $(this).parents('li');
        deleteMember($li);
        return false;
    });

    //分享
    var $share = $('.d-share-wrap');
    $('.dt-share').click(function (){
        $share.toggleClass('hide');
        return false;
    });
    $share.find('.dy-icon-close').click(function (){
        $share.addClass('hide');
    });
    $('.dg-nav-list h2').bind('click', function (){
        var $t = $(this),
            state = $t.data('state');
        if($t.hasClass('active'))
            return false;
        $t.addClass('active').siblings().removeClass('active');
        page = 0;
        currentState = state;
        $topicList.empty().append('<div class="dy-loading"><i></i></div>');
        loadTopics();
    });
    $('#btn-joinMark').bind('click', function (){
        publishJoint();
        return false;
    });
    $(window).bind('scroll.dynamic', function (e){
        if(loading)
            return false;
        var winH = $(this).height();
        var pageH = $(document).height();
        var scrollT = $(this).scrollTop(); //滚动条top
        var left = pageH - winH - scrollT;
        if(left < 160){
            page++;
            //动态
            if($dynamicList.length){
                $dynamicList.append('<div class="dy-loading"><i></i></div>');
                loadDynamics();
            }
            //动态
            if($topicList.length){
                $topicList.append('<div class="dy-loading"><i></i></div>');
                loadTopics();
            }
        }
        return false;
    });

    /**
     * 协同阅卷状态辅助
     */
    template.helper('jointHelper', function (dynamic){
        if(!dynamic || dynamic.dynamicType != 5){
            return '';
        }
        switch (dynamic.jointStatus) {
            case 0:
                var isOwner = (dynamic.addedBy == user.id);
                if(!dynamic.distributed){
                    if(isOwner)
                        return S.format('<a class="dy-btn dy-btn-info" href="/marking/allot/{0}" target="_blank">分配任务</a>', dynamic.jointBatch);
                    return S.format('<a class="dy-btn dy-btn-default disabled">{0}</a>', '等待分配');
                }
                if(dynamic.hasMission || isOwner){
                    return S.format('<a class="dy-btn dy-btn-info" target="_blank" href="/marking/mission_v2/{0}">批阅</a>', dynamic.jointBatch);
                }
                return '';
            case 2:
                //已完成
                var url = S.format(S.sites.apps + '/work/teacher/statistics-score-joint?joint_batch={0}&paper_id={1}', dynamic.jointBatch, dynamic.paperId);
                return S.format('<a class="dy-btn dy-btn-success" target="_blank" href="{0}">查看统计</a>', url);
        }
        return '';
    });
    template.config('escape', false);
})(jQuery, SINGER);