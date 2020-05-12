/**
 * 
 * [✓] Native JS
 */
import { postData } from './postData';          // [✓]
import tippy from 'tippy.js';                   // [✓]
import 'tippy.js/dist/tippy.css';               // [✓]
import 'tippy.js/themes/light-border.css';      // [✓]

import { joinGroup } from './ui/joingroup';     // [✓]
import { leaveGroup } from './ui/leavegroup';   // [✓]

// Save to window globals (part of the group hover)
window.joinGroup = joinGroup;
window.leaveGroup = leaveGroup;

export function loadgrouphover(e) {
    e.removeAttribute('onmouseover');
    var groupid = e.getAttribute('data-groupid');
    if (typeof groupid === 'undefined') {
        groupid = -1;
    }
    tippy(e, {
        content: 'Loading...',
        theme: 'light-border',
        allowHTML: true,
        delay: 300,
        interactive: true,
        interactiveBorder: 30,
        flipOnUpdate: true,
        // some async loading code...
        onCreate(instance) {
            // Setup our own custom state properties
            instance._isFetching = false;
            instance._src = null;
            instance._error = null;
        },
        onShow(instance) {
            if (instance._isFetching || instance._src || instance._error) {
                return;
            }
            instance._isFetching = true;
            postData('/Group/Hover/', { 'groupId': groupid })
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
    });
}