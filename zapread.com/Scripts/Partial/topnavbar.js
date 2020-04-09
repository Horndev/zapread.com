//
// Script for TopNavBar (All site content)

var toggleChat; // global function for quotable.  TODO: fix
var ub = 0;

// Used for caching scripts when loaded on-demand.
var LoadedScripts = new Array();
jQuery.getScript = function (url, callback, cache) {
    if ($.inArray(url, LoadedScripts) > -1) {
        callback();
    }
    else {
        LoadedScripts.push(url);
        jQuery.ajax({
            type: "GET",
            url: url,
            success: callback,
            dataType: "script",
            cache: cache
        });
    }
};

$(document).ready(function () {
    $.get("/Account/Balance", function (data, status) {
        // Used in vote.js (needs refactoring)
        userBalance = parseFloat(data.balance);
        $('#userVoteBalance').html(data.balance);
        ub = data.balance;
        if (typeof userVote !== 'undefined') {
            userVote.b = ub;
        }

        // Used for account-payments-ui.js  (needs refactoring)
        $('#userDepositBalance').html(data.balance);

        $(".userBalanceValue").each(function (i, e) {
            $(e).html(data.balance);
        });
    });

    var urla = $("#unreadAlerts").data("url");
    $("#unreadAlerts").load(urla);

    var urlm = $("#unreadMessages").data("url");
    $("#unreadMessages").load(urlm);

    // Textarea autoexpand
    jQuery.each(jQuery('textarea[data-autoresize]'), function () {
        var offset = this.offsetHeight - this.clientHeight;
        var resizeTextarea = function (el) {
            jQuery(el).css('height', 'auto').css('height', el.scrollHeight + offset);
        };
        jQuery(this).on('keyup input', function () { resizeTextarea(this); }).removeAttr('data-autoresize');
    });
});
