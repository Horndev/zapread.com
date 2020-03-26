/* 
 */

var go = function () {
    var gid = $('#groupSearch').val();
    var url = '/Group/GroupDetail';
    url = url + '/' + gid;
    location.href = url;
};

var groupTable = {};

$(document).ready(function () {
    // Table
    groupTable = $('#groupTable').DataTable({
        "searching": true,
        "bInfo": true,
        "lengthChange": true,
        "ordering": true,
        "pageLength": 10,
        "processing": true,
        "serverSide": true,
        "bDeferRender": true,
        "sDom": '<<"text-center"f><"text-center"i><"text-center"p>t<"pull-left"l><"text-center"p>>',
        "ajax": {
            type: "POST",
            contentType: "application/json",
            url: "/Group/GetGroupsTable",
            headers: getAntiForgeryToken(),
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        "columns": [{
            "data": null,
            "name": 'Name',
            "className": "project-status",
            "orderable": false,
            "mRender": function (data, type, row) {
                return "<div class='forum-icon'><i class='fa " + data.Icon + "'></i></div>";
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "Name",
            "className": "project-title",
            "mRender": function (data, type, row) {
                let td = "<a href='/Group/GroupDetail/" + data.Id + "'>" + data.Name + "</a>";
                if (data.IsAdmin) {
                    td += "<i class='fa fa-gavel text-primary' data-toggle='tooltip' data-placement='right' title='Administrator'></i>";
                }
                else if (data.IsMod) {
                    td += "<i class='fa fa-gavel text-success' data-toggle='tooltip' data-placement='right' title='Moderator'></i>";
                }
                td += "<br />";
                td += "<small>Created " + data.CreatedddMMMYYYY + "</small>";
                return td;
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "LastSeen",
            "className": "project-title",
            "mRender": function (data, type, row) {
                let td = "";
                for (var i = 0, len = data.Tags.length; i < len; i++) {
                    td += "<span class='badge badge-light' style='margin-left: 3px;'>" + data.Tags[i] + " </span>";
                }
                td += "<br /><small>Tags</small>";
                return td;
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "NumPosts",
            "className": "project-completion",
            "mRender": function (data, type, row) {
                let td = "<small class='d-none d-md-block'>Progress to next tier</small>" +
                    "<div class='progress progress-mini d-none d-md-block'>" +
                    "<div style='width: " + data.Progress + "%;' class='progress-bar'></div>" +
                    "</div>";
                return td;
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "NumComments",
            "className": "project-people",
            "mRender": function (data, type, row) {
                return "<span class='views-number d-none d-sm-block'>" +
                    data.Level +
                    "</span>" +
                    "<div class='d-none d-sm-block'>" +
                    "<small>Tier</small>" +
                    "</div>";
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "NumComments",
            "className": "project-people",
            "mRender": function (data, type, row) {
                let td = "<a href='/Group/Members/" + data.Id + "' class='btn btn-link'>" +
                    "<span class='views-number d-none d-sm-block' id='group_membercount_" + data.Id + "'>" +
                    data.NumMembers +
                    "</span>" +
                    "<div class='d-none d-sm-block'>" +
                    "<small>Members</small>" +
                    "</div>" +
                    "</a>";
                return td;
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "NumComments",
            "className": "project-people",
            "mRender": function (data, type, row) {
                return "<span class='views-number d-none d-sm-block'>" +
                    data.NumPosts +
                    "</span>" +
                    "<div class='d-none d-sm-block'>" +
                    "<small>Posts</small>" +
                    "</div>";
            }
        }, {
            "data": null,
            "orderable": false,
            "name": "Balance",
            "className": "project-actions",
            "mRender": function (data, type, row) {
                let td = "";
                if (!data.IsLoggedIn) {
                    td += "<button class='btn btn-primary btn-sm' disabled><i class='fa fa-user-plus'></i> Join </button>";
                } else if (data.IsMember) {
                    td += "<a href='javascript:void(0);' id='j_" + data.Id + "' onclick='leaveGroup(" + data.Id + ",this)' class='btn btn-primary btn-outline btn-sm'><i class='fa fa-user-times'></i> Leave </a>";
                } else {
                    td += "<a href='javascript:void(0);' id='j_" + data.Id + "' onclick='joinGroup(" + data.Id + ",this)' class='btn btn-primary btn-outline btn-sm'><i class='fa fa-user-plus'></i> Join </a>";
                }
                return td;
            }
        }
        ]
    });

    $('#groupSearch').selectize({
        valueField: 'Id',
        labelField: 'Name',
        searchField: ['Name', 'Tags'],
        create: false,
        options: [],
        maxItems: 1,
        render: {
            option: function (item, escape) {
                //alert(JSON.stringify(item));
                var actors = [];
                //for (var i = 0, n = item.abridged_cast.length; i < n; i++) {
                //    actors.push('<span>' + escape(item.abridged_cast[i].name) + '</span>');
                //}
                //console.log(JSON.stringify(item));
                //console.log(item.Icon);

                var tagstr = '';
                item.Tags.forEach(function (t, ix) {
                    tagstr = tagstr + '<span class="badge badge-light">' + t + '</span>&nbsp;';
                });

                var str = '<div class="forum-item">' +
                    '<div class="row">' +
                    '<div class="col-1"></div>' +
                    '<div class="col">' +
                    '<i class="fa fa-' + item.Icon + '"></i>' +
                    '</div>' +
                    '<div class="col text-left">' +
                    '<span class="forum-item-title">' + item.Name + '</span>' +
                    '</div>' +
                    '<div class="col text-left">' +
                    tagstr +
                    '</div>' +
                    '</div>' +
                    '</div>';
                //console.log(str);
                return str;

                //return '<div>' +
                //    '<span class="title">' +
                //    '<span class="name">' + escape(item.Name) + '</span>' +
                //    '</span>' +
                //    '</div>';
            },
            item: function (item, escape) {
                //alert(JSON.stringify(item));
                return '<div>' +
                    '<span class="title">' +
                    escape(item.Name) +
                    '</div>';
            }
        },
        load: function (query, callback) {
            if (!query.length) return callback();
            //alert(JSON.stringify(query));
            $.ajax({
                async: true,
                url: '/Group/Search',
                type: 'POST',
                dataType: 'json',
                data: { searchstr: JSON.stringify(query) },
                error: function () {
                    callback();
                },
                success: function (res) {
                    console.log(JSON.stringify(res.groups));
                    callback(res.groups);
                }
            });
        }
    });
});