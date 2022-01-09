/**
 * 
 **/

import React, { useCallback, useEffect, useState } from 'react';
import BootstrapTable from 'react-bootstrap-table-next';
import paginationFactory, { PaginationProvider, PaginationTotalStandalone, PaginationListStandalone } from 'react-bootstrap-table2-paginator';
import { Container, Row, Col, ButtonGroup, Button } from 'react-bootstrap';
import { postJson } from '../../../utility/postData';

import 'react-bootstrap-table-next/dist/react-bootstrap-table2.min.css';

export default function LightningTable(props) {
    const [data, setData] = useState([]);
    const [numRecords, setNumRecords] = useState(0);

    const columns = [
        { dataField: 'Time', text: 'Time' },
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

    useEffect(() => {
        async function getData() {
            await postJson("/api/v1/account/transactions/lightning/", {
                Start: 0,
                Length: props.pageSize,
            }).then((response) => {
                var newData = response.data;
                setData(newData);
                setNumRecords(response.recordsTotal);
            });
        }

        getData();
    }, []);
    
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
                Start: (page-1) * sizePerPage,
                Length: props.pageSize,
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
                                <PaginationProvider pagination={paginationFactory(options)}>
                                    {
                                        ({
                                            paginationProps,
                                            paginationTableProps
                                        }) => (
                                                <div>
                                                    <BootstrapTable
                                                        remote
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