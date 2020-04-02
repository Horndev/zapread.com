﻿//
//

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
                    if (data.IsOnline) {
                        return "Yes";
                    } else {
                        return "No";
                    }
                    
                    //return "<img class='img-circle user-image-30' src='/Home/UserImage/?size=30&UserId=" + data.AppId + "' /> <a class='post-username userhint' data-userid='" + data.Id + "' target='_blank' href='/user/" + encodeURIComponent(data.UserName) + "'>" + data.UserName + "</a>";
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