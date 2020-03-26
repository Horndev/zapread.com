//
// script for _PartialModalMessageCompose.cshtml

$('#sendPrivateMessage').click(function () {
    $(".m_input").summernote({
        callbacks: {
            onImageUpload: function (files) {
                that = $(this);
                sendFile(files[0], that);
            }
        },
        focus: false,
        placeholder: 'Write message...',
        disableDragAndDrop: false,
        dialogsInBody: true,
        toolbar: ['bold', 'italic', 'underline', 'strikethrough', 'fontsize', 'color', 'link'],
        minHeight: 100,
        maxHeight: 600,
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
                //return '@@' + item;
                //return $('<span />').addClass('badge').addClass('userhint').html('@@' + item)[0];
                return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
            }
        }
    });
});

var sendMessage = function (id) {
    //event.preventDefault();
    //event.stopImmediatePropagation();
    var action = "/Messages/SendMessage";
    var contentType = "application/json; charset=utf-8";
    var dataval = '';
    var dataString = '';
    var messageElement = '#message_input';
    dataval = $(messageElement).summernote('code');
    dataString = JSON.stringify({ id: id, content: dataval });
    console.log(dataString);
    $.ajax({
        type: "POST",
        url: action,
        data: dataString,
        dataType: "json",
        contentType: contentType,
        success: function (response) {
            if (response.success) {
                $('#messageComposeModal').modal('hide');
            }
            else {
                alert(response.message);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
};