/*
 * 
 */
import $ from 'jquery';
import '../../summernote/dist/summernote-bs4';
import 'summernote/dist/summernote-bs4.css';

import { sendFile } from '../utility/sendfile';

export function initCommentInput(id) {
    //console.log('Init summernote: ' + id.toString());
    $(".c_input_" + id.toString()).summernote({
        callbacks: {
            onImageUpload: function (files) {
                sendFile(files[0], this);
            }
        },
        focus: false,
        placeholder: 'Write comment...',
        disableDragAndDrop: false,
        toolbar: [['style', ['style']], ['para', ['ul', 'ol', 'paragraph']], 'bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],//false,
        minHeight: 60,
        maxHeight: 300,
        hint: {
            match: /\B@(\w*)$/,
            search: function (keyword, callback) {
                console.log(keyword);
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
                return $("<span class='badge badge-info userhint'>").html('@' + item)[0];
            }
        }
    });
    return false;
}