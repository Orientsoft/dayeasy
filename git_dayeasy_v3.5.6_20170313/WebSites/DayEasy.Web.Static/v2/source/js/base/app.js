/**
 * Angular App
 * Created by shoy on 2014/10/24.
 */
angular.module("dayeasyApp", ["ngRoute"])
    .factory('dayeasyInjector', ['$q', '$injector', function ($q, $injector) {
        /**
         * 过滤器
         */
        return {
            request: function (config) {
//                if (config.data) {
//                    config.data = (angular.isObject(config.data) && String(config.data) !== '[object File]')
//                        ? singer.stringify(angular.fromJson(angular.toJson(config.data)))
//                        : config.data;
//                }
                return config;
            }, response: function (config) {
                var json = angular.toJson(config.data);
                if (json && json.login) {
                    try {
                        window.top.location.href = json.url;
                        return false;
                    } catch (e) {
                    }
                }
                return config;
            }
        };
    }])
    .config(['$routeProvider', '$httpProvider', function ($routeProvider, $httpProvider) {
        //解决AngularJS的$http.post的XHR2 header BUG
        $httpProvider.defaults.useXDomain = true;
        delete $httpProvider.defaults.headers.common['X-Requested-With'];
        $httpProvider.defaults.headers.post['X-Requested-With'] = 'XMLHttpRequest';//IsAjaxRequest()
//        $httpProvider.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded; charset=UTF-8';
        $httpProvider.interceptors.push('dayeasyInjector');
    }]
);