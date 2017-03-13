/**
 * Created by luoyong on 2015/6/26.
 */
(function ($, S) {
    var list = [
        {
            type: 1,
            title: '如何注册？',
            show: 1
        },
        {
            type: 2,
            title: '平台网页登陆不了？',
            show: 1
        },
        {
            type: 3,
            title: '登陆账号不成功？',
            show: 1
        },
        {
            type: 4,
            title: '错题库下载题目图片无法显示？',
            show: 1
        },
        {
            type: 5,
            title: '错题库下载步骤'
        },
        {
            type: 6,
            title: '二维码弄丢了？',
            show: 1
        },
        {
            type: 7,
            title: '答题弄丢了？',
            show: 1
        },
        {
            type: 8,
            title: '阅卷时无法绑定学生？',
            show: 1
        }
    ];
    S._mix(S, {
        random: function (max) {
            return Math.floor(Math.random() * 10) % max;
        },
        hots: function (max, length, t) {
            var arr = [];
            while (arr.length < length) {
                var num = S.random(max);
                if (num != t && arr.indexOf(num) < 0)
                    arr.push(num);
            }
            return arr.sort();
        }
    });
    var loadHots = function (len, type) {
        var hots = S.hots(list.length, len, type);
        var $hots = $('.q-hots ul');
        for (var i = 0; i < hots.length; i++) {
            $hots.append(S.format('<li><a href="/helper/questions?t={type}">{title}</a></li>', list[hots[i]]));
        }
    };
    var loadList = function () {
        var $dl = $('.questions dl');
        for (var i = 0; i < list.length; i++) {
            var item = list[i];
            if (item.show) {
                $dl.append(S.format('<dd><a href="/helper/questions?t={type}">{title}</a></dd>', item));
            }
        }
        loadHots(4, -1);
    };
    var loadQuestion = function () {
        var type = S.uri().t || 0;
        if (type == 0) {
            location.href = "/helper";
        }
        var $tmp = $('.q-template[data-type="' + type + '"]');
        $tmp.show();
        loadHots(4, type - 1);
    };
    S._mix(S, {
        loadList: loadList,
        loadQuestion: loadQuestion
    });
})(jQuery, SINGER);