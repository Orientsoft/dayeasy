/**
 * Created by bozhuang on 2016/8/10.
 */

// Tag 指定颜色随机;
var msg = ["bg0", "bg1", "bg2"];
$('.sort-sticker dl').each(function () {
    $(this).addClass(msg[Math.floor(Math.random() * msg.length)]);
});

