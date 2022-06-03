/**
 *
 * [✓] Native JS
 */
import React from "react";
import ReactDOM from "react-dom";
import { Dropdown } from 'bootstrap.native/dist/bootstrap-native-v4';
import { applyHoverToChildren } from './userhover';                             // [✓]
import { loadgrouphover } from './grouphover';                                  // [✓]
import { updatePostTimes } from './datetime/posttime';                          // [✓]
import { makePostsQuotable, makeCommentsQuotable } from './quotable/quotable';  // [✓]
import { togglePostFollow, postIgnore } from "../shared/postui";
const getSwal = () => import('sweetalert2');
import ReactionBar from '../Components/ReactionBar';
import SharePostButton from '../Components/Share/SharePostButton';

function findAncestor(el, sel) {
  while ((el = el.parentElement) && !((el.matches || el.matchesSelector).call(el, sel)));
  return el;
}

export async function addPostFollowClickHandler() {
  var elements = document.querySelectorAll(".btnFollowPost");
  Array.prototype.forEach.call(elements, function (el, _i) {
    el.addEventListener("click", (e) => {
      togglePostFollow(el);
    });
    el.classList.remove('btnFollowPost');
  });
}

export async function addPostIgnoreClickHandler() {
  var elements = document.querySelectorAll(".btnIgnorePost");
  Array.prototype.forEach.call(elements, function (el, _i) {
    el.addEventListener("click", (e) => {
      postIgnore(el);
    });
    el.classList.remove('btnIgnorePost');
  });
}

export async function enableVoting(className, d, t, idel, container) {
  if (!container) {
    container = document;
  }
  var elements = container.querySelectorAll("." + className);
  Array.prototype.forEach.call(elements, function (el, _i) {
    var postid = el.getAttribute(idel);
    var isBanished = el.getAttribute("data-isbanished");
    el.addEventListener("click", (e) => {
      // Emit vote event
      if (isBanished == "True" && d == "down") {
        // Note that we compare against the string True since the data element is not a bool
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "You are banished from this group and can't vote down", "error");
        });
      } else {
        //console.log(el);
        if (!el.children[0].classList.contains('fa-spin')) {
          const event = new CustomEvent('vote', {
            detail: {
              direction: d,
              type: t,
              id: postid,
              target: e.target,
              userInfo: window.userInfo
            }
          });
          document.dispatchEvent(event);
        } else {
          console.log('already voting');
        }
      }
    });
    // remove className to indicate event handler is attached
    el.classList.remove(className);
  });
}
/* Does the vote/comment up/down handler attachment in parallel */
async function enableVotingAsync() {
  await Promise.all([
    enableVoting("vote-post-up", 'up', 'post', 'data-postid'),
    enableVoting("vote-post-dn", 'down', 'post', 'data-postid'),
    enableVoting("vote-comment-up", 'up', 'comment', 'data-commentid'),
    enableVoting("vote-comment-dn", 'down', 'comment', 'data-commentid')
  ]);
}

/**
 * 
 * [✓] native JS
 **/
export function onLoadedMorePosts() {
  enableVotingAsync(); // Done in parallel
  //console.log('[DEBUG] onLoadedMorePosts');
  // User mention hover
  applyHoverToChildren(document, ".userhint");

  var elements = document.querySelectorAll(".grouphint");
  Array.prototype.forEach.call(elements, function (el, _i) {
    loadgrouphover(el);
    el.classList.remove('grouphint');
  });

  // Render reactions UI
  elements = document.querySelectorAll(".post-reaction-bar");
  Array.prototype.forEach.call(elements, function (el, _i) {
    //console.log(el);
    var postId = el.getAttribute("data-postid");
    var l = el.getAttribute("data-l");
    ReactDOM.render(<ReactionBar l={l} postId={postId}/>, el);
  });

  // Render share button UI
  elements = document.querySelectorAll(".post-social-button");
  Array.prototype.forEach.call(elements, function (el, _i) {
    var postId = el.getAttribute("data-postid");
    var sfb = findAncestor(el, ".social-feed-box");
    var title = sfb.querySelectorAll(".vote-title").item(0).innerHTML;
    var url = "https://www.zapread.com" + sfb.querySelectorAll(".vote-title").item(0).getAttribute("href");
    ReactDOM.render(<SharePostButton postId={postId} title={title} url={url}/>, el);
  });

  // activate dropdown (done manually using bootstrap.native)
  elements = document.querySelectorAll(".dropdown-toggle");
  Array.prototype.forEach.call(elements, function (el, _i) {
    if (el.id != 'input-group-dropdown-search') { // This is because this one is managed by React not bsn
      var dropdownInit = new Dropdown(el);
    }
  });

  // show the read more
  setTimeout(() => {
    var postElements = document.querySelectorAll(".post-box");
    Array.prototype.forEach.call(postElements, function (el, _i) {
      var elHeight = parseFloat(getComputedStyle(el, null).height.replace("px", ""));
      if (elHeight >= 800) {
        el.querySelectorAll(".read-more-button").item(0).style.display = 'initial';
        el.style.overflowY = "hidden";
      } else {
        el.style.overflowY = "visible";
      }
    });
  }, 3000); // Timer is a quickfix, need to make a more event-driven solution which works once post size is known.  This could fail still if loading images is slow.

  //var postElements = document.querySelectorAll(".post-box");
  //Array.prototype.forEach.call(postElements, function (el, _i) {
  //  var loadscript = document.createElement('script');
  //  loadscript.type = 'text/javascript';
  //  var code = 'console.log("!");';
  //  loadscript.appendChild(document.createTextNode(code));
  //  el.appendChild(loadscript);
  //});

  //var loadedCallback = function (mutationsList) {
  //  for (var mutation of mutationsList) {
  //    if (mutation.type == "attributes") {
  //      console.log(mutation);
  //      console.log('The ' + mutation.attributeName + ' attribute was modified.');
  //      if (targetNode.style.display == "block") {
  //        document.getElementById("textToHide").style.display = "none";
  //      }
  //    }
  //  }
  //}

  //var observer = new MutationObserver(loadedCallback);

  // --- update impressions counts
  var impressionObserver = new IntersectionObserver(function (entries) {
    // since there is a single target to be observed, there will be only one entry
    if (entries[0]['isIntersecting'] === true) {
      var el = entries[0]['target'];
      var impressionEl = el.parentElement.querySelector(".impression");
      if (impressionEl != null) {
        var url = impressionEl.getAttribute('data-url');
        //console.log('obs', url, el);
        fetch(url).then(function (response) {
          return response.text();
        }).then(function (html) {
          impressionEl.innerHTML = html;
          impressionEl.classList.remove('impression');
        });
      }
    }
  }, { threshold: [0.1] });

  elements = document.querySelectorAll(".post-observe");
  Array.prototype.forEach.call(elements, function (el, _i) {
    impressionObserver.observe(el);
    el.classList.remove('post-observe');
  });

  addPostFollowClickHandler();
  addPostIgnoreClickHandler();

  // --- relative times
  updatePostTimes();
  // ---

  // --- socials buttons
  // TODO: implement using non-jquery library
  elements = document.querySelectorAll(".sharing");
  Array.prototype.forEach.call(elements, function (el, _i) {
    var url = el.getAttribute('data-url');
    var sharetext = el.getAttribute('data-sharetext');
    //el.jsSocials({
    //    url: url,
    //    text: sharetext,
    //    showLabel: false,
    //    showCount: false,
    //    shareIn: "popup",
    //    shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
    //});
    el.classList.remove('sharing');
  });

  elements = document.querySelectorAll(".pop");
  Array.prototype.forEach.call(elements, function (el, _i) {
    el.classList.remove('pop');
  });

  // Make post quotable
  makePostsQuotable();

  // Make comments quotable
  makeCommentsQuotable();

  try {
    (tarteaucitron.job = tarteaucitron.job || []).push('zyoutube');
    (tarteaucitron.job = tarteaucitron.job || []).push('zapreadARRA');

    tarteaucitron.init({
      "privacyUrl": "https://www.zapread.com/Home/About/", /* Privacy policy url */
      "hashtag": "#tarteaucitron", /* Open the panel with this hashtag */
      "cookieName": "tarteaucitron", /* Cookie name */
      "orientation": "bottom", /* Banner position (top - bottom - middle - popup) */
      "groupServices": false, /* Group services by category */
      "showAlertSmall": false, /* Show the small banner on bottom right */
      "cookieslist": false, /* Show the cookie list */
      "showIcon": true, /* Show cookie icon to manage cookies */
      "iconSrc": "/Content/privacy.png", /* Optionnal: URL or base64 encoded image */
      "iconPosition": "BottomRight", /* Position of the icon between BottomRight, BottomLeft, TopRight and TopLeft */
      "adblocker": false, /* Show a Warning if an adblocker is detected */
      "DenyAllCta": true, /* Show the deny all button */
      "AcceptAllCta": true, /* Show the accept all button when highPrivacy on */
      "highPrivacy": true, /* HIGHLY RECOMMANDED Disable auto consent */
      "handleBrowserDNTRequest": false, /* If Do Not Track == 1, disallow all */
      "removeCredit": true, /* Remove credit link */
      "moreInfoLink": false, /* Show more info link */
      "useExternalCss": true, /* If false, the tarteaucitron.css file will be loaded */
      //"cookieDomain": ".zapread.com", /* Shared cookie for subdomain website */
      "readmoreLink": "", /* Change the default readmore link pointing to tarteaucitron.io */
      "mandatory": true /* Show a message about mandatory cookies */
    });
  } catch (err) {
    console.log(err);
  }

}