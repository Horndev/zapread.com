/**
 * 
 **/

import React, { useCallback, useEffect, useState } from 'react';
import { Container, Row, Col, ButtonGroup, Button } from 'react-bootstrap';
import { postJson } from '../../../utility/postData';

export default function ChatsTable(props) {
  const [data, setData] = useState([]);
  const [numRecords, setNumRecords] = useState(0);

  //async function getData(page, sizePerPage) {
  //    await postJson(dataURL, {
  //        Start: (page - 1) * sizePerPage,
  //        Length: props.pageSize,
  //        Columns: [{ Name: "LastMessage" }, { Name: "From" }],
  //        Order: [{ Column: 0, Dir: "dsc" }] //asc
  //    }).then((response) => {
  //        setData(response.data);
  //        setNumRecords(response.recordsTotal);
  //    });
  //}

  useEffect(() => {
    //getData(1, props.pageSize);
  }, []);

  return (
    <div className="ibox float-e-margins">
      <div className="ibox-title">
        <h5>{props.title}</h5>
      </div>
      <div className="ibox-content">
        <Container fluid="md">
          <Row>
            <Col lg={12}>


            </Col>
          </Row>
        </Container>
      </div>
    </div>
  );
}
