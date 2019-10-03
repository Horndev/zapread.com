/* These are scripts for Manage/Index page*/

var updateLanguages = function () {
    console.log('updateLanguages');
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

var leave = function (id) {
    var leaveurl = "/Group/LeaveGroup";
    var data = JSON.stringify({ 'gid': id });
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: leaveurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result === "success") {
                $("#j_" + id.toString()).html("<i class='fa fa-user-plus'></i> Rejoin ");
                $("#j_" + id.toString()).attr("onClick", "javascript: join(" + id.toString() + "); ");
            }
        }
    });
    return false; // Prevent jump to top of page
};

var join = function (id) {
    var joinurl = "/Group/JoinGroup";
    var data = JSON.stringify({ 'gid': id });
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: joinurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result === "success") {
                $("#j_" + id.toString()).html("<i class='fa fa-user-times'></i> Leave ");
                $("#j_" + id.toString()).attr("onClick", "javascript: leave(" + id.toString() + "); ");
            }
        }
    });
    return false; // Prevent jump to top of page
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
                    var time = moment.utc($(e).html()).local().calendar();
                    var date = moment.utc($(e).html()).local().format("DD MMM YYYY");
                    $(e).html('<span>' + time + ' - ' + date + '</span>');
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