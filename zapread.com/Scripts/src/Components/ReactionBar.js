/**
 * A Reaction Bar attached to a post or comment
 */

import React, { useCallback, useEffect, useState, forwardRef } from "react";
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import Tippy from '@tippyjs/react';
import { postJson } from "../utility/postData";
const getSwal = () => import('sweetalert2');
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';

//const ThisWillWork = forwardRef((props, ref) => {
//  return <button ref={ref}>Reference</button>;
//});

export default function ReactionBar(props) {
  const [isLoaded, setIsLoaded] = useState(false);
  const [addShown, setAddShown] = useState(true);
  const [availableReactions, setAvailableReactions] = useState([]);
  const [reactions, setReactions] = useState([]);
  const [commonReactions, setCommonReactions] = useState([]);

  async function getAvailableReactions() {
    await fetch('/api/v1/user/reactions/list/' + props.postId + '/')
      .then(response => response.json())
      .then(json => {
        setAvailableReactions(json.Reactions);
        setCommonReactions(json.CommonReactions);
      });
  }

  async function getReactions() {
    await fetch('/api/v1/post/reactions/list/' + props.postId + '/')
      .then(response => response.json())
      .then(json => {
        setReactions(json.Reactions);
      });
  }

  async function initialize() {
    if (!isLoaded) {
      setIsLoaded(true);
      await getAvailableReactions();
    }
  }

  async function react(reactionId) {
    if (props.l != "1") {
      getSwal().then(({ default: Swal }) => {
        Swal.fire("Please log in to add a reaction.");
        return;
      });
    }

    var reactionData = {
      ReactionId: reactionId,
      PostId: props.postId
    }

    postJson("/api/v1/post/reactions/add", reactionData).then(response => {
      console.log(response);
      if (response.success) {
        getAvailableReactions();
        //const newReactions = [...reactions, response.Reaction];
        if (response.AlreadyReacted) {
          // do nothing?  remove?
          setReactions(response.Reactions);
        } else {
          setReactions(response.Reactions);
        }
      }
      else {
        if (response.NotAvailable) {
          getSwal().then(({ default: Swal }) => {
            Swal.fire("Reaction not unlocked.");
            return;
          });
        }
      }
    }).catch(error => {
      console.log("error", error);
      if (error instanceof Error) {
        Swal.fire("Error", `${error.message}`, "error");
      }
      error.json().then(data => {
        Swal.fire("Error", `${data.Message}`, "error");
      })
    });
  }

  useEffect(() => {
    //initialize();
    getReactions();
  }, []); // Fire once

  return (
    <>
      <div className="zr-reactions-bar">
        <div className="zr-reactions" style={{}}>
          {isLoaded && availableReactions.length > 0 ? (<>
            <Tippy
              theme="light-border"
              interactive={true}
              interactiveBorder={30}
              interactiveDebounce={75}
              content={
                <Container title="React" style={{
                  letterSpacing: "normal",
                  fontSize: "18px",
                  width: "200px",
                  maxHeight: "180px",
                  overflowY: "scroll"
                }}>
                  <Row>
                    <Col>
                      <div className="strike">
                        <span>Frequently Used</span>
                      </div>
                      {commonReactions.map((reaction, index) => (
                        <a key={reaction.ReactionId} role="button"
                          className="zr-reaction-icon btn btn-link"
                          onClick={() => react(reaction.ReactionId)}>
                          <span key={reaction.ReactionId}
                            className="reaction-icon"
                            dangerouslySetInnerHTML={{ __html: reaction.ReactionIcon }}></span>
                          {reaction.IsApplied ? (<span className="reaction-applied">✓</span>) : (<><span className="reaction-applied">&nbsp;</span></>)}
                        </a>
                      ))}
                      <div className="strike">
                        <span>Available Reactions</span>
                      </div>
                      {availableReactions.map((reaction, index) => (
                        <a key={reaction.ReactionId} role="button"
                          className="zr-reaction-icon btn btn-link"
                          onClick={() => react(reaction.ReactionId)}>
                          <span key={reaction.ReactionId}
                            className="reaction-icon" dangerouslySetInnerHTML={{ __html: reaction.ReactionIcon }}></span>
                          {reaction.IsApplied ? (<span className="reaction-applied">✓</span>) : (<><span className="reaction-applied">&nbsp;</span></>)}
                        </a>
                      ))}
                    </Col>
                  </Row>
                </Container>
              }>
              <span className="reaction-icon-add-post btn btn-link" style={addShown ? {} : { display: "none" }}><i className="fa-solid fa-plus"></i></span>
            </Tippy>
          </>) : (<>
            {props.l == "1" ? (
              <span className="reaction-icon-add-post btn btn-link"
                onMouseOver={() => { initialize() }}
                style={addShown ? {} : { display: "none" }}><i className="fa-solid fa-plus"></i></span>
            ): (<></>)}
          </>)}

          {reactions.slice(0,4).map((reaction, index) => (
            <a key={reaction.ReactionId} role="button" className="zr-reaction-icon zr-reaction-icon-main" onClick={() => react(reaction.ReactionId)}>
              <span
                title={reaction.UserNames.join(" ")}
                className="reaction-icon" dangerouslySetInnerHTML={{ __html: reaction.ReactionIcon }}>
              </span>
              {reaction.NumReactions > 1 ? (<span className="reaction-count">
                {reaction.NumReactions}
              </span>) : (<></>)}
              {reaction.IsApplied ? (<span className="reaction-applied">✓</span>) : (<><span className="reaction-applied">&nbsp;</span></>)}
            </a>
          ))}

          {reactions.length > 4 ? (<>
            <Tippy
              theme="light-border"
              interactive={true}
              interactiveBorder={30}
              content={
                <Container style={{
                  letterSpacing: "normal",
                  fontSize: "18px",
                  width: "200px",
                  maxHeight: "65px",
                  overflowY: "scroll"
                }}>
                  <Row>
                    <Col>
                      <div className="strike">
                        <span>More Reactions</span>
                      </div>
                      {reactions.slice(4).map((reaction, index) => (
                        <a key={reaction.ReactionId}
                          style={{ paddingLeft: "4px" }}
                          role="button"
                          className="zr-reaction-icon btn btn-link"
                          onClick={() => react(reaction.ReactionId)}>
                          <span
                            title={reaction.UserNames.join(" ")}
                            className="reaction-icon"
                            dangerouslySetInnerHTML={{ __html: reaction.ReactionIcon }}></span>
                          {reaction.NumReactions > 1 ? (<span className="reaction-count">
                            {reaction.NumReactions}
                          </span>) : (<></>)}
                          {reaction.IsApplied ? (<span className="reaction-applied">✓</span>) : (<><span className="reaction-applied">&nbsp;</span></>)}
                        </a>
                      ))}
                    </Col>
                  </Row>
                </Container>
              }>
              <span className="reaction-icon-more btn btn-link"><i className="fa-solid fa-angle-right"></i></span>
            </Tippy>
          </>) : (<></>)}

        </div>
      </div>
    </>
  );
}