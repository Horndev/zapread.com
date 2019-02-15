/* Functions for posts */

/**
 * Dismiss messages an alerts
 * @param {any} t  : type (1 = alert)
 * @param {any} id : object id
 * @returns {bool} : true on success
 */
var dismiss = function (t, id) {
    var url = "";
    if (t === 1) {
        url = "/Messages/DismissAlert";
    }
    else if (t === 0) {
        url = "/Messages/DismissMessage";
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
};

var stickyPost = function (id) {
    $.ajax({
        type: "POST",
        url: "/Post/ToggleStickyPost",
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
};

var nsfwPost = function (id) {
    $.ajax({
        type: "POST",
        url: "/Post/ToggleNSFW",
        data: JSON.stringify({ "id": id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            if (result.Result === "Success") {
                alert("Post successfully toggled NSFW.");
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
};

var showNSFW = function (id) {
    $("#nsfw_" + id).hide();
    $("#nsfwb_" + id).hide();
};

var toggleChat = function (id, show) {
    show = typeof show !== 'undefined' ? show : false;
    $(".c_input_" + id.toString()).summernote({
        callbacks: {
            onImageUpload: function (files) {
                that = $(this);
                sendFile(files[0], that);
            }
        },
        focus: false,
        placeholder: 'Write comment...',
        disableDragAndDrop: false,
        toolbar: [['style', ['style']], ['para', ['ul', 'ol', 'paragraph']], 'bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],//false,
        minHeight: 60,
        maxHeight: 300,
        hint: {
            match: /\B@@(\w*)$/,
            search: function (keyword, callback) {
                if (!keyword.length) return callback();
                var msg = JSON.stringify({ 'searchstr': keyword.toString() })
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
                //return '@@' + item;
                //return $('<span />').addClass('badge').addClass('userhint').html('@@' + item)[0];
                return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
            }
        }
    });

    //$('.c_input_' + id.toString()).each(function (i, e) {
    //    $(e).removeClass("c_input_" + id.toString());
    //});
    $(".note-statusbar").css("display", "none");
    if (!show) {
        $('#comments_' + id.toString()).slideToggle(200);
        $('#preply_' + id.toString()).slideToggle(200);
    }
    else {
        $('#comments_' + id.toString()).slideDown(200);
        $('#preply_' + id.toString()).slideDown(200);
    }
};

var showReply = function (id) {
    $(".c_input_" + id.toString()).summernote({
        callbacks: {
            onImageUpload: function (files) {
                that = $(this);
                sendFile(files[0], that);
            }
        },
        focus: false,
        placeholder: 'Write comment...',
        disableDragAndDrop: false,
        toolbar: ['bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],//false,
        minHeight: 60,
        maxHeight: 300,
        hint: {
            match: /\B@@(\w*)$/,
            search: function (keyword, callback) {
                if (!keyword.length) return callback();
                var msg = JSON.stringify({ 'searchstr': keyword.toString() })
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
                //return '@@' + item;
                //return $('<span />').addClass('badge').addClass('userhint').html('@@' + item)[0];
                return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
            }
        }
    });

    $('.c_input_' + id.toString()).each(function (i, e) {
        $(e).removeClass("c_input_" + id.toString());
    });

    $(".note-statusbar").css("display", "none");
    $('#preply_' + id.toString()).toggle('show');
};

var deleteComment = function (id) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this comment!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then(function(willDelete) {
        if (willDelete) {
            $.post("/Comment/DeleteComment",
            { "Id": id },
            function (data) {
                if (data.Success) {
                    $('#comment_' + id.toString()).hide();
                    swal("Deleted! Your comment has been deleted.", {
                        icon: "success",
                    });
                }
                else {
                    swal("Error", "Error deleting comment.", "error");
                }
            });
        } else {
        console.log("cancelled delete");
        }
    });
};

var setPostLanguage = function (id) {
    swal({
        text: 'Enter new language code',
        content: "input",
        button: {
            text: "Ok",
            closeModal: false,
        }
    }).then(function(name) {
        if (!name) throw null;
        $.post("/Post/ChangeLanguage",
        { "postId": id, "newLanguage": name },
        function (data) {
            if (data.success) {
                swal("Post language has been updated!", {
                    icon: "success",
                });
            }
            else {
                swal("Error", "Error: " + data.message, "error");
            }
        });
    }).catch (function(err) {
        if (err) {
            swal("Error", "Error updating language.", "error");
        } else {
            swal.stopLoading();
            swal.close();
        }
    });
};

var deletePost = function (id) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this post!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then(function(willDelete) {
        if (willDelete) {
            $.post("Post/DeletePost",
            { "PostId": id },
            function (data) {
                if (data.Success) {
                    $('#post_' + id.toString()).hide();
                    swal("Deleted! Your post has been deleted.", {
                        icon: "success"
                    });
                }
                else {
                    swal("Error", "Error deleting post.", "error");
                }
            });
        } else {
            console.log("cancelled delete");
        }
    });
};

// For submitting comments (TODO: move this to own file)
var isCommenting = false;

var submitCommentA = function (postId, commentId, isReply) {
    if (!isCommenting) {
        var action = "/Comment/AddComment";
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
        contentType = "application/json; charset=utf-8";
        processData = false;
        isCommenting = true;

        $.ajax({
            type: "POST",
            url: action,
            data: dataString,
            dataType: "json",
            contentType: contentType,
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
};

var onAjaxCommentSuccessA = function (result) {
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
            $('#c_input_' + result.PostId.toString()).summernote('reset');
            $("#comments_" + result.PostId.toString()).append(result.HTMLString);
        }
        $('.postTime').each(function (i, e) {
            var time = moment.utc($(e).html()).local().calendar();
            var date = moment.utc($(e).html()).local().format("DD MMM YYYY");
            $(e).html('<span>' + time + ' - ' + date + '</span>');
            $(e).css('display', 'inline');
            $(e).removeClass("postTime");
        });
    }
};

var dofeedback = function () {
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
};

var OkButton = function (context) {
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
        };

var CancelButton = function (context) {
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
};

var editingId = -1;
var isEditing = false;
var editComment = function (id) {
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
                match: /\B@@(\w*)$/,
                search: function (keyword, callback) {
                    if (!keyword.length) return callback();
                    var msg = JSON.stringify({ 'searchstr': keyword.toString() })
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
                    //return '@@' + item;
                    //return $('<span />').addClass('badge').addClass('userhint').html('@@' + item)[0];
                    return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
                }
            }
        });
        isEditing = true;
    }
    else {
        alert("You can only edit one comment at a time.  Save or Cancel your editing.");
    }
};

var replyComment = function (id) {
    $('#c_reply_' + id.toString()).toggle('show');
    $('#c_reply_' + id.toString()).load('/Comment/GetInputBox' + "/" + id.toString(), function () {
        $(".c_input").summernote({
            callbacks: {
                onImageUpload: function (files) {
                    that = $(this);
                    sendFile(files[0], that);
                }
            },
            focus: false,
            placeholder: 'Write comment...',
            disableDragAndDrop: true,
            toolbar: ['bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],
            minHeight: 60,
            maxHeight: 300,
            hint: {
                match: /\B@@(\w*)$/,
                search: function (keyword, callback) {
                    if (!keyword.length) return callback();
                    var msg = JSON.stringify({ 'searchstr': keyword.toString() })
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
                    //return '@@' + item;
                    //return $('<span />').addClass('badge').addClass('userhint').html('@@' + item)[0];
                    return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
                }
            }
        });
        $(".note-statusbar").css("display", "none");
    });
};