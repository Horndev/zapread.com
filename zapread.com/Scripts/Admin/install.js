/**/

var grantSiteAdmin = function () {
    var msg = JSON.stringify({
        'adminKey': $('#ZRAdminKey').val(),
        'grantUser': $('#Username').val()
    });

    var form = $('#__AjaxAntiForgeryForm');
    var token = $('input[name="__RequestVerificationToken"]', form).val();
    var headers = {};
    headers['__RequestVerificationToken'] = token;

    $.ajax({
        async: true,
        type: "POST",
        url: "Home/Install/GrantAdmin/",
        data: msg,
        headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                alert("Successfully saved.");
            }
        },
        failure: function (response) {
            alert("Error saving.");
        },
        error: function (response) {
            alert("Error saving.");
        }
    });
};