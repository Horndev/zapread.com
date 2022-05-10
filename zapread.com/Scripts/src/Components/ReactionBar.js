/**
 * A Reaction Bar attached to a post or comment
 */

import React, { useCallback, useEffect, useState, forwardRef } from "react";
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import Tippy from '@tippyjs/react';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';

//const ThisWillWork = forwardRef((props, ref) => {
//  return <button ref={ref}>Reference</button>;
//});

export default function ReactionBar(props) {
  const [addShown, setAddShown] = useState(true);
  const [availableReactions, setAvailableReactions] = useState([]);

  async function getReactions() {
    await fetch('/api/v1/user/reactions/list/')
      .then(response => response.json())
      .then(json => {
        setAvailableReactions(json.Reactions);
      });
  }

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
        await getReactions();
      }
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <div className="zr-reactions-bar" title={props.postId}>
        <div className="zr-reactions" style={{}}>
          {availableReactions.length > 0 ? (<>
            <Tippy
              theme="light-border"
              interactive={true}
              interactiveBorder={30}
              content={
                <Container title="React" style={{
                  letterSpacing: "normal",
                  fontSize: "18px",
                  width: "200px",
                  maxHeight: "50px",
                  overflowY: "scroll"
                }}>
                  <Row>
                    <Col>
                      {availableReactions.map((reaction, index) => (
                        <span key={reaction.ReactionId} className="reaction-icon" title="1" onClick={() => {alert("add")}}>
                          <div dangerouslySetInnerHTML={{ __html: reaction.ReactionIcon }} />
                        </span>
                      ))}
                    </Col>
                  </Row>
                </Container>
              }
            >
              <span className="reaction-icon-add-post" style={addShown ? {} : { display: "none" }}>+</span>
            </Tippy>
          </>) : (<></>)}
          <span className="reaction-icon">😀</span>
          <span className="reaction-icon">🤩</span>
          <span className="reaction-count">3</span>
          <span className="reaction-icon">😞</span>
          <span className="reaction-count">2</span>
          <span className="reaction-icon">😠</span>
          <span className="reaction-count">9</span>
          <Tippy
            theme="light-border"
            interactive={true}
            interactiveBorder={30}
            content={
              <Container style={{
                letterSpacing: "normal",
                fontSize: "18px",
                width: "200px",
                maxHeight: "60px",
                overflowY: "scroll"
              }}>
                <Row>
                  <Col>
                    <h4>More Reactions</h4>
                  </Col>
                </Row>
                <Row>
                  <Col>
                    <span className="reaction-icon" title="1">⚡</span>
                    <span className="reaction-icon">💕</span>
                    <span className="reaction-icon">❤️</span>
                    <span className="reaction-count">9</span>
                    <span className="reaction-icon">✨</span>
                    <span className="reaction-icon">🎉</span>
                    <span className="reaction-icon">🔥</span>
                    <span className="reaction-icon">₿</span>
                    <span className="reaction-icon">🚩</span>
                    <span className="reaction-count">9</span>
                    <span className="reaction-icon">😀</span>
                    <span className="reaction-icon">🚀</span>
                    <span className="reaction-icon">😀</span>
                    <span className="reaction-icon">🍆</span>
                    <span className="reaction-icon">😀</span>
                    <span className="reaction-icon">☘️</span>
                    <span className="reaction-icon">🛸</span>
                    <span className="reaction-icon">🎂</span>
                    <span className="reaction-icon">😀</span>
                  </Col>
                </Row>
              </Container>
            }>
            <span className="reaction-icon-more btn btn-link"><i className="fa-solid fa-circle-chevron-right"></i></span>
          </Tippy>
        </div>
      </div>
    </>
  );
}