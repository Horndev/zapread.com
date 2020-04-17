//
// script for _PartialModalVote.cshtml

function createNewEvent(eventName) {
    var event;
    if (typeof Event === 'function') {
        event = new Event(eventName);
    } else {
        event = document.createEvent('Event');
        event.initEvent(eventName, true, true);
    }
    return event;
}

var userVote = { id: 0, d: 0, t: 0, amount: 1, tx: 0, b: 0 };
var userTip = { username: "", amount: 1 };
var isTip = false;
var voteReadyEvent = createNewEvent('voteReady');//new Event('voteReady');

$(document).ready(function () {
    var userdefaultvote = '1';
    $('#payAmount').html(userdefaultvote);
    $('#voteValueAmount').val(userdefaultvote);

    var userBalance = userVote.b;
    $('#userVoteBalance').html(userBalance);

    // If the user updates the amount
    $('#voteValueAmount').on('input', function () {
        amt = $(this).val();
        userVote.amount = amt;
        userTip.amount = amt;
        if (parseInt(userVote.amount) > parseInt(userVote.b)) {
            $('#voteDepositInvoiceFooter').html('Please pay lightning invoice.');
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
            $("#voteOkButton").html('Get Invoice');
        }
        else {
            if (isTip) {
                $('#voteDepositInvoiceFooter').html("Click tip to confirm.");
                $("#voteOkButton").html('Tip');
            }
            else {
                $('#voteDepositInvoiceFooter').html("Click vote to confirm.");
                $("#voteOkButton").html('Vote');
            }
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
        }
    });
    document.dispatchEvent(voteReadyEvent);
});

