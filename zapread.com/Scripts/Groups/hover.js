/* Group Hover 
 */

//$(document).ready(function () {
//    $('.grouphint').each(function () {
//        $(this).mouseover(function () {
//            loadgrouphover(this);
//        });
//    });
//});

var loadgrouphover = function (e) {
    $(e).removeAttr('onmouseover');
    var groupid = $(e).data('groupid');
    if (typeof groupid === 'undefined') {
        groupid = -1;
    }
    var msg = JSON.stringify({
        'groupId': groupid
    });
    var appearTimeout;
    $.ajax({
        type: "POST",
        url: "/Group/Hover",
        data: msg,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
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
                        appearTimeout = setTimeout(function () {
                            $(_this).popover("show");
                            $(".popover").addClass("tooltip-hover");
                            $(".popover").on("mouseleave", function () {
                                $(_this).popover('hide');
                            });
                        }, 500);
                    })
                    .on("mouseleave", function () {
                        clearTimeout(appearTimeout);
                        var _this = this;
                        setTimeout(function () {
                            if (!$(".popover:hover").length) {
                                $(_this).popover("hide");
                            }
                        }, 300);
                    });
                appearTimeout = setTimeout(function () {
                    $(e).popover("show");
                    $(".popover").addClass("tooltip-hover");
                    $(".popover").on("mouseleave", function () {
                        setTimeout(function () {
                            $(e).popover('hide');
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
