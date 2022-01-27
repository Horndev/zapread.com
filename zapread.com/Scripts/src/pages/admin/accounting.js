/*
 * View a group posts
 */

import '../../shared/shared';
import '../../realtime/signalr';

import React, { useEffect, useState, useRef } from 'react';
import ReactDOM from "react-dom";
import { useLocation, useParams, BrowserRouter as Router, Route } from 'react-router-dom';

import { Tabs, Tab, Button } from 'react-bootstrap'
import Spreadsheet from "react-spreadsheet";
import Picker from 'react-month-picker'

import "react-month-picker/css/month-picker.css";

import "react-selectize/themes/base.css";
import "react-selectize/themes/index.css";

import '../../shared/sharedlast';

function useQuery() {
  return new URLSearchParams(useLocation().search);
}

function Page() {
  const [isLoaded, setIsLoaded] = useState(false);
  const [monthYear, setMonthYear] = useState({ year: 2022, month: 1 });

  let query = useQuery();
  let pickAMonth = useRef();

  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

  const seeddata = [
    [{ value: "Day" }, { value: "# Transactions" }, { value: "Volume (Sat)" }, { value: "Fees (Sat)" }],
  ];

  const [data, setData] = useState(seeddata);

  const handleMYOnChange = (year, month) => {
    setMonthYear({ year: year, month: month });
  };

  const handleMYOnClose = (value) => {
    setMonthYear(value);
  };

  useEffect(() => {
    async function getRevenue() {
      await fetch(`/api/v1/admin/accounting/${monthYear.year}/${monthYear.month}/`) // api/v1/admin/accounting/{year}/{month}
        .then(response => response.json())
        .then(json => {
          var adata = json.data.map(i => i.Cells)
          setData(adata);
        });
    }
    getRevenue();
  }, [monthYear]);

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

      <div className="wrapper wrapper-content white-bg">
        <div className="row">
          <div className="col-sm-2"></div>
          <div className="col-lg-8">
            <div className="social-feed-box-nb">
              <span></span>
            </div>

            <Tabs defaultActiveKey="overview" id="accounting-tabs-views">
              <Tab eventKey="overview" title="Overview">
                <p>
                  TODO
                </p>
              </Tab>
              <Tab eventKey="revenue" title="Revenue">
                <div>
                  <Picker
                    ref={pickAMonth}
                    years={5}
                    value={monthYear}
                    lang={months}
                    onChange={handleMYOnChange}
                    onDismiss={handleMYOnClose}
                  />
                  <Button onClick={() => {
                    pickAMonth.current.show();
                  }}>{months[monthYear.month - 1]}-{monthYear.year}</Button>
                </div>
                <br />
                <Spreadsheet data={data} />
              </Tab>
              <Tab eventKey="liabilities" title="Liabilities">
                <p>
                  TODO
                </p>
              </Tab>
              <Tab eventKey="assets" title="Assets">
                <p>
                  TODO
                </p>
              </Tab>
              <Tab eventKey="transactions" title="Transactions">
                <p>
                  TODO
                </p>
              </Tab>
            </Tabs>

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