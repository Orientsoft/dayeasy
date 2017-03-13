seajs.config({

    base: "../source/js/",
    alias: {
      "jquery": "jquery.js"
    }
  });


seajs.use(['base/base.js','http://static.100hg.com/js/v1/jquery-1.9.1.min.js'],function(ex){
	// source/js/sea/sea.js
	ex.show();  //1http://static.100hg.com/js/v1/jquery-1.9.1.min.js
	

	ex.aaaa();




	show();  //2  
	
	function show(){
		//alert(2);
	}
	
});