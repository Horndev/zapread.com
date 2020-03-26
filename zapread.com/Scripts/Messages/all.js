//
// Scripts for /Messages/All/

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
        //"sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Messages/GetMessagesTable",
            headers: getAntiForgeryToken(),
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            {
                "data": null,
                "name": 'Status',
                "orderable": true,
                "mRender": function (data, type, row) {
                    if (data.Status === "Read") {
                        return "<span class='badge badge-primary' >" + data.Status + "</span>"
                    }
                    return "";
                }
            },
            { "data": "Date", "orderable": true, "name": "Date", "type": "date", "orderSequence": ["desc", "asc"] },
            {
                "data": null,
                "name": 'From',
                "orderable": true,
                "mRender": function (data, type, row) {
                    return "<div style='display:inline-block;white-space: nowrap;'><img class='img-circle' src='/Home/UserImage/?UserID=" + encodeURIComponent(data.FromID) + "&size=30'/><a target='_blank' href='/user/" + encodeURIComponent(data.From) + "''> " + data.From + "</a></div>";
                }
            },

            { "data": "Message", "orderable": false, "name": "Message" },
            {
                "data": null,
                "name": 'Link',
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "<a href='/Post/Detail/" + data.Link + "#cid_" + data.Anchor + "'>Go to Comment</a>";
                }
            },
            {
                "data": null,
                "name": 'Action',
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "TODO";
                }
            }
        ]
    });
});