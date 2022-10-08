/**
 * 
 * [✓] does not use jQuery
 * 
 */

import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';

export async function updatePostTimesOnEl(e, tzAdj = true) {
  if (tzAdj == undefined) { tzAdj = true; }
  var elements = e.querySelectorAll(".postTime");
  Array.prototype.forEach.call(elements, function (el, _i) {
    //console.log(el.innerHTML);
    var datefn = parseISO(el.innerHTML);
    if (tzAdj) {
      datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
    }
    var date = format(datefn, "dd MMM yyyy");
    var time = formatDistanceToNow(datefn, { addSuffix: false });
    el.innerHTML = '<span>' + time + ' ago - ' + date + '</span>';
    el.style.display = 'inline';
    el.classList.remove('postTime');
  });
}

export async function updatePostTimes() {
  await updatePostTimesOnEl(document);
}

/**
 * Convert an ISO time string to a relative text string
 * @param {string} timestring
 */
export function ISOtoRelative(timestring, future=false) {
  var datefn = parseISO(timestring);
  datefn = subMinutes(datefn, (new Date()).getTimezoneOffset());
  var date = format(datefn, "dd MMM yyyy");
  var time = formatDistanceToNow(datefn, { addSuffix: false });
  if (future) {
    return 'in ' + time + ' - ' + date;
  }
  else {
    return time + ' ago - ' + date;
  }
}