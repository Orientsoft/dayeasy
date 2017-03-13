/**
 * Created by Bai on 2016/12/26.
 */
(function ($,S){
    var myLinks = [
        S.sites.static + "/v3/plugs/activity2017/teacherday/topblock.min.css",
        S.sites.static + '/v3/plugs/activity2017/teacherday/index.min.js',
    ];
    S.loadScript(myLinks[0], {
        success: function (){
            S.loadScript(myLinks[1], {
                success: function (){
                    var $bottomBlock=$('#bottomBlock');
                    if($bottomBlock.data('isstudentindex')=='StudentIndex'){
                        $('.bottomblock2016OpenBtn').trigger('click');
                    }
                },
                charset: 'utf-8'
            });
        },
        charset: 'utf-8'
    });
})(jQuery, SINGER);