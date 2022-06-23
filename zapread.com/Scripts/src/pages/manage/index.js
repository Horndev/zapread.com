/**
 * Scripts for User Management
 * 
 * Native JS
 */

import "../../shared/shared";
/*import '../../utility/ui/vote';*/
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { Row, Col, Button } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faArrowDown,
  faCircleNotch
} from '@fortawesome/free-solid-svg-icons'

import UserProfile from "../../Components/User/UserProfile";
import UserReferralInfo from "../../Components/User/UserReferralInfo";
import SecuritySettings from "../../Components/User/SecuritySettings";
import NotificationSettings from "../../Components/User/NotificationSettings";
import CustomizationSettings from "../../Components/User/CustomizationSettings";
import UserFinanceInfo from "../../Components/User/UserFinanceInfo";
import UserFollowInfo from "../../Components/User/UserFollowInfo";
import UserGroupInfo from "../../Components/User/UserGroupInfo";
import LoadingBounce from "../../Components/LoadingBounce";
const PostList = React.lazy(() => import("../../Components/PostList"));
const VoteModal = React.lazy(() => import("../../Components/VoteModal"));

import '../../css/pages/manage/manage.scss';
import '../../shared/sharedlast';

function Page() {
  const [hasMorePosts, setHasMorePosts] = useState(false);
  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postsLoaded, setPostsLoaded] = useState(false);
  const [postBlockNumber, setPostBlockNumber] = useState(0);
  const [posts, setPosts] = useState([]);
  const [sort, setSort] = useState("Score");

  async function getMorePosts() {
    if (!isLoadingPosts) {
      setIsLoadingPosts(true);

      await postJson("/api/v1/post/feed/", {
        Sort: sort,
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
  }, [sort]); // Fire once

  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>

      <div className="wrapper wrapper-content">
        <Row>
          <Col md={4}>
            <UserProfile userAppId={window.userAppId} isOwnProfile={true}/>
            <UserFollowInfo userAppId={window.userAppId} isOwnProfile={true}/>
            <UserReferralInfo />
            <UserFinanceInfo />
            <SecuritySettings />
            <NotificationSettings />
            <CustomizationSettings />
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

// Make global (called from html)
//window.writeComment = writeComment;
//window.replyComment = replyComment;
//window.editComment = editComment;
//window.loadMoreComments = loadMoreComments;
//window.BlockNumber = 10;  //Infinite Scroll starts from second block
//window.NoMoreData = false;
//window.inProgress = false;

//async function LoadActivityPostsAsync() {
//  await fetch("/Manage/GetActivityPosts").then(response => {
//    return response.text();
//  }).then(html => {
//    var loadingEl = document.getElementById("posts-loading");
//    if (loadingEl) {
//      loadingEl.classList.remove("sk-loading");
//    }
//    var postsBoxEl = document.getElementById("posts");
//    if (postsBoxEl) {
//      postsBoxEl.innerHTML = html;
//    }
//    getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
//      onLoadedMorePosts();
//    });
//  })
//}
//LoadActivityPostsAsync();

///**
// * Wrapper for load more
// **/
//export function manageloadmore(userId) {
//  loadmore({
//    url: '/User/InfiniteScroll/',
//    blocknumber: window.BlockNumber,
//    sort: "New",
//    userId: userId
//  });
//}
//window.loadmore = manageloadmore;

// Set group list as clickable
//var elements = document.querySelectorAll(".clickable-row");
//Array.prototype.forEach.call(elements, function (el, _i) {
//  el.addEventListener("click", function (e) {
//    console.log(e,el);
//    var url = el.getAttribute('data-href')
//    window.location = url;
//  }, false);
//});