/*
 * 
 */

export function toggleHidden(e) {
    if (e.style.display === "none") {
        e.style.display = "block";
    } else {
        e.style.display = "none";
    }
}