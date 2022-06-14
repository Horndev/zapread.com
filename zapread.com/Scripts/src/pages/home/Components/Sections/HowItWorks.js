
import React, { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";

export default function HowItWorks(props) {

  return (
    <section id="howitworks" className="text-left-img-right">
      <Row className="mission-header">
        <Col className="text-center">
          <div className="navy-line"></div>
          <h1>How ZapRead Works</h1>
        </Col>
      </Row>
      <Row>
        <Col md={2}></Col>
        <Col>
          <p>
            Completely free to join and start publishing
            <small>Encouraging adoption.</small>
          </p>
          <p>
          Earn your first Bitcoin here!
          Content is posts & comments in groups
          Groups have admins and moderators
          Moderators are curators, not censors
          </p>
          <p>
            Users vote on posts and comments using Sats
          60% to Author, 20% Group, 10% Community, 10% ZapRead
          80% to Group,  10% Community, 10% ZapRead
          Votes adjust user reputation
          </p>
        </Col>
        <Col md={2}></Col>
      </Row>
    </section>)
}