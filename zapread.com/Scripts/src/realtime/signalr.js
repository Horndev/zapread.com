/**
 * 
 */
navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;

//import * as signalR from "@microsoft/signalr";
const getSignalR = () => import('@microsoft/signalr');
import { onchatreceived } from './chat/onchatreceived'
import { onusermessage } from './notification/onusermessage'
import { onpayment } from './notification/onpayment'

var connection;

function connectStream(url) {
  getSignalR().then(({ HubConnectionBuilder, LogLevel }) => {
    connection = new HubConnectionBuilder()
      .withUrl(url) // Connect using authorization token (unique to client)
      .configureLogging(LogLevel.Warning)
      .build();

    // Connect message handlers
    connection.on("UserChat", onchatreceived);
    connection.on("Payment", onpayment);
    connection.on("UserMessage", onusermessage);
    connection.on("ReceiveMessage", function (userId, message) {
      // Don't handle this yet.
    });

    console.log('connecting...');
    connection.start().then(function () {
      console.log('Connected!');
    }).catch(function (err) {
      return console.error(err.toString());
    });

    window.connection = connection; // Is this needed?
  });
}

async function getstream() {
    const response = await fetch("/api/v1/stream/request");
    const json = await response.json();
    if (json.success) {
        const url = json.url;
        connectStream(url);
    } else {
        console.log("Streaming connection not established.");
    }
}

// execute connection as soon as loaded
getstream();