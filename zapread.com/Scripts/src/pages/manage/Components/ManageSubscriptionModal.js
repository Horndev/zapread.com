/*
 * Update the about me in the user profile page
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card, Form } from "react-bootstrap";
import { postJson } from "../../../utility/postData";

export default function ManageSubscriptionModal(props) {
  const handleClose = () => {
    props.onClose();
  };

  const submit = () => {
    
  }

  useEffect(() => {
    async function initialize() {
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <Modal id="ManageSubscription" show={props.show} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>Manage Subscription</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          
        </Modal.Body>
        <Modal.Footer>
          <Button variant="warning" onClick={handleClose}>Cancel</Button>
          <Button variant="primary" onClick={() => submit()}>Save</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}