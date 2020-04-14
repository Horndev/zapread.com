
import Swal from 'sweetalert2';
import { getAntiForgeryToken } from '../utility/antiforgery';
import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';

/**
 * Dismiss messages an alerts
 * @param {any} t  : type (1 = alert)
 * @param {any} id : object id
 * @returns {bool} : true on success
 */
/* exported dismiss */
export function dismiss(t, id) {
    var url = "";
    if (t === 1) {
        url = "/Messages/DismissAlert/";
    }
    else if (t === 0) {
        url = "/Messages/DismissMessage/";
    }
    $.ajax({
        type: "POST",
        url: url,
        data: JSON.stringify({ "id": id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            if (result.Result === "Success") {
                // Hide post
                if (t === 1) {
                    if (id === -1) { // Dismissed all
                        $('[id^="a_"]').hide();
                        $('[id^="a1_"]').hide();
                        $('[id^="a2_"]').hide();
                    } else {
                        $('#a_' + id).hide();
                        $('#a1_' + id).hide();
                        $('#a2_' + id).hide();
                    }
                    var urla = $("#unreadAlerts").data("url");
                    $("#unreadAlerts").load(urla);
                }
                else {
                    if (id === -1) { // Dismissed all
                        $('[id^="m_"]').hide();
                        $('[id^="m1_"]').hide();
                        $('[id^="m2_"]').hide();
                    } else {
                        $('#m_' + id).hide();
                        $('#m1_' + id).hide();
                        $('#m2_' + id).hide();
                    }
                    var urlm = $("#unreadMessages").data("url");
                    $("#unreadMessages").load(urlm);
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
    return false;
}
window.dismiss = dismiss;

/* exported stickyPost */
export function stickyPost(id) {
    $.ajax({
        type: "POST",
        url: "/Post/ToggleStickyPost/",
        data: JSON.stringify({ "id": id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            if (result.Result === "Success") {
                alert("Post successfully toggled Sticky.");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
}
window.stickyPost = stickyPost;

/* exported nsfwPost */
export function nsfwPost(id) {
    $.ajax({
        type: "POST",
        url: "/Post/ToggleNSFW",
        data: JSON.stringify({ "id": id }),
        dataType: "json",
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            if (result.success) {
                var message = "Successfully removed NSFW flag from post.";
                if (result.IsNSFW) {
                    message = "Successfully marked post NSFW.";
                }
                Swal.fire(message, {
                    icon: "success"
                });
            }
        },
        failure: function (response) {
            Swal.fire(response.message, {
                icon: "error"
            });
        },
        error: function (response) {
            Swal.fire(response.message, {
                icon: "error"
            });
        }
    });
}
window.nsfwPost = nsfwPost;

/* exported showNSFW */
export function showNSFW(id) {
    $("#nsfw_" + id).hide();
    $("#nsfwb_" + id).hide();
}
window.showNSFW = showNSFW;

/* exported deleteComment */
export function deleteComment(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this comment!",
        icon: "warning",
        showCancelButton: true
    }).then(function(willDelete) {
        if (willDelete.value) {
            $.post("/Comment/DeleteComment/",
            { "Id": id },
            function (data) {
                if (data.Success) {
                    $('#comment_' + id.toString()).hide();
                    Swal.fire("Deleted! Your comment has been deleted.", {
                        icon: "success"
                    });
                }
                else {
                    Swal.fire("Error", "Error deleting comment.", "error");
                }
            });
        } else {
        console.log("cancelled delete");
        }
    });
}
window.deleteComment = deleteComment;

/* exported setPostLanguage */
export function setPostLanguage(id) {
    Swal.fire({
        text: 'Enter new language code',
        input: 'text',
        inputValue: '',
        showCancelButton: true
    }).then(function(name) {
        if (!name.value) throw null;
        $.post("/Post/ChangeLanguage/",
        { "postId": id, "newLanguage": name.value },
        function (data) {
            if (data.success) {
                Swal.fire("Post language has been updated!", {
                    icon: "success"
                });
            }
            else {
                Swal.fire("Error", "Error: " + data.message, "error");
            }
        });
    }).catch (function(err) {
        if (err) {
            Swal.fire("Error", "Error updating language.", "error");
        } else {
            Swal.stopLoading();
            Swal.close();
        }
    });
}
window.setPostLanguage = setPostLanguage;

/* exported deletePost */
export function deletePost(id) {
    Swal.fire({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this post!",
        icon: "warning",
        showCancelButton: true
    }).then(function(willDelete) {
        if (willDelete.value) {
            $.post("/Post/DeletePost/",
            { "PostId": id },
            function (data) {
                if (data.Success) {
                    $('#post_' + id.toString()).hide();
                    Swal.fire("Deleted! Your post has been deleted.", {
                        icon: "success"
                    });
                }
                else {
                    Swal.fire("Error", "Error deleting post.", "error");
                }
            });
        } else {
            console.log("cancelled delete");
        }
    });
}
window.deletePost = deletePost;

// For submitting comments (TODO: move this to own file)
/* exported isCommenting */
var isCommenting = false;

/* exported submitCommentA */
export function submitCommentA(postId, commentId, isReply) {
    if (!isCommenting) {
        var action = "/Comment/AddComment/";
        var dataval = '';
        var commentElement = '';
        var dataString = '';
        if (isReply) {
            $('#sc_' + commentId.toString()).children('.ibox-content').addClass('sk-loading');
            commentElement = '#cr_input_' + commentId.toString();
            dataval = $(commentElement).summernote('code');
            dataString = JSON.stringify({ CommentContent: dataval, PostId: postId, CommentId: commentId, IsReply: isReply });
            $('#csr_' + commentId.toString()).show();
            $('#bcr_' + commentId.toString()).prop('disabled', true);
        }
        else {
            $('#pc_' + postId.toString()).children('.ibox-content').addClass('sk-loading');
            commentElement = '#c_input_' + postId.toString();
            dataval = $(commentElement).summernote('code');
            dataString = JSON.stringify({ CommentContent: dataval, PostId: postId, CommentId: commentId, IsReply: isReply });
            $('#cs_' + postId.toString()).show();
            $('#bc_' + postId.toString()).prop('disabled', true);
        }
        //contentType = "application/json; charset=utf-8";
        //processData = false;
        isCommenting = true;

        $.ajax({
            type: "POST",
            url: action,
            data: dataString,
            headers: getAntiForgeryToken(),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (result) {
                isCommenting = false;
                onAjaxCommentSuccessA(result);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                isCommenting = false;
                alert("fail");
            }
        });
    }
    return false;
}
window.submitCommentA = submitCommentA;

/* exported onAjaxCommentSuccessA */
export function onAjaxCommentSuccessA(result) {
    $('#cs_' + result.PostId.toString()).hide();
    $('#csr_' + result.CommentId.toString()).hide();
    $('#bc_' + result.PostId.toString()).prop('disabled', false);
    $('#bcr_' + result.CommentId.toString()).prop('disabled', false);
    $('#pc_' + result.PostId.toString()).children('.ibox-content').removeClass('sk-loading');
    $('#sc_' + result.CommentId.toString()).children('.ibox-content').removeClass('sk-loading');
    $('#comments_' + result.PostId.toString()).show();
    if (!result.success) {
        if (result.IsReply) {
            $('#cr_input_' + result.CommentId.toString()).summernote('reset');
            $('#cr_input_' + result.CommentId.toString()).summernote('destroy');
            $('#cr_input_' + result.CommentId.toString()).hide();
            $('#c_reply_' + result.CommentId.toString()).remove();
        }
        else {
            $('#c_input_' + result.PostId.toString()).summernote('reset');
        }
        alert(result.message);
    } else {
        if (result.IsReply) {
            $('#cr_input_' + result.CommentId.toString()).summernote('reset');
            $('#cr_input_' + result.CommentId.toString()).summernote('destroy');
            $('#cr_input_' + result.CommentId.toString()).hide();
            $('#c_reply_' + result.CommentId.toString()).remove();
            $("#rcomments_" + result.CommentId.toString()).prepend(result.HTMLString);
        }
        else {
            $('#preply_' + result.PostId.toString()).hide();
            $('#c_input_' + result.PostId.toString()).summernote('reset');
            $("#comments_" + result.PostId.toString()).prepend(result.HTMLString);
            $("#wc_" + result.PostId.toString()).show();
        }
        $('.postTime').each(function (i, e) {
            var datefn = parseISO($(e).html());
            datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
            var date = format(datefn, "dd MMM yyyy");
            var time = formatDistanceToNow(datefn, { addSuffix: false });
            $(e).html('<span>' + time + ' ago - ' + date + '</span>');
            $(e).css('display', 'inline');
            $(e).removeClass("postTime");
        });
    }
}
window.onAjaxCommentSuccessA = onAjaxCommentSuccessA;

/* exported dofeedback */
export function dofeedback() {
    var msg = $('#feedbackText').val();
    var feebackLocation = window.location.href;
    $.ajax({
        url: "/Home/SendFeedback",
        type: "POST",
        dataType: "json",
        data: { msg: msg, loc: feebackLocation },
        success: function (data) {
            alert('Feedback successfully sent.  Thank you!');
        }
    });

    $('.open-small-chat').children().toggleClass('fa-comments').toggleClass('fa-remove');
    $('.small-chat-box').toggleClass('active');
}
window.dofeedback = dofeedback;

/* exported OkButton */
export function OkButton(context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '<i class="fa fa-save"/> Save',
        tooltip: false,
        click: function () {
            var e = "#commentText_" + editingId.toString();
            $(e).summernote('destroy');
            var content = $(e).html();
            var msg = { "CommentContent": content.trim(), "CommentId": editingId };
            console.log(msg);
            $.post("/Comment/UpdateComment",
            msg,
                function (data) {
                    if (data.Success) {
                        console.log('update comment successful.');
                    }
                    else {
                        alert("Error updating comment");
                    }
                });
            isEditing = false;
            }
        });
    return button.render();   // return button as jquery object
}
window.OkButton = OkButton;

/* exported CancelButton */
export function CancelButton(context) {
    var ui = $.summernote.ui;
    // create button
    var button = ui.button({
        contents: '<i class="fa fa-times"/> Cancel',
        tooltip: false,
        click: function () {
            var e = "#commentText_" + editingId.toString();
            $(e).summernote('reset');
            // This returns the editor to normal state
            $(e).summernote('destroy');
            isEditing = false;
        }
    });
    return button.render();   // return button as jquery object
}
window.CancelButton = CancelButton;

var editingId = -1;
var isEditing = false;
/* exported editComment */
export function editComment(id) {
    if (!isEditing) {
        console.log("edit " + id.toString());
        var e = "#commentText_" + id.toString();
        editingId = id;
        $(e).summernote({
            focus: true,
            disableDragAndDrop: true,
            toolbar: [
                ['okbutton', ['ok']],
                ['cancelbutton', ['cancel']],
                'bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'
            ],
            buttons: {
                ok: OkButton,
                cancel: CancelButton
            },
            height: 100,
            hint: {
                match: /\B@(\w*)$/,
                search: function (keyword, callback) {
                    if (!keyword.length) return callback();
                    var msg = JSON.stringify({ 'searchstr': keyword.toString() });
                    $.ajax({
                        async: true,
                        url: '/Comment/GetMentions/',
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
        isEditing = true;
    }
    else {
        alert("You can only edit one comment at a time.  Save or Cancel your editing.");
    }
}
window.editComment = editComment;

