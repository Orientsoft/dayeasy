$(document)
    .delegate(".set-top", "click", function () {
        var $t = $(this),
            $parent = $t.parent();
        $.post("/querys/top", {
            id: $parent.data("id")
        }, function (json) {
            if (json.status) {
                if ($t.hasClass("sq-top"))
                    $t.removeClass("sq-top");
                else
                    $t.addClass("sq-top");
            }
        });
    })
;