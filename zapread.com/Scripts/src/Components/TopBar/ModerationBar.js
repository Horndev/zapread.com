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

import React, { useState } from 'react';
import CollapseBar from "../CollapseBar";
import { getJson } from '../../utility/getData';

export default function ModerationBar(props) {
  const [numGroups, setNumGroups] = useState(0);

  useEffect(() => {
    //getJson("/api/v1/user/banneralerts/")
    //  .then((response) => {
    //    if (response.success) {
    //    }
    //  }).catch((error) => {
    //    console.log(error);
    //  });
  }, []);

  return (
    <>
      <CollapseBar
        isVisible={numGroups > 0}
        isDisabled={false}
        title={"Moderator Tools: You moderate " + numGroups + " group" + numGroups > 1 ? "s" : ""}
        bg={"bg-info"}
        isCollapsed={true}>
        <h2>
          Group Moderation
        </h2>
      </CollapseBar>
    </>
  );
}