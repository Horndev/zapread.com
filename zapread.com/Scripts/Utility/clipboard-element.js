var copyToClipboard = function (e, elemId) {
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