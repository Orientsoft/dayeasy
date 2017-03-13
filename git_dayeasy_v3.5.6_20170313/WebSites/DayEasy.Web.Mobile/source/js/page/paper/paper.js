/**
 * Created by bozhuang on 2016/6/15.
 */

//转换成汉字数字
var convertToChinese = function(num) {
    var sortNum = ["〇", "一", "二", "三", "四", "五", "六", "七", "八", "九", "十", "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十"];
    var no = parseInt(num);
    if (isNaN(no) || no > 20) {
        no = 1;
    }
    return sortNum[no];
};