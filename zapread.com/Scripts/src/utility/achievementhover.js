/*
 * 
 */
import $ from 'jquery';
import tippy from 'tippy.js';                       // [✓]
import 'tippy.js/dist/tippy.css';                   // [✓]
import 'tippy.js/themes/light-border.css';          // [✓]
import { postData } from './postData';              // [✓]

export function loadachhover(e) {
  e.removeAttribute('onmouseover');//$(e).removeAttr('onmouseover');
  var achid = e.getAttribute('data-achid');//$(e).data('achid');
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

  //var msg = JSON.stringify({
  //  'id': achid
  //});
  //$.ajax({
  //  type: "POST",
  //  url: "/User/Achievement/Hover",
  //  data: msg,
  //  contentType: "application/json; charset=utf-8",
  //  dataType: "json",
  //  success: function (response) {
  //    if (response.success) {
  //      $(e).attr("data-content", response.HTMLString);
  //      $(e).popover({
  //        trigger: "manual",
  //        html: true,
  //        sanitize: false,
  //        animation: false,
  //        placement: "top",
  //        container: "body",
  //        title: ""
  //      })
  //        .on("mouseenter", function () {
  //          var _this = this;
  //          $(this).popover("show");
  //          $(".popover").addClass("tooltip-hover");
  //          $(".popover").on("mouseleave", function () {
  //            $(_this).popover('hide');
  //          });
  //        })
  //        .on("mouseleave", function () {
  //          var _this = this;
  //          setTimeout(function () {
  //            if (!$(".popover:hover").length) {
  //              $(_this).popover("hide");
  //            }
  //          }, 300);
  //        });
  //      $(e).popover("show");
  //      $(".popover").addClass("tooltip-hover");
  //      $(".popover").on("mouseleave", function () {
  //        $(e).popover('hide');
  //      });
  //      $(e).removeClass("zr-user");
  //    }
  //    else {
  //      console.log(response.Message);
  //    }
  //  },
  //  failure: function (response) {
  //    console.log('load more failure');
  //  },
  //  error: function (response) {
  //    console.log('load more error');
  //  }
  //});
}