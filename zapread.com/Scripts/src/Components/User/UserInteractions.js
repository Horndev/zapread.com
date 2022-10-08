/*
 * 
 */

import React, { useState, useEffect } from 'react';
import { Row, Col, Button } from "react-bootstrap";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faCircleNotch } from '@fortawesome/free-solid-svg-icons'
import { postJson } from '../../utility/postData';
import { getJson } from '../../utility/getData';

function FollowButton(props) {
  const toggleFollow = () => {
    var url = "/api/v1/user/";
    if (props.isFollowing) { url = url + "unfollow"; } else { url = url + "follow"; }
    postJson(url, {
      UserAppId: props.userAppId
    }).then((response) => {
      if (response.success) {
        props.updateFollowing(!props.isFollowing);
      }
    });
  };

  return (
  <>
      <Button
        block
        variant={props.isFollowing ? "warning" : "primary"}
        size="sm"
        onClick={toggleFollow}
        disabled={!props.isLoggedIn}>
        {props.isFollowing ? (<>Unfollow</>) : (<>Follow</>)}
      </Button>
  </>);
}

function BlockButton(props) {
  const toggleBlock = () => {
    var url = "/api/v1/user/";
    if (props.isBlocking) { url = url + "unblock"; } else { url = url + "block"; }
    postJson(url, {
      UserAppId: props.userAppId
    }).then((response) => {
      if (response.success) {
        props.updateBlocking(!props.isBlocking);
      }
    });
  };

  return (
    <>
      <Button
        block
        variant={props.isBlocking ? "primary" : "warning"}
        size="sm"
        onClick={toggleBlock}
        disabled={!props.isLoggedIn}>
        {props.isBlocking ? (<>Unblock</>) : (<>Block</>)}
      </Button>
    </>);
}

function IgnoreButton(props) {
  const toggleIgnore = () => {
    var url = "/api/v1/user/";
    if (props.isIgnoring) { url = url + "unignore"; } else { url = url + "ignore"; }
    postJson(url, {
      UserAppId: props.userAppId
    }).then((response) => {
      if (response.success) {
        props.updateIgnoring(!props.isIgnoring);
      }
    });
  };

  return (
    <>
      <Button
        block
        variant={props.isIgnoring ? "primary" : "warning"}
        size="sm"
        onClick={toggleIgnore}
        disabled={!props.isLoggedIn}>
        {props.isIgnoring ? (<>Unignore</>) : (<>Ignore</>)}
      </Button>
    </>);
}

export default function UserInteractions(props) {
  const [isFollowing, setIsFollowing] = useState(false);
  const [isBlocking, setIsBlocking] = useState(false);
  const [isIgnoring, setIsIgnoring] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function initialize() {
      var url = "/api/v1/user/interaction";
      if (props.userAppId) { url = url + "/" + props.userAppId + "/"; }

      getJson(url)
        .then((response) => {
          if (response.success) {
            setIsFollowing(response.IsFollowing);
            setIsBlocking(response.IsBlocking);
            setIsIgnoring(response.IsIgnoring);
            setIsLoading(false);
          }
        }).catch((error) => {
          console.log(error);
        });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <div className="ibox float-e-margins">
        <div className="ibox-content profile-content">
          {props.isLoggedIn ? (
            <>
              <div className="user-button">
                <Row>
                  <Col lg={6}>
                    {isLoading ? (<><Button block variant="outline-primary"><FontAwesomeIcon icon={faCircleNotch} spin /></Button></>) : (
                    <>
                      <FollowButton
                        updateFollowing={(value) => setIsFollowing(value)}
                        isFollowing={isFollowing}
                        userAppId={props.userAppId}
                        isLoggedIn={props.isLoggedIn} />
                    </>)}
                  </Col>
                  <Col lg={6}>
                    {isLoading ? (<><Button block variant="outline-primary"><FontAwesomeIcon icon={faCircleNotch} spin /></Button></>) : (
                    <>
                      <BlockButton
                        updateBlocking={(value) => setIsBlocking(value)}
                        isBlocking={isBlocking}
                        userAppId={props.userAppId}
                        isLoggedIn={props.isLoggedIn} />
                    </>)}
                  </Col>
                </Row>
              </div>
              <div className="user-button">
                <Row>
                  <Col lg={6}>
                    {isLoading ? (<><Button block variant="outline-primary"><FontAwesomeIcon icon={faCircleNotch} spin /></Button></>) : (
                    <>
                      <Button
                        block
                        size="sm"
                        variant="info"
                        onClick={() => {
                          window.open("/Messages/Chat/" + encodeURIComponent(props.userName) + "/", '_blank').focus();
                        }}
                        disabled={!props.isLoggedIn}>
                        <i className="fa fa-comment"></i> <span>Chat</span>
                      </Button>
                    </>)}
                  </Col>
                  <Col lg={6}>
                    {isLoading ? (<><Button block variant="outline-primary"><FontAwesomeIcon icon={faCircleNotch} spin /></Button></>) : (
                    <>
                      <IgnoreButton
                        updateIgnoring={(value) => setIsIgnoring(value)}
                        isIgnoring={isIgnoring}
                        userAppId={props.userAppId}
                        isLoggedIn={props.isLoggedIn} />
                    </>)}
                  </Col>
                </Row>
              </div>
            </>) : (<></>)}
        </div>
      </div>
    </>);
}