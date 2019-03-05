/* Code backing _PartialModalLNTransaction.cshtml
 * 
*/

var onGetInvoice = function (e) {
    $("#btnCheckLNDeposit").show();
    $("#doLightningTransactionBtn").hide();

    var amount = $("#depositValueAmount").val();
    var memo = 'ZapRead.com deposit';

    var msg = JSON.stringify({
        "amount": amount.toString(),
        "memo": memo,
        "anon": '0',
        "use": "userDeposit",
        "useId": -1,           // undefined
        "useAction": -1        // undefined
    });

    $.ajax({
        type: "POST",
        url: "/Lightning/GetDepositInvoice/",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            $("#lightningDepositInvoiceInput").val(response.Invoice);
            $("#lightningDepositInvoiceLink").attr("href", "lightning:" + response.Invoice);
            $("#lightningDepositQR").attr("src", "/Img/QR?qr=" + encodeURI("lightning:" + response.Invoice));

            $("#lightningTransactionInvoiceResult").removeClass("bg-success");
            $("#lightningTransactionInvoiceResult").removeClass("bg-error");
            $("#lightningTransactionInvoiceResult").removeClass("bg-muted");
            $("#lightningTransactionInvoiceResult").addClass("bg-info");
            $("#lightningTransactionInvoiceResult").html("Please pay invoice to complete your deposit.");
            $("#lightningTransactionInvoiceResult").show();
            $("#lightningDepositQR").show();
            $("#lightningDepositInvoice").show();
        },
        failure: function (response) {
            $("#lightningTransactionInvoiceResult").html("Failed to generate invoice");
            $("#lightningTransactionInvoiceResult").removeClass("bg-success");
            $("#lightningTransactionInvoiceResult").removeClass("bg-muted");
            $("#lightningTransactionInvoiceResult").removeClass("bg-info");
            $("#lightningTransactionInvoiceResult").addClass("bg-error");
            $("#lightningTransactionInvoiceResult").show();
        },
        error: function (response) {
            $("#lightningTransactionInvoiceResult").html("Error generating invoice");
            $("#lightningTransactionInvoiceResult").removeClass("bg-success");
            $("#lightningTransactionInvoiceResult").removeClass("bg-info");
            $("#lightningTransactionInvoiceResult").removeClass("bg-muted");
            $("#lightningTransactionInvoiceResult").addClass("bg-error");
            $("#lightningTransactionInvoiceResult").show();
        }
    });
};

var onPayInvoice = function (e) {
    var invoice = $("#lightningWithdrawInvoiceInput").val();
    var msg = '{ request: "' + invoice.toString() + '" }';
    $("#btnPayLNWithdraw").attr("disabled", "disabled");
    $.ajax({
        type: "POST",
        url: "/Lightning/SubmitPaymentRequest",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            $("#btnPayLNWithdraw").removeAttr("disabled");
            if (response.Result === "success") {
                $("#lightningTransactionInvoiceResult").html("Payment successfully sent.");
                $("#lightningTransactionInvoiceResult").removeClass("bg-info");
                $("#lightningTransactionInvoiceResult").removeClass("bg-error");
                $("#lightningTransactionInvoiceResult").removeClass("bg-muted");
                $("#lightningTransactionInvoiceResult").addClass("bg-success");
                $("#lightningTransactionInvoiceResult").show();
                $('#withdrawModal').modal('hide');

                $.get("/Account/GetBalance", function (data, status) {
                    $(".userBalanceValue").each(function (i, e) {
                        $(e).html(data.balance);
                    });
                });
                $('#depositModal').modal('hide');
            }
            else {
                $("#lightningTransactionInvoiceResult").html(response.Result);
                $("#lightningTransactionInvoiceResult").removeClass("bg-success");
                $("#lightningTransactionInvoiceResult").removeClass("bg-muted");
                $("#lightningTransactionInvoiceResult").removeClass("bg-info");
                $("#lightningTransactionInvoiceResult").addClass("bg-error");
                $("#lightningTransactionInvoiceResult").show();
            }
        },
        failure: function (response) {
            $("#btnPayLNWithdraw").removeAttr("disabled");
            $("#lightningTransactionInvoiceResult").html("Failed to receive invoice");
            $("#lightningTransactionInvoiceResult").removeClass("bg-success");
            $("#lightningTransactionInvoiceResult").addClass("bg-error");
            $("#lightningTransactionInvoiceResult").show();
        },
        error: function (response) {
            $("#btnPayLNWithdraw").removeAttr("disabled");
            $("#lightningTransactionInvoiceResult").html("Error receiving invoice");
            $("#lightningTransactionInvoiceResult").removeClass("bg-success");
            $("#lightningTransactionInvoiceResult").removeClass("bg-muted");
            $("#lightningTransactionInvoiceResult").removeClass("bg-info");
            $("#lightningTransactionInvoiceResult").addClass("bg-error");
            $("#lightningTransactionInvoiceResult").show();
        }
    });
};

/**
 * Resets the LN deposit/withdraw invoice
 * @param {any} e button element which clicked
 */
var onCancelDepositWithdraw = function (e) {
    $("#btnCheckLNDeposit").hide();
    $("#doLightningTransactionBtn").show();

    $("#lightningTransactionInvoiceResult").hide();
    $("#lightningDepositQR").hide();
    $("#lightningDepositInvoice").hide();
};

/**
 * Check if the LN invoice was paid
 * @param {any} e Element calling the function
 */
var checkInvoicePaid = function (e) {
    var invoice = $("#lightningDepositInvoiceInput").val();
    $("#spinCheckPayment").show();

    var postData = JSON.stringify({
        "invoice": invoice.toString(),
        "isDeposit": true
    });

    $.ajax({
        type: "POST",
        url: "/Lightning/CheckPayment/",
        data: postData,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            $("#spinCheckPayment").hide();
            if (response.success) {
                if (response.result === true) {
                    // Payment has been successfully made
                    alert('Payment confirmed');
                }
            }
            else {
                alert(response.message);
            }
        },
        failure: function (response) {
            $("#spinCheckPayment").hide();
            alert(response.message);
        },
        error: function (response) {
            $("#spinCheckPayment").hide();
            alert(response.message);
        }
    });
};

var switchWithdraw = function () {
    $('#doLightningTransactionBtn').hide();
    $('#btnCheckLNDeposit').hide();
    $('#btnPayLNWithdraw').show();
    $("#lightningTransactionInvoiceResult").show();
    $("#lightningDepositQR").hide();
    $("#lightningDepositInvoice").hide();
    $("#lightningTransactionInvoiceResult").removeClass("bg-info");
    $("#lightningTransactionInvoiceResult").removeClass("bg-error");
    $("#lightningTransactionInvoiceResult").addClass("bg-muted");
    $("#lightningTransactionInvoiceResult").removeClass("bg-success");
    $("#lightningTransactionInvoiceResult").html("Paste invoice to withdraw");
};

var switchDeposit = function () {
    $('#doLightningTransactionBtn').show();
    $('#btnCheckLNDeposit').hide();
    $('#btnPayLNWithdraw').hide();
    $("#lightningTransactionInvoiceResult").removeClass("bg-info");
    $("#lightningTransactionInvoiceResult").removeClass("bg-error");
    $("#lightningTransactionInvoiceResult").addClass("bg-muted");
    $("#lightningTransactionInvoiceResult").removeClass("bg-success");
    $("#lightningTransactionInvoiceResult").html("Specify deposit amount to deposit and get invoice.");
};