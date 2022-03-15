/**
 * A List of posts
 */

import React, { useCallback, useEffect, useState } from "react";
import PostView from "./PostView"

export default function PostList(props) {
  const [posts, setPosts] = useState([]);
  const [hasMorePosts, setHasMorePosts] = useState(props.hasMorePosts);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  // Monitor for changes in props
  useEffect(
    () => {
      setPosts(props.posts);
      setIsLoggedIn(props.isLoggedIn);
      setHasMorePosts(props.hasMorePosts);
    },
    [props.posts, props.isLoggedIn, props.hasMorePosts]
  );

  return (
    <>
      {posts.map((post, index) => (
        <PostView key={post.PostId} post={post} isLoggedIn={isLoggedIn} isGroupMod={props.isGroupMod}/>
      ))}

      {hasMorePosts ? (
        <div className="social-feed-box-nb" id="showmore">
          <button id="btnLoadmore" className="btn btn-primary btn-block"
            onClick={props.onMorePosts}>
            <i className="fa-solid fa-arrow-down"></i>&nbsp;Show More&nbsp;<i id="loadmore" className="fa-solid fa-circle-notch fa-spin" style={{ display: "none" }}></i>
          </button>
        </div>
      ) : (<></>)}
    </>
  );
}