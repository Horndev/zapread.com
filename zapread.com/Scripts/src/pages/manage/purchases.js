/**
 * User purchases page
 * 
 * 
 * 

$2 worth of Bitcoin in your balance to spend
<div class="hr-line-primary"></div>
Soon, 1 random reaction unlock per month
<div class="hr-line-primary"></div>
Soon, an optional icon next to your username.


$5 worth of Bitcoin in your balance to spend﻿
<div class="hr-line-primary"></div>
Soon, 2 random reaction unlocks per month
<div class="hr-line-primary"></div>
Soon, an optional icon next to your username
<div class="hr-line-primary"></div>
Soon, your username will be optionally a different colour.


$10 worth of Bitcoin in your balance to spend
<div class="hr-line-primary"></div>
Soon, 3 random reaction unlocks per month
<div class="hr-line-primary"></div>
Soon, an optional icon next to your username
<div class="hr-line-primary"></div>
Soon, an optional different colour username
<div class="hr-line-primary"></div>
and an optional animation on your username.

    <PaymentForm
        applicationId="sandbox-sq0idb-jPMnuQnCOu9VWs0SH70DdQ"
        cardTokenizeResponseReceived={(token, buyer) => {
          console.log("response received");
          console.log(token, buyer);
          console.info({ token, buyer });
        }}
        locationId="LEVZ15Q21DCG6">
        <CreditCard>
          Subscribe CA$2 monthly
        </CreditCard>
      </PaymentForm >

 */

import "../../shared/shared";
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState, useRef } from "react";
import ReactDOM from "react-dom";
import { Container, Row, Col, Button, Card, CardDeck } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faArrowDown,
  faCircleNotch
} from '@fortawesome/free-solid-svg-icons'
const ManageSubscriptionModal = React.lazy(() => import("./Components/ManageSubscriptionModal"));
import StartSubscriptionModal from "./Components/StartSubscriptionModal";
const getSwal = () => import('sweetalert2');

import "./payments.css";
import '../../css/pages/manage/manage.scss';
import '../../shared/sharedlast';

const styles = {
  name: {
    verticalAlign: 'top',
    display: 'none',
    margin: 0,
    border: 'none',
    fontSize: "16px",
    fontFamily: "Helvetica Neue",
    padding: "16px",
    color: "#373F4A",
    backgroundColor: "transparent",
    lineHeight: "1.15em",
    placeholderColor: "#000",
    _webkitFontSmoothing: "antialiased",
    _mozOsxFontSmoothing: "grayscale",
  },
  leftCenter: {
    float: 'left',
    textAlign: 'center'
  },
  blockRight: {
    display: 'block',
    float: 'right'
  },
  center: {
    textAlign: 'center'
  }
}

function PaymentForm(props) {
  const [cardBrand, setCardBrand] = useState("");
  const [nonce, setNonce] = useState(null);
  const [googlePay, setGooglePay] = useState(false);
  const [applePay, setApplePay] = useState(false);
  const [masterpass, setMasterpass] = useState(false);

  var paymentForm = useRef(null);

  function requestCardNonce() {
    paymentForm.current.requestCardNonce();
  }

  useEffect(() => {
    console.log("Nonce is", nonce);
  }, [nonce]);

  useEffect(() => {
    async function initialize() {
      const config = {
        applicationId: "sq0idp-rARHLPiahkGtp6mMz2OeCA",
        locationId: "GMT96A77XABR1",
        inputClass: "sq-input",
        autoBuild: false,
        inputStyles: [
          {
            fontSize: "16px",
            fontFamily: "Helvetica Neue",
            padding: "16px",
            color: "#373F4A",
            backgroundColor: "transparent",
            lineHeight: "1.15em",
            placeholderColor: "#000",
            _webkitFontSmoothing: "antialiased",
            _mozOsxFontSmoothing: "grayscale"
          }
        ],
        applePay: {
          elementId: 'sq-apple-pay'
        },
        masterpass: {
          elementId: 'sq-masterpass'
        },
        googlePay: {
          elementId: 'sq-google-pay'
        },
        cardNumber: {
          elementId: "sq-card-number",
          placeholder: "• • • •  • • • •  • • • •  • • • •"
        },
        cvv: {
          elementId: "sq-cvv",
          placeholder: "CVV"
        },
        expirationDate: {
          elementId: "sq-expiration-date",
          placeholder: "MM/YY"
        },
        postalCode: {
          elementId: "sq-postal-code",
          placeholder: "Zip"
        },
        callbacks: {
          methodsSupported: (methods) => {
            console.log("methodsSupported", methods);

            if (methods.googlePay) {
              setGooglePay(methods.googlePay);
            }
            if (methods.applePay) {
              setApplePay(methods.applePay);
            }
            if (methods.masterpass) {
              setMasterpass(methods.masterpass);
            }
            return;
          },
          createPaymentRequest: () => {
            return {
              requestShippingAddress: false,
              requestBillingInfo: true,
              currencyCode: "CAD",
              countryCode: "CA",
              total: {
                label: "MERCHANT NAME",
                amount: "100",
                pending: false
              },
              lineItems: [
                {
                  label: "Subtotal",
                  amount: "100",
                  pending: false
                }
              ]
            };
          },
          cardNonceResponseReceived: (errors, nonce, cardData) => {
            if (errors) {
              // Log errors from nonce generation to the Javascript console
              console.log("Encountered errors:");
              errors.forEach(function (error) {
                console.log("  " + error.message);
              });

              return;
            }
            setNonce(nonce);
            console.log({ nonce, cardData });
          },
          unsupportedBrowserDetected: () => {
          },
          inputEventReceived: (inputEvent) => {
            switch (inputEvent.eventType) {
              case "focusClassAdded":
                break;
              case "focusClassRemoved":
                break;
              case "errorClassAdded":
                document.getElementById("error").innerHTML =
                  "Please fix card information errors before continuing.";
                break;
              case "errorClassRemoved":
                document.getElementById("error").style.display = "none";
                break;
              case "cardBrandChanged":
                if (inputEvent.cardBrand !== "unknown") {
                  setCardBrand(inputEvent.cardBrand);
                } else {
                  setCardBrand("");
                }
                break;
              case "postalCodeChanged":
                break;
              default:
                break;
            }
          },
          paymentFormLoaded: function () {
            document.getElementById('name').style.display = "inline-flex";
          }
        }
      };

      paymentForm.current = new props.paymentForm(config);
      paymentForm.current.build();
    }
    initialize();
  }, []); // Fire once

  return (
  <>
    <div className="container" style={{marginBottom:"200px"} }>
      <div id="form-container">
        <div id="sq-walletbox">
          <button style={{ display: (applePay) ? 'inherit' : 'none' }}
            className="wallet-button"
            id="sq-apple-pay"></button>
          <button style={{ display: (masterpass) ? 'block' : 'none' }}
            className="wallet-button"
            id="sq-masterpass"></button>
          <button style={{ display: (googlePay) ? 'inherit' : 'none' }}
            className="wallet-button"
            id="sq-google-pay"></button>
          <hr />
        </div>

        <div id="sq-ccbox">
          <p>
            <span style={styles.leftCenter}>Enter Card Info Below </span>
            <span style={styles.blockRight}>
              {cardBrand.toUpperCase()}
            </span>
          </p>
          <div id="cc-field-wrapper">
            <div id="sq-card-number"></div>
            <input type="hidden" id="card-nonce" name="nonce" />
            <div id="sq-expiration-date"></div>
            <div id="sq-cvv"></div>
          </div>
          <input
            id="name"
            style={styles.name}
            type="text"
            placeholder="Name"
          />
          <div id="sq-postal-code"></div>
        </div>
        <button className="button-credit-card"
          onClick={requestCardNonce}>Pay</button>
      </div>
      <p style={styles.center} id="error"></p>
    </div>
  </>);
}

function Page() {
  const [subscriptions, setSubscriptions] = useState([]);
  const [purchasing, setPurchasing] = useState({Name: "", Price: 0});
  const [showManageModal, setShowManageModal] = useState(false);
  const [showStartSubscriptionModal, setShowStartSubscriptionModal] = useState(false);
  const [sqLoaded, setSqLoaded] = useState(false);

  async function loadSubscriptions() {
    getJson("/api/v1/account/purchases/subscriptions/")
      .then((response) => {
      //console.log(response);
      if (response.success) {
        setSubscriptions(response.Subscriptions);
        console.log(response);
      }
    });
  }

  useEffect(() => {
    async function initialize() {
      await loadSubscriptions();
      let sqPaymentScript = document.createElement("script");
      sqPaymentScript.src = "https://js.squareup.com/v2/paymentform";
      sqPaymentScript.type = "text/javascript";
      sqPaymentScript.async = false;
      sqPaymentScript.onload = () => {
        setSqLoaded(true);
      };
      document.getElementsByTagName("head")[0].appendChild(sqPaymentScript);
    }
    initialize();
  }, []); // Fire once

  const onSubscribe = (sub) => {
    setPurchasing(sub);
    setShowStartSubscriptionModal(true);

    if (false) { //debug
      getSwal().then(({ default: Swal }) => {
        const isSubscribed = subscriptions.filter((s) => s.IsSubscribed).length > 0;

        if (isSubscribed) {
          Swal.fire({
            title: "Already Subscribed",
            text: "You are already subscribed to another plan.  Do you wish to change your subscription?",
            icon: 'warning',
            showCancelButton: true
          }).then((result) => {
            if (result.isConfirmed) {
              console.log(result);
            }
          });
        } else {
          setShowStartSubscriptionModal(true);
        }
      });
    }
    
  };

  return (
    <>
      <Suspense fallback={<></>}>
        <ManageSubscriptionModal
          show={showManageModal}
          onClose={() => setShowManageModal(false)}
          onUpdated={(value) => { }} />
      </Suspense>

      

        <StartSubscriptionModal
          show={showStartSubscriptionModal}
          product={purchasing}
          onClose={() => setShowStartSubscriptionModal(false)}
          onUpdated={(value) => { }} />
      

      <div className="wrapper wrapper-content">
        <div className="ibox-content">
          <Row>
            <Col className="mt-5">
              <h1>Subscription Plans</h1>
            </Col>
          </Row>
          <Row>
            <Col lg={2}></Col>
            <Col lg={8}>
          <br /><br />
          <CardDeck>
            {subscriptions.map((sub, ix) => (
              <Card key={sub.Id} style={sub.IsSubscribed ? { border: "2px solid #1ab394" } : { border: "2px solid #1ab394" } }>
                <Card.Header className="zr-bg-primary" style={{ fontSize: "large", textAlign: "center", fontWeight: "bold" }}>
                  {sub.Name}
                </Card.Header>
                <Card.Body>
                  <Card.Title>{ sub.Subtitle }</Card.Title>
                  <Card.Subtitle className="mb-2 text-muted">CAD${ sub.Price } monthly</Card.Subtitle>
                  <Card.Text>
                    {sub.IsSubscribed ? (<><b>You are subscribed</b></>): (<>This subscription gives you</>) }
                  </Card.Text>
                  <div dangerouslySetInnerHTML={{ __html: sub.DescriptionHTML }}/>
                </Card.Body>

                <Card.Footer className="text-center">
                  {sub.IsSubscribed ? (
                    <>
                      <Button variant="outline-primary"
                        onClick={() => setShowManageModal(true)}>
                        Manage Subscription
                      </Button>
                    </>) : (
                      <>
                        <Button variant="primary"
                          onClick={() => onSubscribe(sub)}>
                          Subscribe Now
                        </Button>
                    </>)}
                </Card.Footer>
              </Card>))}
          </CardDeck>
            </Col>
          </Row>
          {/*<Row>*/}
          {/*  <Col>*/}
          {/*    <h1>Reactions</h1>*/}
          {/*  </Col>*/}
          {/*</Row>*/}
          {/*<Row>*/}
          {/*  <Col>*/}
          {/*    Comming soon.*/}
          {/*  </Col>*/}
          {/*</Row>*/}
        </div>
      </div>
      {sqLoaded ? (
        <>
          Loaded
          <PaymentForm paymentForm={window.SqPaymentForm} />
        </>
      ) : (
        <>
          Loading
        </>)}

      <div style={{marginBottom:"100px"}}></div>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));