/*
 * Top user balance and options widget
 */

import React, { useState } from 'react';
import ReactDOM from "react-dom";
import { Form, Dropdown, Modal, Nav, Tab, Container, Row, Col, ButtonGroup, Button, Card } from "react-bootstrap";
const getDepositWithdrawModal = () => import("../DepositWithdrawModal");
import { useUserInfo } from "../hooks/useUserInfo";
import { updateUserInfo } from '../../utility/userInfo';
import { useEffect } from 'react';
import { postJson } from '../../utility/postData';
import Tippy from '@tippyjs/react';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';

export default function BalanceWidget(props) {
  const [depositModalLoaded, setDepositModalLoaded] = useState(false);
  const [quickVoteAmount, setQuickVoteAmount] = useState(0);
  const [quickVoteOn, setQuickVoteOn] = useState(false);
  const userInfo = useUserInfo(); // Custom hook

  useEffect(() => {
    setQuickVoteAmount(userInfo.quickVoteAmount);
    setQuickVoteOn(userInfo.quickVote);
  }, [userInfo]);

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

  const onUpdateQuickVote = (value) => {
    setQuickVoteAmount(value);

    postJson("/api/v1/account/quickvote/update/", {
      QuickVoteOn: quickVoteOn,
      QuickVoteAmount: value
    }).then((response) => {
      if (response.success) {

      }
    }).catch((error) => {
      console.log(error);
    });

    updateUserInfo({
      quickVoteAmount: value
    });
  };

  const onClickQuickVote = (e) => {
    postJson("/api/v1/account/quickvote/update/", {
      QuickVoteOn: !quickVoteOn,
      QuickVoteAmount: quickVoteAmount
    }).then((response) => {
      if (response.success) {
        
      }
    }).catch((error) => {
      console.log(error);
    });

    updateUserInfo({
      quickVote: !quickVoteOn
    });
    setQuickVoteOn(!quickVoteOn);
    e.stopPropagation();
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
            {quickVoteOn ? (<>
              <a role="button" className="dropdown-item btn btn-sm btn-link nav-link" onClick={onClickQuickVote}>&nbsp;
                <i className="fa-solid fa-check-double"></i> Turn quick-vote off
              </a>
              <Form>
                <Row>
                  <Col style={{ margin: "0 8px 8px 8px", display: "flex" }}>
                    <Form.Control
                      type="number"
                      value={quickVoteAmount}
                      onChange={({ target: { value } }) => {
                        onUpdateQuickVote(value);
                      }} // Controlled input
                      min={1}
                      max={userInfo.balance}
                      onClick={(e) => { e.stopPropagation(); }}
                      style={{ height: 32 }}
                    />
                    <span className="btn btn-sm btn-link"
                      onClick={(e) => { e.stopPropagation(); }}>Sats</span>
                  </Col>
                </Row>
              </Form>
            </>) : (<>
                <Tippy
                  theme="light-border"
                  interactive={false}
                  content={
                    <>
                      Turn on to set the value of a single click instead of manually voting each time.
                    </>
                  }>
                  <a role="button" className="dropdown-item btn btn-sm btn-link nav-link" onClick={onClickQuickVote}>&nbsp;
                    <i className="fa-solid fa-check-double"></i> Turn quick-vote on
                  </a>
                </Tippy>
            </>)}
          </div>
        </li>
      </ul>
    </>
  );
}