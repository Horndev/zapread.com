/**
 * User information page
 * 
 **/
import '../../shared/shared';
/*import '../../utility/ui/vote';*/
import '../../realtime/signalr';

import React, { Suspense, useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { Row, Col, Button } from "react-bootstrap";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faArrowDown,
  faCircleNotch
} from '@fortawesome/free-solid-svg-icons'

//import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
//const getOnLoadedMorePosts = () => import('../../utility/onLoadedMorePosts');
//import { writeComment } from '../../comment/writecomment';
//import { replyComment } from '../../comment/replycomment';
//import { editComment } from '../../comment/editcomment';
//import { loadMoreComments } from '../../comment/loadmorecomments';
//import { loadachhover } from '../../utility/achievementhover';
//import { loadmore } from '../../utility/loadmore';
import { postJson } from "../../utility/postData";
//import '../../shared/postfunctions';
//import '../../shared/readmore';
//import '../../shared/postui';

import UserProfile from "../../Components/User/UserProfile";
import UserFollowInfo from "../../Components/User/UserFollowInfo";
import UserGroupInfo from "../../Components/User/UserGroupInfo";
import LoadingBounce from "../../Components/LoadingBounce";
import UserInteractions from "../../Components/User/UserInteractions";

import '../../css/pages/manage/manage.scss';
import '../../shared/sharedlast';

const PostList = React.lazy(() => import("../../Components/PostList"));
const VoteModal = React.lazy(() => import("../../Components/VoteModal"));

function Page() {
  const [hasMorePosts, setHasMorePosts] = useState(false);
  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postsLoaded, setPostsLoaded] = useState(false);
  const [postBlockNumber, setPostBlockNumber] = useState(0);
  const [posts, setPosts] = useState([]);
  //const [userId, setUserId] = useState(window.userId);
  const [userAppId, setUserAppId] = useState(window.userAppId);

  async function getMorePosts() {
    if (!isLoadingPosts) {
      setIsLoadingPosts(true);
      await postJson("/api/v1/user/feed/", {
        Sort: "New",
        UserAppId: userAppId,
        BlockNumber: postBlockNumber
      }).then((response) => {
        if (response.success) {
          var postlist = posts.concat(response.Posts); // Append posts to list - this will re-render them.
          setPosts(postlist);
          setPostsLoaded(true);
          setHasMorePosts(response.HasMorePosts);
          if (response.HasMorePosts) {
            setPostBlockNumber(postBlockNumber + 1);
          }
        }
        setIsLoadingPosts(false);
      });
    }
  }

  useEffect(() => {
    async function initialize() {
      // Do this in parallel
      await getMorePosts();
    }
    initialize();
  }, [userAppId]); // Fire once

  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>

      <div className="wrapper wrapper-content">
        <Row>
          <Col md={4}>
            <UserProfile userAppId={window.userAppId} isOwnProfile={false} />
            <UserInteractions userAppId={window.userAppId} isLoggedIn={window.IsAuthenticated} userName={window.userName}/>
            <UserFollowInfo userAppId={window.userAppId} isOwnProfile={false} />
            {/*<UserReferralInfo />*/}
            {/*<UserFinanceInfo />*/}
            {/*<SecuritySettings />*/}
            {/*<NotificationSettings />*/}
            {/*<CustomizationSettings />*/}
            <UserGroupInfo userAppId={window.userAppId} isOwnProfile={false}/>
            <div style={{ marginBottom: "100px" }}></div>
          </Col>

          <Col md={8} className="pb-5 pt-3">
            {postsLoaded ? (<>
              <Suspense fallback={<><LoadingBounce /></>}>
                <PostList
                  posts={posts}
                  isLoggedIn={window.IsAuthenticated} />

                {hasMorePosts ? (
                  <div className="social-feed-box-nb">
                    <Button block variant="primary" onClick={() => { getMorePosts(); }}>
                      <FontAwesomeIcon icon={faArrowDown} />{" "}Show More{" "}
                      {isLoadingPosts ? (<><FontAwesomeIcon icon={faCircleNotch} spin /></>) : (<></>)}
                    </Button>
                  </div>
                ) : (<></>)}

              </Suspense>
            </>) : (<><LoadingBounce /></>)}
          </Col>
        </Row>
      </div>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));

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
//window.loadMoreComments = loadMoreComments;
//window.toggleUserIgnore = toggleUserIgnore;
//window.BlockNumber = 10;                        // Infinite Scroll starts from second block
//window.NoMoreData = false;
//window.inProgress = false;

//var elements = document.querySelectorAll(".ach-hover");
//Array.prototype.forEach.call(elements, function (el, _i) {
//  loadachhover(el);
//});

/**
 * userAppId is set externally in the razor view
*/
//async function LoadFollowersAsync() {
//  //console.log("LoadFollowersAsync", window.userAppId);
//  await fetch("/User/Followers/" + window.userAppId).then(response => {
//    return response.text();
//  }).then(html => {
//    var groupsBoxEl = document.getElementById("top-followers");
//    groupsBoxEl.innerHTML = html;
//  })
//}

//async function LoadFollowingAsync() {
//  //console.log("LoadFollowingAsync", window.userAppId);
//  await fetch("/User/Following/" + window.userAppId).then(response => {
//    return response.text();
//  }).then(html => {
//    var groupsBoxEl = document.getElementById("top-following");
//    groupsBoxEl.innerHTML = html;
//  })
//}

//async function AddBlockClickHandler() {
//  var buttonEl = document.getElementById("btnBlockUser");
//  var userId = buttonEl.getAttribute("data-userid");
//  buttonEl.addEventListener("click", (e) => {
//    toggleUserBlock(userId);
//  });
//}

//async function Initialize() {
//  await Promise.all([
//    AddBlockClickHandler(),
//    LoadFollowersAsync(),
//    LoadFollowingAsync()
//  ]);
//}

//Initialize();

/**
 * Wrapper for loadmore
 * 
 * [✓] Native JS
 * 
 **/
//export function userloadmore(userId) {
//  loadmore({
//    url: '/User/InfiniteScroll/',
//    blocknumber: window.BlockNumber,
//    sort: "New",
//    userId: userId
//  });
//}
//window.loadmore = userloadmore;

//getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
//  onLoadedMorePosts();
//});

export function toggleUserIgnore(id) {
  postJson("/User/ToggleIgnore/", {
    id: id
  }).then((response) => {
    if (response.success) {
      if (response.added) {
        document.getElementById('i_' + id.toString()).innerHTML = "<i class='fa-solid fa-circle'></i> Un-Ignore ";
      }
      else {
        document.getElementById('i_' + id.toString()).innerHTML = "<i class='fa-solid fa-ban'></i> Ignore ";
      }
    }
  });
  return false;
}

export function toggleUserBlock(id) {
  postJson("/User/ToggleBlock/", {
    id: id
  }).then((response) => {
    if (response.success) {
      if (!response.added) {
        document.getElementById("btnBlockUser").innerHTML = "<i class='fa-solid fa-comment-slash'></i> Block ";
      }
      else {
        document.getElementById("btnBlockUser").innerHTML = "<i class='fa-solid fa-comment'></i> Un-Block ";
      }
    }
  });
  return false;
}