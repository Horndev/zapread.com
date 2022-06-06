/*
 * Post Detail view
 */
import '../../shared/shared';
import '../../utility/ui/vote';
import '../../realtime/signalr';
const getOnLoadedMorePosts = () => import('../../utility/onLoadedMorePosts');
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';
import React from "react";
import ReactDOM from "react-dom";
const getVoteModal = () => import("../../Components/VoteModal");
import '../../css/quill/quillcustom.scss';
import '../../css/posts.css'

/* Vote Modal Component */
getVoteModal().then(({ default: VoteModal }) => {
  ReactDOM.render(<VoteModal />, document.getElementById("ModalVote"));
  const event = new Event('voteReady');
  document.dispatchEvent(event);
});

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;

getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
  onLoadedMorePosts();
});

async function Initialize() {
  // This opens up the vote modal if user clicked to vote (on incomming)
  if (showVoteDialog) {
    document.addEventListener("voteReady", function (e) {
      // Check if vote is for the post, or a comment - this is done using the anchor hash in the url
      var hash = window.location.hash; ///$(location).attr('hash');
      //alert(hash);
      var el;
      if (hash && hash.split('_')[0] === "#cid") {
        var commentId = hash.split('_')[1];

        if (detailPostVote === 1) {
          el = document.getElementById('uVotec_' + commentId);
        } else {
          el = document.getElementById('dVotec_' + commentId);
        }

        // Show vote modal - vote for comment
        const event = new CustomEvent('vote', {
          detail: {
            direction: detailPostVote == 1 ? 'up' : 'down',
            type: 'comment',
            id: commentId,
            target: el
          }
        });
        document.dispatchEvent(event);
      } else if (hash) {
        // Bad anchor
        alert('Error voting');
      } else {
        if (detailPostVote === 1) {
          el = document.getElementById('uVote_' + detailPostId);
        } else {
          el = document.getElementById('dVote_' + detailPostId);
        }
        // Show vote modal - vote for post
        const event = new CustomEvent('vote', {
          detail: {
            direction: detailPostVote == 1 ? 'up' : 'down',
            type: 'post',
            id: detailPostId,
            target: el
          }
        });
        document.dispatchEvent(event);
      }
    });
  }
}

Initialize();