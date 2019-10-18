/**/

$(document).ready(function () {
    // This formats the timestamps on the page
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
    // show the read more
    $(".post-box").each(function (index, item) {
        if ($(item).height() >= 800) {
            $(item).find(".read-more-button").show();
        }
    });

    // Make post quotable
    makePostsQuotable();

    // Make comments quotable
    makeCommentsQuotable();

    $.fn.extend({
        placeCursorAtEnd: function () {
            // Places the cursor at the end of a contenteditable container (should also work for textarea / input)
            if (this.length === 0) {
                throw new Error("Cannot manipulate an element if there is no element!");
            }
            var el = this[0];
            var range = document.createRange();
            var sel = window.getSelection();
            var childLength = el.childNodes.length;
            if (childLength > 0) {
                var lastNode = el.childNodes[childLength - 1];
                var lastNodeChildren = lastNode.childNodes.length;
                range.setStart(lastNode, lastNodeChildren);
                range.collapse(true);
                sel.removeAllRanges();
                sel.addRange(range);
            }
            return this;
        }
    });

    $(".impression").each(function (ix, e) {
        $(e).load($(e).data("url"));
        $(e).removeClass("impression");
    });
});

var makeCommentsQuotable = function () {
    $(".comment-quotable").each(function (ix, e) {
        // Trigger when mouse is released (i.e. possible selection made)
        $(e).mouseup(function () {
            var selection = getSelected();
            $(selectionMarker).popover('hide');
            if (selection && selection !== "") {
                // User made a selection
                var markerId = "sel_" + new Date().getTime() + "_" + Math.random().toString().substr(2);
                selectionMarker = markSelection(markerId);
                selectionText = encode(selection.toString());
                var commentid = $(e).data('commentid');
                var popText = selectionText + '<hr/>' +
                    '<button class="btn btn-sm btn-link" onclick="commentQuoteComment(' + commentid + ');"><i class="fa fa-reply"></i> Reply</button>' +
                    '<button class="btn btn-sm btn-link" onclick="commentQuoteComment(' + commentid + ',true);">' +
                    '<i class="fa fa-reply"></i><i class="fa fa-bell"></i> Mention</button>';
                $(selectionMarker).popover({
                    trigger: "hover",
                    html: true,
                    sanitize: false,
                    animation: false,
                    title: "Quote",
                    placement: "top",
                    content: popText
                }).on('hidden.bs.popover', function () {
                    $(selectionMarker).popover('dispose');
                })
                    .popover("show");
            }
        });
        $(e).removeClass("post-quotable");
    });
};

var encode = function (s) {
    var x = document.createElement("div");
    x.innerText = s;
    return x.innerHTML;
};

var makePostsQuotable = function () {
    $(".post-quotable").each(function (ix, e) {
        // Trigger when mouse is released (i.e. possible selection made)
        $(e).mouseup(function () {
            var selection = getSelected();
            $(selectionMarker).popover('hide');
            if (selection && encode(selection.toString()) !== "") {
                // User made a selection
                var markerId = "sel_" + new Date().getTime() + "_" + Math.random().toString().substr(2);
                selectionMarker = markSelection(markerId);
                selectionText = encode(selection.toString());
                var postId = $(e).data('postid');
                var popText = selectionText + '<hr/>' +
                    '<button class="btn btn-sm btn-link" onclick="postQuoteComment(' + postId + ');"><i class="fa fa-reply"></i> Reply</button>' +
                    '<button class="btn btn-sm btn-link" onclick="postQuoteComment(' + postId + ',true);">' +
                    '<i class="fa fa-reply"></i><i class="fa fa-bell"></i> Mention</button>';
                $(selectionMarker).popover({
                    trigger: "hover",
                    html: true,
                    sanitize: false,
                    animation: false,
                    title: "Quote",
                    placement: "top",
                    content: popText
                }).on('hidden.bs.popover', function () {
                    $(selectionMarker).popover('dispose');
                })
                .popover("show");
            }
        });
        $(e).removeClass("post-quotable");
    });
};
