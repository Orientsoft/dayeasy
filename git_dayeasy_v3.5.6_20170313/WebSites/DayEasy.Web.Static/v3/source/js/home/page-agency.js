/**
 * 机构主页
 * Created by shay on 2016/8/15.
 */
(function ($, S) {
    var agencyId,
        tagCss = ["bg0", "bg1", "bg2"],
        currentPage = 0,
        pageSize = 3,
        hotImpression,
        hotQuotations,
        otherTeachers,
        lastVisited,
        hotAgencies,
        elective,
        addRelation;
    agencyId = $('.index-school').data('agency');
    /**
     *  人气名师
     */
    hotImpression = function (impressions) {
        var $wrap = $('.sticker').find('.content'),
            html;
        $wrap.find('.dy-loading').remove();
        if (!impressions || !impressions.length) {
            html = S.showNothing({
                word: '啊哦，暂时还没有老师~'
            });
        } else {
            html = template('hotImpressions', impressions);
        }
        $wrap.html(html);
        //贴纸样式
        $('.sort-sticker').each(function () {
            tagCss.sort(function () {
                return Math.random() >= 0.5 ? 1 : -1;
            });
            $(this).find('dl').each(function (index, node) {
                $(node).addClass(tagCss[index]);
            });
        });
    };
    /**
     * 热门语录
     */
    hotQuotations = function () {
        var $more = $('#quotation-more'),
            $quotationsList = $('#quotation-list'),
            quotationLoad = function () {
                $more.addClass('hide');
                $quotationsList.append('<div class="dy-loading"><i></i></div>');
                $.get('/agency/hot-quotations', {
                    agencyId: agencyId,
                    page: currentPage,
                    size: pageSize
                }, function (json) {
                    bind(json.data);
                    $quotationsList.find('.dy-loading').remove();
                });
            },
            bind = function (data) {
                var html;
                if (!data || !data.quotations || !data.quotations.length) {
                    $more.remove();
                    html = S.showNothing({
                        word: '啊哦，暂时还没有老师语录~'
                    });
                } else {
                    html = template('hotQuotations', data);
                }
                $quotationsList.append(html);
                if (data.count <= (currentPage + 1) * pageSize) {
                    $more.remove();
                } else {
                    $more.removeClass('hide');
                }
            };
        $quotationsList
            .on('click', '.b-support', function () {
                //支持&取消
                var $t = $(this),
                    id = $t.data('qid'),
                    supported = $t.hasClass('supported'),
                    url = '/user/support-quotations';
                if (supported) {
                    //url = '/user/cancel-support-quotations';
                    return false;
                }
                $.post(url, {
                    id: id
                }, function (json) {
                    if (json.login) return false;
                    if (!json.status) {
                        json.message && S.alert(json.message);
                        return false;
                    }
                    var $count = $t.find('span');
                    $count.html(~~$count.html() + (supported ? -1 : 1));
                    $t.toggleClass('supported');
                    $t.attr('title', supported ? '点击支持' : '');
                });
            });
        quotationLoad();
        $more.bind('click', function (e) {
            e.stopPropagation();
            currentPage++;
            quotationLoad();
            return false;
        });
    };
    /**
     * 学校名师
     */
    otherTeachers = function (teacherList) {
        var $tab, html = '', i, len;
        if (!teacherList || !teacherList.length) {
            html = S.showNothing({
                css: {
                    height: 160,
                    lineHeight: '160px',
                    word: '啊哦，暂时还没有老师~'
                }
            });
        } else {
            len = teacherList.length;
            for (i = 0; i < Math.ceil(len / 10); i++) {
                var data = teacherList.splice(0, 10);
                html += template('otherTeacher', data);
            }
        }
        $('#b-teachers').html(html);
        $tab = $('.new-actives');
        if (len > 10) {
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
    /**
     * 最近访问
     */
    lastVisited = function (userList) {
        if (!userList || !userList.length)
            return false;
        var html = template('lastVisit', userList);
        $('#visit-history').html(html);
    };

    /**
     * 常逛机构
     */
    hotAgencies = function (agencies) {
        if (!agencies || !agencies.length)
            return false;
        var html = template('hotAgencies', agencies);
        $('#b-agencies').html(html);
    };

    /**
     * 机构选修课
     */
    elective = function () {
        $.ajax({
            type: "GET",
            url: S.sites.apps + "/elective/courseBatch",
            data:{agencyId:agencyId},
            dataType: "jsonp",
            success: function (json) {
                if (json && S.isString(json)) {
                    var $elective = $('<div class="page-course mb20">');
                    $elective.append(S.format('<a href="{0}/elective/item?batch={1}" target="_blank"></a>', S.sites.apps, json));
                    $('.m-side').prepend($elective);
                }
            }
        });
    };

    /**
     * 添加关系
     * @param status
     * @param start
     * @param end
     */
    addRelation = function (status, start, end) {
        $.post('/user/add-relation', {
            agencyId: agencyId,
            status: status,
            start: start,
            end: end
        }, function (json) {
            if (!json.status) {
                json.message && S.alert(json.message);
                return false;
            }
            S.msg('添加成功~!', 2000, function () {
                location.reload(true);
            });
        });
    };

    $.get('/agency/init-data', {
        agencyId: agencyId
    }, function (json) {
        otherTeachers(json.otherTeachers);
        lastVisited(json.visitors);
        var agencies = [];
        S.each(json.hotAgencies, function (name, id) {
            agencies.push({id: id, name: name});
        });
        hotAgencies(agencies);
        hotImpression(json.impressions);
    });
    elective();
    hotQuotations();
    $('.b-add-relation').bind('click', function () {
        var $t = $(this),
            role = $t.data('role'),
            html, years = [], year = new Date().getFullYear();
        for (var i = 0; i < 50; i++) {
            years.push(year - i);
        }
        html = template('addRelationTpl', {});
        S.dialog({
            title: '添加关系',
            content: html,
            width: 360,
            cancelValue: '取消',
            okValue: '确认',
            ok: function () {
                var $node = $(this.node),
                    $status = $node.find('.status-list li.selected'),
                    status, start, end;
                if ($status.length == 0) {
                    S.msg('请选择关系！');
                    return false;
                }
                status = $status.data('status');
                if (status == 0) {
                    start = $node.find('.current .t-start').val();
                    if (!start) {
                        S.msg('请选择开始年月！');
                        return false;
                    }
                } else if (status == 1) {
                    start = $node.find('.history .t-start').val();
                    end = $node.find('.history .t-end').val();
                    if (!start || !end) {
                        S.msg('请选择开始年月和结束年月！');
                        return false;
                    }
                }
                addRelation(status, start, end);
            },
            cancel: function () {
            },
            onshow: function () {
                var $node = $(this.node);
                $node.find('.d-time').monthPicker({years: years, topOffset: 20});
                $node.find('.status-list li').bind('click', function () {
                    var $t = $(this),
                        status = $t.data('status');
                    if ($t.hasClass('selected'))
                        return false;
                    $t.addClass('selected').siblings().removeClass('selected');
                    $node.find('.status-time .current').toggleClass('hide', status != 0);
                    $node.find('.status-time .history').toggleClass('hide', status != 1);
                });
            }
        }).showModal();
    });
})(jQuery, SINGER);