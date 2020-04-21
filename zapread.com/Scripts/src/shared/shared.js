/*
 * 
 */

import '../utility/appinsights';

// yuck ...
import $ from 'jquery';
import "jquery-ui-dist/jquery-ui";
import "jquery-ui-dist/jquery-ui.min.css";

import 'popper.js';
import 'bootstrap';
import 'bootstrap/dist/css/bootstrap.min.css';
import 'font-awesome/css/font-awesome.min.css';

import '../utility/ui/vote';
import '../utility/ui/paymentsscan';
import '../utility/ui/accountpayments';

import './postfunctions';
import './readmore';
import './postui';
//import 'bootstrap-chosen/dist/chosen.jquery-1.4.2/chosen.jquery';
//import 'bootstrap-chosen/bootstrap-chosen.css';

// Toastr requires jquery (boo!) [Done: now using tata.js]
//import * as Toastr from 'toastr';
//import 'toastr/build/toastr.css';

//import './sharedlast';

//Toastr.options.closeMethod = 'fadeOut';
//Toastr.options.closeDuration = 700;
//Toastr.options.positionClass = 'toast-bottom-right';
//Toastr.options.closeEasing = 'swing';
//Toastr.options.closeButton = true;
//Toastr.options.hideMethod = 'slideUp';
//Toastr.options.progressBar = true;
//Toastr.options.timeOut = 30000; // How long the toast will display without user interaction
//Toastr.options.extendedTimeOut = 60000; // How long the toast will display after a user hovers over it
//window.Toastr = Toastr;

export function copyToClipboard(e, elemId) {
    $("#" + elemId).focus();
    $("#" + elemId).select();
    try {
        var successful = document.execCommand('copy');
        var msg = successful ? 'successful' : 'unsuccessful';
        console.log('Copying text command was ' + msg);
        $(e).html("<span class='fa fa-copy'></span> Copied");
        setTimeout(function () { $(e).html("<span class='fa fa-copy'></span> Copy"); }, 10000);
    } catch (err) {
        console.log('Oops, unable to copy');
    }
}
window.copyToClipboard = copyToClipboard;

$(document).ready(function () {
    // this could possibly be removed.
    $.fn.extend({
        placeCursorAtEnd: function () {
            // Places the cursor at the end of a contenteditable container (should also work for textarea / input)
            if (this.length === 0) {
                throw new Error("Cannot manipulate an element if there is no element!");
            }
            var el = this[0];
            var range = document.createRange();
            var sel = window.getSelection();
            var childLength = el.childNodes.length;
            if (childLength > 0) {
                var lastNode = el.childNodes[childLength - 1];
                var lastNodeChildren = lastNode.childNodes.length;
                range.setStart(lastNode, lastNodeChildren);
                range.collapse(true);
                sel.removeAllRanges();
                sel.addRange(range);
            }
            return this;
        }
    });
});


$("ul.dropdown-menu").on("click", "[data-keepOpenOnClick]", function (e) {
    e.stopPropagation();
});

$('.collapse-link').on('click', function () {
    var ibox = $(this).closest('div.ibox');
    if (typeof $(this).data('id') !== 'undefined') {
        ibox = $('#' + $(this).data('id'));
    }
    var button = $(this).find('i');
    var content = ibox.children('.ibox-content');
    content.slideToggle(200);
    button.toggleClass('fa-chevron-up').toggleClass('fa-chevron-down');
    ibox.toggleClass('').toggleClass('border-bottom');
    setTimeout(function () {
        ibox.resize();
        ibox.find('[id^=map-]').resize();
    }, 50);
});

// Close ibox function
$('.close-link').on('click', function () {
    var content = $(this).closest('div.ibox');
    content.remove();
});

$(".pop").popover({
    trigger: "manual",
    html: true,
    sanitize: false,
    animation: false
})
    .on("mouseenter", function () {
        var _this = this;
        setTimeout(function () {

            $(this).popover("show");
            $('[data-toggle="tooltip"]').tooltip();
            $(".popover").addClass("tooltip-hover");
            $(".popover").on("mouseleave", function () {
                $(_this).popover('hide');
            });
        }, 1000);
    })
    .on("mouseleave", function () {
        var _this = this;
        setTimeout(function () {
            if (!$(".popover:hover").length) {
                $(_this).popover("hide");
            }
        }, 300);
    });

// TODO - move into section specifically for loading the top bar.
$.ajax({
    type: "POST",
    url: "/Messages/CheckUnreadChats",
    data: "",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function (response) {
        if (response.success) {
            if (response.Unread > 0) {
                $("#topChat").css("color", "red");
            }
        }
        else {
            alert(response.Message);
        }
    },
    failure: function (response) {
        console.log('load more failure');
    },
    error: function (response) {
        console.log('load more error');
    }
});

$(".pop").each(function () {
    $(this).removeClass("pop");
});

$('[data-toggle="tooltip"]').tooltip();

// This loads all async partial views on page [I don't think there are any]
//$(".partialContents").each(function (index, item) {
//    var url = $(item).data("url");
//    if (url && url.length > 0) {
//        $(item).load(url);
//    }
//});