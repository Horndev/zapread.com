/*
 *
 */
//import 'date-fns-1/dist/date_fns';
import { applyHoverToChildren } from './userhover';
import { loadgrouphover } from './grouphover';
import { updatePostTimes } from './datetime/posttime';
import { makePostsQuotable, makeCommentsQuotable } from './quotable/quotable';

export function onLoadedMorePosts() {
    //console.log('[DEBUG] onLoadedMorePosts');
    // User mention hover
    applyHoverToChildren(document, ".userhint");
    //var elements = document.querySelectorAll(".userhint");
    //Array.prototype.forEach.call(elements, function (el, _i) {
    //    loaduserhover(el);
    //    el.classList.remove('userhint');
    //});

    var elements = document.querySelectorAll(".grouphint");
    Array.prototype.forEach.call(elements, function (el, _i) {
        loadgrouphover(el);
        el.classList.remove('grouphint');
    });

    // show the read more
    elements = document.querySelectorAll(".post-box");
    Array.prototype.forEach.call(elements, function (el, _i) {
        if (parseFloat(getComputedStyle(el, null).height.replace("px", "")) >= 800) {
            el.querySelectorAll(".read-more-button").item(0).style.display = 'initial';
        }
    });

    // --- update impressions counts
    elements = document.querySelectorAll(".impression");
    Array.prototype.forEach.call(elements, function (el, _i) {
        var url = el.getAttribute('data-url');
        fetch(url).then(function (response) {
            return response.text();
        }).then(function (html) {
            el.innerHTML = html;
            el.classList.remove('impression');
        });
    });

    // --- relative times
    updatePostTimes();
    // ---

    // --- socials buttons
    // TODO: implement using non-jquery library
    elements = document.querySelectorAll(".sharing");
    Array.prototype.forEach.call(elements, function (el, _i) {
        var url = el.getAttribute('data-url');
        var sharetext = el.getAttribute('data-sharetext');
        //el.jsSocials({
        //    url: url,
        //    text: sharetext,
        //    showLabel: false,
        //    showCount: false,
        //    shareIn: "popup",
        //    shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
        //});
        el.classList.remove('sharing');
    });
    // old version
    //    $(".sharing").each(function () {
    //        $(this).jsSocials({
    //            url: $(this).data('url'),
    //            text: $(this).data('sharetext'),
    //            showLabel: false,
    //            showCount: false,
    //            shareIn: "popup",
    //            shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
    //        });
    //        $(this).removeClass("sharing");
    //    });
    // ---

    elements = document.querySelectorAll(".pop");
    Array.prototype.forEach.call(elements, function (el, _i) {
        el.classList.remove('pop');
    });
    // old version
    //    $(".pop").each(function () {
    //        $(this).removeClass("pop");
    //    });

    // Make post quotable
    makePostsQuotable();     // TODO: remove jquery

    // Make comments quotable
    makeCommentsQuotable();  // TODO: remove jquery
}

// OLD CODE

//var zrOnLoadedMorePosts = function () {

///    // User mention hover
///    $('.userhint').each(function () {
///        $(this).mouseover(function () {
///            loaduserhover(this);
///        });
///    });

///    $('.grouphint').each(function () {
///        $(this).mouseover(function () {
///            loadgrouphover(this);
///        });
///    });

///    // show the read more
///    $(".post-box").each(function (index, item) {
///        if ($(item).height() >= 800) {
///            $(item).find(".read-more-button").show();
///        }
///    });

///    $(".impression").each(function (ix, e) {
///        $(e).load($(e).data("url"));
///        $(e).removeClass("impression");
///    });

///    $('.postTime').each(function (i, e) {
///        var datefn = dateFns.parse($(e).html());
///        // Adjust to local time
///        datefn = dateFns.subMinutes(datefn, (new Date()).getTimezoneOffset());
///        var date = dateFns.format(datefn, "DD MMM YYYY");
///        var time = dateFns.distanceInWordsToNow(datefn);
///        $(e).html('<span>' + time + ' ago - ' + date + '</span>');
///        $(e).css('display', 'inline');
///        $(e).removeClass("postTime");
///    });

///    $(".sharing").each(function () {
///        $(this).jsSocials({
///            url: $(this).data('url'),
///            text: $(this).data('sharetext'),
///            showLabel: false,
///            showCount: false,
///            shareIn: "popup",
///            shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
///        });
///        $(this).removeClass("sharing");
///    });

//    $(".pop").popover({
//        trigger: "manual",
//        html: true,
//        sanitize: false,
//        animation: false
//    }).on("mouseenter", function () {
//        var _this = this;
//        $(this).popover("show");
//        $('[data-toggle="tooltip"]').tooltip()
//        $(".popover").addClass("tooltip-hover");
//        $(".popover").on("mouseleave", function () {
//            $(_this).popover('hide');
//        });
//    }).on("mouseleave", function () {
//        var _this = this;
//        setTimeout(function () {
//            if (!$(".popover:hover").length) {
//                $(_this).popover("hide");
//            }
//        }, 300);
//    });

///    $(".pop").each(function () {
///        $(this).removeClass("pop");
///    });

///    // Make post quotable
///    makePostsQuotable();

///    // Make comments quotable
///    makeCommentsQuotable();

//    //console.log("[DEBUG] Done initializing after load more.");
//};