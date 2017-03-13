;(function() {

	seajs.config({
		alias: {
			'base': 'base/base',
			'text': 'base/text'
		},

	});

	seajs.use(['base', 'text'], function(base, text) {
		// base 
		(function() {
			base.wantTo($(".m-nav"), 360, 150, "z-fixed");
			$(window).scroll(function() {
				$("#searchPoints").click();
				$("#tree_searchTree").hide();
			});
		})();
		text 
		text.show();


	});

})();