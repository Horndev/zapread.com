/*
 * 
 */
navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;

//import "signalr/jquery.signalR";

import { hubConnection } from 'signalr-no-jquery';

const connection = hubConnection("/signalr", { useDefaultPath: false });
const hubProxy = connection.createHubProxy('notificationHub');

// add hub listeners here.
//SendUserMessage
//NotifyInvoicePaid

hubProxy.on('SendUserMessage', function (message) {
    console.log(JSON.stringify(message));
});


console.log('connecting signalr...');
connection.start({ waitForPageLoad: false},
    function () {
        var cn = this;
        window.addEventListener("beforeunload", function () {
            cn.stop();
        });
    })
    .done(function () {
        console.log("Hub Connected!, transport = " + hubProxy.transport.name);
    })
    .fail(function () {
        console.log("Could not Connect!");
    });