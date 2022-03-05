/**
 * Modal for user voting
 */

import React, { useCallback, useEffect, useState, createRef } from "react";
import { Modal, Container, Row, Col, ButtonGroup, Button, Card } from "react-bootstrap";
import { onVote, onCancelVote } from "../utility/ui/vote";
import { checkInvoicePaid } from "../utility/ui/accountpayments";
import { useUserInfo } from "./hooks/useUserInfo";
import { on, off } from "../utility/events";
import { refreshUserBalance } from '../utility/refreshUserBalance';
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
  const [voteTx, setVoteTx] = useState(-1); // Transaction Id (unspent) to register with the vote
  const [voteDirection, setVoteDirection] = useState("up");
  const [invoice, setInvoice] = useState("");
  const [invoiceQRURL, setInvoiceQRURL] = useState("~/Content/FFFFFF-0.png");
  const [voteTarget, setVoteTarget] = useState(null);

  const [showGetInvoiceButton, setShowGetInvoiceButton] = useState(false);
  const [showCheckPaymentButton, setShowCheckPaymentButton] = useState(false);
  const [showVoteButton, setShowVoteButton] = useState(true);

  const [show, setShow] = useState(false);
  const [showQRLoading, setShowQRLoading] = useState(false);

  const handleClose = () => {
    // Cleanup & reset
    setShow(false);
    setShowGetInvoiceButton(false);
    setShowCheckPaymentButton(false);
    setShowVoteButton(true);
    setFooterMessage("Click vote to confirm");
  };
  const handleShow = () => setShow(true);

  const inputRef = createRef();

  // Ideally we should try remove these refs by refactoring
  const checkPaidButtonRef = createRef();
  const voteButtonRef = createRef();
  const voteCancelButtonRef = createRef();
  const userInfo = useUserInfo(); // Custom hook

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
    //setFooterMessage("Please pay lightning invoice");
  }

  function setStateCheckPayment() {
    setShowCheckPaymentButton(true);
    setShowVoteButton(false);

  }

  function handleVote() {
    if (voteAmount > userInfo.balance) {
      // Not enough funds for the vote
      setStateGetInvoice();
    }
    else {
      spinnerOn(voteTarget);
      doVote();
    }
  }

  /**
   * Clicked on the get invoice button 
   **/
  function handleGetInvoice() {
    setShowVoteButton(false);
    setShowGetInvoiceButton(false);
    setShowCheckPaymentButton(true);
    setShowQRLoading(true);

    var memo = "ZapRead.com vote on " + voteType;

    // Get an invoice
    postJson("/Lightning/GetDepositInvoice/", {
      amount: voteAmount,
      memo: memo,
      anon: IsAuthenticated ? 1 : 0,
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
      }
      else {
        setFooterMessage(response.message);
        setFooterBg("bg-danger");
        setShowQRLoading(false);
      }
    })
    .then(() => {
      //showVoteModal();
    })
    .catch((error) => {
      console.log(error);
      setFooterMessage("Error generating invoice");
      setFooterBg("bg-danger");
      setShowQRLoading(false);
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
    var icon = voteTarget;
    icon.classList.remove('fa-circle-notch');
    icon.classList.remove('fa-spin');
    icon.classList.add(direction == "up" ? 'fa-chevron-up' : 'fa-chevron-down');
    icon.style.color = '';
  }

  async function doVote() {
    handleClose(); // Close the modal

    var uid = voteType == "post" ? 'uVote_' : voteType == "comment" ? "uVotec_" : ""; // element for up arrow
    var did = voteType == "post" ? 'dVote_' : voteType == "comment" ? "dVotec_" : ""; // element for down arrow
    var sid = voteType == "post" ? "sVote_" : voteType == "comment" ? "sVotec_" : ""; // element for score
    var voteurl = voteType == "post" ? "/Vote/Post" : "/Vote/Comment";

    // Do the vote
    await postJson(voteurl, {
      Id: voteId,
      d: voteDirection == "up" ? 1 : 0,
      a: voteAmount,
      tx: voteTx
    }).then((data) => {
      if (data.success) {
        updateUserInfo({
          balance: userInfo.balance - voteAmount
        });
        spinnerOff(voteTarget, voteDirection);

        var val = data.scoreStr;
        document.getElementById(sid + voteId).innerHTML = val.toString();

        var deltaCommunity = data.deltaCommunity;
        var amountEl = document.getElementById("amount-info-payout");
        if (amountEl != null) {
          amountEl.innerHTML = parseInt(amountEl.innerHTML) + deltaCommunity;
        }

        var delta = Number(data.delta);
        if (delta === 1) {
          document.getElementById(uid + voteId).classList.remove("text-muted");
          document.getElementById(did + voteId).classList.add("text-muted");
        }
        else if (delta === 0) {
          document.getElementById(uid + voteId).classList.add("text-muted");
          document.getElementById(did + voteId).classList.add("text-muted");
        }
        else {
          document.getElementById(did + voteId).classList.remove("text-muted");
          document.getElementById(uid + voteId).classList.add("text-muted");
        }
      } else {
        // not successful
        setShow(true); // Bring the modal back
        setFooterMessage(data.message);
        setFooterBg("bg-danger")
      }
    });
  }

  const onVoteEventHandler = useCallback((e) => {
    setVoteType(e.detail.type);
    setVoteId(e.detail.id);
    setVoteTarget(e.detail.target);
    setVoteDirection(e.detail.direction);

    if (!IsAuthenticated) {
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
    } else {
      refreshUserBalance().then((userBalance) => {
        updateUserInfo({
          balance: userBalance
        });
        if (userBalance < voteAmount) {
          console.log(userInfo, voteAmount);
          setStateGetInvoice();
        }
        setShow(true); // Show the Modal
      });
    }
  }, []);

  const onPaidEventHandler = useCallback((e) => {
    setVoteTx(e.detail.tx);
    spinnerOn(voteTarget);
    doVote();
  });

  // Register event handler for vote click
  useEffect(() => {
    on("vote", onVoteEventHandler);

    return () => {
      off("vote", onVoteEventHandler);
    }
  }, [onVoteEventHandler]);

  // Register event handler for vote click
  useEffect(() => {
    on("zapread:vote:invoicePaid", onPaidEventHandler);

    return () => {
      off("zapread:vote:invoicePaid", onPaidEventHandler);
    }
  }, [onPaidEventHandler]);

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
                      onChange={({ target: { value } }) => setVoteAmount(value)} // Controlled input
                      ref={inputRef}
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

                <a href={"lightning:" + invoice} style={showQRLoading ? {} : { display: "none" }}>
                  <img loading="lazy" src={invoiceQRURL} className="img-fluid" />
                </a>

                <div className="input-group mb-3" id="voteDepositInvoice" style={showQRLoading ? {} : { display: "none" }}>
                  <div className="input-group-prepend">
                    <a href="lightning:xxx" id="lnDepositInvoiceLink" className="btn btn-primary" role="button" aria-pressed="true"><span className="fa fa-bolt"></span></a>
                  </div>
                  <input type="text" id="voteDepositInvoiceInput" className="form-control" placeholder="invoice" aria-label="invoice" aria-describedby="basic-addon2" />
                  <div className="input-group-append">
                    <button className="btn btn-primary" type="button"
                      onClick={() => {
                        //"copyToClipboard(this,'voteDepositInvoiceInput');"
                      }}><span className="fa fa-copy"></span>&nbsp;Copy</button>
                  </div>
                </div>

              </Container>
            </Card.Body>
            <Card.Footer className={footerBg}> {footerMessage} </Card.Footer>
          </Card>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="info" onClick={handleClose} style={showCheckPaymentButton ? {} : { display: "none" }}>
            Check Payment
          </Button>
          <Button variant="primary" onClick={handleVote} style={showVoteButton ? {} : { display: "none" }}>
            Vote
          </Button>
          <Button variant="primary" onClick={handleGetInvoice} style={showGetInvoiceButton ? {} : { display: "none" }}>
            Get Invoice
          </Button>
          <Button variant="secondary" onClick={handleClose}>
            Cancel
          </Button>
        </Modal.Footer>
      </Modal>

      <div className="modal fade" id="voteModal" data-backdrop="static" tabIndex="-1" role="dialog" aria-labelledby="voteModalTitle" aria-hidden="true">
        <div className="modal-dialog modal-dialog-centered" role="document">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title" id="voteModalTitle">Modal title</h5>
              <button type="button" className="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
              </button>
            </div>
            <div className="modal-body">
              <div className="card ">
                <div className="card-body">
                  <div className="row">
                    <div className="col-lg-12">
                      <div className="row">
                        <div className="col-5">
                          <span> Pay </span>
                          <input
                            type="number"
                            min={1}
                            id="voteValueAmount"
                            placeholder="Amount"
                            className="form-control font-bold"
                            aria-label="Amount" />
                          <small className="text-muted">Satoshi</small>
                        </div>
                        <div className="col-5 text-right">
                          <span> Balance </span>
                          <h2 className="font-bold"><span id="userVoteBalance">0</span> </h2>
                          <small className="text-muted">Satoshi</small>
                        </div>
                        <div className="col-2">
                          <i className="fa fa-bolt fa-5x"></i>
                        </div>
                      </div>
                      <h2 className="font-bold"><span id="payAmount" style={{ display: "none" }}></span></h2>
                    </div>
                  </div>

                  <div id="voteQRloading" style={{ display: "none" }}>
                    <div className="sk-loading" style={{ BorderStyle: "none" }}>
                      <div className="sk-spinner sk-spinner-three-bounce">
                        <div className="sk-bounce1"></div>
                        <div className="sk-bounce2"></div>
                        <div className="sk-bounce3"></div>
                      </div>
                    </div>
                  </div>

                  <a id="lnDepositInvoiceImgLink" href="lightning:xxx">
                    <img loading="lazy" id="voteDepositQR" src="~/Content/FFFFFF-0.png" className="img-fluid" />
                  </a>

                  <div className="input-group mb-3" id="voteDepositInvoice" style={{ display: "none" }}>
                    <div className="input-group-prepend">
                      <a href="lightning:xxx" id="lnDepositInvoiceLink" className="btn btn-primary" role="button" aria-pressed="true"><span className="fa fa-bolt"></span></a>
                    </div>
                    <input type="text" id="voteDepositInvoiceInput" className="form-control" placeholder="invoice" aria-label="invoice" aria-describedby="basic-addon2" />
                    <div className="input-group-append">
                      <button className="btn btn-primary" type="button"
                        onClick={() => {
                          //"copyToClipboard(this,'voteDepositInvoiceInput');"
                        }}><span className="fa fa-copy"></span>&nbsp;Copy</button>
                    </div>
                  </div>
                </div>
                <div className="card-footer bg-info" id="voteDepositInvoiceFooter">
                  Click vote to confirm.
                </div>
              </div>
            </div>
            <div className="modal-footer">
              <button ref={checkPaidButtonRef}
                onClick={() => {
                  checkInvoicePaid(checkPaidButtonRef.current);
                }}
                id="btnCheckLNVote"
                type="button"
                className="btn btn-info btn-ln-checkPayment"
                style={{ display: "none" }}
                data-invoice-element="voteDepositInvoiceInput"
                data-spin-element="spinCheckPaymentVote">
                Check Payment <i id="spinCheckPaymentVote" className="fa-solid fa-circle-notch fa-spin" style={{ display: "none" }}></i>
              </button>
              <button ref={voteButtonRef} onClick={() => {
                onVote(voteButtonRef.current);
              }} id="voteOkButton" type="button" className="btn btn-primary">
                Vote
              </button>
              <button ref={voteCancelButtonRef} onClick={() => {
                onCancelVote(voteCancelButtonRef.current);
              }} type="button" className="btn btn-secondary" data-dismiss="modal">
                Cancel
              </button>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
