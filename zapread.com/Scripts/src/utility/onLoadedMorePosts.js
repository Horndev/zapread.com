/*
 *
 */
import * as bsn from 'bootstrap.native/dist/bootstrap-native-v4';               // [✓]

import { applyHoverToChildren } from './userhover';                             // [✓]
import { loadgrouphover } from './grouphover';
import { updatePostTimes } from './datetime/posttime';
import { makePostsQuotable, makeCommentsQuotable } from './quotable/quotable';

/**
 * 
 * [✓] native JS
 **/
export function onLoadedMorePosts() {
    //console.log('[DEBUG] onLoadedMorePosts');
    // User mention hover
    applyHoverToChildren(document, ".userhint");
    //var elements = document.querySelectorAll(".userhint");
    //Array.prototype.forEach.call(elements, function (el, _i) {
    //    loaduserhover(el);
    //    el.classList.remove('userhint');
    //});

    var elements = document.querySelectorAll(".grouphint");
    Array.prototype.forEach.call(elements, function (el, _i) {
        loadgrouphover(el);
        el.classList.remove('grouphint');
    });

    // activate dropdown (done manually using bootstrap.native)
    elements = document.querySelectorAll(".dropdown-toggle");
    Array.prototype.forEach.call(elements, function (el, _i) {
        var dropdownInit = new bsn.Dropdown(el);
    });

    // show the read more
    elements = document.querySelectorAll(".post-box");
    Array.prototype.forEach.call(elements, function (el, _i) {
        if (parseFloat(getComputedStyle(el, null).height.replace("px", "")) >= 800) {
            el.querySelectorAll(".read-more-button").item(0).style.display = 'initial';
        }
    });

    // --- update impressions counts
    elements = document.querySelectorAll(".impression");
    Array.prototype.forEach.call(elements, function (el, _i) {
        var url = el.getAttribute('data-url');
        fetch(url).then(function (response) {
            return response.text();
        }).then(function (html) {
            el.innerHTML = html;
            el.classList.remove('impression');
        });
    });

    // --- relative times
    updatePostTimes();
    // ---

    // --- socials buttons
    // TODO: implement using non-jquery library
    elements = document.querySelectorAll(".sharing");
    Array.prototype.forEach.call(elements, function (el, _i) {
        var url = el.getAttribute('data-url');
        var sharetext = el.getAttribute('data-sharetext');
        //el.jsSocials({
        //    url: url,
        //    text: sharetext,
        //    showLabel: false,
        //    showCount: false,
        //    shareIn: "popup",
        //    shares: ["email", "twitter", "facebook", "linkedin", "pinterest", "whatsapp", "copy"]
        //});
        el.classList.remove('sharing');
    });

    elements = document.querySelectorAll(".pop");
    Array.prototype.forEach.call(elements, function (el, _i) {
        el.classList.remove('pop');
    });

    // Make post quotable
    makePostsQuotable();     // TODO: remove jquery

    // Make comments quotable
    makeCommentsQuotable();  // TODO: remove jquery
}