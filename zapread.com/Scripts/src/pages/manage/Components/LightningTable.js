/**
 * 
 **/

import React, { useCallback, useEffect, useState } from 'react';
import BootstrapTable from 'react-bootstrap-table-next';
import paginationFactory, { PaginationProvider, PaginationTotalStandalone, PaginationListStandalone } from 'react-bootstrap-table2-paginator';
import { Container, Row, Col, DropdownButton, Dropdown, ButtonGroup, Button } from 'react-bootstrap';
import { postJson } from '../../../utility/postData';
import 'react-bootstrap-table-next/dist/react-bootstrap-table2.min.css';
import { format, parseISO } from 'date-fns'

export default function LightningTable(props) {
  const [data, setData] = useState([]);
  const [LNFilter, setLNFilter] = useState("All");
  const [numRecords, setNumRecords] = useState(0);

  const columns = [
    {
      dataField: 'Time', text: 'Time', formatter: (cell, row) => {
        return (<span>{format(new Date(row.Time), 'yyyy-MM-dd HH:mm:ss')}</span>);} },
    { dataField: 'Type', text: 'Type', formatter: typeFormatter },
    { dataField: 'Memo', text: 'Memo' },
    { dataField: 'Amount', text: 'Amount' }];

  function typeFormatter(cell, row) {
    return (
      <span>
        {row.IsSettled && <i className='fa fa-check text-success' title='Completed'></i>}
        {!row.IsSettled && <i className='fa fa-times text-danger' title='Not Completed'></i>}
        {row.IsLimbo && <i className='fa fa-clock text-info' title='In Limbo'></i>}
        {row.Type && <> Deposit</>}
        {!row.Type && <> Withdraw</>}
      </span>
    );
  }

  async function getData() {
    await postJson("/api/v1/account/transactions/lightning/", {
      Start: 0,
      Length: props.pageSize,
      Filter: LNFilter
    }).then((response) => {
      var newData = response.data;
      setData(newData);
      setNumRecords(response.recordsTotal);
    });
  }

  useEffect(() => {
    getData();
  }, [LNFilter]);

  const options = {
    paginationSize: 4,
    sizePerPage: props.pageSize,
    custom: true,
    totalSize: numRecords,
    onPageChange: (page, sizePerPage) => {
      //console.log('Page change!!!');
      //console.log('Newest size per page:' + sizePerPage);
      //console.log('Newest page:' + page);
      postJson("/api/v1/account/transactions/lightning/", {
        Start: (page - 1) * sizePerPage,
        Length: props.pageSize,
        Filter: LNFilter
      }).then((response) => {
        setData(response.data);
        setNumRecords(response.recordsTotal);
      });
    }
  };

  function handleTableChange(type, { page, sizePerPage }) {
    const currentIndex = (page - 1) * sizePerPage;
    // Actually done elsewhere
    console.log('CurrentIndex: ' + currentIndex);
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
              <div>
                <DropdownButton id="dropdown-basic-button" variant="primary" title={LNFilter}>
                  <Dropdown.Item onClick={() => { setLNFilter("All"); }}>All</Dropdown.Item>
                  <Dropdown.Item onClick={() => { setLNFilter("Completed"); }}>Completed</Dropdown.Item>
                  <Dropdown.Item onClick={() => { setLNFilter("Processing"); }}>Processing</Dropdown.Item>
                  <Dropdown.Item onClick={() => { setLNFilter("Failed/Cancelled"); }}>Failed/Cancelled</Dropdown.Item>
                </DropdownButton>
                <PaginationProvider pagination={paginationFactory(options)}>
                  {
                    ({
                      paginationProps,
                      paginationTableProps
                    }) => (
                      <div>
                        <BootstrapTable
                          remote
                          bootstrap4
                          keyField="Id"
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
                    )
                  }
                </PaginationProvider>
              </div>
            </Col>
          </Row>
        </Container>
      </div>
    </div>
  );
}