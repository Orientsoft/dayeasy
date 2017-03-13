/**
 * 公式解析器
 * Created by shoy on 2014/10/1.
 */
(function (S){
    var isLoad = S.deyi.loadFormula || false,
        formulaCallBack = function (obj, callback){
            if(callback){
                MathJax && MathJax.Hub.Typeset(obj[0], callback);
            } else {
                MathJax && MathJax.Hub.Typeset();
            }
        };

    S._mix(S, {
        loadFormula: function (obj, callback){
            if(!isLoad) return false;
            obj = obj || $("body");
            var $formula = $("#formula"),
                $config = $("#formula-config");
            if(!$config.length){
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
            if(!$formula.length){
                $formula = $('<script id="formula" type="text/javascript"></script>');
                var formulaSite = 'http://static.dayez.net';
                if(S.sites.static.indexOf('.dayeasy.net') > 0){
                    formulaSite = S.sites.static;
                }
                $formula.attr("src", formulaSite + "/web_formula/MathJax.js?config=TeX-AMS-MML_HTMLorMML");
                $formula.appendTo($("head"));
                $formula.load(function (){
                    formulaCallBack(obj, callback);
                });
            } else {
                formulaCallBack(obj, callback);
            }
        }
    });
})(SINGER);