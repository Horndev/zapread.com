

import React, { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";
import PlotlyChart from "../../../../Components/PlotlyChart";

export default function Stats(props) {

  var trace1 = {
    name: 'Sats',
    x: [0, 1, 2, 3, 4, 5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29],
    y: [1550, 1233, 1411, 10, 800, 211, 2321, 55, 99, 23, 1550, 1233, 1411, 10, 800, 211, 2321, 55, 99, 23, 1550, 1233, 1411, 10, 800, 211, 2321, 55, 99, 23],
    type: 'scatter',
    fill: 'tozeroy',
    yaxis: 'y2',
  };

  var trace2 = {
    name: 'Posts',
    x: [0, 1, 2, 3, 4, 5],
    y: [1, 8, 4, 23, 12, 19],
    width: 0.8,
    type: 'bar'
  };

  var trace3 = {
    name: 'Comments',
    x: [0, 1, 2, 3, 4, 5],
    y: [1, 2, 1, 2, 3, 4],
    width: [0.2,0.2,0.2,0.2,0.2,0.2],
    type: 'bar'
  };

  var layout = {
    title: '',
    barmode: 'overlay',
    yaxis: { title: 'Comments and Posts' },
    yaxis2: {
      title: 'Sats spent',
      titlefont: { color: 'rgb(148, 103, 189)' },
      tickfont: { color: 'rgb(148, 103, 189)' },
      overlaying: 'y',
      side: 'right'
    }
  };

  var data = [trace1, trace2, trace3];

  return (
    <section id="stats" className="text-left-img-right">
      <Row className="mission-header">
        <Col className="text-center">
          <div className="navy-line"></div>
          <h1>Statistics Previous 30 Days</h1>
        </Col>
      </Row>
      <Row>
        <Col className="text-center">
          <PlotlyChart data={data} layout={layout}/>
        </Col>
      </Row>
    </section>)
}