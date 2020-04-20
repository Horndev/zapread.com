/**
 * 
 */
navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;

//import "signalr/jquery.signalR";  // Got rid of this!
import * as signalR from "@microsoft/signalr";

import { onchatreceived } from './chat/onchatreceived'
import { onusermessage } from './notification/onusermessage'
import { onpayment } from './notification/onpayment'

var connection;

function connectStream(url) {
    connection = new signalR.HubConnectionBuilder()
        .withUrl(url) // Connect using authorization token (unique to client)
        .build();

    // Connect message handlers
    connection.on("UserChat", onchatreceived);
    connection.on("Payment", onpayment);
    connection.on("UserMessage", onusermessage);

    console.log('connecting...');
    connection.start().then(function () {
        console.log('Connected!');
    }).catch(function (err) {
        return console.error(err.toString());
    });

    window.connection = connection; // Is this needed?
}

async function getstream() {
    const response = await fetch("/api/v1/stream/request/");
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