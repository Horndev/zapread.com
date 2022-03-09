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
  console.log("invoice paid");
  if (navigator.vibrate) { navigator.vibrate(300); } // vibration API supported
  var eventName = 'zapread:invoicePaid'; // default

  if (isDeposit(invoice)) {
    console.log("Deposit invoice paid");
    eventName = 'zapread:deposit:invoicePaid'
  } else if (isVote(invoice)) {
    console.log("Vote invoice paid");
    eventName = 'zapread:vote:invoicePaid'
  }

  const event = new CustomEvent(eventName, {
    detail: {
      tx: txid
    }
  });
  document.dispatchEvent(event);
  console.log('dispached: ' + eventName, txid)

  //await updateuserbalance(); // update UI
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
  var depositInvoiceEl = document.getElementById("lightningDepositInvoiceInput");
  if (depositInvoiceEl != null) {
    var depositInvoice = depositInvoiceEl.value;
    return invoice === depositInvoice;
  }
  return false;
}

function isVote(invoice) {
  var voteInvoiceEl = document.getElementById("voteDepositInvoiceInput");
  if (voteInvoiceEl != null) {
    var voteInvoice = voteInvoiceEl.value;
    return invoice === voteInvoice;
  }
  return false;
}