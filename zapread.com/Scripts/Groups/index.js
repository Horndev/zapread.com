/* 
 */

var go = function () {
    var gid = $('#groupSearch').val();
    var url = '/Group/GroupDetail';
    url = url + '/' + gid;
    location.href = url;
};

