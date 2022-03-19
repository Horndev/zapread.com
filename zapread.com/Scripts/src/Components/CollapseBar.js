/*
 * React implementation of collapse bar
 **/

import React, { useEffect, useState } from "react";
import { Row, Col} from "react-bootstrap";

export default function CollapseBar(props) {
  const [title, setTitle] = useState(props.title);
  const [bg, setBg] = useState(props.bg);
  const [isCollapsed, setIsCollapsed] = useState(props.isCollapsed);
  const [isClosed, setIsClosed] = useState(false);
  const [isDisabled, setIsDisabled] = useState(false);

  // Monitor for changes in props
  useEffect(
    () => {
      setTitle(props.title);
      setBg(props.bg);
      setIsCollapsed(props.isCollapsed);
      setIsDisabled(props.isDisabled);
    },
    [props.title, props.title, props.isCollapsed, props.isDisabled]
  );

  useEffect(() => {
    if (isCollapsed == false) {
      if ('onExpand' in props) {
        props.onExpand();
      }
    }
  }, [isCollapsed]);

  return (
    <>
      <div className="wrapper wrapper-content " style={isClosed ? { display: "none" } : {}}>
        <div id="overlay" style={isDisabled ? { display: "block" } : {display: "none"}}></div>
        <Row>
          <Col lg={12}>
            <div className="ibox float-e-margins" style={{ marginBottom: "0px" }}>
              <div className={"ibox-title " + bg}>
                <h5>
                  {title}
                </h5>
                <div className="ibox-tools">
                  <a className="collapse-link" onClick={() => { setIsCollapsed(!isCollapsed);}}>
                    <i className={isCollapsed ? "fa-solid fa-chevron-down" : "fa-solid fa-chevron-up"}></i>
                  </a>
                  <a style={props.showClose ? {} : {display: "none"} } className="close-link" onClick={() => {
                    setIsClosed(true);
                  }}>
                    <i className="fa-solid fa-xmark"></i>
                  </a>
                </div>
              </div>
              <div className="ibox-content" style={isCollapsed ? { display: "none" } : {}}>
                {props.children}
              </div>
            </div>
          </Col>
        </Row>
      </div>
    </>
  );
}
