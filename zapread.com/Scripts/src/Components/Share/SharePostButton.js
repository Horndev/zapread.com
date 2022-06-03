/*
 * Share button
 */

import React, { useCallback, useEffect, useState, forwardRef } from "react";
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import {
  EmailShareButton,
  FacebookShareButton,
  RedditShareButton,
  TelegramShareButton,
  TwitterShareButton,
} from "react-share";
import Tippy from '@tippyjs/react';

export default function SharePostButton(props) {
  const [tip, setTip] = useState(null);

  const shareUrl = 'https://www.zapread.com';

  return (
    <>
      <Tippy
        theme="light-border"
        interactive={true}
        interactiveBorder={30}
        interactiveDebounce={75}
        content={<>
          <Container>
            <Row>
              <Col>
                <TwitterShareButton
                  className="btn btn-link"
                  resetButtonStyle={false}
                  url={props.url}
                  title={props.title.trim()}
                  via={"Zapread"}><i className="fa-brands fa-twitter"></i></TwitterShareButton>
                <RedditShareButton
                  className="btn btn-link"
                  resetButtonStyle={false}
                  url={props.url}
                  title={props.title.trim()}><i className="fa-brands fa-reddit-alien"></i></RedditShareButton>
                <FacebookShareButton
                  className="btn btn-link"
                  resetButtonStyle={false}
                  quote={props.title.trim()}
                  url={props.url}><i className="fa-brands fa-facebook-f"></i></FacebookShareButton>
                <TelegramShareButton
                  className="btn btn-link"
                  resetButtonStyle={false}
                  url={props.url}
                  title={props.title.trim()}><i className="fa-brands fa-telegram"></i></TelegramShareButton>
                <EmailShareButton
                  className="btn btn-link"
                  resetButtonStyle={false}
                  url={props.url}
                  subject={"Sharing " + props.title.trim()}
                  body={props.title.trim()}
                ><i className="fa-solid fa-envelope"></i></EmailShareButton>
              </Col>
            </Row>
          </Container>
        </>}
        >
            <span className="btn btn-link btn-sm"><i className="fa-solid fa-share-nodes"></i> Share</span>
      </Tippy>
    </>
  );
}