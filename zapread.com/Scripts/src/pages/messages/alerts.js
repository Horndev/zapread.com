/*
 *
 */

import "../../css/pages/messages/alerts.scss";
import "../../shared/shared";
import "../../realtime/signalr";
import React, { useState, useEffect, useRef } from "react";
import { Row, Col, Button } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../components/page-heading";
import { deleteJson } from "../../utility/deleteData";
import { putJson } from "../../utility/putData";
import { ISOtoRelative } from "../../utility/datetime/posttime"
import "../../shared/sharedlast";

function Page() {
  const [isLoaded, setIsLoaded] = useState(false);
  const [alertsPage, setAlertsPage] = useState(0);
  const [numNewAlerts, setNumNewAlerts] = useState(0);
  const [alerts, setAlerts] = useState([]);

  const alertsLoadingRef = useRef();

  async function getAlerts() {
    await fetch(`/api/v1/alerts/get/${alertsPage}/read/`)
      .then(response => response.json())
      .then(json => {
        setAlerts(json.alerts);
        setNumNewAlerts(json.numAlerts);
        alertsLoadingRef.current.classList.remove("sk-loading");
      });
  }

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
        await Promise.all([getAlerts()]);
      }
    }
    initialize();
  }, []); // Fire once

  async function deleteAlert(id) {
    await deleteJson(`/api/v1/alerts/user/${id}`, {})
      .then(response => {
        if (response.success) {
          getAlerts();
        }
      });
  }

  async function dismissAlert(id) {
    await putJson(`/api/v1/alerts/user/mark-read/${id}`, {})
      .then(response => {
        if (response.success) {
          getAlerts();
        }
      });
  }

  return (
    <>
      <PageHeading
        title="Alerts"
        controller="Home"
        method="Messages"
        function="Alerts"
      />
      <Row>
        <Col lg={2}></Col>
        <Col lg={6}>
          <div className="ibox float-e-margins">
            <div className="ibox-title">
              <div className="ibox-content ibox-heading">
                <h3><i className="fa fa-bell"></i> New alerts</h3>
                <small>
                  <i className="fa fa-tim"></i>&nbsp;You have {numNewAlerts} new alerts.
                </small>
                <span className="pull-right">&nbsp;
                  <Button variant="danger" size="sm" onClick={() => { deleteAlert(-1); }}>
                    Delete all&nbsp;<i className="fa fa-times-circle"></i>
                  </Button>
                </span>
              </div>
            </div>
            <div className="ibox-content">
              <div className="feed-activity-list" style={{ minHeight: "30px" }}>
                <div ref={alertsLoadingRef} className="ibox-content no-padding sk-loading" style={{ borderStyle: "none" }}>
                  <div className="sk-spinner sk-spinner-three-bounce">
                    <div className="sk-bounce1"></div>
                    <div className="sk-bounce2"></div>
                    <div className="sk-bounce3"></div>
                  </div>
                </div>
                <div className="items-list">
                  <table className="table table-hover">
                    <tbody>
                      {alerts.map((alert, index) => (
                        <tr key={alert.Id} className={alert.IsRead ? "" : "alert-row-unread"}>
                          <td className="project-status" onClick={() => { dismissAlert(alert.Id); }}>
                            {alert.IsRead ?
                              <>
                                <span className="badge badge-primary">Read</span>
                              </> :
                              <>
                                <span className="badge badge-info">Unread</span>
                              </>}
                          </td>
                          <td className="project-people">
                            {alert.FromUserAppId != "" ?
                              <>
                                <img alt="image" className="rounded-circle" loading="lazy" width="32" height="32"
                                  src={"/Home/UserImage/?size=32&UserId=" + encodeURIComponent(alert.FromUserAppId) + "&v=" + alert.FromUserProfileImageVersion}/>
                              </> : <></>}
                          </td>
                          {/*<td>*/}
                          {/*  <div dangerouslySetInnerHTML={{ __html: alert.Content }} />*/}
                          {/*</td>*/}
                          <td className="project-title">
                            <small className="postTime text-muted">{ ISOtoRelative(alert.TimeStamp) }</small>
                            <br />
                            <a href={alert.CommentId > 0 ? ("/Post/Detail/" + alert.PostId + "/#cid_" + alert.CommentId) : alert.PostId > 0 ? "/Post/Detail/" + alert.PostId : "#"}><div dangerouslySetInnerHTML={{ __html: alert.Title }} /></a>
                          </td>
                          <td className="project-actions">
                            <a className="btn btn-sm btn-outline-warning" onClick={() => { deleteAlert(alert.Id); }}>
                              <i className="fa fa-minus-circle"></i>{" "}Dismiss
                            </a>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                {/*<br /><br />OLD<br /><br />*/}
                {/*{alerts.map((alert, index) => (*/}
                {/*  <div*/}
                {/*    key={alert.Id}*/}
                {/*    id={"a_" + alert.Id}*/}
                {/*    className="feed-element">*/}
                {/*    <div>*/}
                {/*      <span className="pull-right">&nbsp;*/}
                {/*        <a className="btn btn-sm btn-danger" onClick={() => { deleteAlert(alert.Id); }}>*/}
                {/*          Delete&nbsp;<i className="fa fa-times-circle"></i>*/}
                {/*        </a>*/}
                {/*      </span>*/}
                {/*      {alert.IsRead ?*/}
                {/*        <>*/}
                {/*          <span className="badge badge-primary">Read</span>*/}
                {/*        </> :*/}
                {/*        <>*/}
                {/*          <span className="badge badge-info">Unread</span>*/}
                {/*        </>}*/}
                {/*      <strong>*/}
                {/*        <div dangerouslySetInnerHTML={{ __html: alert.Title }} />*/}
                {/*      </strong>*/}
                {/*      <div>*/}
                {/*        {alert.PostId > 0 ? (<>*/}
                {/*          <span>Post:</span>*/}
                {/*          <a href={"/Post/Detail/" + alert.PostId}>*/}
                {/*            {alert.PostTitle != "" ? (<>{alert.PostTitle}</>) : (<>Link</>)}*/}
                {/*          </a>*/}
                {/*        </>) : (<></>)}*/}
                {/*      </div>*/}
                {/*      <div dangerouslySetInnerHTML={{ __html: alert.Content }} />*/}
                {/*      <small className="postTime text-muted">{ ISOtoRelative(alert.TimeStamp) }</small>*/}
                {/*    </div>*/}
                {/*  </div>*/}
                {/*))}*/}
              </div>
            </div>
          </div>
        </Col>
        <Col lg={2}></Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));

//import $ from 'jquery';
//import '../../shared/shared';
//import '../../realtime/signalr';
//import 'datatables.net-bs4';
//import 'datatables.net-scroller-bs4';
//import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
//import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';

//import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';
//import { getAntiForgeryToken } from '../../utility/antiforgery';
//import '../../shared/sharedlast';

///* exported alertsTable */
//var alertsTable = {};

//$(document).ready(function () {
//    // Table
//    alertsTable = $('#alertsTable').DataTable({
//        "searching": false,
//        "bInfo": false,
//        "lengthChange": false,
//        "ordering": true,
//        "order": [[1, "desc"]],
//        "pageLength": 25,
//        "processing": true,
//        "serverSide": true,
//        "ajax": {
//            type: "POST",
//            contentType: "application/json",
//            url: "/Messages/GetAlertsTable/",
//            data: function (d) {
//                return JSON.stringify(d);
//            }
//        },
//        "columns": [
//            {
//                "data": null,
//                "name": 'Status',
//                "orderable": true,
//                "mRender": function (data, type, row) {
//                    if (data.Status === "Read") {
//                        return "<span class='badge badge-primary' >" + data.Status + "</span>";
//                    }
//                    return "";
//                }
//            },
//            {
//                "data": "Date",
//                "orderable": true,
//                "name": "Date",
//                "type": "date",
//                "orderSequence": ["desc", "asc"],
//                "mRender": function (data, type, row) {
//                    var datefn = parseISO(data);
//                    datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
//                    var date = format(datefn, "dd MMM yyyy");
//                    var time = formatDistanceToNow(datefn, { addSuffix: true });
//                    return date + ", " + time;
//                }
//            },
//            { "data": "Title", "orderable": false, "name": "Title" },
//            {
//                "data": null,
//                "name": 'Link',
//                "orderable": false,
//                "mRender": function (data, type, row) {
//                    var linkText = "Go to Post";
//                    if (data.HasLink) {
//                        linkText = "Go to Post";
//                    }
//                    else if (data.HasCommentLink) {
//                        linkText = "Go to Comment";
//                    }
//                    else {
//                        return "";
//                    }
//                    return "<a href='/Post/Detail/" + data.Link + "#cid_" + data.Anchor + "'>" + linkText + "</a>";
//                }
//            },
//            {
//                "data": null,
//                "name": 'Action',
//                "orderable": false,
//                "mRender": function (data, type, row) {
//                    return "<button class='btn btn-danger btn-outline btn-sm' id='a_" + data.AlertId + "' onclick='deletea(" + data.AlertId + ");'>Delete</button>";
//                }
//            }
//        ]
//    });
//}); // end ready

///*
// * Delete an alert
// * @param {any} id : alert identifier
// */
//export function deletea(id) {
//    var url = "/Messages/DeleteAlert";

//    $.ajax({
//        async: true,
//        type: "POST",
//        url: url,
//        data: JSON.stringify({ "id": id }),
//        dataType: "json",
//        contentType: "application/json; charset=utf-8",
//        success: function (result) {
//            if (result.Result === "Success") {
//                $('#a_' + id).parent().parent().hide();
//            }
//        },
//        error: function (jqXHR, textStatus, errorThrown) {
//            alert("fail");
//        }
//    });
//    return false;
//}
//window.deletea = deletea;