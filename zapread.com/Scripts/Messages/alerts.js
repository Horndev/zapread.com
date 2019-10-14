/** Scripts for Messages/Alerts
 **/

/* exported alertsTable */
var alertsTable = {};

$(document).ready(function () {
    // Table
    alertsTable = $('#alertsTable').DataTable({
        "searching": false,
        "bInfo": false,
        "lengthChange": false,
        "ordering": true,
        "order": [[1, "desc"]],
        "pageLength": 25,
        "processing": true,
        "serverSide": true,
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Messages/GetAlertsTable",
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
                        return "<span class='badge badge-primary' >" + data.Status + "</span>";
                    }
                    return "";
                }
            },
            {
                "data": "Date",
                "orderable": true,
                "name": "Date",
                "type": "date",
                "orderSequence": ["desc", "asc"],
                "mRender": function (data, type, row) {
                    var datefn = dateFns.parse(data);
                    var date = dateFns.format(datefn, "DD MMM YYYY");
                    var time = dateFns.distanceInWordsToNow(datefn, { addSuffix: true });
                    return date + ", " + time;
                }
            },
            { "data": "Title", "orderable": false, "name": "Title" },
            {
                "data": null,
                "name": 'Link',
                "orderable": false,
                "mRender": function(data, type, row) {
                    var linkText = "Go to Post";
                    if (data.HasLink) {
                        linkText = "Go to Post";
                    }
                    else if (data.HasCommentLink) {
                        linkText = "Go to Comment";
                    }
                    else {
                        return "";
                    }
                    return "<a href='/Post/Detail/" + data.Link + "#cid_" + data.Anchor + "'>" + linkText + "</a>";
                }
            },
            {
                "data": null,
                "name": 'Action',
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "<button class='btn btn-danger btn-outline btn-sm' id='a_" + data.AlertId + "' onclick='deletea(" + data.AlertId + ");'>Delete</button>";
                }
            }
        ]
    });
}); // end ready

/**
 * Delete an alert
 * @param {any} id : alert identifier
 */
/* exported deletea */
var deletea = function (id) {
    var url = "/Messages/DeleteAlert";

    $.ajax({
        async: true,
        type: "POST",
        url: url,
        data: JSON.stringify({ "id": id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            if (result.Result === "Success") {
                $('#a_' + id).parent().parent().hide();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
    return false;
};