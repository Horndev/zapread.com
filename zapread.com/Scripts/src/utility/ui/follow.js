/*
 * 
 */
// TODO - remove requirement
import $ from 'jquery';

import { getAntiForgeryToken } from '../antiforgery';

export function follow(uid, s, e) {
    var msg = JSON.stringify({ 'id': uid, 's': s });
    $.ajax({
        type: "POST",
        url: "/user/SetFollowing",
        data: msg,
        headers: getAntiForgeryToken(),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.Result === "Success") {
                if (s === 1) { // Subscribed
                    if ($(e).hasClass('hover-follow')) {
                        $(e).html("<i class='fa fa-user'></i><i class='fa fa-check'></i>");
                        $(e).attr('title', 'Un-follow');
                        $(e).attr('onclick', 'follow(' + uid + ',0, this);');
                    }
                    $('#subBtnText').html("Unsubscribe");
                    $('#sublink').attr("onclick", "follow(uid,0);");
                } else { // Un-subscribed
                    if ($(e).hasClass('hover-follow')) {
                        $(e).html("<i class='fa fa-user-plus'></i>");
                        $(e).attr('title', 'Follow');
                        $(e).attr('onclick', 'follow(' + uid + ',1, this);');
                    }
                    $('#subBtnText').html("Subscribe");
                    $('#sublink').attr("onclick", "follow(uid,1);");
                }
            }
            else {
                alert(response.Message);
            }
        },
        failure: function (response) {
            console.log('follow failure');
        },
        error: function (response) {
            console.log('follow error');
        }
    });
    return false;
}