/*
 * Top user balance and options widget
 */

import React, { useState } from 'react';
import ReactDOM from "react-dom";
import { Dropdown, Modal, Nav, Tab, Container, Row, Col, ButtonGroup, Button, Card } from "react-bootstrap";
const getDepositWithdrawModal = () => import("../DepositWithdrawModal");
import { useUserInfo } from "../hooks/useUserInfo";

export default function BalanceWidget(props) {
  const [depositModalLoaded, setDepositModalLoaded] = useState(false);
  const userInfo = useUserInfo(); // Custom hook

  const openDepositWithdrawModal = () => {
    const event = new Event('zapread:depositwithdraw');
    document.dispatchEvent(event);
  }

  const onClickDepositWithdraw = () => {
    if (!depositModalLoaded) {
      getDepositWithdrawModal().then(({ default: DepositWithdrawModal }) => {
        ReactDOM.render(<DepositWithdrawModal />, document.getElementById("ModalDepositWithdraw"));
      }).then(() => {
        openDepositWithdrawModal();
        setDepositModalLoaded(true);
      });
    } else {
      openDepositWithdrawModal();
    }
  }

  return (
    <>
      <ul className="nav navbar-nav flex-row" style={{marginLeft: "10px"}}>
        <li className="nav-item dropdown">
          <a className="nav-link dropdown-toggle" href="#" id="AuthedUserMenu" data-toggle="dropdown"
            style={{ whiteSpace: "nowrap", paddingRight: "10px" }}>
            <i className="fa fa-bitcoin"></i>&nbsp;
            <span id="topUserBalance" className="userBalanceValue" data-toggle="tooltip" data-placement="bottom" title="Balance">
              {userInfo.balance}
            </span>
          </a>
          <div className="dropdown-menu dropdown-menu-right"
            style={{ textAlign: "left", position: "absolute" }}>
            <a role="button" className="dropdown-item btn btn-sm btn-link nav-link" onClick={onClickDepositWithdraw}>&nbsp;
              <i className="fa-solid fa-right-left"></i> Deposit/Withdraw
            </a>
          </div>
        </li>
      </ul>
          {/*<a className="nav-link dropdown-toggle" href="#" id="AuthedUserMenu" data-toggle="dropdown" style="white-space:nowrap; padding-right: 10px;">*/}
          {/*  <i class="fa fa-bitcoin"></i>&nbsp;*/}
          {/*  <span id="topUserBalance" class="userBalanceValue" data-toggle="tooltip" data-placement="bottom" title="Balance deposit/withdraw">*/}
          {/*    {userBalance}*/}
          {/*  </span>*/}
          {/*</a>*/}
          {/*<div className="dropdown-menu dropdown-menu-right" style={"text-align: left;position: absolute;"}>*/}
          {/*  <a class="dropdown-item btn btn-sm btn-link nav-link" href="">*/}
          {/*    &nbsp;<i class="fa-solid fa-user"></i> Profile*/}
          {/*  </a>*/}
          {/*  <a class="dropdown-item btn btn-sm btn-link nav-link" href="">*/}
          {/*    &nbsp;<i class="fa-solid fa-bitcoin"></i> Financial*/}
          {/*  </a>*/}
          {/*</div>*/}
    </>
  );
}