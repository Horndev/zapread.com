/**
 * Post interaction user interface
 * 
 * [✓] Does not use jQuery
 */

import { toggleHidden } from '../utility/toolbox';

function findAncestor(el, sel) {
  while ((el = el.parentElement) && !((el.matches || el.matchesSelector).call(el, sel)));
  return el;
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
