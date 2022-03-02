/**
 * 
 **/

import '../../shared/shared';                                               // [✓]
import '../../utility/ui/vote';                                             // [✓]
import '../../realtime/signalr';                                            // [✓]
import Swal from 'sweetalert2';                                             // [✓]
import { addposts, loadmore } from '../../utility/loadmore';                // [✓]
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';        // [✓]
import { writeComment } from '../../comment/writecomment';                  // [✓]
import { replyComment } from '../../comment/replycomment';                  // [✓]
import { editComment } from '../../comment/editcomment';                    // [✓]
import { loadMoreComments } from '../../comment/loadmorecomments';          // [✓]
import { createPieChart } from "micro-charts";
import { ready } from "../../utility/ready";
import { getJson } from "../../utility/getData";
/*
 * This is the main landing page for ZapRead
 */

import '../../css/Site.css';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';
import '../../css/posts.css'

import tippy from 'tippy.js';
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;
window.loadMoreComments = loadMoreComments;
window.loadmore = loadmore;

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

async function LoadTopPostsAsync() {
  var request = new XMLHttpRequest();
  request.open('GET', '/Home/TopPosts/?sort=' + postSort, true);
  request.onload = function () {
    var resp = this.response;
    var response = {};
    if (this.status >= 200 && this.status < 400) {
      // Success!
      response = JSON.parse(resp);
      if (response.success) {
        // Insert posts
        document.querySelectorAll('#posts').item(0).querySelectorAll('.ibox-content').item(0).classList.remove("sk-loading");
        addposts(response, onLoadedMorePosts);
        document.querySelectorAll('#btnLoadmore').item(0).style.display = '';
      } else {
        // Did not work
        Swal.fire("Error", "Error loading posts: " + response.message, "error");
      }
    } else {
      response = JSON.parse(resp);
      // We reached our target server, but it returned an error
      Swal.fire("Error", "Error loading posts (status ok): " + response.message, "error");
    }
  };
  request.onerror = function () {
    // There was a connection error of some sort
    var response = JSON.parse(this.response);
    Swal.fire("Error", "Error requesting posts: " + response.message, "error");
  };
  request.send();
}

async function LoadTopGroupsAsync() {
  await fetch("/Home/TopGroups").then(response => {
    return response.text();
  }).then(html => {
    var groupsBoxEl = document.getElementById("group-box");
    groupsBoxEl.innerHTML = html;
  })
}

LoadTopPostsAsync();
LoadTopGroupsAsync();

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
            { id: '1', percent: 100-percent, color: '#FFFFFFFF' }, // green '#4CAF50'
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
    //if (distance < 0) {
    //  clearInterval(timer);
    //  console.log("payout!!");
    //}
  }, 1000);
}

// Show community payout timer
showCommunityPayoutTimer();

// Apply the loading tippy
var infoEl = document.getElementById("hover-info-payout");
tippy(infoEl, {
  content: '<div class="text-center" style="margin-top:5px;"><strong>Community Payout Timer!</strong></div><hr>This community payout is distributed daily to the highest scoring posts over the last 30 days.  Your votes increase this payout.',
  theme: 'light-border',
  allowHTML: true,
  delay: 300,
  interactive: true,
  interactiveBorder: 30,
});