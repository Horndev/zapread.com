﻿/**
 * 
 * [✓] does not use jQuery
 * 
 */

import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';

export function updatePostTimes() {
    var elements = document.querySelectorAll(".postTime");
    Array.prototype.forEach.call(elements, function (el, _i) {
        var datefn = parseISO(el.innerHTML);
        datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
        var date = format(datefn, "dd MMM yyyy");
        var time = formatDistanceToNow(datefn, { addSuffix: false });
        el.innerHTML = '<span>' + time + ' ago - ' + date + '</span>';
        el.style.display = 'inline';
        el.classList.remove('postTime');
    });
}

/**
 * Convert an ISO time string to a relative text string
 * @param {string} timestring
 */
export function ISOtoRelative(timestring) {
  var datefn = parseISO(timestring);
  datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
  var date = format(datefn, "dd MMM yyyy");
  var time = formatDistanceToNow(datefn, { addSuffix: false });
  var returnstring = time + ' ago - ' + date;
  return returnstring;
}