/**
 * 
 **/

import React, { useCallback, useEffect, useState } from 'react';
import BootstrapTable from 'react-bootstrap-table-next';
import paginationFactory, { PaginationProvider, PaginationTotalStandalone, PaginationListStandalone } from 'react-bootstrap-table2-paginator';
import { Container, Row, Col, ButtonGroup, Button } from 'react-bootstrap';
import { postJson } from '../../../utility/postData';
import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';

import 'react-bootstrap-table-next/dist/react-bootstrap-table2.min.css';

export default function DraftsTable(props) {
    const [data, setData] = useState([]);
    const [numRecords, setNumRecords] = useState(0);

    const dataURL = "/Post/GetDrafts/";

    const columns = [
        { dataField: 'PostTitle', text: 'Title', formatter: titleFormatter },
        { dataField: 'TimeStamp', text: 'Last Modified', formatter: timeFormatter },
        { dataField: 'PostId', text: 'Action', formatter: actionFormatter }];

    function timeFormatter(cell, row) {
        var datefn = parseISO(row.TimeStamp);
        datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
        var time = formatDistanceToNow(datefn, { addSuffix: true });
        return (
            <>
                { time }
            </>
        );
    }

    function titleFormatter(cell, row) {
        return (
            <>
                { row.PostTitle }
            </>
        );
    }

    function actionFormatter(cell, row) {
        return (
            //if (data.Memo !== "") {
            //    return "<a href='" + data.URL + "'>" + data.Type + " (" + data.Memo + ")" + "</a>";
            //}
            //return data.Type;
            <div>
                <Button
                    variant="outline-primary"
                    onClick={() => { props.onLoadPost(row.PostId) }}
                >
                    <i className="fa fa-arrow-circle-up"></i>
                </Button>{' '}
                <Button
                    variant="outline-danger"
                    onClick={() => { props.onDeleteDraft(row.PostId) }}
                >
                    <i className="fa fa-trash"></i></Button>
            </div>
        );
    }

    useEffect(() => {
        async function getData() {
            await postJson(dataURL, {
                Start: 0,
                Length: props.pageSize,
            }).then((response) => {
                var newData = response.data;
                setData(newData);
                setNumRecords(response.recordsTotal);
            });
        }

        getData();
    }, []); // only on load

    useEffect(() => {
        async function getData() {
            await postJson(dataURL, {
                Start: 0,
                Length: props.pageSize,
            }).then((response) => {
                var newData = response.data;
                setData(newData);
                setNumRecords(response.recordsTotal);
            });
        }

        getData();
    }, [props.numSaves]); // when numSaves changes

    const options = {
        paginationSize: 4,
        sizePerPage: props.pageSize,
        custom: true,
        totalSize: numRecords,
        onPageChange: (page, sizePerPage) => {
            postJson(dataURL, {
                Start: (page - 1) * sizePerPage,
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
                                                        keyField="PostId"
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