/*
 * jQuery Panel Slider plugin v0.0.1
 * https://github.com/eduardomb/jquery-panelslider
 */

var jsui = {};
jsui.bd = $('body');

(function ($) {
    'use strict';

    var $body = $('body'),
        _sliding = false;

    function _slideIn(panel, options) {
        var panelWidth = panel.outerWidth(true),
            bodyAnimation = {},
            panelAnimation = {};

        if (panel.is(':visible') || _sliding) {
            return;
        }

        _sliding = true;
        panel.addClass('ps-active-panel').css({
            position: 'fixed',
            top: 0,
            height: '100%',
            'z-index': 999999
        });
        panel.data(options);

        switch (options.side) {
            case 'left':
                panel.css({
                    left: '-' + panelWidth + 'px',
                    right: 'auto'
                });
                bodyAnimation['margin-left'] = '+=' + panelWidth;
                panelAnimation.left = '+=' + panelWidth;
                break;

            case 'right':
                panel.css({
                    left: 'auto',
                    right: '-' + panelWidth + 'px'
                });
                bodyAnimation['margin-left'] = '-=' + panelWidth;
                panelAnimation.right = '+=' + panelWidth;
                break;
        }

        $body.animate(bodyAnimation, options.duration);
        panel.show().animate(panelAnimation, options.duration, function () {
            _sliding = false;
        });
    }

    $.panelslider = function (element, options) {
        var active = $('.ps-active-panel');
        var defaults = {
            side: 'left', // panel side: left or right
            duration: 200, // Transition duration in miliseconds
            clickClose: true // If true closes panel when clicking outside it
        };

        options = $.extend({}, defaults, options);

        // If another panel is opened, close it before opening the new one
        if (active.is(':visible') && active[0] != element[0]) {
            $.panelslider.close(function () {
                _slideIn(element, options);
            });
        } else if (!active.length || active.is(':hidden')) {
            _slideIn(element, options);
        }
    };

    $.panelslider.close = function (callback) {
        var active = $('.ps-active-panel'),
            duration = active.data('duration'),
            panelWidth = active.outerWidth(true),
            bodyAnimation = {},
            panelAnimation = {};

        if (!active.length || active.is(':hidden') || _sliding) {
            return;
        }

        _sliding = true;

        switch (active.data('side')) {
            case 'left':
                bodyAnimation['margin-left'] = '-=' + panelWidth;
                panelAnimation.left = '-=' + panelWidth;
                break;

            case 'right':
                bodyAnimation['margin-left'] = '+=' + panelWidth;
                panelAnimation.right = '-=' + panelWidth;
                break;
        }

        active.animate(panelAnimation, duration);
        $body.animate(bodyAnimation, duration, function () {
            active.hide();
            active.removeClass('ps-active-panel');
            _sliding = false;

            if (callback) {
                callback();
            }
        });
    };

    // Bind click outside panel and ESC key to close panel if clickClose is true
    $(document).bind('touchstart keyup', function (e) {
        var active = $('.ps-active-panel');

        if (e.type == 'keyup' && e.keyCode != 27) {
            return;
        }

        if (active.is(':visible') && active.data('clickClose')) {
            $.panelslider.close();
        }
    });

    // Prevent click on panel to close it
    $(document).on('touchstart', '.ps-active-panel', function (e) {
        e.stopPropagation();
    });

    $.fn.panelslider = function (options) {
        this.click(function (e) {
            var active = $('.ps-active-panel'),
                panel = $(this.getAttribute('href'));

            // Close panel if it is already opened otherwise open it
            if (active.is(':visible') && panel[0] == active[0]) {
                $.panelslider.close();
            } else {
                $.panelslider(panel, options);
            }

            e.preventDefault();
            e.stopPropagation();
        });

        return this;
    };


    /*
     * jsui  ����
     * ====================================================
     */
//    var jsui = {};
//    jsui.bd  = $('body');

    var $leftPanel = $('[data-slide-id="left-panel-link"]');
    var $rightPanel = $('[data-slide-id="right-panel-link"]');
    var $bubble_lookup = $('.bubble-lookup');

    $leftPanel
        .panelslider({clickClose: false})
        .click(appendClosurebox);

    $rightPanel
        .panelslider({side: 'right', duration: 200, clickClose: false})
        .click(appendClosurebox);

    // ��ӵ���ɰ�
    // **
    function appendClosurebox() {
        $bubble_lookup.hide();
        $('[data-id="main"]').append('<div class="closure-box"></div>');


    }

    jsui.bd.on('touchstart', '.closure-box', function (event) {
        event.preventDefault();
        $.panelslider.close();
        $(this).remove();
        $bubble_lookup.show();

    });


})(jQuery);

