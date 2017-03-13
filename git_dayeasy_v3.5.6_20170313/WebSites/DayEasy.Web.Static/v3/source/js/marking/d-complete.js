/**
 * 完成阅卷
 * Created by shay on 2016/4/18.
 */
(function ($, S) {
    var pageUri = S.uri(),
        isJoint = false,
        batch = pageUri.batch,
        paperId,
        updateMarkingStatus,
        completeMarking,
        showProcess,
        confirmDialog,
        finishedDialog,
        submiting = false,
        processDialog,
        logger = S.getLogger('marking-complete');
    if (!batch) {
        batch = $('.dm-header').data('joint');
        isJoint = true;
    }
    /**
     * 完成阅卷
     * @param autoSetIcon
     * @param autoSetScore
     * @param callback
     */
    completeMarking = function (autoSetIcon, autoSetScore, callback) {
        var postData = {
            batch: batch,
            isJoint: isJoint,
            setIcon: autoSetIcon,
            setMarks: autoSetScore
        };
        $.post("/marking/complete", postData, function (json) {
            processDialog && processDialog.close().remove();
            if (json.status) {
                //S.msg("已完成阅卷", 2000, function () {
                //    var url = S.sites.apps + "/work/teacher";
                //    if (isJoint) {
                //        url += S.format('/statistics-score-joint?joint_batch={0}&paper_id={1}', batch, paperId);
                //    }
                //    window.location.href = url;
                //});
                finishedDialog && finishedDialog.close().remove();
                var url = S.sites.apps || '';
                S.dialog({
                    title: '提示',
                    content: '<p>已成功结束阅卷</p><p>您可以在【报表中心】查看本次批阅的统计报告</p>',
                    okValue: '立即查看',
                    ok: function () {
                        window.location.href = url + '/report';
                    },
                    cancelValue: '稍后查看',
                    cancel: function () {
                        window.location.href = url + "/work/teacher";
                    }
                }).showModal();
            } else {
                callback && callback.call(this);
                S.msg(json.message);
            }
            submiting = false;
        });
    };
    /**
     * 更新批阅状态
     */
    updateMarkingStatus = function (status, callback) {
        var postData = {
            batch: batch,
            status: status
        };
        $.post("/marking/update-status", postData, function (json) {
            processDialog && processDialog.close().remove();
            if (json.status) {
                S.msg("已完成阅卷", 2000, function () {
                    window.location.href = S.sites.apps + "/work/teacher";
                });
            } else {
                callback && callback.call(this);
                S.msg(json.message);
            }
            submiting = false;
        });
    };
    /**
     * 结束确认弹框
     * @param setAuto
     * @param callback
     * @param error
     */
    confirmDialog = function (setAuto, callback, error) {
        var check = function () {
            var finishedHtml, step = 0, allFinished = true;
            if (isJoint) {
                var $ranks = $('.d-rank'), i;
                for (i = 0; i < $ranks.length; i++) {
                    if ($ranks.eq(i).data('rank') < 100) {
                        allFinished = false;
                        break;
                    }
                }
            }
            finishedHtml = template('finished-template', {
                finished: allFinished,
                isJoint: isJoint
            });

            var dialogInit = function () {
                var $cbxAgree = $("#cbxFinishedAgree"),
                    $btnOk = $("#btnFinishedOk"),
                    $step2 = $(".finished-step2");
                if (!setAuto) {
                    $btnOk.html('结束阅卷');
                }
                //取消
                $("#btnFinishedCancel").bind("click", function () {
                    error && error.call(this);
                    finishedDialog.close().remove();
                });
                $cbxAgree.bind("click", function () {
                    if ($cbxAgree[0].checked) {
                        $btnOk.removeAttr("disabled");
                    } else {
                        $btnOk.attr("disabled", "disabled");
                    }
                });
                $btnOk.bind("click", function () {
                    if (submiting)
                        return false;
                    submiting = true;
                    $btnOk.disableField('稍后..');
                    if (setAuto) {
                        if (step == 0) {
                            $(".finished-step1").remove();
                            $step2.removeClass("hide");
                            step = 1;
                            submiting = false;
                            //防止鼠标连击
                            S.later(function () {
                                $btnOk.undisableFieldset();
                                $btnOk.html("确认结束");
                            }, 500);
                            return;
                        }
                        var setIcon = $("#cbxFinishedAutoSetIcon")[0].checked,
                            setScore = $("#cbxFinishedAutoSetScore")[0].checked;
                        callback && S.isFunction(callback) && callback.call(this, $btnOk, setIcon, setScore);
                    } else {
                        callback && callback.call(this, $btnOk);
                    }
                });
            };
            finishedDialog = S.dialog({
                title: '结束阅卷',
                content: finishedHtml,
                fixed: true,
                backdropOpacity: .7,
                cancelValue: '取消',
                cancelDisplay: false,
                cancel: function () {
                    error && error.call(this);
                    finishedDialog.close().remove();
                },
                onshow: function () {
                    dialogInit();
                }
            });
            finishedDialog.showModal();
        };
        if (typeof template === "undefined") {
            S.loadScript(S.sites.static + '/js/artTemplate.min.js', {
                success: function () {
                    check();
                }
            });
        } else {
            check();
        }
    };
    var timer;
    /**
     * 展示进度
     * @param process
     */
    showProcess = function (process) {
        process = process || 10;
        timer && timer.cancel();
        var $bar = $('<div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="45" aria-valuemin="0" aria-valuemax="100" style="min-width:2em;width: 0%">0%</div>');
        var $progress = $('<div class="progress"></div>');
        $progress.append($bar);
        var $content = $('<div style="width:500px;">正在结束阅卷...<br/><br/></div>');
        $content.append($progress);
        processDialog = S.dialog({
            content: $content,
            fixed: true,
            backdropOpacity: .7
        });
        processDialog.showModal();
        timer = S.later(function () {
            if (process < 99)
                process += 1;
            else {
                timer.cancel();
            }
            $bar.css({
                width: process + '%'
            }).html(process + '%');
        }, 100, true);
    };
    $(document)
        .delegate('.b-complete', 'click', function () {
            var $btn = $(this);
            $btn.blur();
            if (!batch || !batch.length) {
                S.msg("没有检测到批次号，请重试");
                return;
            }
            $btn.disableField('<i class="fa fa-spin fa-spinner fa-1x"></i>&nbsp;&nbsp;请稍候...');

            confirmDialog(true, function ($btnOk, setIcon, setScore) {
                showProcess();
                if (isJoint) {
                    paperId = $btn.data('paper');
                    completeMarking(setIcon, setScore, function () {
                        $btnOk.undisableFieldset();
                    });
                } else {
                    paperId = S.config('paperId');
                    completeMarking(setIcon, setScore, function () {
                        $btnOk.undisableFieldset();
                    });
                }
            }, function () {
                $btn.undisableFieldset();
            });
        })
        .delegate('.b-update-status', 'click', function () {
            var $btn = $(this),
                status = $btn.data('status');
            $btn.disableField('稍后..');
            confirmDialog(false, function ($btnOk) {
                showProcess();
                updateMarkingStatus(status, function () {
                    $btn.undisableFieldset();
                    $btnOk.undisableFieldset()
                });
            }, function () {
                $btn.undisableFieldset();
            });
        })
    ;
})(jQuery, SINGER);
