/**
 *  List of chats going on with user
 *  
 */

import '../../shared/shared';
import '../../realtime/signalr';
import React, { useCallback, useEffect, useState, useRef } from 'react';
import { Container, Row, Col, Table } from 'react-bootstrap';
import ReactDOM from 'react-dom';
import PageHeading from '../../components/page-heading';
import { ISOtoRelative } from "../../utility/datetime/posttime"
import '../../shared/sharedlast';

function Page() {
  const [isLoaded, setIsLoaded] = useState(false);
  const [chats, setChats] = useState([]);
  const [numChats, setNumChats] = useState(0);
  const [chatsPage, setChatsPage] = useState(0);
  const chatsLoadingRef = useRef();
  const chatsTableRef = useRef();

  async function getChats() {
    await fetch(`/api/v1/chats/list/${chatsPage}/`)
      .then(response => response.json())
      .then(json => {
        console.log(json);
        setChats(json.chats);
        setNumChats(json.numChats);
        chatsLoadingRef.current.classList.remove("sk-loading");
        chatsTableRef.current.style.display = "table";
      });
  }

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
        await getChats();
      }
    }
    initialize();
  }, []); // Fire once

  return (
    <div>
      <PageHeading title="Ongoing Chats" controller="Messages" method="Chats" function="List" />
      <div><Row><Col lg={12}><br /></Col></Row></div>
      <div className="wrapper wrapper-content">
        <div>
          <Row>
            <Col lg={12}>
              <div className="ibox-content">
                <Container fluid="md">
                  <div className="feed-activity-list" style={{ minHeight: "25px" }}>
                    <div ref={chatsLoadingRef} className="ibox-content no-padding sk-loading" style={{ borderStyle: "none" }}>
                      <div className="sk-spinner sk-spinner-three-bounce">
                        <div className="sk-bounce1"></div>
                        <div className="sk-bounce2"></div>
                        <div className="sk-bounce3"></div>
                      </div>
                    </div>
                    <Table ref={chatsTableRef} bordered hover responsive style={{ display: "none" }}>
                      <thead>
                        <tr>
                          <td>From</td>
                          <td>Last Activity</td>
                          <td>Status</td>
                        </tr>
                      </thead>
                      <tbody>
                        {chats.map((chat, index) => (
                          <tr key={chat.Id} onClick={() => {
                            window.open("/Messages/Chat/" + encodeURIComponent(chat.FromName) + "/", '_blank').focus();
                          }} style={{cursor: "pointer"}}>
                            <td>
                              <div style={{ display: "inline-block" }}>
                                {chat.FromOnline ? (<>
                                  <span style={{ display: "inline", color: "green" }} data-toggle='tooltip' data-placement='bottom' title='Online'>
                                    <i className="fa fa-check-circle"></i>
                                  </span>
                                </>) : (<>
                                    <span style={{ display: "inline", color: "lightgray" }} data-toggle='tooltip' data-placement='bottom' title='Offline'>
                                      <i className="fa fa-minus-circle"></i>
                                    </span>
                                </>)}&nbsp;
                                <a className="post-username"
                                  target="_blank"
                                  href={"/Messages/Chat/" + encodeURIComponent(chat.FromName) + "/"}>
                                  <img className="img-circle" src={"/Home/UserImage/?UserID=" + chat.FromAppId + "&size=30&v=" + chat.FromProfileImageVersion} style={{paddingRight: "10px"}} />
                                    {chat.FromName}
                                </a>
                              </div>
                            </td>
                            <td>{ISOtoRelative(chat.TimeStamp)}</td>
                            <td>
                              {chat.IsRead ? (<></>) : (<>Unread&nbsp;</>)}
                              {chat.IsReplied ? (<>Replied</>) : (<></>)}
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </Table>
                  </div>
                </Container>
                <br />
                <br />
                <br />
              </div>
            </Col>
          </Row>
        </div>
      </div>
    </div>
  )
}

ReactDOM.render(
  <Page />
  , document.getElementById("root"));