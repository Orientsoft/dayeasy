/**
 * 题库处理
 * Created by shoy on 2014/9/22.
 */
(function (S, $) {
    //分割词
    var styles, optionWords = [], getTag, tagWords = {}, i, getCorrectOptions, questionTypes;
    styles = {
        0: '题干',
        1: '小问',
        2: '选项',
        4: '答案'
    };
    /**
     * 题型
     * @type {{1: {title: string, name: string}, 2: {title: string, name: string}, 3: {title: string, name: string}, 4: {title: string, name: string}, 5: {title: string, name: string}, 6: {title: string, name: string}, 7: {title: string, name: string}, 8: {title: string, name: string}, 9: {title: string, name: string}, 10: {title: string, name: string}, 11: {title: string, name: string}, 12: {title: string, name: string}, 13: {title: string, name: string}, 14: {title: string, name: string}, 15: {title: string, name: string}, 16: {title: string, name: string}, 17: {title: string, name: string}, 18: {title: string, name: string}, 19: {title: string, name: string}, 20: {title: string, name: string}, 21: {title: string, name: string}, 22: {title: string, name: string}, 23: {title: string, name: string}, 24: {title: string, name: string}}}
     */
    questionTypes = {
        "1": {title: '单', name: '单项选择题'},
        "2": {title: '多', name: '多项选择题'},
        "3": {title: '不', name: '不定项选择题'},
        "4": {title: '完', name: '完形填空'},
        "5": {title: '判', name: '判断题'},
        "6": {title: '听', name: '听力题'},
        "7": {title: '填', name: '填空题'},
        "8": {title: '证', name: '证明题'},
        "9": {title: '图', name: '作图题'},
        "10": {title: '基', name: '基础知识'},
        "11": {title: '现', name: '现代文阅读'},
        "12": {title: '文', name: '文言文阅读'},
        "13": {title: '推', name: '推断分析题'},
        "14": {title: '实', name: '实验探究题'},
        "15": {title: '默', name: '默写题'},
        "16": {title: '解', name: '解答题'},
        "17": {title: '排', name: '排序题'},
        "18": {title: '阅', name: '阅读理解'},
        "19": {title: '连', name: '连线题'},
        "20": {title: '计', name: '计算题'},
        "21": {title: '作', name: '作文'},
        "22": {title: '综', name: '综合题'},
        "23": {title: '翻', name: '翻译'},
        "24": {title: '选', name: '选择题'}
    };
    /**
     * 获取快速出题标签
     * @param code
     * @returns {string}
     */
    getTag = function (code) {
        return "【" + (styles[code] || "") + "】";
    };
    tagWords = {
        title: getTag(0),
        detail: getTag(1),
        option: getTag(2),
        answer: getTag(4)
    };
    //选择字母
    for (i = 0; i < 26; i++) {
        optionWords.push(String.fromCharCode(i + 65));
    }
    //获取正确选项
    getCorrectOptions = function (answers) {
        var options = "";
        for (i = 0; i < answers.length; i++) {
            if (answers[i].is_correct)
                options += optionWords[answers[i].sort];
        }
        return options;
    };
    //快速出题分割符
    S._mix(tagWords, {
        optionItem: '[\\r\\n]|(\\s{4,})',
        detailItem: '[\\r\\n]',
        optionReg: '^[a-z][.．。。、、,，，\\s]'
    });
    S._mix(S, {
        tagWords: tagWords,
        optionWords: optionWords,
        ranges: ["自己", "校内", "全网"],
        questionTypes: questionTypes,
        /**
         * 在光标处插入文本
         * @param obj TextArea
         * @param str 文本
         */
        insertText: function (obj, str) {
            if (document.selection) {
                var sel = document.selection.createRange();
                sel.text = str;
            } else if (S.isNumber(obj.selectionStart) && S.isNumber(obj.selectionEnd)) {
                var startPos = obj.selectionStart,
                    endPos = obj.selectionEnd,
                    cursorPos = startPos,
                    tmpStr = obj.value;
                obj.value = tmpStr.substring(0, startPos) + str + tmpStr.substring(endPos, tmpStr.length);
                cursorPos += str.length;
                obj.selectionStart = obj.selectionEnd = cursorPos;
            } else {
                obj.value += str;
            }
            obj.focus();
        },
        /**
         * 光标移动到末尾
         * @param obj TextArea
         */
        moveToEnd: function (obj) {
            obj.focus();
            var len = obj.value.length;
            if (document.selection) {
                var sel = obj.createTextRange();
                sel.moveStart('character', len);
                sel.collapse();
                sel.select();
            } else if (S.isNumber(obj.selectionStart) && S.isNumber(obj.selectionEnd)) {
                obj.selectionStart = obj.selectionEnd = len;
            }
        },
        /**
         * 移除回车换行符
         * @param str
         * @returns {XML|string|void|*}
         */
        trimTrn: function (str) {
            return str.replace(/(^([\r\n\t\s]+))|(\2$)/gi, '');
        },
        /**
         * 获取单个快速标签
         * @param code
         * @returns {Array}
         */
        getTag: function (code) {
            return styles[code] || "";
        },
        /**
         * 获取支持的快速标签列表
         * @param code
         * @returns {Array}
         */
        getTags: function (code) {
            var list = [];
            for (i in styles) {
                if (i == 0 || (i & code) > 0)
                    list.push(styles[i]);
            }
            return list;
        },
        /**
         * 快速出题识别
         * @param body
         * @param tags
         */
        recognition: function (body, tags) {
            if (!body || !S.isString(body))
                return false;
//            body = body.replace(/(<\/p>)|(<br\/>)/gi, '\r\n');
//            body = body.replace(/&nbsp;/gi, " ");
            body = S.stripTags(body);//排除标签
            var question = {},
                answerStr;
            /**
             * 识别小问
             * @param str
             */
            var recognitionDetails = function (str) {
                    if (!str) return false;
                    var list = str.split(tagWords.detail),
                        details = [];
                    if (list.length < 2) return false;
                    for (var i = 0; i < list.length; i++) {
                        var item = list[i],
                            options = item.split(tagWords.option);
                        //获取小问选项
                        if (singer.inArray(styles[2], tags) && options.length == 2 && i > 0) {
                            var opts = recognitionOptions(options[0]);
                            if (opts) {
                                details[i - 1].answers = opts;
                            }
                            if (options[1].indexOf(tagWords.answer) >= 0) {
                                answerStr = options[1];
                                continue;
                            }
                            options[1] && details.push({
                                body: S.trimTrn(options[1]),
                                images: []
                            });
                        } else {
                            if (item.indexOf(tagWords.answer) >= 0) {
                                answerStr = item;
                                continue;
                            }
                            //回车换行
                            var dList = item.split(new RegExp(tagWords.detailItem, "gi"));
                            for (var j = 0; j < dList.length; j++) {
                                if (dList[j]) {
                                    details.push({
                                            body: S.trimTrn(dList[j]),
                                            images: [],
                                            sort: j
                                        }
                                    );
                                }
                            }
                        }
                    }
                    return details;
                },
                /**
                 * 识别选项
                 * @param str
                 */
                recognitionOptions = function (str) {
                    if (!str) return false;
                    var options = [],
                        optionStr = str.split(tagWords.option),
                        reg = new RegExp(tagWords.optionItem, "gi"),
                        list = [],
                        i;
                    for (i = 0; i < optionStr.length; i++) {
                        if (optionStr[i].indexOf(tagWords.answer) >= 0) {
                            answerStr = optionStr[i];
                            continue;
                        }
                        var itemList = optionStr[i].split(reg);
                        for (var j = 0; j < itemList.length; j++) {
                            if (itemList[j])
                                list.push(itemList[j]);
                        }
                    }
                    for (i = 0; i < list.length; i++) {
                        var item = S.trimTrn(S.trim(list[i]));
                        item && options.push({
                            body: item.replace(new RegExp(tagWords.optionReg, "gi"), ''),
                            is_correct: false,
                            sort: i
                        });
                    }
                    return options;
                },
                /**
                 * 识别答案
                 * @param str
                 */
                recognitionAnswers = function (str) {
                    if (!str) return false;
                    return S.trim(str.split(tagWords.answer)[0]);
                };

            var qList = body.split(tagWords.title);
            if (!qList.length || qList.length < 2 || !qList[1]) {
                question = {body: qList[0], images: []};
                //小问
                if (S.inArray(styles[1], tags)) {
                    question.details = [];
                }
                //选项或答案
                question.answers = [
                    {body: '', images: []}
                ];
                return question;
            }
            question.body = S.trim(qList[0]);
            question.images = [];
            //有小问
            if (S.inArray(styles[1], tags)) {
                if (qList[1].indexOf(tagWords.detail) >= 0) {
                    var details = recognitionDetails(qList[1]);
                    if (details) {
                        question.details = details;
                    }
                } else {
                    question.details = [];
                    answerStr = qList[1];
                }
            } else if (singer.inArray(styles[2], tags)) {
                if (qList[1].indexOf(tagWords.option) >= 0 || !singer.inArray(styles[4], tags)) {
                    //选项
                    var options = recognitionOptions(qList[1]);
                    if (options) {
                        question.answers = options;
                    } else {
                        answerStr = qList[1]
                    }
                } else {
                    question.answers = [];
                }
            } else {
                answerStr = qList[1];
            }
            if (singer.inArray(styles[4], tags)) {
                //答案
                var answer = recognitionAnswers(answerStr);
                question.answers = [];
                question.answers.push({
                    body: answer || "",
                    images: [],
                    is_correct: true
                });
            }
            return question;
        },
        /**
         * 检查问题基础逻辑
         * @param question
         * @param currentType
         */
        checkQuestion: function (question, currentType) {
            var i, j, detail, answer, correct;
            if (!question || (!question.body && !question.images.length)) {
                singer.msg("题干不能为空！");
                return false;
            }
            if (question.details && question.details.length) {
                for (i = 0; i < question.details.length; i++) {
                    detail = question.details[i];
                    //if (!detail.body && !detail.images.length) {
                    //    singer.msg(singer.format("小问{0}内容不能为空！", i + 1));
                    //    return false;
                    //}
                    //选项
                    if (detail.answers && detail.answers.length) {
                        if (detail.answers.length < 2) {
                            singer.msg(singer.format("小问{0}选项至少2个！", i + 1));
                            return false;
                        }
                        correct = 0;
                        for (j = 0; j < detail.answers.length; j++) {
                            answer = detail.answers[j];
                            //if (!answer.body && !answer.images.length) {
                            //    singer.msg(singer.format("小问{0}选项{1}不能为空！", i + 1, singer.optionWords[j]));
                            //    return false;
                            //}
                            if (answer.is_correct)
                                correct++;
                        }
                        if (correct == 0) {
                            singer.msg(singer.format("小问{0}没有设置正确答案！", i + 1));
                            return false;
                        }
                    }
                }
            }
            if (currentType.style == 2) {
                //选项
                if (!question.answers || question.answers.length < 2) {
                    singer.msg("至少2个选项！");
                    return false;
                }
                correct = 0;
                for (i = 0; i < question.answers.length; i++) {
                    answer = question.answers[i];
                    //if (!answer.body && !answer.images.length) {
                    //    singer.msg(singer.format("选项{0}不能为空！", singer.optionWords[i]));
                    //    return false;
                    //}
                    if (answer.is_correct)
                        correct++;
                }
                if (correct == 0) {
                    singer.msg(singer.format("没有设置正确答案！", i + 1));
                    return false;
                }
                if (!currentType.multi && correct > 1) {
                    singer.msg("该题型不支持多个个正确答案！");
                    return false;
                }
            }
            return true;
        },
        /**
         * 本地化数字显示 1,234,567
         * @param num
         * @returns {string}
         */
        formatNum: function (num) {
            return parseFloat(num).toLocaleString().replace(/\.\d+$/gi, '');
        },
        /**
         * 获取选项是否水平显示
         * @param options
         * @returns {boolean}
         */
        optionModel: function (options) {
            for (var i = 0; i < options.length; i++) {
                var item = options[i],
                    len = 0;
                //有公式
                if (item.body.indexOf('\\[') >= 0) return true;
                len += (item.images && item.images.length) ? 18 : 0;
                if (S.lengthCn(item.body) + len > 35) return false;
            }
            return true;
        },
        /**
         * 获取正确答案
         * @param question
         * @returns {string}
         */
        getCorrectAnswers: function (question) {
            var body = "",
                i;
            if (question.is_objective) {
                //客观题
                if (!question.details || !question.details.length) {
                    //无小问
                    body += getCorrectOptions(question.answers);
                } else {
                    //有小问
                    for (i = 0; i < question.details.length; i++) {
                        body += '(' + (i + 1) + '). ';
                        body += getCorrectOptions(question.details[i].answers) + '<br/>';
                    }
                }
            } else {
                body = question.answers[0].body;
            }
            return body;
        },
        /**
         * 将空格回车转换为html标签
         * @param html
         * @returns {XML|string|void|*}
         */
        formatText: function (html) {
            var reg = new RegExp("\n+", "gi");
            if (html.match(reg))
                html = html.replace(reg, '<br/>');
            return html.replace(/\s{2}/gi, '&nbsp;&nbsp;');
        },
        textEditor: function (item, callback) {
            var html = item.body,
                $editor = $("#dy-editor"),
                ue;
            if (!$editor || !$editor.length)
                $("body").append('<script id="dy-editor" type="text/plain"></script>');
            ue = UE.getEditor("dy-editor", {
                toolbars: [
                    ['source', '|', 'Undo', 'Redo', '|', 'bold', 'italic', 'underline', '|', 'inserttable', 'deletetable', 'kityformula', 'spechars', 'superscript', 'subscript']
                ],
                //focus时自动清空初始化时的内容
                autoClearinitialContent: true,
                //关闭字数统计
                wordCount: false,
                //关闭elementPath
                elementPathEnabled: false,
                //关闭自动保存
                enableAutoSave: false,
                //关闭右键菜单功能
                enableContextMenu: false,
                saveInterval: 5 * 1000 * 1000,
                pasteplain: true,
                //是否保持toolbar的位置不动
                //autoFloatEnabled: false,
                //topOffset: 30,
                //默认的编辑区域高度
                initialFrameHeight: 240,
                initialFrameWidth: 650
            });
            var d = singer.dialog({
                title: '内容编辑',
                content: $("#dy-editor"),
                zIndex: 150,
                okValue: '确认',
                ok: function () {
                    var ueHtml = ue.getContent();
                    //替换掉P标签
                    item.body = ueHtml.replace(/(^<p>)|(<\/p>)|(<p><\/p>)/gi, '')
                        .replace(/<p>/gi, '<br/>')
                        .replace(/(<br\/>)+$/gi, '');
                    callback && S.isFunction(callback) && callback.call(this);
                    ue.destroy();
                    this.close().remove();
                },
                cancelValue: '取消',
                cancel: function () {
                    this.close().remove();
                    ue.destroy();
                }
            });
            ue.ready(function () {
                ue.setContent(html);
                d.showModal();
            });
        }
    });
    if (typeof(template) != 'undefined') {
        template.helper('optionWord', function (sort) {
            return String.fromCharCode(sort + 65);
        });
        template.helper('optionModel', function (answers) {
            return S.optionModel(answers) ? 'q-options-horizontal' : '';
        });
    }
    $(document).delegate('.q-image', 'click', function (e) {
        e.stopPropagation();
        var src = $(this).find('img').attr('src');
        src = src.replace(/_s(\d+)x[^\.]*/gi, '');
        S.showImage(src);
        return false;
    });
})(singer, jQuery);