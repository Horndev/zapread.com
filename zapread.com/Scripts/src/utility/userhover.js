/*
 * 
 */
import tippy from 'tippy.js';
import 'tippy.js/dist/tippy.css';
import 'tippy.js/themes/light-border.css';
import { postData } from './postData';

// need to remove jquery here
//var getAntiForgeryTokenValue = function () {
//    var form = $('#__AjaxAntiForgeryForm');
//    var token = $('input[name="__RequestVerificationToken"]', form).val();
//    return token;
//};

export function loaduserhover(e) {
    e.removeAttribute('onmouseover');
    var userid = e.getAttribute('data-userid');
    var username = e.innerHTML.trim().replace('@', '');
    if (typeof userid === 'undefined') {
        userid = -1;
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
            postData('/User/Hover/', { 'userId': userid, 'username': username })
                .then((data) => {
                    instance.setContent(data.HTMLString);
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