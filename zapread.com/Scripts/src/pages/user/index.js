﻿/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';
import '../../../summernote/dist/summernote-bs4';
import 'summernote/dist/summernote-bs4.css';
import '../../utility/summernote/summernote-video-attributes';

import Swal from 'sweetalert2';
import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { loadachhover } from '../../utility/achievementhover';
import { loadmore } from '../../utility/loadmore';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
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

//$(document).ready(function () {
//    // This loads all partial views on page
//    $(".partialContents").each(function (index, item) {
//        var url = $(item).data("url");
//        if (url && url.length > 0) {
//            $(item).load(url);
//        }
//    });

//    //// This formats the timestamps on the page
//    //$('.eventTime').each(function (i, e) {
//    //    var datefn = dateFns.parse($(e).html());
//    //    // Adjust to local time
//    //    datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
//    //    var date = dateFns.format(datefn, "DD MMM YYYY");
//    //    var time = dateFns.distanceInWordsToNow(datefn);
//    //    $(e).html('<span>' + time + ' ago - ' + date + '</span>');
//    //});
//});

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

window.BlockNumber = 10;  //Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

//var userloadmore = function () {
//    if (!inProgress) {
//        inProgress = true;
//        $('#loadmore').show();
//        $('#btnLoadmore').prop('disabled', true);
//        //var userId = ;
//        $.post("/User/InfiniteScroll/",//@Url.Action("InfiniteScroll", "User")",
//            { "BlockNumber": BlockNumber, "userId": userId },
//            function (data) {
//                $('#loadmore').hide();
//                $('#btnLoadmore').prop('disabled', false);
//                BlockNumber = BlockNumber + 10;
//                NoMoreData = data.NoMoreData;
//                $("#posts").append(data.HTMLString);
//                inProgress = false;
//                $('.postTime').each(function (i, e) {
//                    var datefn = dateFns.parse($(e).html());
//                    // Adjust to local time
//                    datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
//                    var date = dateFns.format(datefn, "DD MMM YYYY");
//                    var time = dateFns.distanceInWordsToNow(datefn);
//                    $(e).html('<span>' + time + ' ago - ' + date + '</span>');
//                    $(e).css('display', 'inline');
//                    $(e).removeClass("postTime");
//                });
//                if (NoMoreData) {
//                    $('#showmore').hide();
//                }
//                $(".impression").each(function (ix, e) {
//                    $(e).load($(e).data("url"));
//                    $(e).removeClass("impression");
//                });
//                $(".sharing").each(function () {
//                    $(this).jsSocials({
//                        url: $(this).data('url'),
//                        text: $(this).data('sharetext'),
//                        showLabel: false,
//                        showCount: false,
//                        shareIn: "popup",
//                        shares: ["email", "twitter", "facebook", "googleplus", "linkedin", "pinterest", "whatsapp"]
//                    });
//                    $(this).removeClass("sharing");
//                });
//            });
//    }
//};