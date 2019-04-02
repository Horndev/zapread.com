/* Username hover
*/

$(document).ready(function () {
    $('.userhint').each(function () {
        $(this).mouseover(function () {
            loadhover(this);
        });
    });
});

var loadhover = function (e) {
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
    $.ajax({
        type: "POST",
        url: "/User/Hover",
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
                        $(this).popover("show");
                        $(".popover").addClass("tooltip-hover");
                        $(".popover").on("mouseleave", function () {
                            $(_this).popover('hide');
                        });
                    })
                    .on("mouseleave", function () {
                        var _this = this;
                        setTimeout(function () {
                            if (!$(".popover:hover").length) {
                                $(_this).popover("hide");
                            }
                        }, 300);
                    });
                $(e).popover("show");
                $(".popover").addClass("tooltip-hover");
                $(".popover").on("mouseleave", function () {
                    $(e).popover('hide');
                });
                $(e).removeClass("zr-user");
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