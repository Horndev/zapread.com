/* Code backing _PartialModalLNTransaction.cshtml
 * 
*/

$(document).ready(function () {
    $.get("/Account/GetBalance", function (data, status) {
        $('#userDepositBalance').html(data.balance);
        $(".userBalanceValue").each(function (i, e) {
            $(e).html(data.balance);
        });
    });
});

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

var onValidateInvoice = function (e) {
    var invoice = $("#lightningWithdrawInvoiceInput").val();
    headers = getAntiForgeryToken();
    $.ajax({
        type: "POST",
        url: "/Lightning/ValidatePaymentRequest",
        data: JSON.stringify({ request: invoice.toString() }),
        headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                $('#lightningInvoiceAmount').val(response.num_satoshis);
                $('#confirmWithdraw').show();
                $('#btnPayLNWithdraw').hide();
                $('#btnVerifyLNWithdraw').hide();
                $('#btnPayLNWithdraw').show();
                
                $("#lightningTransactionInvoiceResult").html("Verify amount and click Withdraw");


                console.log('Withdraw Node:' + response.destination);
            }
            else {
                // Error?
            }
        },
        failure: function (response) {
            alert(response.message);
        },
        error: function (response) {
            alert(response.message);
        }
    });

};

/**
 * Validates invoice before payment
 * @param {any} e element clicked
 */
var onPayInvoice = function (e) {
    var invoice = $("#lightningWithdrawInvoiceInput").val();
    $("#btnPayLNWithdraw").attr("disabled", "disabled");
    headers = getAntiForgeryToken();
    $.ajax({
        type: "POST",
        url: "/Lightning/SubmitPaymentRequest",
        data: JSON.stringify({ request: invoice.toString() }),
        headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            $("#btnPayLNWithdraw").removeAttr("disabled");
            $('#btnVerifyLNWithdraw').show();
            $("#btnPayLNWithdraw").hide();
            $('#confirmWithdraw').hide();
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
            $('#btnVerifyLNWithdraw').show();
            $("#btnPayLNWithdraw").hide();
            $('#confirmWithdraw').hide();
            $("#lightningTransactionInvoiceResult").html("Failed to receive invoice");
            $("#lightningTransactionInvoiceResult").removeClass("bg-success");
            $("#lightningTransactionInvoiceResult").addClass("bg-error");
            $("#lightningTransactionInvoiceResult").show();
        },
        error: function (response) {
            $("#btnPayLNWithdraw").removeAttr("disabled");
            $('#btnVerifyLNWithdraw').show();
            $("#btnPayLNWithdraw").hide();
            $('#confirmWithdraw').hide();
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
    var invoice = $("#" + $(e).data('invoice-element')).val();
    $("#" + $(e).data('spin-element')).show();

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
            $("#" + $(e).data('spin-element')).hide();
            if (response.success) {
                if (response.result === true) {
                    handleInvoicePaid(response);
                    // Payment has been successfully made
                    console.log('Payment confirmed');
                }
            }
            else {
                alert(response.message);
            }
        },
        failure: function (response) {
            $("#" + $(e).data('spin-element')).hide();
            alert(response.message);
        },
        error: function (response) {
            $("#" + $(e).data('spin-element')).hide();
            alert(response.message);
        }
    });
};

var switchWithdraw = function () {
    $('#doLightningTransactionBtn').hide();
    $('#btnCheckLNDeposit').hide();
    $('#btnVerifyLNWithdraw').show();
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
    $('#btnVerifyLNWithdraw').hide();
    $("#lightningTransactionInvoiceResult").removeClass("bg-info");
    $("#lightningTransactionInvoiceResult").removeClass("bg-error");
    $("#lightningTransactionInvoiceResult").addClass("bg-muted");
    $("#lightningTransactionInvoiceResult").removeClass("bg-success");
    $("#lightningTransactionInvoiceResult").html("Specify deposit amount to deposit and get invoice.");
};