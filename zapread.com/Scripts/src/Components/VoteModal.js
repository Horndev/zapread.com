/**
 * Modal for user voting
 */

import * as bsn from 'bootstrap.native/dist/bootstrap-native-v4';               // [✓]
import React, { useCallback, useEffect, useState, createRef } from "react";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";

import { onVote, onCancelVote } from "../utility/ui/vote";
import { checkInvoicePaid } from "../utility/ui/accountpayments";

export default function VoteModal(props) {
  const [isInitialized, setIsInitialized] = useState(false);
  const [qrURL, setQrURL] = useState("");

  const checkPaidButtonRef = createRef();
  const voteButtonRef = createRef();
  const voteCancelButtonRef = createRef();

  // Monitor for changes in props
  useEffect(
    () => {
      if (!isInitialized) {
        setQrURL("/Img/QR?qr=" + encodeURIComponent("02cda8c01b2303e91bec74c43093d5f1c4fd42a95671ae27bf853d7dfea9b78c06@lightning.zapread.com:9735"));

        // This initializes the modals (done manually using bootstrap.native)
        var bsnModalInit = new bsn.Modal("#voteModal");
      }
      setIsInitialized(true);
    },
    []
  );

  return (
    <>
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

                  <div className="form-group mb-3">
                    <div className="input-group mb-3" id="depositMemoValue">
                    </div>
                  </div>

                  <div id="voteQRloading" style={{ display: "none"}}>
                    <div className="sk-loading" style={{BorderStyle: "none"}}>
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
                    {/*<div className="col-md-2 pull-right">*/}
                    {/*  <button type="button" className="btn btn-primary" data-toggle="modal" data-target=".vote-modal">*/}
                    {/*    <i className="fa fa-qrcode"></i>*/}
                    {/*  </button>*/}

                    {/*  <div id="voteNodeModal" className="modal fade vote-modal" tabIndex="-1" role="dialog" aria-labelledby="mySmallModalLabel" aria-hidden="true">*/}
                    {/*    <div className="modal-dialog modal-lg">*/}
                    {/*      <div className="modal-content">*/}
                    {/*        <img width="300" height="300" loading="lazy" src={qrURL} className="img-fluid" />*/}
                    {/*        <br />*/}
                    {/*        <textarea className="form-control" value="" readOnly="readonly" rows="3">02cda8c01b2303e91bec74c43093d5f1c4fd42a95671ae27bf853d7dfea9b78c06@lightning.zapread.com:9735</textarea>*/}
                    {/*      </div>*/}
                    {/*    </div>*/}
                    {/*  </div>*/}
                    {/*</div>*/}
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
                Check Payment <i id="spinCheckPaymentVote" className="fa fa-circle-o-notch fa-spin" style={{ display: "none" }}></i>
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
