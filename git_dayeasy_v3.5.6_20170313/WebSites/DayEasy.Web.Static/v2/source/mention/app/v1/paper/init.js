define(function(require, exports) {
	var $ = require("jquery");

	var slideudown = require("slideudown");

	slideudown.wantTo($(".m-nav"), 360, 150, "z-fixed");
	$(window).scroll(function() {
		$("#searchPoints").click();
		$("#tree_searchTree").hide();
	});

// function test(){
	
// }

});