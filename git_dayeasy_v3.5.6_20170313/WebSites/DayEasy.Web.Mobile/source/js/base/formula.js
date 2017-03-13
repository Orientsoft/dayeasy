/**
 * ¹«Ê½½âÎöÆ÷
 * Created by shay on 2014/10/1.
 */
(function (S) {
    var formulaCallBack = function (obj, callback) {
        if (!MathJax || typeof MathJax === 'undefined')
            return false;
        if (callback) {
            MathJax && MathJax.Hub.Typeset(obj[0], callback);
        } else {
            MathJax && MathJax.Hub.Typeset();
        }
    };

    S._mix(S, {
        loadFormula: function (obj, callback) {
            obj = obj || $("body");
            var $formula = $("#formula"),
                $config = $("#formula-config");
            if (!$config.length) {
                var formulaConfigText = "MathJax.Hub.Config({"
                    + "messageStyle: \"none\","
                    + "menuSettings: { zoom: \"Hover\" },"
                    + "showMathMenu: false,"
                    + "showMathMenuMSIE: false,"
                    + "tex2jax: { inlineMath: [[\"\\\\(\", \"\\\\)\"],[\"\\\\[\",\"\\\\]\"],[\"$\", \"$\"]],"
                    + "displayMath:[[\"$$\",\"$$\"]] }"
                    + "});";
                $config = $('<script id="formula-config" type="text/x-mathjax-config"></script>');
                $config.text(formulaConfigText);
                $config.appendTo(obj);
            }
            if (!$formula.length) {
                $formula = $('<script id="formula" type="text/javascript"></script>');
                var formulaSite = 'http://static.dayeasy.net';
                $formula.attr("src", formulaSite + "/web_formula/MathJax.js?config=TeX-AMS-MML_HTMLorMML");
                $formula.appendTo($("head"));
                $formula.load(function () {
                    formulaCallBack(obj, callback);
                });
            } else {
                formulaCallBack(obj, callback);
            }
        }
    });
})(SINGER);