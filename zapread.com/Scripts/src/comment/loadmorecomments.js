/**
 * 
 * [✓] native JS
 */
import { Dropdown } from 'bootstrap.native/dist/bootstrap-native-v4';
import { postJson } from '../utility/postData';
import { enableVoting } from '../utility/onLoadedMorePosts';
import { applyHoverToChildren } from '../utility/userhover';
import { loadgrouphover } from '../utility/grouphover';
import { makeCommentsQuotable } from '../utility/quotable/quotable';
import { updatePostTimesOnEl } from '../utility/datetime/posttime';

export function loadMoreComments(e) {
  postJson("/Comment/LoadMoreComments/", {
    postId: e.getAttribute("data-postid"),
    nestLevel: e.getAttribute("data-nest"),
    rootshown: e.getAttribute("data-shown"),
  })
    .then((response) => {
      if (response.success) {
        e.setAttribute("data-shown", response.shown); // update data
        if (!response.hasMore) {
          e.style.display = 'none';
        }
        e.parentElement.querySelectorAll(".insertComments").item(0).innerHTML += response.HTMLString;

        enableVoting("vote-comment-up", 'up', 'comment', 'data-commentid');
        enableVoting("vote-comment-dn", 'down', 'comment', 'data-commentid');

        var newPostEl = e.parentElement.querySelectorAll(".insertComments").item(0);

        updatePostTimesOnEl(newPostEl);

        applyHoverToChildren(newPostEl, ".userhint");

        var elements = newPostEl.querySelectorAll(".grouphint");
        Array.prototype.forEach.call(elements, function (el, _i) {
          loadgrouphover(el);
          el.classList.remove('grouphint');
        });

        // activate dropdown (done manually using bootstrap.native)
        elements = newPostEl.querySelectorAll(".dropdown-toggle");
        Array.prototype.forEach.call(elements, function (el, _i) {
          if (el.id != 'input-group-dropdown-search') { // This is because this one is managed by React not bsn
            var dropdownInit = new Dropdown(el);
          }
        });

        // Make comments quotable
        makeCommentsQuotable();

      }
      else {
        alert(response.message);
      }
    })
    .catch((error) => {
      console.log('load more error ' + error);
    });
  return false;
}