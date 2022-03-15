﻿/**
 * Render a comment
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button, Dropdown } from "react-bootstrap";

import { toggleComment } from "../shared/postui";
import { replyComment } from "../comment/replycomment";
import { ready } from '../utility/ready';
import { editComment } from "../comment/editcomment";
import { deleteComment } from "../shared/postfunctions";
import { makeQuotable } from "../utility/quotable/quotable";
import { applyHoverToChildren } from '../utility/userhover';
import { ISOtoRelative } from "../utility/datetime/posttime"
import { postJson } from "../utility/postData";
import '../css/posts.css'

function Comment(props) {
  const [isInitialized, setIsInitialized] = useState(false);
  const [childComments, setChildComments] = useState([]);
  const [isIgnoredUser, setIsIgnoredUser] = useState(false);
  const [startVisible, setStartVisible] = useState(props.startVisible);
  const [nestLevel, setNestLevel] = useState(props.nestLevel);
  const [comment, setComment] = useState(props.comment);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  const toggleButtonRef = createRef();
  const dropdownButtonRef = createRef();
  const upVoteRef = createRef();
  const downVoteRef = createRef();
  const commentTextRef = createRef();
  const commentRootRef = createRef();

  useEffect(
    () => {
      setComment(props.comment);
      setStartVisible(props.startVisible);
      setIsLoggedIn(props.isLoggedIn);
      setNestLevel(props.nestLevel);

      var thisChildComments = props.comments.filter(cmt => cmt.ParentCommentId == props.comment.CommentId)
        .sort((c1, c2) => { c1.Score < c2.Score })
        .sort((c1, c2) => { c1.TimeStamp < c2.TimeStamp });

      //console.log("thisChildComments", thisChildComments, "NumReplies", props.comment.NumReplies);

      setChildComments(thisChildComments);
      if (!isInitialized) {

        var commentElement = commentTextRef.current; // save a ref to use in ready function
        var commentRootElement = commentRootRef.current;

        ready(function () {
          //updatePostTimes();  // --- relative times

          makeQuotable(commentElement, false);
          commentElement.classList.remove("comment-quotable");

          applyHoverToChildren(commentRootElement,".userhint")
        });
        setIsInitialized(true);
      }
    },
    [props.comment, props.startVisible, props.isLoggedIn, props.nestLevel]
  );

  return (
    <>
      <div className="media-body" id={"comment_" + props.comment.CommentId}
        ref={commentRootRef}
        style={{ minHeight: "24px" }}>
        <button ref={toggleButtonRef}
          className={"btn btn-sm btn-link " + (startVisible ? "pull-left" : "") + " comment-toggle"}
          style={{ display: "flex", "paddingLeft": "4px" }} onClick={(e) => { toggleComment(toggleButtonRef.current, 0); }
          }>
          <i className={"togglebutton fa " + (!startVisible ? "fa-plus-square" : "fa-minus-square")
          }
            style={{ paddingTop: "2px" }}></i>
          <span id="cel" className="btn btn-link btn-sm"
            style={{ fontSize: "10pt", borderTopWidth: "0px", paddingTop: "0px", verticalAlign: "top", display: startVisible ? "none" : "initial" }}>
            {" "}[{props.comment.Score}]{" "}Show comment...{" "}</span>
        </button>
        <div className="comment-body" style={{ width: "100%", display: startVisible ? "initial" : "none" }}>
          {!props.comment.IsDeleted && isLoggedIn ? (
            <>
              <a id={"cid_" + props.comment.CommentId}></a>
              <Dropdown
                className="pull-right social-action"
                id={"menu_comment_" + props.comment.CommentId}>
                <Dropdown.Toggle bsPrefix="zr-btn" className="dropdown-toggle btn-white"></Dropdown.Toggle>
                <Dropdown.Menu as="ul" align="right" className="zr-dropdown-menu dropdown-menu-right ift-xs">
                  { //is ignored user
                    props.comment.isIgnoredUser ? (<>
                      <Dropdown.Item as="li" >
                        <button className="btn btn-link btn-sm" onClick={() => {
                          alert('Not yet implemented.')
                        }}>
                          <i className="fa fa-eye"></i>{" "}Show Comment
                        </button>
                      </Dropdown.Item>
                    </>) : (<></>)}
                  {!props.comment.IsDeleted ? (<>
                    <Dropdown.Item as="li" >
                      <button className="btn btn-link btn-sm" onClick={() => {
                        replyComment(props.comment.CommentId, props.comment.PostId);
                      }}>
                        <i className="fa fa-reply"></i>{" "}Reply
                      </button>
                    </Dropdown.Item>
                  </>) : (<></>)}
                  {
                    /*@if (User.Identity.Name == Model.UserName) {}*/
                    isLoggedIn && window.UserName == props.comment.UserName ? (<>
                      <Dropdown.Item as="li" >
                        <button className="btn btn-link btn-sm" onClick={() => {
                          editComment(props.comment.CommentId);
                        }}><i className="fa fa-edit"></i>{" "}Edit</button>
                      </Dropdown.Item>
                      <Dropdown.Item as="li" >
                        <button className="btn btn-link btn-sm" onClick={() => {
                          deleteComment(props.comment.CommentId);
                        }}><i className="fa fa-times"></i>{" "}Delete</button>
                      </Dropdown.Item>
                    </>) : (<></>)}
                  {!props.comment.IsDeleted ? (<>
                    <Dropdown.Item as="li" >
                      <button className="btn btn-link btn-sm" onClick={() => {
                        alert("Not yet implemented.");
                      }}>
                        <i className="fa fa-flag"></i>{" "}Report Spam
                      </button>
                    </Dropdown.Item>
                  </>) : (<></>)}
                  <Dropdown.Item as="li" >
                    <button disabled className="btn btn-link btn-sm">
                      CommentId: { props.comment.CommentId }
                    </button>
                  </Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            </>) : (<></>)}
          <div className={props.comment.IsReply ? "social-footer-reply" : "social-footer"}>
            <div className={props.comment.IsReply ? "" : "social-comment"} style={props.comment.IsDeleted ? { minHeight: "1px" } : {}}>
              <div style={{ marginLeft: "22px", marginBottom: "5px"}}>
                {props.comment.IsDeleted ? (<>
                  {/* Don't show score for deleted comments */ }
                </>) : (
                  <div className="vote-actions" style={isIgnoredUser ? { display: none } : { position: "relative" }}>
                    <a role="button" onClick={() => {
                        const event = new CustomEvent('vote', {
                          detail: {
                            direction: 'up',
                            type: 'comment',
                            id: props.comment.CommentId,
                            target: upVoteRef.current
                          }
                        });
                        document.dispatchEvent(event);
                      }}
                      className={props.comment.ViewerUpvoted ? "" : "text-muted"} id={"uVotec_" + props.comment.CommentId}>
                        <i ref={upVoteRef} className="fa-solid fa-chevron-up fa-lg"> </i>
                    </a>
                    <div id={"sVotec_" + props.comment.CommentId}>
                      {props.comment.Score}
                    </div>
                    <a role="button" style={{ position: "relative", zIndex: 1 }} onClick={() => {
                          const event = new CustomEvent('vote', {
                            detail: {
                              direction: 'down',
                              type: 'comment',
                              id: props.comment.CommentId,
                              target: downVoteRef.current
                            }
                          });
                          document.dispatchEvent(event);
                      }}
                      className={props.comment.ViewerDownvoted ? "" : "text-muted"} id={"dVotec_" + props.comment.CommentId}>
                        <i ref={downVoteRef} className="fa-solid fa-chevron-down fa-lg"> </i>
                    </a>
                  </div>
                )}
                {props.comment.IsDeleted || isIgnoredUser ? (<></>) : (<>
                  <a href={"/User/" + encodeURIComponent(props.comment.UserName) + "/"}>
                    <img className={
                      "img-circle user-image-30"
                    } loading="lazy" width="30" height="30" src={
                      "/Home/UserImage/?size=30&UserId=" + encodeURIComponent(props.comment.UserAppId) + "&v=" + props.comment.ProfileImageVersion
                    } style={{ marginBottom: "16px", marginTop: "8px" }} />
                  </a>
                </>)}

                {props.comment.IsDeleted ? (<>&nbsp;deleted&nbsp;</>) : (<>
                  <a className="post-username userhint" data-userid={props.comment.UserId} data-userappid={props.comment.UserAppId} href={
                    "/User/" + encodeURIComponent(props.comment.UserName) + "/"
                  }>
                    {props.comment.isIgnoredUser ? (<>&nbsp;(Ignored)&nbsp;</>) : (<>&nbsp;{props.comment.UserName}&nbsp;</>)}
                  </a>
                </>)}
                <small style={{ display: "inline-block" }}>
                  {" "}-{" "}
                  {props.comment.IsReply ? (
                    <>
                      &nbsp;replied to&nbsp;
                      <a className="userhint" data-userid={props.comment.ParentUserId} data-userappid={props.comment.ParentUserAppId} href={
                        "/User/" + encodeURIComponent(props.comment.ParentUserName) + "/"
                        //"@Url.Action(actionName: " Index", controllerName: "User", routeValues: new {username = Model.ParentUserName.Trim()})"
                      }>
                        @{props.comment.ParentUserName}&nbsp;
                      </a>
                    </>) : (
                    <>
                      &nbsp;commented&nbsp;
                    </>)}
                </small>
                <small className="text-muted" title={props.comment.TimeStamp}>{ISOtoRelative(props.comment.TimeStamp)}</small>
                {props.comment.TimeStampEdited != null ? (
                  <>
                    <span className="text-muted" style={{ display: "inline" }}>&nbsp;edited&nbsp;</span>
                    <small className="text-muted" style={{ display: "inline" }} title={props.comment.TimeStampEdited}>{ISOtoRelative(props.comment.TimeStampEdited)}</small>
                  </>) : (<></>)}
              </div>
              <div className="media-body" style={{ display: "inline" }}>
                <div style={{paddingLeft: "4px"}}>
                  <div className="ql-comment post-comment comment-quotable"
                    ref={commentTextRef}
                    id={"commentText_" + props.comment.CommentId}
                    style={{ position: "relative" }}
                    data-postid={props.comment.PostId}
                    data-commentid={props.comment.CommentId}>
                    {isIgnoredUser ? (<><span><br /></span></>) : (
                      <>
                        {props.comment.IsDeleted ? (<></>) : (
                          <div style={{ padding: "0px 18px 0px 18px"}}
                            dangerouslySetInnerHTML={{ __html: props.comment.Text }} />
                        )}
                      </>)}
                  </div>
                </div>
              </div>

              {props.isLoggedIn && !props.comment.IsDeleted ? (
                <div style={{ position: "relative", left: "12px", top: "-20px", height: "6px" }}>
                  <a role="button" onClick={
                    () => {
                      replyComment(props.comment.CommentId, props.comment.PostId);
                    }
                  } className="btn-link btn btn-sm">
                    <i className="fa fa-reply"></i>
                  </a>
                </div>
              ) : (
                <>
                  {/*This was added in to prevent the white bar at the bottom of comments*/}
                  <div style={{ position: "relative", left: "12px", top: "-20px", height: "6px" }}>
                    &nbsp;
                  </div>
                </>)}

              {/*This is the element where the comment box will render - not sure this is still needed*/}
              <div id={"reply_c" + props.comment.CommentId} style={{ display: "none" }}></div>

              <div id={"rcomments_" + props.comment.CommentId}>
                {childComments.map((cmt, index) => (
                  <Comment
                    key={cmt.CommentId.toString()}
                    comment={cmt}
                    root={[...props.root, cmt.CommentId]} // Append this comment for the root path array
                    comments={props.comments}
                    nestLevel={props.nestLevel + 1}
                    startVisible={true /*props.nestLevel < 4*/}
                    isLoggedIn={props.isLoggedIn}
                    handleLoadMoreComments={props.handleLoadMoreComments}
                    numReplies={cmt.NumReplies}
                  />
                ))}

                {props.numReplies > childComments.length ? (
                  <>
                    <div style={{ background: "#f9f9f9", borderRadius: "0px 0px 10px 10px", padding: "0px 3px 3px 3px" }}>
                      <Button size="sm" variant="link" onClick={() => {
                        props.handleLoadMoreComments(props.comment.CommentId, props.root, childComments.map(c => c.CommentId).join(';'))
                      }}>
                        Continue thread {/*<small>({props.numReplies - childComments.length} more)</small>*/} <i className="fa-solid fa-arrow-turn-down"></i>
                      </Button>
                    </div>
                  </>) : (<></>)}
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

export default function CommentsView(props) {
  const [comments, setComments] = useState(props.comments);
  const [postId, setPostId] = useState(0);
  const [commentsToRender, setCommentsToRender] = useState([]);
  const [isDetailView, setIsDetailView] = useState(false);
  const [hasMoreComments, setHasMoreComments] = useState(false);

  // Monitor for changes in props
  useEffect(
    () => {
      setComments(props.comments);
      setPostId(props.postId);

      var numToShow = 3;
      if (isDetailView) { numToShow = 50; }

      var comments = props.comments.filter(cmt => !cmt.IsReply)
        .sort((c1, c2) => { c1.Score < c2.Score })
        .sort((c1, c2) => { c1.TimeStamp < c2.TimeStamp })
        .map(c => ({...c, updates: 0}));

      if (props.numRootComments > numToShow) {
        setHasMoreComments(true);
      }

      setCommentsToRender(comments);
    },
    [props.comments, props.postId, props.numRootComments]
  );

  function handleLoadMoreComments(parentCommentId, rootPath, rootshown) {
    //console.log("handleLoadMoreComments", parentCommentId)
    // rootshow is the list of currently shown comments on the current level/thread
    // var rootshown = commentsToRender.map(c => c.CommentId).join(';');
    //console.log("parentCommentId", parentCommentId, "rootshown", rootshown);
    postJson("/api/v1/post/comments/loadmore", {
      PostId: postId,
      Rootshown: rootshown,
      ParentCommentId: parentCommentId
    }).then((response) => {
      if (response.success) {
        var newCommentsToAdd = response.Comments.map(c => ({ ...c, updates: 0 }));
        if (parentCommentId < 0) {
          // Comments on post root
          setCommentsToRender([...commentsToRender, ...newCommentsToAdd.filter(cmt => !cmt.IsReply)]);
          setComments([...comments, ...newCommentsToAdd])
          setHasMoreComments(response.HasMoreComments);
        } else {
          setComments([...comments, ...newCommentsToAdd]);
          // We have to update the root updates property to get React to render
          var newCommentsToRender = commentsToRender.map(c => {
            if (rootPath.includes(c.CommentId)) {
              c.updates++;
            }
            return c;
          });
          setCommentsToRender(newCommentsToRender);
        }
      } else {

      }
    });
  }

  return (
    <>
      {comments.length > 0 ? (
        <>
          {commentsToRender.map((cmt, index) => (
            <Comment
              key={cmt.CommentId.toString() + ":" + cmt.updates.toString()}
              comment={cmt}
              comments={comments}
              numReplies={cmt.NumReplies}
              nestLevel={1}
              startVisible={true}
              handleLoadMoreComments={handleLoadMoreComments}
              root={[cmt.CommentId]}
              isLoggedIn={props.isLoggedIn} />
          ))}

          {hasMoreComments ? (<>
            <div style={{ background: "#f9f9f9", borderRadius: "0px 0px 10px 10px", padding: "0px 3px 3px 3px"}}>
              <Button size="sm" variant="link" onClick={() => {
                handleLoadMoreComments(-1, -1, commentsToRender.map(c => c.CommentId).join(';'))
              }}>
                <i className="fa-solid fa-plus"></i> Load more comments
              </Button>
            </div>
          </>) : (<></>)}
        </>
      ) : (<></>)}

      {/* new comments appear below this line - deprecated*/}
      <div className="insertComments" id={"mc_" + postId}></div>
      {
        // <div onClick="loadMoreComments(this);" data-postid="@Model.PostId" data-shown="@String.Join(";",rootshown)" data-commentid="0" data-nest="1"><span class="btn btn-link btn-sm"><i class="fa fa-plus"></i> Load more comments</span></div>
      }
    </>
  );
}