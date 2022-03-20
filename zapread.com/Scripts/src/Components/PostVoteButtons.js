/**
 * Vote Buttons for post
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
const getSwal = () => import('sweetalert2');

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

  const handleVoteUp = () => {
    const event = new CustomEvent('vote', {
      detail: {
        direction: 'up',
        type: 'post',
        id: postId,
        target: upVoteRef.current
      }
    });
    document.dispatchEvent(event);
  }

  const handleVoteDown = () => {
    if (props.viewerIsBanished) {
      getSwal().then(({ default: Swal }) => {
        Swal.fire("Error", "You are banished from this group and can't vote down", "error");
      });
    } else {
      const event = new CustomEvent('vote', {
        detail: {
          direction: 'down',
          type: 'post',
          id: postId,
          target: downVoteRef.current
        }
      });
      document.dispatchEvent(event);
    }
  };

  return (
    <>
      <div className="col-sm-auto vote-actions" style={{ paddingLeft: "0px" }}>
        <div className="vote-actions">
          <a role="button"
            onClick={handleVoteUp}
            className={viewerUpvoted ? "" : "text-muted"}
            id={"uVote_" + postId}>
            <i ref={upVoteRef} className="fa-solid fa-chevron-up fa-lg"> </i>
          </a>
          <div id={"sVote_" + postId}>
            {postScore}
          </div>
          <a role="button"
            onClick={handleVoteDown}
            className={viewerDownvoted ? "" : "text-muted"}
            id={"dVote_" + postId}>
            {props.viewerIsBanished ? (<>
              <i ref={downVoteRef} className="fa-solid fa-minus fa-lg"> </i>
            </>) : (<>
              <i ref={downVoteRef} className="fa-solid fa-chevron-down fa-lg"> </i>
            </>)}
          </a>
        </div>
      </div>
    </>
  );
}
