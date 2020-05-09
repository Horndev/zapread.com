/** 
 * Script for TopNavBar (All site content)
 *
 * [✓] TODO remove jQuery
 * 
 **/
// 

//import $ from 'jquery';
//import "jquery-ui-dist/jquery-ui";
//import "jquery-ui-dist/jquery-ui.min.css";

import { refreshUserBalance } from '../utility/refreshUserBalance'; // [✓]
import { ready } from '../utility/ready';                           // [✓]
import { postJson } from '../utility/postData';                     // [✓]

//var toggleChat; // global function for quotable.  TODO: fix
var ub = 0;
window.ub = ub;

// Used for caching scripts when loaded on-demand.
var LoadedScripts = new Array();
window.LoadedScripts = LoadedScripts;

////jQuery.getScript = function (url, callback, cache) {
////    if ($.inArray(url, LoadedScripts) > -1) {
////        callback();
////    }
////    else {
////        LoadedScripts.push(url);
////        jQuery.ajax({
////            type: "GET",
////            url: url,
////            success: callback,
////            dataType: "script",
////            cache: cache
////        });
////    }
////};

ready(function () {
    refreshUserBalance();

    //if ($("#unreadAlerts").length) {
    //    var urla = $("#unreadAlerts").data("url");
    //    $("#unreadAlerts").load(urla);
    //}
    var url = document.getElementById("unreadAlerts").getAttribute('data-url');
    fetch(url).then(function (response) {
        return response.text();
    }).then(function (html) {
        document.getElementById("unreadAlerts").innerHTML = html;
    });

    //if ($("#unreadMessages").length) {
    //    var urlm = $("#unreadMessages").data("url");
    //    $("#unreadMessages").load(urlm);
    //}
    var urlm = document.getElementById("unreadMessages").getAttribute('data-url');
    fetch(urlm).then(function (response) {
        return response.text();
    }).then(function (html) {
        document.getElementById("unreadMessages").innerHTML = html;
    });

    //// Textarea autoexpand
    //jQuery.each(jQuery('textarea[data-autoresize]'), function () {
    //    var offset = this.offsetHeight - this.clientHeight;
    //    var resizeTextarea = function (el) {
    //        jQuery(el).css('height', 'auto').css('height', el.scrollHeight + offset);
    //    };
    //    jQuery(this).on('keyup input', function () { resizeTextarea(this); }).removeAttr('data-autoresize');
    //});
});

// [X] TODO - move into section specifically for loading the top bar.
postJson("/Messages/CheckUnreadChats/")
    .then((response) => {
        if (response.success) {
            if (response.Unread > 0) {
                document.getElementById("topChat").style.color = "red"; //$("#topChat").css("color", "red");
            }
        }
        else {
            alert(response.Message);
        }
    });

//$.ajax({
//    type: "POST",
//    url: "/Messages/CheckUnreadChats",
//    data: "",
//    contentType: "application/json; charset=utf-8",
//    dataType: "json",
//    success: function (response) {
//        if (response.success) {
//            if (response.Unread > 0) {
//                $("#topChat").css("color", "red");
//            }
//        }
//        else {
//            alert(response.Message);
//        }
//    },
//    failure: function (response) {
//        console.log('load more failure');
//    },
//    error: function (response) {
//        console.log('load more error');
//    }
//});