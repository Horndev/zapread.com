//
//

$(document).ready(function () {
    // This loads all async partial views on page
    $(".partialContents").each(function (index, item) {
        var url = $(item).data("url");
        if (url && url.length > 0) {
            $(item).load(url);
        }
    });
});

var toggleIgnore = function (id) {
    joinurl = "/Group/ToggleIgnore";
    var data = JSON.stringify({ 'groupId': id });
    var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        data: data.toString(),
        type: 'POST',
        url: joinurl,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        headers: headers,
        success: function (response) {
            if (response.result === "success") {
                if (response.added) {
                    $("#i_" + id.toString()).html("<i class='fa fa-circle'></i> Un-Ignore ");
                }
                else {
                    $("#i_" + id.toString()).html("<i class='fa fa-ban'></i> Ignore ");
                }
            }
        }
    });
    return false;
};

/* Infinite scroll */
var BlockNumber = 10;
var NoMoreData = false;
var inProgress = false;

$(window).scroll(function () {
    if ($(window).scrollTop() === $(document).height() - $(window).height() && !NoMoreData && !inProgress) {
        loadmore();
    }
});

var loadmore = function () {
    if (!inProgress) {
        inProgress = true;
        $('#loadmore').show();
        $('#btnLoadmore').prop('disabled', true);
        $.ajax({
            async: true,
            data: JSON.stringify({ "id": groupId, "BlockNumber": BlockNumber, "sort": "New" }),
            type: 'POST',
            url: "/Group/InfiniteScroll/",//"@Url.Action("InfiniteScroll", "Group")",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            headers: getAntiForgeryToken(),
            success: function (response) {
                if (response.Success) {
                    $('#loadmore').hide();
                    $('#btnLoadmore').prop('disabled', false);
                    BlockNumber = BlockNumber + 10;
                    NoMoreData = response.NoMoreData;
                    $("#posts").append(response.HTMLString);
                    inProgress = false;

                    // Wait for new posts to be added then tidy up.

                    // New version using a callback
                    //addposts(response, zrOnLoadedMorePosts);
                    $("#posts").append(response.HTMLString);
                    zrOnLoadedMorePosts();

                    // old version with jquery
                    //$.when(addposts(response), $.ready).then(function () {
                    //    zrOnLoadedMorePosts();
                    //});

                    if (NoMoreData) {
                        $('#showmore').hide();
                    }
                }
            }
        });
    }
};