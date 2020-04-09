/* Username hover
*/

//$(document).ready(function () {
//    $('.userhint').each(function () {
//        $(this).mouseover(function () {
//            loaduserhover(this);
//        });
//    });
//});

var LoadedHovers = new Array();

var loaduserhover = function (e) {
    alert('!');
    $(e).removeAttr('onmouseover');
    var userid = $(e).data('userid');
    var username = $(e).html().trim().replace('@', '');
    if (typeof userid === 'undefined') {
        userid = -1;
    }
    var msg = JSON.stringify({
        'userId': userid,
        'username' : username
    });

    if ($.inArray(userid, LoadedHovers) > -1) {
        //console.log("userid in array");
        return;
    }

    var appearTimeout;

    $.ajax({
        type: "POST",
        url: "/User/Hover/",
        data: msg,
        contentType: "application/json; charset=utf-8",
        headers: getAntiForgeryToken(),
        dataType: "json",
        success: function (response) {
            if (response.success) {
                LoadedHovers.push(userid);
                $(e).attr("data-content", response.HTMLString);
                $(e).popover({
                    trigger: "manual",
                    html: true,
                    sanitize: false,
                    animation: false,
                    placement: "top",
                    container: "body",
                    title: ""
                })
                .on("mouseenter", function () {
                    var _this = this;
                    if ($.inArray(userid, LoadedHovers) > -1) {
                        //console.log("userid in array");
                        return;
                    }
                    appearTimeout = setTimeout(function () {
                        $(_this).popover("show");
                        $(".popover").addClass("tooltip-hover");
                        $(".popover").on("mouseleave", function () {
                            $(_this).popover('hide');
                        });
                    }, 500);
                })
                .on("mouseleave", function () {
                    var _this = this;
                    clearTimeout(appearTimeout);
                    setTimeout(function () {
                        if (!$(".popover:hover").length) {
                            $(_this).popover("hide");
                            // Prevent double popups for same user
                            for (var i = 0; i < LoadedHovers.length; i++) {
                                if (LoadedHovers[i] === userid) {
                                    LoadedHovers.splice(i, 1);
                                }
                            }
                        }
                    }, 300);
                });

                appearTimeout = setTimeout(function () {
                    $(e).popover("show");
                    $(".popover").addClass("tooltip-hover");
                    $(".popover").on("mouseleave", function () {
                        setTimeout(function () {
                            $(e).popover('hide');
                            // Prevent double popups for same user if one already open
                            for (var i = 0; i < LoadedHovers.length; i++) {
                                if (LoadedHovers[i] === userid) {
                                    LoadedHovers.splice(i, 1);
                                }
                            }
                        }, 300);
                    });
                }, 500);
            }
            else {
                console.log(response.Message);
            }
        },
        failure: function (response) {
            console.log('load more failure');
        },
        error: function (response) {
            console.log('load more error');
        }
    });
};