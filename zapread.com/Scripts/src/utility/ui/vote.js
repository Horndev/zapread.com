
// This function is called when a user clicks the button to either pay with balance or invoice
export function onVote(e) {
    var userBalance = userVote.b;
    var depositUse = "userDeposit";
    var memo = "ZapRead.com";
    if (isTip) {
        depositUse = "tip";
        memo = 'ZapRead.com ' + $('#voteModalTitle').html();
    } else if (userVote.t === 1) {
        depositUse = "votePost";
        memo = 'ZapRead.com vote post ID: ' + userVote.id;
    } else if (userVote.t === 2) {
        depositUse = "voteComment";
        memo = 'ZapRead.com vote comment ID: ' + userVote.id;
    }
    var isanon = '1';
    if (IsAuthenticated) {
        isanon = '0';
    }
    else {
        console.log('Anonymous vote.');
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
        $('#voteOkButton').hide();
        $('#btnCheckLNVote').show();
    }
    else {
        if (isTip) {
            doTip(userVote.id, userVote.amount, null);
        }
        else {
            /* Set chevron spinning */
            //console.log('doVote');
            //console.log(userVote);
            var icon = $(userVote.o).find('i');
            icon.removeClass('fa-chevron-up');
            icon.addClass('fa-circle-o-notch');
            icon.addClass('fa-spin');
            icon.css('color', 'darkcyan');
            doVote(userVote.id, userVote.d, userVote.t, userVote.amount, 0);
        }
    }
}
window.onVote = onVote;

// This function gets an invoice and displays it to the user
export function updateVoteInvoice(msg) {
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
}
window.updateVoteInvoice = updateVoteInvoice;

export function onCancelVote(e) {
    $('#voteOkButton').show();
    $('#btnCheckLNVote').hide();
    $("#voteDepositInvoiceFooter").hide();
    $("#voteDepositQR").hide();
    $("#voteDepositInvoice").hide();
}
window.onCancelVote = onCancelVote;

// User pressed vote button
// - Use modal dialog to set amount and handle LN transactions if needed
export function vote(id, d, t, b, o) {
    // id : the identifier for the item being voted on
    // d  : the direction of the vote
    // t  : the type of item voted on.  (2 = comment)
    // o  : the object calling vote
    isTip = false;
    var userBalance = 0;
    var voteCost = parseInt($('#voteValueAmount').val());

    /* Configure vote parameters */
    userVote.b = ub;
    userVote.id = id;
    userVote.d = d;
    userVote.t = t;
    userVote.b = ub;
    userVote.o = o;     /* Track the calling object */
    userVote.amount = voteCost;

    /* Prepare vote modal without an invoice, and show it.*/
    $('#voteModalTitle').html("Vote");
    $('#userVoteBalance').html("...");
    $("#voteDepositInvoiceFooter").removeClass("bg-success");
    $("#voteDepositInvoiceFooter").removeClass("bg-error");
    $("#voteDepositInvoiceFooter").addClass("bg-info");
    $("#voteOkButton").html('Vote');
    $('#voteDepositInvoiceFooter').html("Click vote to confirm.");
    $("#voteDepositQR").hide();
    $("#voteDepositInvoice").hide();
    $('#voteModal').modal('show');

    $.get("/Account/Balance", function (data, status) {
        $('#userVoteBalance').html(data.balance);
        userBalance = parseFloat(data.balance);
        $(".userBalanceValue").each(function (i, e) {
            $(e).html(data.balance);
        });

        /* This is done here prior to showing */
        if (userVote.amount > userBalance) {
            $('#voteDepositInvoiceFooter').html('Please pay lightning invoice.');
            $("#voteOkButton").html('Get Invoice');
        }
    });
}
window.vote = vote;

export function doVote(id, d, t, amount, tx) {
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

    if (t === 2) {
        voteurl = '/Vote/Comment';
        uid = '#uVotec_';
        did = '#dVotec_';
        sid = '#sVotec_';
    }

    $('#voteModal').modal('hide');

    var form = $('#__AjaxAntiForgeryFormVote');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    // Do vote
    $.ajax({
        data: data.toString(),
        type: 'POST',
        url: voteurl,
        headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result === "success") {
                var icon = $(userVote.o).find('i');
                icon.removeClass('fa-circle-o-notch');
                icon.removeClass('fa-spin');
                icon.addClass('fa-chevron-up');
                icon.css('color', '');
                var delta = Number(response.delta);
                if (delta === 1) {
                    $(uid + id.toString()).removeClass("text-muted");
                    $(did + id.toString()).addClass("text-muted");
                }
                else if (delta === 0) {
                    $(uid + id.toString()).addClass("text-muted");
                    $(did + id.toString()).addClass("text-muted");
                }
                else {
                    $(did + id.toString()).removeClass("text-muted");
                    $(uid + id.toString()).addClass("text-muted");
                }
                val = response.scoreStr;
                $(sid + id.toString()).html(val.toString());

                // Update user balance displays
                $.get("/Account/Balance", function (data, status) {
                    $(".userBalanceValue").each(function (i, e) {
                        $(e).html(data.balance);
                    });
                });
            }
            else {
                $('#voteModal').modal('show');
                $("#voteDepositInvoiceFooter").removeClass("bg-success");
                $("#voteDepositInvoiceFooter").removeClass("bg-info");
                $("#voteDepositInvoiceFooter").addClass("bg-error");
                $("#voteDepositInvoiceFooter").html(response.message);
                $("#voteDepositInvoiceFooter").show();
            }
        }
    });
}
window.doVote = doVote;