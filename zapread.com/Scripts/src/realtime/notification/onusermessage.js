/**
 * 
 **/

import DOMPurify from 'dompurify';
import * as tata from 'tata-js'

export function onusermessage(content, reason, clickUrl) {
  var cleanContent = DOMPurify.sanitize(content);
  var cleanReason = DOMPurify.sanitize(reason);
  if(/^(https?:\/\/|\/)/.test(clickUrl))
    clickUrl = "https://" + clickUrl;
  tata.info(cleanReason, cleanContent, {
    position: "br",
    duration: 10000,
    onClick: function () {
      window.open(clickUrl, '_blank');
    }
  });
}
