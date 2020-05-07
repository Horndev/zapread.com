/**
 * Common shared imports across ZapRead
 * 
 * [✓] Does not use jQuery
 * [  ] Todo - remove all code and dependencies using jQuery so we can remove it.
 * 
 **/

import '../utility/appinsights';

/**
 * jQuery - we don't want this.
 **/

//import $ from 'jquery';
//import "jquery-ui-dist/jquery-ui";
//import "jquery-ui-dist/jquery-ui.min.css";

/**
 * Bootstrap
 * 
 * Here, we use Bootstrap.native, which does not require jQuery.  It is much lighter, and supports
 * up to version 4.  The Bootstrap css is still imported from the bootstrap distribution.
 **/
//import 'bootstrap';
import 'bootstrap.native/dist/bootstrap-native-v4'  // [✓]
import 'bootstrap/dist/css/bootstrap.min.css';      // [✓]
import 'font-awesome/css/font-awesome.min.css';     // [✓]
import '../utility/ui/paymentsscan';                // [  ]
import '../utility/ui/accountpayments';             // [  ]
import './postfunctions';                           // [✓]
import './readmore';                                // [✓]
import './postui';                                  // [✓]
import './topnavbar';                               // [✓]

/**
 * 
 * @param {any} e
 * @param {any} elemId
 */
export function copyToClipboard(e, elemId) {
    document.getElementById(elemId).focus();            // $("#" + elemId).focus();
    document.getElementById(elemId).trigger("select"); // $("#" + elemId).select();
    
    try {
        var successful = document.execCommand('copy');
        var msg = successful ? 'successful' : 'unsuccessful';
        console.log('Copying text command was ' + msg);
        e.innerHTML = "<span class='fa fa-copy'></span> Copied";
        setTimeout(function () { e.innerHTML="<span class='fa fa-copy'></span> Copy"; }, 10000);
    } catch (err) {
        console.log('Oops, unable to copy');
    }
}
window.copyToClipboard = copyToClipboard;


//$("ul.dropdown-menu").on("click", "[data-keepOpenOnClick]", function (e) {
//    e.stopPropagation();
//});
// *** replaced with:
// $(document).on(eventName, elementSelector, handler);

// [✓] no jQuery
var elements = document.querySelectorAll("ul.dropdown-menu");
Array.prototype.forEach.call(elements, function (el, _i) {
    //console.log('add keepOpenOnClick');
    //console.log(el);
    el.addEventListener("click", function (e) {
        // loop parent nodes from the target to the delegation node
        for (var target = e.target; target && target !== this; target = target.parentNode) {
            if (target.matches("[data-keepOpenOnClick]")) {
                //handler.call(target, e);
                e.stopPropagation();
                break;
            }
        }
    }, false);
});

// Collapse button
// [✓] no jQuery
elements = document.querySelectorAll(".collapse-link");
Array.prototype.forEach.call(elements, function (el, _i) {
    el.addEventListener("click", function (e) {
        var ibox = el.closest('div.ibox');
        if (el.getAttribute('data-id') !== null) {
            ibox = document.getElementById(el.getAttribute('data-id'));//$('#' + $(this).data('id'));
        }
        var button = el.querySelectorAll('i').item(0);//$(this).find('i');
        var content = ibox.querySelectorAll('.ibox-content').item(0);//.children('.ibox-content');
        //content.slideToggle(200);
        //console.log(content);
        //console.log(content.style.display);
        if (content.style.display !== 'block') {
            content.style.display = 'block';
        } else {
            content.style.display = 'none';
        }
        button.classList.toggle('fa-chevron-up');
        button.classList.toggle('fa-chevron-down');
        //button.toggleClass('fa-chevron-up').toggleClass('fa-chevron-down');
        ibox.classList.toggle('border-bottom');
        //ibox.classList.toggle('collapsed');
        //ibox.toggleClass('').toggleClass('border-bottom');
        setTimeout(function () {
            var event = document.createEvent('HTMLEvents');
            event.initEvent('resize', true, false);
            ibox.dispatchEvent(event);
            //ibox.resize();
            var mp = ibox.querySelectorAll('[id^=map-]').item(0);
            if (mp !== null) { mp.dispatchEvent(event); }//.resize();
        }, 50);
    });
});

//$('.collapse-link').on('click', function () {
//    var ibox = $(this).closest('div.ibox');
//    if (typeof $(this).data('id') !== 'undefined') {
//        ibox = $('#' + $(this).data('id'));
//    }
//    var button = $(this).find('i');
//    var content = ibox.children('.ibox-content');
//    content.slideToggle(200);
//    button.toggleClass('fa-chevron-up').toggleClass('fa-chevron-down');
//    ibox.toggleClass('').toggleClass('border-bottom');
//    setTimeout(function () {
//        ibox.resize();
//        ibox.find('[id^=map-]').resize();
//    }, 50);
//});

// Close ibox function [X] no jQuery

//$('.close-link').on('click', function () {
//    var content = $(this).closest('div.ibox');
//    content.remove();
//});

// [✓] no jQuery
elements = document.querySelectorAll(".close-link");
Array.prototype.forEach.call(elements, function (el, _i) {
    el.addEventListener("click", function (e) {
        var content = el.closest('div.ibox');
        content.remove();
    });
});

// [ ] TODO - verify replaced with boostrap native tooltip
//$('[data-toggle="tooltip"]').tooltip();
