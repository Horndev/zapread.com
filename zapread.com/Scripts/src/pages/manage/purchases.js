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

 */

import "../../shared/shared";
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState, useRef } from "react";
import ReactDOM from "react-dom";
import { Container, Row, Col, Button, Card, CardDeck } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";
const ManageSubscriptionModal = React.lazy(() => import("./Components/ManageSubscriptionModal"));
import StartSubscriptionModal from "./Components/StartSubscriptionModal";
const getSwal = () => import('sweetalert2');

import "./purchases.css";
import '../../css/pages/manage/manage.scss';
import '../../shared/sharedlast';

function Page() {
  const [subscriptions, setSubscriptions] = useState([]);
  const [purchasing, setPurchasing] = useState({Name: "", Price: 0});
  const [showManageModal, setShowManageModal] = useState(false);
  const [showStartSubscriptionModal, setShowStartSubscriptionModal] = useState(false);
  const [applicationId, setApplicationId] = useState("");
  const [locationId, setLocationId] = useState("");

  async function loadSubscriptions() {
    getJson("/api/v1/account/purchases/subscriptions/")
      .then((response) => {
      if (response.success) {
        setSubscriptions(response.Subscriptions);
        // These are the identifiers to use for the Point of Sale service API
        setLocationId(response.LocationId);
        setApplicationId(response.ApplicationId);
      }
    });
  }

  useEffect(() => {
    async function initialize() {
      await loadSubscriptions();
    }
    initialize();
  }, []); // Fire once

  const onSubscribe = (sub) => {
    setPurchasing(sub);
    //setShowStartSubscriptionModal(true);

    const isSubscribed = subscriptions.filter((s) => s.IsSubscribed).length > 0;
    const isEnding = subscriptions.filter((s) => s.IsEnding).length > 0;

    if (isSubscribed) { //debug
      getSwal().then(({ default: Swal }) => {
        if (isSubscribed && !isEnding) {
          Swal.fire({
            title: "Already Subscribed",
            text: "You are already subscribed to another plan.  Do you wish to change your subscription?",
            icon: 'warning',
            showCancelButton: true
          }).then((result) => {
            //console.log(result);
            if (result.isConfirmed) {
              setShowStartSubscriptionModal(true);
            }
          });
        } else if (isSubscribed && isEnding) {
          Swal.fire({
            title: "Subscription still active",
            text: "Your existing subscription is still ending.  If you change your subscription, you will lose the remainder of time remaining on the previous subscription.  Do you wish to change your subscription now?",
            icon: 'warning',
            showCancelButton: true
          }).then((result) => {
            //console.log(result);
            if (result.isConfirmed) {
              setShowStartSubscriptionModal(true);
            }
          });
        }
      });
    }
  };

  const onSubscribed = (planId) => {
    setShowStartSubscriptionModal(false);
    setShowManageModal(false);
    var newSubscriptions = subscriptions;
    var foundIndex = newSubscriptions.findIndex(x => x.PlanId == planId);
    newSubscriptions[foundIndex].IsSubscribed = true;
    setSubscriptions(newSubscriptions);
    getSwal().then(({ default: Swal }) => {
      Swal.fire("Success", "You are now subscribed", "success");
    });
  }

  const onUnsubscribed = (subId) => {
    setShowManageModal(false);
    var newSubscriptions = subscriptions;
    var foundIndex = newSubscriptions.findIndex(x => x.SubscriptionId == subId);
    newSubscriptions[foundIndex].IsEnding = true;
    setSubscriptions(newSubscriptions);
    getSwal().then(({ default: Swal }) => {
      Swal.fire("Success", "You have stopped your subscription", "success");
    });
  }

  return (
    <>
      <Suspense fallback={<></>}>
        <ManageSubscriptionModal
          product={purchasing}
          show={showManageModal}
          onClose={() => setShowManageModal(false)}
          onSubscribed={onSubscribed}
          onUnubscribed={onUnsubscribed}
          onUpdated={(value) => { }} />
      </Suspense>

      <StartSubscriptionModal
        product={purchasing}
        show={showStartSubscriptionModal}
        applicationId={applicationId}
        locationId={locationId}
        onClose={() => setShowStartSubscriptionModal(false)}
        onUpdated={(value) => { }}
        onSubscribed={onSubscribed}
      />
      
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
                    {sub.IsSubscribed ? (<><b>{sub.IsEnding ? (<>Your subscription is ending soon</>) : (<>You are subscribed</>)}</b></>) : (<>This subscription gives you</>)}
                  </Card.Text>
                  <div dangerouslySetInnerHTML={{ __html: sub.DescriptionHTML }}/>
                </Card.Body>

                <Card.Footer className="text-center">
                  {(sub.IsSubscribed || sub.IsEnding) ? (
                    <>
                      <Button variant="outline-primary"
                        onClick={() => {
                          setPurchasing(sub);
                          setShowManageModal(true);
                        }}>
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

      <div style={{ paddingBottom: "100px", marginBottom:"100px"}}></div>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));