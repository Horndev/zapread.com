/*
 * 
 */
import { initCommentInput } from './initCommentInput';

/**
 * @name writeComment
 * @category Comment Functions
 * @summary Initialize and show comment input for a post
 *
 * @description
 * Shows the comment input box for a post
 * 
 * Does not require jQuery.
 *
 * @param {Element} el - the value to convert
 * @returns {Boolean} the parsed date in the local time zone
 */
export function writeComment(el) {
    var id = el.getAttribute('data-postid');    //var id = $(e).data("postid");
    //console.log('writeComment id: ' + id.toString());
    initCommentInput(id);
    el.style.display = 'none';                  //$(e).hide();
    document.querySelectorAll('.note-statusbar').item(0).style.display = 'none';        //$(".note-statusbar").css("display", "none");
    var replyElement = document.querySelectorAll('#preply_' + id.toString()).item(0);   //$('#preply_' + id.toString()).slideDown(200);
    replyElement.style.display = '';
    return false;
}