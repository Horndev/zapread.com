

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