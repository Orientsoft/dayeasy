/**
 * Created by Boz 2016/6/23.
 */
(function ($, S){
    var $mobile   = $('#mobile'),
        $vcode    = $('#vcode'),
        $vcodeBen = $('#btnSendCode'),
        $Submit   = $('#perSubmit');

    var $tabBxoOna   = $('.j-bxo-ona'),
        $tabBxoTwo   = $('.j-bxo-two'),
        $guserId     = $('.g-user-id'),
        $header      = $('.d-login-title'),
        $hpBack      = $header.find('.hp-left'),
        $headerTitle = $header.find('.hp-h1'),
        $newpwd      = $('#newpwd'),
        $pwd         = $('#pwd'),
        $submibtn    = $('#submibtns');


    /*获取验证码*/
    $vcodeBen.on('click', function (){
        var _this = $(this);
        if(!S.checkPhone($mobile)){
            return false;
        }
        /*短信验证码获取*/
        $.dAjax({
            method   : 'message_sendSmsCode',
            mobile   : $mobile.val(),
            check   :false,
            isEncrypt: false
        }, function (json){
            S.Reacquisition($mobile, _this);
        }, true);
    });

 // 18180230210   214829
    /*NEXT*/
    $Submit.click(function (){
        /*IF*/
        if($(this).hasClass('disabled')){
            return false
        }
        if(!S.checkPhone($mobile) || !S.checkVcod($vcode)){
            return false
        }

        /*Next Show*/
        $tabBxoOna.addClass('hide');
        $tabBxoTwo.removeClass('hide');
        /*Tab Show Data*/
        $guserId.text($mobile.val());
        $headerTitle.text('设置密码');
        $hpBack.addClass('hide');
    });
    /*submi Btn*/
    $submibtn.click(function (event){
        var $this = $(this);
        if($this.hasClass('disabled')){
            return false;
        }
        var _newpwdval = $newpwd.val()
        var _pwdval    = $pwd.val()
        if(_newpwdval == _pwdval){
            /*用户接口  重置密码*/
            $.dAjax({
                method   : 'user_resetPwd',
                mobile   : $mobile.val(),
                password : $pwd.val(),
                vcode    : $vcode.val(),
                isEncrypt: false
            }, function (json){
                if(json.status){
                    window.location='/page/account/resetpwd-success.html';
                }else{
                    $hpBack.removeClass('hide');
                    $tabBxoOna.removeClass('hide');
                    $tabBxoTwo.addClass('hide');
                    $headerTitle.text('验证身份');
                    S.FocusFun($vcode,'验证码不一致');
                }

            }, true);

        } else {
            S.FocusFun($pwd, "密码不一致");
            return false;
        }
    });
    /*SHOW JS*/
    /**
     * Show Password UI
     */
    $('.d-btn-pwd').bind('click', function (){
        var $t    = $(this),
            $warp = $t.parents('.d-input'),
            $pwd  = $warp.find('.d-text');
        if($pwd.attr('type') === "password"){
            $pwd.attr('type', 'text');
            $t.addClass('d-btn-on');
        } else {
            $pwd.attr('type', 'password');
            $t.removeClass('d-btn-on');
        }
    });
    // input 不为空 下一步 btn点亮
    $('#perForm')
        // 文本提交规则
        .on('keyup', '#mobile,#vcode', function (event){
            event.preventDefault();
            var _mobile = $mobile.val(),
                _vcode  = $vcode.val();
            !(_mobile == '') && !(_vcode == '')
                ? $Submit.removeClass('disabled')
                : $Submit.addClass('disabled');
        })
        // 文本提交规则
        .on('keyup', '#newpwd,#pwd', function (event){
            event.preventDefault();
            var _newpwd = $newpwd.val(),
                _pwd    = $pwd.val();
            !(_newpwd == '') && !(_pwd == '')
                ? $submibtn.removeClass('disabled')
                : $submibtn.addClass('disabled');
        });


})(jQuery, SINGER);


