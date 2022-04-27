/**
 * Partial Modal for update user alias
 * 
 */
import { postJson } from "../../utility/postData";
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';

async function Initialize() {
  var btnChangeAlias = document.getElementById('btnChangeAlias');
  btnChangeAlias.onclick = () => {
    getSwal().then(({ default: Swal }) => {
      Swal.fire({
        title: "Change your alias",
        input: "text",
        inputAttributes: {
          autocapitalize: 'off'
        },
        showCancelButton: true,
        showLoaderOnConfirm: true,
        preConfirm: (dataval) => {
          return postJson("/Manage/UpdateUserAlias", {
            alias: dataval
          });
        },
        allowOutsideClick: () => !Swal.isLoading()
      }).then((result) => {
        if (result.isConfirmed) {
          if (result.value.success) {
            document.getElementById('labelUsername').innerHTML = result.value.newName;
            document.getElementById('navbarUserName').innerHTML = result.value.newName;
            Swal.fire("Update successful", `your alias is now ${result.value.newName}`, "success");
          } else {
            Swal.fire("Error", `${response.value.message}`, "error");
          }
        }
      });
    });
  }
}
Initialize();