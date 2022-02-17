/**
 * 
 * [✓] native JS
 */

import { postJson } from '../utility/postData';                     // [✓]

export function loadMoreComments(e) {
    postJson("/Comment/LoadMoreComments/", {
        postId: e.getAttribute("data-postid"),// $(e).data('postid'),
        nestLevel: e.getAttribute("data-nest"),//$(e).data('nest'),
        rootshown: e.getAttribute("data-shown"),// $(e).data('shown')
    })
    .then((response) => {
        if (response.success) {
            e.setAttribute("data-shown", response.shown); //$(e).data('shown', response.shown); // update data
            if (!response.hasMore) {
                e.style.display = 'none';////$(e).hide();
            }
            //$(e).parent().find('.insertComments').append(response.HTMLString); // Inject
            e.parentElement.querySelectorAll(".insertComments").item(0).innerHTML += response.HTMLString;
        }
        else {
            alert(response.message);
        }
    })
    .catch((error) => {
        console.log('load more error ' + error);
    });
    //var msg = JSON.stringify({ 'postId': $(e).data('postid'), 'nestLevel': $(e).data('nest'), 'rootshown': $(e).data('shown') });
    //$.ajax({
    //    type: "POST",
    //    url: "/Comment/LoadMoreComments",
    //    data: msg,
    //    contentType: "application/json; charset=utf-8",
    //    dataType: "json",
    //    success: function (response) {
            
    //    },
    //    failure: function (response) {
    //        console.log('load more failure');
    //    },
    //    error: function (response) {
    //        console.log('load more error');
    //    }
    //});
    return false;
}