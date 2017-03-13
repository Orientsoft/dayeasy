;(function() {
	var configure = false; // true 线上  false  开发版本 
	if (configure) {
		var version = '.min';//可配置版本号后缀
		seajs.config({
			base: '/js/sea/',
			map: [
				['.css', version + '.css'],
				['.js', version + '.js']
			]
		});
	} else {
		seajs.config({
			base: '/source/js/sea/',
			debug: 2,
			charset: 'utf-8',
			timeout: 20000,
		});
	};

})();

