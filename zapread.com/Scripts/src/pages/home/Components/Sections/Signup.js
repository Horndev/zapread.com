
import React, { useEffect, useState, createRef } from "react";
import { Row, Col, Button } from "react-bootstrap";

export default function Signup(props) {
  const [shareLink, setShareLink] = useState("https://www.zapread.com/Home/About/");
  const [copied, setCopied] = useState(false);
  const shareInputRef = createRef();

  async function LoadReferralCode() {
    await fetch("/api/v1/user/referralcode").then(response => {
      return response.json();
    }).then(data => {
      setShareLink("https://www.zapread.com/Home/About/?refId=" + data.refCode)
    })
  }

  useEffect(() => {
    if (window.IsAuthenticated) {
      LoadReferralCode();
    }
  }, []); // Fire once

  const copyShareToClipboard = () => {
    var inputEl = shareInputRef.current;
    inputEl.focus();
    inputEl.select();
    inputEl.setSelectionRange(0, 99999);
    navigator.clipboard
      .writeText(shareLink)
      .then(() => {
        console.log("successfully copied");
      })
      .catch(() => {
        console.log("something went wrong");
      });

    // Create an event to select the contents (native js)
    var event = document.createEvent('HTMLEvents');
    event.initEvent('select', true, false);
    inputEl.dispatchEvent(event);

    try {
      var successful = document.execCommand('copy');
      setTimeout(function () { setCopied(false); }, 10000);
    } catch (err) {
    }

    setCopied(true);
  }

  return (
    <section id="signup" className="text-left-img-right">
      {window.IsAuthenticated ? (
        <>
          <Row className="stats-header">
            <Col className="text-center wow animate__fadeIn">
              <div className="navy-line"></div>
              <h1>Invite Your Friends</h1>
            </Col >
          </Row >
          <Row>
            <Col lg={4}></Col>
            <Col className="text-center">
              Tell your friends about ZapRead using your referral code and link below.
              If they register, you will both earn an extra 5% every time either of you are voted up for the next 6 months!
              <br /><br />
              <div className="input-group mb-3">
                <input type="text" id="lightningDepositInvoiceInput"
                  ref={shareInputRef}
                  value={shareLink}
                  readOnly
                  className="form-control" placeholder="invoice" aria-label="shareLink" aria-describedby="basic-addon2" />
                <div className="input-group-append">
                  <button className="btn btn-primary" type="button"
                    onClick={copyShareToClipboard}>
                    {copied ?
                      <><span className='fa-solid fa-copy'></span>&nbsp;Copied</>
                      :
                      <><span className="fa-solid fa-copy"></span>&nbsp;Copy</>
                    }
                  </button>
                </div>
              </div>
            </Col>
            <Col lg={4}></Col>
          </Row>
        </>
      ) : (
        <>
          <Row className="stats-header">
            <Col className="text-center wow animate__fadeIn">
              <div className="navy-line"></div>
              <h1>Get Started Now</h1>
            </Col>
          </Row >
          <Row>
            <Col lg={4}></Col>
            <Col className="text-center">
                {props.refId ? (
                  <>
                    <Button variant="primary" onClick={() => { window.location = "//zapread.com/Account/Register/?refcode=" + encodeURIComponent(props.refId) }}>Free Registration</Button>
                  </>
                ) : (
                    <>
                      <Button variant="primary" onClick={() => { window.location = "//zapread.com/Account/Register/" }}>Free Registration</Button>
                  </>)}
            </Col>
            <Col lg={4}></Col>
          </Row>
        </>)}
    </section>
  );
}