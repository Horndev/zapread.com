/*
 * 
 */

//import '../../shared/shared';
import $ from 'jquery';

import 'bootstrap';                                                         // still requires jquery :(
import 'bootstrap/dist/css/bootstrap.min.css';                 // [✓]
import 'font-awesome/css/font-awesome.min.css';                // [✓]
import '../../utility/ui/paymentsscan';                        // [  ]
import '../../utility/ui/accountpayments';                     // [  ]
import '../../shared/postfunctions';                           // [✓]
import '../../shared/readmore';                                // [✓]
import '../../shared/postui';                                  // [✓]
import '../../shared/topnavbar';                               // [✓]
import "jquery-ui-dist/jquery-ui";                              // [X]
import "jquery-ui-dist/jquery-ui.min.css";                      // [X]
import '../../../summernote/dist/summernote-bs4';
import 'summernote/dist/summernote-bs4.css';
import '../../utility/summernote/summernote-video-attributes';
import 'selectize';
import 'selectize-bootstrap4-theme/dist/css/selectize.bootstrap4.css';
import 'datatables.net-bs4';
import 'datatables.net-scroller-bs4';
import 'datatables.net-bs4/css/dataTables.bootstrap4.min.css';
import 'datatables.net-scroller-bs4/css/scroller.bootstrap4.min.css';
import '../../utility/editor/posteditor';
import '../../realtime/signalr';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { sendFile } from '../../utility/sendfile';
import Swal from 'sweetalert2';



import '../../shared/sharedlast';

var knownGroups = [''];

$(".click2edit").summernote({
    toolbarContainer: '#editorToolbar',
    otherStaticBar: '.navbar',
    callbacks: {
        onImageUpload: function (files) {
            sendFile(files[0], this);
        }
    },
    toolbar: [
        ['style', ['style']],
        ['font', ['bold', 'italic', 'underline', 'clear', 'strikethrough', 'superscript', 'subscript']],
        ['fontname', ['fontname']],
        ['fontsize', ['fontsize']],
        ['color', ['color']],
        ['para', ['ul', 'ol', 'paragraph']],
        ['table', ['table']],
        ['insert', ['link', 'picture', 'videoAttributes']],
        ['view', ['fullscreen', 'codeview']]
    ],
    focus: true,
    hint: {
        match: /\B@@(\w*)$/,
        search: function (keyword, callback) {
            if (!keyword.length) return callback();
            var msg = JSON.stringify({ 'searchstr': keyword.toString() });
            $.ajax({
                async: true,
                url: '/Comment/GetMentions/',
                type: 'POST',
                contentType: "application/json; charset=utf-8",
                dataType: 'json',
                data: msg,
                error: function () {
                    callback();
                },
                success: function (res) {
                    callback(res.users);
                }
            });
        },
        content: function (item) {
            return $("<span class='badge badge-info userhint'>").html('@@' + item)[0];
        }
    }
});

var draftsTable = $('#draftsTable').DataTable({
    "searching": true,
    "lengthChange": false,
    "pageLength": 10,
    "processing": true,
    "serverSide": true,
    "ajax": {
        type: "POST",
        contentType: "application/json",
        url: "/Post/GetDrafts/",
        headers: getAntiForgeryToken(),
        data: function (d) {
            return JSON.stringify(d);
        }
    },
    "columns": [
        { "data": "Title", "orderable": true },
        {
            "data": null,
            "orderable": true,
            "mRender": function (data, type, row) {
                return "<a href='/Group/GroupDetail/" + data.GroupId + "'>" + data.Group + "</a>";
            }
        },
        { "data": "Time", "orderable": false },
        {
            "data": null,//"Type",
            "orderable": false,
            "mRender": function (data, type, row) {
                return "<button class='btn btn-sm btn-primary' onclick=loadpost(" + data.PostId + ")>Load</button> <button class='btn btn-sm btn-danger' onclick=del(" + data.PostId + ")>Delete</button>"//"<a href='" + data.URL + "'>" + data.Type + "</a>";
            }
        }
    ]
});

$("#postGroup").autocomplete({
    autoFocus: true,
    source: function (request, response) {
        $.ajax({
            async: true,
            url: "/Group/GetGroups/" + request.term,
            type: "GET",
            dataType: "json",
            //data: { prefix: request.term },
            success: function (data) {
                knownGroups = data;
                response($.map(data, function (item) {
                    return { label: item.GroupName, value: item.GroupName };
                }));
            }
        });
    },
    select: function (event, ui) {
        // if user clicked
    },
    change: function (event, ui) {
        var gn = $("#postGroup").val();
        if (typeof knownGroups === 'undefined' || knownGroups.length === 0) {
            // variable is undefined
            $("#postGroup").addClass('is-invalid');
        }
        else {
            if (knownGroups.findIndex(function (i) { return i.GroupName === gn; }) >= 0) {
                $("#postGroup").removeClass('is-invalid');
                gid = knownGroups[knownGroups.findIndex(function (i) { return i.GroupName === gn; })].GroupId;
                $('#postGroupActive').html(gn);
                $('#groupLink').html(gn);
                $('#groupLink').attr('href', '@Url.Action("GroupDetail", "Group")' + '?id=' + gid.toString());
            }
            else {
                $("#postGroup").addClass('is-invalid');
            }
        }
    }
});