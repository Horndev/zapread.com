/**
 * Common shared imports across ZapRead
 **/

import '../utility/appinsights';

/**
 * jQuery - we don't want this.
 * 
 * [ ] Todo - remove all code and dependencies using jQuery so we can remove it.
 **/
import $ from 'jquery';
import "jquery-ui-dist/jquery-ui";
import "jquery-ui-dist/jquery-ui.min.css";

/**
 * Bootstrap
 * 
 * Here, we use Bootstrap.native, which does not require jQuery.  It is much lighter, and supports
 * up to version 4.  The Bootstrap css is imported from the bootstrap distribution.
 **/
//import 'bootstrap';
import 'bootstrap.native/dist/bootstrap-native-v4'
import 'bootstrap/dist/css/bootstrap.min.css';
import 'font-awesome/css/font-awesome.min.css';

import '../utility/ui/vote';
import '../utility/ui/paymentsscan';
import '../utility/ui/accountpayments';

import './postfunctions';
import './readmore';
import './postui';


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

$('[data-toggle="tooltip"]').tooltip();
