
import React, { useEffect, useState } from "react";
import { Row, Col, Card } from "react-bootstrap";

export default function MoreInfo(props) {
  return (
    <section id="moreinfo" className="text-left-img-right">
      <Row className="mission-header">
        <Col className="text-center wow animate__fadeIn">
          <div className="navy-line"></div>
          <h1>More Information</h1>
        </Col>
      </Row>
      <Row>
        <Col lg={4}></Col>
        <Col className="text-center">
          <Card className="wow animate__zoomIn">
            <Card.Body>
              <Card.Title>Privacy Policy</Card.Title>
            </Card.Body>
            <Card.Text>
              We value your privacy.  Read about how we protect it.
            </Card.Text>
            <Card.Link href="/Home/Privacy">Read Policy</Card.Link>
          </Card>
        </Col>
        <Col className="text-center">
          <Card className="wow animate__zoomIn">
            <Card.Body>
              <Card.Title>Terms of Use</Card.Title>
            </Card.Body>
            <Card.Text>
              ZapRead is an open platform which gives users great freedom. 
            </Card.Text>
            <Card.Link href="/Home/Terms">Read Terms</Card.Link>
          </Card>
        </Col>
        <Col lg={4}></Col>
      </Row>
    </section>
  );
}