/* ZapRead LoadMore functionality
*/

var addposts = function (data) {
    $("#posts").append(data.HTMLString);
};

var zrOnLoadedMorePosts = function () {
    console.log("[DEBUG] Initializing after load more.");

    // User mention hover
    $('.userhint').each(function () {
        $(this).mouseover(function () {
            loaduserhover(this);
        });
    });

    $('.grouphint').each(function () {
        $(this).mouseover(function () {
            loadgrouphover(this);
        });
    });

    // show the read more
    $(".post-box").each(function (index, item) {
        if ($(item).height() >= 800) {
            $(item).find(".read-more-button").show();
        }
    });

    $(".impression").each(function (ix, e) {
        $(e).load($(e).data("url"));
        $(e).removeClass("impression");
    });

    $('.postTime').each(function (i, e) {
        var datefn = dateFns.parse($(e).html());
        // Adjust to local time
        datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
        var date = dateFns.format(datefn, "DD MMM YYYY");
        var time = dateFns.distanceInWordsToNow(datefn);
        $(e).html('<span>' + time + ' ago - ' + date + '</span>');
        $(e).css('display', 'inline');
        $(e).removeClass("postTime");
    });

    $(".sharing").each(function () {
        $(this).jsSocials({
            url: $(this).data('url'),
            text: $(this).data('sharetext'),
            showLabel: false,
            showCount: false,
            shareIn: "popup",
            shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
        });
        $(this).removeClass("sharing");
    });

    $(".pop").popover({
        trigger: "manual",
        html: true,
        sanitize: false,
        animation: false
    }).on("mouseenter", function () {
        var _this = this;
        $(this).popover("show");
        $('[data-toggle="tooltip"]').tooltip()
        $(".popover").addClass("tooltip-hover");
        $(".popover").on("mouseleave", function () {
            $(_this).popover('hide');
        });
    }).on("mouseleave", function () {
        var _this = this;
        setTimeout(function () {
            if (!$(".popover:hover").length) {
                $(_this).popover("hide");
            }
        }, 300);
    });

    $(".pop").each(function () {
        $(this).removeClass("pop");
    });

    // Make post quotable
    makePostsQuotable();

    // Make comments quotable
    makeCommentsQuotable();

    console.log("[DEBUG] Done initializing after load more.");
};