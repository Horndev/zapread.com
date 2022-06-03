/**
 * Modal for user voting
 * 
 * List of locations where voting is used:
 *  - home/index
 *  - post/detail
 *  - manage/index
 *  - user
 *  - group/detail
 * 
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
const getSwal = () => import('sweetalert2'); //import Swal from 'sweetalert2';
import { Modal, Container, Row, Col, Button, Card } from "react-bootstrap";
import { useUserInfo } from "./hooks/useUserInfo";
import { on, off } from "../utility/events";
import { refreshUserBalance } from '../utility/refreshUserBalance';
import { oninvoicepaid } from "../utility/payments/oninvoicepaid";
import { updateUserInfo } from '../utility/userInfo';
import { postJson } from '../utility/postData';

export default function VoteModal(props) {
  const [isInitialized, setIsInitialized] = useState(false);
  const [qrURL, setQrURL] = useState("");
  const [voteType, setVoteType] = useState("post");
  const [modalTitle, setModalTitle] = useState("Vote");
  const [footerMessage, setFooterMessage] = useState("Click vote to confirm");
  const [footerBg, setFooterBg] = useState("bg-info");
  const [voteAmount, setVoteAmount] = useState(1);
  const [voteId, setVoteId] = useState(-1);
  const voteTx = useRef(-1);
  const [voteDirection, setVoteDirection] = useState("up");
  const [invoice, setInvoice] = useState("");
  const [invoiceQRURL, setInvoiceQRURL] = useState("/Content/FFFFFF-0.png");
  const [voteTarget, setVoteTarget] = useState(null);
  const [copied, setCopied] = useState(false);
  const [isQuickVoteOn, setIsQuickVoteOn] = useState(false);
  const [showGetInvoiceButton, setShowGetInvoiceButton] = useState(false);
  const [showCheckPaymentSpinner, setShowCheckPaymentSpinner] = useState(false);
  const [showCheckPaymentButton, setShowCheckPaymentButton] = useState(false);
  const [showVoteButton, setShowVoteButton] = useState(true);
  const [show, setShow] = useState(false);
  const [showQRLoading, setShowQRLoading] = useState(false);
  const [showQR, setShowQR] = useState(false);
  const invoiceInputRef = createRef();

  const userInfo = useUserInfo(); // Custom hook
  const userInfoRef = useRef(userInfo);

  useEffect(() => {
    setVoteAmount(userInfo.quickVoteAmount);
    setIsQuickVoteOn(userInfo.quickVote);
  }, [userInfo]);

  const handleClose = () => {
    // Cleanup & reset
    setShow(false);
    setCopied(false);
    setShowGetInvoiceButton(false);
    setShowCheckPaymentButton(false);
    setShowVoteButton(true);
    setShowQR(false);
    setShowQRLoading(false);
    setFooterMessage("Click vote to confirm");
  };

  const handleShow = () => setShow(true);

  const copyInvoiceToClipboard = () => {
    var inputEl = invoiceInputRef.current;
    inputEl.focus();
    inputEl.select();
    inputEl.setSelectionRange(0, 99999);
    navigator.clipboard
      .writeText(invoice)
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
      //var msg = successful ? 'successful' : 'unsuccessful';
      //console.log('Copying text command was ' + msg);
      setTimeout(function () { setCopied(false); }, 10000);
    } catch (err) {
      //console.log('Oops, unable to copy');
    }

    setCopied(true);
  }

  // Monitor for changes in props
  useEffect(
    () => {
      if (!isInitialized) {
        setQrURL("/Img/QR?qr=" + encodeURIComponent("02cda8c01b2303e91bec74c43093d5f1c4fd42a95671ae27bf853d7dfea9b78c06@lightning.zapread.com:9735"));
        // This initializes the modals (done manually using bootstrap.native)
        //var bsnModalInit = new bsn.Modal("#voteModal");
      }
      setIsInitialized(true);
    },
    []
  );

  function setStateGetInvoice() {
    setShowVoteButton(false);
    setShowGetInvoiceButton(true);
    setShowCheckPaymentButton(false);
    setFooterMessage("Click to get a lightning invoice");
  }

  function setStateCheckPayment() {
    setShowCheckPaymentButton(true);
    setShowVoteButton(false);
  }

  function handleVote(e) {
    // Note - don't need to check if authenticated since this only button visible when logged in.
    refreshUserBalance(true).then((userBalance) => {
      if (parseInt(voteAmount) > parseInt(userBalance)) {
        console.log("vote amount", voteAmount, "greater than balance", userBalance);
        // Not enough funds for the vote
        setStateGetInvoice();
      }
      else {
        if (e) {
          spinnerOn(e.detail.target);
          doVote(e);
        } else {
          spinnerOn(voteTarget);
          doVote();
        }
      }
    });
  }

  /**
   * Clicked on the get invoice button 
   **/
  function handleGetInvoice() {
    setShowVoteButton(false);
    setShowGetInvoiceButton(false);
    setShowCheckPaymentButton(true);
    setShowQRLoading(true);

    var memo = "ZapRead.com vote " + voteDirection + " on " + voteType;

    // Get an invoice
    postJson("/Lightning/GetDepositInvoice/", {
      amount: voteAmount,
      memo: memo,
      anon: !IsAuthenticated ? 1 : 0,
      use: voteType,
      useId: voteId,
      useAction: voteDirection == "up" ? 1 : 0 // direction of vote 0=down; 1=up
    }).then((response) => {
      if (response.success) {
        setInvoice(response.Invoice);
        setInvoiceQRURL("/Img/QR?qr=" + encodeURI("lightning:" + response.Invoice));
        setFooterBg("bg-info");
        setFooterMessage("Please pay invoice");
        setShowQRLoading(false);
        setShowQR(true);
      }
      else {
        setFooterMessage(response.message);
        setFooterBg("bg-danger");
        setShowQRLoading(false);
        setShowQR(false);
      }
    })
    .then(() => {
      //showVoteModal();
    })
    .catch((error) => {
      console.log(error);
      setFooterMessage("Error generating invoice");
      setFooterBg("bg-danger");
      setShowVoteButton(false);
      setShowGetInvoiceButton(true);
      setShowCheckPaymentButton(false);
      setShowQRLoading(false);
    });
  }

  function handleCheckPayment() {
    setShowCheckPaymentSpinner(true);
    postJson("/Lightning/CheckPayment/", {
      invoice: invoice,
      isDeposit: true
    })
      .then((response) => {
        setShowCheckPaymentSpinner(false);
        //document.getElementById(spinElId).style.display = "none";//$("#" + $(e).data('spin-element')).hide();
        if (response.success) {
          if (response.result === true) {
            oninvoicepaid(response.invoice, response.balance, response.txid);
            //handleInvoicePaid(response);
            // Payment has been successfully made
            console.log('Payment confirmed');
          } else {
            setFooterMessage("Not yet paid. Please pay invoice");
          }
        }
        else {
          setShowCheckPaymentSpinner(false);
          //alert(response.message);
          setShow(true);
          setFooterMessage(response.message);
          setFooterBg("bg-danger")
        }
      })
      .catch((error) => {
        setShowCheckPaymentSpinner(false);
        //document.getElementById(spinElId).style.display = "none";//$("#" + $(e).data('spin-element')).hide();
        //alert(response.message);
        setShow(true);
        setFooterMessage(response.message);
        setFooterBg("bg-danger")
      });
  }

  function spinnerOn(target) {
    var icon = target;
    icon.classList.remove('fa-chevron-up');
    icon.classList.remove('fa-chevron-down');
    icon.classList.add('fa-circle-notch');
    icon.classList.add('fa-spin');
    icon.style.color = 'darkcyan';
  }

  function spinnerOff(target, direction) {
    // Stop the spinner
    var icon = target;
    icon.classList.remove('fa-circle-notch');
    icon.classList.remove('fa-spin');
    icon.classList.add(direction == "up" ? 'fa-chevron-up' : 'fa-chevron-down');
    icon.style.color = '';
  }

  async function doVote(e) {

    handleClose(); // Close the modal

    var currentUserInfo = userInfo;

    var vt = voteType;
    var vtgt = voteTarget;
    var vid = voteId;
    var vd = voteDirection;
    var va = voteAmount;
    if (e) {
      vt = e.detail.type;
      vid = e.detail.id;
      vd = e.detail.direction;
      vtgt = e.detail.target;
      va = e.detail.amount;
      currentUserInfo = e.detail.userInfo;
    }

    var uid = vt == "post" ? 'uVote_' : vt == "comment" ? "uVotec_" : ""; // element for up arrow
    var did = vt == "post" ? 'dVote_' : vt == "comment" ? "dVotec_" : ""; // element for down arrow
    var sid = vt == "post" ? "sVote_" : vt == "comment" ? "sVotec_" : ""; // element for score
    var voteurl = vt == "post" ? "/Vote/Post" : "/Vote/Comment";

    if (currentUserInfo) {
      console.log(currentUserInfo);
      await postJson("/api/v1/account/quickvote/update/", {
        QuickVoteOn: currentUserInfo.quickVote,
        QuickVoteAmount: va
      }).then((response) => {
        if (response.success) { }
      }).catch((error) => {
        console.log(error);
      });
    }
    
    // Do the vote
    await postJson(voteurl, {
      Id: vid,
      d: vd == "up" ? 1 : 0,
      a: va,
      tx: voteTx.current
    }).then((data) => {
      if (data.success) {
        updateUserInfo({
          balance: window.userInfo.balance - va
        });
        spinnerOff(vtgt, vd);

        var val = data.scoreStr;
        var valEl = document.getElementById(sid + vid);
        if (valEl) {
          valEl.innerHTML = val.toString();
        }

        var deltaCommunity = data.deltaCommunity;
        var amountEl = document.getElementById("amount-info-payout");
        if (amountEl != null) {
          amountEl.innerHTML = parseInt(amountEl.innerHTML) + deltaCommunity;
        }

        var delta = Number(data.delta);
        if (delta === 1) {
          document.getElementById(uid + vid).classList.remove("text-muted");
          document.getElementById(did + vid).classList.add("text-muted");
        }
        else if (delta === 0) {
          document.getElementById(uid + vid).classList.add("text-muted");
          document.getElementById(did + vid).classList.add("text-muted");
        }
        else {
          document.getElementById(did + vid).classList.remove("text-muted");
          document.getElementById(uid + vid).classList.add("text-muted");
        }
      } else {
        // not successful
        setShow(true); // Bring the modal back
        setFooterMessage(data.message);
        setFooterBg("bg-danger")
      }
    });
  }

  /**
   * Event registration
   **/
  const onVoteEventHandler = useCallback((e) => {    

    // Refreshes user info
    var currentUserInfo = e.detail.userInfo;//window.userInfo;

    //console.log("e.detail.userInfo",e.detail.userInfo);

    setVoteType(e.detail.type);
    setVoteId(e.detail.id);
    setVoteTarget(e.detail.target);
    setVoteDirection(e.detail.direction);

    if (!IsAuthenticated) {
      getSwal().then(({ default: Swal }) => {
        Swal.fire({
          icon: 'info',
          title: 'Anonymous Vote',
          text: 'You are not logged in, but you can still vote anonymously with a Bitcoin Lightning Payment.',
          footer: '<a href="/Account/Login">Log in instead</a>'
        }).then(() => {
          setShow(true); // Show the Modal
          setStateGetInvoice();
          //appInsights.trackEvent({
          //  name: 'Anonymous Vote',
          //  properties: {
          //    amount: userVote.amount.toString()
          //  }
          //});
        });
      });
    } else {
      refreshUserBalance(true).then((userBalance) => {
        if (userBalance < voteAmount) {
          //console.log(userInfo, voteAmount);
          setStateGetInvoice();
        }
        if (currentUserInfo.quickVote) {
          console.log('quickvote');
          e.detail.amount = currentUserInfo.quickVoteAmount;
          handleVote(e); // Do the vote
        } else {
          setShow(true); // Show the Modal
        }
      });
    }
  }, []);

  const onPaidEventHandler = useCallback((e) => {
    console.log("VoteModal onPaidEventHandler", e.detail);
    //setVoteTx(e.detail.tx);
    voteTx.current = e.detail.tx;
    spinnerOn(voteTarget);
    doVote();
  });

  // Register event handler for vote click
  useEffect(() => {
    on("vote", onVoteEventHandler);

    return () => {
      off("vote", onVoteEventHandler);
    }
  }, [onVoteEventHandler, userInfo]);

  // Register event handler for invoice paid
  useEffect(() => {
    on("zapread:vote:invoicePaid", onPaidEventHandler);

    return () => {
      off("zapread:vote:invoicePaid", onPaidEventHandler);
    }
  }, [onPaidEventHandler]);

  const onUpdateVoteAmount = (value) => {
    setVoteAmount(value)
  };

  return (
    <>
      <Modal id="r-modal" show={show} onHide={handleClose}>
        <Modal.Header closeButton>
          <Modal.Title>{modalTitle}</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <Card>
            <Card.Body>
              <Container>
                <Row>
                  <Col xs={6}>
                    <span>Pay{" "}</span>
                    <input
                      onChange={({ target: { value } }) => onUpdateVoteAmount(value) } // Controlled input
                      value={voteAmount}
                      type="number"
                      min={1}
                      placeholder="Amount"
                      className="form-control font-bold"
                      aria-label="Amount" />
                    <small className="text-muted">Satoshi</small>
                  </Col>
                  <Col xs={6} className="text-right">
                    <span> Balance </span>
                    <h2 className="font-bold">{userInfo.balance}{" "}<i className="fa fa-bolt"></i></h2>
                    <small className="text-muted">Satoshi</small>
                  </Col>
                </Row>

                <div id="voteQRloading" style={showQRLoading ? {} : { display: "none" }}>
                  <div className="sk-loading" style={{ BorderStyle: "none" }}>
                    <div className="sk-spinner sk-spinner-three-bounce">
                      <div className="sk-bounce1"></div>
                      <div className="sk-bounce2"></div>
                      <div className="sk-bounce3"></div>
                    </div>
                  </div>
                </div>

                <a href={"lightning:" + invoice} style={showQR ? {} : { display: "none" }}>
                  <img loading="lazy" src={invoiceQRURL} className="img-fluid" />
                </a>

                <div className="input-group mb-3" id="voteDepositInvoice" style={showQR ? {} : { display: "none" }}>
                  <div className="input-group-prepend">
                    <a href={"lightning:" + invoice} id="lnDepositInvoiceLink" className="btn btn-primary" role="button" aria-pressed="true"><span className="fa fa-bolt"></span></a>
                  </div>
                  <input type="text" id="voteDepositInvoiceInput"
                    ref={invoiceInputRef}
                    value={invoice}
                    readOnly
                    className="form-control" placeholder="invoice" aria-label="invoice" aria-describedby="basic-addon2" />
                  <div className="input-group-append">
                    <button className="btn btn-primary" type="button"
                      onClick={copyInvoiceToClipboard}>
                      {copied ?
                        <><span className='fa-solid fa-copy'></span>&nbsp;Copied</>
                        :
                        <><span className="fa-solid fa-copy"></span>&nbsp;Copy</>
                      }
                    </button>
                  </div>
                </div>
              </Container>
            </Card.Body>
            <Card.Footer className={footerBg}> {footerMessage} </Card.Footer>
          </Card>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="info"
            onClick={handleCheckPayment}
            disabled={showCheckPaymentSpinner || showQRLoading}
            style={showCheckPaymentButton ? {} : { display: "none" }}>
            Check Payment <i className="fa-solid fa-circle-notch fa-spin" style={showCheckPaymentSpinner ? {} : { display: "none" }}></i>
          </Button>
          <Button variant="primary"
            onClick={() => {
              handleVote(); // important to pass parameterless so that event target is original vote icon
            }} style={showVoteButton ? {} : { display: "none" }}>
            Vote
          </Button>
          <Button variant="primary"
            onClick={handleGetInvoice}
            disabled={showQRLoading}
            style={showGetInvoiceButton ? {} : { display: "none" }}>
            Get Invoice
          </Button>
          <Button variant="secondary" onClick={handleClose}>
            Cancel
          </Button>
        </Modal.Footer>
      </Modal>
    </>
  );
}
