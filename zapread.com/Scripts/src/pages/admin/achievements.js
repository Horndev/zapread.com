/*
 * 
 */

import $ from 'jquery';

import '../../shared/shared';
import '../../realtime/signalr';
import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
import Dropzone from 'dropzone';
import 'dropzone/dist/basic.css';
import 'dropzone/dist/dropzone.css';
import Swal from 'sweetalert2';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { Modal } from 'bootstrap.native/dist/bootstrap-native-v4';
import '../../shared/sharedlast';
window.Dropzone = Dropzone;

var id = -1;
window.id = id;
Dropzone.options.dropzoneForm = {
  url: "/Admin/Achievements/Upload/",
  paramName: "file", // The name that will be used to transfer the file
  maxFilesize: 5, // MB
  createImageThumbnails: true,
  acceptedFiles: "image/*",
  maxFiles: 1,
  addRemoveLinks: true,
  params: {
    id: window.id
  },
  init: function () {
    window.dz = this;
    this.on("sending", function (file, xhr, formData) {
      formData.append("id", id);
      //console.log(formData);
    });
    this.on("addedfile", function () {
    });
    this.on("success", function (file, response) {
      //console.log(response.Id);
      //alert(response.Id);
      id = response.Id;
      $('#aImage').attr('src', '/Img/AchievementImage/' + id + '/');
      window.id = -1; // reset for next
    });
  },
  dictDefaultMessage: "<strong>Drop files here or click to upload!</strong>"
};

var achievementstable = {};
$(document).ready(function () {
  achievementstable = $('#achievementsTable').DataTable({
    "searching": false,
    "lengthChange": false,
    "pageLength": 10,
    "processing": true,
    "serverSide": true,
    "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
    "ajax": {
      type: "POST",
      contentType: "application/json",
      url: "/Admin/GetAchievements",
      data: function (d) {
        return JSON.stringify(d);
      }
    },
    "columns": [
      {
        //"data": "Id",
        "data": null,
        "orderable": true,
        "mRender": function (data, type, row) {
          return "<a href='javascript:void(0);' onclick='editAchImage(" + data.Id + ")'><i class='fa fa-edit fa-2x text-info'></i></a>"
            + data.Id + ": <img src='/Img/AchievementImage/" + data.Id + "/'>";
        }
      },
      {
        //"data": "Name",
        "data": null,
        "orderable": true,
        "mRender": function (data, type, row) {
          return "<a href='javascript:void(0);' onclick='editAchName(" + data.Id + ")'><i class='fa fa-edit fa-2x text-warning'></i></a>"
            + "<span id='acn_" + data.Id + "'>" + data.Name + "</span>";
        }
      },
      {
        //"data": "Description",
        "data": null,
        "orderable": false,
        "mRender": function (data, type, row) {
          return "<a href='javascript:void(0);' onclick='editAchDescription(" + data.Id + ")'><i class='fa fa-edit fa-2x text-info'></i></a>"
            + "<span id='acd_" + data.Id + "'>" + data.Description + "</span>";
        }
      },
      {
        "data": "Value",
        "orderable": false
      },
      {
        "data": "Awarded",
        "orderable": false
      },
      {
        "data": null, // Actions
        "orderable": false,
        "mRender": function (data, type, row) {
          //alert(JSON.stringify(data));
          return "<a href='javascript:void(0);' onclick='delAchievement(" + data.Id + ")'><i class='fa fa-trash fa-2x text-danger'></i></a>"
            + "&nbsp;&nbsp;"
            + "<a href='javascript:void(0);' onclick='grantAchievement(" + data.Id + ")'><i class='fa fa-user-plus fa-2x text-info'></i></a>";
        }
      }
    ]
  });
});

function showImageModal() {
  if (Object.prototype.hasOwnProperty.call(document.getElementById('ModalFileUpload'), "Modal")) {
    document.getElementById('ModalFileUpload').Modal.show();
  } else {
    var modalEl = document.getElementById('ModalFileUpload');
    var modal = new Modal(modalEl);
    modal.show();
  }
}

export function uploadImage() {
  //console.log(id);
  //$('#ModalFileUpload').modal('show');
  showImageModal();
  return false;
}
window.uploadImage = uploadImage;

export function editAchImage(achId) {
  id = achId;
  showImageModal();
}
window.editAchImage = editAchImage;

export function add() {
  var aName = $('#aName').val();
  var aDescription = $('#aDescription').val();
  var aValue = $('#aValue').val();
  var msg = { id: id, name: aName, description: aDescription, value: aValue };

  $.ajax({
    async: true,
    type: "POST",
    url: "/Admin/AddAchievement",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify(msg),
    success: function (response) {
      achievementstable.ajax.reload();
    },
    failure: function (response) {
      //alert("failure " + JSON.stringify(response));
    },
    error: function (response) {
      //alert("error " + JSON.stringify(response));
    }
  });
  return false;
}

window.add = add;

export function delAchievement(item) {
  var msg = { id: item };
  $.ajax({
    async: true,
    type: "POST",
    url: "/Admin/DeleteAchievement",
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    data: JSON.stringify(msg),
    success: function (response) {
      achievementstable.ajax.reload(null, false);
    },
    failure: function (response) {
      //alert("failure " + JSON.stringify(response));
    },
    error: function (response) {
      //alert("error " + JSON.stringify(response));
    }
  });
  return false;
}

window.delAchievement = delAchievement;

export function editAchDescription(id) {
  Swal.fire({
    text: 'Enter new description',
    input: 'text',
    inputValue: $('#acd_' + id).html(),
    showCancelButton: true
  })
    .then(result => {
      if (!result) throw null;
      $.post("/Admin/Achievements/Description/Update/",
        { "id": id, "description": result.value },
        function (data) {
          if (data.success) {
            achievementstable.ajax.reload();
            Swal.fire("Description has been updated!", {
              icon: "success"
            });
          }
          else {
            Swal.fire("Error", "Error updating description: " + data.message, "error");
          }
        });
    })
    .catch(err => {
      if (err) {
        Swal.fire("Error", "Error updating description.", "error");
      } else {
        Swal.stopLoading();
        Swal.close();
      }
    });
  return false;
}

window.editAchDescription = editAchDescription;

export function editAchName(id) {
  Swal.fire({
    text: 'Enter new name',
    input: 'text',
    inputValue: $('#acn_' + id).html(),
    showCancelButton: true
  })
    .then(result => {
      if (!result) throw null;
      $.post("/Admin/Achievements/Name/Update/",
        { "id": id, "name": result.value },
        function (data) {
          if (data.success) {
            achievementstable.ajax.reload();
            Swal.fire("Name has been updated!", {
              icon: "success"
            });
          }
          else {
            Swal.fire("Error", "Error updating name: " + data.message, "error");
          }
        });
    })
    .catch(err => {
      if (err) {
        Swal.fire("Error", "Error updating name.", "error");
      } else {
        Swal.stopLoading();
        Swal.close();
      }
    });
  return false;
}

window.editAchName = editAchName;

export function grantAchievement(id) {
  Swal.fire({
    text: 'Enter user name',
    input: 'text',
    inputValue: '',
    showCancelButton: true
  })
    .then(name => {
      if (!name) throw null;
      $.post("/Admin/Achievements/Grant/",
        { "id": id, "username": name.value },
        function (data) {
          if (data.success) {
            achievementstable.ajax.reload();
            Swal.fire("Achievement has been granted!", {
              icon: "success"
            });
          }
          else {
            Swal.fire("Error", "Error granting: " + data.message, "error");
          }
        });
    })
    .catch(err => {
      if (err) {
        Swal.fire("Error", "Error granting.", "error");
      } else {
        Swal.stopLoading();
        Swal.close();
      }
    });
  return false;
}
window.grantAchievement = grantAchievement;