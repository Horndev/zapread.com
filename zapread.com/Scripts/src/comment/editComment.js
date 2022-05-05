/**
 * 
 * [✓] Native JS
 **/

import { postData } from '../utility/postData';                         // [✓]
import { applyHoverToChildren } from '../utility/userhover';            // [✓]
import { updatePostTimes } from '../utility/datetime/posttime';         // [✓]
const getMakeQuillComment = () => import('./utility/makeQuillComment');
import { makeCommentsQuotable } from '../utility/quotable/quotable';    // [✓]

var isEditing = false;
//var editingId;
var initialHTML;
/* exported editComment */
export function editComment(commentId) {
  if (!isEditing) {
    //editingId = commentId;
    isEditing = true;
    initialHTML = document.getElementById('commentText_' + commentId.toString()).innerHTML;
    console.log("edit " + commentId.toString());
    getMakeQuillComment().then(({ makeQuillComment }) => {
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
          contentEl.classList.remove('ql-snow');
          contentEl.classList.remove('ql-container');
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
    });
  }
  else {
    alert("You can only edit one comment at a time.  Save or Cancel your editing.");
  }
}
window.editComment = editComment;