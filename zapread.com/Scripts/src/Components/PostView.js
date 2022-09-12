/**
 * A Single Post
 */

import React, { Suspense, useCallback, useEffect, useState, createRef } from "react";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faBell, faBellSlash
} from '@fortawesome/free-solid-svg-icons'
import { Dropdown } from "react-bootstrap";
import { readMoreButton } from "../shared/readmore";
import { deletePost } from "../shared/postfunctions";
import { applyHoverToChildren } from '../utility/userhover';
import { loadgrouphover } from '../utility/grouphover';
import { writeComment } from '../comment/writecomment'
import { setPostLanguage, nsfwPost, stickyPost } from "../shared/postfunctions";
import { makeQuotable } from "../utility/quotable/quotable";
import { ISOtoRelative } from "../utility/datetime/posttime";
import { postJson } from "../utility/postData";
import PostVoteButtons from "./PostVoteButtons";
import ReactionBar from './ReactionBar';
import SharePostButton from './Share/SharePostButton';
const CommentsView = React.lazy(() => import("./CommentsView"));
const getSwal = () => import('sweetalert2');

export default function PostView(props) {
  const [post, setPost] = useState(props.post);
  const [isHidden, setIsHidden] = useState(false);
  const [isVisible, setIsVisible] = useState(props.isVisible ? props.isVisible : true);
  const [isIgnored, setIsIgnored] = useState(true);
  const [isSiteAdmin, setIsSiteAdmin] = useState(false);
  const [isAdmin, setIsAdmin] = useState(false);
  const [isMod, setIsMod] = useState(false);
  const [isFollowing, setIsFollowing] = useState(false);
  const [isAuthor, setIsAuthor] = useState(false);
  const [isDetailView, setIsDetailView] = useState(true);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isHoveringTitle, setIsHoveringTitle] = useState(false);

  const [isNSFWRevealed, setIsNSFWRevealed] = useState(false);

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

      setTimeout(() => {
        if (node !== undefined) {
          var height = parseFloat(getComputedStyle(node, null).height.replace("px", ""));
          if (height >= 800) {
            var readmoreButtonEl = node.querySelectorAll(".read-more-button").item(0);
            if (readmoreButtonEl != null) {
              readmoreButtonEl.style.display = "initial";
            }
            node.style.overflowY = "hidden";
          } else {
            //node.style.overflowY = "visible";
          }
        }
      }, 3000);

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
      if (props.post.ViewerIgnoredUser) {
        setIsVisible(false);
      }

      setIsLoggedIn(props.isLoggedIn);
      setIsMod(props.isGroupMod);
      setIsFollowing(props.post.ViewerIsFollowing);

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

  function toggleFollow() {
    var url = isFollowing ? "/api/v1/post/unfollow/" : "/api/v1/post/follow/"
    postJson(url, {
      PostId: post.PostId
    }).then((response) => {
      if (response.success) {
        setIsFollowing(!isFollowing);
        setIsHoveringTitle(false);
      }
    });
  }

  function ignorePost() {
    getSwal().then(({ default: Swal }) => {
      Swal.fire({
        title: "Are you sure?",
        text: "Once ignored, you will not see this post.",
        icon: "warning",
        showCancelButton: true
      }).then(function (willIgnore) {
        if (willIgnore.value) {
          postJson("/api/v1/post/ignore/", {
            PostId: post.PostId
          }).then((response) => {
            if (response.success) {
              // hide post
              setIsHidden(true);
            }
          });
        } else {
          console.log("cancelled ignore");
        }
      });
    });
  }

  const reportSpam = () => {
    makeReport(1, "Report Spam", "Do you wish to report this content as spam?");
  };

  const reportNSFW = () => {
    makeReport(2, "Report Not Safe for Work", "Do you wish to report this content as NSFW?");
  };

  const makeReport = (reportType, title, text) => {
    getSwal().then(({ default: Swal }) => {
      Swal.fire({
        title: title,
        text: text,
        icon: "warning",
        showCancelButton: true
      }).then(function (willReport) {
        if (willReport.value) {
          postJson("/api/v1/post/report/", {
            PostId: post.PostId,
            ReportType: reportType
          }).then((response) => {
            if (response.success) {
              Swal.fire("Success", "Your report has been sent to the moderators.", "success");
            }
            else {
              Swal.fire("Error", "Error: " + response.message, "error");
            }
          });
        } else {
          console.log("cancelled report");
        }
      });
    });
  }

  const revealNSFW = () => {
    setIsNSFWRevealed(true);
  };

  const postBodyClass = (post) => {
    return "post-quotable post-content ql-container ql-snow post-box"
      + ((post.IsNSFW && !isNSFWRevealed) ? " zr-nsfw-on" : "");
  }

  return (
    <>
      <div className="social-feed-box" id={"post_" + post.PostId} style={isHidden ? { display: "none" } : {}}>
        <button className="pull-left btn btn-sm btn-link" style={{
          display: "flex",
          paddingLeft: "4px"
        }} onClick={toggleVisible} >
          <i className={isVisible ? "fa fa-minus-square togglebutton" : "fa fa-plus-square togglebutton"} ref={toggleVisibleIconRef}></i>
        </button>

        <Dropdown className="pull-right social-action">
          <Dropdown.Toggle bsPrefix="zr-btn" className="dropdown-toggle btn-white"></Dropdown.Toggle>
          <Dropdown.Menu as="ul" align="right" className="zr-dropdown-menu dropdown-menu-right m-t-xs">
            <Dropdown.Item as="li" disabled={true}>
              <button className="btn btn-link btn-sm">
                <i className="fa fa-eye"></i>&nbsp;{impressions}&nbsp;Impression(s)
              </button>
            </Dropdown.Item>
            {isLoggedIn ? (
              <>

                {isFollowing ? (
                  <Dropdown.Item as="li" onClick={toggleFollow}>
                    <button className="btn btn-link btn-sm">
                      <i className="fa-regular fa-bell-slash"></i>&nbsp;Stop following
                    </button>
                  </Dropdown.Item>
                ) : (
                  <Dropdown.Item as="li" onClick={toggleFollow}>
                    <button className="btn btn-link btn-sm">
                      <i className="fa-regular fa-bell"></i>&nbsp;Follow post
                    </button>
                  </Dropdown.Item>
                )}
                <Dropdown.Item as="li" onClick={ignorePost}>
                  <button className="btn btn-link btn-sm">
                    <i className="fa-regular fa-eye-slash"></i>&nbsp;Ignore post
                  </button>
                </Dropdown.Item>

              </>) : (<></>)}
            
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

                {isLoggedIn ? (
                  <>
                    <Dropdown.Item as="li" onClick={reportNSFW}>
                      <button className="btn btn-link btn-sm">
                        <i className="fa fa-exclamation-triangle"></i> Report NSFW
                      </button>
                    </Dropdown.Item>
                  </>) : (<></>)}

            </>)}

            {isLoggedIn ? (
              <>
                <Dropdown.Item as="li" onClick={reportSpam}>
                  <button className="btn btn-link btn-sm" type="submit">
                    <i className="fa fa-flag"></i> Report Spam
                  </button>
                </Dropdown.Item>
              </>) : (<></>)}
            
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
            <div
              onMouseEnter={() => setIsHoveringTitle(true)} onMouseLeave={() => setIsHoveringTitle(false)}
              className="vote-title">
              {!isIgnored ? (
                <a href={post.PostIdEnc ? ("/p/" + post.PostIdEnc + "/" + post.PostTitleEnc) : ("/Post/Detail/" + post.PostId)}>
                {post.PostTitle == "" ? (
                  <>Post</>
                ) : (
                  post.PostTitle
                )}
                </a>) : (<><span>(Ignored)</span></>)}
                {isFollowing ? (
                  <>
                  {" "}
                  <FontAwesomeIcon
                    title="Stop Following Post"
                    icon={isHoveringTitle ? (faBellSlash) : (faBell)} onClick={toggleFollow} style={{ display: "inline" }} />
                  </>) : (<></>)}
             </div>
            <div className="vote-info" style={{ marginLeft: "110px" }}>

              <a className="post-username userhint" data-userid={post.UserId} data-userappid={post.UserAppId}
                href={"/User/" + post.UserName}>
                {isIgnored ? (<>(Ignored)</>) : (<>{post.UserName}</>)}
              </a>
              {post.IsNonIncome ? (
                <span title="Non-Income Post">
                  &nbsp;<i className="fa-solid fa-hand-holding-heart" style={{ color: "blueviolet" }}></i>&nbsp;
                </span>
              ) : (<></>)}
              {post.IsSticky ? (
                <span title="Sticky">
                  &nbsp;<i className="fa fa-map-pin" style={{ color: "lightgreen" }}></i>&nbsp;
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
              <div className={postBodyClass(post)}
                ref={postContentRef}
                data-postid={post.PostId}
                data-userappid={post.UserAppId}
                data-userid={post.UserId}>
                <div className="post-content ql-editor ql-container ql-snow">
                  <div dangerouslySetInnerHTML={{ __html: post.Content }} />
                </div>

                {(post.IsNSFW && !isNSFWRevealed) ? (
                  <>
                    <div id={"nsfw_" + post.PostId} className="zr-nsfw-post"></div>
                    <button id={"nsfwb_" + post.PostId}
                      className="btn btn-danger btn-outline btn-block zr-nsfw-post-button"
                      onClick={revealNSFW}>Show NSFW</button>
                  </>) : (<></>)}

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

        {/* If logged in, show interactive reaction bar*/}
        <ReactionBar l={isLoggedIn ? "1" : "0"} postId={post.PostId} />
        <div style={{ height: "30px" }}>
          <div id={"wc_" + post.PostId}>
            {(post.IsNSFW ? (isNSFWRevealed && isVisible) : isVisible) ? (
              <>
                <span className="btn btn-link btn-sm" onClick={() => {
                  if (isLoggedIn) {
                    writeComment(post.PostId);
                  } else {
                    window.location = "https://www.zapread.com/Account/Login/";
                  }
                  
                }}>
                  <span className="badge badge-light">
                    {post.CommentVms.length}
                  </span>
                  {" "}<i className="fa fa-comments"></i>{" "}Write a comment
                </span>
              </>) : (
              <>
                <span className="btn btn-link btn-sm" onClick={() => setIsVisible(true)}>
                  <span className="badge badge-light">
                    {post.CommentVms.length}
                  </span>
                  {" "}<i className="fa fa-comments"></i>{" "}Comments
                </span>
              </>)}
            <SharePostButton postId={post.PostId} title={post.PostTitle}
              url={"https://www.zapread.com" + (post.PostIdEnc ? ("/p/" + post.PostIdEnc + "/" + post.PostTitleEnc) : ("/Post/Detail/" + post.PostId))} />
          </div>
        </div>

        {isLoggedIn ? (
          <>
            <div id={"reply_p" + post.PostId} style={{ display: "none" }}></div>
          </>
        ) : (<></>)}

        <div className="social-comment-box" id={"comments_" + post.PostId}
          style={{ display: (post.IsNSFW ? (isNSFWRevealed && isVisible) : isVisible) ? "block" : "none" }}>
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