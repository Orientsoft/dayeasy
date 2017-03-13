jQuery(document).ready(function($) {
	// 调用
	var markObj = $.mark.Slide();

	// 点击展开关闭
	markObj.otoggleClass($('.f-incr'), $('.m-g-1').find('.u-student-lst'), "click");
	// markObj.otoggleClass($('.u-big'), $('.g-binding-stude'), "click");
	//markObj.otoggleClass($('.u-prompt'),$('.u-prompt'));
	markObj.Close($('.f-close'), $('.u-prompt')); // 关闭按钮
	// markObj.Close2($('.Close2'), $('.g-binding-stude')); // 关闭按钮
	// markObj._ulistshow($('.g-binding-stude'));  //学生弹出框



	markObj.Thecurrent(); // 姓名弹出层
	markObj.Linkagedisplay(); // 主观题 点击层效果
	markObj.ghover(); // 打分滑动层 
	markObj.Center($('.g-wrong'));
	markObj.Centerbinding($('.f-pop'));
	// markObj.ofixed($('.g-hd'));     // fixed  浮动

	// 窗口改变 
	$(window).resize(function(e) {
		markObj.Center($('.g-wrong'));
		markObj.Centerbinding($('.g-binding-stude'));
	});

	// 滚动条滑动
	$(window).scroll(function(event) {
		markObj.Centerbinding($('.g-binding-stude'));
	});

	// 弹出框

	function popdy(obj, Taobj, oCloseobj) {


		var oBtn = Taobj; //目标
		var popWindow = obj; //操作
		var oClose = oCloseobj; //关闭
		var browserWidth = $(window).width();
		var browserHeight = $(window).height();
		var browserScrollTop = $(window).scrollTop();
		var browserScrollLeft = $(window).scrollLeft();
		var popWindowWidth = popWindow.outerWidth(true);
		var popWindowHeight = popWindow.outerHeight(true);
		var positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
		var positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;
		var oMask = '<div class="mask"></div>'
		var maskWidth = $(document).width();
		var maskHeight = $(document).height();
		oBtn.click(function() {
			popWindow.show().animate({
				'left': positionLeft + 'px',
				'top': positionTop + 'px'
			}, 0);

			$('body').append(oMask);
			$('.mask').width(maskWidth).height(maskHeight);

		});

		$(window).resize(function() {
			if (popWindow.is(':visible')) {
				browserWidth = $(window).width();
				browserHeight = $(window).height();
				positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
				positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;

				popWindow.animate({
					'left': positionLeft + 'px',
					'top': positionTop + 'px'
				}, 0);
			}
		});

		$(window).scroll(function() {
			if (popWindow.is(':visible')) {
				browserScrollTop = $(window).scrollTop();
				browserScrollLeft = $(window).scrollLeft();
				positionLeft = browserWidth / 2 - popWindowWidth / 2 + browserScrollLeft;
				positionTop = browserHeight / 2 - popWindowHeight / 2 + browserScrollTop;
				popWindow.animate({
					'left': positionLeft + 'px',
					'top': positionTop + 'px'
				}, 0).dequeue();
			}

		});

		oClose.click(function() {
			popWindow.hide();
			$('.mask').remove();

		});
		$('.mask').live('click', function(event) {
			popWindow.hide();
			$(this).remove();
		});
	}

	popdy($('.g-binding-stude'), $('.u-big'), $('.g-binding-stude .Close2'));



	function aaaa(obj) {
		var offset = obj.offset(),
			// objleft  =offset.left;
			objtop = offset.top;
		obj.data('marginLeft', obj.position().left)

		$(window).scroll(function() {
			var scrollTop = $(window).scrollTop();
			if (scrollTop >= objtop) {

				obj.css({
					position: 'fixed',
					marginLeft: '790px',
					top: '0'
				});

			} else {
				obj.css({
					position: 'relative',
					marginLeft: '',
					top: '0'
				});
			};

		});
	}


	aaaa($('.g-sd'));


});

(function($) {
	$.extend({
		mark: {
			Slide: function() {
				return new marking();
			}
		}
	});

	var marking = function() {
		this.att1 = 10;
		this.att2 = true;
	};

	marking.prototype = {
		// 点击展开层  
		otoggleClass: function(obj, targetObj, eventType) {
			obj.bind(eventType, function() {
				targetObj.toggleClass('hide');
			});
		},
		// 点击关闭
		Close: function(obg, Target) {
			obg.click(function(event) {
				// Target.empty();
				Target.slideUp(400);
			});
		},
		Close2: function(obg, Target) {
			obg.click(function(event) {
				// Target.empty();
				Target.toggleClass('hide');
			});
		},
		Center: function(obj) {
			var _t = this,
				ww = $(window).width(),
				popWindow = obj,
				popTarget = $('.m-mn-img');

			obj.css("position", "fixed");



			var X = $('.m-mn-img').offset().top;
			var Y = $('.m-mn-img').offset().left;



			// console.log(X,Y);
			// 目标宽度
			var popWindowWidth = popWindow.outerWidth(true);
			// 操作宽度
			var popTargetwWidth = popTarget.outerWidth(true);
			// 居中left
			var positionLeft = popTargetwWidth / 2 - popWindowWidth / 2;
			var pageY = Y + positionLeft;

			obj.css('left', pageY);
			//_t.objs.push(obj);
			return _t;
		},
		Centerbinding: function(obj) {

			obj.css({
				position: 'absolute',
				zIndex: '1000'
			});
			obj.css("top", ($(window).height() - obj.height()) / 2 + $(window).scrollTop() + "px");
			obj.css("left", ($(window).width() - obj.width()) / 2 + $(window).scrollLeft() + "px");
			return this;


		},
		ofixed: function(obj) {
			obj.addClass('z-fixed');
			obj.css("top", ($(window).height() - obj.height()) / 2 + $(window).scrollTop() + "px");
			// obj.css("left", ( $(window).width() - obj.width() ) / 2+$(window).scrollLeft() + "px"); 
		},
		// 当前状态选中样式
		Thecurrent: function() {
			this._ulistshow($('.u-student-lst'));
		},
		Linkagedisplay: function() {
			//click
			// var  


			var oulst = $('.m-lst-2 .u-lst'),
				odt = oulst.find('dt');
			odt.click(function(event) {

				$(this).parents('.u-lst').siblings().find('dd').hide().end().find('dt').removeClass('z-sel');

				$(this).addClass('z-sel').next().show();
			});

			oulst.find('.u-ul-1 li').click(function(event) {
				$(this).parents('dd').hide();
			});
		},
		ghover: function() {
			$('.g-fraction-1').find('.u-layer').hover(function() {
				$(this).css('height', 'auto');
			}, function() {
				$(this).css('height', '40px');
			});
		},
		// 对象内部方法
		_ulistshow: function(obj) {
			obg = obj.find('.ul-li');

			obg.find('li').click(function(event) {
				$(this).siblings().removeClass('z-sel').end().addClass('z-sel');
				obj.toggleClass('hide');
			});
		}
	};

})(jQuery);



// $(function(){
// 	var x = 10;
// 	var y = 20;

// 	$('.g-sd').find('dl.u-lst').mouseover(function(e){
// 	 	ddhtml = $(this).find('dd').html();
// 		$(this).find('dd').html("");

// 		var tooltip = "<div id='tooltip'>"+ ddhtml +"<\/div>"; 

// 		$("body").append(tooltip);							 
// 		$(this).find('dd')
// 			.css({
// 				"top": (e.pageY+y) + "px",
// 				"left":  (e.pageX+x)  + "px"
// 			}).show("fast");	  
//     }).mouseout(function(){
//     	// alert(ddhtml);

// 		$(this).find('dd').html(ddhtml);	
// 		// $("#tooltip").remove();	 
//     }).mousemove(function(e){
// 		$('#tooltip')
// 			.css({
// 				"top": (e.pageY+y) + "px",
// 				"left":  (e.pageX+x)  + "px"
// 			});
// 	});
// })


$(function() {

	$('.g-sd').find('dl.u-lst').click(function(event) {

		var _this = $(this);

		function scroTop() {

			var obj = _this.find('dt.z-sel em');
			var
				H = 30,
				X = obj.position().top,
				Y = obj.position().left;
			_this.children('dd').css({
				top: X + H,
				left: Y
			});
		}

		scroTop();


		$('.g-sd').find('.m-mn').scroll(function(event) {
			console.log("scroll");
			var odd = $('.g-sd').find('dl.u-lst').children('dd');

			// console.log($('.g-sd').find('.m-mn').scrollTop());
			if (odd.is(':visible')) {
				scroTop();
			};
		});
	});



});