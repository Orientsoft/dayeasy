/**
 * 放于body标签之下，优先加载(先于jQuery)
 * Created by shay on 2016/1/7.
 */
(function (S){
    S.config({
        debug: true,
        loggerLevel: S.Logger.Level.DEBUG
    });
    /**
     * 站点列表
     */
    S._mix(S, {
        deyi: (function (){
            return window.DEYI || {};
        })(),
        sites: (function (){
            return (window.DEYI || {}).sites || {};
        })()
    });
    /**
     * 低版本浏览器
     */
    if(S.UA.ie && S.UA.ie <= 8){
        var baseUri = S.sites.static + '/v3/image/ie/',
            recommendedBrowser = [{
                text: 'Chrome',
                logo: baseUri + "chrome_logo.png",
                url: 'http://dlsw.baidu.com/sw-search-sp/soft/9d/14744/ChromeStandalone_46.0.2490.80_Setup.1445829883.exe'
            }, {
                text: 'Firefox',
                logo: baseUri + "firefox_logo.png",
                url: 'http://www.firefox.com.cn/download/'
            }, {
                text: 'Safari',
                logo: baseUri + "safari_logo.png",
                url: 'https://developer.apple.com/safari/resources/'
            }, {
                text: 'IE9及以上',
                logo: baseUri + "ie_logo.png",
                url: 'http://windows.microsoft.com/zh-cn/internet-explorer/ie-11-worldwide-languages/'
            }],
            notSupport = '<div class="not-support">' +
                '<div class="browser-info">' +
                '<div class="notice-info">' +
                '<p>得一教育平台对您现在使用的<strong>{0}</strong>浏览器已不提供支持。推荐下载以下浏览器，以获得<strong>更好、更安全</strong>的基础教育服务体验！</p></div>' +
                '<div class="browser-list">{1}</div></div></div>',
            recommendTemp = '<span><a href="{url}" target=_blank><img src="{logo}"><br>{text}</a></span>',
            recommendHtml = '';
        for (var i = 0; i < recommendedBrowser.length; i++) {
            recommendHtml += S.format(recommendTemp, recommendedBrowser[i]);
        }
        notSupport = S.format(notSupport, (S.UA.ie ? 'IE' + S.UA.ie : ''), recommendHtml);
        document.write(notSupport);
    }
    /**
     * 手机网站
     */
    if(S.UA.mobile && location.pathname == '/' && S.uri().from != 'wap'){
        location.href = 'http://m.deyi.com';
        return false;
    }
    var loading;
    /**
     * singer扩展
     */
    S._mix(S, {
        pageLoading: function (opts){
            //高端浏览器添加页面加载效果
            if(S.UA.firefox || S.UA.chrome || S.UA.ie > 9){
                if(loading){
                    loading.className = 'page-loading';
                    loading.removeAttribute('style');
                } else {
                    loading = document.createElement('div');
                    loading.className = 'page-loading';
                    loading.innerHTML = '<div class="spinner"><div></div><div></div><div></div><div></div><div></div><div></div><aside>页面加载中...</aside></div>';
                    document.body.appendChild(loading);
                }
                if(opts){
                    opts = S.mix({top: 0, left: 0, width: -1}, opts);
                    (opts.top != 0 && (loading.style.top = opts.top + 'px'));
                    (opts.left != 0 && (loading.style.left = opts.left + 'px'));
                    (opts.width > 0 && (loading.style.width = opts.width + 'px'));
                }
            }
        },
        pageLoaded: function (){
            if(!loading) return false;
            loading.className += ' page-loaded';
            setTimeout(function (){
                loading.style.display = 'none';
                loading.style.zIndex = -1;
            }, 1000);
        },
        /**
         * 缩略图
         * @param url 原图链接
         * @param width 宽度
         * @param height 高度
         * @returns {string} 缩略图链接
         */
        makeThumb: function (url, width, height){
            if(!url) return url;
            width = width || 50;
            height = height || 'auto';
            return url.replace(/(\.[a-z]{3,4})$/gi, '_s' + width + 'x' + height + '$1');
        },
        /**
         * 格式化数字
         * @param num
         * @returns {*}
         */
        formatNum: function (num){
            if(!/^(\+|-)?\d+(\.\d+)?$/.test(num))
                return num;
            var reg = new RegExp().compile("(\\d)(\\d{3})(,|\\.|$)");
            num += "";
            while (reg.test(num))
                num = num.replace(reg, "$1,$2$3");
            return num;
        },
        /**
         * 分页计算
         * @param option 配置
         */
        page: function (option){
            var opt = $.extend({}, {
                    current: 0,     //当前页
                    size: 15,       //每页显示数
                    total: 0,       //总数
                    near: 3          //显示附近页码数
                }, option || {}),
                totalPage,
                pages = {
                    prev: false,
                    next: false,
                    current: 1,
                    data: []
                }, i, page;
            if(opt.total <= 0 || opt.size <= 0)
                return pages;
            totalPage = Math.ceil(opt.total / opt.size);
            if(totalPage < 2)
                return pages;
            if(opt.current < 0) opt.current = 0;
            if(opt.current >= totalPage) opt.current = totalPage - 1;
            pages.prev = opt.current > 0;
            pages.next = opt.current < totalPage - 1;
            pages.current = opt.current + 1;
            if(totalPage < 9){
                for (i = 0; i < totalPage; i++) {
                    pages.data.push({
                        page: i + 1,
                        isActive: opt.current == i
                    });
                }
                return pages;
            }
            pages.data.push({
                page: 1,
                isActive: opt.current == 0
            });
            page = opt.current - opt.near;
            if(page >= 2){
                if(page == 2)
                    pages.data.push({page: page, isActive: false});
                else
                    pages.data.push({page: -1});
            }
            for (i = opt.current - opt.near; i <= opt.current + opt.near; i++) {
                if(i < 1 || i > totalPage - 2) continue;
                pages.data.push({
                    page: i + 1,
                    isActive: opt.current == i
                });
            }
            page = opt.current + opt.near + 1;
            if(totalPage >= page + 2){
                if(totalPage > page + 2)
                    pages.data.push({page: -1});
                else
                    pages.data.push({page: totalPage - 1, isActive: false});
            }
            pages.data.push({
                page: totalPage,
                isActive: opt.current == totalPage - 1
            });
            return pages;
        },
        /**
         * 学段
         */
        stages: [
            {id: 1, name: '小学'},
            {id: 2, name: '初中'},
            {id: 3, name: '高中'}
        ]
    });
    if(S.deyi.pageLoading){
        S.pageLoading();
        window.onload = function (){
            S.pageLoaded();
        };
    }
    var arrayCk, arrayAndCallCk;
    arrayCk = function (list){
        return list && list.length && S.isArray(list);
    };
    arrayAndCallCk = function (list, fn){
        return fn && S.isFunction(fn) && arrayCk(list);
    };
    /**
     * singer.array
     */
    S._mix(S, {
        array: {
            /**
             * 还回数组值
             * @param list
             * @param fn
             * @returns {*}
             */
            find: function (list, fn){
                if(!arrayAndCallCk(list, fn))
                    return 0;
                for (var i = 0; i < list.length; i++) {
                    if(fn.call(null, list[i], i) === true)
                        return list[i];
                }
                return 0;
            },
            filter: function (list, fn){
                var result = [];
                if(!arrayAndCallCk(list, fn))
                    return result;
                for (var i = 0; i < list.length; i++) {
                    if(fn.call(null, list[i], i) === true)
                        result.push(list[i]);
                }
                return result;
            },
            /**
             * 数组颠倒操作
             * @param list
             * @returns {Array}
             */
            reverse: function (list){
                var result = [];
                if(!arrayCk(list)) return result;
                for (var i = list.length - 1; i >= 0; i--) {
                    result.push(list[i]);
                }
                return result;
            }
        }
    });
})(SINGER);

