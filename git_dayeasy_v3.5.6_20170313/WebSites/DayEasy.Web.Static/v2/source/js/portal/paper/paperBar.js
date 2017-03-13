(function ($) {
    $.extend({
        TestBar: function (options) {
            var barObj = new paperBar(options);
            barObj.create(true);

            return barObj;
        }
    });

    var currentDataCache = "current_selectData";
    var bar;
    var paperBar = function (options) {
        this.defaults = {
            paperBaseInfo: '',//试卷基础信息
            maxNum: 100 //添加题目总数量
        };
        this.options = $.extend({}, this.defaults, options);
        this.newSelected = [];
        this.baseInfo = eval('(' + this.options.paperBaseInfo + ')');
        this.initData = this.baseInfo.ChooseQus;
    }

    paperBar.prototype = {
        create: function (isInit) {
            bar = this;
            if (this.initData && isInit) {//初始化
                window[currentDataCache] = this.initData;
            }

            var barStr = barInit();

            var $bar = $(barStr);
            $bar.find("#li_cancel").bind("click", this.cancel);
            $bar.find("#li_sure").bind("click", this.complete);

            $('body').append($bar);
        },
        add: function (type, id) {
            if (this.getCurrentDataCount() + 1 <= this.options.maxNum) {
                //已经选中的缓存
                var currentData = window[currentDataCache];
                window[currentDataCache] = addData(currentData, type, id);

                //本次选择的题目
                this.newSelected.push(type + "_" + id);

                this.create();

                return true;
            } else {
                $.Dayez.msg("题目总数量不能超过 " + this.options.maxNum + " ！");

                return false;
            }
        },
        del: function (type, id) {
            var currentData = window[currentDataCache];
            if (currentData) {
                var tempQIds = currentData[type];
                if (tempQIds) {
                    $.each(tempQIds, function (index, item) {
                        if (item && item.QId == id) {
                            tempQIds.splice(index, 1);
                        }
                    });
                }
            }
            window[currentDataCache] = currentData;

            this.create();
        },
        getCurrentData: function () {
            return window[currentDataCache];
        },
        getCurrentDataCount: function () {
            var count = 0;

            var currentData = window[currentDataCache];
            if (currentData) {
                $.each(currentData, function (qIndex, qItem) {
                    if (qItem) {
                        count += qItem.length;
                    }
                });
            }
            return count;
        },
        cancel: function () {
            if (bar.initData) {
                window[currentDataCache] = bar.initData;
            }
            //清除当前选择的题目
            if (bar.newSelected.length > 0) {
                $.each(bar.newSelected, function (index, item) {
                    var dataArr = $.trim(item).split('_');
                    if (dataArr.length == 2) {
                        bar.del(dataArr[0], dataArr[1]);
                    }
                });
            }

            bar.complete();
        },
        complete: function () {
            var paperBase = bar.baseInfo;
            paperBase.ChooseQus = window[currentDataCache];

            // 创建Form
            var completeUrl = bar.baseInfo.CompleteUrl;
            if (!completeUrl) {
                completeUrl = '/paper/createpaper/a';
            }
            if (!bar.baseInfo.Type) {
                bar.baseInfo.Type = 'a';
            }

            var form = $('<form></form>');
            form.attr('action', completeUrl);
            form.attr('method', 'post');
            form.attr('target', '_self');

            //paperBaseHidden
            var paperBaseHidden = $('<input type="hidden" name="paperBase" />');
            paperBaseHidden.attr('value', JSON.stringify(paperBase));
            form.append(paperBaseHidden);
            form.appendTo("body");
            form.submit();

            return false;// 注意return false取消链接的默认动作  
        }
    }

    //bar 初始化
    var barInit = function () {
        var barStr = '<div class="right-slide"><div class="r-w-1 trs">';

        var currentQtype = [];

        var currentData = window[currentDataCache];
        if (currentData) {
            $.each(currentData, function (qIndex, qItem) {
                if (qItem) {
                    var num = parseInt(currentQtype[qIndex]);
                    if (isNaN(num)) {
                        num = 0;
                    }
                    currentQtype[qIndex] = num + qItem.length;
                }
            });
        }

        barStr += '<ul class="u-g-1">';
        var qTypeStr = '';
        if (currentQtype.length > 0) {
            var liNum = 0;
            $.each(currentQtype, function (index, item) {
                if (item) {
                    var text = convert2QuType(index);
                    if (liNum < 8) {
                        qTypeStr += '<li><a href="javascript:void(0);" title="' + text + '">' + text + '</a><span class="s-top">' + item + '</span></li>';
                    } else {
                        if (liNum == 8) {
                            qTypeStr += '<li class="g-more">...<div class="g-posa"><ul class="u-m-1">';
                        }
                        qTypeStr += '<li><a href="javascript:void(0);">' +
                            text + '</a><span class="s-top">' +
                           item + '</span></li>';
                        if (index == currentQtype.length - 1) {
                            qTypeStr += '</ul><i class="g-icon2"></i></div></li>';
                        }
                    }
                    liNum++;
                }
            });
        }
        barStr += qTypeStr;
        barStr += '</ul>';

        barStr += '<ul class="u-g-2">' +
            '<li id="li_cancel"><i class="sprite-1 g_icon_9"></i><span class="s-sp-1">取消</span></li>' +
            '<li id="li_sure"><i class="sprite-1 g_icon_10"></i><span class="s-sp-1">完成</span></li>' +
            '</ul>';
        barStr += '</div></div>';

        return barStr;
    }

    //选中添加
    var addData = function (data, type, id) {
        var qIds = [];
        if (data) {
            var tempQIds = data[type];
            if (tempQIds) {
                $.each(tempQIds, function (index, item) {
                    if (item && item.QId == id) {
                        tempQIds.splice(index, 1);
                    }
                });
                qIds = tempQIds;
            }
        } else {
            data = [];
        }

        var newId = {};
        newId.QId = id;
        newId.Type = type;
        newId.Score = 0;

        qIds.push(newId);
        data[type] = qIds;

        return data;
    }

    var convert2QuType = function (type) {
        var qTypes = { "1": "单选", "2": "多选", "3": "不定项", "4": "完形", "5": "判断", "6": "听力", "7": "填空", "8": "证明", "9": "作图", "10": "基础知识", "11": "现代文阅读", "12": "文言文阅读", "13": "推断分析", "14": "实验探究", "15": "默写", "16": "解答", "17": "排序", "18": "阅读理解", "19": "连线", "20": "计算", "21": "作文", "22": "综合", "23": "翻译", "24": "选择" };

        return qTypes[type];
    }

})(jQuery);