import * as React from 'react';
import { CreditCard, GooglePay, ApplePay, PaymentsForm } from 'react-square-web-payments-sdk'

export default function PaymentInputs() {

  return (
    <>
      payment
    </>
  );
};

/*
 *     <PaymentsForm
      applicationId="sandbox-sq0idb-jPMnuQnCOu9VWs0SH70DdQ"
      cardTokenizeResponseReceived={(token, buyer) => {
        console.info({ token, buyer });
      }}
        locationId="LEVZ15Q21DCG6"
    >
      <CreditCard />
    </PaymentsForm>
 */

//import { PaymentInputsWrapper, usePaymentInputs } from 'react-payment-inputs';
//import images from 'react-payment-inputs/images';

//export default function PaymentInputs() {
//  const {
//    wrapperProps,
//    getCardImageProps,
//    getCardNumberProps,
//    getExpiryDateProps,
//    getCVCProps
//  } = usePaymentInputs();

//  return (
//    <PaymentInputsWrapper {...wrapperProps}>
//      <svg {...getCardImageProps({ images })} />
//      <input {...getCardNumberProps()} />
//      <input {...getExpiryDateProps()} />
//      <input {...getCVCProps()} />
//    </PaymentInputsWrapper>
//  );
//}


// Bootstrap version
//import React from 'react';
//import { usePaymentInputs } from 'react-payment-inputs';
//import { Modal, Container, Row, Col, Button, Card, Form } from "react-bootstrap";

//export default function PaymentInputs() {
//  const {
//    meta,
//    getCardNumberProps,
//    getExpiryDateProps,
//    getCVCProps
//  } = usePaymentInputs();
//  const { erroredInputs, touchedInputs } = meta;

//  return (
//    <Form>
//      <Form.Row>
//        <Form.Group as={Col} style={{ maxWidth: '15rem' }}>
//          <Form.Label>Card number</Form.Label>
//          <Form.Control
//            // Here is where React Payment Inputs injects itself into the input element.
//            {...getCardNumberProps()}
//            // You can retrieve error state by making use of the error & touched attributes in `meta`.
//            isInvalid={touchedInputs.cardNumber && erroredInputs.cardNumber}
//            placeholder="0000 0000 0000 0000"
//          />
//          <Form.Control.Feedback type="invalid">{erroredInputs.cardNumber}</Form.Control.Feedback>
//        </Form.Group>
//        <Form.Group as={Col} style={{ maxWidth: '10rem' }}>
//          <Form.Label>Expiry date</Form.Label>
//          <Form.Control
//            {...getExpiryDateProps()}
//            isInvalid={touchedInputs.expiryDate && erroredInputs.expiryDate}
//          />
//          <Form.Control.Feedback type="invalid">{erroredInputs.expiryDate}</Form.Control.Feedback>
//        </Form.Group>
//        <Form.Group as={Col} style={{ maxWidth: '7rem' }}>
//          <Form.Label>CVC</Form.Label>
//          <Form.Control
//            {...getCVCProps()}
//            isInvalid={touchedInputs.cvc && erroredInputs.cvc}
//            placeholder="123"
//          />
//          <Form.Control.Feedback type="invalid">{erroredInputs.cvc}</Form.Control.Feedback>
//        </Form.Group>
//      </Form.Row>
//    </Form>
//  );
//}