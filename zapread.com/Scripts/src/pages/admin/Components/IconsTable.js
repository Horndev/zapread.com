/**
 *
 **/

import React, { useCallback, useEffect, useState, useRef } from "react";
import BootstrapTable from "react-bootstrap-table-next";
import paginationFactory, {
  PaginationProvider,
  PaginationTotalStandalone,
  PaginationListStandalone
} from "react-bootstrap-table2-paginator";
import { Container, Row, Col, ButtonGroup, Button } from "react-bootstrap";
import { postJson } from "../../../utility/postData";
import { getAntiForgeryToken } from "../../../utility/antiforgery";
import "react-bootstrap-table-next/dist/react-bootstrap-table2.min.css";

export default function IconsTable(props) {
  const [data, setData] = useState([]);
  const [numRecords, setNumRecords] = useState(0);
  const [updateImgId, setUpdateImgId] = useState(0);

  const inputFile = useRef(null);

  const dataURL = "/Admin/Group/Icons/List/";

  const columns = [
    { dataField: "IconId", text: "Image", formatter: iconFormatter },
    { dataField: "Icon", text: "FA-Icon" },
    { dataField: "GroupName", text: "Group" },
    { dataField: "GroupId", text: "Action", formatter: actionFormatter }
  ];

  function iconFormatter(cell, row) {
    return (
      <>
        <img src={`/Img/Group/Icon/${row.GroupId}/`} />
      </>
    );
  }

  function actionFormatter(cell, row) {
    return (
      <>
        <Button
          variant="outline-primary"
          onClick={e => updateIcon(row.GroupId, e)}
        >
          Update Icon
        </Button>{" "}
        <Button
          variant="outline-warning"
          onClick={e => updateIcon(row.GroupId, e)}
        >
          Toggle NSFW
        </Button>{" "}
        <Button
          variant="outline-danger"
          onClick={e => removeIcon(row.GroupId, e)}
        >
          Remove Icon
        </Button>{" "}
      </>
    );
  }

  function updateIcon(id, e) {
    console.log(e);
    console.log(id);
    setUpdateImgId(id);
    inputFile.current.click();
  }

  function removeIcon(id, e) {
    console.log(e);
    console.log(id);
    setUpdateImgId(id);
    inputFile.current.click();
  }

  useEffect(() => {
    async function getData() {
      await postJson(dataURL, {
        Start: 0,
        Length: props.pageSize
      }).then(response => {
        var newData = response.data;
        setData(newData);
        setNumRecords(response.recordsTotal);
      });
    }

    getData();
  }, []);

  const options = {
    paginationSize: 10,
    sizePerPage: props.pageSize,
    custom: true,
    totalSize: numRecords,
    onPageChange: (page, sizePerPage) => {
      postJson(dataURL, {
        Start: (page - 1) * sizePerPage,
        Length: props.pageSize
      }).then(response => {
        setData(response.data);
        setNumRecords(response.recordsTotal);
      });
    }
  };

  function handleTableChange(type, { page, sizePerPage }) {
    const currentIndex = (page - 1) * sizePerPage;
    // Actually done elsewhere
    console.log("CurrentIndex: " + currentIndex);
  }

  function handleFileChange(selectorFiles) {
    //console.log("handleFileChange");
    var file = selectorFiles[0];

    var fd = new FormData();
    fd.append("file", file);
    const xhr = new XMLHttpRequest();

    // updateImgId is from the react state
    xhr.open("POST", "/Img/Group/Icon/" + updateImgId + "/", true);
    var headers = getAntiForgeryToken();

    for (var index in headers) {
      xhr.setRequestHeader(index, headers[index]);
    }

    // listen callback
    xhr.onload = () => {
      if (xhr.status === 200) {
        var data = JSON.parse(xhr.responseText);
        console.log(data.imgId);
      }
    };

    xhr.send(fd);

    console.log(selectorFiles);
    console.log(updateImgId);
  }

  return (
    <div className="ibox float-e-margins">
      <div className="ibox-title">
        <h5>{props.title}</h5>
      </div>
      <div className="ibox-content">
        <Container fluid="md">
          <Row>
            <Col lg={12}>
              <input
                type="file"
                id="file"
                ref={inputFile}
                accept="image/*"
                onChange={e => handleFileChange(e.target.files)}
                style={{ display: "none" }}
              />
              <div>
                <PaginationProvider pagination={paginationFactory(options)}>
                  {({ paginationProps, paginationTableProps }) => (
                    <div>
                      <BootstrapTable
                        remote
                        keyField="GroupId"
                        data={data}
                        columns={columns}
                        onTableChange={handleTableChange}
                        {...paginationTableProps}
                      />
                      <Row>
                        <Col sm={6}>
                          <PaginationTotalStandalone {...paginationProps} />
                        </Col>
                        <Col sm={6}>
                          <div style={{ float: "right" }}>
                            <PaginationListStandalone {...paginationProps} />
                          </div>
                        </Col>
                      </Row>
                    </div>
                  )}
                </PaginationProvider>
              </div>
            </Col>
          </Row>
        </Container>
      </div>
    </div>
  );
}
