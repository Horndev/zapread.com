/**
 * This is the main landing page for ZapRead
 **/

import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';
import '../../shared/shared';
import '../../realtime/signalr';
import React, { Suspense, useEffect, useState } from "react";
import ReactDOM from "react-dom";
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom';
import { Row, Col, Button } from "react-bootstrap";
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";

const getloadMoreComments = () => import('../../comment/loadmorecomments');
const getMicroCharts = () => import('micro-charts');
const getTippy = () => import('tippy.js');
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import {
  faPlus,
  faArrowDown,
  faCircleNotch,
  faFire,
  faCertificate,
  faComments
} from '@fortawesome/free-solid-svg-icons'

import PageHeading from "../../Components/PageHeading";
import LoadingBounce from "../../Components/LoadingBounce";
import TopGroups from "../../Components/TopGroups";
const PostList = React.lazy(() => import("../../Components/PostList"));
const VoteModal = React.lazy(() => import("../../Components/VoteModal"));

import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';
import '../../css/site.scss';
import '../../css/quill/quillcustom.scss';
import '../../css/posts.css'

window.addEventListener('resize', function (event) {
  var elements = document.querySelectorAll(".post-box");
  Array.prototype.forEach.call(elements, function (el, _i) {
    if (!el.classList.contains('read-more-expanded')) {
      if (parseFloat(getComputedStyle(el, null).height.replace("px", "")) >= 800) {
        el.querySelectorAll(".read-more-button").item(0).style.display = 'initial';
      }
      else {
        // Hide
        el.querySelectorAll(".read-more-button").item(0).style.display = 'none';
      }
    }
  });
}, true);

async function LoadTopGroupsAsync() {
  await fetch("/Home/TopGroups").then(response => {
    return response.text();
  }).then(html => {
    var groupsBoxEl = document.getElementById("group-box");
    if (groupsBoxEl != null) {
      groupsBoxEl.innerHTML = html;
    }
  })
}
LoadTopGroupsAsync();

getMicroCharts().then(({ createPieChart }) => {
  function getCanvas(id) {
    return document.getElementById(id);
  }
  var payoutDate = new Date();
  payoutDate.setUTCHours(24, 0, 0, 0); //next midnight
  var timer;
  function getTimeString() {
    var now = new Date().getTime();
    var distance = payoutDate - now;
    var hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    var minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
    var seconds = Math.floor((distance % (1000 * 60)) / 1000);
    var timeStr = hours.toString().padStart(2, '0')
      + ":" + minutes.toString().padStart(2, '0')
      + ":" + seconds.toString().padStart(2, '0');
    return timeStr;
  }
  async function showCommunityPayoutTimer() {
    await getJson("/Home/GetPayoutInfo/")
      .then(response => {
        if (response.success) {
          var amountEl = document.getElementById("amount-info-payout");
          amountEl.innerHTML = response.community;
          var now = new Date().getTime();
          var distance = payoutDate - now;
          var percent = 100 * distance / (1000 * 60 * 60 * 24);
          var bgcolor = window.getComputedStyle(document.getElementById('zrph'), null).getPropertyValue('background-color');
          createPieChart(getCanvas('pc-community'),
            [
              { id: '1', percent: 100 - percent, color: bgcolor }, // green '#4CAF50'
              { id: '2', percent: percent, color: '#1ab39455' }  // red '#E91E63'
            ],
            { size: 25 });
        }
      });

    var timerEl = document.getElementById("timer-info-payout");
    timerEl.innerHTML = getTimeString();
    // Update every second
    timer = setInterval(function () {
      var timerEl = document.getElementById("timer-info-payout");
      timerEl.innerHTML = getTimeString();
    }, 1000);
  }

  showCommunityPayoutTimer();

  getTippy().then(({ default: tippy }) => {
    var infoEl = document.getElementById("hover-info-payout");
    tippy(infoEl, {
      content: '<div class="text-center" style="margin-top:5px;"><strong>Community Payout Timer!</strong></div><hr>This community payout is distributed daily to the highest scoring posts over the last 30 days.  Your votes increase this payout.',
      theme: 'light-border',
      allowHTML: true,
      delay: 300,
      interactive: true,
      interactiveBorder: 30,
    });
  })
})

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  
  const [hasMorePosts, setHasMorePosts] = useState(false);
  const [isLoadingPosts, setIsLoadingPosts] = useState(false);
  const [postsLoaded, setPostsLoaded] = useState(false);
  const [postBlockNumber, setPostBlockNumber] = useState(0);
  const [posts, setPosts] = useState([]);
  const [sort, setSort] = useState("Score");
  const [topGroupsIsExpanded, setTopGroupsIsExpanded] = useState(false); // Start open on groups page  

  let query = useQuery();

  async function getMorePosts() {
    if (!isLoadingPosts) {
      setIsLoadingPosts(true);

      let qsort = query.get("sort");
      if (qsort != null) {
        setSort(qsort);
      }

      await postJson("/api/v1/post/feed/", {
        Sort: qsort ? qsort : sort,
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
      await Promise.all([getMorePosts()]);
    }
    initialize();
  }, [sort]); // Fire once

  const SORTNAMES = {
    score: "Popular",
    new: "New",
    active: "Active"
  }

  return (
    <>
      <Suspense fallback={<></>}>
        <VoteModal />
      </Suspense>

      <PageHeading
        pretitle={<></>}
        title={<>{SORTNAMES[sort.toLowerCase()]}</>}
        controller="Home"
        method="Feeds"
        function={SORTNAMES[sort.toLowerCase()]}
        topGroups={true}
        topGroupsExpanded={topGroupsIsExpanded}
        onTopGroupsClosed={() => { setTopGroupsIsExpanded(false) }}
        onTopGroupsOpened={() => { setTopGroupsIsExpanded(true) }}
        breadcrumbRight={true}
        middleCol={
          <>
            <a href="/?sort=Score" className="zr-top-btn-rounded"><FontAwesomeIcon icon={faFire} />{" "}hot</a>
            <a href="/?sort=New" className="zr-top-btn-rounded"><FontAwesomeIcon icon={faCertificate} />{" "}new</a>
            <a href="/?sort=Active" className="zr-top-btn-rounded"><FontAwesomeIcon icon={faComments} />{" "}active</a>
          </>
        }
        rightCol={
          <>
            <div className="ibox-title" id="hover-info-payout" style={{whiteSpace: "nowrap", textAlign: "center"}}>
              <b><span id="amount-info-payout"></span>&nbsp;</b>
              <canvas id="pc-community" className="piechart" style={{verticalAlign: "middle", maxHeight: "25px"}}></canvas>&nbsp;
              <span id="timer-info-payout"></span>
            </div>
          </>}>
      </PageHeading>

      <div className="wrapper wrapper-content">
        <Row>
          <Col lg={2}>
            <TopGroups expanded={topGroupsIsExpanded} />
          </Col>
          <Col lg={8}>
            <div className="social-feed-box-nb"><span></span></div>
            <div className="social-feed-box-nb">
              <Button variant="primary" block onClick={() => { location.href = "/Post/Edit/"; }}><FontAwesomeIcon icon={faPlus} />{" "}Add Post</Button>
            </div>
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
            <div className="social-feed-box-nb"><span></span></div>
            <div className="social-feed-box-nb" style={{ marginBottom: "70px" }}><span></span></div>
          </Col>
        </Row>
      </div>
    </>
  );
}

ReactDOM.render(
  <Router>
    <Route path="/">
      <Page />
    </Route>
  </Router>
  , document.getElementById("root"));