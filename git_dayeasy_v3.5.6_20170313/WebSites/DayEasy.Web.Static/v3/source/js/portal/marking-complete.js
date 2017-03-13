/**
 * Created by shay on 2016/4/18.
 */
(function ($, S) {
    var pageUri = S.uri(),
        isJoint = false,
        batch = pageUri.batch,
        paperId,
        finished,
        jointFinished,
        setProgressBar,
        confirmDialog,
        submiting = false,
        logger = S.getLogger('marking-complete');
    if (!batch) {
        batch = pageUri.joint;
        isJoint = true;
    }
    /**
     * 结束阅卷 - 非协同
     * @param autoSetIcon
     * @param autoSetScore
     */
    finished = function (autoSetIcon, autoSetScore) {
        if (isJoint) {
            submiting = false;
            return false;
        }
        var postData = {
            batch: batch,
            paper_id: paperId,
            type: pageUri.type,
            auto_set_icon: autoSetIcon,
            auto_set_score: autoSetScore
        };
        //logger.info(postData);
        //return false;
        $.post("/marking/finished", postData, function (json) {
            if (json.status) {
                S.msg("已结束阅卷", 2000, function () {
                    window.location.href = S.sites.apps+ "/work/teacher";
                });
            } else {
                S.msg(json.message);
            }
            submiting = false;
        });
    };
    /**
     * 结束阅卷 - 协同
     * @param batches
     * @param autoSetIcon
     * @param autoSetScore
     */
    jointFinished = function (batches, autoSetIcon, autoSetScore) {
        var $bar = $('<div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="45" aria-valuemin="0" aria-valuemax="100" style="min-width:2em;width: 0%">0%</div>');
        var $progress = $('<div class="progress"></div>');
        $progress.append($bar);
        var $content = $('<div style="width:500px;">正在结束阅卷...<br/><br/></div>');
        $content.append($progress);
        var dialog = S.dialog({
            content: $content,
            fixed: true,
            backdropOpacity: .7
        });
        dialog.showModal();

        setProgressBar(0, batches, autoSetIcon, autoSetScore, $bar, function () {
            $.post("/marking/joint-finished", {batch: batch}, function (json) {
                S.msg("已结束阅卷", 2000, function () {
                    dialog.close();
                    window.location.href = "/";
                });
                submiting = false;
            });
        }, function () {
            dialog.close();
            S.alert("操作失败，请刷新重试！");
            submiting = false;
        });
    };
    /**
     * 设置进度
     * @param i
     * @param batches
     * @param autoSetIcon
     * @param autoSetScore
     * @param $bar
     * @param callback
     * @param errorCallback
     */
    setProgressBar = function (i, batches, autoSetIcon, autoSetScore, $bar, callback, errorCallback) {
        if (i >= batches.length) {
            if (callback && S.isFunction(callback)) {
                callback();
            }
            return false;
        }
        var postData = {
            batch: batches[i],
            paper_id: paperId,
            type: 3,
            auto_set_icon: autoSetIcon,
            auto_set_score: autoSetScore
        };
        //logger.info(postData);
        //setProgressBar(++i, batches, autoSetIcon, autoSetScore, $bar, callback, errorCallback);
        //return false;

        $.post("/marking/finished", postData, function (json) {
            if (json.status) {
                var val = Math.round((i + 1) / batches.length * 100);
                $bar.attr("style", "min-width:2em;width:" + val + "%;").text(val + "%");
                setTimeout(function () {
                    setProgressBar(++i, batches, autoSetIcon, autoSetScore, $bar, callback, errorCallback);
                }, 1000);
            } else {
                if (errorCallback && S.isFunction(errorCallback)) {
                    errorCallback();
                }
            }
        });
    };
    /**
     * 结束确认弹框
     * @param $btn
     * @param callback
     */
    confirmDialog = function ($btn, callback) {
        $btn.disableField('<i class="fa fa-spin fa-spinner fa-1x"></i>&nbsp;&nbsp;正在提交...');
        var check = function () {
            var finishedHtml = template('finished-template'),
                finishedDialog = S.dialog({
                    content: finishedHtml,
                    fixed: true,
                    backdropOpacity: .7
                }),
                step = 0;
            finishedDialog.showModal();
            var $cbxAgree = $("#cbxFinishedAgree"),
                $btnOk = $("#btnFinishedOk"),
                $step2 = $(".finished-step2");
            //取消
            $("#btnFinishedCancel").bind("click", function () {
                $btn.undisableFieldset();
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
                $btnOk.disableField();
                if (step == 0) {
                    $(".finished-step1").remove();
                    $step2.removeClass("hide");
                    $(this).html("确认结束");
                    step = 1;
                    submiting = false;
                    //防止鼠标连击
                    S.later(function () {
                        $btnOk.undisableFieldset();
                    }, 500);
                    return;
                }
                var setIcon = $("#cbxFinishedAutoSetIcon")[0].checked,
                    setScore = $("#cbxFinishedAutoSetScore")[0].checked;
                callback && S.isFunction(callback) && callback.call(this, setIcon, setScore);
            });
        };
        if (typeof template === "undefined") {
            S.loadScript(S.sites.static + '/js/artTemplate.min.js', {
                success: function () {
                    check();
                }
            })
        } else {
            check();
        }
    };
    $(document)
        .delegate('.b-complete', 'click', function () {
            var $btn = $(this);
            if (!batch || !batch.length) {
                S.msg("没有检测到批次号，请重试");
                return;
            }
            confirmDialog($btn, function (setIcon, setScore) {
                if (isJoint) {
                    $.post("/marking/joint-finished-check", {batch: batch}, function (json) {
                        $btn.removeAttr("disabled").html('结束批阅');
                        if (!json.status) {
                            S.msg(json.message);
                            submiting = false;
                            return;
                        }
                        var batches = json.data.batchs;
                        paperId = $btn.data('paper');
                        jointFinished(batches, setIcon, setScore);
                    });
                } else {
                    paperId = S.config('paperId');
                    finished(setIcon, setScore);
                }
            });
        })
    ;
})(jQuery, SINGER);
