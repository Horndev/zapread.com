

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import { Dropzone } from "dropzone";
import { getAntiForgeryTokenValue } from '../../utility/antiforgery';
import 'dropzone/dist/basic.css';
import 'dropzone/dist/dropzone.css';

export default function ProfileImageModal(props) {
  const [show, setShow] = useState(props.show);
  const [token, setToken] = useState("");

  const handleClose = () => {
    setShow(false);
  };

  useEffect(() => {
    if (show) {
      Dropzone.discover();
    }
  }, [show]); // Update after shown

  useEffect(() => {
    setShow(props.show);
  }, [props.show]); // Update from props

  useEffect(() => {
    async function initialize() {
      var aftoken = getAntiForgeryTokenValue();
      setToken(aftoken);

      Dropzone.options.dropzoneForm = {
        paramName: "file", // The name that will be used to transfer the file
        maxFilesize: 15, // MB
        acceptedFiles: "image/*",
        maxFiles: 1,
        uploadMultiple: false,
        init: function () {
          this.on("addedfile", function () {
          });
          this.on("success", function (file, response) {
            if (response.success) {
              props.onUpdated(response.version);
              //updateImagesOnPage(response.version); // Reload images
            } else {
              // Did not work
              Swal.fire({
                icon: 'error',
                title: 'Image Update Error',
                text: "Error updating image: " + response.message
              })
            }
          });
        },
        dictDefaultMessage: "<strong>Drop user image here or click to upload</strong>"
      };

      Dropzone.discover();
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <Modal id="ModalProfileImage" show={show} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Update Profile Image</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <form action="/Manage/UpdateProfileImage/" className="dropzone" id="dropzoneForm" method="post" role="form">
            <input name="__RequestVerificationToken" type="hidden" value={token} />
            <div className="fallback">
              <input name="file" type="file" />
            </div>
          </form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="primary" onClick={() => setShow(false) }>Done</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}