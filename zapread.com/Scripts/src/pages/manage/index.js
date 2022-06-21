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

/*import Swal from 'sweetalert2';*/
/*const getOnLoadedMorePosts = () => import('../../utility/onLoadedMorePosts');*/
//import { writeComment } from '../../comment/writecomment';
//import { replyComment } from '../../comment/replycomment';
//import { editComment } from '../../comment/editcomment';
//import { loadMoreComments } from '../../comment/loadmorecomments';
/*import { loadmore } from '../../utility/loadmore';*/
//import { postJson } from "../../utility/postData";
//import { getJson } from "../../utility/getData";

import UserProfile from "../../Components/User/UserProfile";
import UserReferralInfo from "../../Components/User/UserReferralInfo";
import SecuritySettings from "../../Components/User/SecuritySettings";
import NotificationSettings from "../../Components/User/NotificationSettings";
import CustomizationSettings from "../../Components/User/CustomizationSettings";
import UserFinanceInfo from "../../Components/User/UserFinanceInfo";
import UserFollowInfo from "../../Components/User/UserFollowInfo";
import UserGroupInfo from "../../Components/User/UserGroupInfo";

const VoteModal = React.lazy(() => import("../../Components/VoteModal"));

import '../../css/pages/manage/manage.scss';

//import '../../shared/postfunctions';
//import '../../shared/readmore';
//import '../../shared/postui';
import '../../shared/sharedlast';

function Page() {
  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>

      <div className="wrapper wrapper-content">
        <Row>
          <Col md={4}>
            <UserProfile />
            <UserFollowInfo />
            <UserReferralInfo />
            <UserFinanceInfo />
            <SecuritySettings />
            <NotificationSettings />
            <CustomizationSettings />
            <UserGroupInfo />
            <div style={{ marginBottom: "100px" }}></div>
          </Col>

          <Col md={8}>

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