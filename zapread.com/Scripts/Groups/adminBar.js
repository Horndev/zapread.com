//
// script used in _PartialGroupAdminBar.cshtml

var changeShortDesc = function (groupId) {
    swal({
        text: 'Enter new group description',
        content: "input",
        button: {
            text: "Ok",
            closeModal: false
        }
    })
    .then(desc => {
        if (!desc) throw null;
        var headers = getAntiForgeryToken();
        $.ajax({
            async: true,
            url: "/Group/ChangeShortDesc/",//"@Url.Action("ChangeShortDesc", "Group")",
            type: "POST",
            dataType: "json",
            data: { "groupId": groupId, "newDesc": desc },
            headers: headers,
            success: function (data) {
                if (data.success) {
                    swal("Your group short description has been updated!", {
                        icon: "success",
                    });
                }
                else {
                    swal("Error", "Error updating group short description: " + data.message, "error");
                }
            }
        });
    })
    .catch(err => {
        if (err) {
            swal("Error", "Error updating group short description.", "error");
        } else {
            swal.stopLoading();
            swal.close();
        }
    });
};

var changeName = function (groupId) {
    swal({
        text: 'Enter new group name',
        content: "input",
        button: {
            text: "Ok",
            closeModal: false
        }
    })
    .then(name => {
        if (!name) throw null;
        var headers = getAntiForgeryToken();
        $.ajax({
            async: true,
            url: "/Group/ChangeName/",//"@Url.Action("ChangeName", "Group")",
            type: "POST",
            dataType: "json",
            data: { "groupId": groupId, "newName": name },
            headers: headers,
            success: function (data) {
                if (data.success) {
                    swal("Your group name has been updated!", {
                        icon: "success",
                    });
                }
                else {
                    swal("Error", "Error updating group name: " + data.message, "error");
                }
            }
        });
    })
    .catch(err => {
        if (err) {
            swal("Error", "Error updating group name.", "error");
        } else {
            swal.stopLoading();
            swal.close();
        }
    });
};