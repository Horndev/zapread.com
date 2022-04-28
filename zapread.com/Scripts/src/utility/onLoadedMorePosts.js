/**
 *
 * [✓] Native JS
 */
import { Dropdown } from 'bootstrap.native/dist/bootstrap-native-v4';
import { applyHoverToChildren } from './userhover';                             // [✓]
import { loadgrouphover } from './grouphover';                                  // [✓]
import { updatePostTimes } from './datetime/posttime';                          // [✓]
import { makePostsQuotable, makeCommentsQuotable } from './quotable/quotable';  // [✓]
const getSwal = () => import('sweetalert2');

export async function enableVoting(className, d, t, idel) {
  var elements = document.querySelectorAll("." + className);
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
        const event = new CustomEvent('vote', {
          detail: {
            direction: d,
            type: t,
            id: postid,
            target: e.target
          }
        });
        document.dispatchEvent(event);
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
  //var elements = document.querySelectorAll(".userhint");
  //Array.prototype.forEach.call(elements, function (el, _i) {
  //    loaduserhover(el);
  //    el.classList.remove('userhint');
  //});

  var elements = document.querySelectorAll(".grouphint");
  Array.prototype.forEach.call(elements, function (el, _i) {
    loadgrouphover(el);
    el.classList.remove('grouphint');
  });

  // activate dropdown (done manually using bootstrap.native)
  elements = document.querySelectorAll(".dropdown-toggle");
  Array.prototype.forEach.call(elements, function (el, _i) {
    if (el.id != 'input-group-dropdown-search') { // This is because this one is managed by React not bsn
      var dropdownInit = new Dropdown(el);
    }
  });

  // show the read more
  elements = document.querySelectorAll(".post-box");
  Array.prototype.forEach.call(elements, function (el, _i) {
    if (parseFloat(getComputedStyle(el, null).height.replace("px", "")) >= 800) {
      el.querySelectorAll(".read-more-button").item(0).style.display = 'initial';
    }
  });

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
  //Array.prototype.forEach.call(elements, function (el, _i) {
  //    var url = el.getAttribute('data-url');
  //    fetch(url).then(function (response) {
  //        return response.text();
  //    }).then(function (html) {
  //        el.innerHTML = html;
  //        el.classList.remove('impression');
  //    });
  //});

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