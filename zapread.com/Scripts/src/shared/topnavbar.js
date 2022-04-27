/** 
 * Script for TopNavBar (All site content)
 * 
 **/

import { refreshUserBalance } from '../utility/refreshUserBalance';
import { ready } from '../utility/ready';
import { postJson } from '../utility/postData';
const getDepositWithdrawModal = () => import("../Components/DepositWithdrawModal");

import React from "react";
import ReactDOM from "react-dom";
import UserBalance from "../Components/UserBalance";
import ZapreadSearch from "../Components/ZapreadSearch";

/* Vote Modal Component */
var userBalanceEl = document.getElementById("topUserBalance");
if (userBalanceEl != null) {
  ReactDOM.render(<UserBalance />, userBalanceEl);
}

/* Top Search */
var searchEl = document.getElementById("ZRSearch");
if (searchEl != null) {
  ReactDOM.render(<ZapreadSearch />, searchEl);
}

var DepositModalLoaded = false;

function openDepositWithdrawModal() {
  const event = new Event('zapread:depositwithdraw');
  document.dispatchEvent(event);
}

/* DepositWithdraw modal */
var topBalanceEl = document.getElementById("topBalance"); // This is only there when logged in
if (topBalanceEl != null) {
  // Attach click
  topBalanceEl.addEventListener("click", (e) => {
    // If not loaded - load
    if (!DepositModalLoaded) {
      getDepositWithdrawModal().then(({ default: DepositWithdrawModal }) => {
        ReactDOM.render(<DepositWithdrawModal />, document.getElementById("ModalDepositWithdraw"));
      }).then(() => {
        openDepositWithdrawModal();
        DepositModalLoaded = true;
      });
    } else {
      openDepositWithdrawModal();
    }
  });
}

var ub = 0;
window.ub = ub;

async function GetUnreadMessages() {
  var messagesEl = document.getElementById("RecentUnreadMessages");
  if (messagesEl != null) {
    await fetch("/Messages/RecentUnreadMessages").then(response => {
      return response.text();
    }).then(html => {
      messagesEl.innerHTML += html;
    })
  }
}
GetUnreadMessages();

async function GetUnreadAlerts() {
  var alertsEl = document.getElementById("RecentUnreadAlerts");
  if (alertsEl != null) {
    await fetch("/Messages/RecentUnreadAlerts").then(response => {
      return response.text();
    }).then(html => {

      alertsEl.innerHTML += html;
    })
  }
}
GetUnreadAlerts();

/**
 * Dismiss messages and alerts
 * 
 * @param {any} t  : type (1 = alert)
 * @param {any} id : object id
 * @returns {bool} : true on success
 */
export function dismiss(t, id) {
  var url = "";
  if (t === 1) {
    url = "/Messages/DismissAlert/";
  }
  else if (t === 0) {
    url = "/Messages/DismissMessage/";
  }

  postJson(url, { "id": id })
    .then((result) => {
      if (result.Result === "Success") {
        // Hide post
        if (t === 1) {
          if (id === -1) { // Dismissed all
            Array.prototype.forEach.call(document.querySelectorAll('[id^="a1_"]'), function (e, _i) {
              e.style.display = 'none';
            });
            Array.prototype.forEach.call(document.querySelectorAll('[id^="a2_"]'), function (e, _i) {
              e.style.display = 'none';
            });
            document.getElementById("topChat").style.color = "";
          } else {
            document.getElementById("a1_" + id).style.display = 'none';
            document.getElementById("a2_" + id).style.display = 'none';
          }

          var url = document.getElementById("unreadAlerts").getAttribute('data-url');
          fetch(url).then(function (response) {
            return response.text();
          }).then(function (html) {
            document.getElementById("unreadAlerts").innerHTML = html;
          });
        }
        else {
          if (id === -1) { // Dismissed all
            Array.prototype.forEach.call(document.querySelectorAll('[id^="m1_"]'), function (e, _i) {
              e.style.display = 'none';
            });
            Array.prototype.forEach.call(document.querySelectorAll('[id^="m2_"]'), function (e, _i) {
              e.style.display = 'none';
            });
          } else {
            document.getElementById("m1_" + id).style.display = 'none';
            document.getElementById("m2_" + id).style.display = 'none';
          }
          var urlm = document.getElementById("unreadMessages").getAttribute('data-url');
          fetch(urlm).then(function (response) {
            return response.text();
          }).then(function (html) {
            document.getElementById("unreadMessages").innerHTML = html;
          });
        }
      }
    });
  return false;
}
window.dismiss = dismiss;

ready(function () {
  refreshUserBalance();

  var alertEl = document.getElementById("unreadAlerts");
  if (alertEl !== null) {
    var url = alertEl.getAttribute('data-url');
    fetch(url).then(function (response) {
      return response.text();
    }).then(function (html) {
      alertEl.innerHTML = html;
    });
  }

  var messageEl = document.getElementById("unreadMessages");
  if (messageEl !== null) {
    var urlm = messageEl.getAttribute('data-url');
    fetch(urlm).then(function (response) {
      return response.text();
    }).then(function (html) {
      messageEl.innerHTML = html;
    });
  }
});

async function CheckChats() {
  await postJson("/Messages/CheckUnreadChats/")
    .then((response) => {
      if (response.success) {
        if (response.Unread > 0) {
          document.getElementById("topChat").style.color = "red";
        }
      }
      else {
        alert(response.message);
      }
    });
}

CheckChats();