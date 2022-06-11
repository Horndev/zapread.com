/*
 * Banner alerts to show to user - there could be more than one
 */

import React, { useState, useEffect } from 'react';
import ReactDOM from "react-dom";
import BannerAlert from "./BannerAlert";
import { getJson } from '../../utility/getData';

export default function BannerAlerts(props) {
  const [alerts, setAlerts] = useState([]);

  useEffect(() => {
    getJson("/api/v1/user/banneralerts/")
    .then((response) => {
      if (response.success) {
        setAlerts(response.Alerts);
      }
    }).catch((error) => {
      console.log(error);
    });
  }, []);

  return (
    <>
      {alerts.map((alert, index) => (
        <BannerAlert
          key={alert.Id}
          id={alert.Id}
          show={true}
          title={alert.Title}
          IsGlobal={alert.IsGlobalSend}
          variant={alert.Priority}
          text={alert.Text} />   
      ))}
    </>
  );
}