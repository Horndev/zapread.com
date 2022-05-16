/*
 * React component for the top header and breadcrumb
 */
import React, { useState } from 'react';
import { ThemeColorContext } from "./Theme/ThemeContext";
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";

export default function PageHeading(props) {
  const [topGroupsIsExpanded, setTopGroupsIsExpanded] = useState(props.topGroupsExpanded);

  return (
    <ThemeColorContext.Consumer>
      {({ bgColor }) => (
        <div
          style={props.topGroups ? { paddingBottom: "0" } : {} }
          className={"wrapper border-bottom " + bgColor + "-bg page-heading"}>
          <Row>
            <Col lg={12}>
              <br />
              {props.pretitle}
              <h2>{props.title}</h2>
              <ol className="breadcrumb">
                <li className="breadcrumb-item"><a href="/">{props.controller}</a></li>
                <li className="breadcrumb-item"><a href="/">{props.method}</a></li>
                <li className="breadcrumb-item active">{props.function}</li>
              </ol>
              {props.children}
            </Col>
            <Col lg={2} />
          </Row>
          {props.topGroups ? (
            <Row>
              <Col lg={2}>
                <div className="ibox-title" style={{whiteSpace: "nowrap"}}>
                  <h5>
                    Your Top Groups
                  </h5>
                  <div className="ibox-tools">
                    <a
                      className="collapse-link"
                      onClick={() => {
                        topGroupsIsExpanded ? props.onTopGroupsClosed() : props.onTopGroupsOpened();
                        setTopGroupsIsExpanded(!topGroupsIsExpanded);
                      }}>
                      <i className={topGroupsIsExpanded ? "fa-solid fa-chevron-up" : "fa-solid fa-chevron-down"}></i>
                    </a>
                  </div>
                </div>
              </Col>
              <Col lg={8} />
              <Col lg={2} />
            </Row>
          ): (<></>)}
        </div>
      )}
    </ThemeColorContext.Consumer>
  );
}