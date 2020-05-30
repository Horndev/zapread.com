/**
 * Partial Modal for update user alias
 * 
 * [  ] Native JS
 * 
 */
import $ from 'jquery';

import { getAntiForgeryToken } from '../../utility/antiforgery';    // [✓]

export function updateAlias() {
    event.preventDefault();
    event.stopImmediatePropagation();
    var action = "/Manage/UpdateUserAlias";
    var contentType = "application/json; charset=utf-8";
    var dataval = '';
    var dataString = '';
    var messageElement = '#userAliasInput';
    dataval = $(messageElement).val();
    dataString = JSON.stringify({
        alias: dataval
    });
    console.log(dataString);
    $.ajax({
        async: true,
        type: "POST",
        url: action,
        data: dataString,
        dataType: "json",
        contentType: contentType,
        headers: getAntiForgeryToken(),
        success: function (response) {
            if (response.success) {
                $('#userAliasModal').modal('hide');
                alert('Update successful.  It may take a few minutes for the change to propagate.');
            }
            else {
                alert(response.message);
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
}
window.updateAlias = updateAlias;