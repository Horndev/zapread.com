//
// script used in _PartialGroupAdminBar.cshtml
import Swal from 'sweetalert2';
import { getAntiForgeryToken } from '../../utility/antiforgery';

export function changeShortDesc(groupId) {
    Swal.fire({
        text: 'Enter new group description',
        input: 'text',
        inputValue: '',
        showCancelButton: true
    })
    .then(desc => {
        if (!desc.value) throw null;
        $.ajax({
            async: true,
            url: "/Group/ChangeShortDesc/",//"@Url.Action("ChangeShortDesc", "Group")",
            type: "POST",
            dataType: "json",
            data: { "groupId": groupId, "newDesc": desc.value },
            headers: getAntiForgeryToken(),
            success: function (data) {
                if (data.success) {
                    Swal.fire("Your group short description has been updated!", {
                        icon: "success"
                    });
                }
                else {
                    Swal.fire("Error", "Error updating group short description: " + data.message, "error");
                }
            }
        });
    })
    .catch(err => {
        if (err) {
            Swal.fire("Error", "Error updating group short description.", "error");
        } else {
            Swal.stopLoading();
            Swal.close();
        }
    });
}
window.changeShortDesc = changeShortDesc;

export function changeName(groupId) {
    Swal.fire({
        text: 'Enter new group name',
        input: 'text',
        inputValue: '',
        showCancelButton: true
    })
    .then(name => {
        if (!name.value) throw null;
        $.ajax({
            async: true,
            url: "/Group/ChangeName/",//"@Url.Action("ChangeName", "Group")",
            type: "POST",
            dataType: "json",
            data: { "groupId": groupId, "newName": name.value },
            headers: getAntiForgeryToken(),
            success: function (data) {
                if (data.success) {
                    Swal.fire("Your group name has been updated!", {
                        icon: "success",
                    });
                }
                else {
                    Swal.fire("Error", "Error updating group name: " + data.message, "error");
                }
            }
        });
    })
    .catch(err => {
        if (err) {
            Swal.fire("Error", "Error updating group name.", "error");
        } else {
            Swal.stopLoading();
            Swal.close();
        }
    });
}
window.changeName = changeName;