/*
 * 
 */

import "../../shared/shared";
import "../../realtime/signalr";

import React, { useState, useEffect, useRef } from "react";
import { Row, Col, Button } from "react-bootstrap";
import ReactDOM from "react-dom";

import PageHeading from "../../components/page-heading";
import { deleteJson } from "../../utility/deleteData";
import { putJson } from "../../utility/putData";
import { ISOtoRelative } from "../../utility/datetime/posttime"

import "../../shared/sharedlast";

function Page() {
  const [isLoaded, setIsLoaded] = useState(false);
  const [numNewMessages, setNumNewMessages] = useState(0);
  const [numNewAlerts, setNumNewAlerts] = useState(0);
  const [messagesPage, setMessagesPage] = useState(0);
  const [alertsPage, setAlertsPage] = useState(0);
  const [messages, setMessages] = useState([]);
  const [alerts, setAlerts] = useState([]);

  const messagesLoadingRef = useRef();
  const alertsLoadingRef = useRef();

  async function getAlerts() {
    await fetch(`/api/v1/alerts/get/${alertsPage}/`)
      .then(response => response.json())
      .then(json => {
        setAlerts(json.alerts);
        setNumNewAlerts(json.numAlerts);
        alertsLoadingRef.current.classList.remove("sk-loading");
      });
  }

  async function getMessages() {
    await fetch(`/api/v1/messages/get/${messagesPage}/`)
      .then(response => response.json())
      .then(json => {
        setMessages(json.messages);
        setNumNewMessages(json.numMessages);
        messagesLoadingRef.current.classList.remove("sk-loading");
      });
  }

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
        await Promise.all([getMessages(), getAlerts()]);
      }
    }
    initialize();
  }, []); // Fire once

  async function deleteAlert(id) {
    await deleteJson(`/api/v1/alerts/user/${id}`, {})
      .then(response => {
        if (response.success) {
          getAlerts();
        }
      });
  }

  async function dismissAlert(id) {
    await putJson(`/api/v1/alerts/user/mark-read/${id}`, {})
      .then(response => {
        if (response.success) {
          getAlerts();
        }
      });
  }

  async function deleteMessage(id) {
    await deleteJson(`/api/v1/messages/user/${id}`, {})
      .then(response => {
        if (response.success) {
          getMessages();
        }
      });
  }

  async function dismissMessage(id) {
    await putJson(`/api/v1/messages/user/mark-read/${id}`, {})
      .then(response => {
        if (response.success) {
          getMessages();
        }
      });
  }

  return (
    <>
      <PageHeading
        title="Messages"
        controller="Home"
        method="All"
        function="List"
      />
      <Row>
        <Col lg={6}>
          <div className="ibox float-e-margins">
            <div className="ibox-title">
              <div className="ibox-content ibox-heading">
                <h3><i className="fa fa-envelope-o"></i> New messages</h3>
                <small>
                  <i className="fa fa-tim"></i>&nbsp;You have {numNewMessages} new messages.
                </small>
                <span className="pull-right">&nbsp;
                  <Button variant="danger" size="sm" onClick={() => { deleteMessage(-1); }}>
                    Delete all&nbsp;<i className="fa fa-times-circle"></i>
                  </Button>
                </span>
                <span className="pull-right">
                  <Button variant="warning" size="sm" onClick={() => { dismissMessage(-1); }}>
                    Dismiss all&nbsp;<i className="fa fa-times-circle"></i>
                  </Button>
                </span>
              </div>

            </div>
            <div className="ibox-content">
              <div className="feed-activity-list" style={{ minHeight: "30px" }}>
                <div ref={messagesLoadingRef} className="ibox-content no-padding sk-loading" style={{ borderStyle: "none" }}>
                  <div className="sk-spinner sk-spinner-three-bounce">
                    <div className="sk-bounce1"></div>
                    <div className="sk-bounce2"></div>
                    <div className="sk-bounce3"></div>
                  </div>
                </div>
                {messages.map((message, index) => (
                  <div key={message.Id}
                    id={"m_" + message.Id}
                    style={message.IsDeleted | message.IsRead ? {display: "none"} : {}} // hide if deleted
                    className="feed-element">
                    <div>
                      <span className="pull-right text-white">
                        &nbsp;<a role="button" className="btn btn-sm btn-danger" onClick={() => { deleteMessage(message.Id); }}>
                          Delete <i className="fa fa-times-circle"></i>
                        </a>
                      </span>
                      <span className="pull-right text-white">
                        &nbsp;<a className="btn btn-sm btn-warning" onClick={() => { dismissMessage(message.Id); }}>
                          Dismiss <i className="fa fa-minus-circle"></i>
                        </a>
                      </span>
                      <small className="postTime text-muted">{ISOtoRelative(message.TimeStamp)}</small>
                      <strong>
                        <div dangerouslySetInnerHTML={{ __html: message.Title }} />
                      </strong>
                      <div>
                        {message.PostId > 0 ? (<>
                          <span>Post:</span>
                          <a href={"/Post/Detail/" + message.PostId}>
                            {message.PostTitle ? (<>{message.PostTitle}</>) : (<>Link</>)}
                          </a>
                        </>) : (<></>)}
                        {message.CommentLink ? (<></>) : (<></>)}
                        {message.IsPrivateMessage ? (<>
                          <p>
                            <a href={"/Messages/Chat/" + encodeURIComponent(message.FromName)}>Go to chat</a>
                          </p>
                        </>) : (<></>)}
                      </div>
                      <div dangerouslySetInnerHTML={{ __html: message.Content }} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </Col>

        <Col lg={6}>
          <div className="ibox float-e-margins">
            <div className="ibox-title">
              <div className="ibox-content ibox-heading">
                <h3><i className="fa fa-bell"></i> New alerts</h3>
                <small>
                  <i className="fa fa-tim"></i>&nbsp;You have {numNewAlerts} new alerts.
                </small>
                <span className="pull-right">&nbsp;
                  <Button variant="danger" size="sm" onClick={() => { deleteAlert(-1); }}>
                    Delete all&nbsp;<i className="fa fa-times-circle"></i>
                  </Button>
                </span>
                <span className="pull-right">
                  <Button variant="warning" size="sm" onClick={() => { dismissAlert(-1); }}>
                    Dismiss all&nbsp;<i className="fa fa-times-circle"></i>
                  </Button>
                </span>
              </div>
            </div>
            <div className="ibox-content">
              <div className="feed-activity-list" style={{ minHeight: "30px" }}>
                <div ref={alertsLoadingRef} className="ibox-content no-padding sk-loading" style={{ borderStyle: "none" }}>
                  <div className="sk-spinner sk-spinner-three-bounce">
                    <div className="sk-bounce1"></div>
                    <div className="sk-bounce2"></div>
                    <div className="sk-bounce3"></div>
                  </div>
                </div>
                {alerts.map((alert, index) => (
                  <div
                    key={alert.Id}
                    id={"a_" + alert.Id}
                    className="feed-element">
                    <div>
                      <span className="pull-right">&nbsp;
                        <a className="btn btn-sm btn-danger" onClick={() => { deleteAlert(alert.Id); }}>
                          Delete&nbsp;<i className="fa fa-times-circle"></i>
                        </a>
                      </span>
                      <span className="pull-right text-white">&nbsp;
                        <a className="btn btn-sm btn-warning" onClick={() => { dismissAlert(alert.Id); }}>
                          Dismiss&nbsp;<i className="fa fa-minus-circle"></i>
                        </a>
                      </span>
                      <strong>
                        <div dangerouslySetInnerHTML={{ __html: alert.Title }} />
                      </strong>
                      <div>
                        {alert.PostId > 0 ? (<>
                          <span>Post:</span>
                          <a href={"/Post/Detail/" + alert.PostId}>
                            {alert.PostTitle != "" ? (<>{alert.PostTitle}</>) : (<>Link</>)}
                          </a>
                        </>) : (<></>)}
                      </div>
                      <div dangerouslySetInnerHTML={{ __html: alert.Content }} />
                      <small className="postTime text-muted">{alert.TimeStamp}</small>
                    </div>
                  </div>
                  ))}
              </div>
            </div>
          </div>
        </Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));