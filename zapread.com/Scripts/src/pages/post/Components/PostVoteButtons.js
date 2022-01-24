/**
 * Vote Buttons for post
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import { postJson } from "../../../utility/postData"; // [✓]
import { vote } from "../../../utility/ui/vote";      // [✓]

export default function PostVoteButtons(props) {
  const [postId, setPostId] = useState(0);
  const [postScore, setPostScore] = useState(0);
  const [viewerUpvoted, setViewerUpvoted] = useState(false);
  const [viewerDownvoted, setViewerDownvoted] = useState(false);

  const upVoteRef = createRef();
  const downVoteRef = createRef();

  // Monitor for changes in props
  useEffect(
    () => {
      setViewerUpvoted(props.viewerUpvoted);
      setViewerDownvoted(props.viewerDownvoted);
      setPostId(props.postId);
      setPostScore(props.postScore);
    },
    [props.viewerUpvoted, props.viewerDownvoted, props.postScore, props.postId]
  );

  return (
    <>
      <div className="col-sm-auto vote-actions" style={{ paddingLeft: "0px" }}>
        <div className="vote-actions">
          <a ref={upVoteRef} role="button"
            onClick={ () => {
              vote(postId, 1, 1, 100, upVoteRef.current);
            }} className={viewerUpvoted ? "" : "text-muted"} id={"uVote_" + postId}>
            <i className="fa fa-chevron-up"> </i>
          </a>
          <div id={"sVote_" + postId}>
            {postScore}
          </div>
          <a ref={downVoteRef} role="button"
            onClick={ () => {
              vote(postId, 0, 1, 100, downVoteRef.current);
            }} className={viewerDownvoted ? "" : "text-muted"} id={"dVote_" + postId}>
            <i className="fa fa-chevron-down"> </i>
          </a>
        </div>
      </div>
    </>
  );
}
