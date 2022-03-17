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
      <CollapseBar title={"Group Administration : You have administration privilages for this group"} bg={"bg-warning"} isCollapsed={true}>
        <h2>
          Group Actions
        </h2>
        <a className="btn btn-link btn-sm" href={"/Group/Edit?groupId=" + groupId} ><i className="fa fa-edit"></i> Edit</a>
      </CollapseBar>
    </>
  );
}
