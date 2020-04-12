/*
 * 
 */

import '../../shared/shared';
import '../../realtime/signalr';

import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';

var lightningTable = {};

$(document).ready(function () {
    $.get("/Admin/UserBalance/" + username + '/', function (data, status) {
        $("#userAuditBalance").html(data.value);
    });

    $.get("/Admin/UserLimboBalance/" + username + '/', function (data, status) {
        $("#userAuditBalanceLimboValue").html(data.value);
    });

    $.get("/Admin/GetLNFlow/" + username + "/7", function (data, status) {
        $("#lightningFlowBalance").html(data.value);
    });

    $.get("/Admin/GetEarningsSum/" + username + "/7", function (data, status) {
        $("#weeklyEarningsBalance").html(data.value);
        $("#totalEarningsBalance").html(data.total);
    });

    $.get("/Admin/GetSpendingSum/" + username + "/7", function (data, status) {
        $("#weeklySpendingBalance").html(data.value);
        $("#totalSpendingBalance").html(data.total);
    });

    lightningTable = $('#lightningTable').DataTable({
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
            url: "/Admin/GetLNTransactions/" + username + "/",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            { "data": "Created", "orderable": false },
            { "data": "Time", "orderable": true },
            { "data": "Type", "orderable": false },
            { "data": "Amount", "orderable": false },
            { "data": "Memo", "orderable": false },
            //{ "data": "Settled", "orderable": false },
            {
                "data": null,//"Type",
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "<span title='" + data.PaymentHash + "'>" + data.Settled.toString() + "</span>";
                }
            },
            { "data": "PaymentHash", "orderable": false },
            {
                "data": null,
                "name": 'Action',
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "<a href='/Admin/Audit/Transaction/" + data.id + "/' class='btn btn-primary btn-outline'>Check</a>";
                }
            },
        ],
    });

    $('#earningTable').DataTable({
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
            url: "/Admin/GetEarningEvents/" + username + "/",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            { "data": "Time", "orderable": true },
            { "data": "Type", "orderable": false },
            { "data": "Amount", "orderable": false }
        ]
    });

    $('#spendingTable').DataTable({
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
            url: "/Admin/GetSpendingEvents/" + username + "/",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [
            { "data": "Time", "orderable": true },
            {
                "data": null,//"Type",
                "orderable": false,
                "mRender": function (data, type, row) {
                    return "<a href='" + data.URL + "'>" + data.Type + "</a>";
                }
            },
            { "data": "Amount", "orderable": false }
        ]
    });
});

var LNSummary = function (days) {
    console.log(days, event);
    var a = event.target;

    $('#ln1').removeClass('active');
    $('#ln7').removeClass('active');
    $('#ln30').removeClass('active');
    $('#ln365').removeClass('active');
    $(a).addClass('active');

    $.get("/Admin/GetLNFlow/" + username + "/" + days.toString(), function (data, status) {
        $("#lightningFlowBalance").html(data.value);
    });
};

var ESummary = function (days) {
    console.log(days, event);
    var a = event.target;

    $('#e1').removeClass('active');
    $('#e7').removeClass('active');
    $('#e30').removeClass('active');
    $('#e365').removeClass('active');
    $(a).addClass('active');

    $.get("/Admin/GetEarningsSum/" + username + "/" + days.toString(), function (data, status) {
        $("#weeklyEarningsBalance").html(data.value);
        $("#totalEarningsBalance").html(data.total);
    });
};

var SSummary = function (days) {
    console.log(days, event);
    var a = event.target;

    $('#s1').removeClass('active');
    $('#s7').removeClass('active');
    $('#s30').removeClass('active');
    $('#s365').removeClass('active');
    $(a).addClass('active');

    $.get("/Admin/GetSpendingSum/" + username + "/" + days.toString(), function (data, status) {
        $("#weeklySpendingBalance").html(data.value);
        $("#totalSpendingBalance").html(data.total);
    });
};