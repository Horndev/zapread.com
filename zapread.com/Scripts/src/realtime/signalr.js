/*
 * 
 */
navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;

//import "signalr/jquery.signalR";
import * as signalR from "@microsoft/signalr";
import Swal from 'sweetalert2'

function connectStream(url, token) {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`${url}/notificationHub?a=${token}`) // Connect using quthorization token (unique to client)
        .build();

    console.log('connecting...');
    connection.start().then(function () {
        console.log('Connected!');
    }).catch(function (err) {
        return console.error(err.toString());
    });
}

async function getstream() {
    const response = await fetch("/api/v1/stream/request/");
    const json = await response.json();
    if (json.success) {
        const url = json.url;
        connectStream(url, "ABC123");
    } else {
        Swal.fire({
            icon: "error",
            title: `Error revoking key: ${json.message}`
        });
    }
}

getstream();

//import { hubConnection } from 'signalr-no-jquery';

//const connection = hubConnection("/signalr", { useDefaultPath: false });
//const hubProxy = connection.createHubProxy('notificationHub');

//// add hub listeners here.
////SendUserMessage
////NotifyInvoicePaid

//hubProxy.on('SendUserMessage', function (message) {
//    console.log(JSON.stringify(message));
//});


//console.log('connecting signalr...');
//connection.start({ waitForPageLoad: false},
//    function () {
//        var cn = this;
//        window.addEventListener("beforeunload", function () {
//            cn.stop();
//        });
//    })
//    .done(function () {
//        console.log("Hub Connected!, transport = " + hubProxy.transport.name);
//    })
//    .fail(function () {
//        console.log("Could not Connect!");
//    });