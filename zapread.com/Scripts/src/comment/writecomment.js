/**
 * 
 * [✓] Native JS
 **/
import { Dropdown } from 'bootstrap.native/dist/bootstrap-native-v4';
import { postJson } from '../utility/postData';
import { applyHoverToChildren } from '../utility/userhover';
import { updatePostTimesOnEl } from '../utility/datetime/posttime';
//import { makeQuillComment } from './utility/makeQuillComment';
const getMakeQuillComment = () => import('./utility/makeQuillComment');
import { makeCommentsQuotable } from '../utility/quotable/quotable';
import { enableVoting } from '../utility/onLoadedMorePosts';

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

    getMakeQuillComment().then(({ makeQuillComment }) => {
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
          commentsEl.innerHTML = data.HTMLString + commentsEl.innerHTML; // prepend
          var newCommentEl = commentsEl.querySelector('#comment_' + data.CommentId.toString());
          // If user inserted any at mentions - they become hoverable.
          applyHoverToChildren(newCommentEl, '.userhint');
          // Format timestamp
          updatePostTimesOnEl(newCommentEl, false);
          // Make new comment quotable
          makeCommentsQuotable();
          // activate dropdown (done manually using bootstrap.native)
          var menuDropdownEl = newCommentEl.querySelector(".dropdown-toggle");
          var dropdownInit = new Dropdown(menuDropdownEl);
          // make comment voteable
          enableVoting("vote-comment-up", 'up', 'comment', 'data-commentid', newCommentEl);
          enableVoting("vote-comment-dn", 'down', 'comment', 'data-commentid', newCommentEl);
        },
        submitCallback: function (commentHTML) {
          // Submit comment
          return postJson("/Comment/AddComment/", {
            CommentContent: commentHTML,
            CommentId: -1,
            PostId: postId,
            IsReply: false
          });
        }
      });
    });
  });
  return false;
}
window.writeComment = writeComment;