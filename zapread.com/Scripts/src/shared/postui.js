/**/

/**
 * Toggle the comment and it's children
 * @param {any} e element of close button
 * @param {any} d direction force (1 force close, -1 force open, 0 toggle)
 */
export function toggleComment(e, d) {
    $(e).parent().find('.comment-body').first().fadeToggle({ duration: 0 });
    if (d === 1 || (d === 0 && $(e).find('.togglebutton').hasClass("fa-minus-square"))) {
        $(e).removeClass('pull-left');
        $(e).addClass('commentCollapsed');
        $(e).find('.togglebutton').removeClass("fa-minus-square");
        $(e).find('.togglebutton').addClass("fa-plus-square");
        $(e).find('#cel').show();
    }
    else if (d === -1 || (d === 0 && $(e).find('.togglebutton').hasClass("fa-plus-square"))) {
        $(e).addClass('pull-left');
        $(e).removeClass('commentCollapsed');
        $(e).find('.togglebutton').removeClass("fa-plus-square");
        $(e).find('.togglebutton').addClass("fa-minus-square");
        $(e).find('#cel').hide();
    }
}
window.toggleComment = toggleComment;

export function togglePost(e) {
    $(e).parent().find('.social-body').slideToggle();
    if ($(e).find('.togglebutton').hasClass("fa-minus-square")) {
        toggleComment($(e).parent().find('.social-comment-box').find('.comment-toggle'), 1);
        $(e).find('.togglebutton').removeClass("fa-minus-square");
        $(e).find('.togglebutton').addClass("fa-plus-square");
    }
    else {
        toggleComment($(e).parent().find('.social-comment-box').find('.comment-toggle'), -1);
        $(e).find('.togglebutton').removeClass("fa-plus-square");
        $(e).find('.togglebutton').addClass("fa-minus-square");
    }
}
window.togglePost = togglePost;
