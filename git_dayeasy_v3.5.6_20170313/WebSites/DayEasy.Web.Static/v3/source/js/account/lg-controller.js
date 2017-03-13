var deyi = window.DEYI || {},
    sites = deyi.sites || {},
    mainCtrl = ['$scope', function ($scope) {
    }],
    loginCtrl = [
        '$scope', '$http', function ($scope, $http) {
            var EMPTY = '',
                errorSubmit,
                checkField,
                showError,
                emails = [
                    'dayeasy.net', 'dy.com', 'qq.com',
                    '163.com', '126.com', 'sina.com', 'sohu.com',
                    'yahoo.cn', 'hotmail.com', 'live.cn',
                    'outlook.com'
                ];
            $scope.user = {account: EMPTY, password: EMPTY};
            $scope.error = {type: EMPTY, msg: EMPTY, count: 0, link: EMPTY, linkText: EMPTY};
            $scope.logining = false;
            $scope.vcodeUrl = EMPTY;
            $scope.refreshVcode = function () {
                $scope.vcodeUrl = '/login/vcode?t=' + Math.random();
            };
            /**
             * 错误的提交
             */
            errorSubmit = function () {
                $scope.error.count = ~~(singer.cookie.get('__dayeasy_err_c') || '0');
                if ($scope.error.count > 2) {
                    $scope.refreshVcode();
                }
            };
            /**
             * 表单检查
             */
            checkField = function () {
                var account = $scope.user.account,
                    accountError = false,
                    emailReg = /^[a-zA-Z0-9_]+@[a-zA-Z0-9]+.[a-zA-Z]+$/,
                    mobileReg = /^1[3|5|7|8][0-9]{9}$/,
                    codeReg = /^[\d]{5,}$/g;

                if (!accountError && (!account || !account.length)) {
                    accountError = true;
                    $scope.error.msg = '请输入登录帐号！';
                }
                if (!accountError && !emailReg.test(account) && !mobileReg.test(account) && !codeReg.test(account)) {
                    accountError = true;
                    $scope.error.msg = '登录帐号格式不正确！';
                }
                if (accountError) {
                    $scope.error.type = 'error-show';
                    try {
                        loginForm.account.focus();
                    } catch (e) {
                    }
                    return false;
                }
                if ($scope.loginForm.$valid) return true;
                $scope.error.type = 'error-show';
                if ($scope.loginForm.account.$invalid) {
                    $scope.error.type += ' account';
                    if ($scope.loginForm.account.$error.required) {
                        $scope.error.msg = '请输入登录帐号！';
                    }
                    try {
                        loginForm.account.focus();
                    } catch (e) {
                    }
                } else if ($scope.loginForm.pwd.$invalid) {
                    $scope.error.type += ' password';
                    if ($scope.loginForm.pwd.$error.required) {
                        $scope.error.msg = '请输入登录密码！';
                    } else if ($scope.loginForm.pwd.$error.minlength) {
                        $scope.error.msg = '登录密码不少于6位！';
                    }
                    try {
                        loginForm.pwd.focus();
                    } catch (e) {
                    }
                } else if ($scope.loginForm.vcode.$invalid) {
                    $scope.error.type += ' vcode';
                    if ($scope.loginForm.vcode.$error.required) {
                        $scope.error.msg = '请输入验证码！';
                    } else if ($scope.loginForm.vocde.$error.minlength || $scope.loginForm.vocde.$error.maxlength) {
                        $scope.error.msg = '验证码应为4位！';
                    }
                    try {
                        loginForm.vcode.focus();
                    } catch (e) {
                    }
                }
                return false;
            };
            /**
             * 显示错误信息
             * @param msg
             */
            showError = function (msg) {
                $scope.error.type = 'error-show';
                if (msg.indexOf('密码') >= 0) {
                    $scope.error.type += ' password';
                    try {
                        loginForm.pwd.focus();
                    } catch (e) {
                    }
                } else if (msg.indexOf('验证码') >= 0) {
                    $scope.error.type += ' vcode';
                    try {
                        loginForm.vcode.focus();
                    } catch (e) {
                    }
                } else {
                    try {
                        loginForm.account.focus();
                    } catch (e) {
                    }
                }
                if (msg.indexOf('尚未注册') >= 0) {
                    $scope.error.link = (sites.reg || "http://reg.dayeasy.net") + "?email=" + encodeURIComponent($scope.user.account);
                    $scope.error.linkText = '立即注册';
                } else {
                    $scope.error.link = EMPTY;
                    $scope.error.linkText = EMPTY;
                }
                $scope.error.msg = msg;
            };
            errorSubmit();
            $scope.maskClick = function () {
                $scope.error.type = EMPTY;
                $scope.error.msg = EMPTY;
            };
            /**
             * 登录
             */
            $scope.login = function () {
                $scope.logining = true;
                $scope.error.type = EMPTY;
                if (!checkField()) {
                    $scope.logining = false;
                    return false;
                }
                $http.post('/login/login', $scope.user)
                    .success(function (json) {
                        if (!json.status) {
                            errorSubmit();
                            showError(json.message);
                            $scope.logining = false;
                            return false;
                        }
                        var returnUrl = decodeURIComponent(singer.uri().return_url || '') || sites.main + '/user';
                        if (returnUrl.indexOf('login') >= 0)
                            returnUrl = sites.main;
                        location.href = returnUrl;
                        return true;
                    })
                    .error(function (error) {
                        $scope.logining = false;
                        errorSubmit();
                        $scope.error.type = 'error-show';
                        $scope.error.msg = '登录超时，请重试！';
                    });
                return true;
            };
            $scope.thirdLogin = function (type) {
                window.location.href = '/login/third-login?type=' + type;
                return false;
            };
            $('#email').autoMail({
                emails: emails,
                onChange: function () {
                    $scope.user.account = $('#email').val();
                }
            });
        }
    ];
(function ($) {
    var dataList = [],
        imageList = [],
        i,
        bindBackStretch,
        $control = $('.slider-controls');
    $.get(sites.static + '/v3/data/login-data.json', function (json) {
        dataList = json;
        bindBackStretch();
    }, "json");
    /**
     * 绑定背景
     */
    bindBackStretch = function () {
        for (i = 0; i < dataList.length; i++) {
            dataList[i].src && imageList.push(sites.static + '/v3' + dataList[i].src);
        }
        $(".lg-slider").backstretch(imageList, {
            fade: 1000,
            duration: 8000
        });
        for (i = 0; i < imageList.length; i++) {
            $control.append('<li class="' + (i == 0 ? 'active' : '') + '">');
        }
        $(window).on("backstretch.before", function (e, instance, index) {
            var item = dataList[index],
                $stretch = $('.backstretch');
            if (!item) return false;
            if (item.title) {
                $stretch.attr('title', item.title);
            } else {
                $stretch.attr("title", "");
            }
            if (item.link) {
                $stretch.css('cursor', 'pointer');
            } else {
                $stretch.css('cursor', 'default');
            }
            var $t = $control.find('li:eq(' + index + ')');
            $control.find('li').not($t).removeClass('active');
            $t.addClass('active');
        });
        $('.j-autoHeight').animate({opacity: 1,},200);
    };

    $(window).bind("resize.backstretch", function () {
        $(".lg-slider").backstretch('resize');
    });
    $(document)
        .delegate('.slider-controls li', 'click', function () {
            var $t = $(this),
                $controls = $control.find('li'),
                index = $controls.index($t);
            if ($t.hasClass('active')) return false;
            $('.lg-slider').backstretch('show', index);
            $controls.not($t).removeClass('active');
            $t.addClass('active');
            return false;
        })
        .delegate('.backstretch', 'click', function (e) {
            e.stopPropagation();
            var index = $('.lg-slider').data('backstretch').index;
            if (dataList[index] && dataList[index].link) {
                singer.open(dataList[index].link);
            }
        });
})(jQuery);