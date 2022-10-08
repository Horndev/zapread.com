/**
 * Financial page
 * 
 * [✓] Native JS
 */

import '../../shared/shared';                                           // [✓]
import '../../realtime/signalr';                                        // [✓]
import React, { useCallback, useEffect, useState } from 'react';        // [✓]
import { Container, Row, Col } from 'react-bootstrap';                  // [✓]
import ReactDOM from 'react-dom';                                       // [✓]
import PageHeading from '../../components/PageHeading';                // [✓]
import FinancialSummaryBox from './Components/FinancialSummaryBox';     // [✓]
import LightningTable from './Components/LightningTable';               // [✓]
import EarningTable from './Components/EarningTable';                   // [✓]
import SpendingTable from './Components/SpendingTable';                 // [✓]
import { useUserInfo } from "../../Components/hooks/useUserInfo";
import '../../shared/sharedlast';                                       // [✓]

function Page() {

  const userInfo = useUserInfo(); // Custom hook

  return (
    <div>
      <PageHeading title="User Financial" controller="Manage" method="Financial" function="Overview" />
      <div><Row><Col lg={12}><br /></Col></Row></div>
      <div className="wrapper wrapper-content">
        <div>
          <Row>
            <Col lg={4}>
              <FinancialSummaryBox
                title="Lightning Network Transactions"
                subtitle="Net Flow"
                subtitle2="Balance"
                value2={userInfo.balance}
                units="Satoshi"
                idsummary="lightningFlowBalance"
                idvalue=""
                summaryValue="0"
                idprefix="ln"
                dataUrl="/Account/GetLNFlow/"           // This is where it will fetch the data from
                secondvalue={true}
                idsecondvalue="userBalanceLimboValue"
                titlesecondvalue="Limbo Balance"
                iconclass="fa fa-bolt fa-3x"
                iconstyle={{ color: "gold" }}
              />
            </Col>
            <Col lg={4}>
              <FinancialSummaryBox
                title="Earning Summary"
                subtitle="Earnings"
                subtitle2="Total Earned"
                value2="..."
                units="Satoshi"
                idsummary="earningsBalance"
                idvalue="totalEarningsBalance"
                summaryValue="0"
                idprefix="e"
                dataUrl="/Account/GetEarningsSum/"
                secondvalue={false}
                iconclass="fa fa-arrow-up fa-3x"
                iconstyle={{ color: "#0d901e" }}
              />
            </Col>
            <Col lg={4}>
              <FinancialSummaryBox
                title="Spending Summary"
                subtitle="Spending"
                subtitle2="Total Spent"
                value2="..."
                units="Satoshi"
                idsummary="spendingBalance"
                idvalue="totalSpendingBalance"
                summaryValue="0"
                idprefix="s"
                dataUrl="/Account/GetSpendingSum/"
                secondvalue={false}
                iconclass="fa fa-cart-arrow-down fa-3x"
                iconstyle={{ color: "#9a8200" }}
              />
            </Col>
          </Row>
          <Row>
            <Col sm={12}>
              <LightningTable title="LN Transaction History" pageSize={10} />
            </Col>
          </Row>
          <Row>
            <Col sm={12}>
              <EarningTable title="Earning Events" pageSize={10} />
            </Col>
          </Row>
          <Row>
            <Col sm={12}>
              <SpendingTable title="Spending History" pageSize={10} />
            </Col>
          </Row>
        </div>
      </div>
    </div>
  );
}

ReactDOM.render(
  <Page />
  , document.getElementById("root"));