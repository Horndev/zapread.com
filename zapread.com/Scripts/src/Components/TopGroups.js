/*
 * React component for the top header and breadcrumb
 */
import React, { useEffect, useState } from 'react';
import Tippy from '@tippyjs/react';
import { Button } from "react-bootstrap";
import { getJson } from "../utility/getData";
import 'tippy.js/dist/tippy.css';

export default function TopGroups(props) {
  const [groups, setGroups] = useState([]);

  useEffect(() => {
    getJson('/api/v1/groups/top/list').then((response) => {
      if (response.success) {
        setGroups(response.Groups);
      } else {
        // Did not work
      }
    });
  }, []); // Fire once

  return (
    <>
      <div id="topGroups" className="ibox float-e-margins d-none d-lg-block">
        <div id="group-box"
          className="ibox-content"
          style={props.expanded ? {} : { display: "none" }}>
          {groups.map((g, index) => (
            <Button
              key={g.Id}
              variant="link"
              block={true}
              onClick={() => { window.location = "/Group/Detail/" + g.Id + "/" }}
              className="text-left top-groups-btn" >
              <i className={"fa fa-" + g.Icon}></i> {g.Name}
              {g.IsAdmin ? (
                <Tippy content={"Administrator"}>
                  <i className="fa fa-gavel text-primary"></i>
                </Tippy>
              ) : (<></>)}
              {g.IsMod ? (
                <Tippy content={"Moderator"}>
                  <i className="fa fa-gavel text-success"></i>
                </Tippy>
              ): (<></>)}
            </Button>
            ))}
        </div>
      </div>
    </>
  );
}