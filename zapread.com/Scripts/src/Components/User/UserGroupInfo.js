/*
 * 
 */

import React, { useState, useEffect } from 'react';

function GroupRow(props) {
  return (
    <>
      <tr className='clickable-row'
        onClick={() => {
          var url = "/Group/Detail/" + props.id;
          window.location = url;
        }}>
        <td className="zr-tdw">
          <a className="btn-sm btn-link" href={"/Group/Detail/" + props.id}>
            { props.name }
          </a></td>
        {props.includeCounts ? (<><td>{props.numPosts} / {props.userPosts}</td></>) : (<></>)}
        <td>
          {props.isAdmin ? (<>Admin{" "}</>) : (<></>)}
          {props.isMod ? (<>Moderator{" "}</>) : (<></>)}
        </td>
      </tr>
    </>);
}

export default function UserGroupInfo(props) {
  const [groups, setGroups] = useState([]);
  useEffect(() => {
    async function initialize() {
      var url = "/api/v1/user/groupinfo";
      if (props.userAppId) { url = url + "/" + props.userAppId + "/"; }
      await fetch(url).then(response => {
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
          <table className="table table-hover group-table">
            <thead>
              <tr>
                <th>Group</th>
                {props.isOwnProfile ? (<><th>Posts/Yours</th></>) : (<></>)}
                <th>Roles</th>
              </tr>
            </thead>
            <tbody>
              {groups.map((g) => (
                <GroupRow key={g.Id} includeCounts={props.isOwnProfile} name={g.Name} id={g.Id} isMod={g.IsMod} isAdmin={g.IsAdmin} numPosts={g.NumPosts} userPosts={g.UserPosts} />
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  );
}