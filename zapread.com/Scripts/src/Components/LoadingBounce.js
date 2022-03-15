/*
 * Displays the User Balance in the top navbar
*/

import React, { useCallback, useEffect, useState, createRef } from "react";

export default function LoadingBounce(props) {

  return (
    <div className="ibox-content no-padding sk-loading" style={{ borderStyle: "none" }}>
      <div className="sk-spinner sk-spinner-three-bounce">
        <div className="sk-bounce1"></div>
        <div className="sk-bounce2"></div>
        <div className="sk-bounce3"></div>
      </div>
    </div>
  )
}