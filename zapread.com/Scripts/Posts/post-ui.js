
var toggleComment = function (e) {
    $(e).parent().find('.comment-body').first().fadeToggle({ duration: 0 });

    //$(e).parent().find('.comment-body').each(function (index, item) {
    //    $(item).show();
    //});

    if ($(e).find('.togglebutton').hasClass("fa-minus-square")) {
        $(e).removeClass('pull-left');
        $(e).addClass('commentCollapsed');
        $(e).find('.togglebutton').removeClass("fa-minus-square");
        $(e).find('.togglebutton').addClass("fa-plus-square");
        $(e).find('#cel').show();
    }
    else {
        $(e).addClass('pull-left');
        $(e).removeClass('commentCollapsed');
        $(e).find('.togglebutton').removeClass("fa-plus-square");
        $(e).find('.togglebutton').addClass("fa-minus-square");
        $(e).find('#cel').hide();
    }
};

var togglePost = function (e) {
    $(e).parent().find('.social-body').slideToggle();
    if ($(e).find('.togglebutton').hasClass("fa-minus-square")) {
        $(e).find('.togglebutton').removeClass("fa-minus-square");
        $(e).find('.togglebutton').addClass("fa-plus-square");
    }
    else {
        $(e).find('.togglebutton').removeClass("fa-plus-square");
        $(e).find('.togglebutton').addClass("fa-minus-square");
    }
};
