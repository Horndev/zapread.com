/*
 * 
 * 
 **/

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

export default function CollapseBar(props) {
  const [title, setTitle] = useState(props.title);
  const [bg, setBg] = useState(props.bg);

  const collapseRef = createRef();
  const closeRef = createRef();

  // Monitor for changes in props
  useEffect(
    () => {
      setTitle(props.title);
      setBg(props.bg);
    },
    [props.title, props.title]
  );

  // This is the initialization of the component
  useEffect(() => {
    var el = collapseRef.current;
    function toggle(e) {
      var ibox = el.closest('div.ibox');
      if (el.getAttribute('data-id') !== null) {
        ibox = document.getElementById(el.getAttribute('data-id'));
      }
      var button = el.querySelectorAll('i').item(0);
      var content = ibox.querySelectorAll('.ibox-content').item(0);
      if (content.style.display !== 'block') {
        content.style.display = 'block';
      } else {
        content.style.display = 'none';
      }
      button.classList.toggle('fa-chevron-up');
      button.classList.toggle('fa-chevron-down');
      ibox.classList.toggle('border-bottom');
      setTimeout(function () {
        var event = document.createEvent('HTMLEvents');
        event.initEvent('resize', true, false);
        ibox.dispatchEvent(event);
        var mp = ibox.querySelectorAll('[id^=map-]').item(0);
        if (mp !== null) { mp.dispatchEvent(event); }
      }, 50);
    }

    if (collapseRef && collapseRef.current) {
      collapseRef.current.addEventListener("click", toggle);
    }

    if (closeRef && closeRef.current) {
      var closeEl = closeRef.current;
      closeRef.current.addEventListener("click", function (e) {
        var content = closeEl.closest('div.ibox');
        content.remove();
      });
    }
  }, []);

  return (
    <>
      <div className="wrapper wrapper-content ">
        <div className="row ">
          <div className="col-lg-12">
            <div className="ibox float-e-margins collapsed" style={{ marginBottom: "0px" }}>
              <div className={"ibox-title " + bg}>
                <h5>
                  {title}
                </h5>
                <div className="ibox-tools">
                  <a ref={collapseRef} className="collapse-link">
                    <i className="fa fa-chevron-up"></i>
                  </a>
                  <a ref={closeRef} className="close-link">
                    <i className="fa fa-times"></i>
                  </a>
                </div>
              </div>
              <div className="ibox-content">
                {props.children}
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
