/*
 * Update the about me in the user profile page
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card, Form } from "react-bootstrap";
import { postJson } from "../../../utility/postData";
import { ISOtoRelative } from "../../../utility/datetime/posttime";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faPause,
  faPlay
} from '@fortawesome/free-solid-svg-icons'

export default function ManageSubscriptionModal(props) {
  const handleClose = () => {
    props.onClose();
  };

  const onCancel = () => {
    postJson("/api/v1/account/purchases/subscriptions/unsubscribe",
      {
        SubscriptionId: props.product.SubscriptionId,
      }).then(response => {
        if (response.success) {
          props.onUnubscribed(props.product.SubscriptionId);
        }
      }).catch(error => {
        console.log("error", error);
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        error.json().then(data => {
          Swal.fire("Error", `${data.Message}`, "error");
        })
      });
  }

  const onResubscribe = () => {
    postJson("/api/v1/account/purchases/subscriptions/subscribe",
      {
        PlanId: props.product.PlanId,
      }).then(response => {
        if (response.success) {
          props.onSubscribed(props.product.PlanId);
        }
      }).catch(error => {
        console.log("error", error);
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        error.json().then(data => {
          Swal.fire("Error", `${data.Message}`, "error");
        })
      });
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
          <h1 >You are subscribed as {props.product.Name}</h1>
          {props.product.IsSubscribed ? (
            <>
              Your current subscription {props.product.IsEnding ? "ends" : "renews"} {" "} {ISOtoRelative(props.product.EndDate, true)}.
            </>) : (<></>)}
          
          <br /><br />
          {props.product.IsEnding ? (
            <>
              <h4>Changed your mind?</h4>
              You have cancelled your subscription to this plan.
              <br/><br/>
              It will remain active until it expires {ISOtoRelative(props.product.EndDate, true)}.
              <br /><br />
              <Button block variant="primary" onClick={onResubscribe}><FontAwesomeIcon icon={faPlay} /> Resume Subscription</Button>
            </>
          ) : (
            <>
              {/*<Button block variant="primary" onClick={onUpdate}>Change Payment Method</Button>*/}
                <h4>Need a break?</h4>
                If you wish to stop your subscription, you may pause and then resume at any time.
                <br />
                <br />
                You will keep your current benefits until the next renewal date.
                <br />
                <br />
                <Button block variant="danger" onClick={onCancel}><FontAwesomeIcon icon={faPause} /> Pause Subscription</Button>
            </>)}
        </Modal.Body>
        <Modal.Footer>
          <Button variant="secondary" onClick={handleClose}>Close</Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}