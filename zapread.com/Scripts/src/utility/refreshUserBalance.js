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
          spendOnlyBalance: data.spendOnlyBalance,
          quickVote: data.QuickVoteOn,
          quickVoteAmount: quickVoteAmount > 0 ? quickVoteAmount : 1
        });
      }
      return {
        balance: data.balance,
        spendOnlyBalance: data.spendOnlyBalance
      };
    });
}