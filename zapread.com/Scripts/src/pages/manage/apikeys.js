﻿/*
 * Manage user API keys
 */

import "../../shared/shared";
import "../../realtime/signalr";

import React, { useCallback, useEffect, useState } from "react";
import ReactDOM from "react-dom";
import PageHeading from "../../components/PageHeading";
import KeysTable from "./keystable";

import "../../shared/sharedlast";

function Page() {
  return (
    <div>
      <PageHeading
        title="Manage API Keys"
        controller="Manage"
        method="API Keys"
        function="list"
      />
      <div className="row">
        <div className="col-lg-12">
          <div className="wrapper wrapper-content animated fadeInUp">
            <div className="ibox">
              <div className="ibox-content">
                <KeysTable />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));
