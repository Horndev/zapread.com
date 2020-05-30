//
// script for _PartialGroupEditTags.cshtml
import $ from 'jquery';

import { getAntiForgeryToken } from '../../utility/antiforgery';

$(document).ready(function () {
    $('#tagsel').selectize({
        plugins: ['restore_on_backspace', 'remove_button'],
        delimiter: ',',
        persist: false,
        create: function (input) {
            return {
                value: input,
                text: input
            };
        },
        render: {
            option: function (data, escape) {

                return '<div class="option" style="color: #fff;background-color:#1ab394;">' + escape(data.text) + '</div>';
            },
            item: function (data, escape) {
                return '<div class="item" style="color: #fff;background-color:#1ab394;">' + escape(data.text) + '</div>';
            }
        }
    });
});

$('#GroupTagSaveChanges').click(function () {
    var gid = groupId;
    var tags = $('#tagsel').val();
    $.ajax({
        async: true,
        url: "/Group/UpdateGrouptags/",
        type: "POST",
        dataType: "json",
        data: { groupId: gid, tags: tags },
        headers: getAntiForgeryToken(),
        success: function (data) {
            alert('Update successful.');
            $('#GAdminEditTagModal').modal('hide');
        }
    });
});