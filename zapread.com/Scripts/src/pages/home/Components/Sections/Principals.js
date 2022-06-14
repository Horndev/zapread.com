
import React, { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCircleNodes } from '@fortawesome/free-solid-svg-icons'

export default function Principals(props) {

  return (
    <section id="principals">
      <Row className="principals-header">
        <Col className="text-center">
          <div className="navy-line"></div>
          <h1>Our Principals</h1>
          <p>Let's build this platform together</p>
        </Col>
      </Row>
      <Row>
        <Col md={1}></Col>
        <Col lg={4} className="text-center mx-5">
          <div>
            <h2>Decentralization of Authority</h2>
            <i className="principal-icon float-right">
              <FontAwesomeIcon icon={faCircleNodes} />
            </i>
            <p>
              Community driven moderation which uses a reputation system to empower the user community.
              Content is grouped into communities of interest to ensure that every voice has a fair platform.
            </p>
          </div>
        </Col>
        <Col lg={4} className="text-center mx-5">
          <div>
            <h2>Freedom of Thought</h2>
            <i className="fa-solid fa-lightbulb principal-icon float-right"></i>
            <p>
              It's ok to disagree.  Let's have a conversation instead of censorship.
            </p>
          </div>
        </Col>
        <Col md={1}></Col>
      </Row>
      <Row>
        <Col md={1}></Col>
        <Col lg={4} className="text-center mx-5">
          <div>
            <h2>Game-theoretic controls</h2>
            <i className="fa-solid fa-chess principal-icon float-right"></i>
            <p>
              A self-regulating incentives system encourages positive interactions.
              It doesn't pay to spam or post poor content
            </p>
          </div>
        </Col>
        <Col lg={4} className="text-center mx-5">
          <div>
            <h2>No Paywalls</h2>
            <i className="fa-solid fa-book-open principal-icon float-right"></i>
            <p>
              "I love paywalls"<br />
              - Said Nobody!
            </p>
            <p>
              It's completely free to start publishing and earning.
            </p>
          </div>
        </Col>
        <Col md={1}></Col>
      </Row>
      <Row>
        <Col md={1}></Col>
        <Col lg={4} className="text-center mx-5">
          <div>
            <h2>Bitcoin Focus</h2>
            <i className="fa-brands fa-btc principal-icon float-right"></i>
            <p>
              Bitcoin is unique.  With no centralized control or authority, it is a real hard money.
              Earn and spend Bitcoin using the Lightning Network.
            </p>
          </div>
        </Col>
        <Col lg={4} className="text-center mx-5">
          <div>
            <h2>Privacy</h2>
            <i className="fa-solid fa-lock principal-icon float-right"></i>
            <p>
              You own your data.  Your publishing and discussions are your own.
              Your anonymous lightning wallet can be used as your identity.
            </p>
          </div>
        </Col>
        <Col md={1}></Col>
      </Row>
    </section>
  );
}