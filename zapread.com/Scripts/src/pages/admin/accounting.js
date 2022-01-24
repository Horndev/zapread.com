/*
 * View a group posts
 */

import '../../shared/shared';       // [✓]
import '../../realtime/signalr';    // [✓]

import React, { useEffect, useState } from 'react'; // [✓]
import ReactDOM from "react-dom";                                        // [✓]
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom'; // [✓]

import Spreadsheet from "react-spreadsheet";

import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";

import '../../shared/sharedlast';                                           // [✓]

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [isLoaded, setIsLoaded] = useState(false);

  let query = useQuery();

  const data = [
    [{ value: "Day" },         { value: "# Transactions" }, { value: "Volume (Sat)" }, { value: "Fees (Sat)" } ],
    [{ value: "23 Jan 2022" }, { value: "152" }, { value: "10000" }, { value: "100" }],
    [{ value: "24 Jan 2022" }, { value: "189" }, { value: "50000" }, { value: "500" }],
  ];

  useEffect(() => {
    async function initialize() {
      if (!isLoaded) {
        setIsLoaded(true);
      }
    }
    initialize();
  }, []); // Fire once

  return (
    <>
      <div className="wrapper border-bottom white-bg page-heading">
        <div className="col-lg-10">
          <br />
          <h2>
            Accounting Portal
          </h2>
          <ol className="breadcrumb">
            <li className="breadcrumb-item"><a href="/">Home</a></li>
            <li className="breadcrumb-item"><a href="/Group">Admin</a></li>
            <li className="breadcrumb-item active">Accounting</li>
          </ol>
        </div>
        <div className="col-lg-2">
        </div>
      </div>

      <p>
        [Select month]
      </p>

      <p>
        [Select report]
      </p>
      <Spreadsheet data={data} />

      <div className="wrapper wrapper-content ">
        <div className="row">
          <div className="col-sm-2"></div>
          <div className="col-lg-8">
            <div className="social-feed-box-nb">
              <span></span>
            </div>


          </div>
        </div>
      </div>
    </>
  );
}

ReactDOM.render(
  <Router>
    <Route path="/Admin/Accounting">
      <Page />
    </Route>
  </Router>
  , document.getElementById("root"));