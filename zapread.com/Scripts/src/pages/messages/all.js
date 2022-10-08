/*
 * 
 */

import "../../css/pages/messages/alerts.scss";
import "../../shared/shared";
import "../../realtime/signalr";
import React, { useState, useEffect, useRef } from "react";
import { Row, Col, Button } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../components/PageHeading";
import { deleteJson } from "../../utility/deleteData";
import { putJson } from "../../utility/putData";
import { ISOtoRelative } from "../../utility/datetime/posttime"
import "../../shared/sharedlast";

function Page() {
  const [isLoaded, setIsLoaded] = useState(false);
  const [messagesPage, setMessagesPage] = useState(0);
  const [numNewMessages, setNumNewMessages] = useState(0);
  const [messages, setMessages] = useState([]);

  const loadingRef = useRef();

  async function getMessages() {
    await fetch(`/api/v1/messages/get/${messagesPage}/read/`)
      .then(response => response.json())
      .then(json => {
        setMessages(json.messages);
        setNumNewMessages(json.numMessages);
        loadingRef.current.classList.remove("sk-loading");
      });
  }

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
        await Promise.all([getMessages()]);
      }
    }
    initialize();
  }, []); // Fire once

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
        title="Alerts"
        controller="Home"
        method="Messages"
        function="Alerts"
      />
      <Row>
        <Col lg={2}></Col>
        <Col lg={6}>
          <div className="ibox float-e-margins">
            <div className="ibox-title">
              <div className="ibox-content ibox-heading">
                <h3><i className="fa fa-envelope-o"></i> New messages</h3>
                <small>
                  <i className="fa fa-tim"></i>&nbsp;You have {numNewMessages} new messages.
                </small>
                <span className="pull-right">&nbsp;
                  <Button variant="danger" size="sm" onClick={() => { dismissMessage(-1); }}>
                    Dismiss all&nbsp;<i className="fa fa-times-circle"></i>
                  </Button>
                </span>
              </div>
            </div>
            <div className="ibox-content">
              <div className="feed-activity-list" style={{ minHeight: "30px" }}>
                <div ref={loadingRef} className="ibox-content no-padding sk-loading" style={{ borderStyle: "none" }}>
                  <div className="sk-spinner sk-spinner-three-bounce">
                    <div className="sk-bounce1"></div>
                    <div className="sk-bounce2"></div>
                    <div className="sk-bounce3"></div>
                  </div>
                </div>
                <div className="items-list">
                  <table className="table table-hover">
                    <tbody>
                      {messages.map((alert, index) => (
                        <tr key={alert.Id} className={alert.IsRead ? "" : "alert-row-unread"}>
                          <td className="project-status" onClick={() => { dismissMessage(alert.Id); }}>
                            {alert.IsRead ?
                              <>
                                <span className="badge badge-primary">Read</span>
                              </> :
                              <>
                                <span className="badge badge-info">Unread</span>
                              </>}
                          </td>
                          <td className="project-people">
                            {alert.FromUserAppId != "" ?
                              <a href={"/user/" + encodeURIComponent(alert.UserName)}>
                                <img alt="image" className="rounded-circle" loading="lazy" width="32" height="32"
                                  src={"/Home/UserImage/?size=32&UserId=" + encodeURIComponent(alert.FromUserAppId) + "&v=" + alert.FromUserProfileImageVersion} />
                              </a> : <></>}
                          </td>
                          <td className="project-title">
                            <small className="postTime text-muted">{ISOtoRelative(alert.TimeStamp)}</small>
                            <br />
                            <a href={alert.CommentId > 0 ? ("/Post/Detail/" + alert.PostId + "/#cid_" + alert.CommentId) : alert.PostId > 0 ? "/Post/Detail/" + alert.PostId : "#"}><div dangerouslySetInnerHTML={{ __html: alert.Title }} /></a>
                            <br />
                            <div className="alert-content" style={{ overflow: "hidden" }} dangerouslySetInnerHTML={{ __html: alert.Content }}></div>
                          </td>
                          <td className="project-actions">
                            <a className="btn btn-sm btn-outline-warning" onClick={() => { dismissMessage(alert.Id); }}>
                              <i className="fa fa-minus-circle"></i>{" "}Dismiss
                            </a>
                            {alert.IsPrivateMessage ? <></> :
                              <a className="btn btn-sm btn-outline-danger" onClick={() => { deleteMessage(alert.Id); }}>
                                <i className="fa fa-times-circle"></i>{" "}Delete
                              </a>
                            }
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                {/*<br /><br />OLD<br /><br />*/}
                {/*{alerts.map((alert, index) => (*/}
                {/*  <div*/}
                {/*    key={alert.Id}*/}
                {/*    id={"a_" + alert.Id}*/}
                {/*    className="feed-element">*/}
                {/*    <div>*/}
                {/*      <span className="pull-right">&nbsp;*/}
                {/*        <a className="btn btn-sm btn-danger" onClick={() => { deleteAlert(alert.Id); }}>*/}
                {/*          Delete&nbsp;<i className="fa fa-times-circle"></i>*/}
                {/*        </a>*/}
                {/*      </span>*/}
                {/*      {alert.IsRead ?*/}
                {/*        <>*/}
                {/*          <span className="badge badge-primary">Read</span>*/}
                {/*        </> :*/}
                {/*        <>*/}
                {/*          <span className="badge badge-info">Unread</span>*/}
                {/*        </>}*/}
                {/*      <strong>*/}
                {/*        <div dangerouslySetInnerHTML={{ __html: alert.Title }} />*/}
                {/*      </strong>*/}
                {/*      <div>*/}
                {/*        {alert.PostId > 0 ? (<>*/}
                {/*          <span>Post:</span>*/}
                {/*          <a href={"/Post/Detail/" + alert.PostId}>*/}
                {/*            {alert.PostTitle != "" ? (<>{alert.PostTitle}</>) : (<>Link</>)}*/}
                {/*          </a>*/}
                {/*        </>) : (<></>)}*/}
                {/*      </div>*/}
                {/*      <div dangerouslySetInnerHTML={{ __html: alert.Content }} />*/}
                {/*      <small className="postTime text-muted">{ ISOtoRelative(alert.TimeStamp) }</small>*/}
                {/*    </div>*/}
                {/*  </div>*/}
                {/*))}*/}
              </div>
            </div>
          </div>
        </Col>
        <Col lg={2}></Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));