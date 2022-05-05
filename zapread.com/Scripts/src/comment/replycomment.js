﻿/**
 * Handle user click on reply to comment
 **/

import { postData } from '../utility/postData';
import { applyHoverToChildren } from '../utility/userhover';
import { updatePostTimes } from '../utility/datetime/posttime';
const getMakeQuillComment = () => import('./utility/makeQuillComment');
import { makeCommentsQuotable } from '../utility/quotable/quotable';
import { enableVoting } from '../utility/onLoadedMorePosts';

/**
 * Handle the loading of the comment reply into the DOM, and creation of
 * the text editor, submission, toolbars, etc.
 * 
 * @example <XX onclick="replyComment(1,5);" \>
 *
 * [X] Native JS implementation
 *
 * @param {number} commentId the comment id
 * @param {number} postId 
 **/
export async function replyComment(commentId, postId, content) {
  var el = document.getElementById('reply_c' + commentId.toString());
  el.style.display = '';
  var spinnerHTML = "" +
    '<div class="sk-loading" style="border-style: none;">' +
    '<div class="sk-spinner sk-spinner-three-bounce">' +
    '<div class="sk-bounce1"></div>' +
    '<div class="sk-bounce2"></div>' +
    '<div class="sk-bounce3"></div>' +
    '</div>' +
    '</div>';
  el.innerHTML = spinnerHTML;

  // The comment box is currently rendered on server TODO: move to client-side render
  var url = '/Comment/GetInputBox' + "/" + commentId.toString();
  fetch(url).then(data => data.text()).then(data => {
    el.innerHTML = data
  }).then(function () {
    getMakeQuillComment().then(({ makeQuillComment }) => {
      makeQuillComment({
        content: content,
        showloading: true,
        selector: 'editor-container_c' + commentId.toString(),
        uid: '_c' + commentId.toString(),
        cancelCallback: function () {
          // remove the editor
          var replyEl = document.getElementById('reply_c' + commentId.toString());
          replyEl.innerHTML = '';//.parentNode.removeChild(replyEl);
        },
        preSubmitCallback: function () { },
        onSubmitSuccess: function (data) {
          // remove the editor
          var replyEl = document.getElementById('reply_c' + commentId.toString());
          replyEl.parentNode.removeChild(replyEl);
          // and replace with HTML
          var commentsEl = document.getElementById('rcomments_' + commentId.toString());
          commentsEl.innerHTML = data.HTMLString + commentsEl.innerHTML;
          // If user inserted any at mentions - they become hoverable.
          applyHoverToChildren(commentsEl, '.userhint');
          // Format timestamp
          updatePostTimes();
          // Make new comment quotable
          makeCommentsQuotable();
          // make comment voteable
          enableVoting("vote-comment-up", 'up', 'comment', 'data-commentid', commentsEl);
          enableVoting("vote-comment-dn", 'down', 'comment', 'data-commentid', commentsEl);
        },
        submitCallback: function (commentHTML) {
          // Submit comment
          return postData("/Comment/AddComment/", {
            CommentContent: commentHTML,
            CommentId: commentId,
            PostId: postId,
            IsReply: true
          });
        }
      });
    });
  });

  return false;
}
window.replyComment = replyComment;