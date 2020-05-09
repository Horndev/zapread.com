/**
 *  
 **/

import { getAntiForgeryTokenValue } from './antiforgery';  // [✓]

export async function refreshUserBalance() {
    return fetch('/Account/Balance/', {
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
        var ve = document.getElementById('userVoteBalance');
        if (ve !== null) {
            ve.innerHTML = data.balance;//$('#userVoteBalance').html(data.balance);
        }

        var elements = document.querySelectorAll(".userBalanceValue");
        Array.prototype.forEach.call(elements, function (el, _i) {
            el.innerHTML = data.balance;
        });

        return data.balance;
    });
}