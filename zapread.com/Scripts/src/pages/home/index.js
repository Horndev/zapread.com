/**
 * This is the main landing page for ZapRead
 **/

import '../../shared/shared';
import '../../utility/ui/vote';
import '../../realtime/signalr';
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';
const getloadMoreComments = () => import('../../comment/loadmorecomments');
const getMicroCharts = () => import('micro-charts');
const getVoteModal = () => import("../../Components/VoteModal");
const getOnLoadedMorePosts = () => import('../../utility/onLoadedMorePosts');
import { getJson } from "../../utility/getData";
import React from "react";
import ReactDOM from "react-dom";

/* Vote Modal Component */
getVoteModal().then(({ default: VoteModal }) => {
  ReactDOM.render(<VoteModal />, document.getElementById("ModalVote"));
  const event = new Event('voteReady');
  document.dispatchEvent(event);
});

import '../../css/Site.css';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';
import '../../css/posts.css'
const getTippy = () => import('tippy.js');
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';

//import { vote } from '../../utility/ui/vote';

// Make global (called from html)
import('../../comment/writecomment').then(({ writeComment }) => {
  window.writeComment = writeComment;
});
import('../../comment/replycomment').then(({ replyComment }) => {
  window.replyComment = replyComment;
});
import('../../comment/editcomment').then(({ editComment }) => {
  window.editComment = editComment;
});
getloadMoreComments().then(({ loadMoreComments }) => {
  window.loadMoreComments = loadMoreComments;
});

import('../../utility/loadmore').then(({ addposts, loadmore }) => {
  window.loadmore = loadmore;
  getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
    async function LoadTopPostsAsync() {
      await getJson('/Home/TopPosts/?sort=' + postSort).then((response) => {
        if (response.success) {
          document.querySelectorAll('#posts').item(0).querySelectorAll('.ibox-content').item(0).classList.remove("sk-loading");
          addposts(response, onLoadedMorePosts); // Insert posts
          document.querySelectorAll('#btnLoadmore').item(0).style.display = '';
        } else {
          // Did not work
          getSwal().then(({ default: Swal }) => {
            Swal.fire("Error", "Error loading posts: " + response.message, "error");
          });
        }
      });
    }
    //Execute
    LoadTopPostsAsync();
  });
});

/** Global vars */
window.BlockNumber = 10;   //Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

window.addEventListener('resize', function (event) {
  //console.log(event);
  var elements = document.querySelectorAll(".post-box");
  Array.prototype.forEach.call(elements, function (el, _i) {
    if (!el.classList.contains('read-more-expanded')) {
      if (parseFloat(getComputedStyle(el, null).height.replace("px", "")) >= 800) {
        el.querySelectorAll(".read-more-button").item(0).style.display = 'initial';
      }
      else {
        // Hide
        el.querySelectorAll(".read-more-button").item(0).style.display = 'none';
      }
    }
  });
}, true);

async function LoadTopGroupsAsync() {
  await fetch("/Home/TopGroups").then(response => {
    return response.text();
  }).then(html => {
    var groupsBoxEl = document.getElementById("group-box");
    if (groupsBoxEl != null) {
      groupsBoxEl.innerHTML = html;
    }
  })
}
LoadTopGroupsAsync();

getMicroCharts().then(({ createPieChart }) => {
  function getCanvas(id) {
    return document.getElementById(id);
  }
  var payoutDate = new Date();
  payoutDate.setUTCHours(24, 0, 0, 0); //next midnight
  var timer;
  function getTimeString() {
    var now = new Date().getTime();
    var distance = payoutDate - now;
    var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    var seconds = Math.floor((distance % (1000 * 60)) / 1000);
    var timeStr = hours.toString().padStart(2, '0')
      + ":" + minutes.toString().padStart(2, '0')
      + ":" + seconds.toString().padStart(2, '0');
    return timeStr;
  }
  async function showCommunityPayoutTimer() {
    await getJson("/Home/GetPayoutInfo/")
      .then(response => {
        if (response.success) {
          var amountEl = document.getElementById("amount-info-payout");
          amountEl.innerHTML = response.community;
          var now = new Date().getTime();
          var distance = payoutDate - now;
          var percent = 100 * distance / (1000 * 60 * 60 * 24);
          createPieChart(getCanvas('pc-community'),
            [
              { id: '1', percent: 100 - percent, color: '#FFFFFFFF' }, // green '#4CAF50'
              { id: '2', percent: percent, color: '#1ab39455' }  // red '#E91E63'
            ],
            { size: 25 });
        }
      });

    var timerEl = document.getElementById("timer-info-payout");
    timerEl.innerHTML = getTimeString();
    // Update every second
    timer = setInterval(function () {
      var timerEl = document.getElementById("timer-info-payout");
      timerEl.innerHTML = getTimeString();
    }, 1000);
  }

  showCommunityPayoutTimer();

  getTippy().then(({ default: tippy }) => {
    var infoEl = document.getElementById("hover-info-payout");
    tippy(infoEl, {
      content: '<div class="text-center" style="margin-top:5px;"><strong>Community Payout Timer!</strong></div><hr>This community payout is distributed daily to the highest scoring posts over the last 30 days.  Your votes increase this payout.',
      theme: 'light-border',
      allowHTML: true,
      delay: 300,
      interactive: true,
      interactiveBorder: 30,
    });
  })
})