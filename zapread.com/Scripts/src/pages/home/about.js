/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';

import React, { useEffect, useState, useRef } from "react";
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
  const [show, setShow] = useState(true);
  const [bgopac, setBgopac] = useState(0);

  useEffect(() => {
    new WOW.WOW({
      animateClass: 'animate__animated',//'animated',
      live: false,
      offset: 20,
    }).init();

    // Initialize fading background
    //var firstel = document.querySelector("section").offsetTop;
    //window.onscroll = function () {
    //  if (window.pageYOffset > 0) {
    //    var opac = window.pageYOffset / firstel;
    //    var containerEl = document.getElementById("about");
    //    //console.log(containerEl);
    //    var bg = window.getComputedStyle(containerEl, null).getPropertyValue('background');
    //    //console.log(bg);
    //    const regex = /(?:url\((\S+)\))/g;
    //    var imgurl = bg.match(regex)[0].replace("url(", "").replace(")", "");
    //    //console.log(imgurl);
    //    containerEl.style.background = "linear-gradient(rgba(255, 255, 255, " + opac + "), rgba(255, 255, 255, " + opac + ")), url(" + imgurl + ") no-repeat";
    //  }
    //}
  }, []); // Fire once

return (
  <div id="about" className="content-container" style={show ? {} : { display: 'none' }}>
    <Mission />
    <Principals />
    <HowItWorks />
    <Stats />
    <MoreInfo />
  </div>
);
}

ReactDOM.render(<Page />, document.getElementById("root"));