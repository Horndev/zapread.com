/**
 * 
 **/

import '../../shared/shared';                                           // [✓]
import '../../realtime/signalr';                                        // [✓]
import React, { useCallback, useEffect, useState, useRef } from 'react';           // [✓]
import { Container, Row, Col } from 'react-bootstrap';                  // [✓]
import ReactDOM from 'react-dom';                                       // [✓]
import Swal from 'sweetalert2';                                         // [✓]
import Input from './Components/Input';                                 // [✓]
import Editor from './Components/Editor';                               // [✓]
import DraftsTable from './Components/DraftsTable';                     // [✓]
import { postJson } from '../../utility/postData';                      // [✓]
import PageHeading from '../../components/page-heading';                // [✓]
import '../../shared/sharedlast';                                       // [✓]

function Page() {
    const [postContent, setPostContent] = useState('');
    const [numSaves, setNumSaves] = useState(0);
    const [postId, setPostId] = useState(-1);
    const [groupId, setGroupId] = useState(1);
    const [groupName, setGroupName] = useState('');
    const [postTitle, setPostTitle] = useState('');

    const handleSaveDraft = useCallback(() => {
        console.log({
            postId: postId,
            groupId: groupId,
            content: postContent,
            postTitle: postTitle,
            isDraft: true
        });

        postJson("/Post/Submit/", {
            postId: postId,
            groupId: groupId,
            content: postContent,
            postTitle: postTitle,
            isDraft: true
        }).then((response) => {
            console.log(response);
            setPostId(response.postId);
        });
    }, [postTitle, postContent, postId, groupId]);

    function handleDeleteDraft(postId) {
        Swal.fire({
            title: "Are you sure?",
            text: "This draft will be permanently deleted.",
            icon: "warning",
            showCancelButton: true
        }).then((willDelete) => {
            if (willDelete.value) {
                postJson("/Post/Draft/Load/", {
                    postId: postId,
                }).then((response) => {
                    console.log(response);
                    setValue(response.draftPost.Content);   // set the editor content
                    setPostId(postId);                      // this is the post we are now editing
                    setGroupName(response.draftPost.GroupName);
                    setPostTitle(response.draftPost.PostTitle);
                });
            } else {
                console.log("cancelled load");
            }
        });
    }

    function handleLoadPost(postId) {
        Swal.fire({
            title: "Are you sure?",
            text: "Any unsaved changes in the current post will be lost.",
            icon: "warning",
            showCancelButton: true
        }).then((willLoad) => {
            if (willLoad.value) {
                postJson("/Post/Draft/Load/", {
                    postId: postId,
                }).then((response) => {
                    console.log(response);
                    setValue(response.draftPost.Content);   // set the editor content
                    setPostId(postId);                      // this is the post we are now editing
                    setGroupName(response.draftPost.GroupName);
                    setPostTitle(response.draftPost.PostTitle);
                });

                //var form = document.createElement('form');
                //document.body.appendChild(form);
                //form.method = 'post';
                //form.action = "/Post/Edit/";
                //var data = { 'PostId': postId };
                //for (var name in data) {
                //    var input = document.createElement('input');
                //    input.type = 'hidden';
                //    input.name = name;
                //    input.value = data[name];
                //    form.appendChild(input);
                //}
                //form.submit();
            } else {
                console.log("cancelled load");
            }
        });
    }

    return (
        <div>
            <PageHeading title="New Post" controller="Post" method="Edit" function="New" />
            <div>
                <Row>
                    <Col lg={2}></Col>
                    <Col lg={8}>
                        <div className="ibox-title" style={{ display: "inline-flex", width: "100%", marginTop: "8px"}}>
                            <div className="social-avatar" style={{paddingTop: "5px"}}>
                                <img className="img-circle user-image-45" width="45" height="45" src="/Home/UserImage/?size=45" />
                            </div>
                            <Input
                                id="postTitle"
                                label="Title"
                                value={postTitle}
                                setValue={setPostTitle}
                                predicted=""
                                locked={false}
                                active={false}
                            />
                        </div>
                        <div className="ibox-title">
                            <Input
                                id="groupTitle"
                                label="Group"
                                value={groupName}
                                setValue={setGroupName}
                                predicted="Community"
                                locked={false}
                                active={false}
                            />
                        </div>
                    </Col>
                </Row>
            </div>
            <div className="wrapper wrapper-content">
                <Row>
                    <Col lg={2}></Col>
                    <Col lg={8}>
                        <div className="savingoverlay" id="savingnotification" style={{ display: "none" }}>
                            <i className="fa fa-circle-o-notch fa-spin"></i> saving...
                        </div>
                        <Editor
                            value={postContent}
                            setValue={setPostContent}
                            onSaveDraft={handleSaveDraft}
                            onSubmitPost={() => { console.log('submit post') }}
                        />
                    </Col>
                </Row>
                <Row><Col lg={12}><br /></Col></Row>
                <Row>
                    <Col lg={2}></Col>
                    <Col lg={8}>
                        <DraftsTable
                            title="Your saved drafts"
                            onLoadPost={handleLoadPost}
                            onDeleteDraft={handleDeleteDraft}
                            pageSize={10} />
                    </Col>
                </Row>
                <Row><Col lg={12}><br /></Col></Row>
            </div>
        </div>
    );
}

ReactDOM.render(
    <Page />
    , document.getElementById("root"));


////import '../../shared/shared';
//import $ from 'jquery'; //yuck

//import 'bootstrap';                                             // [X]            // still requires jquery :(
//import 'bootstrap/dist/css/bootstrap.min.css';                  // [✓]
//import 'font-awesome/css/font-awesome.min.css';                 // [✓]
//import '../../utility/ui/paymentsscan';                         // [  ]
//import '../../utility/ui/accountpayments';                      // [  ]
//import '../../shared/postfunctions';                            // [✓]
//import '../../shared/readmore';                                 // [✓]
//import '../../shared/postui';                                   // [✓]
//import '../../shared/topnavbar';                                // [✓]
//import "jquery-ui-dist/jquery-ui";                              // [X]
//import "jquery-ui-dist/jquery-ui.min.css";                      // [X]

//import '../../../summernote/dist/summernote-bs4';
//import 'summernote/dist/summernote-bs4.css';
//import '../../utility/summernote/summernote-video-attributes';
//import 'selectize';
//import 'selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css';
//import 'datatables.net-bs4';
//import 'datatables.net-scroller-bs4';
//import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
//import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
//import '../../utility/editor/posteditor';
//import '../../realtime/signalr';
//import { getAntiForgeryToken } from '../../utility/antiforgery';
//import { sendFile } from '../../utility/sendfile';
//import Swal from 'sweetalert2';
//import '../../shared/sharedlast';

//var knownGroups = [''];

//$(".click2edit").summernote({
//    toolbarContainer: '#editorToolbar',
//    otherStaticBar: '.navbar',
//    callbacks: {
//        onImageUpload: function (files) {
//            sendFile(files[0], this);
//        }
//    },
//    toolbar: [
//        ['style', ['style']],
//        ['font', ['bold', 'italic', 'underline', 'clear', 'strikethrough', 'superscript', 'subscript']],
//        ['fontname', ['fontname']],
//        ['fontsize', ['fontsize']],
//        ['color', ['color']],
//        ['para', ['ul', 'ol', 'paragraph']],
//        ['table', ['table']],
//        ['insert', ['link', 'picture', 'videoAttributes']],
//        ['view', ['fullscreen', 'codeview']]
//    ],
//    focus: true,
//    hint: {
//        match: /\B@@(\w*)$/,
//        search: function (keyword, callback) {
//            if (!keyword.length) return callback();
//            var msg = JSON.stringify({ 'searchstr': keyword.toString() });
//            $.ajax({
//                async: true,
//                url: '/Comment/GetMentions/',
//                type: 'POST',
//                contentType: "application/json; charset=utf-8",
//                dataType: 'json',
//                data: msg,
//                error: function () {
//                    callback();
//                },
//                success: function (res) {
//                    callback(res.users);
//                }
//            });
//        },
//        content: function (item) {
//            return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
//        }
//    }
//});

//var draftsTable = $('#draftsTable').DataTable({
//    "searching": true,
//    "lengthChange": false,
//    "pageLength": 10,
//    "processing": true,
//    "serverSide": true,
//    "ajax": {
//        type: "POST",
//        contentType: "application/json",
//        url: "/Post/GetDrafts/",
//        headers: getAntiForgeryToken(),
//        data: function (d) {
//            return JSON.stringify(d);
//        }
//    },
//    "columns": [
//        { "data": "Title", "orderable": true },
//        {
//            "data": null,
//            "orderable": true,
//            "mRender": function (data, type, row) {
//                return "<a href='/Group/GroupDetail/" + data.GroupId + "'>" + data.Group + "</a>";
//            }
//        },
//        { "data": "Time", "orderable": false },
//        {
//            "data": null,//"Type",
//            "orderable": false,
//            "mRender": function (data, type, row) {
//                return "<button class='btn btn-sm btn-primary' onclick=loadpost(" + data.PostId + ")>Load</button> <button class='btn btn-sm btn-danger' onclick=del(" + data.PostId + ")>Delete</button>"//"<a href='" + data.URL + "'>" + data.Type + "</a>";
//            }
//        }
//    ]
//});

//$("#postGroup").autocomplete({
//    autoFocus: true,
//    source: function (request, response) {
//        $.ajax({
//            async: true,
//            url: "/Group/GetGroups/" + request.term,
//            type: "GET",
//            dataType: "json",
//            //data: { prefix: request.term },
//            success: function (data) {
//                knownGroups = data;
//                response($.map(data, function (item) {
//                    return { label: item.GroupName, value: item.GroupName };
//                }));
//            }
//        });
//    },
//    select: function (event, ui) {
//        // if user clicked
//    },
//    change: function (event, ui) {
//        var gn = $("#postGroup").val();
//        if (typeof knownGroups === 'undefined' || knownGroups.length === 0) {
//            // variable is undefined
//            $("#postGroup").addClass('is-invalid');
//        }
//        else {
//            if (knownGroups.findIndex(function (i) { return i.GroupName === gn; }) >= 0) {
//                $("#postGroup").removeClass('is-invalid');
//                gid = knownGroups[knownGroups.findIndex(function (i) { return i.GroupName === gn; })].GroupId;
//                $('#postGroupActive').html(gn);
//                $('#groupLink').html(gn);
//                $('#groupLink').attr('href', '@Url.Action("GroupDetail", "Group")' + '?id=' + gid.toString());
//            }
//            else {
//                $("#postGroup").addClass('is-invalid');
//            }
//        }
//    }
//});