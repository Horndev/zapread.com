﻿/**
 * Handle chat messages being received
 **/

import DOMPurify from 'dompurify';
import { updatePostTimes } from '../../utility/datetime/posttime'

/**
 * Handle a chat message recieved from another user
 * The global variable ChattingWithId will be set to filter out the userId who 
 * is being chatted with to prevent cross-contaminating multiple chats.
 * 
 * @param {string} HTMLString Rendered HTML of the chat message
 * @param {string} userId user who is being chatted with
 **/
export function onchatreceived(HTMLString, userId) {
  var cleanHTMLString = DOMPurify.sanitize(HTMLString);
  if (typeof ChattingWithId !== 'undefined') {
    if (userId === ChattingWithId) {
      document.querySelectorAll('#endMessages').item(0).innerHTML += cleanHTMLString;
      updatePostTimes();
      window.scrollTo(0, document.body.scrollHeight + 10);
    }
  }
}