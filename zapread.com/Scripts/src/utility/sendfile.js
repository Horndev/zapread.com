/*
 * 
 */
import $ from 'jquery';

export function sendFile(file, el) {
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
            $(el).summernote('insertImage', '/Img/Content/' + result.imgId, function (i) {
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