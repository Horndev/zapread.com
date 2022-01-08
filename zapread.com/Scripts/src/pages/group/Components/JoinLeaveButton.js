/**
 *
 */

import React, { useCallback, useEffect, useState } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import { postJson } from "../../../utility/postData"; // [✓]

export default function JoinLeaveButton(props) {
  const [isMember, setIsMember] = useState(props.isMember);
  const [groupId, setGroupId] = useState(props.id);

  // Monitor for changes in props
  useEffect(
    () => {
      setGroupId(props.id);
      setIsMember(props.isMember);
    },
    [props.id, props.isMember]
  );

  function joinGroup(id) {
    postJson("/Group/JoinGroup/", { gid: id }).then(response => {
      if (response.success) {
        setIsMember(true);
      }
    });
  }

  function leaveGroup(id) {
    postJson("/Group/LeaveGroup/", { gid: id }).then(response => {
      if (response.success) {
        setIsMember(false);
      }
    });
  }

  if (isMember) {
    return (
      <>
        <button
          onClick={() => {
            leaveGroup(groupId);
          }}
          className="btn btn-primary btn-outline btn-sm"
        >
          <i className="fa fa-user-times" /> Leave
        </button>
      </>
    );
  } else {
    return (
      <>
        <button
          onClick={() => {
            joinGroup(groupId);
          }}
          className="btn btn-primary btn-outline btn-sm"
        >
          <i className="fa fa-user-plus" /> Join
        </button>
      </>
    );
  }
}
