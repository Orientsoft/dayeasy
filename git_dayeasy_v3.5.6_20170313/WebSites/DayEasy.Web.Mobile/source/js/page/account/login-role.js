/**
 * Created by Boz 2016/5/17.
 */
(function ($, S){
    /**
     * 注册身份效果
     */
    var $checkbox     = $('.role-checkbox');
    var $checkbox_li  = $checkbox.find('.hang-base');
    var $subject_li   = $('.subject');
    var $login_header = $('d-base-header');
    var $hp_text      = $('#textid');
    var $hp_right     = $login_header.find('.hp-right');
    var $NextStep     = $('#NextStep');  // 下一步
    /*注册页面*/
    var $notesLogin = $('.notes-login');

    /*获取用户角色*/
    /* 2  学生   4教师   1家长 */
    /*教师学科*/
    var dataRole, dataSubject = 0;
    /*表单前端验证*/
    var $mobile   = $('#mobile');
    var $password = $('#password');
    var $vcode    = $('#vcode');

    /**
     * 验证返回接口数据类型
     * @param 请输入密码  密码无6位数 无法提交
     * @param 短信验证码验证失败
     * @param 帐号已被使用
     * @returns {boolean}
     */
    /*错误提示函数*/
    function FocusFun(obj, text){
        obj.parents('.d-input').removeClass('d-input-err').find('.d-err-tips').remove();
        obj.parents('.d-input').addClass('d-input-err').append('<span class="d-err-tips">' + text + '</span>');
        obj.focus(function (){
            $(this).parents('.d-input').removeClass('d-input-err').find('.d-err-tips').remove();
        });
    }
    function isEmail(field){
        if(field.indexOf("帐号已被使用") !== -1){
            FocusFun($mobile, '帐号已被使用');
        }
          if(field.indexOf("帐号已注册") !== -1){
            FocusFun($mobile, '帐号已注册！');
              return false;
        }
        if(field.indexOf("手机号不正确") !== -1){
            FocusFun($mobile, '手机号不正确');
        }
        if(field.indexOf("密码") !== -1){
            FocusFun($password, '最少6位密码');
        }

        if(field.indexOf("短信验证码验证失败") !== -1){
            FocusFun($vcode, '验证码验证失败');
        }
        if(field.indexOf("签名无效") !== -1){
            FocusFun($vcode, '验证码错误');
        }
    }
    /*手机号码正则*/
    function checkPhone(phone){
        if(!(/^1[3|4|5|7|8]\d{9}$/.test(phone))){
            isEmail('手机号不正确');
        }
        return false;
    }
    /*密码必须为6-20位字符*/
    function password(psd){
        if(!(/^[^\s]{6,20}$/.test(psd))){
            isEmail('最少6位密码');
        }
        return false;
    }
    // 选择角色
    $checkbox_li.not("#selected").on('click', function (){
        $(this).addClass('on').siblings('.hang-base').removeClass('on');
        $NextStep.find('span').removeClass('disabled');
    });
    // 选择科目
    $subject_li.on('click', 'li', function (){
        var _this = $(this);
        _this.addClass('on').siblings().removeClass('on');
        var Otext = _this.find('span').text();
        $checkbox_li.find('.b-text').text(Otext);
        $checkbox_li.find('.b-text').text(Otext).parents('.hang-base').addClass('on').siblings().removeClass('on');
        $NextStep.find('span').removeClass('disabled');
        $('.closure-box').remove();
        $.panelslider.close();
    });
    // 下一步操作
    $NextStep.on('click', '.d-btn', function (){
        // 选择角色判断
        if($checkbox.find('.on').length){
            $checkbox.hide();
            $hp_right.hide();
            $notesLogin.show();

            $hp_text.text('注册账号')
                .end().find('.hp-text').show();
            $NextStep.find('.d-err-tips').remove();
        } else {
            $NextStep.find('.d-err-tips').remove();
            S.msg('请选择用户身份');
        }
    })
    /*模拟返回上一步操作*/
    $('.hp-text').click(function (){
        $(this).hide();
        $notesLogin.hide();
        $hp_text.text('注册身份');
        $checkbox.show();
        $hp_right.css('display', 'inline-block');
    });
    /**
     * 验证码图片刷新
     */
//    $('#identityCode').click(function (){
//        $(this).attr("src", "http://account.dayeasy.dev/login/vcode?rnd" + Math.random());
//        return false;
//    });
//
    /*获取验证码*/
    $('#btnSendCode').on('click', function (){

        var _this = $(this);
        /*短信验证码获取*/
        //注册测试手机号码： 18280380118     911148  784665  //
        $.dAjax({
            method   : 'message_sendSmsCode',
            mobile   : $mobile.val(),
            isEncrypt: false
        }, function (json){
            console.log(json);
            if(!json.status){
                isEmail(json.message);
                return false;
            }
            S.Reacquisition($mobile, _this);
        }, true);
    });
    $('#perForm')
        // 文本提交规则
        .on('keyup', '#mobile,#password,#vcode', function (event){
            event.preventDefault();
            var _mobile   = $mobile.val(),
                _password = $password.val(),
                _vcode    = $vcode.val();
            !(_mobile == '') && !(_password == '') && !(_vcode == '')
                ? $('#perSubmit').removeClass('disabled')
                : $('#perSubmit').addClass('disabled');
        });
    /**
     * 表单提交
     * @param mobile 手机号
     * @param password 新密码
     * @param vcode  短信验证码
     * @param isEncrypt  密码加密
     */
    $('#perSubmit').click(function (event){
        if($(this).hasClass('disabled')){
            return false;
        }
        var user_name = $mobile.val(); // 手机号 ID
        var user_pwd = $password.val(); // 新密码 ID
        var user_vcode = $vcode.val(); // 手机短信验证码 ID

        checkPhone(user_name);
        password(user_pwd);

        $('.hang-base').each(function (){
            var $this = $(this);
            if($this.hasClass('on')){
                dataRole = $this.find('.role').data('role');
                if(dataRole == 4){
                    $('#right-panel').find('li').each(function (){
                        if($(this).hasClass('on')){
                            dataSubject = $(this).find('span').data('subject');
                        }
                    });
                    return false;
                }
                dataSubject = 0;
            }
        })

        /*提交注册信息*/
        $.dAjax({
                method   : "user_regist",
                mobile   : user_name,
                password : user_pwd,
                vcode    : user_vcode,
                role     : dataRole,
                subjectId: dataSubject,
                isEncrypt: false
            },
            function (json){
                if(json.status){
                    // 登录成功
                    $.setToken(json.data.token);
                    /*角色提交*/
                    window.location.href = "/page/account/signup-success.html";
                } else {
                    // 登录失败
                    /*检测返回错误信息*/
                    isEmail(json.message);
                }
            }, "json");
        return false;
    });

    $('#perForm').submit(function (){
    });


    /**
     * 显示密码
     */
    $('.d-btn-pwd').bind('click', function () {

        var $t = $(this),
            $pwd = $('#password');
        if ($pwd.attr('type') === "password") {
            $pwd.attr('type', 'text');
            $t.addClass('d-btn-on');
        } else {
            $pwd.attr('type', 'password');
            $t.removeClass('d-btn-on');
        }
    });

})(jQuery, SINGER);


