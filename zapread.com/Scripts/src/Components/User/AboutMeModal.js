/*
 * Update the about me in the user profile page
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card, Form } from "react-bootstrap";
import { postJson } from "../../utility/postData";

export default function AboutMeModal(props) {
  const [show, setShow] = useState(props.show);
  const [aboutMe, setAboutMe] = useState(props.aboutMe);

  const handleClose = () => {
    setShow(false);
  };

  const submit = () => {
    postJson('/Manage/UpdateAboutMe/',
      {
        AboutMe: aboutMe
      }
    ).then(response => {
      if (response.success) {
        props.onUpdated(aboutMe);
        setShow(false);
      } else {
        Swal.fire("Error", `${response.message}`, "error");
      }
    });
  }

  useEffect(() => {
    setShow(props.show);
  }, [props.show]); // Update from props

  useEffect(() => {
    async function initialize() {
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <Modal id="ModalAboutMe" show={show} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Update About Me</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Form>
            <Form.Group controlId="aboutme.textarea">
              <Form.Label>AboutMe</Form.Label>
              <Form.Control
                as="textarea"
                value={aboutMe}
                onChange={({ target: { value } }) => {
                  setAboutMe(value);
                }} // Controlled input
                rows={3} />
            </Form.Group>
          </Form>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="warning" onClick={handleClose}>Cancel</Button>
          <Button variant="primary" onClick={() => submit()}>Save</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}