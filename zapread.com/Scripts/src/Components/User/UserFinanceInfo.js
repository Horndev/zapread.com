/*
 * 
 */

import React, { useState, useEffect, createRef } from 'react';
import { Container, Row, Col } from "react-bootstrap";

import { postJson } from "../../utility/postData";


export default function UserFinanceInfo(props) {
  const [settings, setSettings] = useState(null);
  const [knownLanguages, setKnownLanguages] = useState([]);
  const [languages, setLanguages] = useState([]);
  const [langSelector, setLangSelector] = useState(null);
  const languageInputRef = createRef();

  useEffect(() => {
    async function initialize() {
      
    };
    initialize();
  }, []); // Update after shown

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