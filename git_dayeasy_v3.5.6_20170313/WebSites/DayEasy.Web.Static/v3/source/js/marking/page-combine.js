/**
 * 组合/单题阅卷
 * Created by shay on 2016/7/5.
 */
(function ($, S) {
    var staticPath = function (path) {
            return S.sites.static + path;
        },
        configCookie = '__marking_setting',
        marking = {
            data: {},
            pictureParam: {
                joint: "",      //协同批次
                groups: []      //分组题目列表
            },
            config: {
                minScore: 1,            //最小分值
                scoreReg: '^[0-9]{0,3}\.??[0-9]{1,2}$',
                semiAuto: false,        //半勾是否自动扣分
                semiScore: '50%',       //半勾自动扣分的值
                errorAuto: false,       //叉是否自动扣分
                errorScore: '100%',     //叉自动扣分的值
                iconPath: staticPath('/v1/image/icon/marking/'),//图片基础路径
                icons: [],
                markIcons: ["full.png", "semi.png", "error.png"],//批阅图标
                markType: 0,
                index: 0,
                maxIndex: 0,            //最多批阅
                current: -1,            //当前批阅
                zoom: 1                 //缩放比例
            },
            ui: {
                $window: $(window),     //
                $groups: null,          //分组
                $pictures: null,        //图片区域
                $areas: null,           //题目区域
                $inputs: null,          //输入框
                scoreBoxDialog: null,   //选分弹框
                showScores: null,       //分数选择器
                settingDialog: null,     //设置框
                showSetting: null,      //阅卷设置
                setMark: null,           //设置批阅
                setZoom: null            //放大区域
            },
            event: {
                init: null,            //初始化
                loadPictures: null,    //加载图片
                scroll: null,          //滚动事件
                setMarkType: null,     //设置批阅类型
                scoreBox: null,        //打分栏事件
                tools: null,           //工具事件
                marking: null,         //批阅
                setScore: null,        //设置分数
                reportException: null, //报告异常
                submit: null,          //提交
                loadGroups: null,      //加载题目
                change: null,          //切换
                jump: null             //跳转
            }
        };
    /**
     * 获取图片路径
     * @param type
     * @param word
     * @param isCursor
     * @returns {*}
     */
    var getPath = function (type, word, isCursor) {
        var path = marking.config.iconPath;
        if (type < 0) {
            path += "clear.png";
        }
        else if (type <= 2) {
            path += marking.config.markIcons[type];
        } else if (type == 5) {
            path += 'brow/' + word;
        } else {
            path += word;
        }
        if (isCursor) {
            path = path.replace(/\.[a-z]+$/gi, '.cur');
        }
        return path;
    };
    /**
     * 激活区域
     * @param $area
     * @param enabled
     * @param step
     */
    var enabledArea = function ($area, enabled, step) {
        var $picture = $area.find('.dm-picture'),
            $finished = $area.find('.dm-picture-finished');
        if (!enabled) {
            $finished.remove();
            $picture.addClass('hide');
            var html = '<div class="dm-picture-finished">' +
                '<div class="d-message"><i class="iconfont dy-icon-radiohv"></i>该题已经批阅完成</div>' +
                '<div class="d-actions">' +
                '<button type="button" class="dy-btn dy-btn-info btn-remove">隐藏该题目</button>' +
                '</div>' +
                '</div>';
            if (step < -1) {
                html = '<div class="dm-picture-finished">' +
                    '<div class="d-message nothing">该题无更多批阅记录</div>' +
                    '</div>';
            }
            $picture.before(html);
            $area.find('.dm-exception').addClass('hide');
            $area.addClass('disabled');
            $area.find('input').attr('readonly', 'readonly');
            return;
        }
        if ($finished.length > 0) {
            $area.removeClass('disabled');
            $picture.removeClass('hide');
            $area.find('.dm-exception').removeClass('hide');
            $area.find('input').removeAttr('readonly');
            $finished.remove();
        }
    };

    /**
     * 设置批阅图标
     * @param qid       问题Id
     * @param $marks    图标区域
     * @param type      类型
     * @param score     扣分
     * @param offset    定位
     * @param state     状态:1,新增；0,加载
     */
    marking.ui.setMark = function (qid, $marks, type, score, offset, state) {
        var $mark = $(S.format('<div class="dm-mark" data-qid="{0}">', qid)),
            icon = getPath(type, '', false),
            $group = $marks.parents('.dm-item'),
            region = $group.data('region');
        $mark.append(S.format('<img src="{0}" />', icon));
        if (score && score != 0) {
            $mark.append(S.format('<span class="sub-score">-{0}</span>', score));
        }
        var zoom = marking.config.zoom;
        if (!state && zoom != 1) {
            offset.top = offset.top * zoom;
            offset.left = offset.left * zoom;
        }
        var rawOffset = {
            top: offset.top / zoom,
            left: offset.left / zoom
        };
        //坐标计算
        $mark
            .css({
                top: offset.top,
                left: offset.left
            })
            .data('offset', rawOffset)
            .data('state', state || 0)
            .data('mark', {
                id: qid,
                x: rawOffset.left + region.x,
                y: rawOffset.top + region.y,
                t: type,
                w: Math.abs(score)
            });
        $marks.append($mark);
        if (state == 1) {
            $mark.addClass('mark-change');
            var setMarkScore = function (dockScore) {
                    if (dockScore == 0) return false;
                    $mark.append(S.format('<span class="sub-score">{0}</span>', dockScore));
                    var mark = $mark.data('mark');
                    mark.w = Math.abs(dockScore);
                    $mark.data('mark', mark);
                }, setScore,
                chooseScore = function () {
                    marking.ui.showScores($mark, qid, true, function (dockScore) {
                        if (S.isUndefined(dockScore)) {
                            $mark.remove();
                        } else {
                            setMarkScore(dockScore);
                        }
                    });
                };
            //扣分
            switch (type) {
                case 0:
                    break;
                case 1:
                    if (marking.config.semiAuto) {
                        setScore = marking.event.setScore(qid, marking.config.semiScore);
                        setMarkScore(setScore);
                    } else {
                        chooseScore()
                    }
                    break;
                case 2:
                    if (marking.config.errorAuto) {
                        setScore = marking.event.setScore(qid, marking.config.errorScore);
                        setMarkScore(setScore);
                    } else {
                        chooseScore();
                    }
                    break;
            }
        }
        $mark.bind('click.marking', function () {
            if (marking.config.markType != -1)
                return false;
            var mark = $mark.data('mark');
            if ($mark.data('state') == 1) {
                $mark.remove();
            } else {
                $mark.addClass('mark-change hide');
                mark.t = -1;
                $mark.data('mark', mark);
            }
            marking.event.setScore(qid, parseFloat(mark.w));
            var $score = $group.find('.dm-score-box');
            $('.dm-score-box').not($score).removeClass('active');
            $score.addClass('active');
        });
    };
    /**
     * 显示阅卷配置页
     */
    marking.ui.showSetting = function () {
        var html = template('settingBox');
        marking.ui.settingDialog = S.dialog({
            title: '批阅设置',
            content: html,
            quickClose: true,
            fixed: true,
            backdropOpacity: 0.3,
            align: 'left',
            okValue: '保存',
            ok: function () {
                var $node = $(this.node),
                    $minScore = $node.find('input[name="minScore"]:checked'),
                    $semiAuto = $node.find('input[name="semiAuto"]:checked'),
                    $errorAuto = $node.find('input[name="errorAuto"]:checked');
                marking.config.minScore = $minScore.val();
                marking.config.semiAuto = ($semiAuto.val() == 1);
                marking.config.errorAuto = ($errorAuto.val() == 1);
                if (marking.config.semiAuto) {
                    marking.config.semiScore = $('.d-semi').val();
                    if (marking.config.semiScore.indexOf('%') == -1)
                        marking.config.semiScore = parseFloat(marking.config.semiScore);
                }
                if (marking.config.errorAuto) {
                    marking.config.errorScore = $('.d-error').val();
                    if (marking.config.errorScore.indexOf('%') == -1)
                        marking.config.errorScore = parseFloat(marking.config.errorScore);
                }
                var config = {
                    minScore: marking.config.minScore,
                    semiAuto: marking.config.semiAuto,
                    semiScore: marking.config.semiScore,
                    errorAuto: marking.config.errorAuto,
                    errorScore: marking.config.errorScore
                };
                S.cookie.set(configCookie, S.json(config), 0);
            },
            cancelValue: '取消',
            cancel: function () {

            },
            onshow: function () {
                var $node = $(this.node),
                    $minScore = $node.find('input[name="minScore"]'),
                    $semiAuto = $node.find('input[name="semiAuto"]'),
                    $errorAuto = $node.find('input[name="errorAuto"]'),
                    config = marking.config,
                    bindScores;
                $minScore.bind('click', function () {
                    var $t = $(this),
                        minScore = $t.val();
                    bindScores(minScore == 0.5)
                });
                bindScores = function (decimalMode) {
                    var $semi = $node.find('.d-semi'),
                        $error = $node.find('.d-error'),
                        semiScore = $semi.val(),
                        errorScore = $error.val();
                    $semi.empty().append('<option value="50%">扣一半</option>');
                    $error.empty().append('<option value="100%">全扣</option>');
                    for (var i = 0; i < 5; i++) {
                        if (decimalMode) {
                            $semi.add($error).append(S.format('<option value="{0}">{0}分</option>', -0.5 - i));
                        }
                        $semi.add($error).append(S.format('<option value="{0}">{0}分</option>', -1 - i));
                    }
                    $semi.append('<option value="100%">全扣</option>');
                    $error.append('<option value="50%">扣一半</option>');
                    $semi.val(semiScore);
                    $error.val(errorScore);
                };

                $semiAuto.add($errorAuto).bind('click', function () {
                    var $t = $(this),
                        $select = $t.parents('.d-control').find('select');
                    if ($t.val() == 1) {
                        $select.removeAttr('disabled').removeClass('disabled');
                    } else {
                        $select.attr('disabled', 'disabled').addClass('disabled');
                    }
                });
                $minScore.eq(config.minScore == 1 ? 0 : 1).click();
                $semiAuto.eq(config.semiAuto ? 1 : 0).click();
                $errorAuto.eq(config.errorAuto ? 1 : 0).click();
                if (config.semiAuto) {
                    $('.d-semi').val(config.semiScore);
                }
                if (config.errorAuto) {
                    $('.d-error').val(config.errorScore);
                }
            }
        });
        marking.ui.settingDialog.showModal($('.tool-setting').get(0));
    };

    /**
     * 分数选择器
     * @param $ele
     * @param qid       问题ID
     * @param isDock    是否扣分
     * @param callback  回调函数
     */
    marking.ui.showScores = function ($ele, qid, isDock, callback) {
        var setCallback = function (score) {
                if (!S.isUndefined(score)) {
                    score = marking.event.setScore(qid, score, !isDock);
                }
                callback && callback.call(this, score);
            }, data = {quick: [], scores: []},
            $input, maxScore, decimalMode;
        $input = $('input[data-qid="' + qid + '"]');
        decimalMode = (marking.config.minScore == 0.5);
        //扣分
        if (isDock) {
            maxScore = parseFloat($input.val());
            if (maxScore <= 0) {
                return false;
            }
            decimalMode = decimalMode && maxScore < 20;
            if (maxScore > 4) {
                data.quick.push({text: '扣一半', score: '50%'});
                data.quick.push({text: '全扣', score: '100%'});
            }
            if (maxScore % 1 > 0) {
                data.scores.push(0 - maxScore);
                maxScore -= maxScore % 1;
            }
            while (maxScore > 0) {
                data.scores.push(0 - maxScore);
                if (!decimalMode) {
                    maxScore -= 1;
                } else {
                    maxScore -= 0.5;
                }
            }
            data.scores.sort(function (a, b) {
                return b - a;
            });
        } else {
            //得分
            if (!marking.data.questions.hasOwnProperty(qid))
                return false;
            var question = marking.data.questions[qid];
            maxScore = question.score;
            decimalMode = decimalMode && maxScore < 20;
            if (maxScore % 1 > 0) {
                data.scores.push(maxScore);
                maxScore -= maxScore % 1;
            }
            while (maxScore >= 0) {
                data.scores.push(maxScore);
                if (decimalMode) {
                    maxScore -= 0.5;
                } else {
                    maxScore -= 1;
                }
            }
            data.scores.sort(function (a, b) {
                return a - b;
            });
        }
        var html = template('scoreBox', data);
        marking.ui.scoreBoxDialog && marking.ui.scoreBoxDialog.close().remove();
        marking.ui.scoreBoxDialog = S.dialog({
            padding: 5,
            align: 'right',
            content: html,
            skin: 'score-dialog',
            quickClose: true,
            backdropOpacity: 0.3,
            cancelValue: '撤销',
            cancelDisplay: false,
            onshow: function () {
                var $node = $(this.node),
                    $scores = $node.find('.d-score'),
                    $revoke = $node.find('.d-revoke');
                $scores.bind('click.marking', function () {
                    var score = $(this).data('score');
                    setCallback(score);
                    marking.ui.scoreBoxDialog.close().remove();
                });
                $revoke.bind('click.marking', function () {
                    setCallback();
                    marking.ui.scoreBoxDialog.close().remove();
                });
            }
        }).showModal($ele.get(0));
    };

    /**
     * 放大缩小
     * @param zoom
     */
    marking.ui.setZoom = function (zoom) {
        var widthDiff = 780 * (zoom - 1);
        $('.dm-body').css({width: 1040 + widthDiff});
        $('.dm-picture-box').css({width: 782 + widthDiff});
        $('.dm-picture > img').css({width: 780 + widthDiff});
        $('.dm-mark').each(function (i, item) {
            var $item = $(item),
                offset = $item.data('offset');
            $item.css({
                top: offset.top * zoom,
                left: offset.left * zoom
            });
        });
        $('.dm-area').each(function (i, item) {
            var $item = $(item),
                offset = $item.data('offset');
            $item.css({
                top: offset.y * zoom,
                left: offset.x * zoom,
                width: offset.w * zoom,
                height: offset.h * zoom
            });
        });
        marking.config.zoom = zoom;
    };
    /**
     * 页面加载
     */
    marking.event.init = function () {
        var groups = marking.data.groups,
            group, html, $item, list;
        $('.header-wrap h3').html(S.format('《{0}》组合批阅', marking.data.paperTitle));
        document.title = marking.data.paperTitle + ' - ' + document.title;
        if (!groups || groups.length == 0)
            return false;
        var config = S.json(S.cookie.get(configCookie));
        S.mix(marking.config, config);
        for (var i = 0; i < groups.length; i++) {
            group = groups[i];
            list = [];
            S.each(group.questionIds, function (id) {
                var item = $.extend({}, marking.data.questions[id]);
                item.id = id;
                //减去区域顶点坐标
                item.area.x -= group.region.x;
                item.area.y -= group.region.y;
                item.area = S.json(item.area);
                list.push(item);
            });
            html = template('pictureTemp', {
                section: group.section,
                list: list
            });
            $item = $(html);
            $item.data('region', group.region);
            $item.data('group', group);
            $('.dm-areas').append($item);
        }
        //areas
        marking.ui.$areas = $('.dm-area');
        marking.ui.$groups = $('.dm-item');
        marking.ui.$pictures = $('.dm-picture');
        marking.ui.$inputs = $('.dm-input').find('input');

        //设置区域位置
        marking.ui.$areas.each(function (i, area) {
            var $area = $(area),
                offset = eval('(' + $area.data('offset') + ')');
            $area
                .data('offset', offset)
                .css({
                    top: parseFloat(offset.y),
                    left: parseFloat(offset.x),
                    width: offset.w,
                    height: offset.h
                });
        });
        $('.dm-fixed').each(function (i, item) {
            $(item).data('top', parseFloat($(item).css('top')));
        });
        marking.event.tools();
        marking.event.marking();
        marking.event.scoreBox();
        $('.dm-prev,.dm-next').bind('click', function () {
            var step = ($(this).hasClass('dm-next') ? 1 : -1);
            marking.event.submit(function () {
                marking.event.change(step);
            }, step);
        });
        $('#saveBtn').bind('click.marking', function () {
            var $btn = $(this);
            $btn.disableField('请稍后...');
            marking.event.submit(function () {
                S.msg('保存成功！');
                $btn.undisableFieldset();
                S.pageLoaded();
            });
        });
        //判断能否放大？
        $(document)
            .delegate('.btn-remove', 'click', function () {
                var $btn = $(this);
                S.confirm('确认隐藏该题目？', function () {
                    $btn.disableField('请稍后..');
                    var $items = $('.dm-item'),
                        $item = $btn.parents('.dm-item');
                    S.groups.splice($items.index($item), 1);
                    //重新提交
                    marking.event.submit(function () {
                        var postUrl, $form, $input;
                        postUrl = '/marking/combine/' + S.joint;
                        $form = $('<form method="post" class="hide">');
                        $form.attr('action', postUrl);
                        $input = $('<input type="text" name="qList" />');
                        $input.val(encodeURIComponent(S.json(S.groups)));
                        $form.append($input);
                        $('body').append($form);
                        $form.submit();
                    });
                    //$item.remove();
                    //marking.ui.$areas = $('.dm-area');
                    //marking.ui.$groups = $('.dm-item');
                    //marking.ui.$pictures = $('.dm-picture');
                    //marking.ui.$inputs = $('.dm-input').find('input');
                });
            })
            .delegate('.dm-exception', 'click', function () {
                marking.event.reportException($(this));
            })
            .delegate('.b-jump', 'click', function () {
                var $t = $(this), step;
                if ($t.hasClass('disabled') || $t.attr('disabled'))
                    return false;
                var msg = S.format('即将跳转至您已批阅的{0}试卷', $t.attr('title')),
                    cookieName = '__marking_jump_confirm';
                step = $t.data('step');
                if (step == 'first')
                    step = -marking.config.maxIndex - (marking.config.current == 0 ? -1 : marking.config.current);
                else
                    step = parseInt(step);
                if (S.cookie.get(cookieName)) {
                    marking.event.submit(function () {
                        marking.event.change(step);
                    }, step);
                } else {
                    S.dialog({
                        title: '跳转提示',
                        content: msg,
                        statusbar: '<label class="checkbox"><input type="checkbox">不再提醒</label>',
                        okValue: '确认',
                        cancelValue: '取消',
                        ok: function () {
                            var $node = $(this.node);
                            if ($node.find('input[type=checkbox]').is(':checked')) {
                                S.cookie.set(cookieName, true);
                            }
                            marking.event.submit(function () {
                                marking.event.change(step);
                            }, step);
                        },
                        cancel: function () {

                        }
                    }).showModal();
                }
                //if ($t.hasClass('jump-first'))
                //    S.confirm('即将跳转至您已批阅的第一份试卷', function () {
                //        step = 1 - marking.config.maxIndex;
                //        marking.event.submit(function () {
                //            marking.event.change(step);
                //        }, step);
                //    });
                //else {
                //    S.confirm('即将跳转至您已批阅的最后一份试卷', function () {
                //        step = 0;
                //        marking.event.submit(function () {
                //            marking.event.change(step);
                //        }, step);
                //    });
                //}
                return false;
            })
        ;
    };
    /**
     * 图片加载
     * @param data
     * @param isHistory
     */
    marking.event.loadPictures = function (data, isHistory) {
        var $groups = marking.ui.$groups, $group, $inputs, $input, $marks;
        S.each(data, function (item, i) {
            $group = $groups.eq(i);
            if ($group.length == 0)
                return false;
            var group = marking.pictureParam.groups[i];
            var $history = $group.find('.dm-picture-history');
            //历史记录
            if (isHistory) {
                var current = item.marked + group.step + 1;
                if (current < 0) current = 0;
                $history.find('em').html(S.format('{0}/{1}', current, item.marked));
                $history.removeClass('hide');
            } else {
                $history.addClass('hide');
            }
            //已阅未阅
            var $progress = $group.find('.dm-progress em');
            $progress.eq(0).html(item.marked);
            $progress.eq(1).html(item.left);

            var enabled = item.id && item.id.length > 0;
            enabledArea($group, enabled, group.step);
            if (!enabled) {
                if (group.step == -1) {
                    group.step = 0;
                }
                return;
            }
            if (group.step >= 0) group.step = -1;
            var region = $group.data('region');
            $group.data('picture', item.id);
            var pictureLoadError = function ($img) {
                var $t = $img,
                    $box = $t.parents('.dm-picture');
                $box.children().addClass('hide');
                $box.prepend('<div class="picture-error">图片加载失败，<span>重新加载</span> </div>');
                $box.find('.picture-error span').bind('click', function () {
                    $t.attr('src', item.picture + '?t=' + Math.random());//item.picture + '?t=' + Math.random()
                    $box.find('.picture-error').html('<div class="dy-loading"><i></i></div>');
                });
            };
            //图片
            $group.find('.dm-picture > img')
                .attr('src', item.picture)
                .unbind('error')
                .unbind('load')
                .bind('error', function () {
                    var $box = $(this).parents('.dm-picture');
                    $box.find('.picture-error').remove();
                    pictureLoadError($(this));
                })
                .bind('load', function () {
                    var $box = $(this).parents('.dm-picture');
                    $box.find('.picture-error').remove();
                    $box.children().removeClass('hide');
                });

            //得分
            $inputs = $group.find('.dm-input');
            S.each(item.details, function (detail, id) {
                $input = $inputs.find('input[data-qid="' + id + '"]');
                $input.val(detail.score).data('detail', detail);
            });
            //图标
            $marks = $group.find('.dm-marks-wrap');
            $marks.empty();//清空图标
            //批阅标记
            S.each(item.marks, function (mark) {
                marking.ui.setMark(mark.id, $marks, mark.t, mark.w, {
                    top: parseFloat(mark.y - region.y),
                    left: parseFloat(mark.x - region.x)
                }, 0);
            });
            //批注
            //S.each(item.icons, function (icon) {
            //    var $mark = $('<div class="dm-mark">');
            //    if (icon.t == 4) {
            //        $mark.addClass('mark-word').html(icon.w);
            //    } else {
            //        var path = getPath(icon.t, icon.w);
            //        $mark.append('<img src="' + path + '" />');
            //    }
            //
            //    //坐标计算
            //    $mark.css({
            //        top: parseFloat(icon.y - region.y),
            //        left: parseFloat(icon.x - region.x)
            //    });
            //    $marks.append($mark);
            //});
        });
        marking.ui.$inputs.eq(0).focus().select();
        $("html,body").animate({scrollTop: 0}, 200);
    };
    /**
     * 滚动事件
     */
    marking.event.scroll = function (e) {
        var scrollTop = marking.ui.$window.scrollTop();
        $('.dm-fixed').each(function (i, item) {
            var $item = $(item),
                top = $item.data('top');
            $item.css({
                top: top + scrollTop
            })
        });
        //打分板固定
        $('.dm-item').each(function (i, item) {
            var $item = $(item),
                offset = $item.offset(),
                height = $item.height(),
                diff = scrollTop - offset.top + 55,
                $scoreBox = $item.find('.dm-score-box'),
                scoreHeight = $scoreBox.height(),
                scoreDiff = height - scoreHeight - 55;
            if (diff > 0 && diff <= scoreDiff) {
                $scoreBox.css({
                    marginTop: diff
                });
            } else if (diff < 0) {
                $scoreBox.css({
                    marginTop: 0
                });
            }
        });
    };

    /**
     * 设置鼠标样式
     */
    marking.event.setMarkType = function (type, word) {
        var $pictures = marking.ui.$pictures;
        var cursorPath = getPath(type, word, true);
        $pictures.css({
            cursor: 'url("' + cursorPath + '"),auto'
        });
        marking.config.markType = type;
        if (type <= 2) {
            var $tools = $('.tool'), $tool = $('.tool[data-t="' + (type < 0 ? 'clear' : type) + '"]');
            $tools.not($tool).removeClass('active');
            $tool.addClass('active');
        }
    };
    /**
     * 工具栏事件
     */
    marking.event.tools = function () {
        var $tools = $('.tool'),
            $pictures = $('.dm-picture'),
            keyOn = false,
            changeType;
        /**
         * 工具点击
         */
        $tools.bind('click.marking', function () {
            var $t = $(this),
                type = $t.data('t');

            if ((type == 'clear' || type <= 2) && $t.hasClass('active')) {
                return false;
            }
            if (S.isNumber(type)) {
                marking.event.setMarkType(type);
                return false;
            }
            switch (type) {
                case "clear":
                    marking.event.setMarkType(-1);
                    break;
                case "setting":
                    marking.ui.showSetting();
                    break;
                case "zoom":
                    if (marking.config.zoom == 1) {
                        marking.ui.setZoom(1.2);
                    }
                    else {
                        marking.ui.setZoom(1);
                    }
                    $t.toggleClass('tool-zoom-small', marking.config.zoom > 1);
                    break;
            }
        });
        changeType = function () {
            var type = marking.config.markType + 1;
            if (type > 2) type = -1;
            marking.event.setMarkType(type);
        };
        /**
         * 右键菜单
         */
        $pictures
            .bind('contextmenu', function () {
                changeType();
                return false;
            })
            .bind('mouseover', function () {
                keyOn = true;
            })
            .bind('mouseleave', function () {
                keyOn = false;
            });
        /**
         * 快捷键切换
         */
        $(document).bind('keyup.marking', function (e) {
            //R
            if (keyOn && e.keyCode == 82) {
                changeType();
            }
        });
    };
    /**
     * 打分栏事件
     */
    marking.event.scoreBox = function () {
        marking.ui.$inputs
            .bind('change', function () {
                var $input = $(this),
                    detail = $input.data('detail'),
                    qid = $input.data('qid'),
                    score = $input.val();
                if (!new RegExp(marking.config.scoreReg, "gi").test(score)) {
                    $input.val(detail.score);
                    return false;
                }
                score = parseFloat(score);
                var qItem = marking.data.questions[qid];
                if (score < 0) score = 0;
                if (score > qItem.score) score = qItem.score;
                $input.val(score);
            })
            .bind('keyup', function (e) {
                var $input = $(this),
                    $inputs = marking.ui.$inputs,
                    index = $inputs.index($input);
                var score = $input.val();
                if (score != '' && !new RegExp('^[0-9]{1,3}(\\.[0-9]{0,1})?$', "gi").test(score)) {
                    $input.val(score.substring(0, score.length - 1));
                    return false;
                }
                var question = marking.data.questions[$input.data('qid')];
                score = parseFloat(score);
                if (score > question.score) {
                    $input.val(question.score);
                }
                if (e.keyCode == 13) {
                    if (index < $inputs.length - 1) {
                        $input = $inputs.eq(index + 1);
                        $input.focus().select();
                        //第一个元素滚动
                        var $prev = $input.parents('.dm-score-item').prev();
                        if (!$prev || $prev.length == 0) {
                            var scrollTop = $input.offset().top - 100;
                            if (scrollTop < 0) scrollTop = 0;
                            $("html,body").animate({scrollTop: scrollTop}, 200);
                        }
                    } else {
                        $input.blur();
                        marking.event.submit(function () {
                            marking.event.change(1);
                        }, 1);
                    }
                }
            });
        //全扣
        $('.dm-all').bind('click.marking', function () {
            var $group = $(this).parents('.dm-item'),
                $input, $inputs;
            if ($group.hasClass('disabled'))
                return false;
            $inputs = $group.find('.dm-input input');
            $inputs.each(function (i, item) {
                $input = $(item);
                marking.event.setScore($input.data('qid'), '100%');
            });
        });
        //选分
        $('.dm-score').bind('click.marking', function () {
            var $t = $(this),
                $input = $t.parents('.dm-score-item').find('input'),
                qid = $input.data('qid');
            marking.ui.showScores($t, qid, false);
        });
    };
    /**
     * 阅卷批注
     */
    marking.event.marking = function () {
        var $areas = marking.ui.$areas;
        //批阅图标
        $areas.bind('click.marking', function (e) {
            var $area = $(this),
                type = marking.config.markType,
                qid = $area.data('qid'),
                $picture = $area.parents('.dm-picture');
            if (type < 0)
                return false;
            var offset = $picture.offset(),
                scrollTop = marking.ui.$window.scrollTop(),
                scrollLeft = marking.ui.$window.scrollLeft();
            offset.top = e.clientY - offset.top + scrollTop;
            offset.left = e.clientX - offset.left + scrollLeft;

            if (type == 2) {
                offset.top -= 18;
                offset.left -= 3;
            } else {
                offset.top -= 23;
            }
            var $marks = $picture.find('.dm-marks-wrap');
            marking.ui.setMark(qid, $marks, type, 0, offset, 1);
            var $score = $area.parents('.dm-item').find('.dm-score-box');
            $('.dm-score-box').not($score).removeClass('active');
            $score.addClass('active');
        });
        //擦除
    };
    /**
     * 设置分数
     */
    marking.event.setScore = function (qid, score, isSet) {
        if (!marking.data.questions.hasOwnProperty(qid))
            return 0;
        var qItem = marking.data.questions[qid],
            $input = $('.dm-input input[data-qid="' + qid + '"]'),
            current = parseFloat($input.val()),
            detail = $input.data('detail'),
            tempScore = current;
        if (score && score.toString().indexOf('%') == -1) {
            score = parseFloat(score);
        }
        if (isSet) {
            current = score;
        } else {
            if (S.isNumber(score)) {
                current += score;
            } else {
                if (score == '50%') {
                    current -= qItem.score / 2;
                } else if (score == '100%') {
                    current = 0;
                }
            }
        }
        if (current < 0) current = 0;
        if (current > qItem.score) current = qItem.score;
        $input.val(current);
        return current - tempScore;
    };
    /**
     * 报告异常
     */
    marking.event.reportException = function ($btn) {
        var $group = $btn.parents('.dm-item'), html,
            exceptionDialog, type = 0, message = '', report;
        html = template('exceptionTemp');
        exceptionDialog = S.dialog({
            title: '报告异常',
            content: html,
            okValue: '确　认',
            width: 300,
            quickClose: true,
            backdropOpacity: 0.3,
            cancelValue: '取　消',
            align: 'right',
            ok: function () {
                var $node = $(this.node),
                    $type = $node.find('input[name="exception"]:checked'),
                    $submit = $node.find('button[data-id="ok"]');
                if ($type.length == 0) {
                    S.msg('请选择异常类型！');
                    return false;
                }
                type = $type.val();
                if (type == 0) {
                    message = $node.find('.exception-other input').val();
                    if (!message || message.length > 50) {
                        S.msg('请输入50个字符以内的异常信息！');
                        return false;
                    }
                }
                $submit.disableField('稍后..');
                report(function () {
                    $submit.undisableFieldset();
                });
                return false;
            },
            cancel: function () {
            },
            onshow: function () {
                var $node = $(this.node),
                    $exceptions = $node.find('input[name="exception"]');
                $exceptions.bind('click', function () {
                    var type = $(this).val(),
                        $other = $node.find('.exception-other');
                    $other.toggleClass('hide', type > 0);
                    if (type == 0) {
                        $other.find('input').focus();
                    }
                });
            }
        });
        exceptionDialog.showModal($btn.get(0));
        report = function (callback) {
            var group = $group.data('group'),
                data = {
                    jointBatch: S.joint,
                    pictureId: $group.data('picture'),
                    questionIds: group.questionIds,
                    type: type
                }, sorts = [], i, section = group.section;
            for (i = 0; i < data.questionIds.length; i++) {
                //获取题号
                var id = data.questionIds[i];
                if (!marking.data.questions.hasOwnProperty(id))
                    continue;
                var item = marking.data.questions[id];
                sorts.push(item.sort);
            }
            data.message = S.format('[{0}题号:{1}] {2}',
                section > 0 ? (section == 1 ? 'A卷 ' : 'B卷 ') : '',
                sorts.join('、'),
                message
            );
            $.post('/marking/report-exception', {
                data: encodeURIComponent(S.json(data))
            }, function (json) {
                exceptionDialog.close().remove();
                if (json.status) {
                    marking.event.submit(function () {
                        marking.event.change(0);
                    }, 0);
                    S.msg('报告成功！');
                } else {
                    S.alert(json.message);
                    callback && callback.call(this);
                }
            });
        };
    };
    /**
     * 提交
     */
    marking.event.submit = function (callback, step) {
        var submitData = {pictures: [], details: []}, $group, $marks, picture;
        //标注
        marking.ui.$groups.each(function (i, item) {
            $group = $(item);
            $marks = $group.find('.mark-change');
            if ($marks.length == 0) return;
            picture = {id: $group.data('picture'), marks: []};
            $marks.each(function (j, mark) {
                picture.marks.push($(mark).data('mark'));
            });
            submitData.pictures.push(picture);
        });
        //details
        marking.ui.$inputs.each(function (i, item) {
            var $input = $(item),
                current = parseFloat($input.val()),
                detail = $input.data('detail');
            if (detail && current != detail.score) {
                submitData.details.push({id: detail.id, score: current});
            }
        });

        if (submitData.pictures.length > 0 || submitData.details.length > 0) {
            S.pageLoading();
            $.post('/marking/combine-submit', {
                data: encodeURIComponent(S.json(submitData))
            }, function (json) {
                if (!json.status) {
                    S.pageLoaded();
                    S.msg(json.message);
                    return false;
                }
                callback && callback.call(this);
            });
        } else {
            var cookieName = '__marking_check_operate';
            if (step === 1 && marking.config.current === 0 && !S.cookie.get(cookieName)) {
                S.dialog({
                    title: '批阅提示',
                    content: '您未对该页题目做任何操作<br/>确认<b class="text-danger">该页题目全对</b>吗？',
                    statusbar: '<label class="checkbox"><input type="checkbox">不再提醒</label>',
                    okValue: '确认',
                    cancelValue: '取消',
                    ok: function () {
                        var $node = $(this.node);
                        if ($node.find('input[type=checkbox]').is(':checked')) {
                            S.cookie.set(cookieName, true);
                        }
                        callback && callback.call(this);
                    },
                    cancel: function () {

                    }
                }).showModal();
            } else {
                callback && callback.call(this);
            }
        }
    };

    /**
     * 加载题目
     * @param groups
     */
    marking.event.loadGroups = function (groups) {
        if (!groups) return false;
        S.pageLoading();
        $.get('/marking/change-picture', {
            joint: S.joint,
            groups: encodeURIComponent(S.json(groups)),
            t: Math.random()
        }, function (json) {
            if (!json.status) {
                S.pageLoaded();
                S.msg(json.message);
                if (index && index < 0) {
                    $('.dm-item').each(function (i, item) {
                        enabledArea($(item), false, 0);
                    });
                }
                return false;
            }
            marking.pictureParam.groups = groups;
            var history = false;
            for (var i = 0; i < groups.length; i++) {
                if (groups[i].step < -1) {
                    history = true;
                    break;
                }
            }
            marking.event.loadPictures(json.data, history);

            var marked = 0;
            S.each(json.data, function (item) {
                if (item.marked > marked) marked = item.marked;
            });
            marking.config.maxIndex = marked;
            if (step == 0)
                marking.config.current = -1;
            else {
                if (step < 0 && marking.config.current == 0)
                    marking.config.current = -1;
                marking.config.current += step;
            }
            if (marking.config.current > 0)
                marking.config.current = 0;
            var $first = $('.jump-first'),
                $last = $('.jump-last');
            if (marking.config.maxIndex === -marking.config.current) {
                $first.disableField();
            } else {
                $first.undisableFieldset();
            }
            if (marking.config.current >= -1) {
                $last.disableField();
            } else {
                $last.undisableFieldset();
            }
            S.pageLoaded();
        });
    };

    /**
     * 切换
     * @param conf
     */
    marking.event.change = function (conf) {
        var step, index;
        if (S.isObject(conf)) {
            step = conf.step;
            index = conf.index;
        } else {
            step = conf;
        }
        var groups = $.extend(true, [], marking.pictureParam.groups);
        for (var i = 0; i < groups.length; i++) {
            var group = groups[i];
            if (!index || index >= 0) {
                if (step == 0) {
                    group.step = -1;
                }
                else if (group.step >= 0 && step >= 0) {
                    group.step = 0;
                } else {
                    group.step += step;
                }
            } else {
                group.step = index;
            }
        }
        S.pageLoading();
        $.get('/marking/change-picture', {
            joint: S.joint,
            groups: encodeURIComponent(S.json(groups)),
            //step: current,
            t: Math.random()
        }, function (json) {
            if (!json.status) {
                S.pageLoaded();
                S.msg(json.message);
                if (index && index < 0) {
                    $('.dm-item').each(function (i, item) {
                        enabledArea($(item), false, 0);
                    });
                }
                return false;
            }
            marking.pictureParam.groups = groups;
            var history = false;
            for (var i = 0; i < groups.length; i++) {
                if (groups[i].step < -1) {
                    history = true;
                    break;
                }
            }
            marking.event.loadPictures(json.data, history);

            var marked = 0;
            S.each(json.data, function (item) {
                if (item.marked > marked) marked = item.marked;
            });
            marking.config.maxIndex = marked;
            if (step == 0)
                marking.config.current = -1;
            else {
                if (step < 0 && marking.config.current == 0)
                    marking.config.current = -1;
                marking.config.current += step;
            }
            if (marking.config.current > 0)
                marking.config.current = 0;
            var $first = $('.jump-first'),
                $last = $('.jump-last');
            if (marking.config.maxIndex === -marking.config.current) {
                $first.disableField();
            } else {
                $first.undisableFieldset();
            }
            if (marking.config.current >= -1) {
                $last.disableField();
            } else {
                $last.undisableFieldset();
            }
            S.pageLoaded();
        });
        //$.get(S.sites.static + '/v3/data/marking-pictures.json',
        //    marking.pictureParam,
        //    function (pictures) {
        //        marking.event.loadPictures(pictures);
        //        if (step == 0) {
        //            marking.event.setMarkType(0);
        //        }
        //        S.pageLoaded();
        //    });
    };

    /**
     * 跳转
     * @param index
     */
    marking.event.jump = function (index) {

    };
    S.pageLoading();
    marking.pictureParam.joint = S.joint;
    for (var i = 0; i < S.groups.length; i++) {
        marking.pictureParam.groups.push({
            ids: S.groups[i],
            step: -1
        });
    }

    $(window)
        .bind('scroll.marking', function (e) {
            marking.event.scroll(e);
        })
        .bind('keyup.marking', function (e) {
            if (!e.ctrlKey)
                return;
            if (e.altKey && e.keyCode == 83) {
                marking.event.submit(function () {
                    S.msg('保存成功！');
                    S.pageLoaded();
                });
                return false;
            }
            switch (e.keyCode) {
                case 39:
                    marking.event.change(1);
                    break;
                case 37:
                    marking.event.change(-1);
                    break;
            }
        });

    $.get('/marking/combine-data', {
        joint: S.joint,
        groups: encodeURIComponent(S.json(S.groups)),
        t: Math.random()
    }, function (json) {
        if (!json.status) {
            S.pageLoaded();
            S.msg(json.message, 2000, function () {
                location.href = '/marking/mission_v2/' + S.joint;
            });
            return false;
        }
        S.mix(marking.data, json.data);
        marking.event.init();
        marking.event.change(0);
        marking.event.setMarkType(0);
    });
})(jQuery, SINGER);