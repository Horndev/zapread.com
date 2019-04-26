/* ZapRead global functions */

$(document).ready(function () {
    // Set up social sharing links
    jsSocials.shares.copy = {
        label: "Copy",
        logo: "fa fa-copy",
        shareUrl: "javascript:(function() { copyTextToClipboard('{url}'); return false; })()",
        countUrl: "",
        shareIn: "self"
    };

    $.ajax({
        type: "POST",
        url: "/Messages/CheckUnreadChats",
        data: "",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                if (response.Unread > 0) {
                    $("#topChat").css("color", "red");
                }
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

    $(".sharing").each(function () {
        $(this).jsSocials({
            url: $(this).data('url'),
            text: $(this).data('sharetext'),
            showLabel: false,
            showCount: false,
            shareIn: "popup",
            shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
        });
        $(this).removeClass("sharing");
    });

    // popups for users, groups, etc.
    $(".pop").popover({
        trigger: "manual",
        html: true,
        sanitize: false,
        animation: false
    })
    .on("mouseenter", function () {
        var _this = this;
        $(this).popover("show");
        $('[data-toggle="tooltip"]').tooltip();
        $(".popover").addClass("tooltip-hover");
        $(".popover").on("mouseleave", function () {
            $(_this).popover('hide');
        });
    })
    .on("mouseleave", function () {
        var _this = this;
        setTimeout(function () {
            if (!$(".popover:hover").length) {
                $(_this).popover("hide");
            }
        }, 300);
        });

    $(".pop").each(function () {
        $(this).removeClass("pop");
    });

    toastr.options.closeMethod = 'fadeOut';
    toastr.options.closeDuration = 700;
    toastr.options.positionClass = 'toast-bottom-right';
    toastr.options.closeEasing = 'swing';
    toastr.options.closeButton = true;
    toastr.options.hideMethod = 'slideUp';
    toastr.options.progressBar = true;
    toastr.options.timeOut = 30000; // How long the toast will display without user interaction
    toastr.options.extendedTimeOut = 60000; // How long the toast will display after a user hovers over it

    $('[data-toggle="tooltip"]').tooltip();

    $("ul.dropdown-menu").on("click", "[data-keepOpenOnClick]", function (e) {
        e.stopPropagation();
    });

    // This loads all async partial views on page
    $(".partialContents").each(function (index, item) {
        var url = $(item).data("url");
        if (url && url.length > 0) {
            $(item).load(url);
        }
    });

}); // End document ready

var writeComment = function (e) {
    var id = $(e).data("postid");
    console.log('writeComment id: ' + id.toString());
    initCommentInput(id);
    $(e).hide();
    $(".note-statusbar").css("display", "none");
    $('#preply_' + id.toString()).slideDown(200);
    return false;
};

var toggleChat = function (id, show) {
    show = typeof show !== 'undefined' ? show : false;
    initCommentInput(id);
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

var initCommentInput = function (id) {
    console.log('Init summernote: ' + id.toString());
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
                return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
            }
        }
    });
    return false;
};

var loadMoreComments = function (e) {
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
            toolbar: [['style', ['style']], ['para', ['ul', 'ol', 'paragraph']], 'bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],
            minHeight: 60,
            maxHeight: 300,
            hint: {
                match: /\B@(\w*)$/,
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
        $(".note-statusbar").css("display", "none");
    });
};

var follow = function (uid, s) {
    var msg = JSON.stringify({ 'id': uid, 's': s });
    $.ajax({
        type: "POST",
        url: "/user/SetFollowing",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.Result === "Success") {
                if (s === 1) {
                    // Subscribe
                    $('#subBtnText').html("Unsubscribe");
                    $('#sublink').attr("onclick", "follow(uid,0);");
                } else {
                    $('#subBtnText').html("Subscribe");
                    $('#sublink').attr("onclick", "follow(uid,1);");
                }
            }
            else {
                alert(response.Message);
            }
        },
        failure: function (response) {
            console.log('follow failure');
        },
        error: function (response) {
            console.log('follow error');
        }
    });
    return false;
};

function sendFile(file, that) {
    var data = new FormData();
    data.append('file', file);
    console.log("Uploading File.");
    $("#progressUploadBar").css("width", "0%");
    $("#progressUpload").show();
    $.ajax({
        xhr: function () {
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    percentComplete = parseInt(percentComplete * 100);
                    $("#progressUploadBar").css("width", percentComplete.toString() + "%");
                    if (percentComplete === 100) {
                        $("#progressUploadBar").css("width", "100%");
                    }
                }
            }, false);
            return xhr;
        },
        data: data,
        type: 'POST',
        url: '/Img/UploadImage',
        cache: false,
        contentType: false,
        processData: false,
        success: function (result) {
            $("#progressUpload").hide();
            $(that).summernote('insertImage', '/Img/Content/' + result.imgId, function (i) {
                // Applied to img tag
                i.attr('class', 'img-fluid');
            });
        },
        error: function (data) {
            $("#progressUpload").hide();
            console.log(data);
            alert(JSON.stringify(data));
        }
    });
}

function fallbackCopyTextToClipboard(text) {
    var textArea = document.createElement("textarea");
    textArea.value = text;
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();

    try {
        var successful = document.execCommand('copy');
        var msg = successful ? 'successful' : 'unsuccessful';
        console.log('Fallback: Copying text command was ' + msg);
        if (successful) {
            swal("Post url copied to clipboard", {
                icon: "success"
            });
        }
        else {
            swal("Error", "Error copying url to clipboard", "error");
        }
    } catch (err) {
        console.error('Fallback: Oops, unable to copy', err);
        swal("Error", "Error copying url to clipboard: " + err, "error");
    }

    document.body.removeChild(textArea);
}

function copyTextToClipboard(text) {
    if (!navigator.clipboard) {
        fallbackCopyTextToClipboard(text);
        return;
    }
    navigator.clipboard.writeText(text).then(function () {
        console.log('Async: Copying to clipboard was successful!');
        swal("Post url copied to clipboard", {
            icon: "success"
        });
    }, function (err) {
        swal("Error", "Error copying url to clipboard: " + err, "error");
        console.error('Async: Could not copy text: ', err);
    });
}