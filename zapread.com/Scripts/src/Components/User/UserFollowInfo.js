/*
 * 
 */

import React, { useState, useEffect } from 'react';

export default function UserFollowInfo(props) {
  const [followers, setFollowers] = useState([]);
  const [following, setFollowing] = useState([]);
  const [userName, setUserName] = useState("");

  useEffect(() => {
    async function initialize() {
      var url = "/api/v1/user/followinfo";
      if (props.userAppId) { url = url + "/" + props.userAppId + "/" }
      await fetch(url).then(response => {
        return response.json();
      }).then(data => {
        setFollowers(data.TopFollowers);
        setFollowing(data.TopFollowing);
        setUserName(data.UserName);
      });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <div className="ibox float-e-margins">
        <div className="ibox-content profile-content">
          <h3>{props.isOwnProfile ? (<>You are subscribing to</>) : (<>{userName} is subscribed to</>)}</h3>
          <p className="small">
            {props.isOwnProfile ? (<>You are subscribed to content from these users.</>) : (<>{userName} is subscribed to content from these users.</>)}
          </p>
          <div className="user-friends">
            {following.map((u) => (
              <a key={u.AppId} href={"/User/" + encodeURIComponent(u.Name) + "/"}>
                <img className="img-circle"
                  title={u.Name}
                  src={"/Home/UserImage/?size=30&UserId=" + u.AppId + "&v=" + u.ProfileImageVersion} />
              </a>
            ))}
          </div>
        </div>

        <div className="ibox-content profile-content">
          <h3>{props.isOwnProfile ? (<>Subscribing to you</>) : (<>Subscribing to {userName}</>)}</h3>
          <p className="small">
            {props.isOwnProfile ? (<>These users are subscribing to you.</>) : (<>These users are subscribing to {userName}.</>)}
          </p>
          <div className="user-friends">
            {followers.map((u) => (
              <a key={u.AppId} href={"/User/" + encodeURIComponent(u.Name) + "/"}>
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