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

import { postJson } from "../../utility/postData";

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
                <div className="social-feed-box-nb" style={{ marginBottom: "50px" }}><span></span></div>
              </Suspense>
            </>) : (<><LoadingBounce /></>)}
          </Col>
        </Row>
      </div>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));