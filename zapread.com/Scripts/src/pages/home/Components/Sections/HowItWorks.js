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
  faWarehouse,
  faMedal,
  faStar,
  faCoins,
  faTrophy,
  faHeart,
  faPeopleGroup
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

    new LeaderLine(up, author,
      {
        color: "#1ab394",
        startSocket: 'right',
        endSocket: 'left',
        endLabel: LeaderLine.captionLabel('60%'),
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
        endLabel: LeaderLine.captionLabel('20%'),
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
        endLabel: LeaderLine.captionLabel('10%'),
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
        endLabel: LeaderLine.captionLabel('10%'),
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
        endLabel: LeaderLine.captionLabel('80%'),
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
        endLabel: LeaderLine.captionLabel('10%'),
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
        endLabel: LeaderLine.captionLabel('10%'),
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
              <div className="wow animate__zoomIn">
                {/*<i className="fa-solid fa-users mission-icon"></i>*/}
                <span className="wow animate__bounceIn"><b>Completely free</b></span> to join and start publishing! <br />
                Earn your first (or 21st) Bitcoin here.
              </div>
            </Col>
            <Col className="text-center">
              <div className="wow animate__zoomIn">
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
              <div id="platform" className="how-icon"><FontAwesomeIcon icon={faWarehouse} /></div><br /> Platform
            </Col>
            <Col className="align-self-center text-center">
              <div id="down" className="how-icon"><FontAwesomeIcon icon={faChevronDown} /></div><br/><span className="how-dia-text">Downvote</span>
            </Col>
          </Row>
          <Row>
            <Col>
              <Card className="wow animate__zoomIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faStar} /></span>
                    <br/>
                    Reputation
                  </Card.Title>
                </Card.Body>
                <Card.Text>
                  Earning upvotes on your comments and posts increases your reputation.  Users voting down reduces your reputation.

                  The higher your reputation: <br />
                  <ul>
                    <li>
                      the more your votes count in adjusting the score.
                    </li>
                    <li>
                      the more weight your posts and comments have from downvotes.
                    </li>
                  </ul>
                </Card.Text>
              </Card>
            </Col>
            <Col>
              <Card className="wow animate__zoomIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faMedal} /></span>
                    <br />
                    Score</Card.Title>
                </Card.Body>
                <Card.Text>
                  Recent and higher scoring content will be brought to the top for more exposure.

                  Older and lower scoring content will be less visible.
                </Card.Text>
              </Card>
            </Col>
            <Col>
              <Card className="wow animate__zoomIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faCoins} /></span>
                    <br />
                    Earning</Card.Title>
                </Card.Body>
                <Card.Text>
                  The more frequently you post high quality content, the more you can earn, and the higher your reputation.  Spam and low quality content will not earn you as much Bitcoin.

                </Card.Text>
              </Card>
            </Col>
          </Row>
          <Row>
            <Col>
              <Card className="wow animate__zoomIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faTrophy} /></span>
                    <br />
                    Achievements</Card.Title>
                </Card.Body>
                <Card.Text>
                  There are several achievements on ZapRead that can be unlocked.  The more you explore the website, the more achievements you may discover.
                </Card.Text>
              </Card>
            </Col>
            <Col>
              <Card className="wow animate__zoomIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faUserGroup} /></span>
                    <br />
                    Groups</Card.Title>
                </Card.Body>
                <Card.Text>
                  Groups are sub-communities with administrators and moderators.  A portion of group revenue goes into a group fund which moderators can use to curate group content.

                  <h3>Admins</h3>
                  Group administrators can configure the style of the group, grand and revoke moderation privilages

                  <h3>Mods</h3>
                  Group moderators are curators, not censors.  They can promote content, manage community guidelines, and handle any issues within the group.
                </Card.Text>
              </Card>
            </Col>
            <Col>
              <Card className="wow animate__zoomIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faHeart} /></span>
                    <br />
                    Reactions</Card.Title>
                </Card.Body>
                <Card.Text>
                  You earn reactions when you unlock achievements.
                  You can add reactions to posts and comments as a quick and fun interaction.  Posts with more users reacting are more likely to be brought to the top for more exposure.

                  You will also be able to purchase reactions from the ZapRead store.
                </Card.Text>
              </Card>
            </Col>
          </Row>
        </Col>
        <Col md={2}></Col>
      </Row>
    </section>)
}