/**
 * A Single Post
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";
import * as bsn from 'bootstrap.native/dist/bootstrap-native-v4';               // [✓]

import { postJson } from "../../../utility/postData";                 // [✓]
import { readMoreButton } from "../../../shared/readmore";            // [✓]
import { deletePost } from "../../../shared/postfunctions";           // [✓]
import { ready } from '../../../utility/ready';                       // [✓]
import { applyHoverToChildren } from '../../../utility/userhover';    // [✓]
import { loadgrouphover } from '../../../utility/grouphover';         // [✓]
import { updatePostTimes } from '../../../utility/datetime/posttime'; // [✓]
import { writeComment } from '../../../comment/writecomment'          // [✓]
import { setPostLanguage } from "../../../shared/postfunctions";      // [✓]
import { stickyPost } from "../../../shared/postfunctions";           // [✓]

import PostAuthorName from "./PostAuthorName";
import PostVoteButtons from "./PostVoteButtons";
import CommentsView from "../../../comment/CommentsView";

export default function PostView(props) {
  const [post, setPost] = useState(props.post);
  const [isVisible, setIsVisible] = useState(true);
  const [isIgnored, setIsIgnored] = useState(true);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isMod, setIsMod] = useState(false);
  const [isAuthor, setIsAuthor] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);
  const [isDetailView, setIsDetailView] = useState(true);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  const postContentRef = createRef();
  const readMoreButtonRef = createRef();
  const toggleVisibleIconRef = createRef();

  // Monitor for changes in props
  useEffect(
    () => {
      setPost(props.post);
      setIsIgnored(props.post.ViewerIgnoredUser);
      setIsLoggedIn(props.isLoggedIn);
    },
    [props.post, props.isLoggedIn]
  );

  useEffect(() => {
    if (!isInitialized) {

      ready(function () {
        // activate dropdown (done manually using bootstrap.native)
        var elements = document.querySelectorAll(".dropdown-toggle");
        Array.prototype.forEach.call(elements, function (el, _i) {
          var dropdownInit = new bsn.Dropdown(el);
        });

        elements = document.querySelectorAll(".pop");
        Array.prototype.forEach.call(elements, function (el, _i) {
          el.classList.remove('pop');
        });

        //applyHoverToChildren(postContentRef.current, ".userhint");
        applyHoverToChildren(document, ".userhint");

        elements = document.querySelectorAll(".grouphint");
        Array.prototype.forEach.call(elements, function (el, _i) {
          loadgrouphover(el);
          el.classList.remove('grouphint');
        });

        // --- update impressions counts
        elements = document.querySelectorAll(".impression");
        Array.prototype.forEach.call(elements, function (el, _i) {
          var url = el.getAttribute('data-url');
          fetch(url).then(function (response) {
            return response.text();
          }).then(function (html) {
            el.innerHTML = html;
            el.classList.remove('impression');
          });
        });

        // --- relative times
        updatePostTimes();

        // configure read more
        if (parseFloat(getComputedStyle(postContentRef.current, null).height.replace("px", "")) >= 800) {
          readMoreButtonRef.current.style.display = "initial";
        }

      });

      setIsInitialized(true);
    }
  }, []);

  function toggleVisible() {
    if (isVisible) {
      toggleVisibleIconRef.current.classList.remove("fa-minus-square");
      toggleVisibleIconRef.current.classList.add("fa-plus-square")
    } else {
      toggleVisibleIconRef.current.classList.remove("fa-plus-square")
      toggleVisibleIconRef.current.classList.add("fa-minus-square");
    }
    setIsVisible(!isVisible);
  }

  return (
    <>
      <div className="social-feed-box" id={" post_" + post.PostId}>
        <button className="pull-left btn btn-sm btn-link" style={{
          display: "flex",
          paddingLeft: "4px"
        }} onClick={toggleVisible} >
          <i className="fa fa-minus-square togglebutton" ref={toggleVisibleIconRef}></i>
        </button>

        <div className="pull-right social-action dropdown">
          <button data-toggle="dropdown" className="dropdown-toggle btn-white"></button>
          <ul className="dropdown-menu dropdown-menu-right m-t-xs" style={{ left: "auto" }}>
            <li>
              <button className="btn btn-link btn-sm">
                <i className="fa fa-eye"></i> <span className="impression" data-url={"/Post/Impressions/" + post.PostId}></span> Impression(s)
              </button>
            </li>
            {post.ViewerIgnoredUser ? (
              <li>
                <button className="btn btn-link btn-sm" onClick={() => { alert("not yet implemented"); }}>
                  <i className="fa fa-eye"></i> Show Post
                </button>
              </li>
            ) : (<></>)}

            {isAdmin | isAuthor ? (
              <>
                <li>
                  <a className="btn btn-link btn-sm" href={"/Post/Edit?postId=" + post.PostId}><i className="fa fa-edit"></i> Edit</a>
                </li>
                <li>
                  <button className="btn btn-link btn-sm" type="submit" onClick={() => {
                    deletePost(post.PostId);
                  }}><i className="fa fa-times"></i> Delete</button>
                </li>
              </>
            ) : (<></>)}

            <li>
              <button className="btn btn-link btn-sm" type="submit">
                <i className="fa fa-flag"></i> Report Spam
              </button>
            </li>

            {isAdmin ? (
              <li>
                <button className="btn btn-link btn-sm" type="submit" onClick={() => {
                  setPostLanguage(post.PostId);
                }}>
                  <i className="fa fa-times"></i> Set Language (Admin)
                </button>
              </li>
            ) : (<></>)}

            {isMod | isAdmin | isAuthor ? (
              <>
                <li>
                  <button className="btn btn-link btn-sm" onClick={() => {
                    //"nsfwPost(@Model.PostId)"
                  }}>
                    <i className="fa fa-exclamation-triangle"></i> Toggle NSFW
                  </button>
                </li>
                <li>
                  <button className="btn btn-link btn-sm" onClick={() => {
                    //nsfwPost(@Model.PostId)"
                  }}>
                    <i className="fa fa-exclamation-triangle"></i> Toggle Explicit
                  </button>
                </li>
              </>
            ) : (
              <>
                <li>
                  <button className="btn btn-link btn-sm" onClick={() => {
                    alert("Not yet implemented.  Feature coming soon.");
                  }}>
                    <i className="fa fa-exclamation-triangle"></i> Report NSFW
                  </button>
                </li>
              </>
            )}

            {isMod ? (
              <li>
                <button className="btn btn-link btn-sm" onClick={() => {
                  stickyPost(post.PostId);
                }}>
                  <i className="fa fa-map-pin"></i> Toggle Group Sitcky
                </button>
              </li>
            ) : (<></>)}

          </ul>
        </div>

        <div className="social-avatar" style={{ paddingBottom: "15px" }}>
          <PostVoteButtons postId={post.PostId} postScore={post.Score} viewerUpvoted={post.ViewerUpvoted} viewerDownvoted={post.ViewerDownvoted} />

          {!isIgnored ? (
            <a href={"/user/" + encodeURIComponent(post.UserName)} className="pull-left" style={{ paddingTop: "8px" }}>
              <img className="img-circle post-image-45" loading="lazy" width="45" height="45" data-userid={post.UserId}
                src={"/Home/UserImage/?size=45&UserId=" + encodeURIComponent(post.UserAppId) + "&v=" + post.UserProfileImageVersion} />
            </a >
          ) : (<></>)}

          <div className="media-body">
            <a className="vote-title" href={"/Post/Detail/" + post.PostId} style={{ marginLeft: "110px" }}>
              {post.PostTitle == "" ? (
                <>Post</>
              ) : (
                post.PostTitle
              )}
            </a>
            <div className="vote-info" style={{ marginLeft: "110px" }}>
              {/*<PostAuthorName userId={post.UserId} userName={post.UserName} isIgnored={post.ViewerIgnoredUser} />*/}

              <a className="post-username userhint" data-userid={post.UserId}
                href={"/User/" + post.UserName}>
                {isIgnored ? (<>(Ignored)</>) : (<>{post.UserName}</>)}
              </a>

              {post.IsSticky ? (
                <span title="Sticky">
                  <i className="fa fa-map-pin" style={{ color: "lightgreen" }}></i>
                </span>
              ) : (<></>)}

              &nbsp;posted in&nbsp;

              <a className="post-groupname grouphint" data-groupid={post.GroupId} href={
                "/Group/Detail/" + post.GroupId
                //"@Url.Action(actionName: "GroupDetail", controllerName: "Group", routeValues: new {id = Model.GroupId})"
              } style={{ fontSize: "small", display: "inline" }}>
                {post.GroupName}
              </a>

              {" "}

              <small className="postTime text-muted" style={{ display: "none" }}>
                {post.TimeStamp}
              </small>
              {post.TimeStampEdited ? (
                <>
                  <span className="text-muted" style={{ display: "inline" }}>
                    &nbsp;edited&nbsp;
                  </span>
                  <small className="postTime text-muted" style={{ display: "none" }}>
                    {post.TimeStampEdited}
                  </small>
                </>
              ) : (<></>)}
              <span className="appear" data-src={"/Post/Impressions/" + post.PostId}></span>
            </div>
          </div >
        </div >

        <div className="social-body" style={{ display: isVisible ? "block" : "none" }}>
          <div className="row">
            <div className="col">
              <div className="post-quotable post-content ql-container ql-snow post-box"
                ref={postContentRef}
                data-postid={post.PostId}
                data-userid={post.UserId}>
                <div className="post-content ql-container ql-snow">
                  <div dangerouslySetInnerHTML={{ __html: post.Content }} />
                </div>
                <p className="read-more-button" ref={readMoreButtonRef}>
                  <a role="button" className="button btn btn-primary"
                    onClick={(e) => {
                      readMoreButton(e.target);
                    }}>Read More</a>
                </p>
              </div>
            </div>
          </div>
        </div>

        {isLoggedIn ? (
          <>
            <div id={"wc_" + post.PostId} onClick={() => {
              writeComment(post.PostId);
            }}>
              <span className="btn btn-link btn-sm">
                <span className="badge badge-light">
                  {post.CommentVms.length}
                </span>
                {" "}<i className="fa fa-comments"></i>{" "}Write a comment
              </span>
            </div>
            <div id={"reply_p" + post.PostId} style={{ display: "none" }}></div>
          </>
        ) : (<></>)}

        <div className="social-comment-box" id={"comments_" + post.PostId}>

          <CommentsView comments={post.CommentVms} isLoggedIn={props.isLoggedIn} postId={post.PostId}/>

        </div>

      </div >
    </>
  );
}