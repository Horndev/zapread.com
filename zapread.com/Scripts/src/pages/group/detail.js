/*
 * View a group posts
 */
//import $ from 'jquery';

import '../../shared/shared';       // [✓]
//import '../../utility/ui/vote';     // [✓]
import '../../realtime/signalr';    // [✓]

import React, { useCallback, useEffect, useState, useRef } from 'react'; // [✓]
import { Row, Col, Form, Button, Container } from "react-bootstrap";     // [✓]
import ReactDOM from "react-dom";                                        // [✓]
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom'; // [✓]
import { postJson } from "../../utility/postData";

import JoinLeaveButton from "./Components/JoinLeaveButton";
import GroupAdminBar from "./Components/GroupAdminBar";
import GroupModBar from "./Components/GroupModBar";

//import '../../../summernote/dist/summernote-bs4';
//import 'summernote/dist/summernote-bs4.css';
//import '../../utility/summernote/summernote-video-attributes';
//import Swal from 'sweetalert2';

import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";

//import 'selectize/dist/js/standalone/selectize';
//import 'selectize/dist/css/selectize.css';
//import 'selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css';

//import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
//import { writeComment } from '../../comment/writecomment';
//import { replyComment } from '../../comment/replycomment';
//import { editComment } from '../../comment/editcomment';
//import { loadMoreComments } from '../../comment/loadmorecomments';
//import { getAntiForgeryToken } from '../../utility/antiforgery';
//import { loadmore } from '../../utility/loadmore';

import '../../shared/postfunctions';                                        // [✓]
import '../../shared/readmore';                                             // [✓]
import '../../shared/postui';                                               // [✓]

//import './userroles';
//import './tags';
//import './adminbar';
//import './editicon';

import '../../shared/sharedlast';                                           // [✓]

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [groupName, setgroupName] = useState("");
  const [groupDescription, setgroupDescription] = useState("");
  const [groupId, setGroupId] = useState(-1);
  const [numMembers, setNumMembers] = useState(0);
  const [isLoaded, setIsLoaded] = useState(false);
  const [isGroupAdmin, setIsGroupAdmin] = useState(false);
  const [isGroupMod, setIsGroupMod] = useState(false);
  const [isGroupMember, setIsGroupMember] = useState(false);
  const [imageId, setImageId] = useState(0);

  const { pgroupId } = useParams();

  let query = useQuery();

  useEffect(() => {
    console.log("pgroupId", pgroupId);

    async function initialize() {
      if (pgroupId != null & pgroupId > 0 & !isLoaded) {
        console.log("Viewing: ", pgroupId);
        setGroupId(pgroupId);

        postJson("/api/v1/groups/load/", {
          groupId: pgroupId
        }).then((response) => {
          console.log(response);
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
          }
        });
      }
    }
    initialize();
  }, [pgroupId]); // Fire once

  return (
    <>
      <div className="wrapper border-bottom white-bg page-heading">
        <div className="col-lg-10">
          <br />
          <p className="pull-right">
            <a className="btn btn-sm btn-link" href={"/Group/Members/" + groupId}/*"@Url.Action(" Members", "Group", new {id = Model.GroupId})"*/>
              <i className="fa fa-users"></i> <span id={"group_membercount_" + groupId}>{numMembers}</span> Members
            </a>
            <JoinLeaveButton isMember={isGroupMember} id={groupId} />
          </p>
          <h2>
            <i className=""></i> <span className={isLoaded ? "" : "placeholder col-8 bg-light"}>{groupName}</span> - <span className={isLoaded ? "" : "placeholder col-12 bg-light"}>{groupDescription}</span>
          </h2>
          <ol className="breadcrumb">
            <li className="breadcrumb-item"><a href="/">Home</a></li>
            <li className="breadcrumb-item"><a href="/Group">Groups</a></li>
            <li className="breadcrumb-item active">{groupName}</li>
          </ol>
          <small>Tier x - xxx Satoshi earned</small>
        </div>
        <div className="col-lg-2">
        </div>
      </div>
      {isGroupAdmin ? (<div> <GroupAdminBar id={groupId} /> </div>) : (<div></div>)}
      {isGroupMod ? (<div> <GroupModBar id={groupId} /> </div>) : (<div></div>)}
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

/*

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;
window.loadMoreComments = loadMoreComments;

export function grouploadmore(groupId) {
    console.log("Loading more ", groupId)
    loadmore({
        url: '/Group/InfiniteScroll/',
        blocknumber: window.BlockNumber,
        sort: "Score",
        groupId: groupId
    });
}
window.loadmore = grouploadmore;
window.BlockNumber = 10;  //Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

onLoadedMorePosts();

export function toggleIgnore(id) {
    var data = JSON.stringify({ 'groupId': id });
    var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: "/Group/ToggleIgnore/",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: headers,
        success: function (response) {
            if (response.result === "success") {
                if (response.added) {
                    $("#i_" + id.toString()).html("<i class='fa fa-circle'></i> Un-Ignore ");
                }
                else {
                    $("#i_" + id.toString()).html("<i class='fa fa-ban'></i> Ignore ");
                }
            }
        }
    });
    return false;
}
window.toggleIgnore = toggleIgnore;

var BlockNumber = 10;
var NoMoreData = false;
var inProgress = false;

<!--<div class="wrapper border-bottom white-bg page-heading">
    <div class="col-lg-10">
        <br />
        <p class="pull-right">
            <a class="btn btn-sm btn-link" href="@Url.Action("Members", "Group", new { id = Model.GroupId })"><i class="fa fa-users"></i> <span id="group_membercount_@Model.GroupId">@Model.NumMembers</span> Members</a>
            @if (User.Identity.IsAuthenticated)
            {
                if (Model.IsIgnored)
                {
                    <a href="javascript:void(0);" id="i_@Model.GroupId" onclick="toggleIgnore(@Model.GroupId)" class="btn btn-sm btn-link btn-warning btn-outline"><i class="fa fa-circle"></i> Un-Ignore</a>
                }
                else
                {
                    <a href="javascript:void(0);" id="i_@Model.GroupId" onclick="toggleIgnore(@Model.GroupId)" class="btn btn-sm btn-link btn-warning btn-outline"><i class="fa fa-ban"></i> Ignore</a>
                }
                if (Model.IsMember)
                {
                    <a href="javascript:void(0);" id="j_@Model.GroupId" data-page="detail" onclick="leaveGroup(@Model.GroupId, this)" class="btn btn-primary btn-outline btn-sm"><i class="fa fa-user-times"></i> Leave </a>
                }
                else
                {
                    <a href="javascript:void(0);" id="j_@Model.GroupId" data-page="detail" onclick="joinGroup(@Model.GroupId, this)" class="btn btn-primary btn-outline btn-sm"><i class="fa fa-user-plus"></i> Join </a>
                }
            }
        </p>
        <h2>
            <i class="fa @Model.Icon"></i> @Model.GroupName @if (!string.IsNullOrEmpty(Model.ShortDescription))
            {<text> - </text> @Model.ShortDescription}
        </h2>

        <ol class="breadcrumb">
            <li class="breadcrumb-item"><a href="/">Home</a></li>
            <li class="breadcrumb-item"><a href="/Group">Groups</a></li>
            <li class="breadcrumb-item active">@Model.GroupName</li>
        </ol>

        <small>Tier @Model.Tier - @(Model.TotalEarned + Model.TotalEarnedToDistribute) Satoshi earned</small>
    </div>
    <div class="col-lg-2">
    </div>
</div>

@if (Model.IsGroupAdmin)
{
    Html.RenderAction("GroupAdminBar", controllerName: "Group", routeValues: new { groupId = Model.GroupId });
}

@if (Model.IsGroupMod)
{
    <div class="wrapper wrapper-content ">
        <div class="row ">
            <div class="col-lg-12">
                <div class="ibox float-e-margins collapsed" style="margin-bottom: 0px;">
                    <div class="ibox-title bg-info">
                        <h5>
                            Group Moderation : You have moderation privilages for this group
                        </h5>
                        <div class="ibox-tools">
                            <a class="collapse-link">
                                <i class="fa fa-chevron-up"></i>
                            </a>
                            <a class="close-link">
                                <i class="fa fa-times"></i>
                            </a>
                        </div>
                    </div>
                    <div class="ibox-content">
                        <h2>
                            Group Actions (TODO)
                        </h2>
                        <p>
                            Add user to group admin;
                            <br />
                            Add user to group mod;
                            <br />
                            Add user to group;
                            <br />
                            Delete Group;
                            <br />
                            Ban / Unban users;
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </div>
}

<div class="wrapper wrapper-content ">
    <div class="row">
        <div class="col-sm-2">
            <div class="ibox float-e-margins d-none d-lg-block">
                <div class="ibox-title">
                    <h5>Your Top Groups</h5>
                </div>
                <div class="ibox-content">
                    @foreach (var g in Model.SubscribedGroups)
                    {
                        <button style="white-space:normal !important; word-wrap: break-word; word-break: normal;" onclick="location.href='@Url.Action("GroupDetail", "Group", new { id = g.Id })'" class="btn btn-link btn-block text-left">
                            <i class="fa fa-@g.Icon"></i> @g.Name
                            @if (g.IsAdmin)
                            {
                                <i class="fa fa-gavel text-primary" data-toggle="tooltip" data-placement="right" title="Administrator"></i>
                            }
                            else if (g.IsMod)
                            {
                                <i class="fa fa-gavel text-success" data-toggle="tooltip" data-placement="right" title="Moderator"></i>
                            }
                        </button>
                    }
                </div>
            </div>
        </div>
        <div class="col-lg-8">
            <div class="social-feed-box-nb">
                <span></span>
            </div>
            <div class="social-feed-box-nb">
                <button onclick="location.href='@Url.Action("NewPost", "Post", new { group = Model.GroupId})'" class="btn btn-primary btn-outline btn-block"><i class="fa fa-plus"></i> Add Post</button>
            </div>
            <div id="posts">
                @foreach (var p in Model.Posts)
                {
                    @Html.Partial(partialViewName: "_PartialPostRenderVm", model: p)
                }
            </div>

            @if (Model.HasMorePosts)
            {
                <div class="social-feed-box-nb" id="showmore">
                    <button id="btnLoadmore" class="btn btn-primary btn-block" onclick="loadmore(@Model.GroupId)">
                        <i class="fa fa-arrow-down"></i> Show More <i id="loadmore" class="fa fa-circle-o-notch fa-spin" style="display:none"></i>
                    </button>
                </div>
            }
            <div class="social-feed-box-nb">
                <span></span>
            </div>
            <div class="social-feed-box-nb" style="margin-bottom:50px;">
                <span></span>
            </div>
        </div>
    </div>
</div>

@section Styles {
    @Styles.Render("~/bundles/group/detail/css")
}

@section Scripts {
    <script type="text/javascript">
        var ub = @Model.UserBalance;
        var IsAuthenticated = "@Request.IsAuthenticated" == "True";
        var groupId = @Model.GroupId;
    </script>
    @Scripts.Render("~/bundles/group/detail")
}-->


*/