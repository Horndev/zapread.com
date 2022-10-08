

import '../../shared/shared';
navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;
import * as signalR from "@microsoft/signalr";
import { getAntiForgeryToken } from '../../utility/antiforgery';
import '../../shared/sharedlast';
import { onlnauthlogin } from '../../realtime/auth/onlnauthlogin';
import { ready } from '../../utility/ready';
var connection;

function connectStream(url, k1) {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(url) // Connect using authorization token (unique to client)
    .configureLogging(signalR.LogLevel.Warning)
    .build();

  // Connect message handlers
  connection.on("LnAuthLogin", onlnauthlogin);
  connection.on("ReceiveMessage", function (userId, message) {
    // Don't handle this yet.
    //console.log("ReceiveMessage", userId, message);
  });
  //console.log('connecting...(' + k1 + ')' );
  connection.start().then(function () {
    console.log('Connected!');
  }).catch(function (err) {
    return console.error(err.toString());
  });

  window.connection = connection; // Is this needed?
}

async function getstream() {
  const response = await fetch("/api/v1/stream/request/" + k1 + "/");
  const json = await response.json();
  if (json.success) {
    const url = json.url;
    connectStream(url, k1);
  } else {
    console.log("Streaming connection not established.");
  }
}

ready(() => {
  getstream();

  appInsights.trackEvent({
    name: 'lnurl-auth login start',
  });
  appInsights.flush(); // send now
});