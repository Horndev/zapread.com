/**
 * Handle invoicepayment being received
 * 
 **/

/**
 * Handle notification that an invoice was paid for either an 
 * anonymous vote action, or deposit.
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