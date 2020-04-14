/*
 * 
 */
// TODO - remove requirement
import $ from 'jquery';

import { getAntiForgeryToken } from '../antiforgery';

export function joinGroup(id, e) {
    var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        data: JSON.stringify({ 'gid': id }),
        type: 'POST',
        headers: headers,
        url: "/Group/JoinGroup/",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                $(e).attr("onClick", "leaveGroup(" + id.toString() + ",this);");
                if ($(e).data('page') === 'h') {
                    $(e).html("<i class='fa fa-minus'></i>");
                } else {
                    $(e).html("<i class='fa fa-user-times'></i> Leave ");
                }
                // For group view [TODO: Fix this bad code]
                if ($(e).data('page') === 'detail') {
                    var numMembers = parseInt($('#group_membercount_' + id.toString()).html());
                    $('#group_membercount_' + id.toString()).html(numMembers + 1);
                }
            }
        }
    });
    return false;
};