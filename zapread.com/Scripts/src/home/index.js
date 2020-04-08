/*
 * 
 */
const Swal = require('sweetalert2');
const Globals = require('./globals').default;
const addposts = require('../utility/loadmore').default;

var request = new XMLHttpRequest();
request.open('GET', '/Home/TopPosts/?sort=' + postSort, true);

request.onload = function () {
    var resp = this.response;
    //console.log(response);
    if (this.status >= 200 && this.status < 400) {
        // Success!
        response = JSON.parse(resp);
        if (response.success) {
            // Insert posts
            document.querySelectorAll('#posts').item(0).querySelectorAll('.ibox-content').item(0).classList.remove("sk-loading");
            addposts(response, zrOnLoadedMorePosts); //TODO: zrOnLoadedMorePosts uses jquery
            document.querySelectorAll('#btnLoadmore').item(0).style.display = ''; //$('#btnLoadmore').show();
        } else {
            // Did not work
            Swal.fire("Error", "Error loading posts: " + response.message, "error");
        }
    } else {
        response = JSON.parse(resp);
        // We reached our target server, but it returned an error
        Swal.fire("Error", "Error loading posts (status ok): " + response.message, "error");
    }
};

request.onerror = function () {
    // There was a connection error of some sort
    var response = JSON.parse(this.response);
    swal("Error", "Error requesting posts: " + response.message, "error");
};

request.send();

// This was re-written to not use jquery
//$.ajax({
//    async: true,
//    data: { "sort": postSort },
//    type: 'GET',
//    url: '/Home/TopPosts/',
//    contentType: "application/json; charset=utf-8",
//    dataType: "json",
//    success: function (response) {
        
//    },
//    failure: function (response) {
//        swal("Error", "Failure loading posts: " + response.message, "error");
//    },
//    error: function (response) {
//        swal("Error", "Error loading posts: " + response.message, "error");
//    }
//});

module.exports = {
    Globals,
    addposts
};

