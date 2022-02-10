/*
 * 
 */
import tippy from 'tippy.js';                       // [✓]
import 'tippy.js/dist/tippy.css';                   // [✓]
import 'tippy.js/themes/light-border.css';          // [✓]
import { postData } from './postData';              // [✓]

export function loadachhover(e) {
  e.removeAttribute('onmouseover');
  var achid = e.getAttribute('data-achid');
  if (typeof achid === 'undefined' || achid === null) {
    achid = -1;
  }
  tippy(e, {
    content: 'Loading...',
    theme: 'light-border',
    allowHTML: true,
    delay: 300,
    interactive: true,
    interactiveBorder: 30,
    onCreate(instance) {
      instance._isFetching = false;
      instance._src = null;
      instance._error = null;
    },
    onShow(instance) {
      if (instance._isFetching || instance._src || instance._error) {
        return;
      } else {
        instance._isFetching = true;
        var msg = { 'id': achid };
        postData('/User/Achievement/Hover', msg)
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
    }
  });
}