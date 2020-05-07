/**
 * 
 **/

import React from 'react';
import { Container, Row, Col, ButtonGroup, Button } from 'react-bootstrap';
import { getAntiForgeryTokenValue } from '../../../utility/antiforgery';

class FinancialSummaryBox extends React.Component {
    constructor(props) {
        super(props);
        this.loadData(7) // Initialize
    }

    loadData(d) {
        var url = this.props.dataUrl + d.toString();
        console.log("loading: " + url);
        fetch(url, {
            method: 'GET', // *GET, POST, PUT, DELETE, etc.
            mode: 'same-origin', // no-cors, *cors, same-origin
            cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
            credentials: 'same-origin', // include, *same-origin, omit
            headers: {
                'Content-Type': 'application/json',
                '__RequestVerificationToken': getAntiForgeryTokenValue()
            }
        }).then((response) => {
            return response.json();
        }).then((data) => {
            document.getElementById(this.props.idsummary).innerHTML = data.value;
            if (Object.prototype.hasOwnProperty.call(data, "total")) {
                document.getElementById(this.props.idvalue).innerHTML = data.total;
            }
            if (Object.prototype.hasOwnProperty.call(data, "limbo")) {
                document.getElementById(this.props.idsecondvalue).innerHTML = data.limbo;
            }
        });
    }

    handleClick(event, d) {
        document.getElementById(this.props.idprefix + '1').classList.remove("active");
        document.getElementById(this.props.idprefix + '7').classList.remove("active");
        document.getElementById(this.props.idprefix + '30').classList.remove("active");
        document.getElementById(this.props.idprefix + '365').classList.remove("active");
        event.target.classList.add("active");
        this.loadData(d);
    }

    render() {
        let secondvalue = <span></span>;
        if (this.props.secondvalue) {
            secondvalue = <span>(<span title={this.props.titlesecondvalue} id={this.props.idsecondvalue}>...</span>)</span>
        }
        return (
            <div className="ibox float-e-margins">
                <div className="ibox-title">
                    <h5>{this.props.title}</h5>
                </div>
                <div className="ibox-content">
                    <Container>
                        <Row>
                            <Col lg={12}>
                                <div>
                                    <ButtonGroup>
                                        <Button id={this.props.idprefix + '1'} size="sm"
                                            className="btn-white" variant="light"
                                            onClick={(e) => this.handleClick(e, 1)}>Daily</Button>
                                        <Button id={this.props.idprefix + '7'} size="sm"
                                            className="btn-white active" variant="light"
                                            onClick={(e) => this.handleClick(e, 7)}>Weekly</Button>
                                        <Button id={this.props.idprefix + '30'} size="sm"
                                            className="btn-white" variant="light"
                                            onClick={(e) => this.handleClick(e, 30)}>Monthly</Button>
                                        <Button id={this.props.idprefix + '365'} size="sm"
                                            className="btn-white" variant="light"
                                            onClick={(e) => this.handleClick(e, 365)}>Annual</Button>
                                    </ButtonGroup>
                                </div>
                                <Row>
                                    <Col xs={5}>
                                        <span>{this.props.subtitle}</span>
                                        <h2 className="font-bold"><span id={this.props.idsummary}>{this.props.summaryValue}</span></h2>
                                        <small className="text-muted">{this.props.units}</small>
                                    </Col>
                                    <Col xs={5}>
                                        <span>{this.props.subtitle2}</span>
                                        <h2 className="font-bold"><span id={this.props.idvalue}>...</span> {secondvalue} </h2>
                                        <small className="text-muted">{this.props.units}</small>
                                    </Col>
                                    <Col xs={2}>
                                        <i className={this.props.iconclass} style={this.props.iconstyle}></i>
                                    </Col>
                                </Row>
                            </Col>
                        </Row>
                    </Container>
                </div>
            </div>
        );
    }
}

export default FinancialSummaryBox;

//<div className="col-lg-10">
//    <br /><h2>{this.props.title}</h2>
//    <ol className="breadcrumb">
//        <li className="breadcrumb-item"><a href="/">{this.props.controller}</a></li>
//        <li className="breadcrumb-item"><a href="/">{this.props.method}</a></li>
//        <li className="breadcrumb-item active">{this.props.function}</li>
//    </ol>
//</div>

//<div class="ibox float-e-margins">
//    <div class="ibox-title">
//        <h5>Lightning Network Transactions</h5>
//        <div class="ibox-tools">
//            <a class="dropdown-toggle" data-toggle="dropdown" href="#">
//                <i class="fa fa-wrench"></i>
//            </a>
//            <a class="collapse-link">
//                <i class="fa fa-chevron-up"></i>
//            </a>
//        </div>
//    </div>
//    <div class="ibox-content">
//        <div>
//            <div class="row">
//                <div class="col-lg-12">
//                    <div class="">
//                        <div class="btn-group">
//                            <button id="ln1" type="button" class="btn btn-sm btn-white" onclick="LNSummary(1, this)">Daily</button>
//                            <button id="ln7" type="button" class="btn btn-sm btn-white active" onclick="LNSummary(7, this)">Weekly</button>
//                            <button id="ln30" type="button" class="btn btn-sm btn-white" onclick="LNSummary(30, this)">Monthly</button>
//                            <button id="ln365" type="button" class="btn btn-sm btn-white" onclick="LNSummary(365, this)">Annual</button>
//                        </div>
//                    </div>
//                    <div class="row">
//                        <div class="col-5">
//                            <span> Net Flow </span>
//                            <h2 class="font-bold"><span id="lightningFlowBalance">0</span> </h2>
//                            <small class="text-muted">Satoshi</small>
//                        </div>
//                        <div class="col-5 text-right">
//                            <span> Balance </span>
//                            <h2 class="font-bold"><span class="userBalanceValue" id="userVoteBalance">...</span> (<span title="Limbo Balance" class="userBalanceLimboValue">...</span>) </h2>
//                            <small class="text-muted">Satoshi</small>
//                        </div>
//                        <div class="col-2">
//                            <i class="fa fa-bolt fa-5x"></i>
//                        </div>
//                    </div>
//                    <br />
//                </div>
//            </div>
//        </div>
//    </div>
//</div>