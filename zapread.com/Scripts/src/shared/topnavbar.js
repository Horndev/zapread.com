/** 
 * Script for TopNavBar (All site content)
 *
 * [✓] TODO remove jQuery
 * 
 **/

//import $ from 'jquery';
//import "jquery-ui-dist/jquery-ui";
//import "jquery-ui-dist/jquery-ui.min.css";

import { refreshUserBalance } from '../utility/refreshUserBalance'; // [✓]
import { ready } from '../utility/ready';                           // [✓]
import { postJson } from '../utility/postData';                     // [✓]

//var toggleChat; // global function for quotable.  TODO: fix
var ub = 0;
window.ub = ub;

/**
 * Dismiss messages and alerts
 * 
 * [✓] does not use jQuery
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
                        //$('[id^="a_"]').hide();
                        //$('[id^="a1_"]').hide();
                        //$('[id^="a2_"]').hide();
                        //Array.prototype.forEach.call(document.querySelectorAll('[id^="a_"]'), function (e, _i) {
                        //    e.style.display = 'none';
                        //});
                        Array.prototype.forEach.call(document.querySelectorAll('[id^="a1_"]'), function (e, _i) {
                            e.style.display = 'none';
                        });
                        Array.prototype.forEach.call(document.querySelectorAll('[id^="a2_"]'), function (e, _i) {
                            e.style.display = 'none';
                        });
                    } else {
                        //$('#a_' + id).hide();
                        //$('#a1_' + id).hide();
                        //$('#a2_' + id).hide();
                        //document.getElementById("a_" + id).style.display = 'none';
                        document.getElementById("a1_" + id).style.display = 'none';
                        document.getElementById("a2_" + id).style.display = 'none';
                    }

                    //var urla = $("#unreadAlerts").data("url");
                    //$("#unreadAlerts").load(urla);
                    var url = document.getElementById("unreadAlerts").getAttribute('data-url');
                    fetch(url).then(function (response) {
                        return response.text();
                    }).then(function (html) {
                        document.getElementById("unreadAlerts").innerHTML = html;
                    });
                }
                else {
                    if (id === -1) { // Dismissed all
                        //$('[id^="m_"]').hide();
                        //$('[id^="m1_"]').hide();
                        //$('[id^="m2_"]').hide();
                        //Array.prototype.forEach.call(document.querySelectorAll('[id^="m_"]'), function (e, _i) {
                        //    e.style.display = 'none';
                        //});
                        Array.prototype.forEach.call(document.querySelectorAll('[id^="m1_"]'), function (e, _i) {
                            e.style.display = 'none';
                        });
                        Array.prototype.forEach.call(document.querySelectorAll('[id^="m2_"]'), function (e, _i) {
                            e.style.display = 'none';
                        });
                    } else {
                        //$('#m_' + id).hide();
                        //$('#m1_' + id).hide();
                        //$('#m2_' + id).hide();
                        //document.getElementById("m_" + id).style.display = 'none';
                        document.getElementById("m1_" + id).style.display = 'none';
                        document.getElementById("m2_" + id).style.display = 'none';
                    }
                    //var urlm = $("#unreadMessages").data("url");
                    //$("#unreadMessages").load(urlm);
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