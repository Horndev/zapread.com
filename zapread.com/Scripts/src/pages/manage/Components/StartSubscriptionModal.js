/*
 * Update the about me in the user profile page
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card, Form } from "react-bootstrap";
import { CountryDropdown } from 'react-country-region-selector';

import { postJson } from "../../../utility/postData";
import PaymentInputs from "../../../Components/Payments/PaymentInputs";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faLock,
  faCreditCard
} from '@fortawesome/free-solid-svg-icons'

export default function StartSubscriptionModal(props) {
  const [email, setEmail] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [country, setCountry] = useState("");
  const [city, setCity] = useState("");
  const [postalCode, setPostalCode] = useState("");
  const [address1, setAddress1] = useState("");
  const [address2, setAddress2] = useState("");

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

  const onCountryChange = (val) => {
    //console.log(val);
    setCountry(val);
  }

  return (

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

        <h1 >Subscribe as {props.product.Name}</h1>
        <br />
        <big>Order Total <b>CA${(Math.round(props.product.Price * 100) / 100).toFixed(2)}</b></big> <small> / month</small>
        <br />
        <h3 className="mt-5">Billing Address</h3>
        <strong style={{color:"sienna"}}>This information is only used to securely verify your credit card purchase.  Zapread will not save your name or address information.</strong>
        <br /><br />
        <Form>
          <Form.Row>
            <Form.Group as={Col} controlId="formEmail">
              <Form.Control
                onChange={({ target: { value } }) => setEmail(value)} // Controlled input
                value={email}
                placeholder="Email"
              />
            </Form.Group>
          </Form.Row>

          <Form.Row>
            <Form.Group as={Col} controlId="formFirstName">
              <Form.Control
                onChange={({ target: { value } }) => setFirstName(value)} // Controlled input
                value={firstName}
                placeholder="First Name"
              />
            </Form.Group>
            <Form.Group as={Col} controlId="formLastName">
              <Form.Control
                onChange={({ target: { value } }) => setLastName(value)} // Controlled input
                value={lastName}
                placeholder="Last Name"
              />
            </Form.Group>
          </Form.Row>

          <Form.Group controlId="formGridAddress1">
            <Form.Control
              onChange={({ target: { value } }) => setAddress1(value)} // Controlled input
              value={address1} 
              placeholder="Address line 1 (1234 Main St)" />
          </Form.Group>

          <Form.Group controlId="formGridAddress2">
            <Form.Control
              onChange={({ target: { value } }) => setAddress2(value)} // Controlled input
              value={address2} 
              placeholder="Address Line 2 Apartment, studio, or floor" />
          </Form.Group>

          <Form.Row>
            <Form.Group as={Col} controlId="formGridCity">
              <Form.Control
                onChange={({ target: { value } }) => setCity(value)} // Controlled input
                value={city} 
                placeholder="City"
              />
            </Form.Group>

            <Form.Group as={Col} controlId="formGridCountry">
              <CountryDropdown
                classes="form-control"
                autoComplete="country-name"
                value={country}
                valueType="short"
                onChange={(val) => onCountryChange(val)} />
            </Form.Group>

            <Form.Group as={Col} controlId="formGridZip">
              <Form.Control
                onChange={({ target: { value } }) => setPostalCode(value)} // Controlled input
                value={postalCode}
                placeholder="Zip/Postal Code"
              />
            </Form.Group>
          </Form.Row>
        </Form>

        <h3 className="mt-5">Payment</h3>

        <Container style={{ border: "2px solid #1ab394" }}>
          <Row className="text-center my-4">
            <Col lg={2}></Col>
            <Col lg={8}>
              All transactions are secure and encrypted. <FontAwesomeIcon icon={faLock} style={{ color: "#1ab394" }} />
              <h4>Pay with Credit Card <FontAwesomeIcon icon={faCreditCard} /></h4>
            </Col>
          </Row>
          <Row className="text-center my-2">
            <Col lg={2}></Col>
            <Col lg={8}>
              <PaymentInputs
                onSubmitSubscribe={(cardToken, verifyToken) => {
                  postJson("/api/v1/account/purchases/subscriptions/subscribe",
                    {
                      CardToken: cardToken,
                      VerificationToken: verifyToken,
                      PlanId: props.product.PlanId,
                      CustomerEmail: email,
                      FirstName: firstName,
                      LastName: lastName
                    }).then(response => {
                      //console.log("subscribe", response);
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
                }}
                applicationId={props.applicationId}
                locationId={props.locationId}
                product={props.product}
                customer={{
                  email: email,
                  firstName: firstName,
                  lastName: lastName,
                  city: city,
                  postalCode: postalCode,
                  country: country,
                  address1: address1,
                  address2: address2
                }}>
                Subscribe for CA${props.product.Price} Monthly
              </PaymentInputs>
            </Col>
          </Row>
        </Container>
        <br /><br /><br />
        By subscribing, you agree to the Square Pay <a target="_blank" href="https://squareup.com/legal/general/ua">Terms of Service</a>
        {" "}and{" "}<a target="_blank" href="https://squareup.com/legal/general/privacy">Privacy Notice</a>.
        Your subscription will renew automatically every month.
        <br /><br />
        You may cancel at any time.
      </Modal.Body>
      <Modal.Footer>
        {/*<Button block variant="primary" onClick={() => submit()}>Subscribe CA${ props.product.Price } monthly</Button>*/}
        {/*          <Button variant="warning" onClick={handleClose}>Cancel</Button>*/}
      </Modal.Footer>
    </Modal>

  );
}