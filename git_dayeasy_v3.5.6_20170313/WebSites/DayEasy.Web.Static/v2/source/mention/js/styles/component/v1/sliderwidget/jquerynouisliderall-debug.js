/*! noUiSlider - 7.0.10 - 2014-12-27 14:50:47 */
define("js/styles/component/v1/sliderwidget/jquerynouisliderall-debug", [ "./jquery.nouislider-debug.css", "./jquery.nouislider.pips-debug.css" ], function(require, exports, module) {
    require("./jquery.nouislider-debug.css");
    require("./jquery.nouislider.pips-debug.css");
    return function($) {
        // 放置插件的源代码
        !function() {
            "use strict";
            function a(a) {
                return a.split("").reverse().join("");
            }
            function b(a, b) {
                return a.substring(0, b.length) === b;
            }
            function c(a, b) {
                return a.slice(-1 * b.length) === b;
            }
            function d(a, b, c) {
                if ((a[b] || a[c]) && a[b] === a[c]) throw new Error(b);
            }
            function e(a) {
                return "number" == typeof a && isFinite(a);
            }
            function f(a, b) {
                var c = Math.pow(10, b);
                return (Math.round(a * c) / c).toFixed(b);
            }
            function g(b, c, d, g, h, i, j, k, l, m, n, o) {
                var p, q, r, s = o, t = "", u = "";
                return i && (o = i(o)), e(o) ? (b !== !1 && 0 === parseFloat(o.toFixed(b)) && (o = 0), 
                0 > o && (p = !0, o = Math.abs(o)), b !== !1 && (o = f(o, b)), o = o.toString(), 
                -1 !== o.indexOf(".") ? (q = o.split("."), r = q[0], d && (t = d + q[1])) : r = o, 
                c && (r = a(r).match(/.{1,3}/g), r = a(r.join(a(c)))), p && k && (u += k), g && (u += g), 
                p && l && (u += l), u += r, u += t, h && (u += h), m && (u = m(u, s)), u) : !1;
            }
            function h(a, d, f, g, h, i, j, k, l, m, n, o) {
                var p, q = "";
                return n && (o = n(o)), o && "string" == typeof o ? (k && b(o, k) && (o = o.replace(k, ""), 
                p = !0), g && b(o, g) && (o = o.replace(g, "")), l && b(o, l) && (o = o.replace(l, ""), 
                p = !0), h && c(o, h) && (o = o.slice(0, -1 * h.length)), d && (o = o.split(d).join("")), 
                f && (o = o.replace(f, ".")), p && (q += "-"), q += o, q = q.replace(/[^0-9\.\-.]/g, ""), 
                "" === q ? !1 : (q = Number(q), j && (q = j(q)), e(q) ? q : !1)) : !1;
            }
            function i(a) {
                var b, c, e, f = {};
                for (b = 0; b < l.length; b += 1) if (c = l[b], e = a[c], void 0 === e) f[c] = "negative" !== c || f.negativeBefore ? "mark" === c && "." !== f.thousand ? "." : !1 : "-"; else if ("decimals" === c) {
                    if (!(e >= 0 && 8 > e)) throw new Error(c);
                    f[c] = e;
                } else if ("encoder" === c || "decoder" === c || "edit" === c || "undo" === c) {
                    if ("function" != typeof e) throw new Error(c);
                    f[c] = e;
                } else {
                    if ("string" != typeof e) throw new Error(c);
                    f[c] = e;
                }
                return d(f, "mark", "thousand"), d(f, "prefix", "negative"), d(f, "prefix", "negativeBefore"), 
                f;
            }
            function j(a, b, c) {
                var d, e = [];
                for (d = 0; d < l.length; d += 1) e.push(a[l[d]]);
                return e.push(c), b.apply("", e);
            }
            function k(a) {
                return this instanceof k ? void ("object" == typeof a && (a = i(a), this.to = function(b) {
                    return j(a, g, b);
                }, this.from = function(b) {
                    return j(a, h, b);
                })) : new k(a);
            }
            var l = [ "decimals", "thousand", "mark", "prefix", "postfix", "encoder", "decoder", "negativeBefore", "negative", "edit", "undo" ];
            window.wNumb = k;
        }(), function(a) {
            "use strict";
            function b(b) {
                return b instanceof a || a.zepto && a.zepto.isZ(b);
            }
            function c(b, c) {
                return "string" == typeof b && 0 === b.indexOf("-inline-") ? (this.method = c || "html", 
                this.target = this.el = a(b.replace("-inline-", "") || "<div/>"), !0) : void 0;
            }
            function d(b) {
                if ("string" == typeof b && 0 !== b.indexOf("-")) {
                    this.method = "val";
                    var c = document.createElement("input");
                    return c.name = b, c.type = "hidden", this.target = this.el = a(c), !0;
                }
            }
            function e(a) {
                return "function" == typeof a ? (this.target = !1, this.method = a, !0) : void 0;
            }
            function f(a, c) {
                return b(a) && !c ? (a.is("input, select, textarea") ? (this.method = "val", this.target = a.on("change.liblink", this.changeHandler)) : (this.target = a, 
                this.method = "html"), !0) : void 0;
            }
            function g(a, c) {
                return b(a) && ("function" == typeof c || "string" == typeof c && a[c]) ? (this.method = c, 
                this.target = a, !0) : void 0;
            }
            function h(b, c, d) {
                var e = this, f = !1;
                if (this.changeHandler = function(b) {
                    var c = e.formatInstance.from(a(this).val());
                    return c === !1 || isNaN(c) ? (a(this).val(e.lastSetValue), !1) : void e.changeHandlerMethod.call("", b, c);
                }, this.el = !1, this.formatInstance = d, a.each(k, function(a, d) {
                    return f = d.call(e, b, c), !f;
                }), !f) throw new RangeError("(Link) Invalid Link.");
            }
            function i(a) {
                this.items = [], this.elements = [], this.origin = a;
            }
            function j(b, c, d, e) {
                0 === b && (b = this.LinkDefaultFlag), this.linkAPI || (this.linkAPI = {}), this.linkAPI[b] || (this.linkAPI[b] = new i(this));
                var f = new h(c, d, e || this.LinkDefaultFormatter);
                f.target || (f.target = a(this)), f.changeHandlerMethod = this.LinkConfirm(b, f.el), 
                this.linkAPI[b].push(f, f.el), this.LinkUpdate(b);
            }
            var k = [ c, d, e, f, g ];
            h.prototype.set = function(a) {
                var b = Array.prototype.slice.call(arguments), c = b.slice(1);
                this.lastSetValue = this.formatInstance.to(a), c.unshift(this.lastSetValue), ("function" == typeof this.method ? this.method : this.target[this.method]).apply(this.target, c);
            }, i.prototype.push = function(a, b) {
                this.items.push(a), b && this.elements.push(b);
            }, i.prototype.reconfirm = function(a) {
                var b;
                for (b = 0; b < this.elements.length; b += 1) this.origin.LinkConfirm(a, this.elements[b]);
            }, i.prototype.remove = function() {
                var a;
                for (a = 0; a < this.items.length; a += 1) this.items[a].target.off(".liblink");
                for (a = 0; a < this.elements.length; a += 1) this.elements[a].remove();
            }, i.prototype.change = function(a) {
                if (this.origin.LinkIsEmitting) return !1;
                this.origin.LinkIsEmitting = !0;
                var b, c = Array.prototype.slice.call(arguments, 1);
                for (c.unshift(a), b = 0; b < this.items.length; b += 1) this.items[b].set.apply(this.items[b], c);
                this.origin.LinkIsEmitting = !1;
            }, a.fn.Link = function(b) {
                var c = this;
                if (b === !1) return c.each(function() {
                    this.linkAPI && (a.map(this.linkAPI, function(a) {
                        a.remove();
                    }), delete this.linkAPI);
                });
                if (void 0 === b) b = 0; else if ("string" != typeof b) throw new Error("Flag must be string.");
                return {
                    to: function(a, d, e) {
                        return c.each(function() {
                            j.call(this, b, a, d, e);
                        });
                    }
                };
            };
        }(window.jQuery || window.Zepto), function(a) {
            "use strict";
            function b(b) {
                return a.grep(b, function(c, d) {
                    return d === a.inArray(c, b);
                });
            }
            function c(a, b) {
                return Math.round(a / b) * b;
            }
            function d(a) {
                return "number" == typeof a && !isNaN(a) && isFinite(a);
            }
            function e(a) {
                var b = Math.pow(10, 7);
                return Number((Math.round(a * b) / b).toFixed(7));
            }
            function f(a, b, c) {
                a.addClass(b), setTimeout(function() {
                    a.removeClass(b);
                }, c);
            }
            function g(a) {
                return Math.max(Math.min(a, 100), 0);
            }
            function h(b) {
                return a.isArray(b) ? b : [ b ];
            }
            function i(a) {
                var b = a.split(".");
                return b.length > 1 ? b[1].length : 0;
            }
            function j(a, b) {
                return 100 / (b - a);
            }
            function k(a, b) {
                return 100 * b / (a[1] - a[0]);
            }
            function l(a, b) {
                return k(a, a[0] < 0 ? b + Math.abs(a[0]) : b - a[0]);
            }
            function m(a, b) {
                return b * (a[1] - a[0]) / 100 + a[0];
            }
            function n(a, b) {
                for (var c = 1; a >= b[c]; ) c += 1;
                return c;
            }
            function o(a, b, c) {
                if (c >= a.slice(-1)[0]) return 100;
                var d, e, f, g, h = n(c, a);
                return d = a[h - 1], e = a[h], f = b[h - 1], g = b[h], f + l([ d, e ], c) / j(f, g);
            }
            function p(a, b, c) {
                if (c >= 100) return a.slice(-1)[0];
                var d, e, f, g, h = n(c, b);
                return d = a[h - 1], e = a[h], f = b[h - 1], g = b[h], m([ d, e ], (c - f) * j(f, g));
            }
            function q(a, b, d, e) {
                if (100 === e) return e;
                var f, g, h = n(e, a);
                return d ? (f = a[h - 1], g = a[h], e - f > (g - f) / 2 ? g : f) : b[h - 1] ? a[h - 1] + c(e - a[h - 1], b[h - 1]) : e;
            }
            function r(a, b, c) {
                var e;
                if ("number" == typeof b && (b = [ b ]), "[object Array]" !== Object.prototype.toString.call(b)) throw new Error("noUiSlider: 'range' contains invalid value.");
                if (e = "min" === a ? 0 : "max" === a ? 100 : parseFloat(a), !d(e) || !d(b[0])) throw new Error("noUiSlider: 'range' value isn't numeric.");
                c.xPct.push(e), c.xVal.push(b[0]), e ? c.xSteps.push(isNaN(b[1]) ? !1 : b[1]) : isNaN(b[1]) || (c.xSteps[0] = b[1]);
            }
            function s(a, b, c) {
                return b ? void (c.xSteps[a] = k([ c.xVal[a], c.xVal[a + 1] ], b) / j(c.xPct[a], c.xPct[a + 1])) : !0;
            }
            function t(a, b, c, d) {
                this.xPct = [], this.xVal = [], this.xSteps = [ d || !1 ], this.xNumSteps = [ !1 ], 
                this.snap = b, this.direction = c;
                var e, f = [];
                for (e in a) a.hasOwnProperty(e) && f.push([ a[e], e ]);
                for (f.sort(function(a, b) {
                    return a[0] - b[0];
                }), e = 0; e < f.length; e++) r(f[e][1], f[e][0], this);
                for (this.xNumSteps = this.xSteps.slice(0), e = 0; e < this.xNumSteps.length; e++) s(e, this.xNumSteps[e], this);
            }
            function u(a, b) {
                if (!d(b)) throw new Error("noUiSlider: 'step' is not numeric.");
                a.singleStep = b;
            }
            function v(b, c) {
                if ("object" != typeof c || a.isArray(c)) throw new Error("noUiSlider: 'range' is not an object.");
                if (void 0 === c.min || void 0 === c.max) throw new Error("noUiSlider: Missing 'min' or 'max' in 'range'.");
                b.spectrum = new t(c, b.snap, b.dir, b.singleStep);
            }
            function w(b, c) {
                if (c = h(c), !a.isArray(c) || !c.length || c.length > 2) throw new Error("noUiSlider: 'start' option is incorrect.");
                b.handles = c.length, b.start = c;
            }
            function x(a, b) {
                if (a.snap = b, "boolean" != typeof b) throw new Error("noUiSlider: 'snap' option must be a boolean.");
            }
            function y(a, b) {
                if (a.animate = b, "boolean" != typeof b) throw new Error("noUiSlider: 'animate' option must be a boolean.");
            }
            function z(a, b) {
                if ("lower" === b && 1 === a.handles) a.connect = 1; else if ("upper" === b && 1 === a.handles) a.connect = 2; else if (b === !0 && 2 === a.handles) a.connect = 3; else {
                    if (b !== !1) throw new Error("noUiSlider: 'connect' option doesn't match handle count.");
                    a.connect = 0;
                }
            }
            function A(a, b) {
                switch (b) {
                  case "horizontal":
                    a.ort = 0;
                    break;

                  case "vertical":
                    a.ort = 1;
                    break;

                  default:
                    throw new Error("noUiSlider: 'orientation' option is invalid.");
                }
            }
            function B(a, b) {
                if (!d(b)) throw new Error("noUiSlider: 'margin' option must be numeric.");
                if (a.margin = a.spectrum.getMargin(b), !a.margin) throw new Error("noUiSlider: 'margin' option is only supported on linear sliders.");
            }
            function C(a, b) {
                if (!d(b)) throw new Error("noUiSlider: 'limit' option must be numeric.");
                if (a.limit = a.spectrum.getMargin(b), !a.limit) throw new Error("noUiSlider: 'limit' option is only supported on linear sliders.");
            }
            function D(a, b) {
                switch (b) {
                  case "ltr":
                    a.dir = 0;
                    break;

                  case "rtl":
                    a.dir = 1, a.connect = [ 0, 2, 1, 3 ][a.connect];
                    break;

                  default:
                    throw new Error("noUiSlider: 'direction' option was not recognized.");
                }
            }
            function E(a, b) {
                if ("string" != typeof b) throw new Error("noUiSlider: 'behaviour' must be a string containing options.");
                var c = b.indexOf("tap") >= 0, d = b.indexOf("drag") >= 0, e = b.indexOf("fixed") >= 0, f = b.indexOf("snap") >= 0;
                a.events = {
                    tap: c || f,
                    drag: d,
                    fixed: e,
                    snap: f
                };
            }
            function F(a, b) {
                if (a.format = b, "function" == typeof b.to && "function" == typeof b.from) return !0;
                throw new Error("noUiSlider: 'format' requires 'to' and 'from' methods.");
            }
            function G(b) {
                var c, d = {
                    margin: 0,
                    limit: 0,
                    animate: !0,
                    format: Z
                };
                return c = {
                    step: {
                        r: !1,
                        t: u
                    },
                    start: {
                        r: !0,
                        t: w
                    },
                    connect: {
                        r: !0,
                        t: z
                    },
                    direction: {
                        r: !0,
                        t: D
                    },
                    snap: {
                        r: !1,
                        t: x
                    },
                    animate: {
                        r: !1,
                        t: y
                    },
                    range: {
                        r: !0,
                        t: v
                    },
                    orientation: {
                        r: !1,
                        t: A
                    },
                    margin: {
                        r: !1,
                        t: B
                    },
                    limit: {
                        r: !1,
                        t: C
                    },
                    behaviour: {
                        r: !0,
                        t: E
                    },
                    format: {
                        r: !1,
                        t: F
                    }
                }, b = a.extend({
                    connect: !1,
                    direction: "ltr",
                    behaviour: "tap",
                    orientation: "horizontal"
                }, b), a.each(c, function(a, c) {
                    if (void 0 === b[a]) {
                        if (c.r) throw new Error("noUiSlider: '" + a + "' is required.");
                        return !0;
                    }
                    c.t(d, b[a]);
                }), d.style = d.ort ? "top" : "left", d;
            }
            function H(a, b, c) {
                var d = a + b[0], e = a + b[1];
                return c ? (0 > d && (e += Math.abs(d)), e > 100 && (d -= e - 100), [ g(d), g(e) ]) : [ d, e ];
            }
            function I(a) {
                a.preventDefault();
                var b, c, d = 0 === a.type.indexOf("touch"), e = 0 === a.type.indexOf("mouse"), f = 0 === a.type.indexOf("pointer"), g = a;
                return 0 === a.type.indexOf("MSPointer") && (f = !0), a.originalEvent && (a = a.originalEvent), 
                d && (b = a.changedTouches[0].pageX, c = a.changedTouches[0].pageY), (e || f) && (f || void 0 !== window.pageXOffset || (window.pageXOffset = document.documentElement.scrollLeft, 
                window.pageYOffset = document.documentElement.scrollTop), b = a.clientX + window.pageXOffset, 
                c = a.clientY + window.pageYOffset), g.points = [ b, c ], g.cursor = e, g;
            }
            function J(b, c) {
                var d = a("<div><div/></div>").addClass(Y[2]), e = [ "-lower", "-upper" ];
                return b && e.reverse(), d.children().addClass(Y[3] + " " + Y[3] + e[c]), d;
            }
            function K(a, b, c) {
                switch (a) {
                  case 1:
                    b.addClass(Y[7]), c[0].addClass(Y[6]);
                    break;

                  case 3:
                    c[1].addClass(Y[6]);

                  case 2:
                    c[0].addClass(Y[7]);

                  case 0:
                    b.addClass(Y[6]);
                }
            }
            function L(a, b, c) {
                var d, e = [];
                for (d = 0; a > d; d += 1) e.push(J(b, d).appendTo(c));
                return e;
            }
            function M(b, c, d) {
                return d.addClass([ Y[0], Y[8 + b], Y[4 + c] ].join(" ")), a("<div/>").appendTo(d).addClass(Y[1]);
            }
            function N(b, c, d) {
                function e() {
                    return C[[ "width", "height" ][c.ort]]();
                }
                function j(a) {
                    var b, c = [ E.val() ];
                    for (b = 0; b < a.length; b += 1) E.trigger(a[b], c);
                }
                function k(a) {
                    return 1 === a.length ? a[0] : c.dir ? a.reverse() : a;
                }
                function l(a) {
                    return function(b, c) {
                        E.val([ a ? null : c, a ? c : null ], !0);
                    };
                }
                function m(b) {
                    var c = a.inArray(b, N);
                    E[0].linkAPI && E[0].linkAPI[b] && E[0].linkAPI[b].change(J[c], D[c].children(), E);
                }
                function n(b, d) {
                    var e = a.inArray(b, N);
                    return d && d.appendTo(D[e].children()), c.dir && c.handles > 1 && (e = 1 === e ? 0 : 1), 
                    l(e);
                }
                function o() {
                    var a, b;
                    for (a = 0; a < N.length; a += 1) this.linkAPI && this.linkAPI[b = N[a]] && this.linkAPI[b].reconfirm(b);
                }
                function p(a, b, d, e) {
                    return a = a.replace(/\s/g, W + " ") + W, b.on(a, function(a) {
                        return E.attr("disabled") ? !1 : E.hasClass(Y[14]) ? !1 : (a = I(a), a.calcPoint = a.points[c.ort], 
                        void d(a, e));
                    });
                }
                function q(a, b) {
                    var c, d = b.handles || D, f = !1, g = 100 * (a.calcPoint - b.start) / e(), h = d[0][0] !== D[0][0] ? 1 : 0;
                    c = H(g, b.positions, d.length > 1), f = v(d[0], c[h], 1 === d.length), d.length > 1 && (f = v(d[1], c[h ? 0 : 1], !1) || f), 
                    f && j([ "slide" ]);
                }
                function r(b) {
                    a("." + Y[15]).removeClass(Y[15]), b.cursor && a("body").css("cursor", "").off(W), 
                    U.off(W), E.removeClass(Y[12]), j([ "set", "change" ]);
                }
                function s(b, c) {
                    1 === c.handles.length && c.handles[0].children().addClass(Y[15]), b.stopPropagation(), 
                    p(X.move, U, q, {
                        start: b.calcPoint,
                        handles: c.handles,
                        positions: [ F[0], F[D.length - 1] ]
                    }), p(X.end, U, r, null), b.cursor && (a("body").css("cursor", a(b.target).css("cursor")), 
                    D.length > 1 && E.addClass(Y[12]), a("body").on("selectstart" + W, !1));
                }
                function t(b) {
                    var d, g = b.calcPoint, h = 0;
                    b.stopPropagation(), a.each(D, function() {
                        h += this.offset()[c.style];
                    }), h = h / 2 > g || 1 === D.length ? 0 : 1, g -= C.offset()[c.style], d = 100 * g / e(), 
                    c.events.snap || f(E, Y[14], 300), v(D[h], d), j([ "slide", "set", "change" ]), 
                    c.events.snap && s(b, {
                        handles: [ D[h] ]
                    });
                }
                function u(a) {
                    var b, c;
                    if (!a.fixed) for (b = 0; b < D.length; b += 1) p(X.start, D[b].children(), s, {
                        handles: [ D[b] ]
                    });
                    a.tap && p(X.start, C, t, {
                        handles: D
                    }), a.drag && (c = C.find("." + Y[7]).addClass(Y[10]), a.fixed && (c = c.add(C.children().not(c).children())), 
                    p(X.start, c, s, {
                        handles: D
                    }));
                }
                function v(a, b, d) {
                    var e = a[0] !== D[0][0] ? 1 : 0, f = F[0] + c.margin, h = F[1] - c.margin, i = F[0] + c.limit, j = F[1] - c.limit;
                    return D.length > 1 && (b = e ? Math.max(b, f) : Math.min(b, h)), d !== !1 && c.limit && D.length > 1 && (b = e ? Math.min(b, i) : Math.max(b, j)), 
                    b = G.getStep(b), b = g(parseFloat(b.toFixed(7))), b === F[e] ? !1 : (a.css(c.style, b + "%"), 
                    a.is(":first-child") && a.toggleClass(Y[17], b > 50), F[e] = b, J[e] = G.fromStepping(b), 
                    m(N[e]), !0);
                }
                function w(a, b) {
                    var d, e, f;
                    for (c.limit && (a += 1), d = 0; a > d; d += 1) e = d % 2, f = b[e], null !== f && f !== !1 && ("number" == typeof f && (f = String(f)), 
                    f = c.format.from(f), (f === !1 || isNaN(f) || v(D[e], G.toStepping(f), d === 3 - c.dir) === !1) && m(N[e]));
                }
                function x(a) {
                    if (E[0].LinkIsEmitting) return this;
                    var b, d = h(a);
                    return c.dir && c.handles > 1 && d.reverse(), c.animate && -1 !== F[0] && f(E, Y[14], 300), 
                    b = D.length > 1 ? 3 : 1, 1 === d.length && (b = 1), w(b, d), j([ "set" ]), this;
                }
                function y() {
                    var a, b = [];
                    for (a = 0; a < c.handles; a += 1) b[a] = c.format.to(J[a]);
                    return k(b);
                }
                function z() {
                    return a(this).off(W).removeClass(Y.join(" ")).empty(), delete this.LinkUpdate, 
                    delete this.LinkConfirm, delete this.LinkDefaultFormatter, delete this.LinkDefaultFlag, 
                    delete this.reappend, delete this.vGet, delete this.vSet, delete this.getCurrentStep, 
                    delete this.getInfo, delete this.destroy, d;
                }
                function A() {
                    var b = a.map(F, function(a, b) {
                        var c = G.getApplicableStep(a), d = i(String(c[2])), e = J[b], f = 100 === a ? null : c[2], g = Number((e - c[2]).toFixed(d)), h = 0 === a ? null : g >= c[1] ? c[2] : c[0] || !1;
                        return [ [ h, f ] ];
                    });
                    return k(b);
                }
                function B() {
                    return d;
                }
                var C, D, E = a(b), F = [ -1, -1 ], G = c.spectrum, J = [], N = [ "lower", "upper" ].slice(0, c.handles);
                if (c.dir && N.reverse(), b.LinkUpdate = m, b.LinkConfirm = n, b.LinkDefaultFormatter = c.format, 
                b.LinkDefaultFlag = "lower", b.reappend = o, E.hasClass(Y[0])) throw new Error("Slider was already initialized.");
                C = M(c.dir, c.ort, E), D = L(c.handles, c.dir, C), K(c.connect, E, D), u(c.events), 
                b.vSet = x, b.vGet = y, b.destroy = z, b.getCurrentStep = A, b.getOriginalOptions = B, 
                b.getInfo = function() {
                    return [ G, c.style, c.ort ];
                }, E.val(c.start);
            }
            function O(a) {
                var b = G(a, this);
                return this.each(function() {
                    N(this, b, a);
                });
            }
            function P(b) {
                return this.each(function() {
                    if (!this.destroy) return void a(this).noUiSlider(b);
                    var c = a(this).val(), d = this.destroy(), e = a.extend({}, d, b);
                    a(this).noUiSlider(e), this.reappend(), d.start === e.start && a(this).val(c);
                });
            }
            function Q() {
                return this[0][arguments.length ? "vSet" : "vGet"].apply(this[0], arguments);
            }
            function R(b, c, d, e) {
                if ("range" === c || "steps" === c) return b.xVal;
                if ("count" === c) {
                    var f, g = 100 / (d - 1), h = 0;
                    for (d = []; (f = h++ * g) <= 100; ) d.push(f);
                    c = "positions";
                }
                return "positions" === c ? a.map(d, function(a) {
                    return b.fromStepping(e ? b.getStep(a) : a);
                }) : "values" === c ? e ? a.map(d, function(a) {
                    return b.fromStepping(b.getStep(b.toStepping(a)));
                }) : d : void 0;
            }
            function S(c, d, e, f) {
                var g = c.direction, h = {}, i = c.xVal[0], j = c.xVal[c.xVal.length - 1], k = !1, l = !1, m = 0;
                return c.direction = 0, f = b(f.slice().sort(function(a, b) {
                    return a - b;
                })), f[0] !== i && (f.unshift(i), k = !0), f[f.length - 1] !== j && (f.push(j), 
                l = !0), a.each(f, function(b) {
                    var g, i, j, n, o, p, q, r, s, t, u = f[b], v = f[b + 1];
                    if ("steps" === e && (g = c.xNumSteps[b]), g || (g = v - u), u !== !1 && void 0 !== v) for (i = u; v >= i; i += g) {
                        for (n = c.toStepping(i), o = n - m, r = o / d, s = Math.round(r), t = o / s, j = 1; s >= j; j += 1) p = m + j * t, 
                        h[p.toFixed(5)] = [ "x", 0 ];
                        q = a.inArray(i, f) > -1 ? 1 : "steps" === e ? 2 : 0, !b && k && (q = 0), i === v && l || (h[n.toFixed(5)] = [ i, q ]), 
                        m = n;
                    }
                }), c.direction = g, h;
            }
            function T(b, c, d, e, f, g) {
                function h(a) {
                    return [ "-normal", "-large", "-sub" ][a];
                }
                function i(a, c, d) {
                    return 'class="' + c + " " + c + "-" + k + " " + c + h(d[1], d[0]) + '" style="' + b + ": " + a + '%"';
                }
                function j(a, b) {
                    d && (a = 100 - a), b[1] = b[1] && f ? f(b[0], b[1]) : b[1], l.append("<div " + i(a, "noUi-marker", b) + "></div>"), 
                    b[1] && l.append("<div " + i(a, "noUi-value", b) + ">" + g.to(b[0]) + "</div>");
                }
                var k = [ "horizontal", "vertical" ][c], l = a("<div/>");
                return l.addClass("noUi-pips noUi-pips-" + k), a.each(e, j), l;
            }
            var U = a(document), V = a.fn.val, W = ".nui", X = window.navigator.pointerEnabled ? {
                start: "pointerdown",
                move: "pointermove",
                end: "pointerup"
            } : window.navigator.msPointerEnabled ? {
                start: "MSPointerDown",
                move: "MSPointerMove",
                end: "MSPointerUp"
            } : {
                start: "mousedown touchstart",
                move: "mousemove touchmove",
                end: "mouseup touchend"
            }, Y = [ "noUi-target", "noUi-base", "noUi-origin", "noUi-handle", "noUi-horizontal", "noUi-vertical", "noUi-background", "noUi-connect", "noUi-ltr", "noUi-rtl", "noUi-dragable", "", "noUi-state-drag", "", "noUi-state-tap", "noUi-active", "", "noUi-stacking" ];
            t.prototype.getMargin = function(a) {
                return 2 === this.xPct.length ? k(this.xVal, a) : !1;
            }, t.prototype.toStepping = function(a) {
                return a = o(this.xVal, this.xPct, a), this.direction && (a = 100 - a), a;
            }, t.prototype.fromStepping = function(a) {
                return this.direction && (a = 100 - a), e(p(this.xVal, this.xPct, a));
            }, t.prototype.getStep = function(a) {
                return this.direction && (a = 100 - a), a = q(this.xPct, this.xSteps, this.snap, a), 
                this.direction && (a = 100 - a), a;
            }, t.prototype.getApplicableStep = function(a) {
                var b = n(a, this.xPct), c = 100 === a ? 2 : 1;
                return [ this.xNumSteps[b - 2], this.xVal[b - c], this.xNumSteps[b - c] ];
            }, t.prototype.convert = function(a) {
                return this.getStep(this.toStepping(a));
            };
            var Z = {
                to: function(a) {
                    return a.toFixed(2);
                },
                from: Number
            };
            a.fn.val = function(b) {
                function c(a) {
                    return a.hasClass(Y[0]) ? Q : V;
                }
                if (!arguments.length) {
                    var d = a(this[0]);
                    return c(d).call(d);
                }
                var e = a.isFunction(b);
                return this.each(function(d) {
                    var f = b, g = a(this);
                    e && (f = b.call(this, d, g.val())), c(g).call(g, f);
                });
            }, a.fn.noUiSlider = function(a, b) {
                switch (a) {
                  case "step":
                    return this[0].getCurrentStep();

                  case "options":
                    return this[0].getOriginalOptions();
                }
                return (b ? P : O).call(this, a);
            }, a.fn.noUiSlider_pips = function(b) {
                var c = b.mode, d = b.density || 1, e = b.filter || !1, f = b.values || !1, g = b.format || {
                    to: Math.round
                }, h = b.stepped || !1;
                return this.each(function() {
                    var b = this.getInfo(), i = R(b[0], c, f, h), j = S(b[0], d, c, i);
                    return a(this).append(T(b[1], b[2], b[0].direction, j, e, g));
                });
            };
        }(window.jQuery || window.Zepto);
    };
});

define("js/styles/component/v1/sliderwidget/jquery.nouislider-debug.css", [], function() {
    seajs.importStyle('.noUi-target,.noUi-target *{-webkit-touch-callout:none;-webkit-user-select:none;-ms-touch-action:none;-ms-user-select:none;-moz-user-select:none;-moz-box-sizing:border-box;box-sizing:border-box}.noUi-target{position:relative;direction:ltr}.noUi-base{width:100%;height:100%;position:relative}.noUi-origin{position:absolute;right:0;top:0;left:0;bottom:0}.noUi-handle{position:relative;z-index:1}.noUi-stacking .noUi-handle{z-index:10}.noUi-state-tap .noUi-origin{-webkit-transition:left .3s,top .3s;transition:left .3s,top .3s}.noUi-state-drag *{cursor:inherit!important}.noUi-base{-webkit-transform:translate3d(0,0,0);transform:translate3d(0,0,0)}.noUi-horizontal{height:18px}.noUi-horizontal .noUi-handle{width:34px;height:28px;left:-17px;top:-6px}.noUi-vertical{width:18px}.noUi-vertical .noUi-handle{width:28px;height:34px;left:-6px;top:-17px}.noUi-background{background:#FAFAFA;box-shadow:inset 0 1px 1px #f0f0f0}.noUi-connect{background:#3FB8AF;box-shadow:inset 0 0 3px rgba(51,51,51,.45);-webkit-transition:background 450ms;transition:background 450ms}.noUi-origin{border-radius:2px}.noUi-target{border-radius:4px;border:1px solid #D3D3D3;box-shadow:inset 0 1px 1px #F0F0F0,0 3px 6px -5px #BBB}.noUi-target.noUi-connect{box-shadow:inset 0 0 3px rgba(51,51,51,.45),0 3px 6px -5px #BBB}.noUi-dragable{cursor:w-resize}.noUi-vertical .noUi-dragable{cursor:n-resize}.noUi-handle{border:1px solid #D9D9D9;border-radius:3px;background:#FFF;cursor:default;box-shadow:inset 0 0 1px #FFF,inset 0 1px 7px #EBEBEB,0 3px 6px -3px #BBB}.noUi-active{box-shadow:inset 0 0 1px #FFF,inset 0 1px 7px #DDD,0 3px 6px -3px #BBB}.noUi-handle:after,.noUi-handle:before{content:"";display:block;position:absolute;height:14px;width:1px;background:#E8E7E6;left:14px;top:6px}.noUi-handle:after{left:17px}.noUi-vertical .noUi-handle:after,.noUi-vertical .noUi-handle:before{width:14px;height:1px;left:6px;top:14px}.noUi-vertical .noUi-handle:after{top:17px}[disabled] .noUi-connect,[disabled].noUi-connect{background:#B8B8B8}[disabled] .noUi-handle{cursor:not-allowed}');
});

define("js/styles/component/v1/sliderwidget/jquery.nouislider.pips-debug.css", [], function() {
    seajs.importStyle(".noUi-pips,.noUi-pips *{-moz-box-sizing:border-box;box-sizing:border-box}.noUi-pips{position:absolute;font:400 12px Arial;color:#999}.noUi-value{width:40px;position:absolute;text-align:center}.noUi-value-sub{color:#ccc;font-size:10px}.noUi-marker{position:absolute;background:#CCC}.noUi-marker-large,.noUi-marker-sub{background:#AAA}.noUi-pips-horizontal{padding:10px 0;height:50px;top:100%;left:0;width:100%}.noUi-value-horizontal{margin-left:-20px;padding-top:20px}.noUi-value-horizontal.noUi-value-sub{padding-top:15px}.noUi-marker-horizontal.noUi-marker{margin-left:-1px;width:2px;height:5px}.noUi-marker-horizontal.noUi-marker-sub{height:10px}.noUi-marker-horizontal.noUi-marker-large{height:15px}.noUi-pips-vertical{padding:0 10px;height:100%;top:0;left:100%}.noUi-value-vertical{width:15px;margin-left:20px;margin-top:-5px}.noUi-marker-vertical.noUi-marker{width:5px;height:2px;margin-top:-1px}.noUi-marker-vertical.noUi-marker-sub{width:10px}.noUi-marker-vertical.noUi-marker-large{width:15px}");
});
