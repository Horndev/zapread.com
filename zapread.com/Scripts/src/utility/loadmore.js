﻿/**
 * Load more posts
 * 
 * [✓] Native JS
 */

import { postData } from './postData';                      // [✓]  
import { onLoadedMorePosts } from './onLoadedMorePosts';
import Swal from 'sweetalert2';                             // [✓]

export function addposts(data, callback) {
    document.querySelectorAll('#posts').item(0).innerHTML += data.HTMLString; //.appendChild(data);
    callback();
}

export function loadmore(options) {
    if (typeof options === 'undefined') {
        options = { sort: 'Score', url: '/Home/InfiniteScroll/', blocknumber: window.BlockNumber, userId: -1 };
    }
    if (typeof options.url === 'undefined') {
        options.url = '/Home/InfiniteScroll/';
    }
    if (typeof options.blocknumber === 'undefined') {
        options.blocknumber = window.BlockNumber;
    }
    if (typeof options.sort === 'undefined') {
        options.sort = 'Score';
    }
    if (typeof options.userId === 'undefined') {
        options.userId = -1;
    }
    if (!inProgress) {
        inProgress = true;
        document.querySelectorAll('#loadmore').item(0).style.display = '';
        document.querySelectorAll('#btnLoadmore').item(0).disabled = true;

        postData(options.url, { "BlockNumber": options.blocknumber, "sort": options.sort, "userId": options.userId })
            .then((data) => {
                document.querySelectorAll('#loadmore').item(0).style.display = 'none';  // $('#loadmore').hide();
                document.querySelectorAll('#btnLoadmore').item(0).disabled = false;     // $('#btnLoadmore').prop('disabled', false);
                window.BlockNumber = window.BlockNumber + 10;
                NoMoreData = data.NoMoreData;
                inProgress = false;
                addposts(data, onLoadedMorePosts);
                if (NoMoreData) {
                    document.querySelectorAll('#loadmore').item(0).style.display = 'none'; //$('#showmore').hide();
                }
            })
            .catch((error) => {
                Swal.fire("Error", `Error loading posts: ${error}`, "error");
                document.querySelectorAll('#loadmore').item(0).style.display = 'none';
                document.querySelectorAll('#btnLoadmore').item(0).disabled = false;
                inProgress = false;
            })
            .finally(() => {
                // nothing?
            });
    }
}
