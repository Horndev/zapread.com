
import React, { useEffect, useState } from "react";
import { Row, Col, Card } from "react-bootstrap";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faUserGroup, faStar, faMedal, faCoins, faTrophy, faHeart
} from '@fortawesome/free-solid-svg-icons'

export default function MoreInfo(props) {
  return (
    <section id="moreinfo" className="text-left-img-right">
      <Row className="stats-header">
        <Col className="text-center wow animate__fadeIn">
          <div className="navy-line"></div>
          <h1>More Information</h1>
        </Col>
      </Row>
      <Row>
        <Col md={2}></Col>
        <Col>
          <Row>
            <Col>
              <Card className="wow animate__fadeIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faStar} /></span>
                    <br />
                    Reputation
                  </Card.Title>
                </Card.Body>
                <Card.Text>
                  Earning upvotes on your comments and posts increases your reputation.
                  Receiving downvotes reduces your reputation.

                  The higher your reputation: <br />
                  • The more your votes count in adjusting the score. <br />
                  • The more weight your posts and comments have from downvotes. <br />
                </Card.Text>
              </Card>
            </Col>
            <Col>
              <Card className="wow animate__fadeIn">
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
              <Card className="wow animate__fadeIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faCoins} /></span>
                    <br />
                    Earning</Card.Title>
                </Card.Body>
                <Card.Text>
                  The more frequently you post high quality content, the more you can earn, and the higher your reputation.
                  Spam and low quality content will not earn you as much Bitcoin.
                </Card.Text>
              </Card>
            </Col>
          </Row>
          <Row>
            <Col>
              <Card className="wow animate__fadeIn">
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
              <Card className="wow animate__fadeIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faUserGroup} /></span>
                    <br />
                    Groups</Card.Title>
                </Card.Body>
                <Card.Text>
                  Groups are sub-communities with administrators and moderators.  A portion of group revenue goes into a group fund which moderators can use to curate group content.
                  <br /><br />
                  <b>Admins</b><br />
                  Group administrators can configure the style of the group, grand and revoke moderation privilages
                  <br /><br />
                  <b>Mods</b><br />
                  Group moderators are curators, not censors.  They can promote content, manage community guidelines, and handle any issues within the group.
                </Card.Text>
              </Card>
            </Col>
            <Col>
              <Card className="wow animate__fadeIn">
                <Card.Body>
                  <Card.Title>
                    <span className="mission-icon"><FontAwesomeIcon icon={faHeart} /></span>
                    <br />
                    Reactions</Card.Title>
                </Card.Body>
                <Card.Text>
                  You earn reactions when you unlock achievements.
                  You can add reactions to posts and comments as a quick and fun interaction.  Posts with more users reacting are more likely to be brought to the top for more exposure.
                  <br />
                  You will also be able to purchase reactions from the ZapRead store.
                </Card.Text>
              </Card>
            </Col>
          </Row>
        </Col>
        <Col md={2}></Col>
      </Row>
      <div className="navy-line"></div>
      <Row>
        <Col lg={3}></Col>
        <Col className="text-center">
          <Card className="wow animate__fadeIn">
            <Card.Body>
              <Card.Title>Privacy Policy</Card.Title>
            </Card.Body>
            <Card.Text>
              We value your privacy.  Read about how we protect it.
            </Card.Text>
          </Card>
        </Col>
        <Col className="text-center">
          <Card className="wow animate__fadeIn">
            <Card.Body>
              <Card.Title>Terms of Use</Card.Title>
            </Card.Body>
            <Card.Text>
              ZapRead is an open platform which gives users great freedom. 
            </Card.Text>
          </Card>
        </Col>
        <Col className="text-center">
          <Card className="wow animate__fadeIn">
            <Card.Body>
              <Card.Title>Open Source</Card.Title>
            </Card.Body>
            <Card.Text>
              ZapRead is open source and licensed under <a className="btn btn-sm btn-link" href="https://github.com/Horndev/zapread.com/blob/master/LICENSE">AGPLv3</a>.
            </Card.Text>
          </Card>
        </Col>
        <Col lg={3}></Col>
      </Row>
      <Row className="wow animate__fadeIn">
        <Col lg={3}></Col>
        <Col className="text-center">
          <Card.Link className="btn btn-sm btn-link" href="/Home/Privacy">Read Policy</Card.Link>
        </Col>
        <Col className="text-center">
          <Card.Link className="btn btn-sm btn-link" href="/Home/Terms">Read Terms</Card.Link>
        </Col>
        <Col className="text-center">
          <Card.Link className="btn btn-sm btn-link" href="https://github.com/Horndev/zapread.com">View Source</Card.Link>
        </Col>
        <Col lg={3}></Col>
      </Row>
    </section>
  );
}