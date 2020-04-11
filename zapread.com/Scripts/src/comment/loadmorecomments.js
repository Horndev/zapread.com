/*
 * 
 */
// TODO - remove requirement
import $ from 'jquery';

export function loadMoreComments(e) {
    var msg = JSON.stringify({ 'postId': $(e).data('postid'), 'nestLevel': $(e).data('nest'), 'rootshown': $(e).data('shown') });
    $.ajax({
        type: "POST",
        url: "/Comment/LoadMoreComments",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                $(e).data('shown', response.shown); // update data
                if (!response.hasMore) {
                    $(e).hide();
                }
                $(e).parent().find('.insertComments').append(response.HTMLString); // Inject
            }
            else {
                alert(response.Message);
            }
        },
        failure: function (response) {
            console.log('load more failure');
        },
        error: function (response) {
            console.log('load more error');
        }
    });
    return false;
}