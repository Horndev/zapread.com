/*
 * 
 */

import React from 'react';
import { Container, Row, Col } from "react-bootstrap";

export default function UserFinanceInfo(props) {

  return (
    <>
      <div className="ibox-content profile-content">
        <h4>Customization</h4>
        <Container>
          <Row>
            <Col>
              <a href="/Manage/Financial/" className="btn btn-block btn-primary btn-outline">Account Financial</a>
            </Col>
          </Row>
        </Container>
      </div>
    </>
  );
}