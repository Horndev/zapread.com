/*
 * View a group posts
 */

import '../../shared/shared';       // [✓]
import '../../realtime/signalr';    // [✓]

import React, { useCallback, useEffect, useState, useRef } from 'react'; // [✓]
import { Row, Col, Form, Button, Container } from "react-bootstrap";     // [✓]
import ReactDOM from "react-dom";                                        // [✓]
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom'; // [✓]
import { postJson } from "../../utility/postData";
import { loadmore } from "../../utility/loadmore";

import JoinLeaveButton from "./Components/JoinLeaveButton";
import IgnoreButton from "./Components/IgnoreButton";
import GroupAdminBar from "./Components/GroupAdminBar";
import GroupModBar from "./Components/GroupModBar";
import PostList from "../post/Components/PostList";
import VoteModal from "../../Components/VoteModal";

import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";

import '../../shared/postfunctions';                                        // [✓]
import '../../shared/readmore';                                             // [✓]
import '../../shared/postui';                                               // [✓]
import '../../shared/sharedlast';                                           // [✓]

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [groupName, setgroupName] = useState("");
  const [groupDescription, setgroupDescription] = useState("");
  const [groupId, setGroupId] = useState(-1);
  const [groupTier, setGroupTier] = useState(0);
  const [groupEarned, setGroupEarned] = useState(0);
  const [numMembers, setNumMembers] = useState(0);
  const [isLoaded, setIsLoaded] = useState(false);
  const [isIgnoring, setIsIgnoring] = useState(false);
  const [isGroupAdmin, setIsGroupAdmin] = useState(false);
  const [isGroupMod, setIsGroupMod] = useState(false);
  const [isGroupMember, setIsGroupMember] = useState(false);
  const [imageId, setImageId] = useState(0);
  const [posts, setPosts] = useState([]);
  const [hasMorePosts, setHasMorePosts] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postBlockNumber, setPostBlockNumber] = useState(0);

  const { pgroupId } = useParams();

  let query = useQuery();

  async function getMorePosts() {
    if (!isLoadingPosts) {
      setIsLoadingPosts(true);
      if (document.querySelectorAll('#loadmore').length) {
        document.querySelectorAll('#loadmore').item(0).style.display = '';
        document.querySelectorAll('#btnLoadmore').item(0).disabled = true;
      }
      
      await postJson("/api/v1/groups/posts/", {
        groupId: pgroupId,
        blockNumber: postBlockNumber
      }).then((response) => {
        if (document.querySelectorAll('#loadmore').length) {
          document.querySelectorAll('#loadmore').item(0).style.display = 'none';
          document.querySelectorAll('#btnLoadmore').item(0).disabled = false;
        }
        if (response.success) {
          var postlist = posts.concat(response.Posts); // Append posts to list - this will re-render them.
          setPosts(postlist);
          setHasMorePosts(response.HasMorePosts);
          if (response.HasMorePosts) {
            setPostBlockNumber(postBlockNumber + 1);
          }
          else {
            if (document.querySelectorAll('#loadmore').length) {
              document.querySelectorAll('#loadmore').item(0).style.display = 'none';
            }
          }
        }
        setIsLoadingPosts(false);
      });
    }
  }

  useEffect(() => {
    async function initialize() {
      if (pgroupId != null & pgroupId > 0 & !isLoaded) {
        setGroupId(pgroupId);
        await postJson("/api/v1/groups/load/", {
          groupId: pgroupId
        }).then((response) => {
          if (response.success) {
            setIsLoaded(true);
            setGroupId(response.group.Id);
            setgroupName(response.group.Name);
            setgroupDescription(response.group.ShortDescription);
            setImageId(response.group.IconId);
            setNumMembers(response.group.NumMembers);
            setIsGroupAdmin(response.group.IsAdmin);
            setIsGroupMod(response.group.IsMod);
            setIsGroupMember(response.group.IsMember);
            setIsIgnoring(response.group.IsIgnoring);
            setGroupTier(response.group.Level);
            setGroupEarned(response.group.Earned);
            setIsLoggedIn(response.IsLoggedIn);

            // Needed for the vote.js to work.  [TODO] make this non-global
            window.IsAuthenticated = response.IsLoggedIn;
            window.UserName = response.UserName;
          }
        });

        getMorePosts();
      }
    }
    initialize();
  }, [pgroupId]); // Fire once

  return (
    <>
      <VoteModal />

      <div className="wrapper border-bottom white-bg page-heading">
        <div className="col-lg-10">
          <br />
          <p className="pull-right">
            <a className="btn btn-sm btn-link" href={"/Group/Members/" + groupId}>
              <i className="fa fa-users"></i> <span id={"group_membercount_" + groupId}>{numMembers}</span> Members
            </a>
            <IgnoreButton isIgnoring={isIgnoring} id={groupId}/>
            <JoinLeaveButton isMember={isGroupMember} id={groupId} />
          </p>
          <h2>
            <i className=""></i> <span className={isLoaded ? "" : "placeholder col-8 bg-light"}>{groupName}</span>&nbsp;-&nbsp;<span className={isLoaded ? "" : "placeholder col-12 bg-light"}>{groupDescription}</span>
          </h2>
          <ol className="breadcrumb">
            <li className="breadcrumb-item"><a href="/">Home</a></li>
            <li className="breadcrumb-item"><a href="/Group">Groups</a></li>
            <li className="breadcrumb-item active">{groupName}</li>
          </ol>
          <small>Tier&nbsp;{groupTier}&nbsp;-&nbsp;{groupEarned}&nbsp;Satoshi earned</small>
        </div>
        <div className="col-lg-2">
        </div>
      </div>
      {isGroupAdmin ? (<div> <GroupAdminBar id={groupId} /> </div>) : (<div></div>)}
      {isGroupMod ? (<div> <GroupModBar id={groupId} /> </div>) : (<div></div>)}
      <div className="wrapper wrapper-content ">
        <div className="row">
          <div className="col-sm-2"></div>
          <div className="col-lg-8">
            <div className="social-feed-box-nb">
              <span></span>
            </div>

            <div className="social-feed-box-nb">
              <button onClick={() => {
                  location.href = "/Post/Edit/?groupId=" + groupId;
                }}
                className="btn btn-primary btn-outline btn-block">
                <i className="fa fa-plus"></i>{" "}Add Post
              </button>
            </div>

            <PostList posts={posts} isLoggedIn={isLoggedIn} isGroupMod={isGroupMod}/>

            {hasMorePosts ? (
              <div className="social-feed-box-nb" id="showmore">
                <button id="btnLoadmore" className="btn btn-primary btn-block"
                  onClick={() => {
                    getMorePosts();
                  }}>
                  <i className="fa fa-arrow-down"></i>&nbsp;Show More&nbsp;<i id="loadmore" className="fa fa-circle-o-notch fa-spin" style={{display:"none"}}></i>
                </button>
              </div>
              ) : (<></>)
            }
            <div className="social-feed-box-nb">
              <span></span>
            </div>
            <div className="social-feed-box-nb" style={{marginBottom: "50px"}}>
              <span></span>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

ReactDOM.render(
  <Router>
    <Route path="/Group/GroupDetail/:pgroupId">
      <Page />
    </Route>
  </Router>
  , document.getElementById("root"));