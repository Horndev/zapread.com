

import React, { useEffect, useState } from "react";
import { Row, Col } from "react-bootstrap";
import { getJson } from '../../../../utility/getData';
import PlotlyChart from "../../../../Components/PlotlyChart";

export default function Stats(props) {
  const [postData, setPostData] = useState({ x: [], y: [] });
  const [commentData, setCommentData] = useState({ x: [], y: [] });
  const [spentData, setSpentData] = useState({ x: [], y: [] });

  function formatDate(date) {
    var d = new Date(date),
      month = '' + (d.getMonth() + 1),
      day = '' + d.getDate(),
      year = d.getFullYear();

    if (month.length < 2)
      month = '0' + month;
    if (day.length < 2)
      day = '0' + day;

    return [year, month, day].join('-');
  }

  useEffect(() => {
    getJson('/Admin/GetPostStats/').then((response) => {
      if (response.success) {
        var data0x = [];
        var data0y = [];
        var data1x = [];
        var data1y = [];
        var data2x = [];
        var data2y = [];

        response.postStats.forEach(function (s) {
          data0y.push(s.Count);
          data0x.push(formatDate(new Date(s.TimeStampUtc )));
        });

        response.commentStats.forEach(function (s) {
          data1y.push(s.Count);
          data1x.push(formatDate(new Date(s.TimeStampUtc )));
        });

        response.spendingStats.forEach(function (s) {
          data2y.push(s.Count);
          data2x.push(formatDate(new Date(s.TimeStampUtc )));
        });

        console.log({ x: data0x, y: data0y });

        setPostData({ x: data0x, y: data0y });
        setCommentData({ x: data1x, y: data1y });
        setSpentData({ x: data2x, y: data2y });
      } else {
        console.log(response);
      }
    });
  }, []); // Fire once

  var trace1 = {
    name: 'Spent',
    x: spentData.x,
    y: spentData.y,
    type: 'scatter',
    fill: 'tozeroy',
    yaxis: 'y2',
    marker: {
      color: '#464f8833'
    }
  };

  var trace2 = {
    name: 'Posts',
    x: postData.x,
    y: postData.y,
    width: 0.8 * 1000 * 3600 * 24, //milliseconds
    type: 'bar',
    marker: {
      color: '#1ab39455'
    }
  };

  var trace3 = {
    name: 'Comments',
    x: commentData.x,
    y: commentData.y,
    width: 0.3 * 1000 * 3600 * 24, //milliseconds
    type: 'bar',
    marker: {
      color: '#55000055'
    }
  };

  var layout = {
    title: '',
    barmode: 'overlay',
    xaxis: { type: "date" },
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
      <Row className="stats-header">
        <Col className="text-center wow animate__fadeIn">
          <div className="navy-line"></div>
          <h1>Statistics Previous 30 Days</h1>
        </Col>
      </Row>
      <Row>
        <Col className="text-center">
          <div className="stats-plot wow animate__fadeIn">
            <PlotlyChart data={data} layout={layout} />
          </div>
        </Col>
      </Row>
    </section>)
}