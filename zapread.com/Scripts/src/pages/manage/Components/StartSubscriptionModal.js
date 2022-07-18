/*
 * Update the about me in the user profile page
 */

import React, { StrictMode, useCallback, useEffect, useState, useRef, createRef } from "react";
import { PaymentForm, CreditCard } from 'react-square-web-payments-sdk'
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card, Form } from "react-bootstrap";
import { postJson } from "../../../utility/postData";
import PaymentInputs from "../../../Components/Payments/PaymentInputs";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faLock,
  faCreditCard
} from '@fortawesome/free-solid-svg-icons'

export default function StartSubscriptionModal(props) {
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
    <StrictMode>
      <Modal
        animation={false}
          size="lg"
          centered
          id="ManageSubscription"
          backdrop="static"
          show={props.show} onHide={handleClose}>
          <Modal.Header closeButton>
            <Modal.Title>Subscribe</Modal.Title>
          </Modal.Header>
          <Modal.Body>

            <h1 >{props.product.Name}</h1>
            <br/>
            <big>Order Total <b>CA${props.product.Price}</b></big> <small>/ month</small>
            <br/>
            <h3 className="mt-5">Contact</h3>
            <Form>
              <Form.Row>
                <Col>
                  <Form.Control placeholder="Email address for receipt" />
                </Col>
              </Form.Row>
              <Form.Row className="my-3">
                <Col>
                  <Form.Control placeholder="First name" />
                </Col>
                <Col>
                  <Form.Control placeholder="Last name" />
                </Col>
              </Form.Row>
            </Form>

            <h3 className="mt-5">Payment</h3>
          
            <Container style={{ border: "2px solid #1ab394" }}>
              <Row className="text-center my-4">
                <Col lg={2}></Col>
                <Col lg={8}>
                  <h4>Pay with Credit Card <FontAwesomeIcon icon={faCreditCard} /></h4>
                  All transactions are secure and encrypted. <FontAwesomeIcon icon={faLock} style={{ color: "#1ab394" }} />
                </Col>
              </Row>
              <Row className="text-center my-2">
                <Col lg={2}></Col>
                <Col lg={8}>
                  {/*<PaymentInputs />*/}
                
                  
                    {/*<CreditCard />*/}
                
                </Col>
              </Row>
            </Container>
            <br /><br /><br />
              By subscribing, you agree to the Square Pay <a target="_blank" href="https://squareup.com/legal/general/ua">Terms of Service</a>
            {" "}and{" "}<a target="_blank" href="https://squareup.com/legal/general/privacy">Privacy Notice</a>.
            Your subscription will renew automatically every month.
            <br/><br/>
            You may cancel at any time.
          </Modal.Body>
          <Modal.Footer>
            {/*<Button block variant="primary" onClick={() => submit()}>Subscribe CA${ props.product.Price } monthly</Button>*/}
  {/*          <Button variant="warning" onClick={handleClose}>Cancel</Button>*/}
          </Modal.Footer>
        </Modal>

    </StrictMode>
  );
}