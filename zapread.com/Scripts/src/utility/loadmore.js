/*
 * 
 */

import { postData } from './postData';
import { onLoadedMorePosts } from './onLoadedMorePosts';
import Swal from 'sweetalert2';

export function addposts(data, callback) {
    //$("#posts").append(data.HTMLString);
    document.querySelectorAll('#posts').item(0).innerHTML += data.HTMLString; //.appendChild(data);
    callback();
}

export function loadmore(sort) {
    if (!inProgress) {
        inProgress = true;
        document.querySelectorAll('#loadmore').item(0).style.display = ''; // $('#loadmore').show();
        document.querySelectorAll('#btnLoadmore').item(0).disabled = true; //$('#btnLoadmore').prop('disabled', true);

        postData('/Home/InfiniteScroll/', { "BlockNumber": BlockNumber, "sort": sort })
            .then((data) => {
                document.querySelectorAll('#loadmore').item(0).style.display = 'none';  // $('#loadmore').hide();
                document.querySelectorAll('#btnLoadmore').item(0).disabled = false;     // $('#btnLoadmore').prop('disabled', false);
                BlockNumber = BlockNumber + 10;
                NoMoreData = data.NoMoreData;
                inProgress = false;
                addposts(data, onLoadedMorePosts);
                if (NoMoreData) {
                    document.querySelectorAll('#loadmore').item(0).style.display = 'none'; //$('#showmore').hide();
                }
            })
            .catch((error) => {
                Swal.fire("Error", `Error loading posts: ${error}`, "error");
            })
            .finally(() => {
                // nothing?
            });
    }
}