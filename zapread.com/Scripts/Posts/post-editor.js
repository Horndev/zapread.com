
var knownGroups = [''];
var draftsTable = {};

$(document).ready(function () {
    draftsTable = $('#draftsTable').DataTable({
        "searching": true,
        "lengthChange": false,
        "pageLength": 10,
        "processing": true,
        "serverSide": true,
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Post/GetDrafts",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            { "data": "Title", "orderable": true },
            {
                "data": null,
                "orderable": true,
                "mRender": function (data, type, row) {
                    return "<a href='/Group/GroupDetail/" + data.GroupId + "'>" + data.Group + "</a>";
                }
            },
            { "data": "Time", "orderable": false },
            {
                "data": null,//"Type",
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "<button class='btn btn-sm btn-primary' onclick=loadpost(" + data.PostId + ")>Load</button> <button class='btn btn-sm btn-danger' onclick=del(" + data.PostId + ")>Delete</button>"//"<a href='" + data.URL + "'>" + data.Type + "</a>";
                }
            }
        ]
    });

    $("#postGroup").autocomplete({
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                async: true,
                url: "/Group/GetGroups",
                type: "POST",
                dataType: "json",
                data: { prefix: request.term },
                success: function (data) {
                    knownGroups = data;
                    response($.map(data, function (item) {
                        return { label: item.GroupName, value: item.GroupName };
                    }));
                }
            });
        },
        select: function (event, ui) {
            // if user clicked
        },
        change: function (event, ui) {
            var gn = $("#postGroup").val();
            if (typeof knownGroups === 'undefined' || knownGroups.length === 0) {
                // variable is undefined
                $("#postGroup").addClass('is-invalid');
            }
            else {
                if (knownGroups.findIndex(function (i) { return i.GroupName === gn; }) >= 0) {
                    $("#postGroup").removeClass('is-invalid');
                    gid = knownGroups[knownGroups.findIndex(function (i) { return i.GroupName === gn; })].GroupId;
                    $('#postGroupActive').html(gn);
                    $('#groupLink').html(gn);
                    $('#groupLink').attr('href', '@Url.Action("GroupDetail", "Group")' + '?id=' + gid.toString());
                }
                else {
                    $("#postGroup").addClass('is-invalid');
                }
            }
        }
    });

    $('.click2edit').summernote({
        toolbarContainer: '#editorToolbar',
        otherStaticBar: '.navbar',
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
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['table', ['table']],
            ['insert', ['link', 'picture', 'videoAttributes']],
            ['view', ['fullscreen', 'codeview']]
        ],
        focus: true,
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
});

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
        url = "/Post/Update/";
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
        success: function(response) {
            if (response.success) {
                $('#submit').prop('disabled', false);
                var newPostUrl = "/Post/Detail";
                newPostUrl = newPostUrl + '/' + response.postId;
                window.location.replace(newPostUrl);
            } else {
                $('#submit').prop('disabled', false);
                $('#save').prop('disabled', false);
                $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
                swal("Error", response.message, "error");
            }
        },
        failure: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
            swal("Error", response.message, "error");
        },
        error: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
            swal("Error", response.message, "error");
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
            $('.click2edit').html(response.HTMLContent);
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

var changeGroup = function () {
    swal({
        title: "Changing groups resets the score.",
        text: "Are you sure you want to move this post?",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then(function (willMove) {
        if (willMove) {
            $("#postGroup").autocomplete({
                autoFocus: true,
                source: function (request, response) {
                    $.ajax({
                        async: true,
                        url: "/Group/GetGroups",
                        type: "POST",
                        dataType: "json",
                        data: { prefix: request.term },
                        success: function (data) {
                            knownGroups = data;
                            response($.map(data, function (item) {
                                return { label: item.GroupName, value: item.GroupName };
                            }));
                        }
                    });
                },
                select: function (event, ui) {
                    // if user clicked
                },
                change: function (event, ui) {
                    var gn = $("#postGroup").val();
                    if (typeof knownGroups === 'undefined' || knownGroups.length === 0) {
                        // variable is undefined
                        $("#postGroup").addClass('is-invalid');
                    }
                    else {
                        if (knownGroups.findIndex(function (i) { return i.GroupName === gn; }) >= 0) {
                            $("#postGroup").removeClass('is-invalid');
                            gid = knownGroups[knownGroups.findIndex(function (i) { return i.GroupName === gn; })].GroupId;
                        }
                        else {
                            $("#postGroup").addClass('is-invalid');
                        }
                    }
                }
            });

            $('#changeGroupBtn').hide();
            $('#editGroup').show();
        } else {
            console.log("cancelled move");
        }
    });
    return false;
};