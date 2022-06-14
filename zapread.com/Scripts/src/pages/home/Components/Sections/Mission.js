
import React, { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";

export default function Mission(props) {

  return (
    <section id="mission" className="text-left-img-right">
      <Row className="mission-header">
        <Col className="text-center wow animate__fadeIn">
          <div className="navy-line"></div>
          <h1>ZapRead.com Mission{/*<br /> <span className="mission-subtitle"> creating a social economy</span>*/} </h1>
        </Col>
      </Row>
      <Row>
        <Col lg={4}></Col>
        <Col className="text-center">
          <div className="wow animate__zoomIn">
            <i className="fa-solid fa-message mission-icon"></i>
            <h2>Create a social economy</h2>
            <p>
              Turn publishing, social media, and networking upside-down.
              Give users and the community control over their content.
              Stop the selling of user data and inundation of advertising without compensation.
            </p>
          </div>
          <div className="wow animate__zoomIn">
            <i className="fa-solid fa-users mission-icon"></i>
            <h2>Create a new publishing model</h2>
            <p>
              Authors should be properly compensated for their work by the value as determined by the consumers, not by the publisher.
            </p>
          </div>
          <div className="wow animate__zoomIn">
            <i className="fa-solid fa-chart-line mission-icon"></i>
            <h2>Adoption of decentralized finance and currency</h2>
            <p>
              Bitcoin and the lightning network for global trustless money.
            </p>
          </div>
        </Col>
        <Col lg={4}></Col>
      </Row>
    </section>
  );
}