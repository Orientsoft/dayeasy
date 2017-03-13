(function ($, S) {
    var index = 0,
        $users = $('.dj-users li'),
        count = $users.length,
        loadPicture,
        $header = $('.dj-users h2'),
        $prev = $('.dj-prev'),
        $next = $('.dj-next'),
        $picture = $('.dj-picture img'),
        $user,
        prev,
        next,
        current = {},
        loadDialog,
        loading,
        hideUser;

    loadDialog = S.dialog({
        content: '图片加载中...'
    });
    loadPicture = function (showIndex) {
        if (loading) {
            return false;
        }
        index = showIndex;
        $user = $users.eq(index);
        current = {
            name: $user.data('name'),
            picture: $user.data('img')
        };
        loading = true;
        loadDialog.show();
        $picture.attr('src', current.picture);
        $users.not($user).removeClass('active');
        $user.addClass('active');
        $header.find('span').html(current.name);
        $header.find('small').html(S.format('{0}/{1}', index + 1, count));
        $prev.toggleClass('hide', index == 0);
        $next.toggleClass('hide', index == count - 1);
    };
    prev = function () {
        if (index <= 0)
            return false;
        loadPicture(index - 1);
    };
    next = function () {
        if (index >= count - 1)
            return false;
        loadPicture(index + 1);
    };
    $('.dj-users h2,.dj-users>i').click(function () {
        $('.dj-users').toggleClass('show');
    });
    $prev.bind('click', function () {
        prev();
    });
    $next.bind('click', function () {
        next();
    });
    $users.bind('click', function () {
        var cIndex = $users.index($(this));
        hideUser = true;
        loadPicture(cIndex);
    });
    $picture.bind('load.picture', function () {
        loadDialog.close();
        loading = false;
        $('.dj-users').removeClass('show');
        hideUser = false;
    });
    $(window).bind('keyup', function (e) {
        switch (e.keyCode) {
            case 37:
                prev();
                break;
            case 39:
                next();
                break;
        }
    });
    loadPicture(0);
})(jQuery, SINGER);