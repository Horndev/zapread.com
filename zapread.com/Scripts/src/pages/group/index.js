/**
 * Groups/Index
 *
 * [✓] No javascript
 */

import "../../shared/shared";
import "../../realtime/signalr";

import React from "react";
import { Row, Col } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../components/page-heading";
import GroupsTable from "./Components/GroupsTable";

import "../../shared/sharedlast";

function Page() {
  return (
    <>
      <PageHeading
        title="Groups"
        controller="Home"
        method="Groups"
        function="List"
      />
      <Row>
        <Col lg={12}>
          <div className="wrapper wrapper-content animated fadeInUp">
            <div className="ibox">
              <div className="ibox-title">
                <div className="pull-right forum-desc" />
                <h3>Top Groups</h3>
                <div className="ibox-tools">
                  <a href="/Group/New" className="btn btn-primary btn-xs">
                    Create new group
                  </a>
                </div>
              </div>
              <div className="ibox-content">
                <div className="project-list">
                  <GroupsTable pageSize={20} />
                </div>
              </div>
            </div>
          </div>
        </Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));
