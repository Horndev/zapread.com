/*
 * React component for the top header and breadcrumb
 */
import React from 'react';
import { ThemeColorContext } from "./Theme/ThemeContext";

export default function PageHeading(props) {
  return (
    <ThemeColorContext.Consumer>
      {({ bgColor }) => (
        <div className={"wrapper border-bottom " + bgColor + "-bg page-heading"}>
          <div className="col-lg-10">
            <br />
            {props.pretitle}
            <h2>{props.title}</h2>
            <ol className="breadcrumb">
              <li className="breadcrumb-item"><a href="/">{props.controller}</a></li>
              <li className="breadcrumb-item"><a href="/">{props.method}</a></li>
              <li className="breadcrumb-item active">{props.function}</li>
            </ol>
            {props.children}
          </div>
          <div className="col-lg-2" />
        </div>
      )}
    </ThemeColorContext.Consumer>
  );
}