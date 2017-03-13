
define(function(require,exports,module){  
	 var $ = require("jquery");
	function wantTo(obj, otop, Speed, oclass) {
		$(window).scroll(function() {
			if ($(window).scrollTop() > otop) {
				obj.slideDown(Speed).addClass(oclass);
			} else {
				obj.removeClass(oclass);
			}
		});
	};

	exports.wantTo = wantTo;

});

