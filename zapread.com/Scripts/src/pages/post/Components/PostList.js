/**
 * A List of posts
 */

import React, { useCallback, useEffect, useState } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";
import { postJson } from "../../../utility/postData"; // [✓]

import PostView from "./PostView"

export default function PostList(props) {
  const [posts, setPosts] = useState([]);//props.posts);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  // Monitor for changes in props
  useEffect(
    () => {
      setPosts(props.posts);
      setIsLoggedIn(props.isLoggedIn);
      //console.log("received posts", props.posts);
    },
    [props.posts, props.isLoggedIn]
  );

  return (
    <>
      {posts.map((post, index) => (
        <PostView key={post.PostId} post={post} isLoggedIn={isLoggedIn}/>
        ))}
    </>
  );
}