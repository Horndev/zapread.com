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

// User tip
// user = name of user
// uid = id of user
var tip = function (user, uid) {
    isTip = true;
    $('#voteModalTitle').html("Tip " + user);
    $.get("/Account/Balance", function (data, status) {
        $('#userVoteBalance').html(data.balance);
        userBalance = parseFloat(data.balance);
        $(".userBalanceValue").each(function (i, e) {
            $(e).html(data.balance);
        });
        userVote.id = uid;

        /* This is done here prior to showing */
        if (userVote.amount > userBalance) {
            $('#voteDepositInvoiceFooter').html('Please pay lightning invoice.');
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
            $("#voteOkButton").html('Get Invoice');
        }
        else {
            $('#voteDepositInvoiceFooter').html("Click tip to confirm.");
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
            $("#voteOkButton").html('Tip');
        }

        /* Prepare vote modal without an invoice, and show it.*/
        $("#voteDepositQR").hide();
        $("#voteDepositInvoice").hide();
        $('#voteModal').modal('show');
    });
};

// id : the user receiving the tip
// amount : the amount of the tip
// tx : txid if the tip is anonymous
var doTip = function (id, amount, tx) {
    var data = JSON.stringify({ 'id': id, 'amount': parseInt(amount), 'tx': tx });
    var url = '/Manage/TipUser';
    var headers = getAntiForgeryToken();
    console.log(data);
    $.ajax({
        data: data.toString(),
        type: 'POST',
        url: url,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: headers,
        success: function (response) {
            if (response.Result === "Success") {
                $('#voteModal').modal('hide');
                // Update user balance displays
                $.get("/Account/Balance", function (data, status) {
                    $(".userBalanceValue").each(function (i, e) {
                        $(e).html(data.balance);
                    });
                });
            }
            else {
                $("#voteDepositInvoiceFooter").removeClass("bg-success");
                $("#voteDepositInvoiceFooter").removeClass("bg-info");
                $("#voteDepositInvoiceFooter").addClass("bg-error");
                $("#voteDepositInvoiceFooter").html(Result.Message);
                $("#voteDepositInvoiceFooter").show();
            }
        }
    });
    isTip = false;
};
