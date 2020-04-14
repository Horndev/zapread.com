﻿/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';
import '../../../summernote/dist/summernote-bs4';
import 'summernote/dist/summernote-bs4.css';
import '../../utility/summernote/summernote-video-attributes';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { updatePostTimes } from '../../utility/datetime/posttime';
import '../../shared/sharedlast';

import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';

window.subMinutes = subMinutes;
window.format = format;
window.parseISO = parseISO;
window.formatDistanceToNow = formatDistanceToNow;

updatePostTimes();

$(document).ready(function () {
    $(".m_input").summernote({
        callbacks: {
            onImageUpload: function (files) {
                that = $(this);
                sendFile(files[0], that);
            }
        },
        focus: false,
        placeholder: 'Write message...',
        disableDragAndDrop: false,
        dialogsInBody: true,
        toolbar: ['bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],
        minHeight: 100,
        maxHeight: 600,
        hint: {
            match: /\B@@(\w*)$/,
            search: function (keyword, callback) {
                if (!keyword.length) return callback();
                var msg = JSON.stringify({ 'searchstr': keyword.toString() });
                $.ajax({
                    async: true,
                    url: '/Comment/GetMentions',
                    type: 'POST',
                    contentType: "application/json; charset=utf-8",
                    dataType: 'json',
                    data: msg,
                    error: function () {
                        callback();
                    },
                    success: function (res) {
                        callback(res.users);
                    }
                });
            },
            content: function (item) {
                return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
            }
        }
    });
    window.scrollTo(0, document.body.scrollHeight + 50);
});

/**
 * 
 * @param {any} id: message id
 */
export function sendMessage(id) {
    var action = "/Messages/SendMessage";
    var dataval = '';
    var dataString = '';
    var messageElement = '#message_input';
    dataval = $(messageElement).summernote('code');
    dataString = JSON.stringify({ id: id, content: dataval, isChat: true });
    $('#chatReply').addClass('sk-loading');
    $.ajax({
        type: "POST",
        url: action,
        data: dataString,
        dataType: "json",
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            if (response.success) {
                $(".m_input").summernote('reset');
                $.ajax({
                    type: "POST",
                    url: "/Messages/GetMessage",
                    data: JSON.stringify({ 'id': response.id }),
                    headers: getAntiForgeryToken(),
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (result) {
                        $("#endMessages").append(result.HTMLString);
                        $('.postTime').each(function (i, e) {
                            var datefn = parseISO($(e).html());
                            datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
                            var date = format(datefn, "dd MMM yyyy");
                            var time = formatDistanceToNow(datefn, { addSuffix: true });
                            $(e).html('<span>' + time + ' ago - ' + date + '</span>');
                            $(e).css('display', 'inline');
                            $(e).removeClass("postTime");
                        });
                        window.scrollTo(0, document.body.scrollHeight + 10);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert("fail");
                    }
                });
                $('#chatReply').removeClass('sk-loading');
            }
            else {
                $('#chatReply').removeClass('sk-loading');
                alert(response.message);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
}
window.sendMessage = sendMessage;

/**
 * Loads older chat history and inserts into DOM
 * @param {any} id : User id for other user
 */
export function loadolderchats(id) {
    $.ajax({
        type: "POST",
        url: "/Messages/LoadOlder/",
        data: JSON.stringify({ otherId: ChattingWithId, start: startBlock, blocks: 10 }),
        dataType: "json",
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            if (response.success) {
                $("#startMessages").prepend(response.HTMLString); // Insert at the front
                startBlock += 10;
                $('.postTime').each(function (i, e) {
                    var datefn = parseISO($(e).html());
                    datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
                    var date = format(datefn, "dd MMM yyyy");
                    var time = formatDistanceToNow(datefn, { addSuffix: true });
                    $(e).html('<span>' + time + ' ago - ' + date + '</span>');
                    $(e).css('display', 'inline');
                    $(e).removeClass("postTime");
                });
            }
            else {
                alert(response.message);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
}
window.loadolderchats = loadolderchats;