/* ZapRead global functions */

var toggleComment = function (e) {
    $(e).parent().find('.comment-body').first().fadeToggle({ duration: 0 });
    if ($(e).find('.togglebutton').hasClass("fa-minus-square")) {
        $(e).removeClass('pull-left');
        $(e).addClass('commentCollapsed');
        $(e).find('.togglebutton').removeClass("fa-minus-square");
        $(e).find('.togglebutton').addClass("fa-plus-square");
        $(e).find('#cel').show();
    }
    else {
        $(e).addClass('pull-left');
        $(e).removeClass('commentCollapsed');
        $(e).find('.togglebutton').removeClass("fa-plus-square");
        $(e).find('.togglebutton').addClass("fa-minus-square");
        $(e).find('#cel').hide();
    }
};

var togglePost = function (e) {
    $(e).parent().find('.social-body').slideToggle();
    if ($(e).find('.togglebutton').hasClass("fa-minus-square")) {
        $(e).find('.togglebutton').removeClass("fa-minus-square");
        $(e).find('.togglebutton').addClass("fa-plus-square");
    }
    else {
        $(e).find('.togglebutton').removeClass("fa-plus-square");
        $(e).find('.togglebutton').addClass("fa-minus-square");
    }
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