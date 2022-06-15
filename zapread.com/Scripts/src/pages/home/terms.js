/*
 * Privacy page
 */

import '../../shared/shared';
import '../../realtime/signalr';

import React, { useEffect, useState } from "react";
import ReactDOM from "react-dom";

import '../../shared/sharedlast';

function Page() {

  useEffect(() => {

  }, []); // Fire once

  return (
    <div>
      <Row>
        <Col md={2}></Col>
        <Col className="white-bg">
          <div class="text-center article-title">
            <h1>
              Terms of Use
            </h1>
          </div>
          <p>
            The following terms of use apply to all users of the website:
          </p>
          <p>
            <b>No illegal content will be tolerated.</b> Not only does it put the user at risk, but it also jeopardizes the existence of the website itself.
            Any users posting illegal content will have their posts removed, and accounts disabled.
            Users have 30 days to withdraw funds, after which, any outstanding user balance will be distributed to the community of users, unless prohibited by law.
          </p>
          <p>
            <b>Inactive accounts.</b> Accounts which have not posted or voted over a period of 12 months will be hibernated.
            This means they will no longer receive group or community payments.
            If there is no activity after 18 months, the account will be disabled.
            The user will have 30 days to withdraw funds, after which, any outstanding user balance will be distributed to the community of users, unless prohibited by law.
          </p>
          <p>
            <b>Taxes.</b> Any income earned on Zapread.com is likely taxable income (depending on the residency of the user).
            It is the responsibility of the user to ensure all local tax laws are adhered to.
          </p>
          <p>
            <b>Regulations.</b> Zapread operates as a registered Money Service Business (MSB) in Canada.
            As such, we have <a href="https://www.fintrac-canafe.gc.ca/guidance-directives/transaction-operation/1-eng">reporting requirements</a> for certain virtual currency transactions as stipulated by the Financial Transactions and Reports Analysis Centre of Canada (FINTRAC).
            These regulations are put in place in order to prevent and deter financing of terrorist activities and money laundering of proceeds of crime.
            In particular, we are required to report large virtual currency transactions in an amount equivalent to $10,000 Canadian Dollars or more, or transactions of the equivalent of $10,000 Canadian Dollars or more within a 24-hour window.
            We are also <a href="https://www.fintrac-canafe.gc.ca/guidance-directives/recordkeeping-document/record/msb-eng">required to maintain records</a> of virtual currency transfers quivalent to $1,000 Canadian Dollars or more.
            In addition to the above, we have a requirement to monitor and report some activities relating to domestic and foreign politically exposed persons and heads of international organizations.
          </p>
        </Col>
        <Col md={2}></Col>
      </Row>
    </div>
  );
}

ReactDOM.render(<Page />, document.getElementById("root"));