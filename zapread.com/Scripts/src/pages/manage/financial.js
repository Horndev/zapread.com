/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';
import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import '../../shared/sharedlast';

var lightningTable = {};

$(document).ready(function () {
    $.get("/Account/GetLNFlow/7", function (data, status) {
        $("#lightningFlowBalance").html(data.value);
    });

    $.get("/Account/GetEarningsSum/7", function (data, status) {
        $("#weeklyEarningsBalance").html(data.value);
        $("#totalEarningsBalance").html(data.total);
    });

    $.get("/Account/GetSpendingSum/7", function (data, status) {
        $("#weeklySpendingBalance").html(data.value);
        $("#totalSpendingBalance").html(data.total);
    });

    lightningTable = $('#lightningTable').DataTable({
        "searching": false,
        "lengthChange": false,
        "pageLength": 10,
        "processing": true,
        "serverSide": true,
        "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Manage/GetLNTransactions",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            {
                "data": null,
                "orderable": false,
                "mRender": function (data, type, row) {
                    var datefn = parseISO(data.Time);
                    datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
                    var dateStr = format(datefn, "dd MMM yyyy HH:mm:ss");
                    return dateStr;
                }
            },
            {
                "data": null,
                "orderable": false,
                "mRender": function (data, type, row) {
                    var icon = "<i class='fa fa-check text-success' title='Completed'></i>";
                    if (!data.IsSettled) {
                        icon = "<i class='fa fa-times text-danger' title='Not Completed'></i>";
                    }
                    if (data.IsLimbo) {
                        icon = icon + "<i class='fa fa-clock text-info' title='In Limbo'></i>";
                    }
                    if (data.Type) {
                        return icon + " Deposit";
                    }
                    return icon + " Withdraw";
                }
            },
            { "data": "Amount", "orderable": false },
            { "data": "Memo", "orderable": false }
        ]
    });

    $('#earningTable').DataTable({
        "searching": false,
        "lengthChange": false,
        "pageLength": 10,
        "processing": true,
        "serverSide": true,
        "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Manage/GetEarningEvents",
            headers: getAntiForgeryToken(),
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            { "data": "Time", "orderable": true },
            {
                "data": null,
                "orderable": false,
                "mRender": function (data, type, row) {
                    if (data.Memo !== "") {
                        return "<a href='" + data.URL + "'>" + data.Type + " (" + data.Memo + ")" + "</a>";
                    }
                    return data.Type;
                }
            },
            { "data": "Amount", "orderable": false }
        ]
    });

    $('#spendingTable').DataTable({
        "searching": false,
        "lengthChange": false,
        "pageLength": 10,
        "processing": true,
        "serverSide": true,
        "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Manage/GetSpendingEvents",
            headers: getAntiForgeryToken(),
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            { "data": "Time", "orderable": true },
            {
                "data": null,
                "orderable": false,
                "mRender": function (data, type, row) {
                    var URL = "/";
                    if (data.Type === "Post") {
                        URL = "/Post/Detail/" + data.TypeId + "/";
                    } else if (data.Type === "Comment") {
                        URL = "/Post/Detail/" + data.TypeId + "/";
                    } else if (data.Type === "Group") {
                        URL = "/Group/GroupDetail/" + data.TypeId + "/";
                    }

                    return "<a href='" + URL + "'>" + data.Type + "</a>";
                }
            },
            { "data": "Amount", "orderable": false }
        ]
    });

    $.get("/Account/GetLimboBalance", function (data, status) {
        $(".userBalanceLimboValue").each(function (i, e) {
            $(e).html(data.balance);
        });
    });
});

export function LNSummary(days) {
    console.log(days, event);
    var a = event.target;

    $('#ln1').removeClass('active');
    $('#ln7').removeClass('active');
    $('#ln30').removeClass('active');
    $('#ln365').removeClass('active');
    $(a).addClass('active');

    $.get("/Account/GetLNFlow/" + days.toString(), function (data, status) {
        $("#lightningFlowBalance").html(data.value);
    });
}
window.LNSummary = LNSummary;

export function ESummary(days) {
    console.log(days, event);
    var a = event.target;

    $('#e1').removeClass('active');
    $('#e7').removeClass('active');
    $('#e30').removeClass('active');
    $('#e365').removeClass('active');
    $(a).addClass('active');

    $.get("/Account/GetEarningsSum/" + days.toString(), function (data, status) {
        $("#weeklyEarningsBalance").html(data.value);
        $("#totalEarningsBalance").html(data.total);
    });
}
window.ESummary = ESummary;

export function SSummary(days) {
    console.log(days, event);
    var a = event.target;

    $('#s1').removeClass('active');
    $('#s7').removeClass('active');
    $('#s30').removeClass('active');
    $('#s365').removeClass('active');
    $(a).addClass('active');

    $.get("/Account/GetSpendingSum/" + days.toString(), function (data, status) {
        $("#weeklySpendingBalance").html(data.value);
        $("#totalSpendingBalance").html(data.total);
    });
}
window.SSummary = SSummary;