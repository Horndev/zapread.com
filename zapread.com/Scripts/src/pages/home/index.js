/*
 * 
 */
import '../../shared/shared';
import '../../realtime/signalr';
//import '../../../summernote/dist/summernote-bs4';
//import 'summernote/dist/summernote-bs4.css';
//import '../../utility/summernote/summernote-video-attributes';
import Swal from 'sweetalert2';
const Globals = require('./globals').default;
import { addposts, loadmore} from '../../utility/loadmore';
import { onLoadedMorePosts } from '../../utility/onLoadedMorePosts';
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import '../../shared/sharedlast';

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.loadMoreComments = loadMoreComments;
window.loadmore = loadmore;

var request = new XMLHttpRequest();
request.open('GET', '/Home/TopPosts/?sort=' + postSort, true);

request.onload = function () {
    var resp = this.response;
    var response = {};
    if (this.status >= 200 && this.status < 400) {
        // Success!
        response = JSON.parse(resp);
        if (response.success) {
            // Insert posts
            document.querySelectorAll('#posts').item(0).querySelectorAll('.ibox-content').item(0).classList.remove("sk-loading");
            addposts(response, onLoadedMorePosts); // [ ] TODO: zrOnLoadedMorePosts uses jquery
            document.querySelectorAll('#btnLoadmore').item(0).style.display = ''; //$('#btnLoadmore').show();
        } else {
            // Did not work
            Swal.fire("Error", "Error loading posts: " + response.message, "error");
        }
    } else {
        response = JSON.parse(resp);
        // We reached our target server, but it returned an error
        Swal.fire("Error", "Error loading posts (status ok): " + response.message, "error");
    }
};

request.onerror = function () {
    // There was a connection error of some sort
    var response = JSON.parse(this.response);
    Swal.fire("Error", "Error requesting posts: " + response.message, "error");
};

request.send();
