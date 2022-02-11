/**
 * A Single Post
 */

import React, { useEffect, useState, createRef } from "react";
import { Dropdown } from "react-bootstrap";
import { readMoreButton } from "../../../shared/readmore";            // [✓]
import { deletePost } from "../../../shared/postfunctions";           // [✓]
import { ready } from '../../../utility/ready';                       // [✓]
import { applyHoverToChildren } from '../../../utility/userhover';    // [✓]
import { loadgrouphover } from '../../../utility/grouphover';         // [✓]
import { updatePostTimes } from '../../../utility/datetime/posttime'; // [✓]
import { writeComment } from '../../../comment/writecomment'          // [✓]
import { setPostLanguage, nsfwPost, stickyPost } from "../../../shared/postfunctions";      // [✓]
import { makeQuotable } from "../../../utility/quotable/quotable";
import PostVoteButtons from "./PostVoteButtons";
import CommentsView from "../../../comment/CommentsView";

export default function PostView(props) {
  const [post, setPost] = useState(props.post);
  const [isVisible, setIsVisible] = useState(true);
  const [isIgnored, setIsIgnored] = useState(true);
  const [isSiteAdmin, setIsSiteAdmin] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isMod, setIsMod] = useState(false);
  const [isAuthor, setIsAuthor] = useState(false);
  const [isInitialized, setIsInitialized] = useState(false);
  const [isDetailView, setIsDetailView] = useState(true);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [impressions, setImpressions] = useState(0);

  const postContentRef = createRef();
  const readMoreButtonRef = createRef();
  const toggleVisibleIconRef = createRef();

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

      async function loadImpressions() {
        const url = "/Post/Impressions/" + props.post.PostId
        await fetch(url)
          .then(function (response) {
            return response.text();
          })
          .then(function (html) {
            setImpressions(html);
          });
      }

      loadImpressions();
    },
    [props.post, props.isLoggedIn, props.isGroupMod]
  );

  useEffect(() => {
    if (!isInitialized) {

      var postElement = postContentRef.current; // save a ref to use in ready function

      ready(function () {
        var elements = document.querySelectorAll(".pop");
        Array.prototype.forEach.call(elements, function (el, _i) {
          el.classList.remove('pop');
        });

        applyHoverToChildren(document, ".userhint");

        elements = document.querySelectorAll(".grouphint");
        Array.prototype.forEach.call(elements, function (el, _i) {
          loadgrouphover(el);
          el.classList.remove('grouphint');
        });

        // --- relative times
        updatePostTimes();

        // configure read more
        if (parseFloat(getComputedStyle(postContentRef.current, null).height.replace("px", "")) >= 800) {
          readMoreButtonRef.current.style.display = "initial";
        }

        // Make post quotable
        makeQuotable(postElement, true);
        postElement.classList.remove("post-quotable");
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
                data-userappid={post.UserAppId}
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