
// Called immediately when page loads

// moved to /src/home/index.js
//$.ajax({
//    async: true,
//    data: { "sort": postSort },
//    type: 'GET',
//    url: '/Home/TopPosts/',
//    contentType: "application/json; charset=utf-8",
//    dataType: "json",
//    success: function (response) {
//        if (response.success) {
//            // Insert posts
//            // Wait for new posts to be added then tidy up.
//            $('#posts').children('.ibox-content').removeClass('sk-loading');
//            $.when(addposts(response), $.ready).then(function () {
//                zrOnLoadedMorePosts();
//            });
//            $('#btnLoadmore').show();
//        } else {
//            // Did not work
//            swal("Error", "Error loading posts: " + data.message, "error");
//        }
//    },
//    failure: function (response) {
//        swal("Error", "Failure loading posts: " + response.message, "error");
//    },
//    error: function (response) {
//        swal("Error", "Error loading posts: " + response.message, "error");
//    }
//});

$(window).scroll(function () {
    if ($(window).scrollTop() === $(document).height() -
        $(window).height() && !NoMoreData && !inProgress) {
        loadmore();
    }
});

var loadmore = function (sort) {
    if (!inProgress) {
        inProgress = true;
        $('#loadmore').show();
        $('#btnLoadmore').prop('disabled', true);
        $.post("/Home/InfiniteScroll/",
            { "BlockNumber": BlockNumber, "sort": sort },
            function (data) {
                $('#loadmore').hide();
                $('#btnLoadmore').prop('disabled', false);
                BlockNumber = BlockNumber + 10;
                NoMoreData = data.NoMoreData;
                inProgress = false;
                // Wait for new posts to be added then tidy up.

                // new version using a callback
                //addposts(data, zrOnLoadedMorePosts);
                $("#posts").append(data.HTMLString);
                zrOnLoadedMorePosts();

                // old version using a callback
                //$.when(addposts(data), $.ready).then(function () {
                //    zrOnLoadedMorePosts();
                //});

                if (NoMoreData) {
                    $('#showmore').hide();
                }
            }
        );
    }
};