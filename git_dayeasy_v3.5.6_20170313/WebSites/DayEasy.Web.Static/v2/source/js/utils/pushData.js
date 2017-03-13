/**
 * Created by luoyong on 2014/12/11.
 */
(function ($, S) {
    S._mix(S, {
        pushData: function (options) {
            options = $.extend({}, {
                url: '',
                data: {},
                parent: undefined,
                interval: 1000 * 60 * 3
            }, options || {});
            var timeout, data = [];
            var getData = function () {
                clearInterval(timeout);
                data = data.concat([
                    {name: 'testddd', content: 'wcvcvv', time: S.formatDate(new Date(), "yyyy-MM-dd hh:mm:ss")}
                ]);
                setScroll();
//                $.get(url, data, function (json) {
//                    if (json && json.Status && json.Data.length) {
//                        data = data.concat(json.Data);
//                        setScroll();
//                    }
//                });
            };

            function append(item) {
                if (!item)
                    return;
                var $first,
                    $ni = $(S.format("<li>{name}<div>{content}</div><div>{time}</div></li>", item));
                if (options.parent)
                    $first = $(options.parent).children().first();
                $first.before($ni.hide());
                $ni.slideDown(600, function () {
                    setTimeout(function () {
                        pushNext(item);
                    }, 200);
                });
            }

            function pushNext(pre) {
                if (data.length > 0) {
                    if (pre) {
                        var next = data[data.length - 1];
                        if (next.time != pre.time) {
                            return;
                        }
                    }
                    var item = data.pop();
                    append(item);
                }
                else {
                    clearInterval(timeout);
                }
            }

            function setScroll() {
                if (data.length > 0) {
                    var delay = options.interval / data.length;
                    timeout = setInterval(function () {
                        pushNext();
                    }, delay);
                }
            }

            getData();
            setInterval(getData, options.interval);
        }
    });
})(jQuery, SINGER);