(function ($) {
    //jquery的全局对象扩展
    $.extend({
        Dayez: {
            //这是在jquery全局对象中扩展一个Dayez命名空间。  
            alert: function (content) {
                var d = dialog({
                    title: "消息提示",
                    content: content,
                    okValue: "确定",
                    ok: function () {
                    },
                    cancel: false,
                    backdropBackground: '#000',
                    backdropOpacity: 0.3
                }).width(300).showModal();

                return $(d);
            },
            confirm: function (content, okFunction, cancelFunction) {
                var d = dialog({
                    title: "消息确认提示",
                    backdropBackground: '#000',
                    backdropOpacity: 0.3,
                    content: content,
                    okValue: "确定",
                    ok: okFunction,
                    cancelValue: "取消",
                    cancel: cancelFunction
                }).width(300).showModal();

                return $(d);
            },
            tip: function (obj, content, position) {
                var d = dialog({
                    align: position,
                    content: content,
                    quickClose: true
                }).show(obj[0]);

                return $(d);
            },
            msg: function (content, time) {
                var d = dialog({
                    content: content
                }).show();

                setTimeout(function () {
                    d.close().remove();
                }, time || 2000);

                return $(d);
            },
            dialog: function (title, content, okFunction, cancelFunction) {
                if (!title) {
                    title = '温馨提示';
                }
                var d = dialog({
                    title: title,
                    content: content,
                    okValue: "确定",
                    ok: okFunction,
                    cancelValue: "取消",
                    cancel: cancelFunction,
                    backdropBackground: '#000',
                    backdropOpacity: 0.3
                }).showModal();

                return $(d);
            },
            showImg: function (url) {
                var $img = $('<img />');
                $img.attr("src", url);
                $img.css("max-width", "640px");

                var d = dialog({
                    title: '图片查看',
                    content: $img[0].outerHTML,
                    backdropBackground: '#000',
                    backdropOpacity: 0.3,
                    quickClose: true
                });
                $img.bind("load", function () {
                    d.showModal();
                });

                return $(d);
            }
        },
        Operation: {
            Add: function (arg1, arg2) {
                var r1, r2, m;
                try {
                    r1 = arg1.toString().split(".")[1].length;
                } catch (e) {
                    r1 = 0;
                }
                try {
                    r2 = arg2.toString().split(".")[1].length;
                } catch (e) {
                    r2 = 0;
                }
                m = Math.pow(10, Math.max(r1, r2));
                return (arg1 * m + arg2 * m) / m;
            },
            Sub: function (arg1, arg2) {
                return this.Add(arg1, -arg2);
            },
            Mul: function (arg1, arg2) {
                var m = 0, s1 = arg1.toString(), s2 = arg2.toString();
                try {
                    m += s1.split(".")[1].length;
                } catch (e) {
                }
                try {
                    m += s2.split(".")[1].length;
                } catch (e) {
                }
                return Number(s1.replace(".", "")) * Number(s2.replace(".", "")) / Math.pow(10, m);
            },
            Div: function (arg1, arg2) {
                var t1 = 0, t2 = 0, r1, r2;
                try {
                    t1 = arg1.toString().split(".")[1].length;
                } catch (e) {
                }
                try {
                    t2 = arg2.toString().split(".")[1].length;
                } catch (e) {
                }
                with (Math) {
                    r1 = Number(arg1.toString().replace(".", ""));
                    r2 = Number(arg2.toString().replace(".", ""));
                    return (r1 / r2) * pow(10, t2 - t1);
                }
            }
        },
        URL: {
            BuildURL: function (url, name, value) {
                var newUrl = "";
                var reg = new RegExp("(^|)" + name + "=([^&]*)(|$)");
                var tmp = name + "=" + value;
                if (url.match(reg) != null) {
                    newUrl = url.replace(eval(reg), tmp);
                }
                else {
                    if (url.match("[\\?]")) {
                        newUrl = url + "&" + tmp;
                    }
                    else {
                        newUrl = url + "?" + tmp;
                    }
                }
                return newUrl;
            }
        }
    });

    //jquery的实例对象扩展
    $.fn.extend({
        time: function (time) { //自动关闭 artDialog 对话框
            var _this = this;
            setTimeout(function () {
                _this[0].close().remove();
            }, time);
        },
        dragResize: function (direction) {
            //初始化参数
            var objs = $(this);
            for (var i = 0; i < objs.length; i++) {
                var option = {
                    direction: direction,
                    minW: objs[i].width,
                    minH: objs[i].height
                };
                new dragChangeSize(objs[i], option);
            }
        },
        countDown: function (totalSeconds) {
            var obj = $(this);
            var sys_second = totalSeconds;
            var timer = setInterval(function () {
                var showText = "";
                if (sys_second > 0) {
                    sys_second -= 1;
                    var day = Math.floor((sys_second / 3600) / 24);
                    var hour = Math.floor((sys_second / 3600) % 24);
                    var minute = Math.floor((sys_second / 60) % 60);
                    var second = Math.floor(sys_second % 60);

                    showText = day + "天";
                    showText += (hour < 10 ? "0" + hour : hour) + "小时";
                    showText += (minute < 10 ? "0" + minute : minute) + "分钟";
                    showText += (second < 10 ? "0" + second : second) + "秒";

                    obj.html(showText);
                } else {
                    obj.html("时间到了！");
                    clearInterval(timer);
                }
            }, 1000);
        }
    });

    //拖动改变大小
    var dragChangeSize = function (obj, options) {
        this.$element = $(obj);
        this.defaults = {
            direction: "",
            minW: 100,
            minH: 40
        };

        this.options = $.extend({}, this.defaults, options);

        var _this = this;
        var el = this.$element[0];
        var els = el.style, x = y = Xm = Ym = 0;//鼠标的 X 和 Y 轴坐标
        $(el).mousedown(function (e) {
            e.stopPropagation();
            //按下元素后，计算当前鼠标与对象计算后的坐标
            x = e.clientX - el.offsetWidth;
            y = e.clientY - el.offsetHeight;
            if (el.setCapture) {
                el.setCapture();
                el.onmousemove = function (ev) {
                    mouseMove(ev || event);
                };
                el.onmouseup = mouseUp;
            } else {
                $(document).bind("mousemove", mouseMove).bind("mouseup", mouseUp); //绑定事件
            }
            //防止默认事件发生
            e.preventDefault();
        });
        //移动事件
        function mouseMove(e) {
            e.stopPropagation();
            if ($.trim(_this.options.direction) == "X") {
                Xm = e.clientX - x;
                Xm <= _this.options.minW && (Xm = _this.options.minW);
                els.width = Xm + 'px';
                $(el).css("max-width", "none");
            }
            else if ($.trim(_this.options.direction) == "Y") {
                Ym = e.clientY - y;
                Ym <= _this.options.minH && (Ym = _this.options.minH);
                els.height = Ym + 'px';
                $(el).css("max-height", "none");
            } else {
                Xm = e.clientX - x;
                Ym = e.clientY - y;
                //限制高宽
                Xm <= _this.options.minW && (Xm = _this.options.minW);
                Ym <= _this.options.minH && (Ym = _this.options.minH);
                //设置大小
                els.width = Xm + 'px';
                els.height = Ym + 'px';

                $(el).css("max-width", "none").css("max-height", "none");
            }
        }

        //停止事件
        function mouseUp() {
            if (el.releaseCapture) {
                el.releaseCapture();
                //移除事件
                el.onmousemove = el.onmouseup = null;
            } else {
                $(document).unbind("mousemove", mouseMove).unbind("mouseup", mouseUp);//卸载事件
            }
        }
    }

    //jquery 原生对象扩展
    Date.prototype.format = function (pattern) {
        /*初始化返回值字符串*/
        var returnValue = pattern;
        /*正则式pattern类型对象定义*/
        var format = {
            "y+": this.getFullYear(),
            "M+": this.getMonth() + 1,
            "d+": this.getDate(),
            "H+": this.getHours(),
            "m+": this.getMinutes(),
            "s+": this.getSeconds(),
            "S": this.getMilliseconds(),
            "h+": (this.getHours() % 12),
            "a": (this.getHours() / 12) <= 1 ? "AM" : "PM"
        };
        /*遍历正则式pattern类型对象构建returnValue对象*/
        for (var key in format) {
            var regExp = new RegExp("(" + key + ")");
            if (regExp.test(returnValue)) {
                var zero = "";
                for (var i = 0; i < RegExp.$1.length; i++) {
                    zero += "0";
                }
                var replacement = RegExp.$1.length == 1 ? format[key] : (zero + format[key]).substring((("" + format[key]).length));
                returnValue = returnValue.replace(RegExp.$1, replacement);
            }
        }
        return returnValue;
    };

    //替换所有
    String.prototype.replaceAll = function (reallyDo, replaceWith, ignoreCase) {
        if (!RegExp.prototype.isPrototypeOf(reallyDo)) {
            return this.replace(new RegExp(reallyDo, (ignoreCase ? "gi" : "g")), replaceWith);
        } else {
            return this.replace(reallyDo, replaceWith);
        }
    };

    //根据json数据替换
    String.prototype.replaceByJson = function (obj) {
        return this.replace(/\{\w+\}/gi, function (matchs) {
            var returns = obj[matchs.replace(/\$/g, "")];
            return (returns + "") == "undefined" ? "" : returns;
        });
    };

    //jquery 验证默认设置
    if ($.validator) {
        $.validator.setDefaults({
            debug: true,
            ignore: ":hidden",
            errorClass: "has-error",
            errorElement: "span",
            highlight: function (element, errorClass) {
                $(element).parent().addClass(errorClass);
            },
            errorPlacement: function (error, element) {
                $(error).addClass("text-danger").insertAfter(element);
            },
            success: function (element) {
                $(element).parent().removeClass("has-error");
            }
        });


        /***********************jquery 验证扩展**************************************/
            //自定义正则验证
        $.validator.addMethod("regex", function (value, element, params) {
            var exp = new RegExp(params);
            return exp.test(value);
        }, "格式错误");

        // 手机号码验证
        $.validator.addMethod("mobile", function (value, element) {
            var length = value.length;
            var mobile = /^(((13[0-9]{1})|(14[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/
            return this.optional(element) || (length == 11 && mobile.test(value));
        }, "手机号码格式错误");

        // 电话号码验证
        $.validator.addMethod("phone", function (value, element) {
            var tel = /^(0[0-9]{2,3}\-)?([2-9][0-9]{6,7})+(\-[0-9]{1,4})?$/;
            return this.optional(element) || (tel.test(value));
        }, "电话号码格式错误");

        //联系电话(手机/电话皆可)验证
        $.validator.addMethod("phoneOrMobile", function (value, element) {
            var length = value.length;
            var mobile = /^(((13[0-9]{1})|(14[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
            var tel = /^\d{3,4}-?\d{7,9}$/;
            return this.optional(element) || (tel.test(value) || mobile.test(value));

        }, "电话格式错误");

        // 邮政编码验证
        $.validator.addMethod("postCode", function (value, element) {
            var tel = /^[0-9]{6}$/;
            return this.optional(element) || (tel.test(value));
        }, "邮政编码格式错误");

        // 身份证号码验证
        $.validator.addMethod("idCard", function (value, element) {
            var tel = /^(^\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$/;
            return this.optional(element) || (tel.test(value));
        }, "身份证号码格式错误");

        // QQ号码验证
        $.validator.addMethod("qq", function (value, element) {
            var tel = /^[1-9]\d{4,9}$/;
            return this.optional(element) || (tel.test(value));
        }, "qq号码格式错误");

        // IP地址验证
        $.validator.addMethod("ip", function (value, element) {
            var ip = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
            return this.optional(element) || (ip.test(value) && (RegExp.$1 < 256 && RegExp.$2 < 256 && RegExp.$3 < 256 && RegExp.$4 < 256));
        }, "Ip地址格式错误");

        // 字母和数字的验证
        $.validator.addMethod("charNum", function (value, element) {
            var chrnum = /^([a-zA-Z0-9]+)$/;
            return this.optional(element) || (chrnum.test(value));
        }, "只能输入数字和字母");

        // 字母和数字和下划线的验证
        $.validator.addMethod("charNumUnderline", function (value, element) {
            var chrnum = /^(\w){6,20}$/;
            return this.optional(element) || (chrnum.test(value));
        }, "只能输入数字、字母、下划线");

        // 中文的验证
        $.validator.addMethod("chinese", function (value, element) {
            var chinese = /^[\u4e00-\u9fa5]+$/;
            return this.optional(element) || (chinese.test(value));
        }, "只能输入中文");

        $.extend($.validator.messages, {
            required: "该内容必填",
            remote: "该内容已存在",
            email: "email格式不正确",
            url: "网址不合法",
            date: "日期不合法",
            dateISO: "日期 (ISO)不合法",
            number: "请输入数字",
            digits: "只能输入整数",
            creditcard: "信用卡号不合法",
            equalTo: "两次输入不一致",
            accept: "字符串后缀不合法",
            maxlength: jQuery.validator.format("长度最多是{0}"),
            minlength: jQuery.validator.format("长度最少是{0}"),
            rangelength: jQuery.validator.format("长度介于{0}和{1}之间"),
            range: jQuery.validator.format("大小介于{0}和{1}之间"),
            max: jQuery.validator.format("最大为{0}"),
            min: jQuery.validator.format("最小为{0}")
        });
    }
})(jQuery);