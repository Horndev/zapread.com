/*
 * 
 */

export function addposts(data, callback) {
    //$("#posts").append(data.HTMLString);
    document.querySelectorAll('#posts').item(0).innerHTML += data.HTMLString; //.appendChild(data);
    callback();
}