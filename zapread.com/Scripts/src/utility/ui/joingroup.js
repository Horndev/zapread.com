/**
 * 
 * [✓] Native JS
 */
import { postJson } from '../postData';                     // [✓]

export function joinGroup(id, e) {
    postJson("/Group/JoinGroup/", { 'gid': id })
    .then((response) => {
        if (response.success) {
            e.setAttribute("onclick", "leaveGroup(" + id.toString() + ",this);");//$(e).attr("onClick", "leaveGroup(" + id.toString() + ",this);");
            var page = e.getAttribute("data-page");
            if (page === 'h') {
                e.innerHTML = "<i class='fa fa-minus'></i>";//$(e).html("<i class='fa fa-minus'></i>");
            } else {
                e.innerHTML = "<i class='fa fa-user-times'></i> Leave ";//$(e).html("<i class='fa fa-user-times'></i> Leave ");
            }
            // For group view [TODO: Fix this bad code]
            if (page === 'detail') {
                var numMembers = parseInt(document.getElementById("group_membercount_" + id.toString()).innerHTML);//$('#group_membercount_' + id.toString()).html());
                document.getElementById("group_membercount_" + id.toString()).innerHTML = numMembers + 1;//$('#group_membercount_' + id.toString()).html(numMembers + 1);
            }
        }
    })
    return false;
};