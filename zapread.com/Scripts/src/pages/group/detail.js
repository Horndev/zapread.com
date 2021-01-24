/*
 * 
 */
import $ from 'jquery';

import '../../shared/shared';
import '../../utility/ui/vote';
import '../../realtime/signalr';
//import '../../../summernote/dist/summernote-bs4';
//import 'summernote/dist/summernote-bs4.css';
//import '../../utility/summernote/summernote-video-attributes';
import Swal from 'sweetalert2';
import 'selectize/dist/js/standalone/selectize';
import 'selectize/dist/css/selectize.css';
import 'selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { loadmore } from '../../utility/loadmore';

import '../../shared/postfunctions';                                        // [✓]
import '../../shared/readmore';                                             // [✓]
import '../../shared/postui';                                               // [✓]

import './userroles';
import './tags';
import './adminbar';
import './editicon';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;
window.loadMoreComments = loadMoreComments;

export function grouploadmore(groupId) {
    console.log("Loading more ", groupId)
    loadmore({
        url: '/Group/InfiniteScroll/',
        blocknumber: window.BlockNumber,
        sort: "Score",
        groupId: groupId
    });
}
window.loadmore = grouploadmore;
window.BlockNumber = 10;  //Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

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