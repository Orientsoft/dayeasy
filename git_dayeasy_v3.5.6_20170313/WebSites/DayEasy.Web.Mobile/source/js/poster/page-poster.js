/**
 * 名师大神页
 * Created by shay on 2016/9/3.
 */
(function ($, S) {
    //S.alert(S.UA.iphone);
    //屏蔽非移动端设备
    if (!S.UA.mobile) {
        location.href = '/poster';
        return false;
    }
    var restUrl = 'http://open' + (S.sites.domain || '.v3.deyi.com') + '/activity/',
        getArea, makePoster, word,
        posterData = {
            areaCode: 0,
            name: '',
            school: '',
            creator: '',
            type: 0,
            word: '',
            imageData: ''
        };
    var $province = $('.select1'),
        $city = $('.select2');
    /**
     * 获取区域
     * @param code
     * @param callback
     */
    getArea = function (code, callback) {
        $.get(restUrl + 'areas', {
            code: code
        }, function (json) {
            callback && callback.call(this, json);
        })
    };
    /**
     * 生成海报
     */
    makePoster = function (error) {
        $.post(restUrl + 'make-poster', posterData, function (json) {
            if (json.status) {
                location.href = 'share.html?id=' + json.data.id;
            } else {
                S.alert(json.message, function () {
                    error && error.call(this);
                });
            }
        });
        //location.href = 'share.html?id=' + json.data.id;
    };
    var startPoster = function () {
        var poster = $('.d-poster').poster(),
            sceneList = [],
            $sceneList = $('#sceneList');
        poster.setUser(posterData);
        //获取模板
        $.get('/poster/scenes.json', {t: Math.random()}, function (json) {
            sceneList = json.concat();
            var size = Math.ceil(json.length / 6);
            for (var i = 0; i < size; i++) {
                var $ul = $('<ul>');
                S.each(json.splice(0, 6), function (item) {
                    var html = S.format('<li><img src="{0}" alt=""/></li>', item.preview || item.image);
                    $ul.append(html);
                });
                $sceneList.append($ul);
            }
            var $list = $sceneList.find('li');
            $list.eq(0).addClass('on');
            poster.setScene(sceneList[0]);
            word = sceneList[0].word.text;
            $('#picScroll').on('click', 'li', function () {
                var $t = $(this),
                    $list = $('#sceneList li');
                if ($t.hasClass('on'))
                    return false;
                $list.removeClass('on');
                $t.addClass('on');
                var index = $list.index($t);
                //console.log(index);
                poster.setScene(sceneList[index]);
                word = sceneList[index].word.text;
            });
            TouchSlide({
                slideCell: "#picScroll",
                titCell: ".hd ul", //开启自动分页 autoPage:true ，此时设置 titCell 为导航元素包裹层
                autoPage: true, //自动分页
                pnLoop: "false", // 前后按钮不循环
                switchLoad: "_src" //切换加载，真实图片路径为"_src"
            });
        });
        var loadingDialog;
        //选图片
        $('.btn-img-photo').fileSelector({
            start: function () {
                loadingDialog = S.dialog({
                    content: '正在生成...'
                });
                loadingDialog.showModal();
            },
            finished: function (data) {
                poster.setCustom(data, function () {
                    loadingDialog && loadingDialog.close().remove();
                    $('.scrollbarwrap').removeClass('hide');
                });
            }
        });
        $('.btn-img-ok').bind('click', function () {
            var $t = $(this),
                d = S.dialog({
                    content: '努力生成中...'
                });
            if ($t.attr('disabled')) {
                S.msg('努力生成中..');
                return false;
            }
            d.showModal();
            posterData.imageData = poster.getImage();
            $t.attr('disabled', 'disabled');
            makePoster(function () {
                $t.removeAttr('disabled');
                d && d.close().remove();
            });
        });
        var top = 50, $thumb = $('.scroll_thumb'), $track = $('.scroll_track');
        $('.scrollbar-add')
            .bind('touchstart', function () {
                if (top < 10)
                    return false;
                top -= 5;
                var t = poster.zoomCustom(true);
                if (t) {
                    $thumb.css({top: top + '%'});
                    $track.css({height: (top - 2) + '%'});
                    //console.log(t);
                }
                return false;
            })
            .bind('dblclick', function () {
                return false;
            });
        $('.scrollbar-minus')
            .bind('touchstart', function () {
                if (top > 95)
                    return false;
                top += 5;
                var t = poster.zoomCustom(false);
                if (t) {
                    $thumb.css({top: top + '%'});
                    $track.css({height: (top - 2) + '%'});
                    //console.log(t);
                }
                return false;
            })
            .bind('dblclick', function () {
                return false;
            });
        $('.btn-img-text').bind('click', function () {
            var html = $('#wordTpl').html();
            var d = S.dialog({
                content: html,
                padding: 0,
                backgroundColor: 'transparent',
                skin: 'd-edit-word',
                onshow: function () {
                    var $node = $(this.node),
                        $word = $node.find('.textarea-text');
                    $word.val(word).select().focus();
                    $node.find('#editWord').bind('click', function () {
                        if ($word.val() != word) {
                            posterData.word = word = $word.val();
                            poster.editWord(word);
                        }
                        d.close().remove();
                    });
                }
            });
            d.showModal();
        });
    };
    $('#nextStep').bind('click', function () {
        posterData.areaCode = $city.val();
        if (posterData.areaCode <= 0) {
            S.msg('请选择地区！');
            return false;
        }
        posterData.school = $('#txtSchool').val();
        if (!posterData.school) {
            S.msg('请输入所在学校！');
            return false;
        }
        posterData.name = $('#txtName').val();
        if (!posterData.name) {
            S.msg('请输入老师姓名！');
            return false;
        }
        posterData.creator = $('#txtCreator').val();
        if (!posterData.creator) {
            S.msg('请输入你的名字！');
            return false;
        }
        $('.step01').addClass('hide');
        $('.step02').removeClass('hide');
        startPoster();
        return false;
    });

    //省市
    getArea(0, function (data) {
        S.each(data, function (item) {
            $province.append(S.format('<option value="{key}">{value}</option>', item));
        });
    });
    $province.bind('change', function () {
        var id = $province.val();
        if (id < 0) return false;
        getArea(id, function (data) {
            $city.empty();
            $city.append('<option value="-1">请选择</option>');
            S.each(data, function (item) {
                $city.append(S.format('<option value="{key}">{value}</option>', item));
            });
        });
    });
})(jQuery, SINGER);