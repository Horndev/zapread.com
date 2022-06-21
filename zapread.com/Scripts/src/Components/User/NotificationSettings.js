/*
 * 
 */

import React, { Suspense, useState, useEffect } from 'react';
import { Container, Row, Col } from "react-bootstrap";
//import Swal from 'sweetalert2';
//import { postJson } from "../../utility/postData";
//import { getJson } from "../../utility/getData";
import UserSetting from "./UserSetting";

function SettingRow(props) {
  return (<>
    <Row>
      <Col className="px-1">
        <UserSetting settingName={props.notifyName} isActive={props.isNotifyActive} />
      </Col>
      <Col className="px-1">
        <UserSetting settingName={props.alertName} isActive={props.isAlertActive} />
      </Col>
      <Col>{ props.title }</Col>
    </Row>
  </>);
}

export default function NotificationSettings(props) {
  const [settings, setSettings] = useState(null);

  useEffect(() => {
    async function initialize() {
      await fetch("/api/v1/user/settings/notification").then(response => {
        return response.json();
      }).then(data => {
        setSettings(data.Settings);
      });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <div className="ibox-content profile-content">
        <h4>Notifications</h4>
        <Container>
          
          <Row>
            <Col className="px-1">Email</Col>
            <Col className="px-1">Alert</Col>
            <Col>&nbsp;</Col>
          </Row>

          {settings ? (
            <>
              <SettingRow title={"Your posts receive a comment"} notifyName={"notifyPost"} alertName={"alertPost"}
                isNotifyActive={settings.NotifyOnOwnPostCommented} isAlertActive={settings.AlertOnOwnPostCommented} />

              <SettingRow title={"New post from a subscribed user"} notifyName={"notifyNewPostUser"} alertName={"alertNewPostUser"}
                isNotifyActive={settings.NotifyOnNewPostSubscribedUser} isAlertActive={settings.AlertOnNewPostSubscribedUser} />

              <SettingRow title={"New Private Message"} notifyName={"notifyPrivateMessage"} alertName={"alertPrivateMessage"}
                isNotifyActive={settings.NotifyOnPrivateMessage} isAlertActive={settings.AlertOnPrivateMessage} />

              <SettingRow title={"Received tip"} notifyName={"notifyReceivedTip"} alertName={"alertReceivedTip"}
                isNotifyActive={settings.NotifyOnReceivedTip} isAlertActive={settings.AlertOnReceivedTip} />

              <SettingRow title={"Mentioned by others"} notifyName={"notifyMentioned"} alertName={"alertMentioned"}
                isNotifyActive={settings.NotifyOnMentioned} isAlertActive={settings.AlertOnMentioned} />

            </>
          ) : (<></>)}
        </Container>
      </div>
    </>
  );
}