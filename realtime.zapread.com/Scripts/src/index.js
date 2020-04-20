import React from 'react';
import ReactDOM from 'react-dom';

import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub?a=ABC123") // Connect using quthorization token (unique to client)
    .build();

connection.on("ReceiveMessage", function (userId, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = userId + " says " + msg;
    console.log(encodedMsg);
    //var li = document.createElement("li");
    //li.textContent = encodedMsg;
    //document.getElementById("messagesList").appendChild(li);
});

console.log('connecting...');
connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
    console.log('Connected!');
}).catch(function (err) {
    return console.error(err.toString());
});

const App = () => <div>Hello world!</div>;

ReactDOM.render(<App />, document.getElementById("root"));