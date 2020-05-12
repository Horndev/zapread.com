/**
 * Scripts for User Management
 * 
 */

//import '../../shared/shared';
import $ from 'jquery';

import 'bootstrap';                                             // [X]            // still requires jquery :(
import 'bootstrap/dist/css/bootstrap.min.css';                  // [✓]
import 'font-awesome/css/font-awesome.min.css';                 // [✓]
import '../../utility/ui/paymentsscan';                         // [  ]
import '../../utility/ui/accountpayments';                      // [  ]
import '../../shared/postfunctions';                            // [✓]
import '../../shared/readmore';                                 // [✓]
import '../../shared/postui';                                   // [✓]
import '../../shared/topnavbar';                                // [✓]
import "jquery-ui-dist/jquery-ui";                              // [X]
import "jquery-ui-dist/jquery-ui.min.css";                      // [X]

import '../../utility/ui/vote';                                                 // [✓]
import '../../realtime/signalr';                                                // [✓]

import Dropzone from 'dropzone';
import 'dropzone/dist/basic.css';
import 'dropzone/dist/dropzone.css';
import 'bootstrap-chosen/dist/chosen.jquery-1.4.2/chosen.jquery';
import 'bootstrap-chosen/bootstrap-chosen.css';

import Swal from 'sweetalert2';

import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { loadachhover } from '../../utility/achievementhover';
import { loadmore } from '../../utility/loadmore';

import './updateAlias';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;
window.loadMoreComments = loadMoreComments;

/**
 * Wrapper for load more
 **/
export function manageloadmore() {
    loadmore({
        url: '/Manage/InfiniteScroll/',
        blocknumber: BlockNumber,
        sort: "New"
    });
}
window.loadmore = manageloadmore;

// 
onLoadedMorePosts();

$('.ach-hover').each(function () {
    $(this).mouseover(function () {
        loadachhover(this);
    });
});

Dropzone.options.dropzoneForm = {
    paramName: "file", // The name that will be used to transfer the file
    maxFilesize: 15, // MB
    acceptedFiles: "image/*",
    maxFiles: 1,
    uploadMultiple: false,
    init: function () {
        this.on("addedfile", function () {
        });
        this.on("success", function (file, response) {
            if (response.success) {
                updateImagesOnPage(response.version); // Reload images
            } else {
                // Did not work
                Swal.fire({
                    icon: 'error',
                    title: 'Image Update Error',
                    text: "Error updating image: " + response.message
                })
            }
        });
    },
    dictDefaultMessage: "<strong>Drop user image here or click to upload</strong>"
};

function updateImagesOnPage(ver) {
    document.getElementById("userImageLarge").setAttribute("src", "/Home/UserImage/?size=500&v=" + ver); //$('#userImageLarge').attr("src", "/Home/UserImage/?size=500&v=" + response.version);
    var elements = document.querySelectorAll(".user-image-30");
    Array.prototype.forEach.call(elements, function (el, _i) {
        el.setAttribute("src", "/Home/UserImage/?size=30&v=" + ver);
    });
    elements = document.querySelectorAll(".user-image-45");
    Array.prototype.forEach.call(elements, function (el, _i) {
        el.setAttribute("src", "/Home/UserImage/?size=45&v=" + ver);
    });
    elements = document.querySelectorAll(".user-image-15");
    Array.prototype.forEach.call(elements, function (el, _i) {
        el.setAttribute("src", "/Home/UserImage/?size=15&v=" + ver);
    });
}

$(document).ready(function () {
    $('.chosen-select').chosen({ width: "100%" }).on('change', function (evt, params) {
        var selectedValue = params.selected;
        var values = $('#languagesSelect').val();
        console.log(selectedValue);
        var userlangs = values.join(',');
        console.log(userlangs);
        $.ajax({
            async: true,
            data: JSON.stringify({ 'languages': userlangs }),
            type: 'POST',
            url: '/Manage/UpdateUserLanguages',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.success) {
                    console.log('languages updated.');
                }
                else {
                    console.log(response.message);
                }
            }
        });
    });

    // Set group list as clickable
    $(".clickable-row").click(function () {
        window.location = $(this).data("href");
    });
});

export function requestAPIKey() {
    $.ajax({
        async: true,
        type: 'GET',
        url: '/api/v1/account/apikeys/new?roles=default',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                Swal.fire({
                    icon: "success",
                    title: 'Your new key is:',
                    input: 'text',
                    inputValue: response.Key.Key,
                    showCancelButton: false
                });
            } else {
                // Did not work
                Swal.fire("Error generating key: " + data.message, "error");
            }
        },
        failure: function (response) {
            Swal.fire("Failure generating key: " + response.message, "error");
        },
        error: function (response) {
            Swal.fire("Error generating key: " + response.message, "error");
        }
    });
    return false; // Prevent jump to top of page
}
window.requestAPIKey = requestAPIKey;

export function updateLanguages() {
    console.log('updateLanguages');
}
window.updateLanguages = updateLanguages;

/** Change userprofile image
 * 
 * @param {any} set : 1 = robot, 2 = cat, 3 = human
 * @returns {boolean} false
 */
export function generateRobot(set) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    $.ajax({
        async: true,
        data: JSON.stringify({ "set": set }),
        type: 'POST',
        url: '/Home/SetUserImage/',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: headers,
        success: function (response) {
            if (response.success) {
                // Reload images
                updateImagesOnPage(response.version);
                //$('#userImageLarge').attr("src", "/Home/UserImage/?size=500&v=" + response.version);

                //$(".user-image-30").each(function () {
                //    $(this).attr("src", "/Home/UserImage/?size=30&v=" + response.version);
                //});

                //$(".user-image-45").each(function () {
                //    // Refreshes post user images
                //    $(this).attr("src", "/Home/UserImage/?size=45&v=" + response.version);
                //});

                //$(".user-image-15").each(function () {
                //    $(this).attr("src", "/Home/UserImage/?size=15&v=" + response.version);
                //});
                //$('#userImageLarge').attr("src", "/Home/UserImage/?size=500&r=" + new Date().getTime());
                //$(".user-image-30").each(function () {
                //    $(this).attr("src", "/Home/UserImage/?size=30&r=" + new Date().getTime());
                //});
                //$(".user-image-15").each(function () {
                //    $(this).attr("src", "/Home/UserImage/?size=15&r=" + new Date().getTime());
                //});
                //Swal.fire("Your profile image has been updated!", {
                //    icon: "success"
                //});
            } else {
                // Did not work
                Swal.fire({
                    icon: 'error',
                    title: 'Image Update Error',
                    text: "Error updating image: " + response.message
                })
            }
        },
        failure: function (response) {
            Swal.fire({
                icon: 'error',
                title: 'Image Update Error',
                text: "Error updating image: " + response.message
            })
        },
        error: function (response) {
            Swal.fire({
                icon: 'error',
                title: 'Image Update Error',
                text: "Error updating image: " + response.message
            })
        }
    });
    return false; // Prevent jump to top of page
}
window.generateRobot = generateRobot;

export function settingToggle(e) {
    var setting = e.id;
    var value = e.checked;
    let spinner = $(e).parent().find(".switch-spinner");
    spinner.removeClass("fa-check");
    spinner.addClass("fa-refresh");
    spinner.addClass("fa-spin");
    spinner.show();
    $.ajax({
        async: true,
        data: JSON.stringify({ 'setting': setting, 'value': value }),
        type: 'POST',
        url: '/Manage/UpdateUserSetting',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                spinner.removeClass("fa-refresh");
                spinner.removeClass("fa-spin");
                spinner.addClass("fa-check");
            }
        }
    });
}
window.settingToggle = settingToggle;

window.BlockNumber = 10;  //Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

//export function loadmore() {
//    if (!inProgress) {
//        inProgress = true;
//        $('#loadmore').show();
//        $('#btnLoadmore').prop('disabled', true);
//        $.post("/Manage/InfiniteScroll/",
//            { "BlockNumber": BlockNumber },
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
//                $(".c_input").summernote({
//                    callbacks: {
//                        onImageUpload: function (files) {
//                            let that = $(this);
//                            sendFile(files[0], that);
//                        }
//                    },
//                    focus: false,
//                    placeholder: 'Write comment...',
//                    disableDragAndDrop: true,
//                    toolbar: ['bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],
//                    minHeight: 60,
//                    maxHeight: 300,
//                    hint: {
//                        match: /\B@(\w*)$/,
//                        search: function (keyword, callback) {
//                            if (!keyword.length) return callback();
//                            var msg = JSON.stringify({ 'searchstr': keyword.toString() });
//                            $.ajax({
//                                async: true,
//                                url: '/Comment/GetMentions',
//                                type: 'POST',
//                                contentType: "application/json; charset=utf-8",
//                                dataType: 'json',
//                                data: msg,
//                                error: function () {
//                                    callback();
//                                },
//                                success: function (res) {
//                                    callback(res.users);
//                                }
//                            });
//                        },
//                        content: function (item) {
//                            return $("<span class='badge badge-info userhint'>").html('@' + item)[0];
//                        }
//                    }
//                });
//                $('.c_input').each(function (i, e) {
//                    $(e).removeClass("c_input");
//                });
//                $(".impression").each(function (ix, e) {
//                    $(e).load($(e).data("url"));
//                    $(e).removeClass("impression");
//                });
//                $(".note-statusbar").css("display", "none");
//            });
//    }
//}
//window.loadmore = loadmore;