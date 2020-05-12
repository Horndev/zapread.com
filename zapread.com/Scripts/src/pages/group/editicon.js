//
// script for _PartialGroupEditIcon.cshtml
import $ from 'jquery';

import { getAntiForgeryToken } from '../../utility/antiforgery';
import Swal from 'sweetalert2';

$(document).ready(function () {
    $('#select-icon').selectize({
        labelField: 'name',
        searchField: 'name',
        create: false,
        render: {
            option: function (item, escape) {
                return '<div>' +
                    '<span class="title">' +
                    '&nbsp;&nbsp;<i class="fa fa-' + escape(item.name) + ' fa-2x"></i>' +/* + ' ' + escape(item.name) + '</span>'*/
                    '</div>';
            },
            item: function (item, escape) {
                return '<div>' +
                    '<span class="title">' +
                    '&nbsp;&nbsp;<i class="fa fa-3x fa-' + escape(item.name) + '"></i>' +
                    '</div>';
            }
        }
    });
});

$('#GroupIconSaveChanges').click(function () {
    var gid = groupId;
    var groupIcon = $('#select-icon').val();
    $.ajax({
        async: true,
        url: "/Group/Icon/Update/",
        type: "POST",
        dataType: "json",
        data: { groupId: gid, icon: groupIcon },
        headers: getAntiForgeryToken(),
        success: function (response) {
            if (response.success) {
                Swal.fire("Group icon update successful", {
                    icon: "success"
                });
                $('#GAdminEditIconModal').modal('hide');
            } else {
                Swal.fire("Error", "Error: " + response.message, "error");
            }
        },
        failure: function (response) {
            Swal.fire("Error", "Failure: " + response.message, "error");
        },
        error: function (response) {
            Swal.fire("Error", "Error: " + response.message, "error");
        }
    });
});