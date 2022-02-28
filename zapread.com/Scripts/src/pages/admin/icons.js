/*
 * Admin Panel Icons
 * 
 * [ ] Set the default group icon
 * [ ] Change a group icon
 * [ ] Remove group icon
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
import { Row, Col, Form, Button, Container } from "react-bootstrap";
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
  const [imageId, setImageId] = useState(0); // This is the image used for the group

  const inputFile = useRef(null); // Used for selecting image file

  //const onDrop = useCallback(acceptedFiles => {
  //  console.log(acceptedFiles);

  //  var file = acceptedFiles[0];

  //  var fd = new FormData();
  //  fd.append("file", file);
  //  const xhr = new XMLHttpRequest();
  //  xhr.open("POST", "/Img/Group/DefaultIcon/", true);
  //  var headers = getAntiForgeryToken();

  //  for (var index in headers) {
  //    xhr.setRequestHeader(index, headers[index]);
  //  }

  //  // listen callback
  //  xhr.onload = () => {
  //    if (xhr.status === 200) {
  //      var data = JSON.parse(xhr.responseText);
  //      console.log(data.imgId);
  //    }
  //  };

  //  xhr.send(fd);
  //}, []);

  //const {
  //  getRootProps,
  //  getInputProps,
  //  isDragActive,
  //  isDragAccept,
  //  isDragReject
  //} = useDropzone({
  //  accept: "image/*",
  //  onDrop: onDrop
  //});

  //const style = useMemo(
  //  () => ({
  //    ...baseStyle,
  //    ...(isDragActive ? activeStyle : {}),
  //    ...(isDragAccept ? acceptStyle : {}),
  //    ...(isDragReject ? rejectStyle : {})
  //  }),
  //  [isDragActive, isDragReject, isDragAccept]
  //);

  function handleFileChange(selectorFiles) {
    var file = selectorFiles[0];

    var fd = new FormData();
    fd.append("file", file);
    const xhr = new XMLHttpRequest();

    // updateImgId is from the react state
    //xhr.open("POST", "/Img/Group/Icon/-1/", true);
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
        setImageId(data.imgId); // This is the new image id which the user just uploaded
      }
    };

    // Execute the request
    xhr.send(fd);
  }

  /**
   * initializes the user to select an icon to upload for the group image
   * @param {any} id
   * @param {any} e
   */
  function updateIcon(id, e) {
    inputFile.current.click();
  }

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

                <Row className="align-items-center">
                  <Col xs="auto" sm="auto" md="auto">
                    <input
                      type="file"
                      id="file"
                      ref={inputFile}
                      accept="image"
                      onChange={e => handleFileChange(e.target.files)}
                      style={{ display: "none" }}
                    />
                    <img src={`/Img/Group/IconById/${imageId}/?s=100`} />
                    {/*<img src="/Img/Group/Icon/0" />*/}
                  </Col>
                  <Col xs="auto" sm="auto" md="auto">
                    {/*This shows the image which the group will be assigned*/}
                  </Col>
                  <Col>
                    <Button
                      size="sm"
                      variant="outline-primary"
                      onClick={e => updateIcon(-1, e)}
                    >
                      Change Icon
                    </Button>
                  </Col>
                </Row>

                {/*<div {...getRootProps({ style })}>*/}
                {/*  <input {...getInputProps()} />*/}
                {/*  {isDragActive ? (*/}
                {/*    <p>Drop the file here ...</p>*/}
                {/*  ) : (*/}
                {/*    <p>Drag and drop a file here, or click to select file</p>*/}
                {/*  )}*/}
                {/*</div>*/}
                {/*<span>Default Group Icon:</span>*/}
                {/*<img src="/Img/Group/Icon/0" />*/}
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