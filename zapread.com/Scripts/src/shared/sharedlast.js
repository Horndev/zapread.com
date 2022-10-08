/*
 * 
 */
import { ready } from '../utility/ready';

import '../css/site.scss';
import '../css/roundlinks.css';
import '../css/hover.css';
import '../css/quill/quillfont.css';
import "tarteaucitronjs/css/tarteaucitron.css";

var tarteaucitronForceCDN = '/Content/';
tarteaucitron.cdn = tarteaucitronForceCDN;

// youtube
tarteaucitron.services.zyoutube = {
  "key": "zyoutube",
  "type": "video",
  "name": "YouTube",
  "uri": "https://policies.google.com/privacy",
  "needConsent": true,
  "cookies": ['VISITOR_INFO1_LIVE', 'YSC', 'PREF', 'GEUP'],
  "js": function () {
    "use strict";
    tarteaucitron.fallback(['youtube_player'], function (x) {
      //console.log("js youtube_player",x);
      var frame_title = tarteaucitron.fixSelfXSS(tarteaucitron.getElemAttr(x, "title") || 'Youtube iframe'),
        video_id = tarteaucitron.getElemAttr(x, "videoID"),
        srcdoc = tarteaucitron.getElemAttr(x, "srcdoc"),
        loading = tarteaucitron.getElemAttr(x, "loading"),
        video_width = tarteaucitron.getElemAttr(x, "width"),
        frame_width = 'width=',
        video_height = tarteaucitron.getElemAttr(x, "height"),
        frame_height = 'height=',
        video_frame,
        allowfullscreen = tarteaucitron.getElemAttr(x, "allowfullscreen"),
        attrs = ["theme", "rel", "controls", "showinfo", "autoplay", "mute", "start", "loop"],
        params = attrs.filter(function (a) {

          return tarteaucitron.getElemAttr(x, a) !== null;
        }).map(function (a) {
          return a + "=" + tarteaucitron.getElemAttr(x, a);
        }).join("&");

      if (tarteaucitron.getElemAttr(x, "loop") == 1) {
        params = params + "&playlist=" + video_id;
      }

      if (video_id === undefined) {
        return "";
      }
      if (video_width !== undefined) {
        frame_width += '"' + video_width + '" ';
      } else {
        frame_width += '"" ';
      }
      if (video_height !== undefined) {
        frame_height += '"' + video_height + '" ';
      } else {
        frame_height += '"" ';
      }

      if (srcdoc !== undefined && srcdoc !== null && srcdoc !== "") {
        srcdoc = 'srcdoc="' + srcdoc + '" ';
      } else {
        srcdoc = '';
      }

      if (loading !== undefined && loading !== null && loading !== "") {
        loading = 'loading ';
      } else {
        loading = '';
      }

      video_frame = '<iframe class="embed-responsive-item" title="' + frame_title + '" src="//www.youtube-nocookie.com/embed/' + video_id + '?' + params + '"' + (allowfullscreen == '0' ? '' : ' webkitallowfullscreen mozallowfullscreen allowfullscreen') + ' ' + srcdoc + ' ' + loading + '></iframe>';
      return video_frame;
    });
  },
  "fallback": function () {
    "use strict";
    var id = 'zyoutube';
    tarteaucitron.fallback(['youtube_player'], function (elem) {
      elem.parentElement.style.backgroundColor = 'lightgray';
      elem.style.width = '100%';//elem.getAttribute('width') + 'px';
      //elem.style.height = '80px';//elem.getAttribute('height') + 'px';
      return tarteaucitron.engage(id);
    });
  }
};

// google adsense search (result)
tarteaucitron.services.adsensesearchresult = {
  "key": "adsensesearchresult",
  "type": "ads",
  "name": "Google Adsense Search (result)",
  "uri": "https://adssettings.google.com/",
  "needConsent": true,
  "cookies": [],
  "js": function () {
    "use strict";
    if (tarteaucitron.user.adsensesearchresultCx === undefined) {
      return;
    }
    tarteaucitron.addScript('//www.google.com/cse/cse.js?cx=' + tarteaucitron.user.adsensesearchresultCx);
  },
  "fallback": function () {
    "use strict";
    var id = 'adsensesearchresult';

    if (document.getElementById('gcse_searchresults')) {
      document.getElementById('gcse_searchresults').innerHTML = tarteaucitron.engage(id);
    }
  }
};

tarteaucitron.services.zapreadARRA = {
  "key": "zapreadARRA",
  "type": "other",
  "name": "Cloud Load Balancing",
  "needConsent": false,
  "cookies": ['ARRAffinity'],
  "readmoreLink": "/Home/About/", // If you want to change readmore link
  "js": function () {
    // When user allow cookie
  },
  "fallback": function () {
    // when use deny cookie
    console.log("remove ARRAffinity");
    document.cookie = "ARRAffinity=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
  }
};

(tarteaucitron.job = tarteaucitron.job || []).push('zyoutube');
(tarteaucitron.job = tarteaucitron.job || []).push('zapreadARRA');

ready(function () {
  tarteaucitron.init({
    "privacyUrl": "https://www.zapread.com/Home/About/", /* Privacy policy url */
    "hashtag": "#tarteaucitron", /* Open the panel with this hashtag */
    "cookieName": "tarteaucitron", /* Cookie name */
    "orientation": "bottom", /* Banner position (top - bottom - middle - popup) */
    "groupServices": false, /* Group services by category */
    "showAlertSmall": false, /* Show the small banner on bottom right */
    "cookieslist": false, /* Show the cookie list */
    "showIcon": false, /* Show cookie icon to manage cookies */
    "iconSrc": "/Content/privacy.png", /* Optionnal: URL or base64 encoded image */
    "iconPosition": "BottomLeft", /* Position of the icon between BottomRight, BottomLeft, TopRight and TopLeft */
    "adblocker": false, /* Show a Warning if an adblocker is detected */
    "DenyAllCta": true, /* Show the deny all button */
    "AcceptAllCta": true, /* Show the accept all button when highPrivacy on */
    "highPrivacy": true, /* HIGHLY RECOMMANDED Disable auto consent */
    "handleBrowserDNTRequest": false, /* If Do Not Track == 1, disallow all */
    "removeCredit": true, /* Remove credit link */
    "moreInfoLink": false, /* Show more info link */
    "useExternalCss": true, /* If false, the tarteaucitron.css file will be loaded */
    //"cookieDomain": ".zapread.com", /* Shared cookie for subdomain website */
    "readmoreLink": "", /* Change the default readmore link pointing to tarteaucitron.io */
    "mandatory": true /* Show a message about mandatory cookies */
  });
});
