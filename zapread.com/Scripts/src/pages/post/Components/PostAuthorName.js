/**
 * Author for post
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import { postJson } from "../../../utility/postData";               // [✓]
import { ready } from '../../../utility/ready';                     // [✓]
import { applyHoverToChildren } from '../../../utility/userhover';  // [✓]

export default function PostAuthorName(props) {
  const [userId, setUserId] = useState(0);
  const [userName, setUserName] = useState("");
  const [isIgnored, setIsIgnored] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);

  const userNameRef = createRef();

  // Monitor for changes in props
  useEffect(
    () => {
      //console.log("PostAuthorName received: ", props)
      setUserId(props.userId);
      setUserName(props.userName);
      setIsIgnored(props.isIgnored);
    },
    [props.isIgnored, props.userId, props.userName]
  );

  useEffect(() => {
    if (!isInitialized) {
      setIsInitialized(true);
    }
  }, []);

  return (
    <>
      <a ref={userNameRef} className="post-username userhint" data-userid={userId}
        href={"/User/" + userName}>
        {isIgnored ? (<>(Ignored)</>) : (<>{ userName }</>)}
      </a>
    </>
  );
}
