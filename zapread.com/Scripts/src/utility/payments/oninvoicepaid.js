/**
 * Handle invoicepayment being received
 * 
 * [✓] Native JS
 **/

import { updateuserbalance } from '../ui/updateuserbalance'         // [✓]
import { Modal } from 'bootstrap.native/dist/bootstrap-native-v4'   // [✓]
import { updateUserInfo } from '../userInfo';

/**
 * Handle notification that an invoice was paid for either an 
 * anonymous vote action, or deposit.
 * 
 * [✓] Native JS implementation
 *
 * @param {string} invoice The invoice string
 * @param {string} balance New user's balance (if deposit)
 * @param {number} txid Transaction identifier (for vote)
 **/
export async function oninvoicepaid(invoice, balance, txid) {
  if (isDeposit(invoice)) {
    console.log("Deposit invoice paid");
    var resultElement = document.getElementById("lightningTransactionInvoiceResult");
    resultElement.innerHTML = "Successfully received deposit.";
    resultElement.classList.remove("bg-error");
    resultElement.classList.remove("bg-info");
    resultElement.classList.remove("bg-muted");
    resultElement.classList.add("bg-success");

    document.getElementById("lightningTransactionInvoiceResult").style.display = '';

    document.getElementById("doLightningTransactionBtn").style.display = '';
    document.getElementById("btnVerifyLNWithdraw").style.display = 'none';
    document.getElementById("btnCheckLNDeposit").style.display = 'none';

    document.getElementById("lightningDepositQR").style.display = 'none';
    document.getElementById("lightningDepositInvoice").style.display = 'none';

    if (navigator.vibrate) { navigator.vibrate(300); }

    //var elements = document.querySelectorAll(".userBalanceValue");
    //Array.prototype.forEach.call(elements, function (el, _i) {
    //  el.innerHTML = balance.toString();
    //});

    updateUserInfo({
      balance: balance
    });

    // TODO: remove the next code block using ub to use now userInfo
    if (typeof lighubtningTable !== 'undefined') {
      ub = balance; // Update global var
    } else {
      console.log('ub not defined');
      var ub = balance;
      window.ub = ub;
    }

    document.getElementById("userDepositBalance").innerHTML = balance.toString();

    //document.getElementById("userVoteBalance").innerHTML = balance.toString();

    await updateuserbalance(); // update UI

    //$('#depositModal').modal('hide'); 
    hidePaymentModal();

    if (typeof lightningTable !== 'undefined') {
      try {
        lightningTable.ajax.reload(null, false);
      }
      catch (err) {
        console.log("couldn't refresh lightningTable");
      }
    }
  }
  else if (isVote(invoice)) {
    console.log("Vote invoice paid");
    // Ok, the user paid the invoice.  Now we need to claim the vote.
    // If this transaction id is not found, or already claimed, the vote will not work.
    //userVote.tx = txid;

    if (navigator.vibrate) {
      // vibration API supported
      navigator.vibrate(300);
    }

    const event = new CustomEvent('zapread:vote:invoicePaid', {
      detail: {
        tx: txid
      }
    });
    document.dispatchEvent(event);

    //if (isTip) {
    //  console.log('tip paid');
    //  doTip(userVote.id, userVote.amount, userVote.tx);
    //}
    //else {
    //  doVote(userVote.id, userVote.d, userVote.t, userVote.amount, userVote.tx);
    //}

    //document.getElementById("voteOkButton").style.display = '';
    //document.getElementById("btnCheckLNVote").style.display = 'none';
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

function isDeposit(invoice) {
  var depositInvoice = document.getElementById("lightningDepositInvoiceInput").value;
  return invoice === depositInvoice;
}

function isVote(invoice) {
  var voteInvoice = document.getElementById("voteDepositInvoiceInput").value;
  return invoice === voteInvoice;
}