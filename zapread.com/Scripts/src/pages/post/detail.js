﻿/*
 * Post Detail view
 */
import '../../shared/shared';
import '../../utility/ui/vote';
import '../../realtime/signalr';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;

onLoadedMorePosts();

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
        vote(commentId, detailPostVote, 2, 100, el);
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
        vote(detailPostId, detailPostVote, 1, 100, el);
      }
    });
  }
}

Initialize();