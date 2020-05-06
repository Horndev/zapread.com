/**
 *  List of chats going on with user
 */

import '../../shared/shared';                                               // [✓]
import '../../realtime/signalr';                                            // [✓]
import React, { useCallback, useEffect, useState } from 'react';            // [✓]
import { Container, Row, Col } from 'react-bootstrap';                      // [✓]
import ReactDOM from 'react-dom';                                           // [✓]
import PageHeading from '../../components/page-heading';                    // [✓]
import ChatsTable from './Components/ChatsTable';                           // [✓]

//import 'datatables.net-bs4';
//import 'datatables.net-scroller-bs4';
//import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
//import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';

//import { getAntiForgeryToken } from '../../utility/antiforgery';
import '../../shared/sharedlast';

function Page() {

    return (
        <div>
            <PageHeading title="Ongoing Chats" controller="Messages" method="Chats" function="List" />
            <div><Row><Col lg={12}><br /></Col></Row></div>
            <div className="wrapper wrapper-content">
                <div>
                    <Row>
                        <Col lg={12}>
                            <ChatsTable title="Chats" pageSize={10} />
                        </Col>
                    </Row>
                </div>
            </div>
        </div>
    )
}

ReactDOM.render(
    <Page />
    , document.getElementById("root"));


//var usersTable = {};
//$(document).ready(function () {
//    // Table
//    usersTable = $('#usersTable').DataTable({
//        "searching": false,
//        "bInfo": false,
//        "lengthChange": false,
//        "ordering": true,
//        "order": [[1, "desc"]],
//        "pageLength": 25,
//        "processing": true,
//        "serverSide": true,
//        "ajax": {
//            type: "POST",
//            contentType: "application/json",
//            url: "/Messages/GetChatsTable/",
//            headers: getAntiForgeryToken(),
//            data: function (d) {
//                return JSON.stringify(d);
//            }
//        },
//        "columns": [
//            {
//                "data": null,
//                "name": 'From',
//                "orderable": true,
//                "mRender": function (data, type, row) {
//                    return "<div style='display:inline-block;white-space: nowrap;'><img class='img-circle' src='/Home/UserImage/?UserID=" + encodeURIComponent(data.FromID) + "&size=30'/><a target='_blank' href='/user/" + encodeURIComponent(data.From) + "/''> " + data.From + "</a></div>";
//                }
//            },
//            {
//                "data": null,
//                "name": 'Status',
//                "orderable": true,
//                "mRender": function (data, type, row) {
//                    if (data.IsRead === "Read") {
//                        return "<span class='badge badge-primary' >" + data.IsRead + "</span>";
//                    }
//                    if (data.IsRead === "Unread") {
//                        return "<span class='badge badge-warning' >" + data.IsRead + "</span>";
//                    }
//                    return data.Status;
//                }
//            },
//            {
//                "data": null,
//                "name": 'Conversation',
//                "orderable": true,
//                "mRender": function (data, type, row) {
//                    if (data.Status === "Replied") {
//                        return "<span class='badge badge-primary' >" + data.Status + "</span>";
//                    }
//                    if (data.Status === "Waiting") {
//                        return "<span class='badge badge-warning' >" + data.Status + "</span>";
//                    }
//                    return data.Status;
//                }
//            },
//            { "data": "LastMessage", "orderable": true, "name": "LastMessage", "type": "date", "orderSequence": ["desc", "asc"] },
//            {
//                "data": null,
//                "name": 'Action',
//                "orderable": false,
//                "mRender": function (data, type, row) {
//                    return "<a href='/Messages/Chat/" + encodeURIComponent(data.From) + "/' class='btn btn-primary btn-outline'>Go to Chat</a>";
//                }
//            }
//        ]
//    });
//});