/*
 * Admin bar for a group view
 * 
 **/

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, DropdownButton, Dropdown, ButtonGroup, Button } from "react-bootstrap";
import { postJson } from '../../../utility/postData';
import UserAutosuggest from "../../../Components/UserAutosuggest";
import CollapseBar from "../../../Components/CollapseBar";
const getSwal = () => import('sweetalert2');

export default function GroupAdminBar(props) {
  const [groupId, setGroupId] = useState(props.id);
  const [userAppId, setUserAppId] = useState("");
  const [userName, setUserName] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [adminUsers, setAdminUsers] = useState([]);
  const [modUsers, setModUsers] = useState([]);

  // Monitor for changes in props
  useEffect(
    () => {
      setGroupId(props.id);
    },
    [props.id]
  );

  function onSelected(values) {
    console.log("selected", values);
    setUserAppId(values.UserAppId);
    setUserName(values.userName);
  }

  const grant = (grantType) => {
    setIsLoading(true);
    postJson("/api/v1/groups/admin/grant/" + grantType, {
      UserAppId: userAppId,
      GroupId: groupId
    }).then((response) => {
      setIsLoading(false);
      //console.log("response", response);
      if (response.success) {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Success", "User " + userName + " granted", "success");
        });
      } else {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "Error: " + response.message, "error");
        });
      }
    });
  };

  const handleAdminExpand = () => {
    postJson("/api/v1/groups/list/admin", {
      GroupId: groupId
    }).then((response) => {
      //console.log("list/admin response", response);
      if (response.success) {
        setAdminUsers(response.Users);
      } else {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "Error: " + response.message, "error");
        });
      }
    });
  };

  const handleModExpand = () => {
    postJson("/api/v1/groups/list/mod", {
      GroupId: groupId
    }).then((response) => {
      //console.log("list/mod response", response);
      if (response.success) {
        setModUsers(response.Users);
      } else {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "Error: " + response.message, "error");
        });
      }
    });
  };

  return (
    <>
      <CollapseBar
        isDisabled={isLoading}
        title={"Group Administration : You have administration privilages for this group"}
        bg={"bg-warning"}
        isCollapsed={true}>
        <h2>
          Group Actions
        </h2>
        <a className="btn btn-link btn-sm" href={"/Group/Edit?groupId=" + groupId} ><i className="fa-solid fa-edit"></i> Edit Group</a>
        <h2>
          User Administration
        </h2>
        <Row>
          <Col>
            <UserAutosuggest label="User Name" onSelected={onSelected} url="/api/v1/user/search" />
          </Col>
          <Col>
            <div style={{ display: "flex", justifyContent: "space-evenly", alignItems: "center", height: "60px" }}>
              <Dropdown>
                <Dropdown.Toggle variant="success" id="dropdown-basic">
                  <i className="fa-solid fa-user-check"></i> Grant
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item onClick={() => grant("admin")}>Admin</Dropdown.Item>
                  <Dropdown.Item onClick={() => grant("mod")}>Moderation</Dropdown.Item>
                  <Dropdown.Item onClick={() => grant("membership")}>Membership</Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>

              <Dropdown>
                <Dropdown.Toggle variant="warning" id="dropdown-basic">
                  <i className="fa-solid fa-user-xmark"></i> Revoke
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item href="#/action-1">Admin</Dropdown.Item>
                  <Dropdown.Item href="#/action-2">Moderation</Dropdown.Item>
                  <Dropdown.Item href="#/action-3">Membership</Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>

              <Dropdown>
                <Dropdown.Toggle variant="danger" id="dropdown-basic">
                  <i className="fa-solid fa-ban"></i> Ban
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item href="#/action-1" disabled>Permanent</Dropdown.Item>
                  <Dropdown.Item href="#/action-2" disabled>Silent</Dropdown.Item>
                  <Dropdown.Item href="#/action-3" disabled>Reputation</Dropdown.Item>
                  <Dropdown.Item href="#/action-4" disabled>Day</Dropdown.Item>
                  <Dropdown.Item href="#/action-4" disabled>Week</Dropdown.Item>
                  <Dropdown.Item href="#/action-4" disabled>Month</Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            </div>
          </Col>
        </Row>
        <Row>
          <Col lg={4}>
            <CollapseBar
              isDisabled={false}
              onExpand={handleAdminExpand}
              title={"Administrators"}
              bg={"gray-bg"}
              isCollapsed={true}>
              <div style={{ height: "120px", maxHeight: "120px", overflow: "scroll", overflowX: "hidden" }}>
                {adminUsers.map((user, index) => (
                  <div key={user.UserAppId}>{user.UserName}</div>
                ))}
              </div>
            </CollapseBar>
          </Col>
          <Col lg={4}>
            <CollapseBar
              isDisabled={false}
              onExpand={handleModExpand}
              title={"Moderators"}
              bg={"gray-bg"}
              isCollapsed={true}>
              <div style={{ height: "120px", maxHeight: "120px", overflow: "scroll", overflowX: "hidden" }}>
                {modUsers.map((user, index) => (
                  <div key={user.UserAppId}>{user.UserName}</div>
                ))}
              </div>
            </CollapseBar>
          </Col>
          <Col lg={4}>
            <CollapseBar
              isDisabled={false}
              title={"Banished"}
              bg={"gray-bg"}
              isCollapsed={true}>
              <div>List of Banished - not yet implemented</div>
              <div>Banishment is temporary, depending on reputation and group</div>
            </CollapseBar>
          </Col>
        </Row>
      </CollapseBar>
    </>
  );
}
