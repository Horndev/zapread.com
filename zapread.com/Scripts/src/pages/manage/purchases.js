/**
 * User purchases page
 */

import "../../shared/shared";
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { Container, Row, Col, Button, Card, CardDeck } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faArrowDown,
  faCircleNotch
} from '@fortawesome/free-solid-svg-icons'

import '../../css/pages/manage/manage.scss';
import '../../shared/sharedlast';

function Page() {

  useEffect(() => {
    async function initialize() {
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <Row>
        <Col>
          <h1>Subscription Plans</h1>
        </Col>
      </Row>
      <br /><br />
      <CardDeck>
        <Card>
          <Card.Header className="bg-primary" style={{ fontSize: "large", textAlign: "center", fontWeight: "bold" }}>Supporter</Card.Header>
          <Card.Body>
            <Card.Title>Tier 1</Card.Title>
            <Card.Subtitle className="mb-2 text-muted">$2 monthly</Card.Subtitle>
            <Card.Text>
              You get
              <hr/ >
              $2 worth of Bitcoin in your balance to spend
              <hr/>
              1 random reaction unlock per month
              <hr/>
              an optional icon next to your username. 
            </Card.Text>
          </Card.Body>
          <Card.Footer>
            <Button variant="link">Credit Card</Button>
            <Button variant="link">Bitcoin</Button>
          </Card.Footer>
        </Card>

        <Card>
          <Card.Header className="bg-primary" style={{ fontSize: "large", textAlign: "center", fontWeight: "bold" }}>
            Enthusiast
          </Card.Header>
          <Card.Body>
            <Card.Title>Tier 2</Card.Title>
            <Card.Subtitle className="mb-2 text-muted">Subscribe $5 monthly</Card.Subtitle>
            <Card.Text>
              You get $5 worth of Bitcoin in your balance to spend﻿, 2 random reaction unlocks per month, an optional icon next to your username, and your username will be optionally a different colour.
            </Card.Text>
          </Card.Body>
          <Card.Footer>
            <Button variant="link">Credit Card</Button>
            <Button variant="link">Bitcoin</Button>
          </Card.Footer>
        </Card>

        <Card>
          <Card.Header className="bg-primary" style={{ fontSize: "large", textAlign: "center", fontWeight: "bold" }}>
            Backer
          </Card.Header>
          <Card.Body>
            <Card.Title>Tier 3</Card.Title>
            <Card.Subtitle className="mb-2 text-muted">Subscribe $10 monthly</Card.Subtitle>
            <Card.Text>
              You get $10 worth of Bitcoin in your balance to spend, 3 random reaction unlocks per month, an optional icon next to your username, an optional different colour username, and an optional animation on your username.
            </Card.Text>
          </Card.Body>
          <Card.Footer>
            <Button variant="link">Credit Card</Button>
            <Button variant="link">Bitcoin</Button>
          </Card.Footer>
        </Card>
      </CardDeck>

      <Container>
        <Row>
          <Col>
            <h1>Reactions</h1>
          </Col>
        </Row>
        <Row>
          <Col>

          </Col>
        </Row>
      </Container>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));