/**
 * User information page
 * 
 **/
import '../../shared/shared';
import '../../utility/ui/vote';
import '../../realtime/signalr';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
const getOnLoadedMorePosts = () => import('../../utility/onLoadedMorePosts');
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { loadachhover } from '../../utility/achievementhover';
import { loadmore } from '../../utility/loadmore';
import { postJson } from "../../utility/postData";
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';
import React from "react";
import ReactDOM from "react-dom";
const getVoteModal = () => import("../../Components/VoteModal");

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
window.loadMoreComments = loadMoreComments;
window.loadachhover = loadachhover;
window.toggleUserIgnore = toggleUserIgnore;
window.BlockNumber = 10;                        // Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

/**
 * userAppId is set externally in the razor view
*/
async function LoadFollowersAsync() {
  //console.log("LoadFollowersAsync", window.userAppId);
  await fetch("/User/Followers/" + window.userAppId).then(response => {
    return response.text();
  }).then(html => {
    var groupsBoxEl = document.getElementById("top-followers");
    groupsBoxEl.innerHTML = html;
  })
}

async function LoadFollowingAsync() {
  //console.log("LoadFollowingAsync", window.userAppId);
  await fetch("/User/Following/" + window.userAppId).then(response => {
    return response.text();
  }).then(html => {
    var groupsBoxEl = document.getElementById("top-following");
    groupsBoxEl.innerHTML = html;
  })
}

LoadFollowersAsync();
LoadFollowingAsync();

/**
 * Wrapper for loadmore
 * 
 * [✓] Native JS
 * 
 **/
export function userloadmore(userId) {
  loadmore({
    url: '/User/InfiniteScroll/',
    blocknumber: window.BlockNumber,
    sort: "New",
    userId: userId
  });
}
window.loadmore = userloadmore;

getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
  onLoadedMorePosts();
});

export function toggleUserIgnore(id) {
  postJson("/User/ToggleIgnore/", {
    id: id
  }).then((response) => {
    if (response.success) {
      if (response.added) {
        document.getElementById('i_' + id.toString()).innerHTML = "<i class='fa-solid fa-circle'></i> Un-Ignore ";
      }
      else {
        document.getElementById('i_' + id.toString()).innerHTML = "<i class='fa-solid fa-ban'></i> Ignore ";
      }
    }
  });
  return false;
}