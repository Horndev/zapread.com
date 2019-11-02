/* These are scripts for Manage/Index page*/

Dropzone.options.dropzoneForm = {
    paramName: "file", // The name that will be used to transfer the file
    maxFilesize: 15, // MB
    acceptedFiles: "image/*",
    maxFiles: 1,
    uploadMultiple: false,
    init: function() {
        this.on("addedfile", function() {
        });
        this.on("success", function(file, response) {
            if (response.success) {
                // Reload images
                $('#userImageLarge').attr("src", "/Home/UserImage/?size=500&r=" + new Date().getTime());
                $(".user-image-30").each(function() {
                    $(this).attr("src", "/Home/UserImage/?size=30&r=" + new Date().getTime());
                });
                $(".post-image-45").each(function() {
                    // Refreshes post user images
                    var src = $(this).attr('src');
                    $(this).attr("src", src + "&r=" + new Date().getTime());
                });
                $(".user-image-15").each(function() {
                    $(this).attr("src", "/Home/UserImage/?size=15&r=" + new Date().getTime());
                });
                swal("Your profile image has been updated!", {
                    icon: "success"
                });
                // This doesn't seem to be working properly :(
                $('.cuadro_intro_hover').each(function() {
                    $(this).css('position', 'absolute');
                    $(this).css('position', 'relative');
                });
            } else {
                // Did not work
                swal("Error", "Error updating image: " + data.message, "error");
            }
        });
    },
    dictDefaultMessage: "<strong>Drop user image here or click to upload</strong>"
};

$(document).ready(function() {
    $('.chosen-select').chosen({ width: "100%" }).on('change', function(evt, params) {
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
            success: function(response) {
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
    $(".clickable-row").click(function() {
        window.location = $(this).data("href");
    });

    // This loads all partial views on page
    $(".partialContents").each(function(index, item) {
        var url = $(item).data("url");
        if (url && url.length > 0) {
            $(item).load(url);
        }
    });

    // This formats the timestamps on the page
    $('.eventTime').each(function(i, e) {
        var datefn = dateFns.parse($(e).html());
        // Adjust to local time
        datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
        var date = dateFns.format(datefn, "DD MMM YYYY");
        var time = dateFns.distanceInWordsToNow(datefn);
        $(e).html('<span>' + time + ' ago - ' + date + '</span>');
    });
});

var updateLanguages = function () {
    console.log('updateLanguages');
};

/** Change userprofile image
 * 
 * @param {any} set : 1 = robot, 2 = cat, 3 = human
 * @returns {boolean} false
 */
var generateRobot = function (set) {
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    console.log('generateRobot ' + set);

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
                $('#userImageLarge').attr("src", "/Home/UserImage/?size=500&r=" + new Date().getTime());
                $(".user-image-30").each(function () {
                    $(this).attr("src", "/Home/UserImage/?size=30&r=" + new Date().getTime());
                });
                $(".user-image-15").each(function () {
                    $(this).attr("src", "/Home/UserImage/?size=15&r=" + new Date().getTime());
                });
                swal("Your profile image has been updated!", {
                    icon: "success"
                });
            } else {
                // Did not work
                swal("Error", "Error updating image: " + data.message, "error");
            }
        },
        failure: function (response) {
            swal("Error", "Failure updating image: " + response.message, "error");
        },
        error: function (response) {
            swal("Error", "Error updating image: " + response.message, "error");
        }
    });
    return false; // Prevent jump to top of page
};

var settingToggle = function (e) {
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
};

var BlockNumber = 10;  //Infinite Scroll starts from second block
var NoMoreData = false;
var inProgress = false;

$(window).scroll(function () {
    if ($(window).scrollTop() === $(document).height() -
        $(window).height() && !NoMoreData && !inProgress) {
        loadmore();
    }
});

var loadmore = function () {
    if (!inProgress) {
        inProgress = true;
        $('#loadmore').show();
        $('#btnLoadmore').prop('disabled', true);
        $.post("/Manage/InfiniteScroll/",
            { "BlockNumber": BlockNumber },
            function (data) {
                $('#loadmore').hide();
                $('#btnLoadmore').prop('disabled', false);
                BlockNumber = BlockNumber + 10;
                NoMoreData = data.NoMoreData;
                $("#posts").append(data.HTMLString);
                inProgress = false;
                $('.postTime').each(function (i, e) {
                    var datefn = dateFns.parse($(e).html());
                    // Adjust to local time
                    datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
                    var date = dateFns.format(datefn, "DD MMM YYYY");
                    var time = dateFns.distanceInWordsToNow(datefn);
                    $(e).html('<span>' + time + ' ago - ' + date + '</span>');
                    $(e).css('display', 'inline');
                    $(e).removeClass("postTime");
                });
                if (NoMoreData) {
                    $('#showmore').hide();
                }
                $(".sharing").each(function () {
                    $(this).jsSocials({
                        url: $(this).data('url'),
                        text: $(this).data('sharetext'),
                        showLabel: false,
                        showCount: false,
                        shareIn: "popup",
                        shares: ["email", "twitter", "facebook", "googleplus", "linkedin", "pinterest", "whatsapp"]
                    });
                    $(this).removeClass("sharing");
                });
                $(".c_input").summernote({
                    callbacks: {
                        onImageUpload: function (files) {
                            let that = $(this);
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
                            return $("<span class='badge badge-info userhint'>").html('@' + item)[0];
                        }
                    }
                });
                $('.c_input').each(function (i, e) {
                    $(e).removeClass("c_input");
                });
                $(".impression").each(function (ix, e) {
                    $(e).load($(e).data("url"));
                    $(e).removeClass("impression");
                });
                $(".note-statusbar").css("display", "none");
            });
    }
};