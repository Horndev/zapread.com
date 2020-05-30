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
import '../../shared/sharedlast';
var iconstable = {};
$(document).ready(function () {
    iconstable = $('#iconsTable').DataTable({
        "searching": false,
        //"bInfo": false,
        "lengthChange": false,
        "pageLength": 10,
        "processing": true,
        "serverSide": true,
        "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Admin/GetIcons",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            {
                "data": "Graphic",
                "orderable": true,
                "mRender": function (data, type, row) {
                    return "<i class='fa fa-" + data + " fa-3x'/>";
                }
            },
            {
                "data": "Icon",
                "orderable": true,
            },
            {
                "data": null,
                "orderable": false,
                "mRender": function (data, type, row) {
                    //alert(JSON.stringify(data));
                    return "<a href='javascript:void(0);' onclick='delicon(" + data.Id + ")'><i class='fa fa-trash fa-2x text-danger'></i></a>";
                }
            }
        ]
    });
});

export function delicon(item) {
    var msg = { iD: item };
    $.ajax({
        async: true,
        type: "POST",
        url: "/Admin/DeleteIcon/",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(msg),
        success: function (response) {
            iconstable.ajax.reload(null, false);
        },
        failure: function (response) {
            //alert("failure " + JSON.stringify(response));
        },
        error: function (response) {
            //alert("error " + JSON.stringify(response));
        }
    });
    return false;
}
window.delicon = delicon;

export function add() {
    var iconVal = $('#newIcon').val();
    var msg = { icon: iconVal };
    $.ajax({
        async: true,
        type: "POST",
        url: "/Admin/AddIcon/",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: JSON.stringify(msg),
        success: function (response) {
            iconstable.ajax.reload();
        },
        failure: function (response) {
            //alert("failure " + JSON.stringify(response));
        },
        error: function (response) {
            //alert("error " + JSON.stringify(response));
        }
    });
    return false;
}
window.add = add;