/*
 * Moderation bar, shown on main page under Nav Bar for group mods
 * 
 * -> List of groups where moderation privilages are available
 * -> Statistics / views:
 *    -> New posts in group since last visit
 *    -> New comments in group since last visit
 *    -> Group moderation funds available
 *    -> New Spam reports
 */

import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Card, Table } from "react-bootstrap";
import CollapseBar from "../CollapseBar";
import { getJson } from '../../utility/getData';
import { postJson } from '../../utility/postData';
const getSwal = () => import('sweetalert2');

export default function ModerationBar(props) {
  const [numGroups, setNumGroups] = useState(0);
  const [numReports, setNumReports] = useState(0);
  const [reports, setReports] = useState([]);
  const [groupFundsInfo, setGroupFundsInfo] = useState([]);
  const [isLoaded, setIsLoaded] = useState(false);

  useEffect(() => {
    getJson("/api/v1/groups/mod/reports")
      .then((response) => {
        if (response.success) {
          //console.log(response);
          setNumGroups(response.NumGroupsModerated);
          setNumReports(response.NumReports);
          setGroupFundsInfo(response.BalanceInfo);
        }
      }).catch((error) => {
        console.log(error);
      });
  }, []);

  const handleExpand = () => {
    if (!isLoaded) {
      getJson("/api/v1/groups/mod/reports/unresolved")
        .then((response) => {
          if (response.success) {
            //console.log(response);
            setReports(response.Reports);
            setIsLoaded(true);
          }
        }).catch((error) => {
          console.log(error);
        });
    }
  };

  const reportTypeString = (t) => {
    if (t == 0) return "Other";
    if (t == 1) return "Spam";
    if (t == 2) return "NSFW";
  }

  const closeReport = (reportId) => {
    postJson("/api/v1/groups/mod/reports/resolve/", { ReportId: reportId })
      .then((result) => {
        if (result.success) {
          setReports(reports.filter(r => r.ReportId != reportId));
          getSwal().then(({ default: Swal }) => {
            Swal.fire("Successfully closed mod report.", {
              icon: "success"
            });
          });
        }
      });
  }

  const markNSFW = (postId, reportId) => {
    postJson("/api/v1/post/markNSFW/", { PostId: postId })
      .then((result) => {
        if (result.success) {

          postJson("/api/v1/groups/mod/reports/resolve/", { ReportId: reportId })
            .then((result) => {
              if (result.success) {
                setReports(reports.filter(r => r.ReportId != reportId));
                getSwal().then(({ default: Swal }) => {
                  Swal.fire("Successfully marked post NSFW.", {
                    icon: "success"
                  });
                });
              }
            });
        }
      });
  }

  return (
    <>
      {numGroups > 0 ? (<CollapseBar
        isVisible={numGroups > 0}
        isDisabled={false}
        title={
          <>
            Moderator Tools: You moderate {numGroups} group{numGroups > 1 ? "s" : ""} and there {(numReports > 1 || numReports == 0) ? "are" : "is"} {numReports} unresolved user report{(numReports > 1 || numReports == 0) ? "s" : ""}
          </>}
        bg={"bg-info"}
        onExpand={handleExpand}
        isCollapsed={true}>
        <h2>
          Group Moderation
        </h2>
        <p>There are {groupFundsInfo.reduce((a, o) => { return a + o.Balance }, 0)} Satoshi in your group moderation funds.</p>
        <Table responsive striped bordered hover size="sm">
          <thead>
            <tr>
              <th>Report Type</th>
              <th>Content</th>
              <th>Reported By</th>
              <th>Group</th>
              <th>Date</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {reports.map((r, i) => (
              <tr key={r.ReportId}>
                <td>
                  {reportTypeString(r.ReportType)}
                </td>
                <td>
                  <a className="btn btn-sm btn-primary-outline"
                    onClick={() => { window.open("/Post/Detail/" + r.PostId, '_blank').focus(); }}
                    role="button">link <i className="fa-solid fa-arrow-up-right-from-square"></i></a>
                </td>
                <td>
                  {r.ReportedByName}
                </td>
                <td>
                  {r.GroupName}
                </td>
                <td>
                  {r.TimeStamp}
                </td>
                <td>
                  {r.ReportType == 2 ? (
                    <>
                      <Button size="sm" onClick={() => { markNSFW(r.PostId, r.ReportId) }}>Mark NSFW</Button>{" "}
                    </>) : (<></>)}
                  <Button onClick={() => alert("Not yet implemented.")} size="sm">Downvote (Mod funds)</Button>{" "}
                  <Button onClick={() => closeReport(r.ReportId) }size="sm">Close Report</Button>
                </td>
              </tr>
            ))}
          </tbody>
        </Table>
      </CollapseBar>) : (<></>)}
    </>
  );
}