/*
 * 
 */

import '../../shared/shared';
import '../../realtime/signalr';
import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
import Dropzone from 'dropzone';
import 'dropzone/dist/basic.css';
import 'dropzone/dist/dropzone.css';
import Swal from 'sweetalert2';
import { getAntiForgeryToken } from '../../utility/antiforgery';

window.Dropzone = Dropzone;

var mt = "";

Dropzone.options.dropzoneForm = {
    paramName: "file", // The name that will be used to transfer the file
    maxFilesize: 5, // MB
    acceptedFiles: ".macaroon",
    maxFiles: 1,
    addRemoveLinks: true,
    init: function () {
        this.on("sending", function (file, xhr, formData) {
            formData.append("macaroonType", mt);
            console.log(formData);
        });
        this.on("addedfile", function () {
            //if (this.files[1] != null) {
            //    this.removeFile(this.files[0]);
            //}
        });
        this.on("success", function (file, response) {
            console.log(response.macaroon);
            $('#' + mt).val(response.macaroon);
            $('#ModalFileUpload').modal('hide');
        })
    },
    dictDefaultMessage: "<strong>Drop files here or click to upload. </strong>"
};

export function uploadMacaroon(id) {
    mt = id;
    console.log(id);
    $('#ModalFileUpload').modal('show');
    return false;
}
window.uploadMacaroon = uploadMacaroon;

export function save() {
    var msg = JSON.stringify({
        'LnMainnetHost': $('#LnMainnetHost').val(),
        'LnPubkey': $('#LnPubkey').val(),
        'LnMainnetMacaroonAdmin': $('#LnMainnetMacaroonAdmin').val(),
        'LnMainnetMacaroonInvoice': $('#LnMainnetMacaroonInvoice').val(),
        'LnMainnetMacaroonRead': $('#LnMainnetMacaroonRead').val()
    });
    $.ajax({
        async: true,
        type: "POST",
        url: "/Admin/Lightning/Update/",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result === 'success') {
                alert("Successfully saved.");
            }
        },
        failure: function (response) {
            alert("Error saving.");
        },
        error: function (response) {
            alert("Error saving.");
        }
    });
}
window.save = save;