/*
 * Admin bar for a group view
 * 
 **/

import React, { useCallback, useEffect, useState } from "react";
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
  const [banishedUsers, setBanishedUsers] = useState([]);

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
      if (response.success) {
        handleAdminExpand();
        handleModExpand();
        handleBanishedExpand();
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Success", "User " + userName + " applied", "success");
        });
      } else {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "Error: " + response.message, "error");
        });
      }
    });
  };

  const revoke = (grantType) => {
    setIsLoading(true);
    postJson("/api/v1/groups/admin/revoke/" + grantType, {
      UserAppId: userAppId,
      GroupId: groupId
    }).then((response) => {
      setIsLoading(false);
      if (response.success) {
        handleAdminExpand();
        handleModExpand();
        handleBanishedExpand();
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Success", "User " + userName + " revoked", "success");
        });
      } else {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "Error: " + response.message, "error");
        });
      }
    });
  };

  const handleBanishedExpand = () => {
    postJson("/api/v1/groups/list/banished", {
      GroupId: groupId
    }).then((response) => {
      if (response.success) {
        if (response.Users.length) {
          setBanishedUsers(response.Users);
        } else {
          setBanishedUsers([{ UserAppId: "0", UserName: "*** No users banished ***" }]);
        }
      } else {
        getSwal().then(({ default: Swal }) => {
          Swal.fire("Error", "Error: " + response.message, "error");
        });
      }
    });
  }

  const handleAdminExpand = () => {
    postJson("/api/v1/groups/list/admin", {
      GroupId: groupId
    }).then((response) => {
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
        isDisabled={false}
        title={"Group Moderation : You have moderation privilages for this group"}
        bg={"bg-info"}
        isCollapsed={true}>
        <h2>
          Group Actions (will be implemented here)
        </h2>
        <Row>
          <Col lg={6}>
            <UserAutosuggest label="User Name" onSelected={onSelected} url="/api/v1/user/search" />
          </Col>
          <Col>
          </Col>
        </Row>
        <Row>
          <Col>
            <div style={{ margin: "10px" }}>
              <Dropdown>
                <Dropdown.Toggle variant="danger" id="dropdown-basic">
                  <i className="fa-solid fa-ban"></i> Banish
                </Dropdown.Toggle>
                <Dropdown.Menu>
                  <Dropdown.Item onClick={() => revoke("banish")}>Un-Banish</Dropdown.Item>
                  <Dropdown.Item onClick={() => grant("banish")}>Banish 1 Month</Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>
            </div>
          </Col>
        </Row>
        <Row>
          <Col lg={4}>
            <CollapseBar
              isDisabled={false}
              showClose={false}
              onExpand={handleAdminExpand}
              title={"Administrators"}
              bg={"gray-bg"}
              isCollapsed={false}>
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
              showClose={false}
              onExpand={handleModExpand}
              title={"Moderators"}
              bg={"gray-bg"}
              isCollapsed={false}>
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
              showClose={false}
              onExpand={handleBanishedExpand}
              title={"Banished"}
              bg={"gray-bg"}
              isCollapsed={false}>
              <div style={{ height: "120px", maxHeight: "120px", overflow: "scroll", overflowX: "hidden" }}>
                {banishedUsers.map((user, index) => (
                  <div key={user.UserAppId}>{user.UserName}</div>
                ))}
              </div>
            </CollapseBar>
          </Col>
        </Row>
      </CollapseBar>
    </>
  );
}
