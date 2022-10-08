/*
 * A single banner alert
 */

import React, { useState } from 'react';
import ReactDOM from "react-dom";
import { Alert, Button } from "react-bootstrap";
import { postJson } from '../../utility/postData';

export default function BannerAlert(props) {
  const [show, setShow] = useState(props.show);

  const handleDismiss = () => {
    postJson("/api/v1/user/banneralerts/dismiss/" + props.id + "/", {})
      .then((response) => {
        if (response.success) {
          setShow(false);
        }
      }).catch((error) => {
        console.log(error);
      });
  }

  const handleSnooze = () => {
    postJson("/api/v1/user/banneralerts/snooze/" + props.id + "/", {})
      .then((response) => {
        if (response.success) {
          setShow(false);
        }
      }).catch((error) => {
        console.log(error);
      });
  }

  return (
    <>
      <Alert show={show} variant={props.variant}>
        <Alert.Heading>{props.title}</Alert.Heading>
        <p>
          {props.text}
        </p>
        <hr />
        <div className="d-flex justify-content-end">
          {props.IsGlobal ? (<></>): (
            <>
              <Button size={"sm"} onClick={handleSnooze} variant = "outline-warning">
                Snooze
              </Button>
            </>)}
          <Button size={"sm"} onClick={handleDismiss} variant="outline-success">
            Dismiss
          </Button>
        </div>
      </Alert>
    </>
  );
}