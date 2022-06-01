/**
 *
 **/

import { getAntiForgeryTokenValue } from './antiforgery';  // [✓]
import { updateUserInfo } from './userInfo';

export async function refreshUserBalance(update = true) {
  return await fetch('/Account/Balance/', {
    method: 'GET', // *GET, POST, PUT, DELETE, etc.
    mode: 'same-origin', // no-cors, *cors, same-origin
    cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
    credentials: 'same-origin', // include, *same-origin, omit
    headers: {
      'Content-Type': 'application/json',
      '__RequestVerificationToken': getAntiForgeryTokenValue()
    }
  })
    .then((response) => {
      return response.json();
    })
    .then((data) => {
      if (update) {
        var quickVoteAmount = parseInt(data.QuickVoteAmount);
        updateUserInfo({
          balance: data.balance,
          quickVote: data.QuickVoteOn,
          quickVoteAmount: quickVoteAmount > 0 ? quickVoteAmount : 1
        });
      }

      //var ve = document.getElementById('userVoteBalance');
      //if (ve !== null) {
      //  ve.innerHTML = data.balance;
      //}

      // This is a shim for updating all UI occurances of user balance
      //var elements = document.querySelectorAll(".userBalanceValue");
      //Array.prototype.forEach.call(elements, function (el, _i) {
      //  el.innerHTML = data.balance;
      //});

      return data.balance;
    });
}