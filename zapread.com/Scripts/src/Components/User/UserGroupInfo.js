/*
 * 
 */

import React, { Suspense, useState, useEffect } from 'react';
import { Container, Row, Col, Button, Dropdown, Form } from "react-bootstrap";
import Swal from 'sweetalert2';
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";
import UserSetting from "./UserSetting";
const UpdateEmailModal = React.lazy(() => import("./UpdateEmailModal"));

function GroupRow(props) {

  return (
    <>
      <tr className='clickable-row'
        onClick={() => {
          var url = "/Group/Detail/" + props.id;
          window.location = url;
        }}>
        <td>
          <a className="btn-sm btn-link" href={"/Group/Detail/" + props.id}>
            { props.name }
          </a></td>
        <td>{props.numPosts} / {props.userPosts}</td>
        <td>
          {props.isAdmin ? (<>Admin{" "}</>) : (<></>)}
          {props.isMod ? (<>Moderator{" "}</>) : (<></>)}
        </td>
        {/*<td>*/}
        {/*  <a href="javascript:void(0);" id="j_@g.Id" onclick="leaveGroup(@g.Id)" class="btn btn-primary btn-outline btn-sm"><i class="fa fa-user-times"></i> Leave </a>*/}
        {/*</td>*/}
      </tr>
    </>);
}

export default function UserGroupInfo(props) {
  const [groups, setGroups] = useState([]);

  useEffect(() => {
    async function initialize() {
      await fetch("/api/v1/user/groupinfo").then(response => {
        return response.json();
      }).then(data => {
        setGroups(data.Groups);
      });
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <div className="ibox float-e-margins">
        <div className="ibox-content profile-content">
          <h4>Groups</h4>
          <Container>
            <table className="table table-hover">
              <thead>
                <tr>
                  <th scope="col">Group</th>
                  <th scope="col">Posts/Yours</th>
                  <th scope="col">Roles</th>
                  {/*<th scope="col">Actions</th>*/}
                </tr>
              </thead>
              <tbody>
                {groups.map((g) => (
                  <GroupRow key={g.Id} name={g.Name} id={g.Id} isMod={g.IsMod} isAdmin={g.IsAdmin} numPosts={g.NumPosts} userPosts={g.UserPosts} />
                ))}
              </tbody>
            </table>
          </Container>
        </div>
      </div>
    </>
  );
}