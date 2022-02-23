/**
 * Partial Modal for update user alias
 * 
 */
import { postJson } from "../../utility/postData";
import { Modal } from 'bootstrap.native/dist/bootstrap-native-v4';
import Swal from 'sweetalert2';

export function updateAlias() {
  event.preventDefault();
  event.stopImmediatePropagation();

  var dataval = document.getElementById('userAliasInput').value;
  postJson("/Manage/UpdateUserAlias", {
    alias: dataval
  }).then((response) => {
    if (response.success) {
      hideUpdateAliasModal();
      document.getElementById('labelUsername').innerHTML = dataval;
      document.getElementById('navbarUserName').innerHTML = dataval;
      Swal.fire("Update successful", `your alias is now ${dataval}`, "success");
    } else {
      alert(response.message);
      Swal.fire("Error", `${response.message}`, "error");
    }
  });
}
window.updateAlias = updateAlias;

function showUpdateAliasModal() {
  if (Object.prototype.hasOwnProperty.call(document.getElementById('userAliasModal'), "Modal")) {
    document.getElementById('userAliasModal').Modal.show();
  } else {
    var modalEl = document.getElementById('userAliasModal');
    var modal = new Modal(modalEl);
    modal.show();
  }
}

function hideUpdateAliasModal() {
  if (Object.prototype.hasOwnProperty.call(document.getElementById('userAliasModal'), "Modal")) {
    document.getElementById('userAliasModal').Modal.hide();
  } else {
    var modalEl = document.getElementById('userAliasModal');
    var modal = new Modal(modalEl);
    modal.hide();
  }
}

async function Initialize() {
  var btnChangeAlias = document.getElementById('btnChangeAlias');
  btnChangeAlias.onclick = () => {
    showUpdateAliasModal();
  }
}
Initialize();