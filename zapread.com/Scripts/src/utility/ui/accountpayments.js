/** 
 * Code backing _PartialModalLNTransaction.cshtml
 * 
 * [✓] Native JS
 * 
 **/

import Swal from 'sweetalert2';                                     // [✓]
import { getAntiForgeryToken } from '../antiforgery';               // [✓]
import { oninvoicepaid } from '../payments/oninvoicepaid'           // [✓]
import { postJson } from '../postData';                             // [✓]
import { Modal } from 'bootstrap.native/dist/bootstrap-native-v4'   // [✓]
import { refreshUserBalance } from '../refreshUserBalance';         // [✓]

/**
 * Set up the deposit invoice and UI (QR code)
 * 
 * [✓] Native JS
 * 
 * @param {Element} e
 */
export function onGetInvoice(e) {
    document.getElementById("btnCheckLNDeposit").style.display = '';
    document.getElementById("doLightningTransactionBtn").style.display = 'none';
    var amount = document.getElementById("depositValueAmount").value;
    postJson("/Lightning/GetDepositInvoice/", {
        "amount": amount.toString(),
        "memo": 'ZapRead.com deposit',
        "anon": '0',
        "use": "userDeposit",
        "useId": -1,           // undefined
        "useAction": -1        // undefined
    })
    .then((response) => {
        document.getElementById("lightningDepositInvoiceInput").value = response.Invoice;
        document.getElementById("lightningDepositInvoiceLink").setAttribute("href", "lightning:" + response.Invoice);
        document.getElementById("lightningDepositQR").setAttribute("src", "/Img/QR?qr=" + encodeURI("lightning:" + response.Invoice));
        document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-success", "bg-error", "bg-muted");
        document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-info");
        document.getElementById("lightningTransactionInvoiceResult").innerHTML = "Please pay invoice to complete your deposit";
        document.getElementById("lightningTransactionInvoiceResult").style.display = '';
        document.getElementById("lightningDepositQR").style.display = '';
        document.getElementById("lightningDepositInvoice").style.display = '';
    })
    .catch((error) => {
        document.getElementById("lightningTransactionInvoiceResult").innerHTML = "Failed to generate invoice";
        document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-success", "bg-error", "bg-muted");
        document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-error");
        document.getElementById("lightningTransactionInvoiceResult").style.display = '';
    });
}
window.onGetInvoice = onGetInvoice;

/**
 * Submit the deposit invoice and validate the contents
 * 
 * [✓] Native JS
 * 
 * @param {Element} e
 */
export function onValidateInvoice(e) {
    var invoice = document.getElementById("lightningWithdrawInvoiceInput").value;//$("#lightningWithdrawInvoiceInput").val();
    postJson("/Lightning/ValidatePaymentRequest/", {
        request: invoice.toString()
    })
    .then((response) => {
        if (response.success) {
            document.getElementById("lightningInvoiceAmount").value = response.num_satoshis;//$('#lightningInvoiceAmount').val(response.num_satoshis);
            document.getElementById("confirmWithdraw").style.display = '';//$('#confirmWithdraw').show();
            document.getElementById("btnPayLNWithdraw").style.display = 'none';//$('#btnPayLNWithdraw').hide();
            document.getElementById("btnVerifyLNWithdraw").style.display = 'none';//$('#btnVerifyLNWithdraw').hide();
            document.getElementById("btnPayLNWithdraw").style.display = '';//$('#btnPayLNWithdraw').show();
            document.getElementById("lightningTransactionInvoiceResult").innerHTML = "Verify amount and click Withdraw";//$("#lightningTransactionInvoiceResult").html("Verify amount and click Withdraw");
            console.log('Withdraw Node:' + response.destination);
        }
        else {
            Swal.fire("Error", response.message, "error");
        }
    })
    .catch((error) => {
        Swal.fire("Error", error, "error");
    });
}
window.onValidateInvoice = onValidateInvoice;

/**
 * Validates invoice before payment
 * 
 * [✓] Native JS
 * 
 * @param {any} e element clicked
 */
export function onPayInvoice(e) {
    var invoice = document.getElementById("lightningWithdrawInvoiceInput").value;//$("#lightningWithdrawInvoiceInput").val();
    document.getElementById("btnPayLNWithdraw").disabled = true;//$("#btnPayLNWithdraw").attr("disabled", "disabled");

    postJson("/Lightning/SubmitPaymentRequest/", {
        request: invoice.toString()
    })
    .then((response) => {
        document.getElementById("btnPayLNWithdraw").disabled = false;//$("#btnPayLNWithdraw").removeAttr("disabled");
        document.getElementById("btnVerifyLNWithdraw").style.display = '';//$('#btnVerifyLNWithdraw').show();
        document.getElementById("btnPayLNWithdraw").style.display = 'none';//$("#btnPayLNWithdraw").hide();
        document.getElementById("confirmWithdraw").style.display = 'none';//$('#confirmWithdraw').hide();
        if (response.success) {
            document.getElementById("lightningTransactionInvoiceResult").innerHTML = "Payment successfully sent";//$("#lightningTransactionInvoiceResult").html("Payment successfully sent.");
            document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-info", "bg-error", "bg-muted");//$("#lightningTransactionInvoiceResult").removeClass("bg-info bg-error bg-muted");
            document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-success");//$("#lightningTransactionInvoiceResult").addClass("bg-success");
            document.getElementById("lightningTransactionInvoiceResult").style.display = '';//$("#lightningTransactionInvoiceResult").show();
            refreshUserBalance();
            hidePaymentModal();//$('#depositModal').modal('hide');
        }
        else {
            document.getElementById("lightningTransactionInvoiceResult").innerHTML = response.Result;//$("#lightningTransactionInvoiceResult").html(response.Result);
            document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-info", "bg-success", "bg-muted");//$("#lightningTransactionInvoiceResult").removeClass("bg-success bg-muted bg-info");
            document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-error");//$("#lightningTransactionInvoiceResult").addClass("bg-error");
            document.getElementById("lightningTransactionInvoiceResult").style.display = '';
        }
    })
    .catch((error) => {
        document.getElementById("btnPayLNWithdraw").disabled = false;//$("#btnPayLNWithdraw").removeAttr("disabled");
        document.getElementById("btnVerifyLNWithdraw").style.display = '';//$('#btnVerifyLNWithdraw').show();
        document.getElementById("btnPayLNWithdraw").style.display = 'none';//$("#btnPayLNWithdraw").hide();
        document.getElementById("confirmWithdraw").style.display = 'none';//$('#confirmWithdraw').hide();
        document.getElementById("lightningTransactionInvoiceResult").innerHTML = "Failed to receive invoice";//$("#lightningTransactionInvoiceResult").html("Failed to receive invoice");
        document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-success");//$("#lightningTransactionInvoiceResult").removeClass("bg-success");
        document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-error");//$("#lightningTransactionInvoiceResult").addClass("bg-error");
        document.getElementById("lightningTransactionInvoiceResult").style.display = '';//$("#lightningTransactionInvoiceResult").show();
    });
}
window.onPayInvoice = onPayInvoice;

/**
 * [✓]
 **/
function showPaymentModal() {
    if (Object.prototype.hasOwnProperty.call(document.getElementById('paymentsModal'), "Modal")) {
        document.getElementById('paymentsModal').Modal.show();
    } else {
        var ModalEl = document.getElementById('paymentsModal');
        var ModalObj = new Modal(ModalEl);//.Modal;
        ModalObj.show();
    }
}

/**
 * [✓]
 **/
function hidePaymentModal() {
    if (Object.prototype.hasOwnProperty.call(document.getElementById('paymentsModal'), "Modal")) {
        document.getElementById('paymentsModal').Modal.hide();
    } else {
        var ModalEl = document.getElementById('paymentsModal');
        var ModalObj = new Modal(ModalEl);//.Modal;
        ModalObj.hide();
    }
}

/**
 * Resets the LN deposit/withdraw invoice
 * 
 * [✓] Native JS
 * 
 * @param {any} e button element which clicked
 */
export function onCancelDepositWithdraw(e) {
    document.getElementById("btnCheckLNDeposit").style.display = 'none';//$("#btnCheckLNDeposit").hide();
    document.getElementById("doLightningTransactionBtn").style.display = '';//$("#doLightningTransactionBtn").show();
    document.getElementById("lightningTransactionInvoiceResult").style.display = 'none';//$("#lightningTransactionInvoiceResult").hide();
    document.getElementById("lightningDepositQR").style.display = 'none';//$("#lightningDepositQR").hide();
    document.getElementById("lightningDepositInvoice").style.display = 'none';//$("#lightningDepositInvoice").hide();
}
window.onCancelDepositWithdraw = onCancelDepositWithdraw;

/**
 * Check if the LN invoice was paid
 * 
 * [✓] Native JS
 * 
 * @param {any} e Element calling the function
 */
export function checkInvoicePaid(e) {    
    var invoice = document.getElementById(e.getAttribute("data-invoice-element")).value; //var invoice = $("#" + $(e).data('invoice-element')).val();
    var spinElId = e.getAttribute("data-spin-element");
    document.getElementById(spinElId).style.display = ""; //$("#" + $(e).data('spin-element')).show();

    postJson("/Lightning/CheckPayment/", {
        invoice: invoice.toString(),
        isDeposit: true
    })
    .then((response) => {
        document.getElementById(spinElId).style.display = "none";//$("#" + $(e).data('spin-element')).hide();
        if (response.success) {
            if (response.result === true) {
                oninvoicepaid(response.invoice, response.balance, response.txid);
                //handleInvoicePaid(response);
                // Payment has been successfully made
                console.log('Payment confirmed');
            }
        }
        else {
            alert(response.message);
        }
    })
    .catch((error) => {
        document.getElementById(spinElId).style.display = "none";//$("#" + $(e).data('spin-element')).hide();
        alert(response.message);
    });
}
window.checkInvoicePaid = checkInvoicePaid;

/**
 * User clicked the tab to switch the dialog to withdraw funds
 * 
 * [✓] does not use jQuery
 * 
 **/
export function switchWithdraw() {
    document.getElementById("doLightningTransactionBtn").style.display = "none"; //$('#doLightningTransactionBtn').hide();
    document.getElementById("btnCheckLNDeposit").style.display = "none"; //$('#btnCheckLNDeposit').hide();
    document.getElementById("btnVerifyLNWithdraw").style.display = ""; //$('#btnVerifyLNWithdraw').show();
    document.getElementById("lightningTransactionInvoiceResult").style.display = ""; //$("#lightningTransactionInvoiceResult").show();
    document.getElementById("lightningDepositQR").style.display = "none"; //$("#lightningDepositQR").hide();
    document.getElementById("lightningDepositInvoice").style.display = "none"; //$("#lightningDepositInvoice").hide();
    var resultFooter = document.getElementById("lightningTransactionInvoiceResult");
    resultFooter.classList.remove("bg-info", "bg-error", "bg-success"); //$("#lightningTransactionInvoiceResult").removeClass("bg-info bg-error bg-success");
    resultFooter.classList.add("bg-muted"); //$("#lightningTransactionInvoiceResult").addClass("bg-muted");
    resultFooter.innerHTML = "Paste invoice to withdraw"; //$("#lightningTransactionInvoiceResult").html("Paste invoice to withdraw");
}
window.switchWithdraw = switchWithdraw;

/**
 * User clicked the tab to switch the dialog to withdraw funds
 *
 * [✓] does not use jQuery
 *
 **/
export function switchDeposit() {
    document.getElementById("doLightningTransactionBtn").style.display = ""; //$('#doLightningTransactionBtn').show();
    document.getElementById("btnCheckLNDeposit").style.display = "none"; //$('#btnCheckLNDeposit').hide();
    document.getElementById("btnPayLNWithdraw").style.display = "none"; //$('#btnPayLNWithdraw').hide();
    document.getElementById("btnVerifyLNWithdraw").style.display = "none"; //$('#btnVerifyLNWithdraw').hide();
    var resultFooter = document.getElementById("lightningTransactionInvoiceResult");
    resultFooter.classList.remove("bg-info", "bg-error", "bg-success"); //$("#lightningTransactionInvoiceResult").removeClass("bg-info bg-error bg-success");
    resultFooter.classList.add("bg-muted"); //$("#lightningTransactionInvoiceResult").addClass("bg-muted");
    resultFooter.innerHTML = "Specify deposit amount to deposit and get invoice"; //$("#lightningTransactionInvoiceResult").html("Specify deposit amount to deposit and get invoice.");
}
window.switchDeposit = switchDeposit;