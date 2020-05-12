/**
 * 
 * [✓] Native JS
 */
//import $ from 'jquery';

import { postJson } from '../postData';                     // [✓]

export function leaveGroup(id, e) {
    postJson("/Group/LeaveGroup/", { 'gid': id })
        .then((response) => {
            if (response.success) {
                e.setAttribute("onclick", "joinGroup(" + id.toString() + ",this);");//$(e).attr("onClick", "leaveGroup(" + id.toString() + ",this);");
                var page = e.getAttribute("data-page");
                if (page === 'h') {
                    e.innerHTML = "<i class='fa fa-plus'></i>";//$(e).html("<i class='fa fa-minus'></i>");
                } else {
                    e.innerHTML = "<i class='fa fa-user-plus'></i> Join ";//$(e).html("<i class='fa fa-user-times'></i> Leave ");
                }
                // For group view [TODO: Fix this bad code]
                if (page === 'detail') {
                    var numMembers = parseInt(document.getElementById("group_membercount_" + id.toString()).innerHTML);//$('#group_membercount_' + id.toString()).html());
                    document.getElementById("group_membercount_" + id.toString()).innerHTML = numMembers - 1;//$('#group_membercount_' + id.toString()).html(numMembers + 1);
                }
            }
        })

    //var headers = getAntiForgeryToken();
    //$.ajax({
    //    async: true,
    //    data: JSON.stringify({ 'gid': id }),
    //    type: 'POST',
    //    url: "/Group/LeaveGroup",
    //    headers: headers,
    //    contentType: "application/json; charset=utf-8",
    //    dataType: "json",
    //    success: function (response) {
    //        if (response.success) {
    //            $(e).attr("onClick", "joinGroup(" + id.toString() + ",this);");
    //            if ($(e).data('page') === 'h') {
    //                $(e).html("<i class='fa fa-plus'></i>");
    //            } else {
    //                $(e).html("<i class='fa fa-user-plus'></i> Join ");
    //            }
    //            // For group view [TODO: Fix this bad code]
    //            if ($(e).data('page') === 'detail') {
    //                var numMembers = parseInt($('#group_membercount_' + id.toString()).html());
    //                $('#group_membercount_' + id.toString()).html(numMembers - 1);
    //            }
    //        }
    //    }
    //});
    return false;
};