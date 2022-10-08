/**
 * [✓] Native JS
 */

import { getAntiForgeryToken } from '../antiforgery';       // [✓]
import { postJson } from '../postData';                     // [✓]

/**
 * 
 * [✓] Native JS
 * 
 * @param {any} uid
 * @param {any} s
 * @param {any} e
 */
export function follow(uid, s, e) {
    postJson("/user/SetFollowing/", { 'id': uid, 's': s })
    .then((response) => {
        if (response.success) {
            var el;
            if (s === 1) { // Subscribed
                if (e.classList.contains("hover-follow")) {//$(e).hasClass('hover-follow')) {
                    e.innerHTML = "<i class='fa fa-user'></i><i class='fa fa-check'></i>"; //$(e).html("<i class='fa fa-user'></i><i class='fa fa-check'></i>");
                    e.title = "Un-follow";//$(e).attr('title', 'Un-follow');
                    e.setAttribute('onclick', 'follow(' + uid + ',0, this);');//$(e).attr('onclick', 'follow(' + uid + ',0, this);');
                }
                el = document.getElementById("subBtnText");
                if (el !== null) {
                    el.innerHTML = "Un-Follow"; //$('#subBtnText').html("Unsubscribe");
                }
                el = document.getElementById("sublink");
                if (el !== null) {
                    el.setAttribute("onclick", "follow(" + uid + ",0, this);");//$('#sublink').attr("onclick", "follow(uid,0);");
                }
            } else { // Un-subscribed
                if (e.classList.contains("hover-follow")) {//$(e).hasClass('hover-follow')) {
                    e.innerHTML = "<i class='fa fa-user-plus'></i>";//$(e).html("<i class='fa fa-user-plus'></i>");
                    e.title = "Follow";//$(e).attr('title', 'Follow');
                    e.setAttribute('onclick', 'follow(' + uid + ',1, this);');//$(e).attr('onclick', 'follow(' + uid + ',1, this);');
                }
                el = document.getElementById("subBtnText");
                if (el !== null) {
                    el.innerHTML = "Follow"; //$('#subBtnText').html("Subscribe");
                }
                el = document.getElementById("sublink");
                if (el !== null) {
                    el.setAttribute("onclick", "follow(" + uid + ",1, this);");//$('#sublink').attr("onclick", "follow(uid,1);");
                }
            }
        }
        else {
            alert(response.message);
        }
    })
    .catch((error) => {
        console.log('follow error ' + error);
    });
    return false;
}
