/**
 * 
 * [✓] Native JS
 **/
import { postData } from '../utility/postData';                         // [✓]
import { applyHoverToChildren } from '../utility/userhover';            // [✓]
import { updatePostTimes } from '../utility/datetime/posttime';         // [✓]
import { makeQuillComment } from './utility/makeQuillComment';          // [✓]
import { makeCommentsQuotable } from '../utility/quotable/quotable';    // [✓]

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
 * @param {number} postId - the post being commented on
 * @returns {Boolean} false to stop default action
 */
export async function writeComment(postId, content) {
  //var postId = el.getAttribute('data-postid');    // This is the post being commented on
  var boxEl = document.getElementById('reply_p' + postId.toString());
  boxEl.style.minHeight = "50px";
  boxEl.style.display = '';   // Make visible
  var spinnerHTML = "" +
    '<div class="ibox-content sk-loading" style="borderStyle: none;">' +
    '<div class="sk-spinner sk-spinner-three-bounce">' +
    '<div class="sk-bounce1"></div>' +
    '<div class="sk-bounce2"></div>' +
    '<div class="sk-bounce3"></div>' +
    '</div>' +
    '</div>';
  boxEl.innerHTML = spinnerHTML;
  
  var url = '/Comment/PostReply/' + postId.toString() + '/';
  fetch(url).then(data => data.text()).then(data => {
    boxEl.innerHTML = data
    boxEl.style.minHeight = "";
  }).then(function () {
    // initialize
    boxEl.style.display = '';   // Make visible
    document.getElementById('wc_' + postId.toString()).style.display = 'none';      // Hide the comment button

    makeQuillComment({
      content: content,
      showloading: true,
      selector: 'editor-container_p' + postId.toString(),
      uid: '_p' + postId.toString(),
      cancelCallback: function () {
        // remove the editor
        var replyEl = document.getElementById('reply_p' + postId.toString());
        replyEl.innerHTML = '';
        document.getElementById('wc_' + postId.toString()).style.display = '';      // Show the comment button
      },
      preSubmitCallback: function () { },
      onSubmitSuccess: function (data) {
        // remove the editor
        var replyEl = document.getElementById('reply_p' + postId.toString());
        replyEl.innerHTML = '';
        document.getElementById('wc_' + postId.toString()).style.display = '';      // Show the comment button
        // and replace with HTML
        var commentsEl = document.getElementById('comments_' + postId.toString());
        commentsEl.innerHTML = data.HTMLString + commentsEl.innerHTML;
        // If user inserted any at mentions - they become hoverable.
        applyHoverToChildren(commentsEl, '.userhint');
        // Format timestamp
        updatePostTimes();
        // Make new comment quotable
        makeCommentsQuotable();
      },
      submitCallback: function (commentHTML) {
        // Submit comment
        return postData("/Comment/AddComment/", {
          CommentContent: commentHTML,
          CommentId: -1,
          PostId: postId,
          IsReply: false
        });
      }
    });
  });
  return false;
}
window.writeComment = writeComment;
