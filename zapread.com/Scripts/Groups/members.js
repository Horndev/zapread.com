
// When user clicks to make the selected user an administrator
var toggleGroupAdmin = function (uid,gid,grant) {
    swal({
        title: "Are you sure?",
        text: "This user will have all the privilages of a group administrator.",
        type: "warning",
        buttons: true,
        showCancelButton: true,
        closeOnConfirm: false,
        closeOnCancel: false
    }).then(function (ok) {
        if (ok) {
            if (grant === 1) {
                $.post("/Group/UpdateUserMakeAdmin",
                { "id": uid, "groupId": gid },
                function (response) {
                    if (response.success) {
                        swal("User successfully made administrator.", {
                            icon: "success"
                        });
                    }
                    else {
                        swal("Error", response.message, "error");
                    }
                });
            } else {
                $.post("/Group/UpdateUserRevokeAdmin",
                { "id": uid, "groupId": gid },
                function (response) {
                    if (response.success) {
                        swal("Successfully revoked user administrator rights.", {
                            icon: "success"
                        });
                    }
                    else {
                        swal("Error", response.message, "error");
                    }
                });
            }
        } else {
            console.log("cancelled make admin");
        }
    });
};

var toggleGroupMod = function (uid, gid, grant) {
    swal({
        title: "Are you sure?",
        text: "This user will have all the privilages of a group moderator.",
        type: "warning",
        buttons: true,
        showCancelButton: true,
        closeOnConfirm: false,
        closeOnCancel: false
    }).then(function (ok) {
        if (ok) {
            if (grant === 1) {
                $.post("/Group/UpdateUserMakeMod",
                    { "id": uid, "groupId": gid },
                    function (response) {
                        if (response.success) {
                            swal("User successfully made moderator.", {
                                icon: "success"
                            });
                        }
                        else {
                            swal("Error", response.message, "error");
                        }
                    });
            } else {
                $.post("/Group/UpdateUserRevokeMod",
                    { "id": uid, "groupId": gid },
                    function (response) {
                        if (response.success) {
                            swal("Successfully revoked user moderator rights.", {
                                icon: "success"
                            });
                        }
                        else {
                            swal("Error", response.message, "error");
                        }
                    });
            }
        } else {
            console.log("cancelled make mod");
        }
    });
};