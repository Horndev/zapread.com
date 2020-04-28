/**
 * 
 **/

import { postData } from '../utility/postData';
import { applyHoverToChildren } from '../utility/userhover';
import { updatePostTimes } from '../utility/datetime/posttime';
import { makeQuillComment } from './utility/makeQuillComment';
import { makeCommentsQuotable } from '../utility/quotable/quotable';

var editingId;
var isEditing = false;
var initialHTML;
/* exported editComment */
export function editComment(commentId) {
    if (!isEditing) {
        editingId = commentId;
        isEditing = true;
        initialHTML = document.getElementById('commentText_' + commentId.toString()).innerHTML;
        console.log("edit " + commentId.toString());

        makeQuillComment({
            //content: content,
            selector: 'commentText_' + commentId.toString(),
            showloading: false,
            uid: '_' + commentId.toString(),
            cancelCallback: function () {
                // remove the editor
                var contentEl = document.getElementById('commentText_' + commentId.toString());//.querySelectorAll('.ql-editor').item(0);
                contentEl.innerHTML = initialHTML;
                var editToolbar = contentEl.parentElement.querySelectorAll('.ql-toolbar').item(0);
                contentEl.parentElement.removeChild(editToolbar);
                isEditing = false;
            },
            preSubmitCallback: function () { },
            onSubmitSuccess: function (data) {
                // remove the editor and replace with HTML
                var contentEl = document.getElementById('commentText_' + commentId.toString());//.querySelectorAll('.ql-editor').item(0);
                contentEl.innerHTML = contentEl.querySelectorAll('.ql-editor').item(0).innerHTML;
                var editToolbar = contentEl.parentElement.querySelectorAll('.ql-toolbar').item(0);
                contentEl.parentElement.removeChild(editToolbar);
                isEditing = false;

                //var commentsEl = document.getElementById('rcomments_' + commentId.toString());
                //commentsEl.innerHTML = data.HTMLString + commentsEl.innerHTML;

                // If user inserted any at mentions - they become hoverable.
                applyHoverToChildren(contentEl, '.userhint');
                // Format timestamp
                updatePostTimes();
                // Make new comment quotable
                makeCommentsQuotable();
            },
            submitCallback: function (commentHTML) {
                // Submit comment
                // { "CommentContent": content.trim(), "CommentId": editingId }
                return postData("/Comment/UpdateComment/", {
                    CommentContent: commentHTML,
                    CommentId: commentId,
                    //PostId: postId,
                    //IsReply: true
                });
            }
        });

        //var e = "#commentText_" + id.toString();
        //$(e).summernote({
        //    focus: true,
        //    disableDragAndDrop: true,
        //    toolbar: [
        //        ['okbutton', ['ok']],
        //        ['cancelbutton', ['cancel']],
        //        'bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'
        //    ],
        //    buttons: {
        //        ok: OkButton,
        //        cancel: CancelButton
        //    },
        //    height: 100,
        //    hint: {
        //        match: /\B@(\w*)$/,
        //        search: function (keyword, callback) {
        //            if (!keyword.length) return callback();
        //            var msg = JSON.stringify({ 'searchstr': keyword.toString() });
        //            $.ajax({
        //                async: true,
        //                url: '/Comment/GetMentions/',
        //                type: 'POST',
        //                contentType: "application/json; charset=utf-8",
        //                dataType: 'json',
        //                data: msg,
        //                error: function () {
        //                    callback();
        //                },
        //                success: function (res) {
        //                    callback(res.users);
        //                }
        //            });
        //        },
        //        content: function (item) {
        //            return $("<span class='badge badge-info userhint'>").html('@' + item)[0];
        //        }
        //    }
        //});
    }
    else {
        alert("You can only edit one comment at a time.  Save or Cancel your editing.");
    }
}
window.editComment = editComment;


/* exported OkButton */
//export function OkButton(context) {
//    var ui = $.summernote.ui;

//    // create button
//    var button = ui.button({
//        contents: '<i class="fa fa-save"/> Save',
//        tooltip: false,
//        click: function () {
//            var e = "#commentText_" + editingId.toString();
//            $(e).summernote('destroy');
//            var content = $(e).html();
//            var msg = { "CommentContent": content.trim(), "CommentId": editingId };
//            console.log(msg);
//            $.post("/Comment/UpdateComment",
//            msg,
//                function (data) {
//                    if (data.Success) {
//                        console.log('update comment successful.');
//                    }
//                    else {
//                        alert("Error updating comment");
//                    }
//                });
//            isEditing = false;
//            }
//        });
//    return button.render();   // return button as jquery object
//}
//window.OkButton = OkButton;

///* exported CancelButton */
//export function CancelButton(context) {
//    var ui = $.summernote.ui;
//    // create button
//    var button = ui.button({
//        contents: '<i class="fa fa-times"/> Cancel',
//        tooltip: false,
//        click: function () {
//            var e = "#commentText_" + editingId.toString();
//            $(e).summernote('reset');
//            // This returns the editor to normal state
//            $(e).summernote('destroy');
//            isEditing = false;
//        }
//    });
//    return button.render();   // return button as jquery object
//}
//window.CancelButton = CancelButton;

