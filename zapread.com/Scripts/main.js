/* ZapRead global functions */

//$(document).ready(function () {
    /// vvv moved to shared.js
    // Collapse ibox function
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

    //// Close ibox function
    //$('.close-link').on('click', function () {
    //    var content = $(this).closest('div.ibox');
    //    content.remove();
    //});
    /// ^^^ moved to shared.js

    //// Set up social sharing links
    //jsSocials.shares.copy = {
    //    label: "Copy",
    //    logo: "fa fa-copy",
    //    shareUrl: "javascript:(function() { copyTextToClipboard('{url}'); return false; })()",
    //    countUrl: "",
    //    shareIn: "self"
    //};

    //$(".sharing").each(function () {
    //    $(this).jsSocials({
    //        url: $(this).data('url'),
    //        text: $(this).data('sharetext'),
    //        showLabel: false,
    //        showCount: false,
    //        shareIn: "popup",
    //        shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
    //    });
    //    $(this).removeClass("sharing");
    //});

    //toastr.options.closeMethod = 'fadeOut';
    //toastr.options.closeDuration = 700;
    //toastr.options.positionClass = 'toast-bottom-right';
    //toastr.options.closeEasing = 'swing';
    //toastr.options.closeButton = true;
    //toastr.options.hideMethod = 'slideUp';
    //toastr.options.progressBar = true;
    //toastr.options.timeOut = 30000; // How long the toast will display without user interaction
    //toastr.options.extendedTimeOut = 60000; // How long the toast will display after a user hovers over it

    
//}); // End document ready

/// vvv Moved to /src/utility/antiforgery.js
///**
// * @return {any} REST headers
// * */
//var getAntiForgeryToken = function () {
//    var form = $('#__AjaxAntiForgeryForm');
//    var token = $('input[name="__RequestVerificationToken"]', form).val();
//    var headers = {};
//    headers['__RequestVerificationToken'] = token;
//    return headers;
//};

//var getAntiForgeryTokenValue = function () {
//    var form = $('#__AjaxAntiForgeryForm');
//    var token = $('input[name="__RequestVerificationToken"]', form).val();
//    return token;
//};
/// ^^^ Moved to /src/utility/antiforgery.js

/* exported loadMoreComments */
/* exported joinGroup */
/* exported leaveGroup */
/* exported follow */

/// vvv this was used in jsSocials to copy share link to clipboard
//function fallbackCopyTextToClipboard(text) {
//    var textArea = document.createElement("textarea");
//    textArea.value = text;
//    document.body.appendChild(textArea);
//    textArea.focus();
//    textArea.select();

//    try {
//        var successful = document.execCommand('copy');
//        var msg = successful ? 'successful' : 'unsuccessful';
//        console.log('Fallback: Copying text command was ' + msg);
//        if (successful) {
//            swal("Post url copied to clipboard", {
//                icon: "success"
//            });
//        }
//        else {
//            swal("Error", "Error copying url to clipboard", "error");
//        }
//    } catch (err) {
//        console.error('Fallback: Oops, unable to copy', err);
//        swal("Error", "Error copying url to clipboard: " + err, "error");
//    }

//    document.body.removeChild(textArea);
//}

//function copyTextToClipboard(text) {
//    if (!navigator.clipboard) {
//        fallbackCopyTextToClipboard(text);
//        return;
//    }
//    navigator.clipboard.writeText(text).then(function () {
//        console.log('Async: Copying to clipboard was successful!');
//        swal("Post url copied to clipboard", {
//            icon: "success"
//        });
//    }, function (err) {
//        swal("Error", "Error copying url to clipboard: " + err, "error");
//        console.error('Async: Could not copy text: ', err);
//    });
//}