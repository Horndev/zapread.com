/*
 * 
 */

import "../../shared/shared"; // [✓]
import "../../realtime/signalr"; // [✓]
import React, {
  useCallback,
  useMemo,
  useRef,
  useEffect,
  useState
} from "react";
import ReactDOM from "react-dom";
import PageHeading from "../../components/page-heading";
import IconsTable from "./Components/IconsTable";
import { useDropzone } from "react-dropzone";
import { getAntiForgeryToken } from "../../utility/antiforgery";
import "../../shared/sharedlast"; // [✓]

const baseStyle = {
  flex: 1,
  display: "flex",
  flexDirection: "column",
  alignItems: "center",
  padding: "20px",
  borderWidth: 2,
  borderRadius: 2,
  borderColor: "#eeeeee",
  borderStyle: "dashed",
  backgroundColor: "#fafafa",
  color: "#bdbdbd",
  outline: "none",
  transition: "border .24s ease-in-out"
};

const activeStyle = {
  borderColor: "#2196f3"
};

const acceptStyle = {
  borderColor: "#00e676"
};

const rejectStyle = {
  borderColor: "#ff1744"
};

function Page() {
  const onDrop = useCallback(acceptedFiles => {
    console.log(acceptedFiles);

    var file = acceptedFiles[0];

    var fd = new FormData();
    fd.append("file", file);
    const xhr = new XMLHttpRequest();
    xhr.open("POST", "/Img/Group/DefaultIcon/", true);
    var headers = getAntiForgeryToken();

    for (var index in headers) {
      xhr.setRequestHeader(index, headers[index]);
    }

    // listen callback
    xhr.onload = () => {
      if (xhr.status === 200) {
        var data = JSON.parse(xhr.responseText);
        console.log(data.imgId);
      }
    };

    xhr.send(fd);
  }, []);

  const {
    getRootProps,
    getInputProps,
    isDragActive,
    isDragAccept,
    isDragReject
  } = useDropzone({
    accept: "image/*",
    onDrop: onDrop
  });

  const style = useMemo(
    () => ({
      ...baseStyle,
      ...(isDragActive ? activeStyle : {}),
      ...(isDragAccept ? acceptStyle : {}),
      ...(isDragReject ? rejectStyle : {})
    }),
    [isDragActive, isDragReject, isDragAccept]
  );

  return (
    <div>
      <PageHeading
        title="ZapRead Icons"
        controller="Admin"
        method="Icons"
        function="Edit"
      />
      <div className="row">
        <div className="col-lg-12">
          <div className="wrapper wrapper-content animated fadeInUp">
            <div className="ibox">
              <div className="ibox-content" />
              <div className="ibox-content">
                <h1>Default Group Icon</h1>
                <div {...getRootProps({ style })}>
                  <input {...getInputProps()} />
                  {isDragActive ? (
                    <p>Drop the file here ...</p>
                  ) : (
                    <p>Drag and drop a file here, or click to select file</p>
                  )}
                </div>

                <span>Default Group Icon:</span>
                <img src="/Img/Group/Icon/0" />
              </div>
              <div className="ibox-content">
                <IconsTable title="Group Icons" pageSize={20} />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));

//import $ from 'jquery';

//import '../../shared/shared';
//import '../../realtime/signalr';
//import 'datatables.net-bs4';
//import 'datatables.net-scroller-bs4';
//import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
//import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
//import '../../shared/sharedlast';

//var iconstable = {};
//$(document).ready(function () {
//    iconstable = $('#iconsTable').DataTable({
//        "searching": false,
//        //"bInfo": false,
//        "lengthChange": false,
//        "pageLength": 10,
//        "processing": true,
//        "serverSide": true,
//        "sDom": '<"row view-filter"<"col-sm-12"<"pull-left"l><"pull-right"f><"clearfix">>>t<"row view-pager"<"col-sm-12"<"text-center"ip>>>',
//        "ajax": {
//            type: "POST",
//            contentType: "application/json",
//            url: "/Admin/GetIcons",
//            data: function (d) {
//                return JSON.stringify(d);
//            }
//        },
//        "columns": [
//            {
//                "data": "Graphic",
//                "orderable": true,
//                "mRender": function (data, type, row) {
//                    return "<i class='fa fa-" + data + " fa-3x'/>";
//                }
//            },
//            {
//                "data": "Icon",
//                "orderable": true,
//            },
//            {
//                "data": null,
//                "orderable": false,
//                "mRender": function (data, type, row) {
//                    //alert(JSON.stringify(data));
//                    return "<a href='javascript:void(0);' onclick='delicon(" + data.Id + ")'><i class='fa fa-trash fa-2x text-danger'></i></a>";
//                }
//            }
//        ]
//    });
//});

//export function delicon(item) {
//    var msg = { iD: item };
//    $.ajax({
//        async: true,
//        type: "POST",
//        url: "/Admin/DeleteIcon/",
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        data: JSON.stringify(msg),
//        success: function (response) {
//            iconstable.ajax.reload(null, false);
//        },
//        failure: function (response) {
//            //alert("failure " + JSON.stringify(response));
//        },
//        error: function (response) {
//            //alert("error " + JSON.stringify(response));
//        }
//    });
//    return false;
//}
//window.delicon = delicon;

//export function add() {
//    var iconVal = $('#newIcon').val();
//    var msg = { icon: iconVal };
//    $.ajax({
//        async: true,
//        type: "POST",
//        url: "/Admin/AddIcon/",
//        contentType: "application/json; charset=utf-8",
//        dataType: "json",
//        data: JSON.stringify(msg),
//        success: function (response) {
//            iconstable.ajax.reload();
//        },
//        failure: function (response) {
//            //alert("failure " + JSON.stringify(response));
//        },
//        error: function (response) {
//            //alert("error " + JSON.stringify(response));
//        }
//    });
//    return false;
//}
//window.add = add;
