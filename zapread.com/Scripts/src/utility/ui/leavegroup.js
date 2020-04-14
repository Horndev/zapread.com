/*
 * 
 */
// TODO - remove requirement
import $ from 'jquery';

import { getAntiForgeryToken } from '../antiforgery';

export function leaveGroup(id, e) {
    var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        data: JSON.stringify({ 'gid': id }),
        type: 'POST',
        url: "/Group/LeaveGroup",
        headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                $(e).attr("onClick", "joinGroup(" + id.toString() + ",this);");
                if ($(e).data('page') === 'h') {
                    $(e).html("<i class='fa fa-plus'></i>");
                } else {
                    $(e).html("<i class='fa fa-user-plus'></i> Join ");
                }
                // For group view [TODO: Fix this bad code]
                if ($(e).data('page') === 'detail') {
                    var numMembers = parseInt($('#group_membercount_' + id.toString()).html());
                    $('#group_membercount_' + id.toString()).html(numMembers - 1);
                }
            }
        }
    });
    return false;
};