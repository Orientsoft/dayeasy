/**
 * Created by shoy on 2014/9/24.
 */
(function ($, S){
    var logger = S.getLogger('js/base/utils.js'), zTreeTextClick = null;
    $.fn.extend({
        /**
         * 基于zTree
         * @param options
         * @param url
         * @param data
         * @returns {*}
         */
        tree: function (options, url, data){
            if(!$.fn.zTree){
                logger.error('没有引用zTree相关js文件！');
                return false;
            }
            var $t = $(this).find("#pointTree"),
                opt = $.extend({}, {
                    view: {
                        dblClickExpand: true,
                        showIcon: false,
                        selectedMulti: false
                    },
                    async: {
                        enable: false,
                        url: null
                    },
                    check: {
                        enable: true,
//                        chkStyle:"checkbox",
                        chkboxType: {"Y": "", "N": ""}
                    },
                    data: {
                        key: {
                            children: "children",
                            name: "text",
                            title: "",
                            url: "url"
                        },
                        simpleData: {
                            enable: true,
                            rootPId: "#"
                        }
                    }
                }, options || {});
            if(url){
                opt.async.enable = true;
                opt.async.url = function (treeId, treeNode){
                    var id = treeNode ? treeNode.id : 0;
                    data = data || {};
                    data.parent_id = id;
                    return url + (url.indexOf('?') >= 0 ? '&' : '?') + S.stringify(data);
                };
            }
            $t.addClass("ztree");
            zTreeTextClick = $.fn.zTree.init($t, opt);
            return $.fn.zTree.getZTreeObj($t.attr("id"));
        }
    });
    S._mix(S, {
        /**
         * 标签插件
         * @param option
         */
        tags: function (option){
            var opt = $.extend({}, {
                    container: $(".d-tags"), //容器
                    max: 3, //最多几个
                    maxLength: 10, //标签个数
                    type: 0, //标签类型
                    data: [], //当前标签
                    canEdit: false, //是否可编辑
                    change: undefined //变更事件
                }, option || {}),
                i,
                bind;
            if(!opt.container.length) return false;
            /**
             * 绑定数据
             */
            bind = function (){
                opt.container.empty();
                for (i = 0; i < opt.data.length; i++) {
                    var $item = $(S.format('<mark>{0}<em></em></mark>', opt.data[i]));
                    if(opt.canEdit){
                        $item.append('<div data-index="' + i + '" title="删除标签" class="fa fa-times tag-remove"></div>');
                    }
                    opt.container.append($item);
                }
                if(opt.data.length < opt.max && opt.canEdit){
                    var $edit = $('<div class="tag-edit">'),
                        $btn = $('<button class="btn btn-tag"><i class="fa fa-plus"></i></button>');
                    $edit.append($btn);
                    $edit.append('<span class="help-inline">(最多' + opt.max + '个,每个' + opt.maxLength + '字符以内)</span>');
                    opt.container.append($edit);
                }
            };
            bind();
            if(opt.canEdit){
                /**
                 * 添加标签
                 */
                var addTag = function (){
                    var tag = S.trim(opt.container.find("input[type=text]").val());
                    tag = S.stripTags(tag);
                    if(tag && !S.inArray(tag, opt.data)){
                        opt.data.push(tag);
                        opt.change && S.isFunction(opt.change) && opt.change.call(this, opt.data);
                    }
                    bind();
                };
                //事件绑定
                opt.container
                    .delegate("mark", "mouseenter", function (){
                        $(this).addClass("mark-hover");
                    })
                    .delegate("mark", "mouseleave", function (){
                        $(this).removeClass("mark-hover");
                    })
                    .delegate(".tag-remove", "click", function (){
                        opt.data.splice($(this).data("index"), 1);
                        opt.change && S.isFunction(opt.change) && opt.change.call(this, opt.data);
                        bind();
                    })
                    .delegate("input[type=text]", "blur", function (){
                        addTag();
                    })
                    .delegate("input[type=text]", "keypress", function (e){
                        //Enter键
                        if(e.keyCode === 13){
                            addTag();
                            return false;
                        }
                    })
                    .delegate("input[type=text]", "keydown", function (e){
                        //Tab键
                        if(e.keyCode === 9){
                            addTag();
                            opt.container.find(".btn-tag").click();
                            return false;
                        }
                    })
                    .delegate(".btn-tag", "click", function (){
                        if(opt.data.length >= opt.max)
                            return false;
                        var $edit = opt.container.find(".tag-edit");
                        $edit.empty();
                        var $input = $('<input type="text" class="form-control" maxlength="' + opt.maxLength + '" />');
                        $edit.append($input);
                        $input = $edit.find("input");
                        $input.focus();
                    });
            }

            return {
                get: function (){
                    return opt.data;
                },
                set: function (tag){
                    if(!opt.canEdit || opt.data.length >= opt.max)
                        return false;
                    if(tag && !S.inArray(tag, opt.data)){
                        opt.data.push(tag);
                        opt.change && S.isFunction(opt.change) && opt.change.call(this, opt.data);
                        bind();
                        return true;
                    }
                    return false;
                }
            }
        },
        /**
         * 知识点插件
         * @param options
         */
        points: function (options){
            var option = $.extend({}, {
                    container: $(".d-points"), //容器
                    treeId: '001',
                    subject_id: 0, //科目ID
                    stage: 2, //学段
                    max: 5, //最多选择个数
                    data: [], //当前选择的知识点
                    treeData: [], //树形结构数据
                    body: $('.dy-body'), //body
                    url: S.sites.apps + '/sys/kpoints', //api接口Url
                    init: undefined, //初始化数据
                    change: undefined, //change事件
                    isAppendData: true, //是否追加选中数据
                    check: {
                        enable: true,
                        chkStyle: "checkbox",
                        chkboxType: {"Y": "", "N": ""}
                    }
                }, options || {}),
                treeId = 'tree_' + option.treeId,
                $tree = $('#' + treeId),
                $t = $(option.container),
                $body = $(option.body),
                setOffset,
                checkTree,
                bindTree,
                showPoints,
                placeHolder,
                tree;
            if(!option.container.length) return false;
            /**
             * 设置位置
             */
            setOffset = function (){
                $tree.css({
                    top: $t.offset().top + $t.outerHeight(),
                    left: $t.offset().left
                });
            };
            placeHolder = function (){
                if(!$t.children().length && (!option.data || !option.data.length)){
                    if(!$t.find(".d-place").length)
                        $t.append('<div class="d-place">选择知识点</div>');
                } else {
                    $t.find(".d-place").remove();
                }
            };
            placeHolder();
            /**
             * 检查树容器
             * @returns {boolean}
             */
            checkTree = function (){
                if($tree.length) return true;
                $tree = $('<div class="tree-container" id="' + treeId + '"><ul id="pointTree"></ul></div>');
                $("body").append($tree);
                if(option.isAppendData && option.data && option.data.length){
                    for (var i = 0; i < option.data.length; i++) {
                        var node = option.data[i];
                        $t.append('<div class="d-point" data-id="' + node.li_attr.title + '">' + node.name + '</div>');
                    }
                }
                $t.bind("click.tree", function (e){
                    setOffset(); //增加点击重新定位 by ybg
                    e.stopPropagation();
                    if(option.init && S.isFunction(option.init)){
                        var opt = option.init.call(this) || {};
                        if(opt.subject_id != option.subject_id || opt.stage != option.stage){
                            option = $.extend({}, option, opt);
                            bindTree();
                        }
                    }
                    if(!option.subject_id || !option.stage) return false;
                    showPoints();
                });
            };

            /**
             * 绑定树
             */
            bindTree = function (){
                if(!option.subject_id || !option.stage) return false;
                setOffset();
                var onNodeCreated = undefined;
                if(option.data && option.data.length){
                    onNodeCreated = function (event, treeId, treeNode){
                        for (var i = 0; i < option.data.length; i++) {
                            if(option.data[i].id == treeNode.id)
                                tree.checkNode(treeNode, true);
                        }
                    };
                } else {
                    if(option.isAppendData){
                        $t.empty();
                    }
                }

                tree = window["tree" + option.treeId] = $tree.tree({
                    callback: {
                        beforeCheck: function (treeId, treeNode){
                            if(treeNode.checked) return true;
                            var selected = tree.getCheckedNodes(true);
                            if(selected.length >= option.max){
                                treeNode.checked = false;
                                singer.msg && singer.msg(singer.format("最多选择{0}个知识点!", option.max));
                                return false;
                            }
                        },
                        onCheck: function (e, treeId, treeNode){
                            var selected = tree.getCheckedNodes(true);
                            option.treeData = [];
                            option.data = [];
                            if(option.isAppendData){
                                $t.html("");
                            }
                            for (var i = 0; i < selected.length; i++) {
                                var node = selected[i];
                                option.treeData.push({id: node.li_attr.title, text: node.text});
                                option.data.push({id: node.li_attr.title, name: node.text});
                                if(option.isAppendData){
                                    $t.append('<div class="d-point" data-id="' + node.li_attr.title + '">' + node.text + '</div>');
                                }
                            }
                            placeHolder();
                            setTimeout(setOffset, 200);
                            option.change &&
                            S.isFunction(option.change) &&
                            option.change.call(this, option.data, option.treeData);
                        },
                        onNodeCreated: onNodeCreated,
                        // zTree点击文字勾选复选框
                        onClick: function (e, treeId, treeNode){
                            var bingKnowledge;
                            /**
                             * 绑定知识点列表
                             */
                            bingKnowledge=function(){
                                // 绑定chebox选择
                                zTreeTextClick.checkNode(treeNode, !treeNode.checked, true);
                                var selected = tree.getCheckedNodes(true);
                                option.treeData = [];
                                option.data = [];
                                if(option.isAppendData){
                                    $t.html("");
                                }
                                for (var i = 0; i < selected.length; i++) {
                                    var node = selected[i];
                                    option.treeData.push({id: node.li_attr.title, text: node.text});
                                    option.data.push({id: node.li_attr.title, name: node.text});
                                    if(option.isAppendData){
                                        $t.append('<div class="d-point" data-id="' + node.li_attr.title + '">' + node.text + '</div>');
                                    }
                                }
                                placeHolder();
                                setTimeout(setOffset, 200);
                                option.change &&
                                S.isFunction(option.change) &&
                                option.change.call(this, option.data, option.treeData);
                            }
                            var selected = tree.getCheckedNodes(true);
                            if(selected.length >= option.max){
                                if(treeNode.checked){
                                    bingKnowledge();
                                }else {
                                    singer.msg && singer.msg(singer.format("最多选择{0}个知识点!", option.max));
                                }
                            }else{
                                bingKnowledge();
                            }
                        }
                    },
                    check: option.check
                }, option.url, {
                    stage: option.stage,
                    subject_id: option.subject_id
                });
            };


            /**
             * 展示知识点树
             */
            showPoints = function (){
                if($tree.data("show")){
                    $tree.fadeOut();
                    $tree.data("show", false);
                    $body.unbind("click.tree");
                } else {
                    $tree.fadeIn();
                    $tree.data("show", true);
                    setTimeout(function (){
                        $body.bind("click.tree", function (){
                            $tree.fadeOut();
                            $tree.data("show", false);
                            $body.unbind("click.tree");
                        });
                    }, 200);
                }
            };

            checkTree();
            bindTree();
            return {
                get: function (){
                    return {
                        data: option.data,
                        treeData: option.treeData
                    }
                },
                set: function (data){
                    option.data = data;
                    bindTree();
                    placeHolder();
                }
            };
        },
        /**
         *  分享范围组件
         * @param options
         */
        shareRange: function (options){
            options = $.extend({}, {
                container: $(".q-share"),
                ranges: ["自己", "校内", "全网"],
                current: 0,
                update: undefined
            }, options || {});
            var $container = $(options.container),
                init,
                currentRange,
                $choose,
                updateRanges,
                $item;
            init = function (range){
                if(S.isNumber(range)) options.current = range;
                currentRange = options.ranges[options.current];
                $container.empty();
                $container.append('<span class="q-range">' + currentRange + '</span><i class="fa fa-angle-down"></i>');
                $choose = $('<div class="q-choose">');
                for (var i = 0; i < options.ranges.length; i++) {
                    if(i == options.current) continue;
                    $item = $('<span class="q-choose-item">' + options.ranges[i] + '</span>');
                    $item.data("range", i);
                    $choose.append($item);
                }
                $container.append($choose);
            };
            updateRanges = function (newRanges){
                options.ranges = newRanges;
            };
            init();
            $container
                .delegate(".q-choose-item", "click", function (){
                    var range = $(this).data("range");
                    options.update && S.isFunction(options.update) && options.update.call(this, range);
                })
                .bind("mouseenter", function (){
                    $(this).addClass("share-hover");
                })
                .bind("mouseleave", function (){
                    $(this).removeClass("share-hover");
                });
            return {init: init, updateRanges: updateRanges};
        },
        /**
         * 图片上传
         */
        uploader: window.WebUploader && (function (){
            return window.WebUploader.create({
                // swf文件路径
                swf: singer.sites.static + '/js/Uploader.swf',

                // 文件接收服务端。
                server: singer.sites.file + '/uploader?type=1',

                // 选择文件的按钮。可选。
                // 内部根据当前运行是创建，可能是input元素，也可能是flash.
                pick: '#btn-upload',

                // 不压缩image, 默认如果是jpeg，文件上传前会压缩一把再上传！
                resize: false,
                auto: true,
                fileNumLimit: 1,
                // 只允许选择图片文件。
                accept: {
                    title: '图片文件',
                    extensions: 'gif,jpg,jpeg,png',
                    mimeTypes: 'image/gif,image/jpeg,image/png'
                }
            });
        })()
    });
})(jQuery, SINGER);