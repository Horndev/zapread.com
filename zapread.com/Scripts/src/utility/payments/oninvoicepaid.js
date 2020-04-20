/**
 * Handle invoicepayment being received
 **/

/**
 * Handle notification that an invoice was paid for either an 
 * anonymous vote action, or deposit.
 *
 * @param {string} invoice The invoice string
 * @param {string} balance New user's balance (if deposit)
 * @param {number} txid Transaction identifier (for vote)
 **/
export function oninvoicepaid(invoice, balance, txid) {
    if (isDeposit(invoice)) {
        console.log("Deposit invoice paid");

        $("#lightningDepositInvoiceResult").html("Successfully received deposit.");
        $("#lightningDepositInvoiceResult").removeClass("bg-error");
        $("#lightningDepositInvoiceResult").removeClass("bg-info");
        $("#lightningDepositInvoiceResult").removeClass("bg-muted");
        $("#lightningDepositInvoiceResult").addClass("bg-success");
        $("#getInvoice").html("Get Invoice");    // Change button text from get invoice to update
        $("#lightningDepositInvoiceResult").show();
        $("#lightningDepositQR").hide();
        $("#lightningDepositInvoice").hide();

        if (navigator.vibrate) { navigator.vibrate(300); }

        $(".userBalanceValue").each(function (i, e) {
            $(e).html(invoiceResponse.balance);
        });

        $(".partialContents").each(function (index, item) {
            var url = $(item).data("url");
            if (url && url.length > 0 && url === "/Account/Balance") {
                $(item).load(url);
            }
        });

        ub = invoiceResponse.balance; // Update global var

        var userBalance = ub;
        $('#userDepositBalance').html(userBalance);
        $('#userVoteBalance').html(userBalance);
        $.get("/Account/Balance", function (data, status) {
            $(".userBalanceValue").each(function (i, e) {
                $(e).html(data.balance);
            });
        });

        $('#depositModal').modal('hide');

        try {
            lightningTable.ajax.reload(null, false);
        }
        catch (err) {
            console.log("couldn't refresh lightningTable");
        }
    }
    else if (isVote(invoice)) {
        console.log("Vote invoice paid");
        // Ok, the user paid the invoice.  Now we need to claim the vote.
        // If this transaction id is not found, or already claimed, the vote will not work.
        userVote.tx = invoiceResponse.txid;

        if (navigator.vibrate) {
            // vibration API supported
            navigator.vibrate(300);
        }

        if (isTip) {
            console.log('tip paid');
            doTip(userVote.id, userVote.amount, userVote.tx);
        }
        else {
            doVote(userVote.id, userVote.d, userVote.t, userVote.amount, userVote.tx);
        }

        $('#voteOkButton').show();
        $('#btnCheckLNVote').hide();
    }
}

function isDeposit(invoice) {
    var depositInvoice = document.getElementById("lightningDepositInvoiceInput").value;
    return invoice === depositInvoice;
}

function isVote(invoice) {
    var voteInvoice = document.getElementById("voteDepositInvoiceInput").value;
    return invoice === voteInvoice;
}