/**
 * 
 **/
import '../../shared/shared';
import '../../realtime/signalr';

import Swal from 'sweetalert2';
import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { loadachhover } from '../../utility/achievementhover';
import { loadmore } from '../../utility/loadmore';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;
window.loadMoreComments = loadMoreComments;
window.loadachhover = loadachhover;

// Wrapper for load more
export function userloadmore() {
    loadmore({
        url: '/User/InfiniteScroll/',
        blocknumber: BlockNumber,
        sort: "New",
        userId: userId
    });
}
window.userloadmore = userloadmore;

// 
onLoadedMorePosts();

//Dropzone.options.dropzoneForm = {
//    paramName: "file", // The name that will be used to transfer the file
//    maxFilesize: 5, // MB
//    acceptedFiles: "image/*",
//    maxFiles: 1,
//    addRemoveLinks: true,
//    init: function () {
//        this.on("addedfile", function () {
//            if (this.files[1] !== null) {
//                this.removeFile(this.files[0]);
//            }
//        });
//    },
//    dictDefaultMessage: "<strong>Drop files here or click to upload. </strong>"
//};

export function toggleUserIgnore(id) {
    joinurl = "/User/ToggleIgnore/";
    var data = JSON.stringify({ 'id': id });
    $.ajax({
        data: data.toString(),
        type: 'POST',
        url: joinurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
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
window.toggleUserIgnore = toggleUserIgnore;
window.BlockNumber = 10;                        // Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;