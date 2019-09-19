
var loadpost = function (postId) {
    swal({
        title: "Are you sure?",
        text: "Any unsaved changes in the current post will be lost.",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            var form = document.createElement('form');
            document.body.appendChild(form);
            form.method = 'post';
            form.action = "/Post/Edit";
            var data = { 'PostId': postId };
            for (var name in data) {
                var input = document.createElement('input');
                input.type = 'hidden';
                input.name = name;
                input.value = data[name];
                form.appendChild(input);
            }
            form.submit();
        } else {
            console.log("cancelled load");
        }
    });
};

var del = function (postId) {
    swal({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this draft!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then(function (willDelete) {
        if (willDelete) {
            $.post("/Post/DeletePost/",
            { "PostId": postId },
            function (data) {
                if (data.Success) {
                    swal("Deleted! Your draft has been deleted.", {
                        icon: "success"
                    });
                    draftsTable.ajax.reload(null, false);
                }
                else {
                    swal("Error", "Error deleting draft.", "error");
                }
            });
        } else {
            console.log("cancelled delete");
        }
    });
};

var submit = function (postId, groupId, userId, isUpdate) {
    $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
    $('#submit').prop('disabled', true);
    $('#save').prop('disabled', true);
    var aHTML = $('.click2edit').summernote('code');
    var postTitle = $('#postTitle').val();
    var language = $('#languageSelect').val();
    var msg = JSON.stringify({ 'PostId': postId, 'Content': aHTML, 'GroupId': groupId, 'UserId': userId, 'Title': postTitle, 'IsDraft': false, 'Language': language });
    var url = "/Post/SubmitNewPost/";
    if (isUpdate) {
        url = "/Post/Update";
    }

    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    $.ajax({
        async: true,
        type: "POST",
        url: url,
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: headers,
        success: function (response) {
            $('#submit').prop('disabled', false);
            var newPostUrl = "/Post/Detail";
            newPostUrl = newPostUrl + '/' + response.postId;
            window.location.replace(newPostUrl);
        },
        failure: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
        },
        error: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
        }
    });
    $('.click2edit').summernote('destroy');
};

var save = function (postId, groupId, userId, isUpdate) {
    $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
    $('#submit').prop('disabled', true);
    $('#save').prop('disabled', true);
    var aHTML = $('.click2edit').summernote('code');
    var postTitle = $('#postTitle').val();
    var language = $('#languageSelect').val();
    var msg = JSON.stringify({ 'PostId': postId, 'Content': aHTML, 'GroupId': groupId, 'UserId': userId, 'Title': postTitle, 'IsDraft': true, 'Language': language });

    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    $.ajax({
        async: true,
        type: "POST",
        url: "/Post/SubmitNewPost/",
        data: msg,
        headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
            draftsTable.ajax.reload(null, false);
        },
        failure: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
        },
        error: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
        }
    });
    $('.click2edit').summernote('destroy');
};

var edit = function () {
    $('.click2edit').summernote({
        callbacks: {
            onImageUpload: function (files) {
                that = $(this);
                sendFile(files[0], that);
            }
        },
        toolbar: [
            ['style', ['style']],
            ['font', ['bold', 'italic', 'underline', 'clear', 'strikethrough', 'superscript', 'subscript']],
            ['fontname', ['fontname']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'video']],
            ['view', ['fullscreen']]
        ],
        focus: true
    });
};