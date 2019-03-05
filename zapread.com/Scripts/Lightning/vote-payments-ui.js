
// This function is called when a user clicks the button to either pay with balance or invoice
var onVote = function (e) {
    var userBalance = userVote.b;
    var depositUse = "userDeposit";
    var memo = "ZapRead.com";
    if (isTip) {
        depositUse = "tip";
        memo = 'ZapRead.com ' + $('#voteModalTitle').html();
    } else if (userVote.t == 1) {
        depositUse = "votePost";
        memo = 'ZapRead.com vote post ID: ' + userVote.id;
    } else if (userVote.t == 2) {
        depositUse = "voteComment"
        memo = 'ZapRead.com vote comment ID: ' + userVote.id;
    }
    var isanon = '1';
    if (IsAuthenticated) {
        isanon = '0';
    }

    var msg = JSON.stringify({
        "amount": userVote.amount.toString(),
        "memo": memo,
        "anon": isanon,
        "use": depositUse,
        "useId": userVote.id,
        "useAction": userVote.d    // direction of vote 0=down; 1=up
    });

    if (parseInt(userVote.amount) > parseInt(userBalance)) {
        // Not enough funds - ask for invoice
        updateVoteInvoice(msg);
    }
    else {
        if (isTip) {
            doTip(userVote.id, userVote.amount, null);
        }
        else {
            doVote(userVote.id, userVote.d, userVote.t, userVote.amount, 0);
        }
    }
};

// This function gets an invoice and displays it to the user
var updateVoteInvoice = function (msg) {
    $.ajax({
        type: "POST",
        url: "/Lightning/GetDepositInvoice/",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            $("#voteDepositInvoiceInput").val(response.Invoice);
            $("#voteDepositInvoiceLink").attr("href", "lightning:" + response.Invoice);
            $("#voteDepositQR").attr("src", "/Img/QR?qr=" + encodeURI("lightning:" + response.Invoice));
            $("#lnDepositInvoiceLink").attr("href", "lightning:" + response.Invoice);
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
            $("#voteDepositInvoiceFooter").html("Please pay invoice.");
            $("#voteDepositInvoiceFooter").show();
            $("#voteDepositQR").show();
            $("#voteDepositInvoice").show();
            $('#voteModal').modal('show');

            // Start backup polling to see if it gets paid
            //setTimeout(function () {
            //    checkInvoicePaid(response.Id);
            //}, 5000);
        },
        failure: function (response) {
            //alert("failure " + JSON.stringify(response));
            $("#voteDepositInvoiceFooter").html("Failed to generate invoice");
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").addClass("bg-error");
            $("#voteDepositInvoiceFooter").show();
        },
        error: function (response) {
            //alert("error " + JSON.stringify(response));
            $("#voteDepositInvoiceFooter").html("Error generating invoice");
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-info");
            $("#voteDepositInvoiceFooter").addClass("bg-error");
            $("#voteDepositInvoiceFooter").show();
        }
    });
};

var onCancelVote = function (e) {

};

// User pressed vote button
// - Use modal dialog to set amount and handle LN transactions if needed
var vote = function (id, d, t, b) {
    // id : the identifier for the item being voted on
    // d  : the direction of the vote
    // t  : the type of item voted on.  (2 = comment)
    isTip = false;
    var userBalance = 0;
    var voteCost = parseInt($('#voteValueAmount').val());
    $('#voteModalTitle').html("Vote");

    $.get("/Account/GetBalance", function (data, status) {
        $('#userVoteBalance').html(data.balance);
        userBalance = parseFloat(data.balance);
        userVote.b = ub;
        $(".userBalanceValue").each(function (i, e) {
            $(e).html(data.balance);
        });

        /* Configure vote parameters */
        userVote.id = id;
        userVote.d = d;
        userVote.t = t;
        userVote.b = ub;
        userVote.amount = voteCost;

        /* This is done here prior to showing */
        if (userVote.amount > userBalance) {
            $('#voteDepositInvoiceFooter').html('Please pay lightning invoice.');
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
            $("#voteOkButton").html('Get Invoice');
        }
        else {
            $('#voteDepositInvoiceFooter').html("Click vote to confirm.");
            $("#voteDepositInvoiceFooter").removeClass("bg-success");
            $("#voteDepositInvoiceFooter").removeClass("bg-error");
            $("#voteDepositInvoiceFooter").addClass("bg-info");
            $("#voteOkButton").html('Vote');
        }

        /* Prepare vote modal without an invoice, and show it.*/
        $("#voteDepositQR").hide();
        $("#voteDepositInvoice").hide();
        $('#voteModal').modal('show');
    });
};

var doVote = function (id, d, t, amount, tx) {
    // id : the identifier for the item being voted on
    // d  : the direction of the vote
    // t  : the type of item voted on.  (2 = comment)
    // amount : the size of the vote
    var val = Number($('#sVote_' + id.toString()).html());
    var data = JSON.stringify({ 'Id': id, 'd': d, 'a': amount, 'tx': tx });
    var voteurl = '/Vote/Post';
    var uid = '#uVote_';    // element for up arrow
    var did = '#dVote_';
    var sid = '#sVote_';    // element for score

    if (t == 2) {
        voteurl = '/Vote/Comment';
        uid = '#uVotec_';
        did = '#dVotec_';
        sid = '#sVotec_';
    }

    // Do vote
    $.ajax({
        data: data.toString(),
        type: 'POST',
        url: voteurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result == "success") {
                del = Number(response.delta);
                if (del == 1) {
                    $(uid + id.toString()).removeClass("text-muted");
                    $(did + id.toString()).addClass("text-muted");
                }
                else if (del == 0) {
                    $(uid + id.toString()).addClass("text-muted");
                    $(did + id.toString()).addClass("text-muted");
                }
                else {
                    $(did + id.toString()).removeClass("text-muted");
                    $(uid + id.toString()).addClass("text-muted");
                }
                val = response.scoreStr; //Number(response.score);
                $(sid + id.toString()).html(val.toString());
                $('#voteModal').modal('hide');

                // Update user balance displays
                $.get("/Account/GetBalance", function (data, status) {
                    $(".userBalanceValue").each(function (i, e) {
                        $(e).html(data.balance);
                    });
                });
            }
            else {
                $("#voteDepositInvoiceFooter").removeClass("bg-success");
                $("#voteDepositInvoiceFooter").removeClass("bg-info");
                $("#voteDepositInvoiceFooter").addClass("bg-error");
                $("#voteDepositInvoiceFooter").html(response.message);
                $("#voteDepositInvoiceFooter").show();
            }
        }
    });
}