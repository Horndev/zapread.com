/**/

$(document).ready(function () {
    // Show the comment input box
    initCommentInput(detailPostId);

    // This opens up the vote modal if user clicked to vote
    if (showVoteDialog) {
        document.addEventListener("voteReady", function (e) {
            vote(detailPostId, detailPostVote, 1, 100);
        });
    }
});