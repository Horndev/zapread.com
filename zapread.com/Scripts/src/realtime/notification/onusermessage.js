/**
 * 
 **/

import DOMPurify from 'dompurify';
import * as tata from 'tata-js';

export function onusermessage(content, reason, clickUrl) {
  var cleanContent = DOMPurify.sanitize(content);
  var cleanReason = DOMPurify.sanitize(reason);
  tata.info(cleanReason, cleanContent, {
    position: "br",
    duration: 10000,
    onClick: function () {
      window.open(clickUrl, '_blank');
    }
  });
}