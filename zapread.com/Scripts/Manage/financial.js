/**
 * Scripts for Manage/Financial
 **/

/* export lightningTable */
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
                    var datefn = dateFns.parse(data.Time);
                    datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
                    var dateStr = dateFns.format(datefn, "DD MMM YYYY hh:mm:ss");
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
                    return "<a href='" + data.URL + "'>" + data.Type + "</a>";
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

var LNSummary = function (days) {
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
};

var ESummary = function (days) {
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
};

var SSummary = function (days) {
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
};