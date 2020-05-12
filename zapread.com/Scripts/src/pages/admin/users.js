/*
 * 
 */
import $ from 'jquery';

import '../../shared/shared';
import '../../realtime/signalr';
import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import '../../shared/sharedlast';
export function checkonline(id) {
    $.get("/api/v1/admin/checkonline/" + id + "/", function (result) {
        if (result.success) {
            alert("Check successful.");
        } else {
            alert("Check failed.");
        }
    });
}
window.checkonline = checkonline;

var usersTable = {};
$(document).ready(function () {
    // Table
    usersTable = $('#usersTable').DataTable({
        "searching": false,
        "bInfo": false,
        "lengthChange": false,
        "ordering": true,
        "order": [[1, "desc"]],
        "pageLength": 25,
        "processing": true,
        "serverSide": true,
        "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Admin/UsersTable/",
            headers: getAntiForgeryToken(),
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            {
                "data": null,
                "name": 'Name',
                "orderable": false,
                "mRender": function (data, _type, _row) {
                    return "<img class='img-circle user-image-30' src='/Home/UserImage/?size=30&UserId=" + data.AppId + "' /> <a class='post-username userhint' data-userid='" + data.Id + "' target='_blank' href='/user/" + encodeURIComponent(data.UserName) + "'>" + data.UserName + "</a>";
                }
            },
            {
                "data": null,
                "name": 'IsOnline',
                "orderable": true,
                "mRender": function (data, _type, _row) {
                    var html = "<span onclick='checkonline(" + data.Id + ");'>";
                    if (data.IsOnline) {
                        return html + "Yes" + "</span>";
                    } else {
                        return html + "No" + "</span>";
                    }
                }
            },
            { "data": "DateJoined", "orderable": true, "name": "DateJoined", "type": "date", "orderSequence": ["desc", "asc"] },
            { "data": "LastSeen", "orderable": true, "name": "LastSeen", "type": "date", "orderSequence": ["desc", "asc"] },
            { "data": "NumPosts", "orderable": true, "name": "NumPosts", "type": "num", "orderSequence": ["desc", "asc"] },
            { "data": "NumComments", "orderable": true, "name": "NumComments", "type": "num", "orderSequence": ["desc", "asc"] },
            { "data": "Balance", "orderable": true, "name": "Balance", "type": "num-fmt", "orderSequence": ["desc", "asc"] }
        ]
    });
});