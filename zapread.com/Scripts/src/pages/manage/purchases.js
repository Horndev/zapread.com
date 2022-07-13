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
import React, { Suspense, useEffect, useState } from "react";
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
const getSwal = () => import('sweetalert2');

import '../../css/pages/manage/manage.scss';
import '../../shared/sharedlast';

function Page() {
  const [subscriptions, setSubscriptions] = useState([]);
  const [showManageModal, setShowManageModal] = useState(false);

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
    }
    initialize();
  }, []); // Fire once

  const onSubscribe = (id) => {
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
        Swal.fire({
          title: 'Subscribe',
          text: "",
          icon: 'warning',
          showCancelButton: true
        }).then((result) => {
          if (result.isConfirmed) {
            console.log(result);
          }
        });
      }
    });
  };

  return (
    <>
      <Suspense fallback={<></>}>
        <ManageSubscriptionModal
          show={showManageModal}
          onClose={() => setShowManageModal(false)}
          onUpdated={(value) => { }} />
      </Suspense>

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
                          onClick={() => onSubscribe(sub.Id)}>
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
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));