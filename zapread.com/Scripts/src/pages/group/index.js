/**
 * Groups/Index
 *
 * [✓] No javascript
 */

import "../../shared/shared";
import "../../realtime/signalr";

import React, { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";
import ReactDOM from "react-dom";
import PageHeading from "../../Components/PageHeading";
import GroupsTable from "./Components/GroupsTable";
import TopGroups from "../../Components/TopGroups";
import { useUserInfo } from "../../Components/hooks/useUserInfo";
import { updateUserInfo } from '../../utility/userInfo';
import "../../shared/sharedlast";

function Page() {
  const userInfo = useUserInfo(); // Custom hook
  const [topGroupsIsExpanded, setTopGroupsIsExpanded] = useState(true); // Start open on groups page

  useEffect(() => {
    updateUserInfo({
      isAuthenticated: window.IsAuthenticated
    });
    //console.log("IsAuthenticated", window.IsAuthenticated)
  }, []); // Fire once

  return (
    <>
      <PageHeading
        title="Groups"
        controller="Home"
        method="Groups"
        function="List"
        topGroups={window.IsAuthenticated}
        topGroupsExpanded={true}
        onTopGroupsClosed={() => { setTopGroupsIsExpanded(false) }}
        onTopGroupsOpened={() => { setTopGroupsIsExpanded(true) }}
      />
      <Row>
        <Col lg={12}>
          <div className="wrapper wrapper-content animated fadeInUp">
            <Row>
              <Col lg={2}>
                {window.IsAuthenticated ? (
                  <TopGroups expanded={topGroupsIsExpanded} />
                ): (<></>)}
              </Col>
              <Col lg={8}>
                <div className="ibox">
                  <div className="ibox-title">
                    <a href="/Group/New" className="btn btn-primary btn-xs btn-block">
                      Create new group
                    </a>
                  </div>
                  <div className="ibox-content">
                    <div className="project-list">
                      <GroupsTable pageSize={20} />
                    </div>
                  </div>
                </div>
              </Col>
              <Col lg={2}></Col>
            </Row>
          </div>
        </Col>
      </Row>
    </>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));
