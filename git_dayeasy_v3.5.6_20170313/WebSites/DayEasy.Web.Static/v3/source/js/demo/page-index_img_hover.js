!function (e, n){
    e.fn.indexSlide = function (n){
        if(!this.length)return this;
        var i = e.extend(!0, {}, e.fn.indexSlide.defaults, n);
        return this.each(function (){
            function n(){
                f = setInterval(function (){
                    1 == i.irectiond && r.trigger("click"), 2 == i.irectiond && a.trigger("click")
                }, i.speed)
            }

            var t = e(this), r = t.children(".prev"), a = t.children(".next"), o = t.find(".content-ul-index"), l = o.find("li"), u = l.length, d = l.outerWidth(!0) * i.num, c = 1;
            if(u < i.num)return !1;
            t.addClass("on"), o.width(l.outerWidth(!0) * u);
            var s = function (e){
                var n = Math.ceil(u / i.num);
                o.is(":animated") || (1 == e ? 1 == c ? (o.animate({"margin-left": "-=" + d * (n - 1)}, "slow"), c = n) : (o.animate({"margin-left": "+=" + d}, "slow"), c--) : 2 == e && (c == n ? (o.animate({"margin-left": "0px"}, "slow"), c = 1) : (o.animate({"margin-left": "-=" + d}, "slow"), c++)))
            };
            if(r.on("click", function (e){
                    e.preventDefault(), s(1)
                }), a.on("click", function (e){
                    e.preventDefault(), s(2)
                }), i.timer){
                var f = null;
                setTimeout(n, i.speed), t[0].onmousemove = function (){
                    clearInterval(f)
                }, t[0].onmouseout = n
            }
        }), this
    }, e.fn.indexSlide.defaults = {num: 3, timer: !1, irectiond: 2, speed: 6e3}
}(jQuery, SINGER), function (e, n){
    /msie [6|7|8|9]/i.test(navigator.userAgent) || (new WOW).init(), e(".m-index-nav li").each(function (){
        e(this).on("mouseover", function (n){
            n.preventDefault();
            var i = e(this).index(), t = "on" + i;
            e(".m-index-data > div").removeClass().addClass(t)
        })
    })
}(jQuery, SINGER), $(".wrap-ul").indexSlide({timer: !0, irectiond: 2});