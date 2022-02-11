/**
 * 
 */

import Swal from 'sweetalert2';
import { postJson } from '../utility/postData';

/**
 * User clicks to sticky a post
 * 
 * 
 * @param {any} id
 */
export function stickyPost(id) {
  postJson("/Post/ToggleStickyPost/", { "id": id })
    .then((result) => {
      if (result.Result === "Success") {
        alert("Post successfully toggled Sticky.");
      }
    });
}
window.stickyPost = stickyPost;

/**
 * Toggle if a post is NSFW
 * 
 * 
 * @param {any} id
 */
export function nsfwPost(id) {
  postJson("/Post/ToggleNSFW/", { "id": id })
    .then((result) => {
      if (result.success) {
        var message = "Successfully removed NSFW flag from post.";
        if (result.IsNSFW) {
          message = "Successfully marked post NSFW.";
        }
        Swal.fire(message, {
          icon: "success"
        });
      }
    });
}
window.nsfwPost = nsfwPost;

/**
 * 
 * 
 * @param {any} id
 */
export function showNSFW(id) {
  document.getElementById("nsfw_" + id).style.display = "none"; // $("#nsfw_" + id).hide();
  document.getElementById("nsfwb_" + id).style.display = "none"; // $("#nsfwb_" + id).hide();
}
window.showNSFW = showNSFW;

/**
 * 
 * [✓] does not use jQuery
 * 
 * @param {any} id
 */
export function deleteComment(id) {
  Swal.fire({
    title: "Are you sure?",
    text: "Once deleted, you will not be able to recover this comment!",
    icon: "warning",
    showCancelButton: true
  }).then(function (willDelete) {
    if (willDelete.value) {
      postJson("/Comment/DeleteComment/", { "Id": id })
        .then((data) => {
          if (data.Success) {
            $('#comment_' + id.toString()).hide();
            Swal.fire("Deleted! Your comment has been deleted.", {
              icon: "success"
            });
          }
          else {
            Swal.fire("Error", "Error deleting comment.", "error");
          }
        });
    } else {
      console.log("cancelled delete");
    }
  });
}
window.deleteComment = deleteComment;

/**
 * 
 * 
 * @param {any} id
 */
export function setPostLanguage(id) {
  Swal.fire({
    text: 'Enter new language code',
    input: 'text',
    inputValue: '',
    showCancelButton: true
  }).then(function (name) {
    if (!name.value) throw null;
    postJson("/Post/ChangeLanguage/", { "postId": id, "newLanguage": name.value })
      .then((data) => {
        if (data.success) {
          Swal.fire("Post language has been updated!", {
            icon: "success"
          });
        }
        else {
          Swal.fire("Error", "Error: " + data.message, "error");
        }
      });
  }).catch(function (err) {
    if (err) {
      Swal.fire("Error", "Error updating language.", "error");
    } else {
      Swal.stopLoading();
      Swal.close();
    }
  });
}
window.setPostLanguage = setPostLanguage;

/**
 * 
 * 
 * @param {any} id
 */
export function deletePost(id) {
  Swal.fire({
    title: "Are you sure?",
    text: "Once deleted, you will not be able to recover this post!",
    icon: "warning",
    showCancelButton: true
  }).then(function (willDelete) {
    if (willDelete.value) {
      postJson("/Post/DeletePost/", { "PostId": id })
        .then((data) => {
          if (data.Success) {
            if (document.querySelectorAll('#post_' + id.toString()).length) {
              document.querySelectorAll('#post_' + id.toString()).item(0).style.display = 'none';
            }
            Swal.fire({
              title: "Deleted",
              text: "Deleted! Your post has been deleted.",
              icon: "success"
            });
          }
          else {
            Swal.fire("Error", "Error deleting post.", "error");
          }
        });
    } else {
      console.log("cancelled delete");
    }
  });
}
window.deletePost = deletePost;

// For submitting comments (TODO: move this to own file)
/* exported isCommenting */
var isCommenting = false;

/**
 * 
 * 
 **/
export function dofeedback() {
  var msg = document.getElementById('feedbackText').value; // $('#feedbackText').val();
  var feebackLocation = window.location.href;

  postJson("/Home/SendFeedback/", { msg: msg, loc: feebackLocation })
    .then((data) => {
      alert('Feedback successfully sent.  Thank you!');
    });
}
window.dofeedback = dofeedback;

