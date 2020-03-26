

var deletea = function (t, id) {
    var url = "";
    if (t === 1) {
        url = "/Messages/DeleteAlert";
    }
    else if (t === 0) {
        url = "/Messages/DeleteMessage";
    }

    $.ajax({
        async: true,
        type: "POST",
        url: url,
        data: JSON.stringify({ "id": id }),
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (result) {
            if (result.Result === "Success") {
                if (t === 1) {
                    if (id === -1) { // Deleted all
                        $('[id^="a_"]').hide();
                    } else {
                        $('#a_' + id).hide();
                        $('#a1_' + id).hide();
                        $('#a2_' + id).hide();
                    }
                }
                else {
                    if (id === -1) { // Deleted all
                        $('[id^="m_"]').hide();
                    } else {
                        $('#m_' + id).hide();
                        $('#m1_' + id).hide();
                        $('#m2_' + id).hide();
                    }
                }
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            alert("fail");
        }
    });
    return false;
};

var ignore = function (t, id) {
    if (t === 1) {
        $('#a_' + id).hide();
    }
    else if (t === 0) {
        $('#m_' + id).hide();
    }
    return false;
};