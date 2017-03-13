/**
 * Created by shay on 2016/4/6.
 */
(function ($, S) {
    var editorLinks = [
            S.sites.main + "/Content/ueditor/ueditor-config.min.js",
            S.sites.main + "/Content/ueditor/ueditor-all.min.js",
            S.sites.main + "/Content/ueditor/d-formula.min.js"
        ],
        loadedEditor,
        loading,
        editorContent,
        editDialog,
        loadBody,
        loadEditor,
        initEditor;
    /**
     * 加载题干
     * @param qid
     */
    loadBody = function (qid) {
        $.get('/question/edit-body/' + qid, {}, function (html) {
            editDialog = S.dialog({
                title: '编辑题干',
                content: html,
                onshow: function () {
                    loadEditor();
                },
                fixed: true,
                height: 450,
                zIndex: 999
            });
            editDialog.showModal();
        });
    };
    /**
     * 加载编辑器相关js
     */
    loadEditor = function () {
        if (!loadedEditor) {
            S.loadScript(editorLinks[0], {
                success: function () {
                    S.loadScript(editorLinks[1], {
                        success: function () {
                            S.loadScript(editorLinks[2], {
                                success: function () {
                                    initEditor();
                                    loadedEditor = true;
                                },
                                charset: 'utf-8'
                            });
                        },
                        charset: 'utf-8'
                    });
                },
                charset: 'utf-8'
            });
        } else {
            initEditor();
        }
    };

    /**
     * 初始化编辑器
     */
    initEditor = function () {
        if (editorContent) {
            editorContent.destroy();
        }
        editorContent = UE.getEditor("qContent", {
            toolbars: [
                ['source', '|', 'Undo', 'Redo', '|', 'bold', 'italic', 'underline', '|', 'insertimage', 'inserttable', 'deletetable', 'superscript', 'subscript', '|', 'kityformula', 'spechars']
            ],
            serverUrl: S.sites.file + '/uploader',
            initialContent: $("#qContent").html(),
            //        autoClearinitialContent: true,//focus时自动清空初始化时的内容
            wordCount: false,//关闭字数统计
            elementPathEnabled: false,//关闭elementPath
            enableAutoSave: false,//关闭自动保存
            enableContextMenu: true,//关闭右键菜单功能
            saveInterval: 5 * 1000 * 1000,
            retainOnlyLabelPasted: true,
            autoHeightEnabled: false,
            pasteplain: false,
            //fileNumLimit: 3,
            initialFrameHeight: 370//默认的编辑区域高度
        });
        editDialog.reset();
        var id = $('.q-edit-body').data("qid");
        $('#saveBody').bind("click", function () {
            var $t = $(this);
            $t.disableField('正在提交');
            var body = editorContent.getContent();
            $.post('/question/save-body', {
                id: id,
                body: encodeURIComponent(body)
            }, function (json) {
                if (json.status) {
                    S.alert('保存成功！', function () {
                        location.reload(true);
                    });
                } else {
                    S.msg(json.message);
                    $t.undisableFieldset();
                }
            });
        });
        loading = false;
    };
    $('.pq-edit').bind('click', function () {
        if (loading) {
            S.alert('老师别急，正在加载中...');
            return false;
        }
        loading = true;
        var qid = $(this).siblings('.q-item').find('.q-main').data('qid');
        loadBody(qid);
    });
})(jQuery, SINGER);