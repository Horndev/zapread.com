/**
 * Handle signalR message that a Lightning invoice was paid
 **/

import { oninvoicepaid } from '../../utility/payments/oninvoicepaid'

/**
 * Handle the notification that a payment has completed.
 *
 * @param {string} invoice The invoice (string) which was paid.
 * @param {number} balance The new user balance (if deposit).
 * @param {number} txid The transaction identifier, which is primarily used for anonymous votes.
 **/
export function onpayment(invoice, balance, txid) {
    // Here, we forward to the ui handler
    oninvoicepaid(invoice, balance, txid);
}