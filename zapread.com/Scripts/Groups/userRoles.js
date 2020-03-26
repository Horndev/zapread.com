//
// script used in _PartialAddUserToGroupRoleForm.cshtml

var GroupUserGo = function () {
    $('#GroupUserGroupRoles').show();
};
var GroupUserUpdate = function () {
    // Only works if user is group admin
    var uname = $("#GroupUser").val();
    //var gname = "@Model.GroupName";
    var isAdmin = $('#GroupAdminUserGroupAdministrator').is(":checked");

    // Get our AntiForgeryToken
    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    var data = { group: gname, user: uname, isAdmin: $('#GroupAdminUserGroupAdministrator').is(":checked"), isMod: $('#GroupAdminUserGroupModerator').is(":checked") };
    $.ajax({
        async: true,
        url: "/Group/UpdateUserGroupRoles",
        type: "POST",
        dataType: "json",
        data: data,
        headers: headers,
        success: function (data) {
            if (data.success) {
                alert('Update successful.');
            }
            else {
                alert('Error updating user roles: ' + data.message);
            }
        }
    });
};

$(document).ready(function () {
    $("#GroupUser").autocomplete({
        autoFocus: true,
        source: function (request, response) {
            // Get our AntiForgeryToken
            var form = $('#__AjaxAntiForgeryForm');
            var token = $('input[name="__RequestVerificationToken"]', form).val();
            var headers = {};
            headers['__RequestVerificationToken'] = token;
            $.ajax({
                async: true,
                url: "/Group/GetUsers",
                type: "POST",
                dataType: "json",
                headers: headers,
                data: { group: gname, prefix: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item, value: item };
                    }));
                }
            });
        },
        change: function (event, ui) {
            $.ajax({
                async: true,
                url: "/Group/GetUserGroupRoles",
                type: "POST",
                dataType: "json",
                headers: headers,
                data: { group: gname, user: $("#GroupUser").val() },
                success: function (data) {
                    //$('#UserGroupMember').prop('checked', data.indexOf("Member") > -1)
                    $('#GroupAdminUserGroupModerator').prop('checked', data.indexOf("Moderator") > -1);
                    $('#GroupAdminUserGroupAdministrator').prop('checked', data.indexOf("Administrator") > -1);
                }
            });
        }
    });
});