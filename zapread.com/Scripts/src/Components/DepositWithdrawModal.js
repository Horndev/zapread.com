/**
 * Modal UI for deposit and withdraw
 */

import React, { useCallback, useEffect, useState, useRef, createRef } from "react";
import { Modal, Nav, Tab, Container, Row, Col, ButtonGroup, Button, Card } from "react-bootstrap";
import { useUserInfo } from "./hooks/useUserInfo";
import { on, off } from "../utility/events";
import { postJson } from '../utility/postData';
import { oninvoicepaid } from "../utility/payments/oninvoicepaid";
import { refreshUserBalance } from '../utility/refreshUserBalance';

const getWebrtcAdapter = () => import('webrtc-adapter');
//const getInstascan = () => import('instascan');

import { getScript } from '../utility/getScript';

export default function DepositWithdrawModal(props) {
  const [key, setKey] = useState("deposit");
  const [show, setShow] = useState(false);
  const [showQRLoading, setShowQRLoading] = useState(false);
  const [showQR, setShowQR] = useState(false);
  const [showGetInvoiceButton, setShowGetInvoiceButton] = useState(false);
  const [showButtonSpinner, setShowButtonSpinner] = useState(false);
  const [showCheckPaymentButton, setShowCheckPaymentButton] = useState(false);
  const [showWithdrawButton, setShowWithdrawButton] = useState(false);
  const [showScanQRButton, setShowScanQRButton] = useState(true);
  const [showValidateInvoiceButton, setShowValidateInvoiceButton] = useState(false);
  const [showCameraWindow, setShowCameraWindow] = useState(false);
  const [showWithdrawInvoiceAmount, setShowWithdrawInvoiceAmount] = useState(false);
  const [withdrawInvoiceAmount, setWithdrawInvoiceAmount] = useState("");
  const [withdrawId, setWithdrawId] = useState(-1);

  const [copied, setCopied] = useState(false);

  const [footerMessage, setFooterMessage] = useState("Specify deposit amount to deposit and get invoice");
  const [footerBg, setFooterBg] = useState("bg-muted");
  const [depositAmount, setDepositAmount] = useState(1000);
  const [withdrawInvoice, setWithdrawInvoice] = useState("");
  const [depositInvoice, setDepositInvoice] = useState("");
  const [invoiceQRURL, setInvoiceQRURL] = useState("/Content/FFFFFF-0.png");

  const depositInputRef = createRef();  // for the number of sats to deposit
  const withdrawAmountInputRef = createRef();  // for the number of sats to withdraw (from invoice)
  const withdrawInputRef = createRef(); // for the invoice to pay for withdraw
  const invoiceInputRef = createRef();  // for the invoice to pay for deposit
  const cameraWindowRef = createRef();  // for the invoice to pay for deposit
  const userInfo = useUserInfo(); // Custom hook

  const handleClose = () => {
    // Cleanup & reset
    setShow(false);
    setKey("deposit");
    handleSelect("deposit");
  };

  const handleGetInvoice = () => {
    setShowGetInvoiceButton(false);
    setShowCheckPaymentButton(true);
    setShowQRLoading(true);

    var memo = "ZapRead.com deposit";

    // Get an invoice
    postJson("/Lightning/GetDepositInvoice/", {
      amount: depositAmount,
      memo: memo,
      anon: 0,
      use: "userDeposit",
      useId: -1, // undefined
      useAction: -1 // undefined
    }).then((response) => {
      if (response.success) {
        setDepositInvoice(response.Invoice);
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
        setShowGetInvoiceButton(true);
        setShowCheckPaymentButton(false);
        setShowQRLoading(false);
      });
  };

  const handleCheckPayment = () => {
    setShowButtonSpinner(true);
    postJson("/Lightning/CheckPayment/", {
      invoice: depositInvoice,
      isDeposit: true
    })
      .then((response) => {
        setShowButtonSpinner(false);
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
          setShowButtonSpinner(false);
          //alert(response.message);
          setShow(true);
          setFooterMessage(response.message);
          setFooterBg("bg-danger")
        }
      })
      .catch((error) => {
        setShowButtonSpinner(false);
        //document.getElementById(spinElId).style.display = "none";//$("#" + $(e).data('spin-element')).hide();
        //alert(response.message);
        setShow(true);
        setFooterMessage(response.message);
        setFooterBg("bg-danger")
      });
  };

  const handleWithdraw = () => {
    //var withdrawId = document.getElementById("confirmWithdraw").getAttribute("data-withdrawid");
    setShowButtonSpinner(true);
    postJson("/Lightning/SubmitPaymentRequest/", {
      //request: invoice.toString()
      withdrawId: withdrawId
    }).then((response) => {
      setShowButtonSpinner(false);
      //document.getElementById("btnPayLNWithdraw").disabled = false;//$("#btnPayLNWithdraw").removeAttr("disabled");
      //document.getElementById("btnVerifyLNWithdraw").style.display = '';//$('#btnVerifyLNWithdraw').show();
      //document.getElementById("btnPayLNWithdraw").style.display = 'none';//$("#btnPayLNWithdraw").hide();
      //document.getElementById("confirmWithdraw").style.display = 'none';//$('#confirmWithdraw').hide();
      setShowValidateInvoiceButton(false);
      setShowWithdrawButton(false);
      if (response.success) {
        setFooterMessage("Payment successfully sent");
        setFooterBg("bg-success");
        refreshUserBalance();
      }
      else {
        setFooterMessage(response.message);
        setFooterBg("bg-danger");
      }
    }).catch((error) => {
      setShowButtonSpinner(false);
      setFooterMessage("Failed to pay invoice");
      setFooterBg("bg-error");
    });
  };

  // Withdraw
  const handleValidateInvoice = () => {
    setShowButtonSpinner(true);

    postJson("/Lightning/ValidatePaymentRequest/", {
      request: withdrawInvoice
    }).then((response) => {
      setShowButtonSpinner(false);
      if (response.success) {
        setShowWithdrawButton(true);
        setShowValidateInvoiceButton(false);
        setShowWithdrawInvoiceAmount(true);
        //withdrawInputRef.current.value = response.num_satoshis;
        setWithdrawInvoiceAmount(response.num_satoshis);
        setFooterMessage("Verify amount and click Withdraw");
        setWithdrawId(response.withdrawId); // This is the unique ID for making the withdraw
        // This is the response we need to use later
        //document.getElementById("confirmWithdraw").setAttribute("data-withdrawid", response.withdrawId);
        //document.getElementById("lightningInvoiceAmount").value = response.num_satoshis;//$('#lightningInvoiceAmount').val(response.num_satoshis);
        //document.getElementById("confirmWithdraw").style.display = '';//$('#confirmWithdraw').show();
        //document.getElementById("btnPayLNWithdraw").style.display = 'none';//$('#btnPayLNWithdraw').hide();
        //document.getElementById("btnVerifyLNWithdraw").style.display = 'none';//$('#btnVerifyLNWithdraw').hide();
        //document.getElementById("btnPayLNWithdraw").style.display = '';//$('#btnPayLNWithdraw').show();
        //document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-info", "bg-success", "bg-danger");//$("#lightningTransactionInvoiceResult").removeClass("bg-success bg-muted bg-info");
        //document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-muted");//$("#lightningTransactionInvoiceResult").addClass("bg-error");
        //document.getElementById("lightningTransactionInvoiceResult").innerHTML = "Verify amount and click Withdraw";//$("#lightningTransactionInvoiceResult").html("Verify amount and click Withdraw");
        console.log('Withdraw Node:' + response.destination);
      } else {
        setFooterMessage(response.message);
        setFooterBg("bg-danger");
        //Swal.fire("Error", response.message, "error");
        //document.getElementById("lightningTransactionInvoiceResult").innerHTML = response.message;//$("#lightningTransactionInvoiceResult").html(response.Result);
        //document.getElementById("lightningTransactionInvoiceResult").classList.remove("bg-info", "bg-success", "bg-muted");//$("#lightningTransactionInvoiceResult").removeClass("bg-success bg-muted bg-info");
        //document.getElementById("lightningTransactionInvoiceResult").classList.add("bg-danger");//$("#lightningTransactionInvoiceResult").addClass("bg-error");
        //document.getElementById("lightningTransactionInvoiceResult").style.display = '';
      }
    }).catch((error) => {
      console.log("error", error);
      setShowButtonSpinner(false);
      error.json().then(data => setFooterMessage(data.message));
      setFooterBg("bg-danger");
      //Swal.fire("Error", error, "error");
    });
  };

  useEffect(() => {
    if (showCameraWindow) {
      getWebrtcAdapter().then(({ default: adapter }) => {
        getScript('/Scripts/instascan.min.js', function () {
        //getInstascan().then(({ Instascan }) => {
          let scanner = new Instascan.Scanner({
            video: cameraWindowRef.current //document.getElementById('preview')
          });
          scanner.addListener('scan', function (content) {
            console.log(content);
            setWithdrawInvoice(content);
            setShowCameraWindow(false);
            setShowScanQRButton(false);
            scanner.stop();
          });
          Instascan.Camera.getCameras().then(function (cameras) {
            if (cameras.length > 0) {
              scanner.start(cameras[0]);
            } else {
              console.error('No cameras found.');
            }
          }).catch(function (e) {
            console.error(e);
          });
          //setShowCameraWindow(true);
        //});
        }, true);
      });
      //}, true);
    }
  }, [showCameraWindow])

  const handleScanQR = () => {
    setShowCameraWindow(true);
  };

  const handleSelect = (eventKey) => {
    if (eventKey == "deposit") {
      setShowGetInvoiceButton(true);
      setShowCheckPaymentButton(false);
      setFooterMessage("Specify deposit amount to deposit and get invoice");
      setFooterBg("bg-muted");

      // Reset Deposit QR
      setDepositInvoice("");
      setShowQR(false);
      setShowQRLoading(false);
      setShowButtonSpinner(false);
      setShowValidateInvoiceButton(false);
    }
    else {
      setShowGetInvoiceButton(false);
      setShowCheckPaymentButton(false);
      setShowScanQRButton(true);
      setShowWithdrawInvoiceAmount(false);
      setFooterMessage("Paste invoice or scan QR code");
      setFooterBg("bg-muted");
      setShowValidateInvoiceButton(true);
    }
    setKey(eventKey);
  };

  const copyInvoiceToClipboard = () => {
    var inputEl = invoiceInputRef.current;
    inputEl.focus();
    inputEl.select();
    inputEl.setSelectionRange(0, 99999);
    navigator.clipboard
      .writeText(depositInvoice)
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

  const onPaidEventHandler = useCallback((e) => {
    console.log("DepositWithdrawModal onPaidEventHandler", e.detail);
    refreshUserBalance(); // update UI
    setDepositInvoice("");
    handleClose();
  });

  const onDepositWithdrawEventHandler = useCallback((e) => {
    setKey("deposit"); //Initialize
    handleSelect("deposit");
    setShow(true); //Show
  });

  // Register event handler for invoice paid
  useEffect(() => {
    on("zapread:deposit:invoicePaid", onPaidEventHandler);

    return () => {
      off("zapread:deposit:invoicePaid", onPaidEventHandler);
    }
  }, [onPaidEventHandler]);

  // Register event handler for vote click
  useEffect(() => {
    on("zapread:depositwithdraw", onDepositWithdrawEventHandler);

    return () => {
      off("zapread:depositwithdraw", onDepositWithdrawEventHandler);
    }
  }, [onDepositWithdrawEventHandler]);

  return (
    <Modal show={show} onHide={handleClose}>
      <Modal.Header closeButton>
        <Nav fill variant="tabs" defaultActiveKey={key} onSelect={handleSelect}>
          <Nav.Item>
            <Nav.Link eventKey="deposit">Deposit</Nav.Link>
          </Nav.Item>
          <Nav.Item>
            <Nav.Link eventKey="withdraw">Withdraw</Nav.Link>
          </Nav.Item>
        </Nav>
      </Modal.Header>
      <Modal.Body>
        <Card>
          <Card.Body>
            {key == "deposit" ?
              <>
                <Row>
                  <Col xs={6}>
                    <span>Deposit{" "}</span>
                    <input
                      onChange={({ target: { value } }) => setDepositAmount(value)} // Controlled input
                      ref={depositInputRef}
                      type="number"
                      value={depositAmount}
                      min={1}
                      placeholder="Amount"
                      className="form-control font-bold"
                      aria-label="Deposit Amount" />
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
                <a href={"lightning:" + depositInvoice} style={showQR ? {} : { display: "none" }}>
                  <img loading="lazy" src={invoiceQRURL} className="img-fluid" />
                </a>
                <div className="input-group mb-3" style={showQR ? {} : { display: "none" }}>
                  <div className="input-group-prepend">
                    <a href={"lightning:" + depositInvoice} id="lnDepositInvoiceLink" className="btn btn-primary" role="button" aria-pressed="true"><span className="fa fa-bolt"></span></a>
                  </div>
                  <input type="text" id="lightningDepositInvoiceInput"
                    ref={invoiceInputRef}
                    value={depositInvoice}
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
              </>
              :
              <>
                <Row>
                  <Col xs={6}>
                    <span>&nbsp;{" "}</span>
                    <input
                      onChange={({ target: { value } }) => setWithdrawInvoice(value)} // Controlled input
                      ref={withdrawInputRef}
                      value={withdrawInvoice}
                      placeholder="Paste invoice"
                      className="form-control font-bold"
                      aria-label="Withdrawal Invoice" />
                    <small className="text-muted">&nbsp;</small>
                  </Col>
                  <Col xs={6} className="text-right">
                    <span> Balance </span>
                    <h2 className="font-bold">{userInfo.balance}{" "}<i className="fa fa-bolt"></i></h2>
                    <small className="text-muted">Satoshi</small>
                  </Col>
                </Row>
                <div>
                  <video ref={cameraWindowRef} id="preview"
                    style={showCameraWindow ? {} : { display: "none" }}></video>
                  <Button
                    onClick={handleScanQR}
                    style={showScanQRButton ? {} : { display: "none" }}
                    variant="outline-primary" block>
                    Scan QR
                  </Button>
                </div>
                <div style={showWithdrawInvoiceAmount ? {} : { display: "none" }}>
                  <span>Invoice amount:</span>
                  <input ref={withdrawAmountInputRef}
                    onChange={({ target: { value } }) => setWithdrawInvoiceAmount(value)}
                    value={withdrawInvoiceAmount}
                    type="text"
                    readOnly
                    className="form-control" />
                </div>
              </>
            }
          </Card.Body>
          <Card.Footer className={footerBg}> {footerMessage} </Card.Footer>
        </Card>
      </Modal.Body>
      <Modal.Footer>
        {/*Withdraws*/}
        <Button variant="primary"
          onClick={handleValidateInvoice}
          disabled={showButtonSpinner || showQRLoading}
          style={showValidateInvoiceButton ? {} : { display: "none" }}>
          Validate Invoice <i className="fa-solid fa-circle-notch fa-spin" style={showButtonSpinner ? {} : { display: "none" }}></i>
        </Button>
        <Button variant="primary"
          onClick={handleWithdraw}
          disabled={showButtonSpinner || showQRLoading}
          style={showWithdrawButton ? {} : { display: "none" }}>
          Withdraw <i className="fa-solid fa-circle-notch fa-spin" style={showButtonSpinner ? {} : { display: "none" }}></i>
        </Button>
        {/*<button id="btnVerifyLNWithdraw" type="button" class="btn btn-primary" style="display:none" onclick="onValidateInvoice(this);">Validate Invoice</button>*/}
        {/*<button id="btnPayLNWithdraw" type="button" class="btn btn-primary" style="display:none" onclick="onPayInvoice(this);">Withdraw</button>*/}

        {/*Deposits*/}
        <Button variant="info"
          onClick={handleCheckPayment}
          disabled={showButtonSpinner || showQRLoading}
          style={showCheckPaymentButton ? {} : { display: "none" }}>
          Check Payment <i className="fa-solid fa-circle-notch fa-spin" style={showButtonSpinner ? {} : { display: "none" }}></i>
        </Button>
        <Button variant="primary"
          onClick={handleGetInvoice}
          disabled={showQRLoading}
          style={showGetInvoiceButton ? {} : { display: "none" }}>
          Get Invoice
        </Button>
        <Button variant="secondary" onClick={handleClose}>
          Close
        </Button>
      </Modal.Footer>
    </Modal >
  );
}