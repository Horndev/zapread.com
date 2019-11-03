
//$(document).ready(function () {
    //var headers = getAntiForgeryToken();
    $.ajax({
        async: true,
        data: JSON.stringify({ "sort": postSort }),
        type: 'GET',
        url: '/Home/TopPosts/',
        //headers: headers,
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                // Insert posts
                // Wait for new posts to be added then tidy up.
                $('#posts').children('.ibox-content').removeClass('sk-loading');
                $.when(addposts(response), $.ready).then(function () {
                    zrOnLoadedMorePosts();
                });

                $('#btnLoadmore').show();
            } else {
                // Did not work
                swal("Error", "Error loading posts: " + data.message, "error");
            }
        },
        failure: function (response) {
            swal("Error", "Failure loading posts: " + response.message, "error");
        },
        error: function (response) {
            swal("Error", "Error loading posts: " + response.message, "error");
        }
    });
//    return false; // Prevent jump to top of page
//});

var BlockNumber = 10;  //Infinite Scroll starts from second block
var NoMoreData = false;
var inProgress = false;

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
                $.when(addposts(data), $.ready).then(function () {
                    zrOnLoadedMorePosts();
                });

                if (NoMoreData) {
                    $('#showmore').hide();
                }
            }
        );
    }
};