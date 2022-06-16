/*
 * Section describing how ZapRead works
 */

import React, { useEffect, useState } from "react";
import { Row, Col, Card } from "react-bootstrap";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faUser,
  faChevronUp,
  faChevronDown,
  faUsers,
  faUserGroup,
  faWarehouse
} from '@fortawesome/free-solid-svg-icons'
import LeaderLine from "react-leader-line";

export default function HowItWorks(props) {
  useEffect(() => {
    var up = document.getElementById("up");
    var down = document.getElementById("down");
    var author = document.getElementById("author");
    var group = document.getElementById("group");
    var community = document.getElementById("community");
    var platform = document.getElementById("platform");
    var platformText = document.getElementById("platform-text");
    var referring = document.getElementById("referring");
    var referred = document.getElementById("referred");

    new LeaderLine(up, author,
      {
        color: "#1ab394",
        startSocket: 'right',
        endSocket: 'left',
        endLabel: LeaderLine.captionLabel('60%', { outlineColor: '' }),
        size: 6,
        dash: {
          animation: true
        }
      });

    new LeaderLine(up, group,
      {
        color: "#1ab394",
        startSocket: 'right',
        endSocket: 'left',
        endLabel: LeaderLine.captionLabel('20%', { outlineColor: '' }),
        size: 4,
        dash: {
          animation: true
        }
      });

    new LeaderLine(up, community,
      {
        color: "#1ab394",
        startSocket: 'right',
        endSocket: 'left',
        endLabel: LeaderLine.captionLabel('10%', { outlineColor: '' }),
        size: 3,
        dash: {
          animation: true
        }
      });

    new LeaderLine(up, platform,
      {
        color: "#1ab394",
        startSocket: 'right',
        endSocket: 'left',
        endLabel: LeaderLine.captionLabel('10%', { outlineColor: '' }),
        size: 3,
        dash: {
          animation: true
        }
      });

    new LeaderLine(down, group,
      {
        color: "#1ab394",
        startSocket: 'left',
        endSocket: 'right',
        endLabel: LeaderLine.captionLabel('80%', { outlineColor: '' }),
        size: 8,
        dash: {
          animation: true
        }
      });

    new LeaderLine(down, community,
      {
        color: "#1ab394",
        startSocket: 'left',
        endSocket: 'right',
        endLabel: LeaderLine.captionLabel('10%', { outlineColor: '' }),
        size: 3,
        dash: {
          animation: true
        }
      });

    new LeaderLine(down, platform,
      {
        color: "#1ab394",
        startSocket: 'left',
        endSocket: 'right',
        endLabel: LeaderLine.captionLabel('10%', { outlineColor: '' }),
        size: 3,
        dash: {
          animation: true
        }
      });

    new LeaderLine(platformText, referring,
      {
        color: "#1ab394",
        startSocket: 'bottom',
        endSocket: 'right',
        path: "straight",
        //startSocketGravity: [-100, 100],
        //endSocketGravity: [40, -10],
        endLabel: LeaderLine.captionLabel('5%', { outlineColor: '', offset: [0, -40] }),
        size: 3,
        dash: {
          animation: true
        }
      });

    new LeaderLine(platformText, referred,
      {
        color: "#1ab394",
        startSocket: 'bottom',
        endSocket: 'left',
        path: "straight",
        //startSocketGravity: [100, 100],
        //endSocketGravity: [-40, -10],
        endLabel: LeaderLine.captionLabel('5%', { outlineColor: '', offset: [-20, -40] }),
        size: 3,
        dash: {
          animation: true
        }
      });
  }, []); // Fire once

  return (
    <section id="howitworks" className="text-left-img-right">
      <Row className="works-header">
        <Col className="text-center wow animate__fadeIn">
          <div className="navy-line"></div>
          <h1>How ZapRead Works</h1>
        </Col>
      </Row>
      <Row>
        <Col md={2}></Col>
        <Col>
          <Row>
            <Col className="text-center">
              <div className="wow animate__fadeIn">
                {/*<i className="fa-solid fa-users mission-icon"></i>*/}
                <span className="wow animate__bounceIn"><b>Completely free</b></span> to join and start publishing! <br />
                Earn your first (or 21st) Bitcoin here.
              </div>
            </Col>
            <Col md={2}></Col>
            <Col className="text-center">
              <div className="wow animate__fadeIn">
                {/*<i className="fa-solid fa-users mission-icon"></i>*/}
                Publish your content in posts and discuss published content with others.
                Users vote on posts and comments using Bitcoin on the Lightning Network.
              </div>
            </Col>
          </Row>
          <Row style={{ paddingTop: "30px", paddingBottom: "30px"}}>
            <Col className="align-self-center text-center">
              <div id="up" className="how-icon" ><FontAwesomeIcon icon={faChevronUp} /></div><br /><span className="how-dia-text">Upvote</span>
            </Col>
            <Col className="text-center">
              <div id="author" className="how-icon"><FontAwesomeIcon icon={faUser} /></div><br />Author
              <br/>
              <div id="group" className="how-icon" ><FontAwesomeIcon icon={faUserGroup} /></div><br /> Group
              <br />
              <div id="community" className="how-icon"><FontAwesomeIcon icon={faUsers} /></div><br /> Community
              <br />
              <div id="platform" className="how-icon"><FontAwesomeIcon icon={faWarehouse} /></div><br /> <div id="platform-text">Platform</div>
            </Col>
            <Col className="align-self-center text-center">
              <div id="down" className="how-icon"><FontAwesomeIcon icon={faChevronDown} /></div><br/><span className="how-dia-text">Downvote</span>
            </Col>
          </Row>
          <Row>
            <Col className="align-self-center text-center"></Col>
            <Col className="text-center">
              <div id="referring" className="how-icon"><FontAwesomeIcon icon={faUser} /></div><br />Referring User
            </Col>
            <Col className="text-center">
              <div id="referred" className="how-icon"><FontAwesomeIcon icon={faUser} /></div><br />Referred User
            </Col>
            <Col className="align-self-center text-center"></Col>
          </Row>
          <Row>
            <Col md={2}></Col>
            <Col className="text-center">
              <br/>
              <h3>Every day, funds collected in the Group and Community are paid out to the top authors.</h3>
            </Col>
            <Col md={2}></Col>
          </Row>
        </Col>
        <Col md={2}></Col>
      </Row>
    </section>)
}