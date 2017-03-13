define("js/styles/component/v1/slideud/slideudown-debug", [ "gallery/jquery/1.8.2/jquery-debug" ], function(require, exports, module) {
    var $ = require("gallery/jquery/1.8.2/jquery-debug");
    function wantTo(obj, otop, Speed, oclass) {
        $(window).scroll(function() {
            if ($(window).scrollTop() > otop) {
                obj.slideDown(Speed).addClass(oclass);
            } else {
                obj.removeClass(oclass);
            }
        });
    }
    exports.wantTo = wantTo;
});
