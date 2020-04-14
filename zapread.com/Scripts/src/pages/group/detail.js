/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';
import '../../../summernote/dist/summernote-bs4';
import 'summernote/dist/summernote-bs4.css';
import '../../utility/summernote/summernote-video-attributes';
import Swal from 'sweetalert2';
import 'selectize/dist/js/standalone/selectize';
import 'selectize/dist/css/selectize.css';
import 'selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { loadmore } from '../../utility/loadmore';
import './userroles';
import './tags';
import './adminbar';
import './editicon';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.loadMoreComments = loadMoreComments;

export function grouploadmore() {
    loadmore({
        url: '/Group/InfiniteScroll/',
        blocknumber: BlockNumber,
        sort: "New"
    });
}
window.loadmore = grouploadmore;

onLoadedMorePosts();

export function toggleIgnore(id) {
    var data = JSON.stringify({ 'groupId': id });
    var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: "/Group/ToggleIgnore/",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: headers,
        success: function (response) {
            if (response.result === "success") {
                if (response.added) {
                    $("#i_" + id.toString()).html("<i class='fa fa-circle'></i> Un-Ignore ");
                }
                else {
                    $("#i_" + id.toString()).html("<i class='fa fa-ban'></i> Ignore ");
                }
            }
        }
    });
    return false;
}
window.toggleIgnore = toggleIgnore;

/* Infinite scroll */
var BlockNumber = 10;
var NoMoreData = false;
var inProgress = false;

//export function loadmore() {
//    if (!inProgress) {
//        inProgress = true;
//        $('#loadmore').show();
//        $('#btnLoadmore').prop('disabled', true);
//        $.ajax({
//            async: true,
//            data: JSON.stringify({ "id": groupId, "BlockNumber": BlockNumber, "sort": "New" }),
//            type: 'POST',
//            url: "/Group/InfiniteScroll/",
//            contentType: "application/json; charset=utf-8",
//            dataType: "json",
//            headers: getAntiForgeryToken(),
//            success: function (response) {
//                if (response.Success) {
//                    $('#loadmore').hide();
//                    $('#btnLoadmore').prop('disabled', false);
//                    BlockNumber = BlockNumber + 10;
//                    NoMoreData = response.NoMoreData;
//                    $("#posts").append(response.HTMLString);
//                    inProgress = false;

//                    // Wait for new posts to be added then tidy up.

//                    // New version using a callback
//                    //addposts(response, zrOnLoadedMorePosts);
//                    $("#posts").append(response.HTMLString);
//                    zrOnLoadedMorePosts();

//                    // old version with jquery
//                    //$.when(addposts(response), $.ready).then(function () {
//                    //    zrOnLoadedMorePosts();
//                    //});

//                    if (NoMoreData) {
//                        $('#showmore').hide();
//                    }
//                }
//            }
//        });
//    }
//}
//window.loadmore = loadmore;