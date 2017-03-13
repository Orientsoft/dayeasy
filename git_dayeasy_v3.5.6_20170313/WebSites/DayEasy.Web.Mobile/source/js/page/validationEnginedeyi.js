/**
 * 验证规则
 * Created by boz on 2016/6/23.
 */
(function ($, S){

    S.mix(S, {
        /*验证码6位数正则*/
        checkVcod: function (obj){
            var objval =obj.val();
            if(!(/^\d{6}$/.test(objval))){
//                S.isEmail('验证码不正确',obj);
                S.FocusFun(obj, '验证码不正确');
                return false;
            }else{
                return true;
            }
        },  /*手机号码正则*/
        checkPhone: function (obj){
            var objval =obj.val();
            if(!(/^1[3|4|5|7|8]\d{9}$/.test(objval))){
                S.isEmail('手机号不正确',obj);
                return false;
            }else{
                return true;
            }

        },
        /*密码必须为6-20位字符*/
        password     : function (psd){
            if(!(/^[^\s]{6,20}$/.test(psd))){
                isEmail('最少6位密码');
            }
            return false;
        },
        //obj 展示对象
        //text  提示的文本
        FocusFun     : function (obj, text){
            obj.parents('.d-input').removeClass('d-input-err').find('.d-err-tips').remove();
            obj.parents('.d-input').addClass('d-input-err').append('<span class="d-err-tips">' + text + '</span>');
            obj.focus(function (){
                $(this).parents('.d-input').removeClass('d-input-err').find('.d-err-tips').remove();
            });
        },
        isEmail      : function (field, obj){

            if(field.indexOf("验证码不一致") !== -1){
                S.FocusFun(obj, '验证码不一致');
                return false;
            }

            if(field.indexOf("帐号已被使用") !== -1){
                S.FocusFun(obj, '帐号已被使用');
                return false;
            }
            if(field.indexOf("手机号不正确") !== -1){
                S.FocusFun(obj, '手机号不正确');
                return false;
            }
            if(field.indexOf("密码") !== -1){
                S.FocusFun(obj, '最少6位密码');
                return false;
            }
            if(field.indexOf("短信验证码验证失败") !== -1){
                S.FocusFun(obj, '短信验证码验证失败');
                return false;
            }
            if(field.indexOf("签名无效") !== -1){
                S.FocusFun(obj, '验证码错误');
                return false;
            }
        },
        //获取验证码 60秒倒计时
        //*Obj 手机 input 对象
        //*_this  获取验证码obj对象
        Reacquisition: function (Obj, Obj_vcode){
            // 60秒重新获取验证码
            var countdown = 60;
            var re        = /^([\+][0-9]{1,3}[ \.\-])?([\(]{1}[0-9]{2,6}[\)])?([0-9 \.\-\/]{3,20})((x|ext|extension)[ ]?[0-9]{1,4})?$/; // 电话号码正则
            var result = re.test(Obj.val());

            if(Obj.val() !== '' && result){
                settime(Obj_vcode[0]);
            }
            function settime(_thisval){
                if(countdown == 0){
                    _thisval.removeAttribute("disabled");
                    $(_thisval).removeClass('disabled');
                    _thisval.value = "获取验证码";
                    countdown      = 60;
                    return;
                } else {
                    _thisval.setAttribute("disabled", true);
                    $(_thisval).addClass('disabled');
                    _thisval.value = "已发送(" + countdown + "S)";
                    countdown--;
                }
                setTimeout(function (){
                        settime(_thisval)
                    }
                    , 1000)

                return false;
            }
        }
    });

})(jQuery, SINGER);