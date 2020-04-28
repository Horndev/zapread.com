/**
 * When text is highlighted, the user is presented with option to quote in reply
 **/

//import $ from 'jquery'; // yuck ...

import tippy from 'tippy.js';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';

import '../../css/quotable.css'

import { initCommentInput } from '../../comment/initCommentInput';

var selectionText;
var selectionMarker;
var markerId;

window.selectionMarker = selectionMarker;

function encode(s) {
    var x = document.createElement("div");
    x.innerText = s;
    return x.innerHTML;
}

/**
 * 
 **/
export function makePostsQuotable() {
    var elements = document.querySelectorAll(".post-quotable");
    Array.prototype.forEach.call(elements, function (e, _i) {
        makeQuotable(e, true);
        e.classList.remove("post-quotable");
    });
}

export function makeCommentsQuotable() {
    var elements = document.querySelectorAll(".comment-quotable");
    Array.prototype.forEach.call(elements, function (e, _i) {
        makeQuotable(e, false);
        e.classList.remove("comment-quotable");
    });
}

export function makeQuotable(e, isPost) {
    // Trigger when mouse is released (i.e. possible selection made)
    e.addEventListener("mouseup", function () {
        var selection = getSelected();
        if (selection && encode(selection.toString()) !== "") {
            // User made a selection
            markerId = "sel_" + new Date().getTime() + "_" + Math.random().toString().substr(2);
            window.selectionMarker = markSelection(markerId);
            window.markerId = markerId;
            selectionText = encode(selection.toString());
            var postId = e.getAttribute('data-postid');
            var commentId = isPost ? -1 : e.getAttribute('data-commentid');

            var popText = '<h3 class="quotable-header">Quote</h3><blockquote>' + selectionText + '</blockquote><hr/>' +
                '<button class="btn btn-sm btn-link" onclick="QuoteComment(' + postId + ',' + commentId + ',' + (isPost?'true':'false') +');"><i class="fa fa-reply"></i> Reply</button>';// +
            //'<button class="btn btn-sm btn-link" onclick="postQuoteComment(' + postId + ',true);">' +
            //'<i class="fa fa-reply"></i><i class="fa fa-bell"></i> Mention</button>'; 

            tippy('#' + markerId, {//document.getElementById(markerId), {
                content: popText,
                appendTo: document.body,//document.getElementById(markerId).closest('.post-box').parent,
                theme: 'light-border',
                allowHTML: true,
                delay: 0,
                interactive: true,
                interactiveBorder: 30,
                hideOnClick: true
            });
            setTimeout(function () { document.getElementById(markerId)._tippy.show() }, 0);
        }
    });
}

/**
 * Initialize a comment from the selected quote
 * 
 * @param {number} postId       The identifier for the post
 * @param {number} commentId    The identifier for the comment (only used for comments)
 * @param {boolean} isPost  If true, we are quoting a post.  If false, we are quoting a comment.
 */
async function QuoteComment(postId, commentId, isPost) {
    var quotetext = '<blockquote class="blockquote">' + selectionText + '</blockquote><br/>';
    //var quill = null;
    document.getElementById(markerId)._tippy.hide()
    if (isPost) {
        await writeComment(postId, quotetext);
        //quill = getQuillInstance('#editor-container_p' + postId.toString());
    } else {
        await replyComment(commentId, postId, quotetext);
        //quill = getQuillInstance('#editor-container_c' + commentId.toString());
    }

    //quill.clipboard.dangerouslyPasteHTML(quotetext);
}
window.QuoteComment = QuoteComment; // export as global

function getQuillInstance(quillID) {
    var container = document.querySelector(quillID);
    var quill = container.__quill;
    return quill;
}

//export function makeCommentsQuotable() {
    //$(".comment-quotable").each(function (ix, e) {
    //    // Trigger when mouse is released (i.e. possible selection made)
    //    $(e).mouseup(function () {
    //        var selection = getSelected();
    //        $(selectionMarker).popover('hide');
    //        if (selection && encode(selection.toString()) !== "") {
    //            // User made a selection
    //            var markerId = "sel_" + new Date().getTime() + "_" + Math.random().toString().substr(2);
    //            selectionMarker = markSelection(markerId);
    //            selectionText = encode(selection.toString());
    //            var commentid = $(e).data('commentid');
    //            var popText = selectionText + '<hr/>' +
    //                '<button class="btn btn-sm btn-link" onclick="commentQuoteComment(' + commentid + ');"><i class="fa fa-reply"></i> Reply</button>' +
    //                '<button class="btn btn-sm btn-link" onclick="commentQuoteComment(' + commentid + ',true);">' +
    //                '<i class="fa fa-reply"></i><i class="fa fa-bell"></i> Mention</button>';
    //            $(selectionMarker).popover({
    //                trigger: "hover",
    //                html: true,
    //                sanitize: false,
    //                animation: false,
    //                title: "Quote",
    //                placement: "top",
    //                content: popText
    //            }).on('hidden.bs.popover', function () {
    //                $(selectionMarker).popover('dispose');
    //            })
    //                .popover("show");
    //        }
    //    });
    //    $(e).removeClass("post-quotable");
    //});
//}

// adapted from https://stackoverflow.com/a/1589912/847076
var markSelection = function (markerId) {
    var markerTextChar = "\ufeff";
    var markerTextCharEntity = "&#xfeff;";
    var markerEl = markerId;
    var sel, range;

    if (document.selection && document.selection.createRange) {
        // Clone the TextRange and collapse
        range = document.selection.createRange().duplicate();
        range.collapse(false);

        // Create the marker element containing a single invisible character by creating literal HTML and insert it
        range.pasteHTML('<span class="pop-quote" id="' + markerId + '" style="position: relative;">' + markerTextCharEntity + '</span>');
        markerEl = document.getElementById(markerId);
    } else if (window.getSelection) {
        sel = window.getSelection();

        if (sel.getRangeAt) {
            range = sel.getRangeAt(0).cloneRange();
        } else {
            // Older WebKit doesn't have getRangeAt
            range = document.createRange();
            range.setStart(sel.anchorNode, sel.anchorOffset);
            range.setEnd(sel.focusNode, sel.focusOffset);

            // Handle the case when the selection was selected backwards (from the end to the start in the
            // document)
            if (range.collapsed !== sel.isCollapsed) {
                range.setStart(sel.focusNode, sel.focusOffset);
                range.setEnd(sel.anchorNode, sel.anchorOffset);
            }
        }

        range.collapse(false);

        // Create the marker element containing a single invisible character using DOM methods and insert it
        markerEl = document.createElement("span");
        markerEl.id = markerId;
        markerEl.appendChild(document.createTextNode(markerTextChar));
        range.insertNode(markerEl);
    }
    return markerEl;
};

function getSelected() {
    if (window.getSelection) { return window.getSelection(); }
    else if (document.getSelection) { return document.getSelection(); }
    else {
        var selection = document.selection && document.selection.createRange();
        if (selection.text) { return selection.text; }
        return false;
    }
}

// TODO: Needs cleanup and code refactor with showreply(id)
export function commentQuoteComment(id, mention) {
    //mention = typeof mention !== 'undefined' ? mention : false;
    //$('#c_reply_' + id.toString()).toggle('show');
    //var quotetext = '<blockquote class="blockquote">' + selectionText + '</blockquote><br/>';
    //if (mention) {
    //    var username = $('#comment_' + id.toString()).find('.post-username').data('user');
    //    quotetext = '<span class="badge badge-info userhint" style="margin-bottom: 10px;margin-right: 10px;">@@' + username + '</span>' +
    //        '<blockquote class="blockquote" style="display:inline-flex;">' + selectionText + '</blockquote><br/><br/>';
    //}
    //$('#c_reply_' + id.toString()).load('/Comment/GetInputBox' + "/" + id.toString(), function () {
    //    $(".c_input").summernote({
    //        callbacks: {
    //            onImageUpload: function (files) {
    //                that = $(this);
    //                sendFile(files[0], that);
    //            }
    //        },
    //        focus: false,
    //        placeholder: 'Write comment...',
    //        disableDragAndDrop: true,
    //        toolbar: ['bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],
    //        minHeight: 60,
    //        maxHeight: 300,
    //        hint: {
    //            match: /\B@(\w*)$/,
    //            search: function (keyword, callback) {
    //                if (!keyword.length) return callback();
    //                var msg = JSON.stringify({ 'searchstr': keyword.toString() });
    //                $.ajax({
    //                    async: true,
    //                    url: '/Comment/GetMentions/',
    //                    type: 'POST',
    //                    contentType: "application/json; charset=utf-8",
    //                    dataType: 'json',
    //                    data: msg,
    //                    error: function () {
    //                        callback();
    //                    },
    //                    success: function (res) {
    //                        callback(res.users);
    //                    }
    //                });
    //            },
    //            content: function (item) {
    //                return $("<span class='badge badge-info userhint'>").html('@' + item)[0];
    //            }
    //        }
    //    });
    //    $(".note-statusbar").css("display", "none");
    //    $('#c_reply_' + id.toString()).find('.c_input').summernote('code', quotetext);
    //    $(selectionMarker).popover('hide');
    //    var editbox = $('#c_reply_' + id.toString()).parent().find('.note-editable');
    //    editbox.placeCursorAtEnd();
    //    $('#c_reply_' + id.toString()).find('.c_input').summernote('focus');
    //});
}
window.commentQuoteComment = commentQuoteComment;

function toggleCommentInput(id, show) {
    //show = typeof show !== 'undefined' ? show : false;
    //initCommentInput(id);
    //$(".note-statusbar").css("display", "none");
    //if (!show) {
    //    $('#comments_' + id.toString()).slideToggle(200);
    //    $('#preply_' + id.toString()).slideToggle(200);
    //}
    //else {
    //    $('#comments_' + id.toString()).slideDown(200);
    //    $('#preply_' + id.toString()).slideDown(200);
    //}
}

export function postQuoteComment(id, mention) {
    //mention = typeof mention !== 'undefined' ? mention : false;
    //toggleCommentInput(id, true);
    //var quotetext = '<blockquote class="blockquote">' + selectionText + '</blockquote><br/>';
    //if (mention) {
    //    var username = $('#post_' + id.toString()).find('.post-username').data('user');
    //    quotetext = '<span class="badge badge-info userhint" style="margin-bottom: 10px;margin-right: 10px;">@@' + username + '</span>' +
    //        '<blockquote class="blockquote" style="display:inline-flex;">' + selectionText + '</blockquote><br/><br/>';
    //}
    //$('.c_input_' + id.toString()).summernote('code', quotetext);
    //$(selectionMarker).popover('hide');
    //var editbox = $('.c_input_' + id.toString()).parent().find('.note-editable');
    //editbox.placeCursorAtEnd();
    //$('.c_input_' + id.toString()).summernote('focus');
}
window.postQuoteComment = postQuoteComment;