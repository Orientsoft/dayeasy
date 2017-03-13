/**
 * api接口请求辅助
 * 需要引用md5js
 * Created by shay on 2015/2/11.
 *
 */
(function ($) {
    var rest,
        router,
        tickRest,
        key = {partner: 'dayeasy_mobile', secret: 'l%DD%AAcegik'},
        getTick, param, objSort, initHost,
        compile, unCompile, isFunction, currentTick, currentTime, userToken;
    userToken = (function () {
        return singer.cookie.get('__dayeasy_u');
    })();
    /**
     * 初始化域名
     */
    initHost = function () {
        var host = location.hostname;
        var sites = {};
        rest = 'http://open.v3.deyi.com/';
        if (host.indexOf('.dev') >= 0) {
            sites.domain = '.dayeasy.dev';
        } else if (host.indexOf('deyi.com') >= 0) {
            sites.domain = '.deyi.com';
        } else if (host.indexOf('dayeasy.net') >= 0) {
            rest = 'http://open.dayeasy.net/';
            pk = '';
            sites.domain = '.dayeasy.net';
        } else {
            sites.domain = '';
        }
        router = rest + 'v3/';
        tickRest = rest + 'ticks';
        sites.main = singer.format('http://www{0}/', sites.domain == '' ? '.dayeasy.dev' : sites.domain);
        singer.sites = sites;
    };
    initHost();

    /**
     * 参数化
     * @param obj
     * @param unEncoding
     * @returns {string}
     */
    param = function (obj, unEncoding) {
        var query = '';
        var name, value, fullSubName, subName, subValue, innerObj, i;

        for (name in obj) {
            if (!obj.hasOwnProperty(name))
                continue;
            value = obj[name];
            if (value instanceof Array) {
                for (i = 0; i < value.length; ++i) {
                    subValue = value[i];
                    fullSubName = name + '[' + i + ']';
                    innerObj = {};
                    innerObj[fullSubName] = subValue;
                    query += param(innerObj) + '&';
                }
            } else if (value instanceof Object) {
                for (subName in value) {
                    if (!value.hasOwnProperty(subName))
                        continue;
                    subValue = value[subName];
                    fullSubName = name + '[' + subName + ']';
                    innerObj = {};
                    innerObj[fullSubName] = subValue;
                    query += param(innerObj) + '&';
                }
            } else if (value !== undefined && value !== null) {
                if (!unEncoding) {
                    query += encodeURIComponent(name) + '='
                        + encodeURIComponent(decodeURIComponent(value)) + '&';
                } else {
                    query += name + '=' + decodeURIComponent(value) + '&';
                }
            }
        }
        return query.length ? query.substr(0, query.length - 1) : query;
    };
    /**
     * Object对象冒泡排序
     * @param obj
     * @returns {{}}
     */
    objSort = function (obj) {
        var a = [], temp = {}, i;
        for (i in obj)
            a.push(i);
        a.sort(function (a, b) {
            return a < b ? 1 : -1;
        });
        while (a.length > 0) {
            i = a.pop();
            temp[i] = obj[i];
        }
        return temp;
    };
    /**
     * escape加密
     * @param code
     * @returns {*}
     */
    compile = function (code) {
        var c = String.fromCharCode(code.charCodeAt(0) + code.length);
        for (var i = 1; i < code.length; i++) {
            c += String.fromCharCode(code.charCodeAt(i) + code.charCodeAt(i - 1));
        }
        return escape(c);
    };
    /**
     * escape解密
     * @param code
     * @returns {string}
     */
    unCompile = function (code) {
        if (!this.transformResponse)
            return 'return false';
        code = unescape(code);
        var c = String.fromCharCode(code.charCodeAt(0) - code.length);
        for (var i = 1; i < code.length; i++) {
            c += String.fromCharCode(code.charCodeAt(i) - c.charCodeAt(i - 1));
        }
        return c;
    };
    isFunction = function (fn) {
        return fn && typeof fn === "function";
    };
    key.secret = unCompile.call({transformResponse: true}, key.secret);
    /**
     * 获取服务器时间戳
     * @param callback
     */
    getTick = function (callback) {
        if (!currentTick) {
            $.get(tickRest, function (data) {
                currentTick = data;
                currentTime = new Date();
                isFunction(callback) && callback.call(this, data);
            });
        } else {
            var tick = currentTick + (new Date() - currentTime);
            isFunction(callback) && callback.call(this, tick);
        }
    };
    /**
     * jQuery扩展
     */
    $.extend({
        /**
         * 接口请求
         * @param data 请求数据
         * @param callback 回调函数
         * @param isPost 是否POST方式请求
         * @returns {boolean}
         */
        dAjax: function (data, callback, isPost) {
            if (!key.partner)
                return false;
            delete data.partner;
            getTick(function (tick) {
                data.tick = tick;
                var url = router + data.method;
                delete data.method;
                if (userToken) {
                    data.token = userToken;
                }
                data = objSort(data);
                var pm = param(data, true) + '+' + key.secret;
                console.log(pm);
                data.sign = $.md5(pm);
                data.partner = key.partner;
                var type = isPost ? 'POST' : 'GET';
                $.ajax({
                    url: url,
                    type: type,
                    dataType: 'json',
                    data: data,
                    success: function (json) {
                        // console.log(json);
                        isFunction(callback) && callback.call(this, json);
                    }
                });
            });
        },
        dCompile: compile,
        /**
         * 验证返回的Json
         * @param json
         * @param success
         * @param error
         */
        dValid: function (json, success, error) {
            if (!json.status) {
                isFunction(error) && error.call(this, json.desc);
                return false;
            }
            isFunction(success) && success.call(this, json.data);
            return true;
        },
        /**
         * 设置用户登录凭证
         * @param token
         */
        setToken: function (token) {
            userToken = token;
            singer.cookie.set('__dayeasy_u', token, 0, singer.sites.domain);
        }
    });
})(jQuery);
/**
 dAjax示例：
 $.dAjax({
   method:'user_login',
   account:'{user_account}',
   password:'{user_password}',
   isEncrypt:false
  },function(json){
   if(json.status){
       $.setToken(json.data.token);
       var user = json.data.user;
       console.log(user);
   }else{
       alert(json.message);
  }
  },true);


 */

