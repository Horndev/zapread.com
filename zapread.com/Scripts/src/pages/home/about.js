/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';

import React, { useEffect, useState } from "react";
import ReactDOM from "react-dom";
import WOW from 'wowjs';

import Mission from "./Components/Sections/Mission";
import Principals from "./Components/Sections/Principals";
import HowItWorks from "./Components/Sections/HowItWorks";
import Stats from "./Components/Sections/Stats";
import MoreInfo from "./Components/Sections/MoreInfo";

import "../../css/pages/about/about.scss";
import 'animate.css/animate.css'; // needed for wow
import '../../shared/sharedlast';

function Page() {

  useEffect(() => {
    new WOW.WOW({
      animateClass: 'animate__animated',//'animated',
      live: false
    }).init();
  }, []); // Fire once

  return (
    <div className="content-container">
      <Mission />
      <Principals />
      <HowItWorks />
      <Stats />
      <MoreInfo />
    </div>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));