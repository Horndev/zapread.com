/**
 * Partial Modal for update user alias
 * 
 */
import { postJson } from "../../utility/postData";
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';

async function Initialize() {
  var btnChangeAlias = document.getElementById('btnChangeAlias');
  if (btnChangeAlias) {
    btnChangeAlias.onclick = () => {
      getSwal().then(({ default: Swal }) => {
        
      });
    }
  }
}
Initialize();