/**
 * User to user chat
 * 
 */

import 'quill/dist/quill.core.css'
import 'quill/dist/quill.snow.css'
import '../../css/quill/quillchat.css'; // Some custom overrides
import '../../css/quill/quillcustom.css'; // Some custom overrides
import '../../shared/shared';
import '../../realtime/signalr';
import Quill from 'quill';
import DOMPurify from 'dompurify';
//import 'quill-mention'; // This auto-registers
import Mention from '../../quill/quill-mention/src/quill.mention';
import ImageResize from '../../quill-image-resize-module/src/ImageResize';
import { ImageUpload } from 'quill-image-upload';
import AutoLinks from 'quill-auto-links';
import QuillImageDropAndPaste from 'quill-image-drop-and-paste';
import { getAntiForgeryToken } from '../../utility/antiforgery';
import { updatePostTimes } from '../../utility/datetime/posttime';
import { ready } from '../../utility/ready';
import { subMinutes, format, parseISO, formatDistanceToNow } from 'date-fns';
import { postJson } from '../../utility/postData';
import '../../shared/sharedlast';

Quill.register('modules/imageUpload', ImageUpload);
Quill.register('modules/autoLinks', AutoLinks);
Quill.register('modules/imageResize', ImageResize);
Quill.register('modules/imageDropAndPaste', QuillImageDropAndPaste)

window.subMinutes = subMinutes;
window.format = format;
window.parseISO = parseISO;
window.formatDistanceToNow = formatDistanceToNow;

updatePostTimes();

var toolbarOptions = {
  container: [
    ['send'],
    ['bold', 'italic', 'underline', 'strike'],        // toggled buttons
    ['blockquote'],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }],
    [{ 'color': [] }, { 'background': [] }],          // dropdown with defaults from theme
    ['link', 'image'],          // add's image support
    ['clean']                                         // remove formatting button
  ],
  handlers: {
    'save': function () { }   //dummy - will be updated
  }
};

var icons = Quill.import('ui/icons');
icons['send'] = 'Send <i class="fa fa-paper-plane"></i>';

/**
  * Upload a file with loading bar
  * @param {any} file
  */
function sendImage(file) {
  var fd = new FormData();
  fd.append('file', file);
  document.getElementById("progressUpload").style.display = "flex";
  document.getElementById("progressUploadBar").style.width = "1%";
  // upload image
  const xhr = new XMLHttpRequest();
  // init http query
  xhr.open('POST', '/Img/UploadImage/', true);
  // add custom headers
  var headers = getAntiForgeryToken();
  for (var index in headers) {
    xhr.setRequestHeader(index, headers[index]);
  }

  // progress bar
  xhr.upload.addEventListener("progress", function (evt) {
    if (evt.lengthComputable) {
      var percentComplete = evt.loaded / evt.total;
      percentComplete = parseInt(percentComplete * 100);
      document.getElementById("progressUploadBar").style.width = percentComplete.toString() + "%";
      if (percentComplete === 100) {
        document.getElementById("progressUploadBar").style.width = "100%";
      }
    }
  }, false);

  // listen callback
  xhr.onload = () => {
    document.getElementById("progressUpload").style.display = "none";
    if (xhr.status === 200) {
      var data = JSON.parse(xhr.responseText);
      var index = (quill.getSelection() || {}).index || quill.getLength();
      if (index) {
        quill.insertEmbed(index, 'image', '/i/' + data.imgIdEnc, 'user');
      } else {
        console.log({
          code: xhr.status,
          type: xhr.statusText,
          body: xhr.responseText
        });
      }
    };
  }
  xhr.send(fd);
}

var quill = new Quill("#editor-container", {
  modules: {
    imageUpload: {
      url: '/Img/UploadImage/', // server url. If the url is empty then the base64 returns
      method: 'POST', // change query method, default 'POST'
      name: 'file', // custom form name
      withCredentials: true, // withCredentials
      headers: getAntiForgeryToken(), // add custom headers, example { token: 'your-token'}
      customUploader: (file, dataUrl) => {
        sendImage(file);
      },
      // personalize successful callback and call next function to insert new url to the editor
      //callbackOK: (serverResponse, insertURL) => {
      //  insertURL('/Img/Content/' + serverResponse.imgId + '/');//serverResponse);
      //},
      // personalize failed callback
      callbackKO: serverError => {
        alert(serverError);
      },
      // optional
      // add callback when a image have been chosen
      checkBeforeSend: (file, next) => {
        //console.log(file);
        next(file); // go back to component and send to the server
      }
    },
    uploader: {
      mimetypes: [],
      handler: (range, files) => { } // Disable uploading with default module since we have the image drop and paste module
    },
    imageDropAndPaste: {
      // add an custom image handler
      handler: imageHandler
    },
    autoLinks: true,
    imageResize: {},
    //imageRotate: {},
    mention: {
      minChars: 1,
      onSelect: function onSelect(item, insertItem) {
        item.value = `<span class='userhint'>${item.value}</span>`;
        insertItem(item);
      },
      allowedChars: /^[A-Za-z\sÅÄÖåäö]*$/,
      mentionDenotationChars: ["@"],
      source: async function (searchTerm, renderList) {
        const matchedUsers = await suggestUsers(searchTerm);
        renderList(matchedUsers);
      }
    },
    toolbar: toolbarOptions
  },
  placeholder: 'Write a message...',
  theme: 'snow'
});

// submit button clicked
var sendButton = document.querySelector('.ql-send');
sendButton.addEventListener('click', function () {
  console.log('send clicked!');
  var userId = document.getElementById("editor-container").getAttribute("data-userid");
  sendMessage(userId);
});

ready(function () {
  window.scrollTo(0, document.body.scrollHeight + 50);
});

export async function suggestUsers(searchTerm) {
  var matchedUsers = [];
  var data = await postData("/Comment/Mentions/", {
    searchstr: searchTerm.toString() // not sure if toString is needed here...
  });
  matchedUsers = data.users;
  return matchedUsers;
}

/**
* Do something to our dropped or pasted image
* @param.imageDataUrl {string} - image's dataURL
* @param.type {string} - image's mime type
* @param.imageData {object} - provided more functions to handle the image
*   - imageData.toBlob() {function} - convert image to a BLOB Object
*   - imageData.toFile(filename) {function} - convert image to a File Object
*   - imageData.minify(options) {function)- minify the image, return a promise
*      - options.maxWidth {number} - specify the max width of the image, default is 800
*      - options.maxHeight {number} - specify the max width of the image, default is 800
*      - options.quality {number} - specify the quality of the image, default is 0.8
*/
function imageHandler(imageDataUrl, type, imageData) {
  var filename = 'pastedImage.png'
  var file = imageData.toFile(filename)
  sendImage(file);
}

/**
 * 
 * @param {any} id: message id
 */
export function sendMessage(id) {
  console.log('send to ' + id);

  var contentEl = document.getElementById('editor-container').querySelectorAll('.ql-editor').item(0);
  var commentHTML = DOMPurify.sanitize(contentEl.innerHTML);

  if (commentHTML === "<p><br></p>" || commentHTML.replace(" ", "") === "<p></p>") {
    // don't send empty!
    return;
  }

  document.getElementById("chatReply").classList.add("sk-loading");

  postJson("/Messages/SendMessage/", {
    id: id,
    content: commentHTML,
    isChat: true
  })
    .then((response) => {
      if (response.success) {
        contentEl.innerHTML = "";
        postJson("/Messages/GetMessage/", { 'id': response.id }).then((result) => {
          document.getElementById("endMessages").innerHTML += result.HTMLString;
          updatePostTimes();
          window.scrollTo(0, document.body.scrollHeight + 10);
        });
        document.getElementById("chatReply").classList.remove("sk-loading");
        //$('#chatReply').removeClass('sk-loading');
      }
      else {
        //$('#chatReply').removeClass('sk-loading');
        document.getElementById("chatReply").classList.remove("sk-loading");
        alert(response.message);
      }
    });
}
//window.sendMessage = sendMessage;

/**
 * Loads older chat history and inserts into DOM
 * @param {any} id : User id for other user
 */
export function loadolderchats(id) {
  postJson("/Messages/LoadOlder/", { otherId: ChattingWithId, start: startBlock, blocks: 10 })
    .then((response) => {
      if (response.success) {
        //$("#startMessages").prepend(response.HTMLString); // Insert at the front
        document.getElementById("startMessages").innerHTML = response.HTMLString + document.getElementById("startMessages").innerHTML;
        startBlock += 10;
        updatePostTimes();
      }
      else {
        alert(response.message);
      }
    });
}
window.loadolderchats = loadolderchats;