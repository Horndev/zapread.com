/*
 * Modal user interface for gifting referral.
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
import { Modal, Form, Row, Col, Button, Card } from "react-bootstrap";
import { postJson } from '../../../utility/postData';
import UserAutosuggest from "../../../Components/UserAutoSuggest";
const getSwal = () => import('sweetalert2');

export default function UpdateEmailModal(props) {
  const [show, setShow] = useState(props.show);
  const [showSpinner, setShowSpinner] = useState(false);
  const [userEmail, setUserEmail] = useState("");
  const [userName, setUserName] = useState("");
  const [userCurrentEmail, setUserCurrentEmail] = useState("");

  useEffect(() => {
    // hook into button click
    document.getElementById("updateEmailBtn").addEventListener('click', (e) => {
      setShow(true);
    });

    async function initialize() {
      const response = await fetch("/api/v1/user/email/");
      const json = await response.json();
      setUserCurrentEmail(json.Email);
    }
    initialize();

  }, []);

  const handleClose = () => {
    // Cleanup & reset
    setShow(false);
    props.onClose();
  };

  const onOK = () => {
    const emailRegex = /\S+@\S+\.\S+/;
    if (!emailRegex.test(userEmail)) {
      setShow(false);
      getSwal().then(({ default: Swal }) => {
        Swal.fire("Error", "Email address invalid", "error");
      })
      return;
    }
    setShowSpinner(true);
    getSwal().then(({ default: Swal }) => {
      setShow(false);
      Swal.fire({
        title: 'Are you sure?',
        text: "Withdraws will be locked for 24 hours.",
        icon: 'warning',
        showCancelButton: true
      }).then((result) => {
        if (result.isConfirmed) {
          postJson("/Account/UpdateEmail/", {
            Email: userEmail
          }).then((response) => {
            setShowSpinner(false);
            if (response.success) {
              setShow(false);
              Swal.fire("Success", "You have updated your email!", "success");
            } else {
              Swal.fire("Error", "Error: " + response.message, "error");
            }
          });
        }
      });
    });
    setShowSpinner(false);
  };

  return (
    <>
      <Modal id="gift-referral-modal" show={show} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Update Email Address</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Card>
            <Card.Body>
              <p>
                Enter your new email address.  Note that you will have to re-verify this email address.
              </p>
              <p>
                <i className="fa-solid fa-triangle-exclamation"></i> All withdrawals will be locked for 24 hours for security.
              </p>
              <Form.Group controlId="formHorizontalEmail">
                <Form.Label>
                  Your current email
                </Form.Label>
                <p>{userCurrentEmail}</p>
              </Form.Group>
              <Form>
                <Form.Group controlId="formHorizontalEmail">
                  <Form.Label>
                    New email
                  </Form.Label>
                  <Form.Control type="email" placeholder="Email" value={userEmail} onChange={(e) => setUserEmail(e.target.value)} />
                </Form.Group>
              </Form>
            </Card.Body>
          </Card>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="info"
            onClick={onOK}
            disabled={showSpinner}>
            Ok <i className="fa-solid fa-circle-notch fa-spin" style={showSpinner ? {} : { display: "none" }}></i>
          </Button>
          <Button variant="secondary" onClick={handleClose}>
            Cancel
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}