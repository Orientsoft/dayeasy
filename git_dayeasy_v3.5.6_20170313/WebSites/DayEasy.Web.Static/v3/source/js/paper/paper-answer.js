$(function () {
    var ue;
    if ($("#firstAnswer").length > 0) {
        ue = UE.getEditor('firstAnswer', {
            toolbars: [
                ['source', '|', 'Undo', 'Redo', '|', 'bold', 'italic', 'underline', '|', 'insertimage', 'inserttable', 'deletetable', 'superscript', 'subscript', '|', 'kityformula', 'spechars']
            ],
            serverUrl: $("#fileSite").val() + '/uploader',
            wordCount: false,//关闭字数统计
            elementPathEnabled: false,//关闭elementPath
            enableAutoSave: false,//关闭自动保存
            enableContextMenu: true,//关闭右键菜单功能
            saveInterval: 5 * 1000 * 1000,
            pasteplain: false,
            autoFloatEnabled: false,
            //fileNumLimit: 3,
            initialFrameHeight: 150//默认的编辑区域高度
        });

        ue.ready(function () {
            //阻止工具栏的点击向上冒泡
            $(this.container).click(function (e) {
                e.stopPropagation();
            });
        });

        $(document).bind("click", function () {
            var currentParnet = ue.container.parentNode.parentNode;
            var currentContent = ue.getContent();
            $(currentParnet).html(currentContent);
            $("#content_" + $(currentParnet).data('qid')).html(currentContent);

            singer.loadFormula($(currentParnet));
        });

        //修改答案
        $(".answerEditDiv").click(function () {
            var $target = $(this);
            var content = $("#content_" + $target.data('qid')).html();

            var currentParnet = ue.container.parentNode.parentNode;
            var currentContent = ue.getContent();

            $("#content_" + $(currentParnet).data('qid')).html(currentContent);
            $(currentParnet).html(currentContent);

            singer.loadFormula($(currentParnet));

            $target.html('');
            $target.append(ue.container.parentNode);
            ue.reset();
            ue.setContent('');
            setTimeout(function () {
                ue.setContent(content);
            }, 100);

            return false;
        });

        $("#btnSureAnswer").click(function () {
            var currentParnet = ue.container.parentNode.parentNode;
            var currentContent = ue.getContent();
            $(currentParnet).html(currentContent);
        });
    }

    $("#btnCancelAnswer").click(function () {
        $("#chooseQuDiv").removeClass('hide');
        $("#paperNav").removeClass('hide');
        $("#inputAnswerDiv").empty();
    });

    $(".g-wrap input").focus(function () {
        $(this).addClass('z-crt');
    });

    $(".g-wrap input").blur(function () {
        $(this).removeClass('z-crt');
    });

    $(".g-wrap input").keydown(function (e) {
        e = window.event || e;
        var keycode = e.keyCode || e.which;
        if (!$(this).hasClass("multi")) {
            $(this).val(String.fromCharCode(keycode));
        }
    });

    $(".g-wrap input").keyup(function (e) {
        e = window.event || e;
        var keycode = e.keyCode || e.which;
        if (keycode != 8 && keycode != 46) {
            this.value = this.value.toUpperCase().replace(/[^a-zA-Z]/g, '');
            if (this.value.length > 1 && !$(this).hasClass("multi")) {
                this.value = this.value.substring(0, 1);
            }
            if (/[a-zA-Z]/.test(this.value)) {
                $(this).next().focus();
            }
        } else {
            $(this).val('').prev().focus();
        }
        e.preventDefault();
    });
    var getAnswer = function () {
        var answers = [];

        //处理客观题
        var inputObjs = $('.g-wrap input[type=text]:visible:not(:disabled)');
        if (inputObjs && inputObjs.length > 0) {
            $.each(inputObjs, function (index, item) {
                var qAnswer = {};
                qAnswer.DetailId = $(item).data('detailid');
                qAnswer.QId = $(item).data('qid');
                qAnswer.Answer = $(item).val();
                answers.push(qAnswer);
            });
        }

        //处理主观题
        var textObjs = $(".g-wrap .answerEditDiv");
        if (textObjs && textObjs.length > 0) {
            $.each(textObjs, function (index, item) {
                var qAnswer = {};
                qAnswer.QId = $(item).data('qid');
                qAnswer.Answer = $('<div/>').text($("#content_" + qAnswer.QId).html()).html();

                answers.push(qAnswer);
            });
        }
        return answers;
    };
    $("#btnSaveAnswer").click(function () {
        var $t = $(this);
        $(document).click();

        var answers = JSON.stringify(getAnswer());
        var paperId = $("#paperId").val();
        $t.disableField('正在保存...');
        $.post('/paper/save-answer', {answers: answers, paperId: paperId}, function (res) {
            if (res.Status) {
                singer.alert('保存成功！', function () {
                    location.href = '/paper/answer/' + paperId;
                });
            } else {
                singer.msg(res.Message);
                $t.undisableFieldset();
            }
        });
    });
});