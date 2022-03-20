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
      //console.log("response", response);
      if (response.success) {
        handleAdminExpand();
        handleModExpand();
        handleBanishedExpand();
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
          setBanishedUsers([{ UserAppId: "0", UserName: "*** No users banished ***"}]);
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

  const onSetDescription = () => {
    if (props.tier < 1) {
      getSwal().then(({ default: Swal }) => {
        Swal.fire("Error", "This function requires a group to be at least tier 1", "error");
      });
    } else {
      getSwal().then(({ default: Swal }) => {
        Swal.fire({
          title: "Set group description",
          showCancelButton: true,
          html: '<label>Set your group short description:</label><input type="text" id="description" class="swal2-input">',
          focusConfirm: false,
          preConfirm: () => {
            const description = Swal.getPopup().querySelector('#description').value
            if (!description) {
              Swal.showValidationMessage(`Please enter description`)
            }
            return { description: description }
          }
        }).then((result) => {
          postJson('/api/v1/groups/admin/setdescription', {
            description: result.value.description,
            groupId: groupId
          }).then(response => {
            if (response.success) {
              Swal.fire("Success", "Description updated", "success");
              props.onUpdateDescription(result.value.description);
            }
            else {
              Swal.fire("Error", `${response.message}`, "error");
            }
          });
        })
      });
    }
  }

  return (
    <>
      <CollapseBar
        isDisabled={false}
        title={"Group Administration : You have administration privilages for this group"}
        bg={"bg-warning"}
        isCollapsed={true}>
        <h2>
          Group Actions
        </h2>
        <a className="btn btn-link btn-sm" href={"/Group/Edit?groupId=" + groupId} ><i className="fa-solid fa-edit"></i> Edit Group <i className="fa-solid fa-up-right-from-square"></i></a>
        <br/>
        <button className="btn btn-link btn-sm" onClick={onSetDescription}><i className="fa-solid fa-pen"></i> [Tier 1] Set Description</button>
        <br />
        <button className="btn btn-link btn-sm" onClick={() => { alert('not yet implemented') }}><i className="fa-solid fa-pen"></i> [Tier 2] Set Static Background Image</button>
        <br />
        <button className="btn btn-link btn-sm" onClick={() => { alert('not yet implemented') }}><i className="fa-solid fa-pen"></i> [Tier 3] Set Animated Background Image</button>
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
                  <Dropdown.Item onClick={() => revoke("admin")}>Admin</Dropdown.Item>
                  <Dropdown.Item onClick={() => revoke("mod")}>Moderation</Dropdown.Item>
                  <Dropdown.Item onClick={() => revoke("membership")}>Membership</Dropdown.Item>
                </Dropdown.Menu>
              </Dropdown>

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
