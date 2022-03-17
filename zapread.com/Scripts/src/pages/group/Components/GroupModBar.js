/*
 * Admin bar for a group view
 * 
 **/

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import CollapseBar from "./CollapseBar";

export default function GroupAdminBar(props) {
  const [groupId, setGroupId] = useState(props.id);

  // Monitor for changes in props
  useEffect(
    () => {
      setGroupId(props.id);
    },
    [props.id]
  );

  return (
    <>
      <CollapseBar title={"Group Moderation : You have moderation privilages for this group"} bg={"bg-info"} isCollapsed={true}>
        <h2>
          Group Actions (will be implemented here)
        </h2>
        <p>
          Add user to group admin;
          <br />
          Add user to group mod;
          <br />
          Add user to group;
          <br />
          Delete Group;
          <br />
          Ban / Unban users;
        </p>
      </CollapseBar>
    </>
  );
}
