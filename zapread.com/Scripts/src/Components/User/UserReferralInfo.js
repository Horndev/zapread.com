/*
 * 
 */

import React, { Suspense, useState, useEffect } from 'react';
import { Container, Row, Col, Button, Dropdown, Form } from "react-bootstrap";
import Swal from 'sweetalert2';
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";
const GiftReferralModal = React.lazy(() => import("./GiftReferralModal"));

export default function UserReferralInfo(props) {
  const [canGiftReferral, setCanGiftReferral] = useState(false);
  const [referralCode, setReferralCode] = useState("");
  const [referralLink, setReferralLink] = useState("");
  const [totalReferred, setTotalReferred] = useState(0);
  const [totalReferredActive, setTotalReferredActive] = useState(0);
  const [isReferralActive, setIsReferralActive] = useState(false);
  const [showGiftReferral, setShowGiftReferral] = useState(false);

  async function LoadReferralStats() {
    await fetch("/api/v1/user/referralstats").then(response => {
      return response.json();
    }).then(data => {
      setTotalReferred(data.TotalReferred);
      setTotalReferredActive(data.TotalReferredActive);
      setIsReferralActive(data.IsActive);
      setCanGiftReferral(data.CanGiftReferral);
    });
  }

  async function LoadReferralCode() {
    await fetch("/api/v1/user/referralcode").then(response => {
      return response.json();
    }).then(data => {
      var reglink = "https://www.zapread.com/Account/Register/?refcode=" + data.refCode
      setReferralCode(data.refCode);
      setReferralLink(reglink);
    })
  }

  useEffect(() => {
    async function initialize() {
      await LoadReferralStats();
      await LoadReferralCode();
    };
    initialize();
  }, []); // Update after shown

  return (
    <>
      <Suspense fallback={<></>}>
        <GiftReferralModal show={showGiftReferral}/>
      </Suspense>

      <div className="ibox-content profile-content">
        <div className="form-group">
          <label htmlFor="regLink">Referral Code</label>
          <input value={referralCode} type="text" id="referralCode" className="form-control" readOnly aria-label="Referral Code" />
        </div>
        <div className="form-group">
          <label htmlFor="regLink">Registration Link</label>
          <input value={referralLink} type="text" id="regLink" className="form-control" readOnly aria-label="Registration Link" />
        </div>

        <Row>
          <Col md={4}><h5><strong id="refTotal">{totalReferred}</strong> Referred</h5></Col>
          <Col md={4}><h5><strong id="refTotalActive">{totalReferredActive}</strong> Active</h5></Col>
          <Col md={4}><h5><strong id="refEnrolled">{isReferralActive ? (<>Referral Active</>) : (<></>)}</strong></h5></Col>
        </Row>

        {canGiftReferral ? (
          <>
            <div className="row m-t-lg">
              <Button
                onClick={() => { setShowGiftReferral(true); }}
                variant="outline-primary" block>Gift Referral</Button>
            </div>
          </>
        ) : (<></>)}

      </div>
    </>
  );
}