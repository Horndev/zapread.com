/*
 * 
 */
import tippy from 'tippy.js';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';
import { postData } from './postData';

import { follow } from './ui/follow';

// Save to window globals (part of the user hover)
window.follow = follow;

export function applyHoverToChildren(el, selector) {
    var elements = el.querySelectorAll(selector);
    Array.prototype.forEach.call(elements, function (el, _i) {
        loaduserhover(el);
        el.classList.remove('userhint');
    });
}

export function loaduserhover(e) {
    e.removeAttribute('onmouseover');
    var userid = e.getAttribute('data-userid');
    var username = e.innerHTML.trim().replace('@', '');
    if (typeof userid === 'undefined' || userid === null) {
        userid = -1;
    }

    tippy(e, {
        content: 'Loading...',
        theme: 'light-border',
        allowHTML: true,
        delay: 300,
        interactive: true,
        interactiveBorder: 30,
        //flipOnUpdate: true,
        // some async loading code...
        onCreate(instance) {
            //console.log('created tippy');
            // Setup our own custom state properties
            instance._isFetching = false;
            instance._src = null;
            instance._error = null;
        },
        onShow(instance) {
            if (instance._isFetching || instance._src || instance._error) {
                //console.log('hover cached.');
                return;
            } else {
                //console.log('fetching...');
                instance._isFetching = true;
                var hoverdata = { 'userId': userid, 'username': username };
                postData('/User/Hover/', hoverdata)
                    .then((data) => {
                        instance.setContent(data.HTMLString);
                        instance._src = true;
                    })
                    .catch((error) => {
                        instance._error = error;
                        instance.setContent(`Request failed. ${error}`);
                    })
                    .finally(() => {
                        instance._isFetching = false;
                    });
            }
        }//,
        //onHidden(instance) {
        //    instance.setContent('Loading...');
        //    // Unset these properties so new network requests can be initiated
        //    instance._src = null;
        //    instance._error = null;
        //}
    });
}