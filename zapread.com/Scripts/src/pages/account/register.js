/*
 * 
 */

import '../../shared/shared';
import 'jquery-validation';
import '../../shared/sharedlast';
import tippy from 'tippy.js';                       // [✓]
import 'tippy.js/dist/tippy.css';                   // [✓]
import 'tippy.js/themes/light-border.css';          // [✓]



// Apply the loading tippy
var infoEl = document.getElementById("info-notifications");
tippy(infoEl, {
  content: 'You can later customize which emails you would like to receive such as for notifications of comments, earnings, and chat messages.',
  theme: 'light-border',
  allowHTML: true,
  delay: 300,
  interactive: true,
  interactiveBorder: 30,
});