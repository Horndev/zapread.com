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
        var el1 = document.getElementById(this.props.idvalue);
        if (el1 != null) {
          el1.innerHTML = data.total;
        }
      }
      if (Object.prototype.hasOwnProperty.call(data, "limbo")) {
        var el2 = document.getElementById(this.props.idsecondvalue);
        if (el2 != null) {
          el2.innerHTML = data.limbo;
        }
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
                    <h2 className="font-bold"><span id={this.props.idvalue}>{this.props.value2}</span> {secondvalue} </h2>
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