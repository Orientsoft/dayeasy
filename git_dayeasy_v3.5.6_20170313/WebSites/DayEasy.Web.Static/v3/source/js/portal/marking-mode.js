/**
 * Created by shay on 2016/4/15.
 */
(function ($, S) {
    var $choices = $('.choice-mode'),
        $btn = $('#btnSubmit'),
        mode,
        setModel,
        joint = S.uri().joint;
    setModel = function (callback) {
        if (S.isUndefined(mode) || mode < 0 || mode > 1) {
            S.alert("请选择阅卷方式！", function () {
                callback && S.isFunction(callback) && callback.call(this);
            });
            return false;
        }
        $.post('/marking/set-mode', {
            joint: joint,
            mode: mode
        }, function (json) {
            if (json.status) {
                location.href = "/marking/mission?joint=" + joint;
            } else {
                S.alert(json.message, function () {
                    callback && S.isFunction(callback) && callback.call(this);
                });
            }
        });
    };
    $choices.bind('click', function () {
        var $t = $(this);
        if ($t.hasClass("on"))
            return false;
        $choices.removeClass('on');
        $t.addClass('on');
        $btn.undisableFieldset();
        mode = $choices.index($t);
    });
    $btn.bind('click', function () {
        if (S.isUndefined(mode) || mode < 0 || mode > 1) {
            S.alert("请选择阅卷方式！");
            return false;
        }
        var $content = $('<div class="dm-choice">'),
            $choice = $choices.eq(mode);

        $content.append($choice.find('img').clone());

        $content.append(S.format('<p>确认选择{0}吗？<span class="text-danger">确认后将不能修改！</span></p>', $choice.find('strong')[0].outerHTML));

        S.dialog({
            title: '批阅方式确认',
            content: $content,
            okValue: '确认',
            ok: function () {
                var $t = $(this.node).find('[data-id="ok"]');
                $t.disableField('提交中...');
                setModel(function () {
                    $t.undisableFieldset();
                });
                return false;
            },
            cancelValue: '取消',
            cancel: function () {
                this.close().remove();
            }
        }).showModal();
    });
})
(jQuery, SINGER);