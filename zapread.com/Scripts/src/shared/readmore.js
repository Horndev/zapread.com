/**
 * 
 * [✓] Does not use jQuery
 * 
 */

/**
 * 
 * [✓] Does not use jQuery
 * 
 * @param {any} e
 */
export function readMoreButton(e) {
    
    var el = e;
    var p = el.parentElement;//.parent();
    var up = p.parentElement;//.parent();

    // measure how tall inside should be by adding together heights of all inside paragraphs (except read-more paragraph)
    //var ps = up.querySelectorAll(".post-content");
    //var totalHeight = 0;
    //Array.prototype.forEach.call(ps, function (e, _i) {
    //    totalHeight += e.offsetHeight;//$(this).outerHeight();
    //});

    up.style.height = 'auto'; //totalHeight.toString() + "px";
    up.style.maxHeight = "9999px";

    // fade out read-more
    //p.fadeOut();
    p.style.display = '';

    // prevent jump-down
    return false;
}
window.readMoreButton = readMoreButton;
