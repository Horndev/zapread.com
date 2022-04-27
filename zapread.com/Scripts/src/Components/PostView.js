/**
 * A Single Post
 */

import React, { Suspense, useCallback, useEffect, useState, createRef } from "react";
import { Dropdown } from "react-bootstrap";
import { readMoreButton } from "../shared/readmore";
import { deletePost } from "../shared/postfunctions";
import { applyHoverToChildren } from '../utility/userhover';
import { loadgrouphover } from '../utility/grouphover';
import { writeComment } from '../comment/writecomment'
import { setPostLanguage, nsfwPost, stickyPost } from "../shared/postfunctions";
import { makeQuotable } from "../utility/quotable/quotable";
import { ISOtoRelative } from "../utility/datetime/posttime"
import PostVoteButtons from "./PostVoteButtons";
const CommentsView = React.lazy(() => import("./CommentsView"));

export default function PostView(props) {
  const [post, setPost] = useState(props.post);
  const [isVisible, setIsVisible] = useState(true);
  const [isIgnored, setIsIgnored] = useState(true);
  const [isSiteAdmin, setIsSiteAdmin] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isMod, setIsMod] = useState(false);
  const [isAuthor, setIsAuthor] = useState(false);
  const [isDetailView, setIsDetailView] = useState(true);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [impressions, setImpressions] = useState(0);
  const toggleVisibleIconRef = createRef();

  var impressionObserver = new IntersectionObserver(function (entries) {
    // since there is a single target to be observed, there will be only one entry
    if (entries[0]['isIntersecting'] === true) {
      var url = "/Post/Impressions/" + props.post.PostId;
      fetch(url).then(function (response) {
        return response.text();
      }).then(function (html) {
        setImpressions(html);
      });
    }
  }, { threshold: [0.1] });

  async function initializeHover() {
    await applyHoverToChildren(document, ".userhint");
  }

  async function initialize() {
    // Do this in parallel
    await Promise.all([initializeHover()]);
  }

  const groupLabelRef = useCallback(node => {
    if (node !== null) {
      loadgrouphover(node)
    }
  }, []);

  const postContentRef = useCallback(node => {
    if (node !== null) {
      impressionObserver.observe(node);
      makeQuotable(node, true);
      node.classList.remove("post-quotable");

      // This will trigger after render - check if the readmore button needs to be displayed
      window.requestAnimationFrame(function () {
        if (node !== undefined) {
          var height = parseFloat(getComputedStyle(node, null).height.replace("px", ""));
          if (height >= 800) {
            var readmoreButtonEl = node.querySelectorAll(".read-more-button").item(0);
            if (readmoreButtonEl != null) {
              readmoreButtonEl.style.display = "initial";
            }          
          }
        }
      });

      return () => {
        console.log("unmounted observer", impressionObserver, node);
        impressionObserver.unobserve(node);
      }
    }
  }, []);

  // Monitor for changes in props
  useEffect(
    () => {
      setPost(props.post);
      setIsIgnored(props.post.ViewerIgnoredUser);
      setIsLoggedIn(props.isLoggedIn);
      setIsMod(props.isGroupMod);

      if (window.UserName == props.post.UserName) {
        setIsAuthor(true);
      }

      if (window.IsAdmin) {
        setIsSiteAdmin(true);
      }

      initialize();

      return () => {
        // cleanup
      }
    },
    [props.post, props.isLoggedIn, props.isGroupMod]
  );

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
      <div className="social-feed-box" id={"post_" + post.PostId}>
        <button className="pull-left btn btn-sm btn-link" style={{
          display: "flex",
          paddingLeft: "4px"
        }} onClick={toggleVisible} >
          <i className="fa fa-minus-square togglebutton" ref={toggleVisibleIconRef}></i>
        </button>

        <Dropdown className="pull-right social-action">
          <Dropdown.Toggle bsPrefix="zr-btn" className="dropdown-toggle btn-white"></Dropdown.Toggle>
          <Dropdown.Menu as="ul" align="right" className="zr-dropdown-menu dropdown-menu-right m-t-xs">
            <Dropdown.Item as="li" disabled={true}>
              <button className="btn btn-link btn-sm">
                <i className="fa fa-eye"></i>&nbsp;{impressions}&nbsp;Impression(s)
              </button>
            </Dropdown.Item>
            { post.ViewerIgnoredUser ? (
              <Dropdown.Item as="li" onClick={() => { alert("not yet implemented"); }}>
                <button className="btn btn-link btn-sm">
                  <i className="fa fa-eye"></i>&nbsp;Show Post
                </button>
              </Dropdown.Item>
            ) : (<></>) }
            {isMod ? (
              <Dropdown.Item as="li" onClick={() => { stickyPost(post.PostId); }}>
                <button className="btn btn-link btn-sm">
                  <i className="fa fa-map-pin"></i> Toggle Group Sitcky
                </button>
              </Dropdown.Item>
            ) : (<></>)}
            {isSiteAdmin || isAdmin ? (<>
              <Dropdown.Item as="li" onClick={() => { setPostLanguage(post.PostId); }}>
                <button className="btn btn-link btn-sm" type="submit" >
                  <i className="fa fa-times"></i>&nbsp;Set Language (Admin)
                </button>
              </Dropdown.Item>
            </>) : (<></>) }
            {isSiteAdmin || isAuthor ? (<>
              <Dropdown.Item as="li" onClick={() => {
                location.href = "/Post/Edit?postId=" + post.PostId;
              }}>
                <button className="btn btn-link btn-sm" role="button">
                  <i className="fa fa-edit"></i>&nbsp;Edit
                </button>
              </Dropdown.Item>
              <Dropdown.Item as="li" onClick={() => { deletePost(post.PostId);}}>
                <button className="btn btn-link btn-sm" type="submit">
                  <i className="fa fa-times"></i>&nbsp;Delete
                </button>
              </Dropdown.Item>
            </>) : (<></>) }
            {isMod || isAdmin || isAuthor ? (<>
              <Dropdown.Item as="li" onClick={() => { nsfwPost(post.PostId); }}>
                <button className="btn btn-link btn-sm">
                  <i className="fa fa-exclamation-triangle"></i> Toggle NSFW
                </button>
              </Dropdown.Item>
              <Dropdown.Item as="li" onClick={() => { nsfwPost(post.PostId); }}>
                <button className="btn btn-link btn-sm">
                  <i className="fa fa-exclamation-triangle"></i> Toggle Explicit
                </button>
              </Dropdown.Item>
            </>) : (<>
              <Dropdown.Item as="li" onClick={() => { alert("Not yet implemented.  Feature coming soon."); }}>
                <button className="btn btn-link btn-sm">
                  <i className="fa fa-exclamation-triangle"></i> Report NSFW
                </button>
              </Dropdown.Item>
            </>)}
            <Dropdown.Item as="li" onClick={() => { alert("not yet implemented"); }}>
              <button className="btn btn-link btn-sm" type="submit">
                <i className="fa fa-flag"></i> Report Spam
              </button>
            </Dropdown.Item>
          </Dropdown.Menu>
        </Dropdown>

        <div className="social-avatar" style={{ paddingBottom: "15px" }}>
          <PostVoteButtons
            postId={post.PostId}
            postScore={post.Score}
            viewerUpvoted={post.ViewerUpvoted}
            viewerDownvoted={post.ViewerDownvoted}
            viewerIsBanished={post.ViewerIsBanishedGroup}
          />

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

              <a className="post-username userhint" data-userid={post.UserId} data-userappid={post.UserAppId}
                href={"/User/" + post.UserName}>
                {isIgnored ? (<>(Ignored)</>) : (<>{post.UserName}</>)}
              </a>

              {post.IsSticky ? (
                <span title="Sticky">
                  <i className="fa fa-map-pin" style={{ color: "lightgreen" }}></i>
                </span>
              ) : (<></>)}
              &nbsp;posted in&nbsp;
              <a
                ref={groupLabelRef}
                className="post-groupname"
                data-groupid={post.GroupId}
                href={
                  "/Group/Detail/" + post.GroupId
                }
                style={{ fontSize: "small", display: "inline" }}>
                {post.GroupName}
              </a>
              {" "}
              <small className="text-muted">
                {ISOtoRelative(post.TimeStamp)}
              </small>
              {post.TimeStampEdited ? (
                <>
                  <span className="text-muted">
                    &nbsp;edited&nbsp;
                  </span>
                  <small className="text-muted">
                    {ISOtoRelative(post.TimeStampEdited)}
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
                data-userappid={post.UserAppId}
                data-userid={post.UserId}>
                <div className="post-content ql-editor ql-container ql-snow">
                  <div dangerouslySetInnerHTML={{ __html: post.Content }} />
                </div>
                <p className="read-more-button">
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

        <div className="social-comment-box" id={"comments_" + post.PostId} style={{ display: isVisible ? "block" : "none" }}>
          <Suspense fallback={<></>}>
            <CommentsView
              comments={post.CommentVms}
              numRootComments={post.NumRootComments}
              isLoggedIn={props.isLoggedIn}
              viewerIsBanished={post.ViewerIsBanishedGroup}
              postId={post.PostId} />
          </Suspense>
        </div>
      </div >
    </>
  );
}