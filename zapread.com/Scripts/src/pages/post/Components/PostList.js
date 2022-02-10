/**
 * A List of posts
 */

import React, { useCallback, useEffect, useState } from "react";
import PostView from "./PostView"

export default function PostList(props) {
  const [posts, setPosts] = useState([]);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  // Monitor for changes in props
  useEffect(
    () => {
      setPosts(props.posts);
      setIsLoggedIn(props.isLoggedIn);
    },
    [props.posts, props.isLoggedIn]
  );

  return (
    <>
      {posts.map((post, index) => (
        <PostView key={post.PostId} post={post} isLoggedIn={isLoggedIn} isGroupMod={props.isGroupMod}/>
        ))}
    </>
  );
}