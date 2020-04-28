/*
 * 
 */

import Swal from 'sweetalert2';
import { getAntiForgeryToken } from '../antiforgery';

export function loadpost(postId) {
    Swal.fire({
        title: "Are you sure?",
        text: "Any unsaved changes in the current post will be lost.",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete.value) {
            var form = document.createElement('form');
            document.body.appendChild(form);
            form.method = 'post';
            form.action = "/Post/Edit/";
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
}
window.loadpost = loadpost;

export function del(postId) {
    Swal.fire({
        title: "Are you sure?",
        text: "Once deleted, you will not be able to recover this draft!",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then(function (willDelete) {
        if (willDelete.value) {
            $.post("/Post/DeletePost/",
                { "PostId": postId },
                function (data) {
                    if (data.Success) {
                        Swal.fire("Deleted! Your draft has been deleted.", {
                            icon: "success"
                        });
                        draftsTable.ajax.reload(null, false);
                    }
                    else {
                        Swal.fire("Error", "Error deleting draft.", "error");
                    }
                });
        } else {
            console.log("cancelled delete");
        }
    });
}
window.del = del;

export function submit(postId, groupId, userId, isUpdate) {
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

    $.ajax({
        async: true,
        type: "POST",
        url: url,
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: getAntiForgeryToken(),
        success: function (response) {
            if (response.success) {
                $('#submit').prop('disabled', false);
                var newPostUrl = "/Post/Detail";
                newPostUrl = newPostUrl + '/' + response.postId;
                window.location.replace(newPostUrl);
            } else {
                $('#submit').prop('disabled', false);
                $('#save').prop('disabled', false);
                $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
                Swal.fire("Error", response.message, "error");
            }
        },
        failure: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
            Swal.fire("Error", response.message, "error");
        },
        error: function (response) {
            $('#submit').prop('disabled', false);
            $('#save').prop('disabled', false);
            $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
            Swal.fire("Error", response.message, "error");
        }
    });
    $('.click2edit').summernote('destroy');
}
window.submit = submit;

export function save(postId, groupId, userId, isUpdate) {
    $('#postEdit').children('.ibox-content').toggleClass('sk-loading');
    $('#submit').prop('disabled', true);
    $('#save').prop('disabled', true);
    var aHTML = $('.click2edit').summernote('code');
    var postTitle = $('#postTitle').val();
    var language = $('#languageSelect').val();
    var msg = JSON.stringify({ 'PostId': postId, 'Content': aHTML, 'GroupId': groupId, 'UserId': userId, 'Title': postTitle, 'IsDraft': true, 'Language': language });

    $.ajax({
        async: true,
        type: "POST",
        url: "/Post/SubmitNewPost/",
        data: msg,
        headers: getAntiForgeryToken(),
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
}
window.save = save;

export function edit() {
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
}
window.edit = edit;

export function changeGroup() {
    Swal.fire({
        title: "Changing groups resets the score.",
        text: "Are you sure you want to move this post?",
        icon: "warning",
        showCancelButton: true
    }).then(function (willMove) {
        if (willMove.value) {
            $("#postGroup").autocomplete({
                autoFocus: true,
                source: function (request, response) {
                    $.ajax({
                        async: true,
                        url: "/Group/GetGroups/",
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
}
window.changeGroup = changeGroup;