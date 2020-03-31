//
// script for _PartialGroupEditIcon.cshtml

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
    var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        url: "/Group/Icon/Update/",
        type: "POST",
        dataType: "json",
        data: { groupId: gid, icon: groupIcon },
        headers: headers,
        success: function (response) {
            if (response.success) {
                swal("Group icon update successful", {
                    icon: "success"
                });
                $('#GAdminEditIconModal').modal('hide');
            } else {
                swal("Error", "Error: " + response.message, "error");
            }
        },
        failure: function (response) {
            swal("Error", "Failure: " + response.message, "error");
        },
        error: function (response) {
            swal("Error", "Error: " + response.message, "error");
        }
    });
});