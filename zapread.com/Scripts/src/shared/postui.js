/**
 * Post interaction user interface
 * 
 * [✓] Does not use jQuery
 */

import { toggleHidden } from '../utility/toolbox';
import { postJson } from "../utility/postData";
const getSwal = () => import('sweetalert2');

function findAncestor(el, sel) {
  while ((el = el.parentElement) && !((el.matches || el.matchesSelector).call(el, sel)));
  return el;
}

export function togglePostFollow(e) {
  let isFollowing = e.getAttribute("data-follow") == "1";
  let id = e.getAttribute("data-postid");
  var url = isFollowing ? "/api/v1/post/unfollow/" : "/api/v1/post/follow/";
  postJson(url, {
    PostId: id
  }).then((response) => {
    if (response.success) {
      if (!isFollowing) {
        e.innerHTML = "<i class='fa-regular fa-bell-slash'></i> Stop following";
        e.setAttribute("data-follow", "1");
      }
      else {
        e.innerHTML = "<i class='fa-regular fa-bell'></i> Follow post";
        e.setAttribute("data-follow", "0");
      }
    }
  });
  return false;
}

export function postIgnore(e) {
  let id = e.getAttribute("data-postid");
  var url = "/api/v1/post/ignore/";

  getSwal().then(({ default: Swal }) => {
    Swal.fire({
      title: "Are you sure?",
      text: "Once ignored, you will not see this post.",
      icon: "warning",
      showCancelButton: true
    }).then(function (willIgnore) {
      if (willIgnore.value) {
        postJson(url, {
          PostId: id
        }).then((response) => {
          if (response.success) {
            // hide post
            var postel = document.getElementById("post_" + id.toString());
            postel.style.display = "none";
          }
        });
      } else {
        console.log("cancelled ignore");
      }
    });
  });

  return false;
}

/**
 * Toggle the comment and it's children
 * 
 * [✓] Does not use jQuery
 * 
 * @param {any} e element of close button
 * @param {any} d direction force (1 force close, -1 force open, 0 toggle)
 */
export function toggleComment(e, d) {
  //console.log("toggleComment e:", e);
  var commentBody = e.parentElement.querySelectorAll(".comment-body").item(0);
  //var commentBody = findAncestor(e, ".comment-body");
  //console.log("e.parentElement:", e.parentElement);
  //console.log("toggleComment commentBody:", commentBody);

  toggleHidden(commentBody);  //$(e).parent().find('.comment-body').first().fadeToggle({ duration: 0 });
  var toggleButton = e.querySelectorAll(".togglebutton").item(0);
  if (d === 1 || (d === 0 && toggleButton.classList.contains("fa-minus-square"))) {
    e.classList.remove("pull-left"); //$(e).removeClass('pull-left');
    e.classList.add("commentCollapsed");//$(e).addClass('commentCollapsed');
    toggleButton.classList.remove("fa-minus-square");//$(e).find('.togglebutton').removeClass("fa-minus-square");
    toggleButton.classList.add("fa-plus-square");//$(e).find('.togglebutton').addClass("fa-plus-square");
    e.querySelectorAll("#cel").item(0).style.display = '';//$(e).find('#cel').show();
  }
  else if (d === -1 || (d === 0 && toggleButton.classList.contains("fa-plus-square"))) {
    e.classList.add("pull-left");//$(e).addClass('pull-left');
    e.classList.remove("commentCollapsed");//$(e).removeClass('commentCollapsed');
    toggleButton.classList.remove("fa-plus-square");//$(e).find('.togglebutton').removeClass("fa-plus-square");
    toggleButton.classList.add("fa-minus-square");//$(e).find('.togglebutton').addClass("fa-minus-square");
    e.querySelectorAll("#cel").item(0).style.display = 'none';//$(e).find('#cel').hide();
  }
}
window.toggleComment = toggleComment;

/**
 * Toggle the visibility of a post + comments
 * 
 * [✓] Does not use jQuery
 * 
 * @param {any} e
 */
export function togglePost(e) {
  var socialBody = e.parentElement.querySelectorAll(".social-body").item(0);
  toggleHidden(socialBody);
  var toggleButton = e.querySelectorAll(".togglebutton").item(0);
  var commentToggle = e.parentElement.querySelectorAll(".social-comment-box").item(0).querySelectorAll(".comment-toggle").item(0);
  if (toggleButton.classList.contains("fa-minus-square")) {
    toggleComment(commentToggle, 1);
    toggleButton.classList.remove("fa-minus-square");
    toggleButton.classList.add("fa-plus-square");
  }
  else {
    toggleComment(commentToggle, -1);
    toggleButton.classList.remove("fa-plus-square");
    toggleButton.classList.add("fa-minus-square");
  }
}
window.togglePost = togglePost;
