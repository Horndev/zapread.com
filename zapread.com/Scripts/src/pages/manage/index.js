/**
 * Scripts for User Management
 * 
 * Native JS
 */

import "../../shared/shared";
import '../../utility/ui/vote';
import '../../realtime/signalr';
import Dropzone from 'dropzone';
import Selectr from 'mobius1-selectr';
import 'mobius1-selectr/dist/selectr.min.css'
import 'dropzone/dist/basic.css';
import 'dropzone/dist/dropzone.css';
import Swal from 'sweetalert2';
const getOnLoadedMorePosts = () => import('../../utility/onLoadedMorePosts');
import { writeComment } from '../../comment/writecomment';
import { replyComment } from '../../comment/replycomment';
import { editComment } from '../../comment/editcomment';
import { loadMoreComments } from '../../comment/loadmorecomments';
import { loadachhover } from '../../utility/achievementhover';
import { loadmore } from '../../utility/loadmore';
import { postJson } from "../../utility/postData";
import { getJson } from "../../utility/getData";
import '../../css/pages/manage/manage.css';
import './updateAlias';
import '../../shared/postfunctions';
import '../../shared/readmore';
import '../../shared/postui';
import '../../shared/sharedlast';
import React from "react";
import ReactDOM from "react-dom";
const getVoteModal = () => import("../../Components/VoteModal");

/* Vote Modal Component */
getVoteModal().then(({ default: VoteModal }) => {
  ReactDOM.render(<VoteModal />, document.getElementById("ModalVote"));
  const event = new Event('voteReady');
  document.dispatchEvent(event);
});

// Make global (called from html)
window.writeComment = writeComment;
window.replyComment = replyComment;
window.editComment = editComment;
window.loadMoreComments = loadMoreComments;
window.BlockNumber = 10;  //Infinite Scroll starts from second block
window.NoMoreData = false;
window.inProgress = false;

async function LoadReferralStats() {
  await fetch("/api/v1/user/referralstats").then(response => {
    return response.json();
  }).then(data => {
    //console.log("LoadReferralStats", data);
    var totalEl = document.getElementById("refTotal");
    totalEl.innerHTML = data.TotalReferred;
    var activeEl = document.getElementById("refTotalActive");
    activeEl.innerHTML = data.TotalReferredActive;
    var enrolledEl = document.getElementById("refEnrolled");
    enrolledEl.innerHTML = data.IsActive ? "Referral Active" : "Referral Inactive";
  })
}
LoadReferralStats();

async function LoadReferralCode() {
  await fetch("/api/v1/user/referralcode").then(response => {
    return response.json();
  }).then(data => {
    //console.log(data);
    var codeEl = document.getElementById("referralCode");
    codeEl.value = data.refCode;
    var reglink = "https://www.zapread.com/Account/Register/?refcode=" + data.refCode
    var linkEl = document.getElementById("regLink");
    linkEl.value = reglink;
  })
}
LoadReferralCode();

async function LoadActivityPostsAsync() {
  await fetch("/Manage/GetActivityPosts").then(response => {
    return response.text();
  }).then(html => {
    document.getElementById("posts-loading").classList.remove("sk-loading");
    var postsBoxEl = document.getElementById("posts");
    postsBoxEl.innerHTML = html;
    getOnLoadedMorePosts().then(({ onLoadedMorePosts }) => {
      onLoadedMorePosts();
    });
  })
}
LoadActivityPostsAsync();

async function InitRotateProfileImageButton() {
  var el = document.getElementById('btnRotateProfileImage');
  el.addEventListener("click", function (e) {
    postJson('/Manage/RotateProfileImage/').then(response => {
      if (response.success) {
        console.log('rotated');
        updateImagesOnPage(response.version); // Reload images
      } else {
        // Did not work
        Swal.fire("Error updating: " + data.message, "error");
      }
    }).catch((error) => {
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        else {
          error.json().then(data => {
            Swal.fire("Error", `${data.message}`, "error");
          })
        }
      });
  }, false);
}
InitRotateProfileImageButton();

async function Initialize() {
  var disableEl = document.getElementById('btnDisableGA');
  if (disableEl != null) {
    disableEl.addEventListener("click", function (e) {
      postJson('/Manage/DisableGoogleAuthenticator/').then(response => {
        if (response.success) {
          Swal.fire("Authenticator Disabled", "Google Authenticator is now disabled", "success")
            .then(() => {
              window.location = "/Manage";
            });
        } else {
          Swal.fire("Error", `${response.message}`, "error");
        }
      });
    }, false);
  }

  var el = document.getElementById('btnEnableGA');
  if (el != null) {
    el.addEventListener("click", function (e) {
      postJson('/Manage/ConfigureGoogleAuthenticator/').then(response => {
        if (response.success) {
          Swal.fire({
            title: "Configure Google Authenticator",
            showCancelButton: true,
            html: '<p>Scan this QR code with your authenticator</p><img src="data:image/png;base64, ' + response.QRCodeB64 + '" class="img-fluid" /><br/><label>Enter the 6 digit code generated by authenticator:</label><input type="text" id="code" class="swal2-input">',
            focusConfirm: false,
            preConfirm: () => {
              const code = Swal.getPopup().querySelector('#code').value
              if (!code) {
                Swal.showValidationMessage(`Please enter code`)
              }
              return { code: code, secretKey: response.SecretKey }
            }
          }).then((result) => {
            postJson('/Manage/EnableGoogleAuthenticator/', {
              Code: result.value.code,
              SecretKey: result.value.secretKey
            }).then(response => {
              if (response.success) {
                Swal.fire("Authenticator Enabled", "Your 2FA is now activated with Google Authenticator", "success")
                  .then(() => {
                    window.location = "/Manage";
                  });
              }
              else {
                Swal.fire("Error", `${response.message}`, "error");
              }
              //console.log(response);
            });
            //Swal.fire(`
            //  Code: ${result.value.code}
            //`.trim())
          })
        } else {
          // Did not work
          Swal.fire("Error updating: " + response.message, "error");
        }
      }).catch((error) => {
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        else {
          error.json().then(data => {
            Swal.fire("Error", `${data.message}`, "error");
          })
        }
      });
    }, false);
  }
}
Initialize();

/**
 * Wrapper for load more
 **/
export function manageloadmore(userId) {
  loadmore({
    url: '/User/InfiniteScroll/',
    blocknumber: window.BlockNumber,
    sort: "New",
    userId: userId
  });
}
window.loadmore = manageloadmore;

var elements = document.querySelectorAll(".ach-hover");
Array.prototype.forEach.call(elements, function (el, _i) {
  loadachhover(el);
});

Dropzone.options.dropzoneForm = {
  paramName: "file", // The name that will be used to transfer the file
  maxFilesize: 15, // MB
  acceptedFiles: "image/*",
  maxFiles: 1,
  uploadMultiple: false,
  init: function () {
    this.on("addedfile", function () {
    });
    this.on("success", function (file, response) {
      if (response.success) {
        updateImagesOnPage(response.version); // Reload images
      } else {
        // Did not work
        Swal.fire({
          icon: 'error',
          title: 'Image Update Error',
          text: "Error updating image: " + response.message
        })
      }
    });
  },
  dictDefaultMessage: "<strong>Drop user image here or click to upload</strong>"
};

function updateImagesOnPage(ver) {
  document.getElementById("userImageLarge").setAttribute("src", "/Home/UserImage/?size=500&v=" + ver);
  var elements = document.querySelectorAll(".user-image-30");
  Array.prototype.forEach.call(elements, function (el, _i) {
    el.setAttribute("src", "/Home/UserImage/?size=30&v=" + ver);
  });
  elements = document.querySelectorAll(".user-image-45");
  Array.prototype.forEach.call(elements, function (el, _i) {
    el.setAttribute("src", "/Home/UserImage/?size=45&v=" + ver);
  });
  elements = document.querySelectorAll(".user-image-15");
  Array.prototype.forEach.call(elements, function (el, _i) {
    el.setAttribute("src", "/Home/UserImage/?size=15&v=" + ver);
  });
}

async function initLanguagesSelect() {
  var langEl = document.getElementById("languagesSelect");
  var selectr = new Selectr(langEl, {
    multiple: true
  });
  langEl.addEventListener("change", function (evt) {
    var selectedValues = selectr.getValue();
    var values = [...new Set(selectedValues)]; // Unique set only
    var userlangs = values.join(',');
    console.log(userlangs);
    postJson('/Manage/UpdateUserLanguages', {
        languages: userlangs
      })
      .then(response => {
      if (response.success) {
        console.log('languages updated.');
      } else {
        // Did not work
        Swal.fire("Error updating: " + data.message, "error");
      }
    })
      .catch((error) => {
        if (error instanceof Error) {
          Swal.fire("Error", `${error.message}`, "error");
        }
        else {
          error.json().then(data => {
            Swal.fire("Error", `${data.message}`, "error");
          })
        }
      });
  })
}
initLanguagesSelect();

// Set group list as clickable
var elements = document.querySelectorAll(".clickable-row");
Array.prototype.forEach.call(elements, function (el, _i) {
  el.addEventListener("click", function (e) {
    //console.log(e,el);
    var url = el.getAttribute('data-href')
    window.location = url;
  }, false);
});


export function requestAPIKey() {
  getJson('/api/v1/account/apikeys/new?roles=default')
    .then(response => {
      if (response.success) {
        Swal.fire({
          icon: "success",
          title: 'Your new key is:',
          input: 'text',
          inputValue: response.Key.Key,
          showCancelButton: false
        });
      } else {
        // Did not work
        Swal.fire("Error generating key: " + data.message, "error");
      }
    })
    .catch((error) => {
      if (error instanceof Error) {
        Swal.fire("Error", `${error.message}`, "error");
      }
      else {
        error.json().then(data => {
          Swal.fire("Error", `${data.message}`, "error");
        })
      }
    });
  return false; // Prevent jump to top of page
}
window.requestAPIKey = requestAPIKey;

export function updateLanguages() {
  console.log('updateLanguages');
}
window.updateLanguages = updateLanguages;

/** Change userprofile image
 * 
 * @param {any} set : 1 = robot, 2 = cat, 3 = human
 * @returns {boolean} false
 */
export function generateRobot(set) {
  postJson('/Home/SetUserImage/',
    {
      set: set
    }).then((response) => {
      if (response.success) {
        // Reload images
        updateImagesOnPage(response.version);
      } else {
        // Did not work
        Swal.fire({
          icon: 'error',
          title: 'Image Update Error',
          text: "Error updating image: " + response.message
        })
      }
    }).catch((error) => {
      if (error instanceof Error) {
        Swal.fire("Error", `${error.message}`, "error");
      }
      else {
        error.json().then(data => {
          Swal.fire("Error", `${data.message}`, "error");
        })
      }
    });
  return false; // Prevent jump to top of page
}
window.generateRobot = generateRobot;

export function settingToggle(e) {
  //console.log(e);
  var setting = e.id;
  var value = e.checked;
  let spinner = e.parentElement.querySelector(".switch-spinner");
  //console.log("spinner",spinner);
  spinner.classList.remove("fa-check");
  spinner.classList.add("fa-refresh");
  spinner.classList.add("fa-spin");
  spinner.style.display = 'initial';

  postJson('/Manage/UpdateUserSetting', {
    setting: setting,
    value: value
  }).then((response) => {
    if (response.success) {
      spinner.classList.remove("fa-refresh");
      spinner.classList.remove("fa-spin");
      spinner.classList.add("fa-check");
    }
  });
}
window.settingToggle = settingToggle;