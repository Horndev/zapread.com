/*
 * 
 */

import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Dropdown, Form } from "react-bootstrap";

export default function UserFollowInfo(props) {
  const [followers, setFollowers] = useState([]);
  const [following, setFollowing] = useState([]);

  useEffect(() => {
    async function initialize() {

      await fetch("/api/v1/user/followinfo").then(response => {
        return response.json();
      }).then(data => {
        setFollowers(data.TopFollowers);
        setFollowing(data.TopFollowing);
      });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <div className="ibox float-e-margins">
        <div className="ibox-content profile-content">
          <h3>You are subscribing to</h3>
          <p class="small">
            You are subscribed to content from these users.
          </p>
          <div class="user-friends">
            {following.map((u) => (
              <a key={u.AppId} href={"User/" + encodeURIComponent(u.Name) + "/"}>
                <img className="img-circle"
                  title={u.Name}
                  src={"/Home/UserImage/?size=30&UserId=" + u.AppId + "&v=" + u.ProfileImageVersion} />
              </a>
            ))}
          </div>
        </div>

        <div className="ibox-content profile-content">
          <h3>Subscribing to you</h3>
          <p class="small">
            These users are subscribing to you.
          </p>
          <div class="user-friends">
            {followers.map((u) => (
              <a key={u.AppId} href={"User/" + encodeURIComponent(u.Name) + "/"}>
                <img className="img-circle"
                  title={u.Name}
                  src={"/Home/UserImage/?size=30&UserId=" + u.AppId + "&v=" + u.ProfileImageVersion} />
              </a>
            ))}
          </div>
        </div>
      </div>
    </>);
}