/**
 * Vote Buttons for post
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import { postJson } from "../../../utility/postData";
import { vote } from "../../../utility/ui/vote";

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
          <a role="button" onClick={() => {
              const event = new CustomEvent('vote', {
                detail: {
                  direction: 'up',
                  type: 'post',
                  id: postId,
                  target: upVoteRef.current
                }
              });
              document.dispatchEvent(event);
              //vote(postId, 1, 1, 100, upVoteRef.current);
            }} className={viewerUpvoted ? "" : "text-muted"} id={"uVote_" + postId}>
            <i ref={upVoteRef} className="fa-solid fa-chevron-up fa-lg"> </i>
          </a>
          <div id={"sVote_" + postId}>
            {postScore}
          </div>
          <a role="button" onClick={() => {
              const event = new CustomEvent('vote', {
                detail: {
                  direction: 'down',
                  type: 'post',
                  id: postId,
                  target: downVoteRef.current
                }
              });
              document.dispatchEvent(event);
              //vote(postId, 0, 1, 100, downVoteRef.current);
            }} className={viewerDownvoted ? "" : "text-muted"} id={"dVote_" + postId}>
            <i ref={downVoteRef} className="fa-solid fa-chevron-down fa-lg"> </i>
          </a>
        </div>
      </div>
    </>
  );
}
