import React, { Suspense, useEffect, useState, useRef } from "react";
import { Row, Col, Button } from "react-bootstrap";
import Swal from 'sweetalert2';
import { postJson } from "../../utility/postData";

const isSafari = false;

export async function tokenizePaymentMethod(paymentMethod) {
  //https://codesandbox.io/s/nhgxy?file=/src/App.js:1200-1726
  console.log('tokenize start');
  const tokenResult = await paymentMethod.tokenize();
  console.log('tokenize: ', tokenResult);
  // A list of token statuses can be found here:
  // https://developer.squareup.com/reference/sdks/web/payments/enums/TokenStatus
  if (tokenResult.status === "OK") {
    return tokenResult.token
  }
  let errorMessage = `Tokenization failed-status: ${tokenResult.status}`
  if (tokenResult.errors) {
    errorMessage += ` and errors: ${JSON.stringify(tokenResult.errors)}`
  }
  throw new Error(errorMessage)
}

export default function PaymentInputs(props) {
  const [loaded, setLoaded] = useState(false);
  const [squarePayments, setSquarePayments] = useState(undefined);
  const [squareCard, setSquareCard] = useState(undefined)
  //const [applePay, setApplePay] = useState(undefined)
  const [googlePay, setGooglePay] = useState(undefined)
  const [isSubmitting, setSubmitting] = useState(false)
  const [validFields, setValidFields] = useState({
    cardNumber: false,
    cvv: false,
    expirationDate: false,
    postalCode: false,
  })
  const isCardFieldsValid = Object.values(validFields).every((v) => v)

  // SANDBOX
  const [applicationId, setApplicationId] = useState(props.applicationId); //'sandbox-sq0idb-jPMnuQnCOu9VWs0SH70DdQ');// 'sq0idp-rARHLPiahkGtp6mMz2OeCA');
  const [locationId, setLocationId] = useState(props.locationId); //'LEVZ15Q21DCG6');// 'GMT96A77XABR1');

  // https://codesandbox.io/s/nhgxy?file=/src/App.js:144-242

  // Add Square script to the page
  useEffect(() => {
    const existingScript = document.getElementById("webPayment");
    if (existingScript) setLoaded(true);
    else {
      const script = document.createElement("script");
      script.src = "https://sandbox.web.squarecdn.com/v1/square.js";
      script.id = "webPayment";
      document.body.appendChild(script);
      script.onload = () => {
        setLoaded(true);
      }
    }
  }, [])

  // Instantiate Square payments and store the object in state
  useEffect(() => {
    if (loaded && !squarePayments) {
      if (!window?.Square) {
        console.error("Square.js failed to load properly")
        return
      }
      setSquarePayments(window.Square?.payments(applicationId, locationId))
    }
  }, [loaded, squarePayments])

  const attachGooglePay = (gPay) => {
    const googlePayObject = gPay || googlePay
    googlePayObject.attach("#google-pay", {
      buttonColor: "white",
      buttonSizeMode: "fill",
      buttonType: "long",
    })
  }

  // Not currently used
  const initializeGooglePay = async () => {
    const paymentRequest = squarePayments.paymentRequest(paymentRequestMock)

    // We *MUST* return a PaymentRequestUpdate from shipping contact/option
    // event listeners below
    // https://developer.squareup.com/reference/sdks/web/payments/objects/PaymentRequestUpdate
    const paymentRequestUpdate = {
      // error: "There was an error of some kind",
      // shippingErrors: {
      //   addressLines: "Error with the Address Lines",
      //   city: "Error with the City",
      //   country: "Error with the Country",
      //   postalCode: "Error with the Postal Code",
      //   state: "Error with the state",
      // },
      lineItems: paymentRequestMock.lineItems,
      shippingOption: paymentRequestMock.shippingOptions,
      total: paymentRequestMock.total,
    }

    // Listener for shipping address changes
    paymentRequest.addEventListener("shippingcontactchanged", (contact) => {
      console.log({ contact })

      return paymentRequestUpdate
    })
    // Listener for shipping option changes
    paymentRequest.addEventListener("shippingoptionchanged", (option) => {
      console.log({ option })

      return paymentRequestUpdate
    })

    const gPay = await squarePayments.googlePay(paymentRequest)
    setGooglePay(gPay)
    attachGooglePay(gPay)
  }

  // Attach the Square card to our container and setup event listeners
  const attachCard = (card) => {
    // We pass in the card object during initialization, but re-use it from
    // state for normal re-renders
    const cardObject = card || squareCard
    //cardObject.detach()
    cardObject.attach("#card-container")
    // Listeners: https://developer.squareup.com/reference/sdks/web/payments/objects/Card#Card.addEventListener
    cardObject.addEventListener("submit", () =>
      handlePaymentMethodSubmission(cardObject)
    )
    cardObject.addEventListener("focusClassAdded", handleCardEvents)
    cardObject.addEventListener("focusClassRemoved", handleCardEvents)
    cardObject.addEventListener("errorClassAdded", handleCardEvents)
    cardObject.addEventListener("errorClassRemoved", handleCardEvents)
    cardObject.addEventListener("cardBrandChanged", handleCardEvents)
    cardObject.addEventListener("postalCodeChanged", handleCardEvents)
    console.log('attached card')
  }

  const initializeSquareCard = async () => {
    const card = await squarePayments.card()
    setSquareCard(card)
    attachCard(card)
  }

  // Handle Square payment methods initialization and re-attachment
  useEffect(() => {
    if (squarePayments) {
      console.log('squarePayments not loaded');
      if (!squareCard) {
        console.log('initializeSquareCard');
        initializeSquareCard();
      }
      //if (!applePay && isSafari) initializeApplePay()
      //if (!googlePay) initializeGooglePay()
      //else attachGooglePay()
    }
    // Otherwise, we destroy the objects and reset state
    else {
      console.log('squarePayments not loaded');
      if (squareCard) {
        console.log('destroy squarecard');
        squareCard.destroy()
        setSquareCard(undefined)
      }
      //if (applePay) {
      //  applePay.destroy()
      //  setApplePay(undefined)
      //}
      //if (googlePay) {
      //  googlePay.destroy()
      //  setGooglePay(undefined)
      //}
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [squarePayments])

  async function verifyBuyer(token) {
    const customerInfo = {
      addressLines: [props.customer.address1, props.customer.address2],
      familyName: props.customer.lastName,
      givenName: props.customer.firstName,
      email: props.customer.email,
      countryCode: props.customer.country,
      city: props.customer.city
    };

    var verificationDetails = {
      //amount: props.product.Price, // Not needed for store
      //currencyCode: "CAD",  // Not needed for store
      intent: "STORE", // "CHARGE"
      billingContact: customerInfo
    };

    console.log(verificationDetails);

    const verifyResult = await squarePayments.verifyBuyer(token, verificationDetails);
    return verifyResult;
  }

  const handlePaymentMethodSubmission = async (paymentMethod) => {
    const isCard = paymentMethod?.element?.id === "card-container"
    if (isCard && !isCardFieldsValid) return
    if (!isSubmitting) {
      // Disable the submit button as we await tokenization and make a
      // payment request
      if (isCard) setSubmitting(true)
      try {
        const token = await tokenizePaymentMethod(paymentMethod)
        console.log("TOKEN", token)

        const verifyResult = await verifyBuyer(result.token);
        console.log("verifyResult", verifyResult);

        postJson("/api/v1/account/purchases/subscriptions/subscribe",
          {
            CardToken: result.token,
            VerificationToken: verifyResult.token,
            PlanId: props.product.PlanId,
            CustomerEmail: props.customer.email,
            FirstName: props.customer.firstName,
            LastName: props.customer.lastName
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

      } catch (error) {
        console.error("FAILURE", error)
      } finally {
        isCard && setSubmitting(false)
      }
    }
  }

  // Set each card field validity on various events
  const handleCardEvents = ({ detail }) => {
    if (detail) {
      const { currentState: { isCompletelyValid } = {}, field } = detail
      if (field) {
        setValidFields((prevState) => ({
          ...prevState,
          [field]: isCompletelyValid,
        }))
      }
    }
  }

  return (
    <Row>
      <Col>
        <div style={{ marginBottom: 24 }}>
          <div
            id="google-pay"
            onClick={() => handlePaymentMethodSubmission(googlePay)}/>
        </div>
        <form id="payment-form">
          <div
            style={{
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
            }}>
            <div id="card-container"></div>
            <Button variant="primary"
              id="card-button"
              disabled={!isCardFieldsValid || isSubmitting}
              onClick={() => handlePaymentMethodSubmission(squareCard)}>
              {props.children}
            </Button>
          </div>
        </form>
        <div id="payment-status-container"></div>
        {/*<Button variant="primary"*/}
        {/*  onClick={() => { props.onSubscribed(props.product.PlanId); }}>Test Subscribed</Button>*/}
      </Col>
    </Row>
  );
};