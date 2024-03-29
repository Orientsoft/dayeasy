﻿var JsonFormat = (function() {
    var _toString = Object.prototype.toString;

    function format(object, indent_count) {
        var html_fragment = '';
        switch (_typeof(object)) {
        case 'Null':
            html_fragment = _format_null(object);
            break;
        case 'Boolean':
            html_fragment = _format_boolean(object);
            break;
        case 'Number':
            html_fragment = _format_number(object);
            break;
        case 'String':
            html_fragment = _format_string(object);
            break;
        case 'Array':
            html_fragment = _format_array(object, indent_count);
            break;
        case 'Object':
            html_fragment = _format_object(object, indent_count);
            break;
        }
        return html_fragment;
    }

    function _format_null(object) {
        return '<span class="json_null">null</span>';
    }

    function _format_boolean(object) {
        return '<span class="json_boolean">' + object + '</span>';
    }

    function _format_number(object) {
        return '<span class="json_number">' + object + '</span>';
    }

    function _format_string(object) {
        if (0 <= object.search(/^http/)) {
            object = '<a href="' + object + '" target="_blank" class="json_link">' + object + '</a>'
        }
        return '<span class="json_string">"' + object + '"</span>';
    }

    function _format_indent(str, indent) {
        return '<span style="padding-left:' + ((indent - 2) * 2) + 'em;">' + str + '</span>';
    }

    function _format_key(key, indent) {
        return '<span class="json_key" style="padding-left:' + (indent * 2) + 'em;">' + key + '</span>:';
    }

    function _format_array(object, indent_count) {
        var tmp_array = [];
        for (var i = 0, size = object.length; i < size; ++i) {
            tmp_array.push(format(object[i], indent_count));
        }
        return '['
            + tmp_array.join(',')
            + ']';
    }

    function _format_object(object, indent_count) {
        var tmp_array = [];
        for (var key in object) {
            tmp_array.push(_format_key(key, indent_count) + format(object[key], indent_count + 1));
        }
        return '{' + '<br/>'
            + tmp_array.join(',<br/>')
            + '<br/>'
            + _format_indent('}', indent_count + 1);
    }

    function _typeof(object) {
        var tf = typeof object,
            ts = _toString.call(object);
        return null === object ? 'Null' :
            'undefined' == tf ? 'Undefined' :
            'boolean' == tf ? 'Boolean' :
            'number' == tf ? 'Number' :
            'string' == tf ? 'String' :
            '[object Function]' == ts ? 'Function' :
            '[object Array]' == ts ? 'Array' :
            '[object Date]' == ts ? 'Date' : 'Object';
    };

    function loadCssString() {
        var style = document.createElement('style');
        style.type = 'text/css';
        var code = Array.prototype.slice.apply(arguments).join('');
        try {
            style.appendChild(document.createTextNode(code));
        } catch (ex) {
            style.styleSheet.cssText = code;
        }
        document.getElementsByTagName('head')[0].appendChild(style);
    }

    loadCssString(
        '.json_key{ color: purple;display:inline-block;}',
        '.json_null{color: red;}',
        '.json_boolean{ color: gray;}',
        '.json_number{ color: blue;}',
        '.json_string{ color: #077;}',
        '.json_link{ color: #717171;}',
        '.json_array_brackets{}');

    var _JsonFormat = function(origin_data) {
        this.data = 'string' != typeof origin_data ? origin_data :
            JSON && JSON.parse ? JSON.parse(origin_data || "{}") : eval('(' + origin_data + ')');
    };

    _JsonFormat.prototype = {
        constructor: JsonFormat,
        toString: function() {
            return format(this.data, 1);
        }
    }

    return _JsonFormat;

})();