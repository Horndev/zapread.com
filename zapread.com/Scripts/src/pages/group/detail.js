/*
 * View a group posts
 */

import '../../shared/shared';
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState } from 'react';
import ReactDOM from "react-dom";
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom';
import { Container, Row, Col, DropdownButton, Dropdown, ButtonGroup, Button } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { ISOtoRelative } from "../../utility/datetime/posttime"
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faPlus,
  faArrowDown,
  faCircleNotch
} from '@fortawesome/free-solid-svg-icons'
import JoinLeaveButton from "./Components/JoinLeaveButton";
import IgnoreButton from "./Components/IgnoreButton";
import PageHeading from "../../Components/PageHeading";
import LoadingBounce from "../../Components/LoadingBounce";
const GroupAdminBar = React.lazy(() => import("./Components/GroupAdminBar"));
const GroupModBar = React.lazy(() => import("./Components/GroupModBar"));
const PostList = React.lazy(() => import("../../Components/PostList"));
const VoteModal = React.lazy(() => import("../../Components/VoteModal"));
import {ThemeColorContext} from "../../Components/Theme/ThemeContext";
import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';
import '../../css/quill/quillcustom.scss';

// Force prefetching code in parallel... (https://stackoverflow.com/questions/58687397/react-lazy-and-prefetching-components)
import("../../Components/PostList");
import("../../Components/VoteModal");

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
  const [postsLoaded, setPostsLoaded] = useState(false);
  const [isIgnoring, setIsIgnoring] = useState(false);
  const [isGroupAdmin, setIsGroupAdmin] = useState(false);
  const [isGroupMod, setIsGroupMod] = useState(false);
  const [isGroupBanished, setIsGroupBanished] = useState(false);
  const [banishedExpires, setBanishedExpires] = useState(null);
  const [isGroupMember, setIsGroupMember] = useState(false);
  const [imageId, setImageId] = useState(0);
  const [posts, setPosts] = useState([]);
  const [hasMorePosts, setHasMorePosts] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postBlockNumber, setPostBlockNumber] = useState(0);
  const [bgColor, setBgColor] = useState("white");
  const { pgroupId, pgroupname } = useParams();

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
        groupName: pgroupname,
        blockNumber: postBlockNumber
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

  const getTemplateValue = (template, key, defaultValue) => {
    if (template == null) return defaultValue;
    var ret = defaultValue;
    const kvps = template.split(';');
    kvps.every(kvp => {      
      const kv = kvp.split('=');
      if (kv[0] === key) {
        ret = kv[1];
        return false;
      }
      return true;
    });
    return ret;
  };

  async function loadGroupInfo() {
    if (pgroupId != null & pgroupId > 0 & !isLoaded) {
      setGroupId(pgroupId);
      await postJson("/api/v1/groups/load/", {
        groupId: pgroupId,
        groupName: pgroupname
      }).then((response) => {
        if (response.success) {
          window.document.title = response.group.Name + (response.group.ShortDescription != null ? " " + response.group.ShortDescription : "");
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
          setIsGroupBanished(response.group.IsBanished);
          setBanishedExpires(response.group.BanishExpires);

          var template = response.group.CustomTemplate;
          var newBgColor = getTemplateValue(template, 'headerBgColor', bgColor);
          setBgColor(newBgColor);

          // Needed for the vote.js to work.  [TODO] make this non-global
          window.IsAuthenticated = response.IsLoggedIn;
          window.UserName = response.UserName;
        }
      });
    }
  }

  useEffect(() => {
    async function initialize() {
      // Do this in parallel
      await Promise.all([getMorePosts(), loadGroupInfo()]);
    }
    initialize();
  }, [pgroupId, pgroupname]); // Fire once

  function changeBgColor(color) {
    setBgColor(color);
  }

  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>

      <ThemeColorContext.Provider
        value={{ bgColor: bgColor, changeBgColor: changeBgColor }}
      >
        <PageHeading
          pretitle={
            <>
              <p className="pull-right">
                <a className="btn btn-sm btn-link" href={"/Group/Members/" + groupId}>
                  <i className="fa fa-users"></i> <span id={"group_membercount_" + groupId}>{numMembers}</span> Members
                </a>
                {isLoggedIn ? (<>
                  <IgnoreButton isIgnoring={isIgnoring} id={groupId} />
                  <JoinLeaveButton isMember={isGroupMember} id={groupId} />
                </>) : (<></>)}
              </p>
            </>}
          title={
            <>
              <span className={isLoaded ? "" : "placeholder col-8 bg-light"}>{groupName}</span>&nbsp;-&nbsp;<span className={isLoaded ? "" : "placeholder col-12 bg-light"}>{groupDescription}</span>
            </>
          }
          controller="Home"
          method="Groups"
          function={groupName}
        >
          <small>Tier&nbsp;{groupTier}&nbsp;-&nbsp;{groupEarned}&nbsp;Satoshi earned</small>
        </PageHeading>
      </ThemeColorContext.Provider>

      {isGroupBanished ? (
        <>
          <Row>
            <Col className="red-bg" style={{margin:"15px", padding: "15px"}}>
              <i className="fa-solid fa-triangle-exclamation" style={{ color: "red" }}></i> You are banished from this group.  Your banishment expires { banishedExpires == null ? "soon": ISOtoRelative(banishedExpires, true)}.  You can't vote down or make posts to this group.
            </Col>
          </Row>
        </>) : (<></>)}
      {isGroupAdmin ? (<Suspense fallback={
        <div className="ibox" style={{marginBottom: "0px"}}>
          <div className="ibox-title bg-warning">
            Loading Administration...
          </div>
        </div>}>
        <GroupAdminBar
          id={groupId}
          onUpdateDescription={(description) => setgroupDescription(description)}
          tier={groupTier}
          onUpdateBgColor={(color) => setBgColor(color)}
        />
      </Suspense>) : (<></>)}
      {isGroupMod ? (<Suspense fallback={
        <div className="ibox" style={{ marginBottom: "0px" }}>
          <div className="ibox-title bg-info">
            Loading Moderation...
          </div>
        </div>}>
        <GroupModBar id={groupId} tier={groupTier}/>
      </Suspense>) : (<></>)}
      <div className="wrapper wrapper-content">
        <div className="row">
          <div className="col-sm-2"></div>
          <div className="col-lg-8">
            <div className="social-feed-box-nb">
              <span></span>
            </div>
            {isGroupBanished ? (<></>) : (<>
              <div className="social-feed-box-nb">
                <button onClick={() => {
                  location.href = "/Post/Edit/?groupId=" + groupId;
                }}
                  className="btn btn-primary btn-outline btn-block">
                  <i className="fa fa-plus"></i>{" "}Add Post
                </button>
              </div>
            </>)}
            {postsLoaded ? (<>
              <Suspense fallback={<><LoadingBounce /></>}>
                <PostList
                  posts={posts}
                  isLoggedIn={isLoggedIn}
                  isGroupMod={isGroupMod} />

                {hasMorePosts ? (
                  <div className="social-feed-box-nb">
                    <Button block variant="primary" onClick={() => { getMorePosts(); }}>
                      <FontAwesomeIcon icon={faArrowDown} />{" "}Show More{" "}
                      {isLoadingPosts ? (<><FontAwesomeIcon icon={faCircleNotch} spin /></>) : (<></>)}
                    </Button>
                  </div>
                ) : (<></>)}

              </Suspense>
            </>) : (<><LoadingBounce/></>)}
            <div className="social-feed-box-nb"><span></span></div>
            <div className="social-feed-box-nb" style={{marginBottom: "100px"}}><span></span>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

ReactDOM.render(
  <Router>
    <Route path="/Group/GroupDetail/:pgroupId/:pgroupname?">
      <Page />
    </Route>
    <Route path="/Group/Detail/:pgroupId/:pgroupname?">
      <Page />
    </Route>
    <Route path="/g/:pgroupId/:pgroupname?">
      <Page />
    </Route>
  </Router>
  , document.getElementById("root"));