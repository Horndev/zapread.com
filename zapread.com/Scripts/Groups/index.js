/* 
 */

var go = function () {
    var gid = $('#groupSearch').val();
    var url = '/Group/GroupDetail';
    url = url + '/' + gid;
    location.href = url;
};

var join = function (id) {
    joinurl = "/Group/JoinGroup";
    var data = JSON.stringify({ 'gid': id });
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: joinurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result === "success") {
                $("#j_" + id.toString()).html("<i class='fa fa-user-times'></i> Leave ");
                $("#j_" + id.toString()).attr("onClick", "javascript: leave(" + id.toString() + "); ");
                var numMembers = parseInt($('#group_membercount_' + id.toString()).html());
                $('#group_membercount_' + id.toString()).html(numMembers + 1);
            }
        }
    });
    return false;
};

var leave = function (id) {
    leaveurl = "/Group/LeaveGroup";
    var data = JSON.stringify({ 'gid': id });
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: leaveurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.result === "success") {
                $("#j_" + id.toString()).html("<i class='fa fa-user-plus'></i> Join ");
                $("#j_" + id.toString()).attr("onClick", "javascript: join(" + id.toString() + "); ");
                var numMembers = parseInt($('#group_membercount_' + id.toString()).html());
                $('#group_membercount_' + id.toString()).html(numMembers - 1);
            }
        }
    });
    return false;
};