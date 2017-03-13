/**
 * Created by shoy on 2014/10/6.
 */
(function (S) {
    if (S.UA.ie && S.UA.ie < 8) {
        var notSupport = '<div class="not-support">' +
            '<div class="alert alert-warning alert-dismissible fade in" role="alert">' +
            '为更好的提升用户体验度，得一教育平台对您现在使用的<strong>IE{0}</strong>浏览器已不提供支持。' +
            '请<a target="_blank" href="http://windows.microsoft.com/zh-cn/internet-explorer/download-ie">升级您的浏览器</a>，以获得更好更安全的基础教育服务体验！' +
            '</div></div>';
        notSupport = S.format(notSupport, S.UA.ie);
        document.body.className = "not-support-body";
        var html = document.getElementsByTagName("html")[0];
        html.style.overflow = "hidden";
        document.write(notSupport);
        return false;
    }
    S.config({
        debug: true,
        loggerLevel: S.Logger.Level.DEBUG
    });
    var loading;
    S._mix(S, {
        pageLoading: function () {
            //高端浏览器添加页面加载效果
            if (S.UA.firefox || S.UA.chrome || S.UA.ie > 9) {
                if (loading) {
                    loading.className = 'page-loading';
                    loading.removeAttribute('style');
                } else {
                    loading = document.createElement('div');
                    loading.className = 'page-loading';
                    loading.innerHTML = '<div class="spinner"><div></div><div></div><div></div><div></div><div></div><div></div><aside>页面加载中...</aside></div>';
                    document.body.appendChild(loading);
                }
            }
        },
        pageLoaded: function () {
            if (!loading) return false;
            loading.className += ' page-loaded';
            setTimeout(function () {
                loading.style.display = 'none';
                loading.style.zIndex = -1;
            }, 1000);
        }
    });
    if (!S.config('pageLoading')) {
        S.pageLoading();
        window.onload = function () {
            S.pageLoaded();
        };
    }
})(SINGER);
