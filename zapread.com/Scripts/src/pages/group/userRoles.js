//
// script used in _PartialAddUserToGroupRoleForm.cshtml

import { getAntiForgeryToken } from '../../utility/antiforgery';

export function userGo() {
    $('#UserGroupRoles').show();
}
window.userGo = userGo;

export function userUpdate() {
    // Only works if user is admin
    var uname = $("#User").val();
    var isAdmin = $('#UserGroupAdministrator').is(":checked");
    var data = { group: gname, user: uname, isAdmin: $('#UserGroupAdministator').is(":checked"), isMod: $('#UserGroupModerator').is(":checked"), isMember: $('#UserGroupMember').is(":checked") };
    $.ajax({
        async: true,
        url: "/Admin/UpdateUserGroupRoles/",
        type: "POST",
        dataType: "json",
        data: data,
        headers: getAntiForgeryToken(), // Add token to request header
        success: function (data) {
            if (data.success) {
                alert('Update successful.');
            }
            else {
                alert('Error updating user roles: ' + data.message);
            }
        }
    });
}
window.userUpdate = userUpdate;

export function GroupUserGo() {
    $('#GroupUserGroupRoles').show();
}
window.GroupUserGo = GroupUserGo;

export function GroupUserUpdate() {
    // Only works if user is group admin
    var uname = $("#GroupUser").val();
    //var gname = "@Model.GroupName";
    var isAdmin = $('#GroupAdminUserGroupAdministrator').is(":checked");

    var data = { group: gname, user: uname, isAdmin: $('#GroupAdminUserGroupAdministrator').is(":checked"), isMod: $('#GroupAdminUserGroupModerator').is(":checked") };
    $.ajax({
        async: true,
        url: "/Group/UpdateUserGroupRoles",
        type: "POST",
        dataType: "json",
        data: data,
        headers: getAntiForgeryToken(),
        success: function (data) {
            if (data.success) {
                alert('Update successful.');
            }
            else {
                alert('Error updating user roles: ' + data.message);
            }
        }
    });
}
window.GroupUserUpdate = GroupUserUpdate;

$(document).ready(function () {
    $("#GroupUser").autocomplete({
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                async: true,
                url: "/Group/GetUsers/",
                type: "POST",
                dataType: "json",
                headers: getAntiForgeryToken(),
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
                url: "/Group/GetUserGroupRoles/",
                type: "POST",
                dataType: "json",
                headers: getAntiForgeryToken(),
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

$(document).ready(function () {
    $("#User").autocomplete({
        autoFocus: true,
        source: function (request, response) {
            $.ajax({
                async: true,
                url: "/Admin/GetUsers/",
                type: "POST",
                dataType: "json",
                headers: getAntiForgeryToken(),
                data: { prefix: request.term },
                success: function (data) {
                    response($.map(data, function (item) {
                        return { label: item, value: item };
                    }))
                }
            })
        },
        change: function (event, ui) {
            $.ajax({
                async: true,
                url: "/Admin/GetUserGroupRoles/",
                type: "POST",
                dataType: "json",
                data: { group: "@Model.GroupName", user: $("#User").val() },
                headers: getAntiForgeryToken(),
                success: function (data) {
                    $('#UserGroupMember').prop('checked', data.indexOf("Member") > -1)
                    $('#UserGroupModerator').prop('checked', data.indexOf("Moderator") > -1)
                    $('#UserGroupAdministrator').prop('checked', data.indexOf("Administrator") > -1)
                }
            });
        }
    });
});