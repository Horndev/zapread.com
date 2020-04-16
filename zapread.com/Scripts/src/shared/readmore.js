var $el, $ps, $up, totalHeight, $p;

export function readMoreButton(e) {
    totalHeight = 0;

    $el = $(e);
    $p = $el.parent();
    $up = $p.parent();
    $ps = $up.find(".post-content");

    // measure how tall inside should be by adding together heights of all inside paragraphs (except read-more paragraph)
    $ps.each(function () {
        totalHeight += $(this).outerHeight();
    });
    //console.log(totalHeight);

    $up
        .css({
            // Set height to prevent instant jumpdown when max height is removed
            "height": $up.height(),
            "max-height": 9999
        })
        .animate({
            "height": totalHeight
        });

    // fade out read-more
    $p.fadeOut();

    // prevent jump-down
    return false;
}
window.readMoreButton = readMoreButton;
