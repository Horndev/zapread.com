/*
 * Post Detail view
 * 
 * http://localhost:27543/p/1-/i-just-authenticated-with-lnurl-auth/#cid_145
 * 
 */
import '../../shared/shared';
/*import '../../utility/ui/vote';*/
import '../../realtime/signalr';

import React, { Suspense, useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { Row, Col, Button } from "react-bootstrap";
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom';
import { postJson } from "../../utility/postData";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faPlus,
} from '@fortawesome/free-solid-svg-icons'

import PageHeading from "../../Components/PageHeading";
import PostView from "../../Components/PostView";
import LoadingBounce from "../../Components/LoadingBounce";
const VoteModal = React.lazy(() => import("../../Components/VoteModal"));

import '../../shared/sharedlast';
import '../../css/quill/quillcustom.scss';

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [post, setPost] = useState({});
  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postsLoaded, setPostsLoaded] = useState(false);
  const { ppostidenc, ptitle, ppostid } = useParams();

  let query = useQuery();

  useEffect(() => {
    //post loaded
    if (showVoteDialog) {
      var hash = window.location.hash;
      var el;
      if (hash && hash.split('_')[0] === "#cid") {
        var commentId = hash.split('_')[1];

        if (detailPostVote === 1) {
          el = document.getElementById('uVotec_' + commentId);
        } else {
          el = document.getElementById('dVotec_' + commentId);
        }

        // Show vote modal - vote for comment
        const event = new CustomEvent('vote', {
          detail: {
            direction: detailPostVote == 1 ? 'up' : 'down',
            type: 'comment',
            id: commentId,
            target: el,
            userInfo: window.userInfo
          }
        });
        document.dispatchEvent(event);
      } else if (hash) {
        // Bad anchor
        alert('Error voting');
      } else {
        if (detailPostVote === 1) {
          el = document.getElementById('uVote_' + detailPostId);
        } else {
          el = document.getElementById('dVote_' + detailPostId);
        }
        // Show vote modal - vote for post
        const event = new CustomEvent('vote', {
          detail: {
            direction: detailPostVote == 1 ? 'up' : 'down',
            type: 'post',
            id: detailPostId,
            target: el,
            userInfo: window.userInfo
          }
        });
        document.dispatchEvent(event);
      }
    }
  }, [post])

  useEffect(() => {
    async function initialize() {
      setIsLoadingPosts(true);
      await postJson("/api/v1/post/get/", {
        PostIdEnc: ppostidenc,
        PostId: ppostid ? ppostid : -1
      }).then((response) => {
        if (response.success) {
          setPost(response.Post);
          setPostsLoaded(true);
        }
        setIsLoadingPosts(false);
      });
    }
    initialize();
  }, [ppostidenc]); // Fire once

  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>

      <PageHeading
        title={post.PostTitle}
        controller="Post"
        method="Detail"
        function="View"
        topGroups={false}
        topGroupsExpanded={false}
      />
      <div className="wrapper wrapper-content ">
        <Row style={{ marginTop: "20px" }}>
          <Col sm={2}></Col>
          <Col lg={8}>
            <div className="social-feed-box-nb"><span></span></div>
            <div className="social-feed-box-nb">
              <Button variant="primary" block onClick={() => { location.href = "/Post/Edit/"; }}><FontAwesomeIcon icon={faPlus} />{" "}Add Post</Button>
            </div>
            {postsLoaded ? (<>
              <Suspense fallback={<><LoadingBounce /></>}>
                <PostView key={post.PostId} post={post} isLoggedIn={window.IsAuthenticated} isGroupMod={post.ViewerIsMod} />
              </Suspense>
            </>) : (<><LoadingBounce /></>)}
            <div className="social-feed-box-nb" style={{ marginBottom: "100px" }}><span></span></div>
          </Col>
          <Col sm={2}></Col>
        </Row>
      </div>
    </>);
}

ReactDOM.render(
  <Router>
    <Route path="/p/:ppostidenc/:ptitle?">
      <Page />
    </Route>
    <Route path="/Post/Detail/:ppostid/:ptitle?">
      <Page />
    </Route>
  </Router>,
  document.getElementById("root"));

///* Vote Modal Component */
//getVoteModal().then(({ default: VoteModal }) => {
//  ReactDOM.render(<VoteModal />, document.getElementById("ModalVote"));
//  const event = new Event('voteReady');
//  document.dispatchEvent(event);
//});

// Make global (called from html)
//window.writeComment = writeComment;
//window.replyComment = replyComment;
//window.editComment = editComment;

//getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
//  onLoadedMorePosts();
//});

async function Initialize() {
  // This opens up the vote modal if user clicked to vote (on incomming)
  if (showVoteDialog) {
    document.addEventListener("voteReady", function (e) {
      // Check if vote is for the post, or a comment - this is done using the anchor hash in the url
      var hash = window.location.hash; ///$(location).attr('hash');
      //alert(hash);
      var el;
      if (hash && hash.split('_')[0] === "#cid") {
        var commentId = hash.split('_')[1];

        if (detailPostVote === 1) {
          el = document.getElementById('uVotec_' + commentId);
        } else {
          el = document.getElementById('dVotec_' + commentId);
        }

        // Show vote modal - vote for comment
        const event = new CustomEvent('vote', {
          detail: {
            direction: detailPostVote == 1 ? 'up' : 'down',
            type: 'comment',
            id: commentId,
            target: el
          }
        });
        document.dispatchEvent(event);
      } else if (hash) {
        // Bad anchor
        alert('Error voting');
      } else {
        if (detailPostVote === 1) {
          el = document.getElementById('uVote_' + detailPostId);
        } else {
          el = document.getElementById('dVote_' + detailPostId);
        }
        // Show vote modal - vote for post
        const event = new CustomEvent('vote', {
          detail: {
            direction: detailPostVote == 1 ? 'up' : 'down',
            type: 'post',
            id: detailPostId,
            target: el
          }
        });
        document.dispatchEvent(event);
      }
    });
  }
}

//Initialize();