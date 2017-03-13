/**
 * 用户主页
 * Created by shay on 2016/8/15.
 */
(function ($, S) {
    var $wrap = $('.page-index-teacher'),
        tagCss = ["bg0", "bg1", "bg2"],
        stages = ["小学", "初中", "高中"],
        userId = $wrap.data('uid'),
        agencyList = [],
        isOwner = $wrap.data('owner'),
        editSignature,
        loadAgencyList,
        agencyHistory,
        impressionList,
        quotationsList,
        relatedTeachers,
        relatedStudents,
        hotAgencies,
        lastVisitor,
        addQuotations,
        support,
        pageGuide,
        markingMission;

    /**
     * 编辑个性签名
     */
    editSignature = function () {
        $('.autograph-input').focus(function (event) {
            event.stopPropagation();
            var $t = $(this);
            $t.addClass("on");
            //$t.val() == this.defaultValue && $t.val("");
            var $edit = $(this).siblings('i');
            $edit.toggleClass('dy-icon-index-edit dy-icon-13');
        }).blur(function (event) {
            event.stopPropagation();
            var $t = $(this);
            $t.removeClass('on');
            $t.val() == '' && $t.val(this.defaultValue);
            var content = S.trim($t.val());
            if (content != $t.data('content')) {
                $.post('/user/edit-signature', {
                    content: content
                }, function (json) {
                    if (!json.status) {
                        json.message && S.alert(json.message);
                        $t.val($t.data('content'));
                        return false;
                    }
                    S.msg('编辑成功！');
                    $t.data('content', content);
                });
            }
            var $edit = $(this).siblings('i');
            $edit.toggleClass('dy-icon-index-edit dy-icon-13');
            $edit.removeClass('show');
        }).hover(function () {
            $(this).siblings('.dy-icon-index-edit').addClass('show');
        }, function () {
            $(this).siblings('.dy-icon-index-edit').removeClass('show');
        });
    };
    loadAgencyList = function () {
        var bind = function (list) {
            var $list = $('#agencyList'), hasTarget = false;
            if (list && list.length > 0) {
                S.each(list, function (item) {
                    item.stageCn = stages[item.stage - 1];
                    item.start = S.formatDate(new Date(item.start.replace(/-/gi, '/')), 'yyyy.MM');
                    if (!item.end) item.end = '至今';
                    else item.end = S.formatDate(new Date(item.end.replace(/-/gi, '/')), 'yyyy.MM');
                    if (!item.logo)
                        item.logo = S.sites.file + '/image/default/agency.png';
                    if (item.status == 0) {
                        item.statusCss = ' d-current';
                    }
                    if (item.status == 2) {
                        item.statusCss = ' d-target';
                        hasTarget = true;
                    }
                });
                var html = template('agencyTpl', list);
                $list.html(html);
            }
            if ($wrap.data('target') && !hasTarget) {
                $list.append('<div class="process-row"><div class="no-direction">暂无目标学校 <a class="a01" href="/user/rec-targets">去设定</a></div></div>');
                if (list.length == 3) {
                    $list.find('.process-row:eq(0)').remove();
                }
            }
        };
        $.get('/user/init-data', {
            userId: userId
        }, function (json) {
            json.agencies && bind(json.agencies);
            var $targetCount = $('#targetCount');
            if ($targetCount.length > 0) {
                $targetCount.html(json.targetCount);
            }
            json.visitors && lastVisitor(json.visitors)
        });
    };
    /**
     * 任教/在读经历
     */
    agencyHistory = function () {
        loadAgencyList();
        $('#editHistory').bind('click', function () {
            $.get('/user/history', {}, function (html) {
                S.dialog({
                    title: '完善资料',
                    content: html,
                    cancelValue: '取消',
                    cancelDisplay: false,
                    padding: 0
                }).showModal();
            });
            return false;
        });
    };

    /**
     * 贴纸印象
     */
    impressionList = function () {
        var page = 0, size = 3, addDialog,
            $list = $('#impressionList'),
            $control = $list.next(),
            /**
             * 还没有贴纸
             */
            noImpression = function () {
                var tagList = [],
                    showBox = function (list) {
                        var html = template('noImpressionTpl', {list: list}),
                            $box = $(html);
                        $('.m-mainc').prepend($box);
                        var tag = S.tags({
                            container: $box.find('.d-tags'),
                            canEdit: true,
                            max: 10,
                            maxLength: 18,
                            change: function (tags) {
                                tagList = tags;
                                $box.find('.box2 li.disabled').each(function () {
                                    var $t = $(this);
                                    if (!S.inArray($t.html(), tagList))
                                        $t.removeClass('disabled');
                                });
                            }
                        });
                        $box.find('.btn-tag').click();
                        $box.find('.box2 li').bind('click', function () {
                            var $item = $(this);
                            if ($item.hasClass('disabled'))
                                return false;
                            tag.set($item.html());
                            $item.addClass('disabled');
                        });
                        $list.parents('.sticker').hide();
                        $box.find('.b-submit').bind('click', function () {
                            if (!tagList || tagList.length == 0) {
                                S.msg('请先添加贴纸！');
                                return false;
                            }
                            var $t = $(this);
                            $t.disableField('稍候..');
                            addImpression(tagList, '', function () {
                                $t.undisableFieldset();
                            }, function () {
                                $box.remove();
                                $list.parents('.sticker').show();
                            });
                        });
                    };
                $.get('/user/hot-impressions', {
                    userId: userId
                }, function (json) {
                    showBox(json);
                });
            },
            /**
             * 数据绑定
             * @param list
             */
            bind = function (list) {
                var html;
                if (page == 0 && (!list || list.count == 0)) {
                    if ($('#noImpressionTpl').length > 0) {
                        noImpression();
                        return false;
                    }
                    html = S.showNothing({
                        word: '还没有收到好友印象哦~ 贴张对自己的印象吧~'
                    });
                } else {
                    list.tagCss = tagCss.sort(function () {
                        return Math.random() >= 0.5 ? -1 : 1;
                    });
                    S.each(list.impressions, function (item) {
                        if (item.supportList && item.supportList.length > 0)
                            item.supportLast = item.supportList.concat().splice(0, 8);
                        else
                            item.supportLast = [];
                    });
                    html = template('impressionTpl', list);
                }
                $list.append(html);
                if (list.count > (page + 1) * size) {
                    $control.removeClass('hide');
                } else {
                    $control.remove();
                }
            },
            /**
             * 数据加载
             */
            loadData = function () {
                $control.addClass('hide');
                $list.append('<div class="dy-loading"><i></i></div>');
                $.get('/user/impressions', {
                    userId: userId,
                    page: page,
                    size: size
                }, function (json) {
                    bind(json);
                    $list.find('.dy-loading').remove();
                });
            },
            /**
             * 添加印象
             * @param list
             * @param successWord
             * @param error
             * @param success
             */
            addImpression = function (list, successWord, error, success) {
                $.post('/user/add-impression', {
                    userId: userId,
                    content: list
                }, function (json) {
                    if (!json.status) {
                        json.message && S.alert(json.message);
                        error && error.call(this);
                        return false;
                    }
                    S.msg((successWord || '添加成功！'), 2000, function () {
                        page = 0;
                        $list.empty();
                        loadData();
                        addDialog && addDialog.close().remove();
                        success && success.call(this);
                    });
                });
            };
        loadData();

        $control.bind('click', function () {
            page++;
            loadData();
            return false;
        });
        $('#addImpression').bind('click', function () {
            var text = $(this).prev('.text').html(),
                html, showDialog;
            showDialog = function (list) {
                html = template('addImpressionTpl', {
                    text: text,
                    list: list
                });
                addDialog = S.dialog({
                    content: html,
                    quickClose: true,
                    backdropOpacity: 0.3,
                    cancelValue: '取消',
                    cancelDisplay: false,
                    onshow: function () {
                        var $node = $(this.node),
                            tagList = [];
                        var tag = S.tags({
                            container: $node.find('.d-tags'),
                            canEdit: true,
                            max: 10,
                            maxLength: 18,
                            change: function (tags) {
                                tagList = tags;
                                $node.find('.box2 li.disabled').each(function () {
                                    var $t = $(this);
                                    if (!S.inArray($t.html(), tagList))
                                        $t.removeClass('disabled');
                                });
                            }
                        });
                        $node.find('.btn-tag').click();
                        $node.find('.box2 li').bind('click', function () {
                            var $item = $(this);
                            if ($item.hasClass('disabled'))
                                return false;
                            tag.set($item.html());
                            $item.addClass('disabled');
                        });
                        $node.find('#submitBtn').bind('click', function () {
                            var $btn = $(this);
                            if (!tagList || tagList.length == 0) {
                                S.msg('请输入贴纸');
                                return false;
                            }
                            $btn.disableField('稍候..');
                            addImpression(tagList, '', function () {
                                $btn.undisableFieldset();
                            });
                        });
                    }
                });
                addDialog.showModal();
            };
            $.get('/user/hot-impressions', {
                userId: userId
            }, function (json) {
                showDialog(json);
            });
            return false;
        });
        //支持
        $list.on('click', '.b-more-supports', function () {
            var $t = $(this),
                $pop = $t.next('.pop-cont'),
                $parents = $t.parents('.sticker-list'),
                $siblingsPop = $parents.siblings().removeClass('on').find('.pop-cont');
            $siblingsPop.hide();
            $parents.toggleClass('on');
            $pop.parents('.sticker-list').toggleClass('opened');
            $pop.slideToggle('fast');
        });
        //关闭
        $list.on('click', '.close', function () {
            var $t = $(this),
                $pop = $t.parents('.pop-cont'),
                $parents = $t.parents('.sticker-list');
            $parents.toggleClass('on');
            $pop.parents('.sticker-list').toggleClass('opened');
            $pop.slideToggle('fast');
        });
        $list.on('mouseenter', '.bg-base', function () {
            $(this).find('i').removeClass('hide');
        });
        $list.on('mouseleave', '.bg-base', function () {
            $(this).find('i').addClass('hide');
        });
        //删除
        $list.on('click', '.bg-base i', function () {
            var $item = $(this).parents('.sticker-list'),
                id = $item.data('sid');
            S.confirm('确认删除该贴纸？', function () {
                $.post('/user/delete-impression', {
                    id: id
                }, function (json) {
                    if (!json.status) {
                        json.message && S.alert(json.message);
                        return false;
                    }
                    S.msg('删除成功~！', 2000, function () {
                        $item.remove();
                    });
                });
            });
        });
        //新收到的贴纸
        var $lastImpression = $('.last-impression');
        if ($lastImpression.length > 0) {
            var $container = $lastImpression.find('.d-tags'),
                tagList = $container.data('tags').split(';');
            var tag = S.tags({
                container: $container,
                data: tagList,
                canEdit: true,
                max: 10,
                maxLength: 18,
                change: function (tags) {
                }
            });
            $lastImpression.find('.b-support').bind('click', function () {
                var $t = $(this),
                    list = tag.get();
                if (!list || list.length == 0) {
                    S.msg('没有支持任何印象');
                    return false;
                }
                $t.disableField('稍候..');
                addImpression(list, '支持成功！', function () {
                    $t.undisableFieldset();
                }, function () {
                    $lastImpression.remove();
                    $list.parents('.sticker').show();
                });
            });
            $lastImpression.find('.close').bind('click', function () {
                $lastImpression.remove();
            });
        }
    };

    /**
     * 经典语录
     */
    quotationsList = function () {
        var page = 0, size = 3,
            $list = $('#quotationsList'),
            $control = $list.next(),
            bind = function (json) {
                var html;
                if (page == 0 && (!json || json.count == 0)) {
                    html = S.showNothing({
                        word: '啊哦，暂时还没有老师语录~'
                    });
                } else {
                    var list = json.data;
                    S.each(list, function (item) {
                        item.time = S.formatDate(new Date(item.creationTime.replace(/-/gi, '/')), 'yyyy-MM-dd');
                    });
                    html = template('quotationsTpl', list);
                }
                $list.append(html);
                if (json.count > (page + 1) * size) {
                    $control.removeClass('hide');
                } else {
                    $control.remove();
                }
            },
            loadData = function () {
                $control.addClass('hide');
                $list.append('<div class="dy-loading"><i></i></div>');
                $.get('/user/quotations', {
                    userId: userId,
                    page: page,
                    size: size
                }, function (json) {
                    bind(json);
                    $list.find('.dy-loading').remove();
                });
            };
        if ($list.length == 0)
            return false;
        loadData();
        $('#addQuotations').bind('click', function () {
            var text = $(this).prev('.text').html(),
                html = template('addQuotationsTpl', {
                    text: text
                });
            S.dialog({
                content: html,
                quickClose: true,
                backdropOpacity: 0.3,
                cancelValue: '取消',
                cancelDisplay: false,
                onshow: function () {
                    var $node = $(this.node),
                        $text = $node.find('textarea');
                    $node.find('#submitBtn').bind('click', function () {
                        var text = S.trim($text.val()),
                            $btn = $(this);
                        if (!text) {
                            S.msg('请输入江湖传说~!');
                            return false;
                        }
                        $btn.disableField('稍候..');
                        addQuotations(text, function () {
                            $btn.undisableFieldset();
                        });
                    });
                }
            }).showModal();
        });
        $control.bind('click', function () {
            page++;
            loadData();
            return false;
        });
        $list.on('mouseenter', '.hot-list', function () {
            $(this).find('.b-delete').removeClass('hide');
        });
        $list.on('mouseleave', '.hot-list', function () {
            $(this).find('.b-delete').addClass('hide');
        });
        $list.on('click', '.b-delete', function () {
            var $item = $(this).parents('.hot-list'),
                id = $item.data('sid');
            S.confirm('确认删除该记录？', function () {
                $.post('/user/delete-quotations', {
                    id: id
                }, function (json) {
                    if (!json.status) {
                        json.message && S.alert(json.message);
                        return false;
                    }
                    S.msg('删除成功~！', 2000, function () {
                        $item.remove();
                    });
                });
            });
        });
    };
    /**
     * 最近来访
     */
    lastVisitor = function (list) {
        var $list = $('#lastVisit'), html;
        if (!list || !list.length) {
            html = S.showNothing({
                icon: false,
                css: {
                    height: 100,
                    lineHeight: '100px'
                },
                word: '还没有人来过，多去看看别人有助于提高你的人气哦~'
            });
        } else {
            html = template('lastVisitTpl', list);
        }
        $list.html(html);
    };
    /**
     * 相关教师
     */
    relatedTeachers = function () {
        var $list = $('#relatedTeachers'), bind, loadData;
        if ($list.length == 0)
            return false;
        bind = function (list) {
            var html = '', len, i, size = 3;
            if (!list || list.length == 0) {
                html = S.showNothing({
                    css: {
                        height: 150,
                        lineHeight: '150px'
                    },
                    word: '没有相关的教师~'
                });
            } else {
                len = list.length;
                for (i = 0; i < Math.ceil(len / size); i++) {
                    var data = list.splice(0, size);
                    html += template('relatedTeachersTpl', data);
                }
            }
            $list.html(html);
            if (len > size) {
                var $tab = $list.parents('.new-actives');
                $tab.find('.tab-hd').removeClass('hide');
                $tab.slide({
                    mainCell: ".tab-bd-in",
                    effect: "left",
                    delayTime: 400,
                    pnLoop: false,
                    easing: "easeOutCubic"
                });
            }
        };
        loadData = function () {
            $list.append('<div class="dy-loading"><i></i></div>');
            $.get('/user/related-teachers', {
                userId: userId
            }, function (json) {
                bind(json);
                $list.find('.dy-loading').remove();
            });
        };
        loadData();
    };
    /**
     * 相关同学
     */
    relatedStudents = function () {
        var $list = $('#relatedStudents'), bind, loadData;
        if ($list.length == 0)
            return false;
        bind = function (list) {
            var html = '', len, i, size = 5;
            if (!list || list.length == 0) {
                html = S.showNothing({
                    word: '没有相关的同学~'
                });
            } else {
                len = list.length;
                for (i = 0; i < Math.ceil(len / size); i++) {
                    var data = list.splice(0, size);
                    html += template('relatedStudentsTpl', data);
                }
            }
            $list.html(html);
            if (len > size) {
                var $tab = $list.parents('.new-actives');
                $tab.find('.tab-hd').removeClass('hide');
                $tab.slide({
                    mainCell: ".tab-bd-in",
                    effect: "left",
                    delayTime: 400,
                    pnLoop: false,
                    easing: "easeOutCubic"
                });
            }
        };
        loadData = function () {
            $list.append('<div class="dy-loading"><i></i></div>');
            $.get('/user/related-students', {
                userId: userId
            }, function (json) {
                bind(json);
                $list.find('.dy-loading').remove();
            });
        };
        loadData();
    };
    /**
     * 近期热门学校
     */
    hotAgencies = function () {
        var $list = $('#hotAgencies'), bind, loadData;
        if ($list.length == 0)
            return false;
        bind = function (list) {
            var html = '', len, i, size = 3;
            if (!list || list.length == 0) {
                html = S.showNothing({
                    word: '没有相关的学校~'
                });
            } else {
                len = list.length;
                for (i = 0; i < Math.ceil(len / size); i++) {
                    var data = list.splice(0, size);
                    html += template('hotAgenciesTpl', {
                        start: i * size,
                        data: data
                    });
                }
            }
            $list.html(html);
            if (len > size) {
                var $tab = $list.parents('.new-actives');
                $tab.find('.tab-hd').removeClass('hide');
                $tab.slide({
                    mainCell: ".tab-bd-in",
                    effect: "left",
                    delayTime: 400,
                    pnLoop: false,
                    easing: "easeOutCubic"
                });
            }
        };
        loadData = function () {
            $list.append('<div class="dy-loading"><i></i></div>');
            $.get('/user/hot-agencies', {
                userId: userId
            }, function (json) {
                bind(json);
                $list.find('.dy-loading').remove();
            });
        };
        loadData();
    };
    /**
     * 添加语录
     * @param words
     * @param callback
     */
    addQuotations = function (words, callback) {
        $.post('/user/add-quotations', {
            userId: userId,
            content: words
        }, function (json) {
            if (!json.status) {
                json.message && S.alert(json.message);
                callback && callback.call(this);
                return false;
            }
            S.msg('添加成功！', 2000, function () {
                location.reload(true);
            });
        });
    };
    /**
     * 点赞
     * @param $node
     * @param url
     */
    support = function ($node, url) {
        var id = $node.data('qid');
        $.post(url, {
            id: id
        }, function (json) {
            if (json.login) return false;
            if (!json.status) {
                json.message && S.msg(json.message);
                return false;
            }
            var $count = $node.find('span');
            $count.html(~~$count.html() + 1);
            var $supported = $('<div class="supported">');
            $supported.html($node.html());
            $node.replaceWith($supported);
        });
    };
    /**
     * 页面引导
     */
    pageGuide = function () {
        var _cookie_name = '_user_home_guide',
            $app = $('.d-apps');
        if ($app.length == 0 || S.cookie.get(_cookie_name)) {
            return false;
        }
        var imageUrl = function (name) {
            return S.format('<img src="{0}/v3/image/index/{1}" alt=""/>', S.sites.static, name);
        };
        var steps = [{
            element: '.d-apps',
            intro: imageUrl('intro-user-tool.png'),
            position: 'left'
        }];
        if ($('.note-group').length) {
            steps.push({
                element: '.d-groups',
                intro: imageUrl('intro-no-group.png'),
                position: 'left' // top,bottom,left,right
            });
        } else {
            steps.push({
                element: '.d-groups',
                intro: imageUrl('intro-group.png'),
                position: 'left' // top,bottom,left,right
            });
        }
        introJs().setOptions({
            skipLabel: "跳过",
            prevLabel: "上一步",
            nextLabel: "下一步",
            doneLabel: "结束",
            overlayOpacity: 0.7,
            //对应的数组，顺序出现每一步引导提示
            steps: steps
        }).oncomplete(function () {
            S.cookie.set(_cookie_name, true, 300 * 24 * 60);
        }).onexit(function () {
            S.cookie.set(_cookie_name, true, 300 * 24 * 60);
        }).start();

    };

    /**
     * 批阅任务
     */
    markingMission = function () {
        var cookieName = '__dayeasy_marking_' + userId,
            tip = S.cookie.get(cookieName);
        if (tip) return false;
        $.ajax({
            type: "GET",
            url: S.sites.apps + "/work/teacher/marking-count",
            dataType: "jsonp",
            success: function (json) {
                if (json.status && json.data > 0) {
                    S.dialog({
                        title: '提示',
                        content: S.format('<div class="dy-results">您在【批阅中心】有<em>{0}</em>条新的批阅任务</div>', json.data),
                        okValue: '立即批阅',
                        ok: function () {
                            var url = S.sites.apps + '/work/teacher';
                            try {
                                S.open(url);
                            } catch (e) {
                                window.location.href = url;
                            }
                        },
                        cancelValue: '稍后查看',
                        cancel: function () {
                            S.cookie.set(cookieName, true, 30);//半个小时
                            this.close().remove();
                        }
                    }).showModal();
                }
            }
        });
    };
    //相关人气老师 Tab
    $(".new-actives").slide({
        mainCell: ".tab-bd-in",
        effect: "left",
        delayTime: 400,
        pnLoop: false,
        easing: "easeOutCubic"
    });
    $('#impressionList').on('click', '.b-support', function () {
        support($(this), '/user/support-impression');
    });
    $('#quotationsList').on('click', '.b-support', function () {
        support($(this), '/user/support-quotations');
    });
    if (isOwner) {
        editSignature();
        markingMission();
    }
    pageGuide();
    agencyHistory();
    impressionList();
    quotationsList();
    relatedStudents();
    relatedTeachers();
    hotAgencies();
    S._mix(S, {
        loadAgency: loadAgencyList
    });
})(jQuery, SINGER);