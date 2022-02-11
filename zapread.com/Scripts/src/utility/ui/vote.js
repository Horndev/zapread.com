/**
 * User vote functions - controlling modal and ui interface
 * 
 **/
import Swal from 'sweetalert2';
import { Modal } from 'bootstrap.native/dist/bootstrap-native-v4';
import { getAntiForgeryTokenValue } from '../antiforgery';
import { postJson } from '../postData';
import { ready } from '../ready';

function createNewEvent(eventName) {
  var event;
  if (typeof Event === 'function') {
    event = new Event(eventName);
  } else {
    event = document.createEvent('Event');
    event.initEvent(eventName, true, true);
  }
  return event;
}

var userVote = { id: 0, d: 0, t: 0, amount: 1, tx: 0, b: 0 };
var userTip = { username: "", amount: 1 };
var isTip = false;
var voteReadyEvent = createNewEvent('voteReady');

window.userVote = userVote;
window.userTip = userTip;
window.isTip = isTip;
window.voteReadyEvent = voteReadyEvent;

ready(function () {
  var userdefaultvote = '1';
  document.getElementById('payAmount').innerHTML = userdefaultvote;
  document.getElementById('voteValueAmount').value = userdefaultvote;
  var userBalance = window.userVote.b;
  document.getElementById('userVoteBalance').innerHTML = userBalance;

  // If the user updates the amount
  var voteInput = document.getElementById('voteValueAmount');
  voteInput.addEventListener('input', function () {
    var amt = this.value;
    window.userVote.amount = amt;
    window.userTip.amount = amt;
    if (parseInt(window.userVote.amount) > parseInt(window.userVote.b)) {
      document.getElementById('voteDepositInvoiceFooter').innerHTML = 'Please pay lightning invoice.';
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success");
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-error");
      document.getElementById("voteDepositInvoiceFooter").classList.add("bg-info");
      document.getElementById("voteOkButton").innerHTML = 'Get Invoice';
    }
    else {
      if (window.isTip) {
        document.getElementById('voteDepositInvoiceFooter').innerHTML = "Click tip to confirm.";
        document.getElementById("voteOkButton").innerHTML = 'Tip';
      }
      else {
        document.getElementById('voteDepositInvoiceFooter').innerHTML = "Click vote to confirm.";
        document.getElementById("voteOkButton").innerHTML = 'Vote';
      }
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success");
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-error");
      document.getElementById("voteDepositInvoiceFooter").classList.add("bg-info");
    }
  });
  document.dispatchEvent(voteReadyEvent);
});

/**
 * This function is called when a user clicks the button to either pay with balance or invoice
 * 
 * @param {any} e
 */
export function onVote(e) {
  var userBalance = userVote.b;
  var depositUse = "userDeposit";
  var memo = "ZapRead.com";
  if (window.isTip) {
    depositUse = "tip";
    memo = 'ZapRead.com ' + $('#voteModalTitle').html();
  } else if (userVote.t === 1) {
    depositUse = "votePost";
    memo = 'ZapRead.com vote post ID: ' + userVote.id;
  } else if (userVote.t === 2) {
    depositUse = "voteComment";
    memo = 'ZapRead.com vote comment ID: ' + userVote.id;
  }
  var isanon = '1';
  if (IsAuthenticated) {
    isanon = '0';
  }
  else {
    appInsights.trackEvent({
      name: 'Anonymous Vote',
      properties: {
        amount: userVote.amount.toString()
      }
    });
  }

  if (parseInt(userVote.amount) > parseInt(userBalance)) {
    // Not enough funds - ask for invoice
    updateVoteInvoice({
      "amount": userVote.amount.toString(),
      "memo": memo,
      "anon": isanon,
      "use": depositUse,
      "useId": userVote.id,
      "useAction": userVote.d    // direction of vote 0=down; 1=up
    });
    document.getElementById('voteOkButton').style.display = "none"; // hide
    document.getElementById('btnCheckLNVote').style.display = "";   // show
  }
  else {
    if (window.isTip) {
      doTip(userVote.id, userVote.amount, null);
    }
    else {
      /* Set chevron spinning */
      var icon = userVote.o.querySelectorAll('i').item(0);
      icon.classList.remove('fa-chevron-up');
      icon.classList.add('fa-circle-o-notch');
      icon.classList.add('fa-spin');
      icon.style.color = 'darkcyan';
      doVote(userVote.id, userVote.d, userVote.t, userVote.amount, 0);
    }
  }
}
window.onVote = onVote;

/**
 * This function gets an invoice and displays it to the user
 * 
 * @param {any} msg
 */
export function updateVoteInvoice(msg) {
  document.getElementById("voteQRloading").style.display = '';

  postJson("/Lightning/GetDepositInvoice/", msg)
    .then((response) => {
      if (response.success) {
        document.getElementById("voteDepositInvoiceInput").value = response.Invoice;
        document.getElementById("lnDepositInvoiceLink").setAttribute("href", "lightning:" + response.Invoice);
        document.getElementById("lnDepositInvoiceImgLink").setAttribute("href", "lightning:" + response.Invoice);
        document.getElementById("voteDepositQR").setAttribute("src", "/Img/QR?qr=" + encodeURI("lightning:" + response.Invoice));
        document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success", "bg-error");
        document.getElementById("voteDepositInvoiceFooter").classList.add("bg-info");
        document.getElementById("voteDepositInvoiceFooter").innerHTML = "Please pay invoice.";
        document.getElementById("voteDepositInvoiceFooter").style.display = '';
        document.getElementById("voteDepositQR").style.display = '';
        document.getElementById("voteDepositInvoice").style.display = '';
        document.getElementById("voteQRloading").style.display = 'none';
      }
      else {
        document.getElementById("voteDepositInvoiceFooter").innerHTML = response.message;
        document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success", "bg-info");
        document.getElementById("voteDepositInvoiceFooter").classList.add("bg-error");
        document.getElementById("voteDepositInvoiceFooter").style.display = '';
        document.getElementById("voteQRloading").style.display = 'none';
      }
    })
    .then(() => {
      showVoteModal();
    })
    .catch((error) => {
      console.log(error);
      document.getElementById("voteDepositInvoiceFooter").innerHTML = "Error generating invoice";
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success", "bg-info");
      document.getElementById("voteDepositInvoiceFooter").classList.add("bg-error");
      document.getElementById("voteDepositInvoiceFooter").style.display = '';
      document.getElementById("voteQRloading").style.display = 'none';
    });
}
window.updateVoteInvoice = updateVoteInvoice;

/**
 * 
 * @param {any} e
 */
export function onCancelVote(e) {
  document.getElementById('voteOkButton').style.display = '';
  document.getElementById('btnCheckLNVote').style.display = 'none';
  document.getElementById("voteDepositInvoiceFooter").style.display = 'none';
  document.getElementById("voteDepositQR").style.display = 'none';
  document.getElementById("voteDepositInvoice").style.display = 'none';
  document.getElementById("voteQRloading").style.display = 'none';
}
window.onCancelVote = onCancelVote;

/**
 * User pressed vote button
 * - Use modal dialog to set amount and handle LN transactions if needed
 * 
 * @param {any} id
 * @param {any} d
 * @param {any} t
 * @param {any} b
 * @param {any} o
 */
export function vote(id, d, t, b, o) {
  // id : the identifier for the item being voted on
  // d  : the direction of the vote
  // t  : the type of item voted on.  (2 = comment)
  // o  : the object calling vote
  window.isTip = false;
  var userBalance = 0;
  var voteCost = parseInt(document.getElementById('voteValueAmount').value);

  /* Configure vote parameters */
  userVote.b = ub;
  userVote.id = id;
  userVote.d = d;
  userVote.t = t;
  userVote.b = ub;
  userVote.o = o;     /* Track the calling object */
  userVote.amount = voteCost;

  if (!IsAuthenticated) {
    Swal.fire({
      icon: 'info',
      title: 'Anonymous Vote',
      text: 'You are not logged in, but you can still vote anonymously with a Bitcoin Lightning Payment.',
      footer: '<a href="/Account/Login">Log in instead</a>'
    }).then(() => {
      prepareAndShowVoteModal();
    });
  }
  else {
    prepareAndShowVoteModal();
  }
}

window.vote = vote;

function prepareAndShowVoteModal() {
  /* Prepare vote modal without an invoice, and show it.*/
  document.getElementById('voteModalTitle').innerHTML = "Vote";
  document.getElementById('userVoteBalance').innerHTML = "...";
  document.getElementById('voteDepositInvoiceFooter').classList.remove("bg-success");
  document.getElementById('voteDepositInvoiceFooter').classList.remove("bg-error");
  document.getElementById('voteDepositInvoiceFooter').classList.add("bg-info");
  document.getElementById('voteOkButton').innerHTML = "Vote";
  document.getElementById('voteDepositInvoiceFooter').innerHTML = "Click vote to confirm.";
  document.getElementById('voteDepositQR').style.display = 'none';
  document.getElementById('voteDepositInvoice').style.display = 'none';
  document.getElementById("voteQRloading").style.display = 'none';
  showVoteModal();
  refreshUserBalance().then((userBalance) => {
    /* This is done here prior to showing */
    if (userVote.amount > userBalance) {
      document.getElementById('voteDepositInvoiceFooter').innerHTML = "Please pay lightning invoice.";
      document.getElementById('voteOkButton').innerHTML = "Get Invoice";
    }
  });
}

/**
 * [✓]
 **/
async function refreshUserBalance() {
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
      document.getElementById('userVoteBalance').innerHTML = data.balance;//$('#userVoteBalance').html(data.balance);

      if (typeof userBalance !== 'undefined') {
        userBalance = parseFloat(data.balance);
      } else if (Object.prototype.hasOwnProperty.call(window, "userBalance")) {
        window.userBalance = parseFloat(data.balance);
      } else {
        window.userBalance = parseFloat(data.balance);
      }

      if (Object.prototype.hasOwnProperty.call(window, "userVote")) {
        window.userVote.b = parseFloat(data.balance);
      }

      var elements = document.querySelectorAll(".userBalanceValue");
      Array.prototype.forEach.call(elements, function (el, _i) {
        el.innerHTML = data.balance;
      });

      return data.balance;
    });
}

/**
 * [✓]
 **/
function showVoteModal() {
  if (Object.prototype.hasOwnProperty.call(document.getElementById('voteModal'), "Modal")) {
    document.getElementById('voteModal').Modal.show();
  } else {
    var voteModalEl = document.getElementById('voteModal');
    var voteModal = new Modal(voteModalEl);//.Modal;
    voteModal.show();
  }
}

/**
 * [✓]
 **/
function hideVoteModal() {
  var voteModalEl = document.getElementById('voteModal');
  var voteModal = voteModalEl.Modal;//new Modal(voteModalEl);//.Modal;
  voteModal.hide();
}

/**
 * 
 * [✓] does not use jQuery
 * 
 * @param {any} id      the identifier for the item being voted on
 * @param {any} d       the direction of the vote
 * @param {any} t       the type of item voted on.  (2 = comment)
 * @param {any} amount  the size of the vote
 * @param {any} tx
 */
export function doVote(id, d, t, amount, tx) {
  //var val;// = Number(document.getElementById('sVote_' + id.toString()).innerHTML);//$('#sVote_' + id.toString()).html());
  var body = { 'Id': id, 'd': d, 'a': amount, 'tx': tx };
  var voteurl = '/Vote/Post';
  var uid = 'uVote_';    // element for up arrow
  var did = 'dVote_';
  var sid = 'sVote_';    // element for score

  if (t === 2) {
    voteurl = '/Vote/Comment';
    uid = 'uVotec_';
    did = 'dVotec_';
    sid = 'sVotec_';
  }

  hideVoteModal();//$('#voteModal').modal('hide');

  fetch(voteurl, {
    method: 'POST', // *GET, POST, PUT, DELETE, etc.
    mode: 'same-origin', // no-cors, *cors, same-origin
    cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
    credentials: 'same-origin', // include, *same-origin, omit
    headers: {
      'Content-Type': 'application/json',
      '__RequestVerificationToken': getAntiForgeryTokenValue()
    },
    body: JSON.stringify(body)
  })
    .then((response) => {
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        var icon = userVote.o.querySelectorAll('i').item(0);
        //var icon = $(userVote.o).find('i');
        icon.classList.remove('fa-circle-o-notch');
        icon.classList.remove('fa-spin');
        icon.classList.add('fa-chevron-up');
        icon.style.color = '';//('color', '');

        var delta = Number(data.delta);
        if (delta === 1) {
          document.getElementById(uid + id.toString()).classList.remove("text-muted");
          document.getElementById(did + id.toString()).classList.add("text-muted");
        }
        else if (delta === 0) {
          document.getElementById(uid + id.toString()).classList.add("text-muted");
          document.getElementById(did + id.toString()).classList.add("text-muted");
        }
        else {
          document.getElementById(did + id.toString()).classList.remove("text-muted");
          document.getElementById(uid + id.toString()).classList.add("text-muted");
        }
        var val = data.scoreStr;
        document.getElementById(sid + id.toString()).innerHTML = val.toString();

        refreshUserBalance();
      }
      else {
        showVoteModal();

        document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success");
        document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-info");
        document.getElementById("voteDepositInvoiceFooter").classList.add("bg-error");
        document.getElementById("voteDepositInvoiceFooter").innerHTML = data.message;
        document.getElementById("voteDepositInvoiceFooter").style.display = '';
      }
    });
}
window.doVote = doVote;

/**
 * User tip
 * 
 * [✓] does not use jQuery
 * 
 * @param {any} user name of user
 * @param {any} uid  id of user
 */
export function tip(user, uid) {
  alert("tips disabled.");
  return;
  window.isTip = true;
  document.getElementById('voteModalTitle').innerHTML = "Tip " + user;

  refreshUserBalance().then((userBalance) => {
    document.getElementById('userVoteBalance').innerHTML = userBalance;
    userVote.id = uid;

    /* This is done here prior to showing */
    if (userVote.amount > userBalance) {
      document.getElementById('voteDepositInvoiceFooter').innerHTML = "Please pay lightning invoice.";
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success");
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-error");
      document.getElementById("voteDepositInvoiceFooter").classList.add("bg-info");
      document.getElementById("voteOkButton").innerHTML = "Get Invoice";
    }
    else {
      document.getElementById('voteDepositInvoiceFooter').innerHTML = "Click tip to confirm.";
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success");
      document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-error");
      document.getElementById("voteDepositInvoiceFooter").classList.add("bg-info");
      document.getElementById("voteOkButton").innerHTML = "Tip";
    }

    /* Prepare vote modal without an invoice, and show it.*/
    document.getElementById("voteDepositQR").style.display = "none";
    document.getElementById("voteDepositInvoice").style.display = "none";

    showVoteModal();
  });
}
window.tip = tip;

/**
 * 
 * @param {any} id      the user receiving the tip
 * @param {any} amount  the amount of the tip
 * @param {any} tx      txid if the tip is anonymous
 */
export function doTip(id, amount, tx) {
  var body = { 'id': id, 'amount': parseInt(amount), 'tx': tx };

  fetch('/Manage/TipUser/', {
    method: 'POST', // *GET, POST, PUT, DELETE, etc.
    mode: 'same-origin', // no-cors, *cors, same-origin
    cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
    credentials: 'same-origin', // include, *same-origin, omit
    headers: {
      'Content-Type': 'application/json',
      '__RequestVerificationToken': getAntiForgeryTokenValue()
    },
    body: JSON.stringify(body)
  })
    .then((response) => {
      return response.json();
    })
    .then((data) => {
      if (data.success) {
        hideVoteModal();

        refreshUserBalance();
      }
      else {
        document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-success");
        document.getElementById("voteDepositInvoiceFooter").classList.remove("bg-info");
        document.getElementById("voteDepositInvoiceFooter").classList.add("bg-error");
        document.getElementById("voteDepositInvoiceFooter").innerHTML = data.Message;
        document.getElementById("voteDepositInvoiceFooter").class.display = '';
      }
    });

  window.isTip = false;
}
window.doTip = doTip;