/**
 * Created by shay on 2015/11/27.
 */
(function ($, S) {
    var editApp, deleteApp, addApp;
    /**
     * 编辑应用
     * @param id
     * @param name
     */
    editApp = function (id, name) {
        $.post("/sys/apps/edit", {id: id}, function (res) {
                $.Dayez.dialog(name + " - 编辑应用", res, function () {
                    var data = $("#editForm").serialize();
                    $.ajax({
                        url: "/sys/apps/add",
                        data: data,
                        type: "POST",
                        success: function (res) {
                            if (res.status) {
                                $.Dayez.alert("编辑成功！", function () {
                                    window.location.reload();
                                });
                            } else {
                                $.Dayez.tip($(".ui-dialog-button"), res.message, "left");
                            }
                        }
                    });
                    return false;
                }, function () {
                });
            }
        )
        ;
    };
    /**
     * 删除应用
     * @param id
     */
    deleteApp = function (id) {
        $.Dayez.confirm("您确定要删除该应用吗？", function () {
            $.post("/sys/apps/delete", {id: id}, function (res) {
                    if (res.status) {
                        $.Dayez.alert("删除成功！", function () {
                            window.location.reload();
                        });
                    } else {
                        $.Dayez.msg(res.message);
                    }
                }
            )
            ;
        }, function () {
        });
    };
    /**
     * 添加应用
     */
    addApp = function () {
        var addStr = $("#addAppDiv").html();
        $.Dayez.dialog("添加应用", addStr, function () {
            var data = $("#addForm").serialize();
            $.ajax({
                url: "/sys/apps/add",
                data: data,
                type: "POST",
                success: function (res) {
                    if (res.status) {
                        $.Dayez.alert("添加应用成功！", function () {
                            window.location.reload();
                        });
                    } else {
                        $.Dayez.tip($(".ui-dialog-button"), res.message, "left");
                    }
                }
            });
            return false;
        }, function () {
        });
    };

    //事件绑定
    //编辑
    $(".a-edit").click(function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data("id"),
            name = $tr.data("name");
        editApp(id, name);
        return false;
    });

    //删除
    $(".a-delete").click(function () {
        var $tr = $(this).parents('tr'),
            id = $tr.data("id");
        deleteApp(id);
        return false;
    });

    //添加
    $("#btn-add").click(function () {
        addApp();
    });
    $('input[type="checkbox"][name="lookDelete"]').bind('change', function () {
        if (this.checked) {
            location.href = "/sys/apps?hasDelete=true";
        } else {
            location.href = "/sys/apps";
        }
    });
    $(document)
        .delegate('input[name="roleGroup"]', 'change', function () {
            var roles = $('input[name="roleGroup"]:checked'),
                role = 0;
            roles.each(function (index, item) {
                role += ~~item.value;
            });
            $('input[name="role"]').val(role);
        })
    ;
})(jQuery, SINGER);