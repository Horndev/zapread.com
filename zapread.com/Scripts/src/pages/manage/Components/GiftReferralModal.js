/*
 * Modal user interface for gifting referral.
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import { postJson } from '../../../utility/postData';
import UserAutosuggest from "../../../Components/UserAutoSuggest";
const getSwal = () => import('sweetalert2');

export default function GiftReferralModal(props) {
  const [show, setShow] = useState(props.show);
  const [showSpinner, setShowSpinner] = useState(false);
  const [userAppId, setUserAppId] = useState("");
  const [userName, setUserName] = useState("");

  useEffect(() => {
    // hook into button click
    document.getElementById("giftReferalBtn").addEventListener('click', (e) => {
      setShow(true);
    });
  }, []);

  const handleClose = () => {
    // Cleanup & reset
    setShow(false);
  };

  function onSelected(values) {
    console.log("selected", values);
    setUserAppId(values.UserAppId);
    setUserName(values.userName);
  }

  const onOK = () => {
    setShowSpinner(true);
    getSwal().then(({ default: Swal }) => {
      setShow(false);
      Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true
      }).then((result) => {
        if (result.isConfirmed) {
          postJson("/api/v1/user/giftreferral/", {
            UserAppId: userAppId
          }).then((response) => {
            setShowSpinner(false);
            if (response.success) {
              setShow(false);
              document.getElementById("giftReferalBtn").style.display = "none";
              document.getElementById("refEnrolled").innerHTML = "gifted";
              Swal.fire("Success", "You have gifted your referral!", "success");
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
          <Modal.Title>Gift Referral</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Card>
            <Card.Body>
              <p>
                You may gift a one-time referral bonus to another user to give a 6-month earning bonus to you and the other user.  When you earn upvotes, you will both receive a bonus!
              </p>
              <p>
                Once assigned, this can not be changed.
              </p>
              <UserAutosuggest label="User Name" onSelected={onSelected} url="/api/v1/user/search" />
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