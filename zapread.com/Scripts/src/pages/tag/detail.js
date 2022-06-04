/*
 * View a tag posts
 */

import '../../shared/shared';
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState } from 'react';
import ReactDOM from "react-dom";
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom';
import { Container, Row, Col, DropdownButton, Dropdown, ButtonGroup, Button } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { ISOtoRelative } from "../../utility/datetime/posttime"
import PageHeading from "../../Components/PageHeading";
import LoadingBounce from "../../Components/LoadingBounce";
const PostList = React.lazy(() => import("../../Components/PostList"));
const VoteModal = React.lazy(() => import("../../Components/VoteModal"));
import { ThemeColorContext } from "../../Components/Theme/ThemeContext";
import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';
import '../../css/quill/quillcustom.scss';
import '../../css/posts.css'

// Force prefetching code in parallel... (https://stackoverflow.com/questions/58687397/react-lazy-and-prefetching-components)
import("../../Components/PostList");
import("../../Components/VoteModal");

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [tagName, setTagName] = useState("");
  const [tagId, setTagId] = useState(-1);
  const [isLoaded, setIsLoaded] = useState(false);
  const [postsLoaded, setPostsLoaded] = useState(false);
  const [posts, setPosts] = useState([]);
  const [hasMorePosts, setHasMorePosts] = useState(false);
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postBlockNumber, setPostBlockNumber] = useState(0);
  const [bgColor, setBgColor] = useState("white");
  const { ptagname } = useParams();

  let query = useQuery();

  async function getMorePosts() {
    if (!isLoadingPosts) {
      setIsLoadingPosts(true);
      if (document.querySelectorAll('#loadmore').length) {
        document.querySelectorAll('#loadmore').item(0).style.display = '';
        document.querySelectorAll('#btnLoadmore').item(0).disabled = true;
      }

      await postJson("/api/v1/tag/posts/", {
        TagName: ptagname,
        blockNumber: postBlockNumber
      }).then((response) => {
        if (document.querySelectorAll('#loadmore').length) {
          document.querySelectorAll('#loadmore').item(0).style.display = 'none';
          document.querySelectorAll('#btnLoadmore').item(0).disabled = false;
        }
        if (response.success) {
          var postlist = posts.concat(response.Posts); // Append posts to list - this will re-render them.
          setPosts(postlist);
          setPostsLoaded(true);
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

  async function loadTagInfo() {
    if (ptagname != null & !isLoaded) {
      await postJson("/api/v1/tag/load/", {
        TagName: ptagname
      }).then((response) => {
        if (response.success) {
          window.document.title = "#" + response.Tag.value;
          setIsLoaded(true);
          setTagId(response.Tag.id);
          setTagName(response.Tag.value);
          setIsLoggedIn(response.IsLoggedIn);

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
      await Promise.all([loadTagInfo(), getMorePosts()]);
    }
    initialize();
  }, [ptagname]); // Fire once

  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>
      <ThemeColorContext.Provider value={{ bgColor: bgColor, changeBgColor: null }}>
        <PageHeading
          pretitle={<></>}
          title={<><span className={isLoaded ? "" : "placeholder col-8 bg-light"}>{tagName}</span></>}
          controller="Tag"
          method="Detail"
          function={tagName}
          />
      </ThemeColorContext.Provider>
      
      <div className="wrapper wrapper-content ">
        <div className="row">
          <div className="col-sm-2"></div>
          <div className="col-lg-8">
            <div className="social-feed-box-nb">
              <span></span>
            </div>
            {postsLoaded ? (<>
              <Suspense fallback={<><LoadingBounce /></>}>
                <PostList
                  posts={posts}
                  isLoggedIn={isLoggedIn}
                  isGroupMod={false}
                  hasMorePosts={hasMorePosts}
                  onMorePosts={() => { getMorePosts(); }} />
              </Suspense>
            </>) : (<><LoadingBounce /></>)}
            <div className="social-feed-box-nb">
              <span></span>
            </div>
            <div className="social-feed-box-nb" style={{ marginBottom: "50px" }}>
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
    <Route path="/tag/:ptagname?">
      <Page />
    </Route>
    <Route path="/t/:ptagname?">
      <Page />
    </Route>
  </Router>
  , document.getElementById("root"));