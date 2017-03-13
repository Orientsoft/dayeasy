seajs.config({
	alias: {
		'text2': 'base/text2',
	},

});

define(function(require, exports, module) {


	exports.show = function show() {
		alert(1);
	};

	var a = require('text2'); //引入a模块

	a.aaa();
});