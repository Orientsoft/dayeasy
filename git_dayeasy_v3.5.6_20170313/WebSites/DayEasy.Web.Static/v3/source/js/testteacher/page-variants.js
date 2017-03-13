(function ($, S) {
    var $variantList = $('.dw-variant-list'),
        $send = $('.send-variant'),
        hasVariant = $variantList.data('variant') == "True",
        isAb = $variantList.data('isab') == "True",
        logger = S.getLogger('page-variants'),
        excepts = [], variantData = {}, i, batch, paperId, addData, removeData, variantCount = 0,
        loadSources, loadVariantMission, getVariant, deleteVariant, sendVariants, downloadVariant, classList;
    batch = $variantList.data('batch');
    paperId = $variantList.data('paper');
    /**
     * 删除数组中某一个元素
     * @param val
     */
    Array.prototype.remove = function (val) {
        var index = this.indexOf(val);
        if (index > -1) {
            this.splice(index, 1);
        }
    };
    template.helper('optionModel', function (data) {
        return S.optionModel(data) ? 'q-options-horizontal' : '';
    });
    /**
     * 参考答案
     */
    template.helper('correctAnswer', function (data) {
        return S.getCorrectAnswers(data);
    });

    if (!hasVariant) {
        /**
         * 添加变式
         * @param qid
         * @param vid
         */
        addData = function (qid, vid) {
            var added = false;
            for (var id in variantData) {
                if (!variantData.hasOwnProperty(id))
                    continue;
                if (id == qid) {
                    variantData[id].push(vid);
                    added = true;
                    break;
                }
            }
            if (!added) {
                variantData[qid] = [vid];
            }
            variantCount++;
            $send.html(S.format('推送变式<small>(共{0}题)</small>', variantCount)).undisableFieldset();
        };
        /**
         * 删除变式
         * @param qid
         * @param vid
         */
        removeData = function (qid, vid) {
            for (var id in variantData) {
                if (!variantData.hasOwnProperty(id))
                    continue;
                if (id == qid) {
                    variantData[id].remove(vid);
                    if (variantData[id].length == 0)
                        delete variantData[id];
                    variantCount--;
                    if (variantCount > 0) {
                        $send.html(S.format('推送变式<small>(共{0}题)</small>', variantCount));
                    } else {
                        $send.html('推送变式').disableField();
                    }
                    break;
                }
            }
        };
        /**
         * 添加/更换变式题
         * @param $source   源问题ele
         * @param $variant  当前变式的ele
         * @param quiet     安静模式
         * @param callback  回调
         */
        getVariant = function ($source, $variant, quiet,callback,isdata) {
			
            if (!$source.length)
                return false;
            var qid = $source.data('qid'), vid, $list = $source.find('.dwv-list');
            if ($variant && $variant.length) {
                vid = $variant.data('qid');
            }
			//var isno=$(this).find('.dwv-list').data('isnodiv');

            $.post('/work/teacher/variant', {
                qid: qid,
				isdata:isdata,
                count: 1,
                excepts: excepts.join(',')
            }, function (json) {
                $list.find('.dy-loading,.dy-nothing').remove();
                var $add = $source.find('.add-variant');
                if (json.count > 0) {
                    var model = json.data[0];
                    var item = template('variantItem', model);
                    if (vid) {
                        $variant.replaceWith(item);
                        //excepts.remove(vid);
                        removeData(qid, vid);
                        !quiet && S.msg('更换成功！');
                    } else {
                        $list.append(item);
                        !quiet && S.msg('添加成功！');
                    }
                    excepts.push(model.id);
                    addData(qid, model.id);
                    if ($list.find('.dwv-item').length >= 3) {
                        $add.disableField();
                    } else {
                        $add.undisableFieldset();
                    }
                    !quiet && S.loadFormula();
                } else {
                    if ($list.find('.dwv-item').length == 0) {
                        $list.html('<div class="dy-nothing">没有相关变式题！</div>');
						if(isdata){
                            getVariant($source, $variant, quiet, callback,0);
                        };
						
                    } else {
                        !quiet && S.msg('已没有更多的变式题！');
                        if (vid) {
                            $variant.find('.change-variant').remove();
                        }
                    }
                    $add.disableField();
                }
                S.later(function () {
                    callback && S.isFunction(callback) && callback.call(this);
                }, 300);
            });
        };
        /**
         * 循环加载变式任务
         * @returns {boolean}
         */
        loadVariantMission = function () {
            var $list = $('.dw-variant-item');
            if ($list.length == 0)
                return false;
            var i = -1;
            var load = function () {
                if (i >= $list.length - 1) {
                    S.loadFormula();
                    return false;
                }
                i++;
                getVariant($list.eq(i), false, true, load,1);
            };
            load();
        };
        /**
         * 加载系统推荐源题目
         */
        loadSources = function () {
            $.get('/work/teacher/variant-paper', {
                batch: batch,
                paper_id: paperId
            }, function (json) {
                if (!json.status) {
                    return false;
                }
                var questions = json.data.questions,
                    variants = json.data.variants;
                if (variants.length == 0 || questions.length == 0) {
                    $variantList.html('<div class="dy-nothing">同学们没有错题信息！</div>');
                    return false;
                }
                for (i = 0; i < variants.length; i++) {
                    var item = variants[i];
                    item.model = questions[item.id];
                    item.title = (isAb ? (item.sectionType == 1 ? "A" : "B") + '卷 - 第' + item.sort : '第' + item.sort);
                    excepts.push(item.id);
                }
                var html = template('variantQuestion', variants);
                $variantList.html(html);
                loadVariantMission();
            });
        };
        loadSources();
        /**
         * 删除变式
         * @param $source   源问题ele
         * @param $variant  当前变式的ele
         */
        deleteVariant = function ($source, $variant) {
            if (!$source.length || !$variant.length)
                return false;
            var qid = $source.data('qid'), vid = $variant.data('qid');
            removeData(qid, vid);
            excepts.remove(vid);
            var $list = $variant.parents('.dwv-list');
            $variant.remove();
            logger.info(variantData);
            if ($list.find('.dwv-item').length == 0) {
                $list.append('<div class="dy-othing">请添加变式题！</div>')
				.data('isnodiv','false');
            }else{
				$list.data('isnodiv','true');
			}
            $source.find('.add-variant').removeClass('disabled').removeAttr('disabled');
            return true;
        };
        /**
         * 推送变式
         */
        sendVariants = function () {
            var classIds = [],
                $content,
                send = function () {
                    $.post('/work/teacher/send-variant', {
                        paper_id: paperId,
                        class_ids: classIds.join(','),
                        variants: S.json(variantData)
                    }, function (json) {
                        if (json.status) {
                            S.alert('推送成功！', function () {
                                location.reload(true);
                            });
                        } else {
                            S.alert(json.message);
                        }
                    });
                };
            $content = $('<div class="dwv-classes">');
            for (var item in classList) {
                if (!classList.hasOwnProperty(item))
                    continue;
                $content.append('<label class="checkbox-group group-checkbox">' +
                    '<input type="checkbox" checked="checked" class="groupItem" value="' + item + '" name="options">' +
                    '<span>' + classList[item] + '</span>' +
                    '<i class="iconfont dy-icon-checkbox dy-icon-checkboxhv"></i>' +
                    '</label>');
            }
            S.dialog({
                title: '推送变式题',
                content: $content,
                okValue: '确认推送',
                ok: function () {
                    var $checked = $(this.node).find('input[type="checkbox"]:checked');
                    if ($checked.length == 0) {
                        S.msg('请选择要推送变式的班级！');
                        return false;
                    } else {
                        logger.info($checked.length);
                        for (i = 0; i < $checked.length; i++) {
                            classIds.push($checked.eq(i).val());
                        }
                        send();
                    }
                }
            }).showModal();
        };
        $send.bind('click', function () {
            if (!classList) {
                $.get('/work/teacher/usage-list', {
                    paper_id: paperId
                }, function (json) {
                    if (json.status) {
                        classList = json.data;
                        sendVariants();
                    } else {
                        S.alert(json.message);
                        $send.disableField();
                    }
                });
            } else {
                sendVariants();
            }
        });
    }
    else {
        $.get('/work/teacher/variant-list', {
            batch: batch,
            paper_id: paperId
        }, function (json) {
            if (json.status) {
                var questions = json.data.questions;
                var variants = json.data.variants;
                for (i = 0; i < variants.length; i++) {
                    var item = variants[i];
                    item.model = questions[item.id];
                    item.title = (isAb ? (item.sectionType == 1 ? "A" : "B") + '卷 - 第' + item.sort : '第' + item.sort);
                    item.variants = [];
                    for (var j = 0; j < item.variantIds.length; j++) {
                        item.variants.push(questions[item.variantIds[j]]);
                        excepts.push(item.variantIds[j]);
                    }
                }
                var html = template('variantQuestion', variants);
                $variantList.html(html);
                $('.download-variant').undisableFieldset();
                S.loadFormula();
            }
        });
        /**
         * 下载变式
         */
        downloadVariant = function () {
            S.open(
                S.format('/work/dowload_variant?data={0}&title={1}',
                    encodeURIComponent(S.json(excepts)),
                    '变式训练题')
            );
        };
        $('.download-variant').bind('click', function () {
            downloadVariant();
        });
    }

    $variantList
        .delegate('.dwv-source span i', 'mouseenter', function () {
            //查看原题
            $(this).parents('.dwv-source').siblings('.dwv-source-box').addClass('hover');
        })
        .delegate('.dwv-source span i', 'mouseleave', function () {
            $(this).parents('.dwv-source').siblings('.dwv-source-box').removeClass('hover');
        })
        .delegate('.variant-answer', 'click', function () {
            //答案解析
            $(this).toggleClass('dy-btn-info');
            $(this).parents('.dwv-item').find('.dwv-answer').toggleClass('hide');
            $(this).blur();
        })
        .delegate('.dwv-item', 'mouseenter', function () {
            //换一题 & 删除
            $(this).addClass('hover');
        })
        .delegate('.dwv-item', 'mouseleave', function () {
            //换一题 & 删除
            $(this).removeClass('hover');
        })
        .delegate('.add-variant', 'click', function () {
            //添加变式
            $(this).disableField();
            var $source = $(this).parents('.dw-variant-item');
            getVariant($source);
        })
        .delegate('.change-variant', 'click', function () {
            //更换变式
            var $source = $(this).parents('.dw-variant-item'),
                $variant = $(this).parents('.dwv-item');
            getVariant($source, $variant);
        })
        .delegate('.delete-variant', 'click', function () {
            //删除变式
            var $source = $(this).parents('.dw-variant-item'),
                $variant = $(this).parents('.dwv-item');
            if (deleteVariant($source, $variant)) {
                S.msg('删除成功！');
            }
        })
    ;
})
(jQuery, SINGER);