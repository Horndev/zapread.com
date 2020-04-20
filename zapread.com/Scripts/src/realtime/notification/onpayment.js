/**
 * 
 **/

import { oninvoicepaid } from '../../utility/payments/oninvoicepaid'

export function onpayment(invoice, balance, txid) {
    oninvoicepaid(invoice, balance, txid);
}