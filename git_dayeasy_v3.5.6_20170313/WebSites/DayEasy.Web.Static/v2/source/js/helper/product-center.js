$(function () {
    var BV = new $.BigVideo();
    BV.init();
    BV.show(singer.sites.file + '/video/dayeasy-demo.mp4', {ambient: true});
    $(".section .slide:odd .wrapper div").addClass("float_r");
    $("#fullpageContainer").imagesLoaded();

    $("#videoPlay img").click(function () {
        $("#popup").show();
        BV.getPlayer().pause();
    });
    $("#popup .shade,#popup #icon_close").click(function () {
        $("#popup").hide();
        BV.getPlayer().play();
    });
    $("#fullpageContainer").fullpage({
        sectionsColor: ['', '#ffffd1', '#ebffd6', '#dbffed', '#dbedff'],
        slidesColor: ['#1bbc9b', '#4BBFC3', '#7BAABE', '#f90'],
        css3: true,
        resize: false,
        scrollingSpeed: 1000,
        easing: 'easeInOutQuart',
        loopHorizontal: false,
        slidesNavigation: true,
        navigation: true,
        navigationPosition: "right",
        navigationTooltips: ['总体介绍', '我的得一', '阅卷系统', '平板系统', '马上体验'],
        afterLoad: function (anchorLink, index) {
            if (index == 1) {
                BV.getPlayer().play();
            }
        },
        onLeave: function (index, nextIndex, direction) {
            $(this).fullpage.setAllowScrolling(false);
            if (!($(".section:eq(" + (nextIndex - 1) + ") .slide").size())) {
                $(this).fullpage.setAllowScrolling(true);
            }

            if (index == 1) {
                BV.getPlayer().pause();
            }
        },
        afterResize: function () {
            $(".wrapper").css("height", (500 / $(".section:eq(1)").height() * 100) + "%");
        },
        afterRender: function () {
            $(".wrapper").css("height", (500 / $(".section:eq(1)").height() * 100) + "%");
        }
    });

    $("#fullpageContainer .arrowDown").click(function () {
        $("#fullpageContainer").fullpage.moveSectionDown();
    });
    $(".section").each(function (index, element) {
        if (index == $(".section").size() - 1) {
            mouseEventBind($(element).find(".slide"), "leftRight");
            mouseEventBind($(element).find(".slide").first(), "topRight");
        } else {
            mouseEventBind($(element).find(".slide"), "leftRight");
            mouseEventBind($(element).find(".slide").first(), "topRight");
            mouseEventBind($(element).find(".slide").last(), "leftBottom");
        }
    });
});

var timer = 0;
function mouseEventBind(selecter, controler) {
    $(selecter).unbind();
    $(selecter).mousewheel(function (event, delta, x, y) {
        var date = new Date();
        var timestamp = date.getTime();
        if ((timestamp - timer) < 1000) {
            return;
        } else {
            timer = timestamp;
        }
        if (delta < 0) {
            delta = 0;
        }
        if (controler == 'topRight') {
            if (delta) {
                $(this).fullpage.moveSectionUp();
            } else {
                $(this).fullpage.moveSlideRight();
            }
        } else if (controler == 'leftRight') {
            if (delta) {
                $(this).fullpage.moveSlideLeft();
            } else {
                $(this).fullpage.moveSlideRight();
            }
        } else if (controler == 'leftBottom') {
            if (delta) {
                $(this).fullpage.moveSlideLeft();
            } else {
                $(this).fullpage.moveSectionDown();
            }
        }
    });
}