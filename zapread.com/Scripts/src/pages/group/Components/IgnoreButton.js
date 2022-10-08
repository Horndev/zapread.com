/**
 * Ignore/un-ignore group button
 */

import React, { useCallback, useEffect, useState } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import { postJson } from "../../../utility/postData"; // [✓]

export default function IgnoreButton(props) {
  const [isIgnoring, setIsIgnoring] = useState(props.isIgnoring);
  const [groupId, setGroupId] = useState(props.id);

  // Monitor for changes in props
  useEffect(
    () => {
      setGroupId(props.id);
      setIsIgnoring(props.isIgnoring);
    },
    [props.id, props.isIgnoring]
  );

  function toggleIgnoreGroup(id) {
    postJson("/Group/ToggleIgnore/", { groupId: id }).then(response => {
      if (response.success) {
        setIsIgnoring(!isIgnoring);
        console.log("isIgnoring:", isIgnoring);
      }
    });
  }

  return (
    <>
      <button
        onClick={() => {
          toggleIgnoreGroup(groupId);
        }}
        className="btn btn-sm btn-link btn-warning btn-outline"
      >
        {isIgnoring ? (<><i className="fa-solid fa-circle" /> Un-Ignore</>) : (<><i className = "fa fa-ban" /> Ignore</>) }
      </button>
    </>
  );
}
