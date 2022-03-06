/**
 * Common shared imports across ZapRead
 * 
 **/

import '../utility/appinsights';                    // [✓]

/**
 * Bootstrap
 * 
 * Here, we use Bootstrap.native, which does not require jQuery.  It is much lighter, and supports
 * up to version 4.  The Bootstrap css is still imported from the bootstrap distribution.
 **/
//import 'bootstrap';
import 'bootstrap.native/dist/bootstrap-native-v4';
import 'bootstrap/dist/css/bootstrap.min.css';
//import 'font-awesome/css/font-awesome.min.css';
//import '@fortawesome/fontawesome-free/css/fontawesome.min.css';
import '@fortawesome/fontawesome-free/css/all.min.css';
import '@fortawesome/fontawesome-free/css/v4-shims.min.css';
import '../utility/ui/paymentsscan';
import '../utility/ui/accountpayments';
import './topnavbar';
import '../css/quill/quillfont.css';

/**
 * 
 * @param {any} e
 * @param {any} elemId
 */
export function copyToClipboard(e, elemId) {
  var inputEl = document.getElementById(elemId);
  inputEl.focus();
  inputEl.select();
  inputEl.setSelectionRange(0, 99999);
  navigator.clipboard
    .writeText(inputEl.value)
    .then(() => {
      console.log("successfully copied");
    })
    .catch(() => {
      console.log("something went wrong");
    });

  // Create an event to select the contents (native js)
  var event = document.createEvent('HTMLEvents');
  event.initEvent('select', true, false);
  inputEl.dispatchEvent(event);

  try {
    var successful = document.execCommand('copy');
    //var msg = successful ? 'successful' : 'unsuccessful';
    //console.log('Copying text command was ' + msg);
    e.innerHTML = "<span class='fa fa-copy'></span> Copied";
    setTimeout(function () { e.innerHTML = "<span class='fa fa-copy'></span> Copy"; }, 10000);
  } catch (err) {
    //console.log('Oops, unable to copy');
  }
}
window.copyToClipboard = copyToClipboard;

var elements = document.querySelectorAll("ul.dropdown-menu");
Array.prototype.forEach.call(elements, function (el, _i) {
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
      ibox = document.getElementById(el.getAttribute('data-id'));
    }
    var button = el.querySelectorAll('i').item(0);
    var content = ibox.querySelectorAll('.ibox-content').item(0);
    if (content.style.display !== 'block') {
      content.style.display = 'block';
    } else {
      content.style.display = 'none';
    }
    button.classList.toggle('fa-chevron-up');
    button.classList.toggle('fa-chevron-down');
    ibox.classList.toggle('border-bottom');
    setTimeout(function () {
      var event = document.createEvent('HTMLEvents');
      event.initEvent('resize', true, false);
      ibox.dispatchEvent(event);
      var mp = ibox.querySelectorAll('[id^=map-]').item(0);
      if (mp !== null) { mp.dispatchEvent(event); }
    }, 50);
  });
});

// [✓] no jQuery
elements = document.querySelectorAll(".close-link");
Array.prototype.forEach.call(elements, function (el, _i) {
  el.addEventListener("click", function (e) {
    var content = el.closest('div.ibox');
    content.remove();
  });
});
