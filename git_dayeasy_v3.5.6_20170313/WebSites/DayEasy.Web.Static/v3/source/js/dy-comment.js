/**
 * 评论组件
 * Created by shay on 2015/11/12.
 */
(function ($, S, undefined){
    var sourceId, sendComment, appendComment, deleteComment, logger, updateCount, floors;
    logger = S.getLogger('dy-comment');
    var commentItem = '<li>' +
            '<img width="40" height="40" src="{0}" alt="" />' +
            '<div class="box">' +
            '<div class="box-lg-12 mb10 name-color">{1}<span class="comment-time">{2}</span></div>' +
            '<div class="box-lg-12 mb15 comment-message">{3}</div>' +
            '<div class="box-lg-12 d-comment-actions">' +
            '<span class="b-comment-delete" title="删除" data-cid="{4}"><i class="iconfont dy-icon-delete"></i>删除</span>' +
            '<span class="b-comment-to" data-text="回复 {1}："><i class="iconfont dy-icon-edit2"></i> 回复</span>' +
            '</div>' +
            '</div>' +
            '</li>',
        topComment = '<li class="d-comment-item">' +
            '<img height="40" width="40" src="{0}" alt="" />' +
            '<div class="box f-parents">' +
            '<div class="box-lg-12 mb10 name-color">{1}<span class="comment-time">{2}</span><div class="d-comment-floor">{5}</div></div>' +
            '<div class="box-lg-12 mb15 comment-message">{3}</div>' +
            '<div class="box-lg-12 d-comment-actions">' +
            '<span class="b-comment-delete" title="删除" data-cid="{4}"><i class="iconfont dy-icon-delete"></i>删除</span>' +
            '<span class="b-comment-to" data-text="回复 {1}："><i class="iconfont dy-icon-edit2"></i> 回复</span>' +
            '</div>' +
            '</div>' +
            '<div class="comment-children">' +
            '<div class="comment-input f-cb hide">' +
            '<img width="40" height="40" src="{0}" alt="" />' +
            '<textarea placeholder="我有话要说" name="" class="textarea mb5" cols="30" rows="2" maxlength="140"></textarea>' +
            '<input data-comment-id="{4}" class="dy-btn dy-btn-info f-fr b-comment" type="button" value="回复" />' +
            '</div>' +
            '<div class="comment-text">' +
            '<ul class="comment-list"></ul></div>' +
            '</li>';
    floors = ['<span class="d-floor-first">沙发</span>',
        '<span class="d-floor-second">板凳</span>',
        '<span class="d-floor-third">地板</span>',
        '<span class="d-floor-four">地下室</span>'
    ];

    /**
     * 发表评论
     * @param id
     * @param message
     * @param replyId
     * @param callback
     */
    sendComment = function (id, message, replyId, callback){
        $.post('/message/comment', {
            sourceId: id,
            message: message,
            parentId: replyId
        }, function (json){
            if(json.status){
                callback && S.isFunction(callback) && callback.call(this, json.data);
                updateCount(1);
            } else {
                if(json.message && json.message.length){
                    S.alert(json.message, function (){
                        callback && S.isFunction(callback) && callback.call(this);
                    });
                } else {
                    callback && S.isFunction(callback) && callback.call(this);
                }
            }
        });
    };
    /**
     * 前端评论追加
     * @param data
     * @param obj
     */
    appendComment = function (data, obj){
        var html;
        if(!obj){
            var floor;
            if(data.floor > floors.length){
                floor = data.floor + " 楼";
            } else {
                floor = floors[data.floor - 1];
            }
            html = S.format(topComment, S.makeThumb(data.avatar, 40, 40), data.name, data.time, data.message, data.id, floor);
            $('.ul-list-one').append(html);
        } else {
            html = S.format(commentItem, S.makeThumb(data.avatar, 40, 40), data.name, data.time, data.message, data.id);
            var $children, $ul;
            $children = $(obj).parents('.comment-children');
            $ul = $children.find('.comment-list');
            if(!$ul || $ul.length == 0){
                $ul = $('<ul class="comment-list"></ul>');
                $children.find('.comment-text').append($ul);
            }
            $ul.append(html);
        }
    };

    /**
     * 删除评论
     * @param $obj
     */
    deleteComment = function ($obj){
        var id = $obj.data('cid'),
            sourceId = $obj.parents('.comment-bxo').data('source-id');
        if(!id){
            return false;
        }
        $.post('/message/delete-comment', {id: id, sourceId: sourceId}, function (json){
            if(json.status){
                S.alert('删除成功！', function (){
                    $obj.parents('.box').parent().remove();
                    S.isFunction(S.updateComment) && S.updateComment(0 - json.data);
                    updateCount(0 - json.data);
                });
            } else {
                S.alert(json.message);
            }
        });
    };
    updateCount = function (count){
        var $count = $('.d-comment-all small');
        if($count.length == 0){
            var $countBox = $('<div class="d-comment-all">全部回复<small>(0)</small></div>');
            $('.d-comment-box').prepend($countBox);
            $count = $countBox.find('small');
        }
        count = ~~$count.html().replace(/[^\d]/g, '') + count;
        $count.html('(' + count + ')');
    };
    var setCursor = function ($elem, index){
        var len = $elem.val().length,
            elem = $elem.get(0);
        if(elem.setSelectionRange){ // 标准浏览器
            elem.setSelectionRange(index, index)
        } else { // IE9-
            var range = elem.createTextRange();
            range.moveStart("character", -len);
            range.moveEnd("character", -len);
            range.moveStart("character", index);
            range.moveEnd("character", 0);
            range.select();
        }
    };
    $(document)
        .delegate('.b-comment', 'click', function (){
            var $t = $(this),
                $text = $t.parents('.comment-input').find('textarea'),
                sourceId = $t.parents('.comment-bxo').data('source-id'),
                message = S.trim($text.val()),
                replyId = $t.data('comment-id');
            if(!message || !message.length){
                S.alert('请输入评论内容');
                return false;
            }
            $t.attr('disabled', "disabled");
            sendComment(sourceId, message, replyId, function (data){
                if(data){
                    $text.val('').keyup();
                    if(replyId){
                        $t.parents('.comment-input').addClass('hide');
                    }
                    var $box = $('.comment-bxo');
                    data.avatar = $box.data('avatar');
                    data.name = $box.data('name');
                    appendComment(data, (replyId ? $t : undefined));
                    S.isFunction(S.updateComment) && S.updateComment();
                    S.msg('评论成功~！');
                }
                $t.removeAttr('disabled');
            });
        })
        .delegate('.comment-input textarea', 'keydown', function (event){
            if(event.ctrlKey && event.keyCode == 13){
                $(this).parents('.comment-input').find('.b-comment').click();
            }
        })
        .delegate('.b-comment-to', 'click', function (){
            var $input = $(this).parents('.d-comment-item').find('.comment-input');
            if($input.hasClass('hide')){
                $input.removeClass('hide');
                var $text = $input.find('textarea');
                $text.val($(this).data('text'));
                $text.focus();
                setCursor($text, $text.val().length);
            }
            else {
                $input.addClass('hide');
            }
        })
        .delegate('.b-comment-delete', 'click', function (){
            var $t = $(this);
            S.confirm('确认要删除该回复？', function (){
                deleteComment($t);
            });
        });

})(jQuery, SINGER);