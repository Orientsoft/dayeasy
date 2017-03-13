UE.registerUI('kityformula', function (editor, uiname) {

    // 创建dialog
    var kfDialog = new UE.ui.Dialog({

        // 指定弹出层路径
        iframeUrl: editor.options.UEDITOR_HOME_URL + 'third-party/kityformula-plugin/kityFormulaDialog.html',
//        iframeUrl: singer.sites.main + '/Views/Shared/formulaDialog.html',
        // 编辑器实例
        editor: editor,
        // dialog 名称
        name: uiname,
        // dialog 标题
        title: '插入公式',

        // dialog 外围 css
        cssRules: 'width:783px; height: 386px;',

        //如果给出了buttons就代表dialog有确定和取消
        buttons: [
            {
                className: 'edui-okbutton',
                label: '确定',
                onclick: function () {
                    kfDialog.close(true);
                }
            },
            {
                className: 'edui-cancelbutton',
                label: '取消',
                onclick: function () {
                    kfDialog.close(false);
                }
            }
        ]});

    editor.ready(function () {
        UE.utils.cssRule('kfformula', 'img.kfformula{vertical-align: middle;}', editor.document);
    });

    var iconUrl = editor.options.UEDITOR_HOME_URL + 'third-party/kityformula-plugin/kf-icon.png';
    var tmpLink = document.createElement('a');
    tmpLink.href = iconUrl;
    iconUrl = tmpLink.href;
    //console.log(uiname);
    var kfBtn = new UE.ui.Button({
        name: '插入',
        title: '插入公式',
        //需要添加的额外样式，指定icon图标
        cssRules: 'background: url("' + iconUrl + '") !important',
        onclick: function () {
            //渲染dialog
            kfDialog.render();
            kfDialog.open();
        }
    });

    //当点到编辑内容上时，按钮要做的状态反射
    editor.addListener('selectionchange', function () {
        var state = editor.queryCommandState(uiname);
        if (state == -1) {
            kfBtn.setDisabled(true);
            kfBtn.setChecked(false);
        } else {
            kfBtn.setDisabled(false);
            kfBtn.setChecked(state);
        }
    });
    return kfBtn;
});

UE.Editor.prototype.getKfContent = function (callback) {

    var me = this;
    var actionUrl = me.getActionUrl(me.getOpt('scrawlActionName')),
        params = UE.utils.serializeParam(me.queryCommandValue('serverparam')) || '',
        url = UE.utils.formatUrl(actionUrl + (actionUrl.indexOf('?') == -1 ? '?' : '&') + params);

    // 找到所有的base64
    var count = 0;
    var imgs = me.body.getElementsByTagName('img');
    var base64Imgs = [];
    UE.utils.each(imgs, function (item) {
        var imgType = item.getAttribute('src').match(/^[^;]+/)[0];
        if (imgType === 'data:image/png') {
            base64Imgs.push(item);
        }
    });

    if (base64Imgs.length == 0) {
        execCallback();
    } else {
        UE.utils.each(base64Imgs, function (item) {

            var opt = {};
            opt[me.getOpt('scrawlFieldName')] = item.getAttribute('src').replace(/^[^,]+,/, '');
            opt.onsuccess = function (xhr) {
                var json = UE.utils.str2json(xhr.responseText),
                    url = me.options.scrawlUrlPrefix + json.url;

                item.setAttribute('src', url);
                item.setAttribute('_src', url);

                count++;

                execCallback();
            };
            opt.onerror = function (err) {
                console.error(err);
                count++;

                execCallback();
            };


            UE.ajax.request(url, opt);

        });
    }

    function execCallback() {
        if (count >= base64Imgs.length) {
            ue.sync();
            callback(me.getContent());
        }
    }
};
///import core
///plugin 编辑器默认的过滤转换机制

UE.plugins['defaultfilter'] = function () {
    var me = this;
    me.setOpt({
        'allowDivTransToP': true,
        'disabledTableInTable': true,
        'rgb2Hex': true
    });
    //默认的过滤处理
    //进入编辑器的内容处理
    me.addInputRule(function (root) {
        var allowDivTransToP = this.options.allowDivTransToP;
        var val;

        function tdParent(node) {
            while (node && node.type == 'element') {
                if (node.tagName == 'td') {
                    return true;
                }
                node = node.parentNode;
            }
            return false;
        }

        //进行默认的处理
        root.traversal(function (node) {
            if (node.type == 'element') {
                if (!UE.dom.dtd.$cdata[node.tagName] && me.options.autoClearEmptyNode && UE.dom.dtd.$inline[node.tagName] && !UE.dom.dtd.$empty[node.tagName] && (!node.attrs || UE.utils.isEmptyObject(node.attrs))) {
                    if (!node.firstChild()) node.parentNode.removeChild(node);
                    else if (node.tagName == 'span' && (!node.attrs || UE.utils.isEmptyObject(node.attrs))) {
                        node.parentNode.removeChild(node, true)
                    }
                    return;
                }
                switch (node.tagName) {
                    case 'style':
                    case 'script':
                        node.setAttr({
                            cdata_tag: node.tagName,
                            cdata_data: (node.innerHTML() || ''),
                            '_ue_custom_node_': 'true'
                        });
                        node.tagName = 'div';
                        node.innerHTML('');
                        break;
                    case 'a':
                        if (val = node.getAttr('href')) {
                            node.setAttr('_href', val)
                        }
                        break;
                    case 'img':
                        //todo base64暂时去掉，后边做远程图片上传后，干掉这个
//                        if (val = node.getAttr('src')) {
//                            if (/^data:/.test(val)) {
//                                node.parentNode.removeChild(node);
//                                break;
//                            }
//                        }
                        node.setAttr('_src', node.getAttr('src'));
                        break;
                    case 'span':
                        if (UE.browser.webkit && (val = node.getStyle('white-space'))) {
                            if (/nowrap|normal/.test(val)) {
                                node.setStyle('white-space', '');
                                if (me.options.autoClearEmptyNode && UE.utils.isEmptyObject(node.attrs)) {
                                    node.parentNode.removeChild(node, true)
                                }
                            }
                        }
                        val = node.getAttr('id');
                        if (val && /^_baidu_bookmark_/i.test(val)) {
                            node.parentNode.removeChild(node)
                        }
                        break;
                    case 'p':
                        if (val = node.getAttr('align')) {
                            node.setAttr('align');
                            node.setStyle('text-align', val)
                        }
                        //trace:3431
//                        var cssStyle = node.getAttr('style');
//                        if (cssStyle) {
//                            cssStyle = cssStyle.replace(/(margin|padding)[^;]+/g, '');
//                            node.setAttr('style', cssStyle)
//
//                        }
                        //p标签不允许嵌套
                        UE.utils.each(node.children, function (n) {
                            if (n.type == 'element' && n.tagName == 'p') {
                                var next = n.nextSibling();
                                node.parentNode.insertAfter(n, node);
                                var last = n;
                                while (next) {
                                    var tmp = next.nextSibling();
                                    node.parentNode.insertAfter(next, last);
                                    last = next;
                                    next = tmp;
                                }
                                return false;
                            }
                        });
                        if (!node.firstChild()) {
                            node.innerHTML(UE.browser.ie ? '&nbsp;' : '<br/>')
                        }
                        break;
                    case 'div':
                        if (node.getAttr('cdata_tag')) {
                            break;
                        }
                        //针对代码这里不处理插入代码的div
                        val = node.getAttr('class');
                        if (val && /^line number\d+/.test(val)) {
                            break;
                        }
                        if (!allowDivTransToP) {
                            break;
                        }
                        var tmpNode, p = UE.uNode.createElement('p');
                        while (tmpNode = node.firstChild()) {
                            if (tmpNode.type == 'text' || !UE.dom.UE.dom.dtd.$block[tmpNode.tagName]) {
                                p.appendChild(tmpNode);
                            } else {
                                if (p.firstChild()) {
                                    node.parentNode.insertBefore(p, node);
                                    p = UE.uNode.createElement('p');
                                } else {
                                    node.parentNode.insertBefore(tmpNode, node);
                                }
                            }
                        }
                        if (p.firstChild()) {
                            node.parentNode.insertBefore(p, node);
                        }
                        node.parentNode.removeChild(node);
                        break;
                    case 'dl':
                        node.tagName = 'ul';
                        break;
                    case 'dt':
                    case 'dd':
                        node.tagName = 'li';
                        break;
                    case 'li':
                        var className = node.getAttr('class');
                        if (!className || !/list\-/.test(className)) {
                            node.setAttr()
                        }
                        var tmpNodes = node.getNodesByTagName('ol ul');
                        UE.utils.each(tmpNodes, function (n) {
                            node.parentNode.insertAfter(n, node);
                        });
                        break;
                    case 'td':
                    case 'th':
                    case 'caption':
                        if (!node.children || !node.children.length) {
                            node.appendChild(UE.browser.ie11below ? UE.uNode.createText(' ') : UE.uNode.createElement('br'))
                        }
                        break;
                    case 'table':
                        if (me.options.disabledTableInTable && tdParent(node)) {
                            node.parentNode.insertBefore(UE.uNode.createText(node.innerText()), node);
                            node.parentNode.removeChild(node)
                        }
                }

            }
//            if(node.type == 'comment'){
//                node.parentNode.removeChild(node);
//            }
        })

    });

    //从编辑器出去的内容处理
    me.addOutputRule(function (root) {

        var val;
        root.traversal(function (node) {
            if (node.type == 'element') {

                if (me.options.autoClearEmptyNode && UE.dom.dtd.$inline[node.tagName] && !UE.dom.dtd.$empty[node.tagName] && (!node.attrs || UE.utils.isEmptyObject(node.attrs))) {

                    if (!node.firstChild()) node.parentNode.removeChild(node);
                    else if (node.tagName == 'span' && (!node.attrs || UE.utils.isEmptyObject(node.attrs))) {
                        node.parentNode.removeChild(node, true)
                    }
                    return;
                }
                switch (node.tagName) {
                    case 'div':
                        if (val = node.getAttr('cdata_tag')) {
                            node.tagName = val;
                            node.appendChild(UE.uNode.createText(node.getAttr('cdata_data')));
                            node.setAttr({cdata_tag: '', cdata_data: '', '_ue_custom_node_': ''});
                        }
                        break;
                    case 'a':
                        if (val = node.getAttr('_href')) {
                            node.setAttr({
                                'href': UE.utils.html(val),
                                '_href': ''
                            })
                        }
                        break;
                        break;
                    case 'span':
                        val = node.getAttr('id');
                        if (val && /^_baidu_bookmark_/i.test(val)) {
                            node.parentNode.removeChild(node)
                        }
                        //将color的rgb格式转换为#16进制格式
                        if (me.getOpt('rgb2Hex')) {
                            var cssStyle = node.getAttr('style');
                            if (cssStyle) {
                                node.setAttr('style', cssStyle.replace(/rgba?\(([\d,\s]+)\)/g, function (a, value) {
                                    var array = value.split(",");
                                    if (array.length > 3)
                                        return "";
                                    value = "#";
                                    for (var i = 0, color; color = array[i++];) {
                                        color = parseInt(color.replace(/[^\d]/gi, ''), 10).toString(16);
                                        value += color.length == 1 ? "0" + color : color;
                                    }
                                    return value.toUpperCase();

                                }))
                            }
                        }
                        break;
                    case 'img':
                        if (val = node.getAttr('_src')) {
                            node.setAttr({
                                'src': node.getAttr('_src'),
                                '_src': ''
                            })
                        }
                }
            }
        })
    });
};