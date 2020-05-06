/**
 * 
 **/

import React, { useCallback, useEffect, useState } from 'react';
import BootstrapTable from 'react-bootstrap-table-next';
import paginationFactory, { PaginationProvider, PaginationTotalStandalone, PaginationListStandalone } from 'react-bootstrap-table2-paginator';
import { Container, Row, Col, ButtonGroup, Button } from 'react-bootstrap';
import { postJson } from '../../../utility/postData';
import 'react-bootstrap-table-next/dist/react-bootstrap-table2.min.css';


export default function ChatsTable(props) {
    const [data, setData] = useState([]);
    const [numRecords, setNumRecords] = useState(0);

    const dataURL = "/Messages/GetChatsTable/";

    const columns = [
        { dataField: 'From', text: 'From', formatter: fromFormatter },
        { dataField: 'LastMessage', text: 'Last Activity' },
        //{ dataField: 'IsRead', text: 'Status' },
        { dataField: 'Status', text: 'Conversation', formatter: conversationFormatter }];

    function conversationFormatter(cell, row) {
        return (
            <span>{row.IsRead}, {row.Status} </span>
        );
    }

    function fromFormatter(cell, row) {
        return (
            //"<div style='display:inline-block;white-space: nowrap;'>
            //<img class='img-circle' src='/Home/UserImage/?UserID=" + encodeURIComponent(data.FromID) + "&size=30' />
            //<a target='_blank' href='/user/" + encodeURIComponent(data.From) + "/''> " + data.From + "</a></div>";

            //   /user/${encodeURIComponent(row.From)}/

            //  /Messages/Chat/" + encodeURIComponent(data.From) + "/
            <div style={{
                display: "inline-block",
                whiteSpace: "nowrap"
            }}>
                <a className="post-username" target="_blank" href={`/Messages/Chat/${row.From}/`}>
                    <img
                        className="img-circle"
                        style={{paddingRight: "10px"}}
                        src={`/Home/UserImage/?UserID=${encodeURIComponent(row.FromID)}&size=30`}
                    />
                {row.From}</a>
            </div>
        );
    }

    const rowEvents = {
        onClick: (e, row, rowIndex) => {
            var win = window.open(`/Messages/Chat/${encodeURIComponent(row.From)}/`, '_blank');
            win.focus();
            //console.log(e);
            //console.log(row);
            //console.log(`clicked on row with index: ${rowIndex}`);
        }//,
        //onMouseEnter: (e, row, rowIndex) => {
        //    console.log(`enter on row with index: ${rowIndex}`);
        //}
    };

    const rowClasses = (row, rowIndex) => {
        let classes = null;

        //console.log(row);

        if (row.IsRead !== "Read") {
            classes = 'table-success';
        }

        return classes;
    };

    async function getData(page, sizePerPage) {
        await postJson(dataURL, {
            Start: (page - 1) * sizePerPage,
            Length: props.pageSize,
            Columns: [{ Name: "LastMessage" }, { Name: "From" }],
            Order: [{ Column: 0, Dir: "dsc" }] //asc
        }).then((response) => {
            setData(response.data);
            setNumRecords(response.recordsTotal);
        });
    }

    useEffect(() => {
        getData(1, props.pageSize);
    }, []);

    const options = {
        paginationSize: 4,
        sizePerPage: props.pageSize,
        custom: true,
        totalSize: numRecords,
        onPageChange: (page, sizePerPage) => {
            getData(page, sizePerPage)
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
                                                        rowEvents={rowEvents} 
                                                        rowClasses={rowClasses}
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
