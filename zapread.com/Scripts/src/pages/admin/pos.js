/*
 * Admin Panel Point of Sale
 * 
 */

import "../../shared/shared"; // [✓]
import "../../realtime/signalr"; // [✓]
import React, {
  useCallback,
  useMemo,
  useRef,
  useEffect,
  useState
} from "react";
import ReactDOM from "react-dom";
import { Row, Col, Form, Button, Container } from "react-bootstrap";
import PageHeading from "../../components/PageHeading";
import "../../shared/sharedlast"; // [✓]

function Page() {

  /**
   * initializes the user to select an icon to upload for the group image
   * @param {any} id
   * @param {any} e
   */
  function updateIcon(id, e) {
    inputFile.current.click();
  }

  return (
    <div>
      <PageHeading
        title="ZapRead Point Of Sale"
        controller="Admin"
        method="POS"
        function="Admin"
      />
      <div className="row">
        <div className="col-lg-12">
          <div className="wrapper wrapper-content animated fadeInUp">
            <div className="ibox">
              <div className="ibox-content" />
              <div className="ibox-content">
                <h1>Administer Point Of Sale</h1>
              </div>
              <div className="ibox-content">
                <h2>Subscriptions</h2>
                <Button onClick={() => { } }>Sync Subscriptions</Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));